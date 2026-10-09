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
//! NOTE: despite the crate name, this library now covers both ported
//! Puck.FixedUpdate helpers (wind audio + net collider radius). One plugin,
//! one P/Invoke surface.

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
