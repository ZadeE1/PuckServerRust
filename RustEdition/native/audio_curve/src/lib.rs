//! Native wind-audio curve evaluation for the Puck server.
//!
//! Replaces the per-tick managed `AnimationCurve.Evaluate` + unconditional
//! networked writes in `Puck.Server_UpdateAudio` with one P/Invoke that also
//! change-gates the outputs. Keyframes are copied once at startup; per-tick
//! calls only pass two floats and usually return "no change".
//!
//! Single-thread assumption: Unity calls `FixedUpdate` on the main thread, so
//! the cached curves/last-values use plain statics, not locks.
//!
//! NOTE: despite the crate name, this library now covers all ported
//! per-tick helpers (wind audio + net collider radius + PID + sync change-mask).
//! One plugin, one P/Invoke surface.

// --- Synchronized-object change-mask core (SynchronizedObjectData.GetChangeMask) ---
//
// Bit-exact mirror of the managed float math (IEEE f32 ops in C# order;
// f64 asin like Mathf.Asin). Feel-critical: verified by cross-language fuzz
// against the ground-truth managed implementation, not just eyeballing.

/// Narrow blittable view of the fields GetChangeMask reads.
/// Layout must match the C# mirror struct field-for-field.
/// Field order packs to 32 bytes (u32 last for alignment).
#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct SyncMaskInput {
    pub x: i16,
    pub y: i16,
    pub z: i16,
    pub rx: i16,
    pub ry: i16,
    pub rz: i16,
    pub rw: i16,
    pub vx: i16,
    pub vy: i16,
    pub vz: i16,
    pub ax: i16,
    pub ay: i16,
    pub az: i16,
    pub tick_rate_divisor: u8,
    pub compressed_rotation: u32,
}

/// Mirrors NetworkingUtils.DecompressShortToFloat:
/// InverseLerp(-32768, 32767, s) then Lerp(min, max, t), Clamp01 both times.
fn s_decomp(s: i16, min: f32, max: f32) -> f32 {
    let t = (s as f32 - -32768.0) / (32767.0 - -32768.0);
    let tc = if t < 0.0 { 0.0 } else if t > 1.0 { 1.0 } else { t };
    min + (max - min) * tc
}

/// Mirrors SynchronizedObjectData.GetAxisChangeMask (Abs + strict >).
fn s_axis_mask(
    ax: f32,
    ay: f32,
    az: f32,
    bx: f32,
    by: f32,
    bz: f32,
    thresh: f32,
    mx: u16,
    my: u16,
    mz: u16,
) -> u16 {
    let mut m: u16 = 0;
    if (ax - bx).abs() > thresh {
        m |= mx;
    }
    if (ay - by).abs() > thresh {
        m |= my;
    }
    if (az - bz).abs() > thresh {
        m |= mz;
    }
    m
}

fn s_quat_dot(a: [f32; 4], b: [f32; 4]) -> f32 {
    a[0] * b[0] + a[1] * b[1] + a[2] * b[2] + a[3] * b[3]
}

/// Mirrors Quaternion.Normalize (1/sqrt-magnitude, then multiply).
fn s_quat_normalize(q: [f32; 4]) -> [f32; 4] {
    let m = (q[0] * q[0] + q[1] * q[1] + q[2] * q[2] + q[3] * q[3]).sqrt();
    let s = 1.0 / m;
    [q[0] * s, q[1] * s, q[2] * s, q[3] * s]
}

/// Mirrors Unity.Netcode.QuaternionCompressor.DecompressQuaternion
/// (smallest-three, 10 bits each + 2-bit largest index).
fn s_quat_decompress(mut c: u32) -> [f32; 4] {
    let largest = (c >> 30) as usize;
    let mut q = [0.0f32; 4];
    let mut sumsq = 0.0f32;
    let mut i: i32 = 3;
    while i >= 0 {
        if i as usize != largest {
            let bits = (c & 0x1FF) as f32;
            let sign = if (c & 0x200) != 0 { -1.0 } else { 1.0 };
            let v = sign * (bits * 0.0013837706);
            q[i as usize] = v;
            sumsq += v * v;
            c >>= 10;
        }
        i -= 1;
    }
    q[largest] = (1.0 - sumsq).sqrt();
    q
}

