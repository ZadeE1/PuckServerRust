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
//! per-tick helpers (wind audio + net collider radius + PID). One plugin,
//! one P/Invoke surface.

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
