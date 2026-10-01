//! PhysX 4.1-parity primitives for the exact Unity physics calls Puck uses.
//!
//! Pinned upstream: `H:\PhysX` @ `a2c0428` (branch `4.1`, Unity 6000.3.14f1's
//! integration family — Unity "upgrade to PhysX 4.1" line). Puck.dll call set
//! was isolated via Mono.Cecil member refs: Rigidbody Move/AddForce/AddTorque,
//! linear/angular velocity, CheckSphere, Raycast, Simulate, sphere colliders,
//! contact normal/point/relativeVelocity, CCD mode switching.
//! No heap allocation; no external deps (same rule as physics.rs).

use crate::physics::Vec2;

/// Linear damping factor. PhysX does NOT use exponential damping:
/// `v *= max(0, 1 - damping*dt)`.
// ponytail: direct port; see DyBodyCoreIntegrator.h:59-75 in pinned repo.
#[inline]
pub fn damping_factor(damping: f32, dt: f32) -> f32 {
    (1.0 - damping * dt).max(0.0)
}

/// Unconstrained velocity update: gravity, then linear damping, then clamps.
/// `accel_scale` is 1.0 (Puck never touches it).
// ponytail: mirrors bodyCoreComputeUnconstrainedVelocity, DyBodyCoreIntegrator.h:48-91.
#[inline]
pub fn integrate_velocity(
    vel: &mut Vec2,
    gravity: Vec2,
    damping: f32,
    dt: f32,
    max_speed_sq: f32,
    disable_gravity: bool,
) {
    if !disable_gravity {
        *vel = *vel + gravity * dt;
    }
    *vel = *vel * damping_factor(damping, dt);
    let s2 = vel.length_sq();
    if s2 > max_speed_sq && s2 > 0.0 {
        *vel = *vel * ((max_speed_sq / s2).sqrt());
    }
}

/// Semi-implicit Euler position step: `p += v*dt` after velocity is final.
// ponytail: mirrors integrateCore position part, DyBodyCoreIntegrator.h:133-138.
#[inline]
pub fn integrate_position(pos: &mut Vec2, vel: Vec2, dt: f32) {
    *pos = *pos + vel * dt;
}

/// Sphere-sphere discrete contact: normal + penetration depth.
// ponytail: same normal-along-delta convention as GuContactSphereSphere.cpp.
#[inline]
pub fn sphere_sphere_contact(
    a_pos: Vec2,
    a_radius: f32,
    b_pos: Vec2,
    b_radius: f32,
) -> Option<(Vec2, f32)> {
    let d = b_pos - a_pos;
    let min_d = a_radius + b_radius;
    let dist_sq = d.length_sq();
    if dist_sq < min_d * min_d && dist_sq > 1e-8 {
        let dist = dist_sq.sqrt();
        Some((d * (1.0 / dist), min_d - dist))
    } else {
        None
    }
}

/// Contact velocity resolve: kill approaching normal velocity, apply
/// restitution only above `bounce_threshold` (Unity `Physics.bounceThreshold`
/// default 2.0). Restitution passed in is the pair combine (PhysX/Unity
/// default combine mode is average).
#[inline]
pub fn resolve_contact(
    vel: &mut Vec2,
    nrm: Vec2,
    restitution: f32,
    bounce_threshold: f32,
) {
    let vn = vel.x * nrm.x + vel.y * nrm.y;
    if vn < 0.0 {
        if -vn > bounce_threshold {
            *vel = *vel - nrm * (vn * (1.0 + restitution));
        } else {
            *vel = *vel - nrm * vn;
        }
    }
}

/// Swept sphere vs static plane (boards/ice) time-of-impact in [0, dt].
/// CCD path: Unity `collisionDetectionMode = Continuous` (Puck switches the
/// puck to CCD at speed via PuckCollisionDetectionModeSwitcher).
/// Plane is `dot(n, x) = offset` with unit normal `n`.
pub fn sweep_sphere_vs_plane(
    pos: Vec2,
    vel: Vec2,
    radius: f32,
    nrm: Vec2,
    offset: f32,
    dt: f32,
) -> Option<f32> {
    let dist = offset - (pos.x * nrm.x + pos.y * nrm.y);
    let closing = vel.x * nrm.x + vel.y * nrm.y;
    if closing <= 0.0 || dist <= radius {
        return None;
    }
    let toi = (dist - radius) / closing;
    if toi >= 0.0 && toi <= dt {
        Some(toi)
    } else {
        None
    }
}