/// Mirrors SynchronizedObjectData.GetAngleDegrees (double-precision asin).
fn s_angle_deg(a: [f32; 4], b: [f32; 4]) -> f32 {
    let b = if s_quat_dot(a, b) < 0.0 {
        [-b[0], -b[1], -b[2], -b[3]]
    } else {
        b
    };
    let dx = a[0] - b[0];
    let dy = a[1] - b[1];
    let dz = a[2] - b[2];
    let dw = a[3] - b[3];
    let half = 0.5 * (dx * dx + dy * dy + dz * dz + dw * dw).sqrt();
    let c = if half < 1.0 { half } else { 1.0 };
    (4.0 * (c as f64).asin() as f32) * 57.29578
}

fn s_rot_changed(a: &SyncMaskInput, b: &SyncMaskInput, high: bool) -> bool {
    if high {
        if a.rx == b.rx && a.ry == b.ry && a.rz == b.rz {
            return a.rw != b.rw;
        }
        return true;
    }
    a.compressed_rotation != b.compressed_rotation
}

fn s_get_rot(s: &SyncMaskInput, high: bool) -> [f32; 4] {
    if high {
        s_quat_normalize([
            s_decomp(s.rx, -1.0, 1.0),
            s_decomp(s.ry, -1.0, 1.0),
            s_decomp(s.rz, -1.0, 1.0),
            s_decomp(s.rw, -1.0, 1.0),
        ])
    } else {
        s_quat_decompress(s.compressed_rotation)
    }
}

/// Mirrors SynchronizedObjectData.GetChangeMask bit-for-bit, with an integer
/// fast path: identical compressed inputs provably yield an empty mask, so
/// the ~20 decompressions + quaternion math are skipped entirely.
#[no_mangle]
pub extern "C" fn sync_mask(a: SyncMaskInput, b: SyncMaskInput, high_precision: i32) -> u16 {
    if a.x == b.x
        && a.y == b.y
        && a.z == b.z
        && a.rx == b.rx
        && a.ry == b.ry
        && a.rz == b.rz
        && a.rw == b.rw
        && a.vx == b.vx
        && a.vy == b.vy
        && a.vz == b.vz
        && a.ax == b.ax
        && a.ay == b.ay
        && a.az == b.az
        && a.compressed_rotation == b.compressed_rotation
        && a.tick_rate_divisor == b.tick_rate_divisor
    {
        return 0;
    }
    let high = high_precision != 0;
    let mut m: u16 = 0;
    m |= s_axis_mask(
        s_decomp(a.x, -25.0, 25.0),
        s_decomp(a.y, -50.0, 50.0),
        s_decomp(a.z, -50.0, 50.0),
        s_decomp(b.x, -25.0, 25.0),
        s_decomp(b.y, -50.0, 50.0),
        s_decomp(b.z, -50.0, 50.0),
        0.002,
        1,
        2,
        4,
    );
    if s_rot_changed(&a, &b, high)
        && s_angle_deg(s_get_rot(&a, high), s_get_rot(&b, high)) > 0.05
    {
        m |= 8;
    }
    m |= s_axis_mask(
        s_decomp(a.vx, -100.0, 100.0),
        s_decomp(a.vy, -100.0, 100.0),
        s_decomp(a.vz, -100.0, 100.0),
        s_decomp(b.vx, -100.0, 100.0),
        s_decomp(b.vy, -100.0, 100.0),
        s_decomp(b.vz, -100.0, 100.0),
        0.05,
        16,
        32,
        64,
    );
    m |= s_axis_mask(
        s_decomp(a.ax, -100.0, 100.0),
        s_decomp(a.ay, -100.0, 100.0),
        s_decomp(a.az, -100.0, 100.0),
        s_decomp(b.ax, -100.0, 100.0),
        s_decomp(b.ay, -100.0, 100.0),
        s_decomp(b.az, -100.0, 100.0),
        0.1,
        128,
        256,
        512,
    );
    if a.tick_rate_divisor != b.tick_rate_divisor {
        m |= 0x800;
    }
    m
}

// --- PID controller core (PIDController.Update / UpdateAngle) ---

// Mirrors UnityEngine.Mathf exactly: comparison-based, so NaN passes through.
fn m_clamp(v: f32, min: f32, max: f32) -> f32 {
    if v < min {
        min
    } else if v > max {
        max
    } else {
        v
    }
}

// Unity Mathf.Lerp clamps t to [0, 1].
fn m_lerp(a: f32, b: f32, t: f32) -> f32 {
    a + (b - a) * m_clamp(t, 0.0, 1.0)
}

fn m_repeat(t: f32, length: f32) -> f32 {
    m_clamp(t - (t / length).floor() * length, 0.0, length)
}

// Unity Mathf.DeltaAngle(current, target): shortest signed angle in [-180, 180].
fn m_delta_angle(current: f32, target: f32) -> f32 {
    let n = m_repeat(target - current, 360.0);
    if n > 180.0 {
        n - 360.0
    } else {
        n
    }
}

/// angle_mode: 0 = linear, 1 = angle. measurement: 0 = Velocity, 1 = ErrorRateOfChange.
/// State is passed by pointer so managed keeps a single copy (the C# fallback
/// operates on the same fields). Returns 0 without touching state when
/// dt <= 0, mirroring the managed early-out.
#[no_mangle]
#[allow(clippy::too_many_arguments)]
pub extern "C" fn pid_update(
    error_last: *mut f32,
    value_last: *mut f32,
    integration_stored: *mut f32,
    derivative_last: *mut f32,
    derivative_initialized: *mut i32,
    proportional_gain: f32,
    integral_gain: f32,
    integral_saturation: f32,
    derivative_gain: f32,
    derivative_smoothing: f32,
    output_min: f32,
    output_max: f32,
    measurement: i32,
    delta_time: f32,
    current_value: f32,
    target_value: f32,
    angle_mode: i32,
) -> f32 {
    if error_last.is_null()
        || value_last.is_null()
        || integration_stored.is_null()
        || derivative_last.is_null()
        || derivative_initialized.is_null()
        || delta_time <= 0.0
    {
        return 0.0;
    }
    unsafe {
        let angled = angle_mode != 0;
        let err = if angled {
            m_delta_angle(current_value, target_value)
        } else {
            target_value - current_value
        };
        let err_rate = if angled {
            m_delta_angle(*error_last, err) / delta_time
        } else {
            (err - *error_last) / delta_time
        };
        *error_last = err;
        let vel = if angled {
            m_delta_angle(*value_last, current_value) / delta_time
        } else {
            (current_value - *value_last) / delta_time
        };
        *value_last = current_value;
        let v = *integration_stored + err * delta_time;
        *integration_stored = m_clamp(v, 0.0 - integral_saturation, integral_saturation);
        let mut d = 0.0;
        if *derivative_initialized != 0 {
            let b = if measurement != 0 { err_rate } else { 0.0 - vel };
            d = m_lerp(*derivative_last, b, derivative_smoothing);
            *derivative_last = d;
        } else {
            *derivative_initialized = 1;
            *derivative_last = 0.0;
        }
        m_clamp(
            proportional_gain * err
                + integral_gain * *integration_stored
                + derivative_gain * d,
            output_min,
            output_max,
        )
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn fresh() -> (f32, f32, f32, f32, i32) {
        (0.0, 0.0, 0.0, 0.0, 0)
    }

    #[test]
    fn pid_dt_nonpositive_returns_zero_without_touching_state() {
        let (mut e, mut v, mut i, mut d, mut init) = (1.0, 2.0, 3.0, 4.0, 1);
        let out = pid_update(
            &raw mut e, &raw mut v, &raw mut i, &raw mut d, &raw mut init,
            100.0, 10.0, 1000.0, 50.0, 1.0, f32::MIN, f32::MAX, 0, 0.0, 0.5, 1.0, 0,
        );
        assert_eq!(out, 0.0);
        assert_eq!((e, v, i, d, init), (1.0, 2.0, 3.0, 4.0, 1));
    }

    #[test]
    fn pid_first_linear_call_matches_managed() {
        // p=100 only: err=1, integration=0.02, derivative inits to 0, out=100.
        let (mut e, mut v, mut i, mut d, mut init) = fresh();
        let out = pid_update(
            &raw mut e, &raw mut v, &raw mut i, &raw mut d, &raw mut init,
            100.0, 0.0, f32::MAX, 0.0, 1.0, f32::MIN, f32::MAX, 0, 0.02, 0.0, 1.0, 0,
        );
        assert_eq!(out, 100.0);
        assert_eq!((e, v, init, d), (1.0, 0.0, 1, 0.0));
        assert_eq!(i, 0.02f32);
    }

    #[test]
    fn pid_second_call_velocity_derivative_matches_managed() {
        // err=0.5, vel=25, b=-25, d=lerp(0,-25,1)=-25, out=50-1250=-1200.
        let (mut e, mut v, mut i, mut d, mut init) = (1.0, 0.0, 0.02, 0.0, 1);
        let out = pid_update(
            &raw mut e, &raw mut v, &raw mut i, &raw mut d, &raw mut init,
            100.0, 0.0, f32::MAX, 50.0, 1.0, f32::MIN, f32::MAX, 0, 0.02, 0.5, 1.0, 0,
        );
        assert_eq!(out, -1200.0);
        assert_eq!((e, v, d), (0.5, 0.5, -25.0));
    }

    #[test]
    fn pid_angle_mode_wraps() {
        // DeltaAngle(179, -179) = +2.
        let (mut e, mut v, mut i, mut d, mut init) = fresh();
        let out = pid_update(
            &raw mut e, &raw mut v, &raw mut i, &raw mut d, &raw mut init,
            1.0, 0.0, f32::MAX, 0.0, 1.0, f32::MIN, f32::MAX, 0, 0.02, 179.0, -179.0, 1,
        );
        assert_eq!(out, 2.0);
        assert_eq!(e, 2.0);
    }

    #[test]
    fn math_helpers_match_unity() {
        assert_eq!(m_lerp(0.0, 10.0, 2.0), 10.0); // t clamped to 1
        assert_eq!(m_lerp(0.0, 10.0, -1.0), 0.0); // t clamped to 0
        assert_eq!(m_clamp(5.0, 0.0, 1.0), 1.0);
        assert!(m_clamp(f32::NAN, 0.0, 1.0).is_nan()); // NaN passes through
        assert_eq!(m_delta_angle(179.0, -179.0), 2.0);
        assert_eq!(m_delta_angle(-179.0, 179.0), -2.0);
    }

    fn mask_input(
        x: i16,
        y: i16,
        z: i16,
        rx: i16,
        ry: i16,
        rz: i16,
        rw: i16,
        vx: i16,
        vy: i16,
        vz: i16,
        ax: i16,
        ay: i16,
        az: i16,
        comp: u32,
        div: u8,
    ) -> SyncMaskInput {
        SyncMaskInput {
            x,
            y,
            z,
            rx,
            ry,
            rz,
            rw,
            vx,
            vy,
            vz,
            ax,
            ay,
            az,
            compressed_rotation: comp,
            tick_rate_divisor: div,
        }
    }

    fn zero_input() -> SyncMaskInput {
        mask_input(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1)
    }

    #[test]
    fn sync_mask_layout_is_32_bytes() {
        assert_eq!(std::mem::size_of::<SyncMaskInput>(), 32);
    }

    #[test]
    fn sync_mask_identical_is_zero() {
        let a = mask_input(100, -200, 300, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0xDEAD_BEEF, 2);
        assert_eq!(sync_mask(a, a, 0), 0);
        assert_eq!(sync_mask(a, a, 1), 0);
    }

    #[test]
    fn sync_mask_divisor_only() {
        let a = zero_input();
        let mut b = a;
        b.tick_rate_divisor = 2;
        assert_eq!(sync_mask(a, b, 0), 0x800);
    }

    #[test]
    fn sync_mask_position_bit() {
        // Decompressed step per short at x range: 50/65535 ~= 0.000763.
        // 0 vs 3 shorts => ~0.00229 > 0.002 threshold => bit 1.
        let a = zero_input();
        let mut b = a;
        b.x = 3;
        assert_eq!(sync_mask(a, b, 0) & 1, 1);
        b.x = 2; // ~0.00153 < 0.002 => no bit (rotation shorts still zero-equal)
        assert_eq!(sync_mask(a, b, 0), 0);
    }

    #[test]
    fn sync_mask_decompress_edge() {
        // Endpoints map exactly to range ends.
        assert_eq!(s_decomp(-32768, -25.0, 25.0), -25.0);
        assert_eq!(s_decomp(32767, -25.0, 25.0), 25.0);
        assert_eq!(s_decomp(0, -1.0, 1.0), s_decomp(0, -1.0, 1.0));
    }
}

// --- NetSphereCollider radius core (Puck.FixedUpdate) ---

const RADIUS_EPS: f32 = 0.00001;

/// Mirrors the radius logic exactly (snap up, Mathf.Lerp down). Returns 1 and
/// writes out_radius when the value changed enough to matter, else 0 so the
/// caller skips the managed collider write.
#[no_mangle]
pub extern "C" fn physics_puck_radius(
    grounded: i32,
    predicted_speed: f32,
    radius: f32,
    fixed_dt: f32,
    out_radius: *mut f32,
) -> i32 {
    if out_radius.is_null() {
        return 0;
    }
    let target = if grounded != 0 {
        0.0
    } else {
        (predicted_speed * 0.025).clamp(0.15, 0.75)
    };
    let next = if radius < target {
        target
    } else if radius > target {
        radius + (target - radius) * (fixed_dt * 5.0)
    } else {
        radius
    };
    if (next - radius).abs() > RADIUS_EPS {
        unsafe {
            *out_radius = next;
        }
        1
    } else {
        0
    }
}

#[repr(C)]
#[derive(Clone, Copy)]
struct Key {
    time: f32,
    value: f32,
    in_tangent: f32,
    out_tangent: f32,
}

static mut VOL_KEYS: Vec<Key> = Vec::new();
static mut PITCH_KEYS: Vec<Key> = Vec::new();
// NaN forces the first update through the gate.
static mut LAST_VOL: f32 = f32::NAN;
static mut LAST_PITCH: f32 = f32::NAN;

const EPS: f32 = 0.001;

/// Flat input: `[time, value, inTangent, outTangent] * n`. Resets the gate.
#[no_mangle]
pub extern "C" fn audio_curve_init(
    vol: *const f32,
    vol_keys: i32,
    pitch: *const f32,
    pitch_keys: i32,
) {
    unsafe {
        *(&raw mut VOL_KEYS) = copy_keys(vol, vol_keys);
        *(&raw mut PITCH_KEYS) = copy_keys(pitch, pitch_keys);
        *(&raw mut LAST_VOL) = f32::NAN;
        *(&raw mut LAST_PITCH) = f32::NAN;
    }
}

unsafe fn copy_keys(ptr: *const f32, n: i32) -> Vec<Key> {
    if ptr.is_null() || n <= 0 {
        return Vec::new();
    }
    let floats = std::slice::from_raw_parts(ptr, (n as usize) * 4);
    floats
        .chunks_exact(4)
        .map(|k| Key {
            time: k[0],
            value: k[1],
            in_tangent: k[2],
            out_tangent: k[3],
        })
        .collect()
}

fn evaluate(keys: &[Key], time: f32) -> f32 {
    if keys.is_empty() {
        return 0.0;
    }
    if time <= keys[0].time {
        return keys[0].value;
    }
    if time >= keys[keys.len() - 1].time {
        return keys[keys.len() - 1].value;
    }
    let i = keys.iter().position(|k| k.time > time).unwrap_or(1) - 1;
    let (k0, k1) = (keys[i], keys[i + 1]);
    let dt = k1.time - k0.time;
    if dt <= 0.0 {
        return k0.value;
    }
    // Cubic Hermite, Unity-style tangent scaling (non-weighted keys).
    let t = (time - k0.time) / dt;
    let t2 = t * t;
    let t3 = t2 * t;
    let m0 = k0.out_tangent * dt;
    let m1 = k1.in_tangent * dt;
    (2.0 * t3 - 3.0 * t2 + 1.0) * k0.value
        + (t3 - 2.0 * t2 + t) * m0
        + (-2.0 * t3 + 3.0 * t2) * k1.value
        + (t3 - t2) * m1
}

fn changed(last: f32, next: f32) -> bool {
    last.is_nan() || (last - next).abs() > EPS
}

/// Returns 1 and writes outputs when volume or pitch changed, else 0.
#[no_mangle]
pub extern "C" fn audio_wind_update(
    speed: f32,
    max_speed: f32,
    out_vol: *mut f32,
    out_pitch: *mut f32,
) -> i32 {
    if out_vol.is_null() || out_pitch.is_null() {
        return 0;
    }
    unsafe {
        let t = (speed / max_speed).min(1.0);
        let vol = evaluate(&*(&raw const VOL_KEYS), t);
        let pitch = evaluate(&*(&raw const PITCH_KEYS), t);
        if changed(*(&raw const LAST_VOL), vol) || changed(*(&raw const LAST_PITCH), pitch) {
            *(&raw mut LAST_VOL) = vol;
            *(&raw mut LAST_PITCH) = pitch;
            *out_vol = vol;
            *out_pitch = pitch;
            1
        } else {
            0
        }
    }
}