/// Closest-hit ray vs spheres (Unity `Physics.Raycast` single-hit path used
/// by StickPositioner blade raycasts). Returns (distance, sphere index).
// ponytail: closest-hit convention matches PxScene::raycast single-hit.
pub fn raycast_spheres(
    origin: Vec2,
    dir: Vec2,
    max_dist: f32,
    spheres: &[(Vec2, f32)],
) -> Option<(f32, usize)> {
    let mut best: Option<(f32, usize)> = None;
    for (i, &(c, r)) in spheres.iter().enumerate() {
        let oc = c - origin;
        let tca = oc.x * dir.x + oc.y * dir.y;
        if tca < 0.0 {
            continue;
        }
        let d2 = oc.length_sq() - tca * tca;
        if d2 > r * r {
            continue;
        }
        let thc = (r * r - d2).sqrt();
        let t = (tca - thc).max(0.0);
        if t <= max_dist && best.map_or(true, |(bt, _)| t < bt) {
            best = Some((t, i));
        }
    }
    best
}

/// Any-hit sphere overlap (Unity `Physics.CheckSphere`, e.g. puck grounded
/// checks). Matches `PxScene::overlap` any-hit: true on first touch.
pub fn overlap_sphere_any(center: Vec2, radius: f32, spheres: &[(Vec2, f32)]) -> bool {
    spheres.iter().any(|&(c, r)| {
        let min_d = radius + r;
        (c - center).length_sq() < min_d * min_d
    })
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn damping_is_linear_not_exponential() {
        // PhysX: v *= max(0, 1-d*dt). v=10, d=1, dt=1 -> 0 (exp would give 3.68).
        assert_eq!(damping_factor(1.0, 1.0), 0.0);
        assert!((damping_factor(0.35, 1.0 / 60.0) - (1.0 - 0.35 / 60.0)).abs() < 1e-7);
        // Never flips sign at huge dt.
        assert_eq!(damping_factor(100.0, 1.0), 0.0);
    }

    #[test]
    fn velocity_integrate_clamps_like_physx() {
        let mut v = Vec2::new(40.0, 0.0);
        integrate_velocity(&mut v, Vec2::new(0.0, 0.0), 0.0, 1.0 / 60.0, 900.0, true);
        assert!((v.length() - 30.0).abs() < 1e-4);
    }

    #[test]
    fn sphere_contact_normal_and_penetration() {
        let (n, pen) =
            sphere_sphere_contact(Vec2::new(0.0, 0.0), 0.5, Vec2::new(0.8, 0.0), 0.5).unwrap();
        assert!((n.x - 1.0).abs() < 1e-6 && n.y.abs() < 1e-6);
        assert!((pen - 0.2).abs() < 1e-6);
        assert!(sphere_sphere_contact(Vec2::new(0.0, 0.0), 0.5, Vec2::new(5.0, 0.0), 0.5).is_none());
    }

    #[test]
    fn restitution_only_above_bounce_threshold() {
        let mut slow = Vec2::new(-1.0, 0.0); // below 2.0 default threshold
        resolve_contact(&mut slow, Vec2::new(1.0, 0.0), 0.85, 2.0);
        assert!(slow.x.abs() < 1e-6); // stopped, no bounce
        let mut fast = Vec2::new(-5.0, 0.0);
        resolve_contact(&mut fast, Vec2::new(1.0, 0.0), 0.85, 2.0);
        assert!((fast.x - 4.25).abs() < 1e-5); // 5*0.85 reflected
    }

    #[test]
    fn sweep_finds_board_toi_for_fast_puck() {
        // Puck at x=29.7 moving +x at 30 m/s, board plane x=30.
        let toi = sweep_sphere_vs_plane(
            Vec2::new(29.7, 0.0),
            Vec2::new(30.0, 0.0),
            0.15,
            Vec2::new(1.0, 0.0),
            30.0,
            1.0 / 60.0,
        )
        .unwrap();
        assert!((toi - (0.3 - 0.15) / 30.0).abs() < 1e-6);
        // Slow puck that cannot reach the board this tick: no TOI.
        assert!(sweep_sphere_vs_plane(
            Vec2::new(0.0, 0.0),
            Vec2::new(1.0, 0.0),
            0.15,
            Vec2::new(1.0, 0.0),
            30.0,
            1.0 / 60.0,
        )
        .is_none());
    }

    #[test]
    fn raycast_returns_closest() {
        let spheres = [(Vec2::new(5.0, 0.0), 1.0), (Vec2::new(3.0, 0.0), 1.0)];
        let (t, i) =
            raycast_spheres(Vec2::new(0.0, 0.0), Vec2::new(1.0, 0.0), 10.0, &spheres).unwrap();
        assert_eq!(i, 1);
        assert!((t - 2.0).abs() < 1e-6);
        assert!(
            raycast_spheres(Vec2::new(0.0, 0.0), Vec2::new(0.0, 1.0), 10.0, &spheres).is_none()
        );
    }

    #[test]
    fn overlap_any_matches_checksphere() {
        let spheres = [(Vec2::new(5.0, 0.0), 0.5)];
        assert!(overlap_sphere_any(Vec2::new(5.0, 0.0), 0.5, &spheres));
        assert!(!overlap_sphere_any(Vec2::new(0.0, 0.0), 0.5, &spheres));
    }
}
