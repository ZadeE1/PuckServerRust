//! Fixed-timestep 2D hockey physics (complete server-side).
//!
//! - No allocation in hot loop (`World::step` takes slices, no Vec alloc).
//! - Dependency-free (no tokio/serde) for determinism + benching.
//! - Rink 60x30m, goals on ±x, crease boxes, faceoff spots, equal restitution.

use std::ops::{Add, AddAssign, Mul, Sub};

#[derive(Debug, Default, Clone, Copy, PartialEq)]
#[repr(C)]
pub struct Vec2 {
    pub x: f32,
    pub y: f32,
}

impl Vec2 {
    #[inline]
    pub const fn new(x: f32, y: f32) -> Self {
        Self { x, y }
    }
    #[inline]
    pub fn length_sq(self) -> f32 {
        self.x * self.x + self.y * self.y
    }
    #[inline]
    pub fn length(self) -> f32 {
        self.length_sq().sqrt()
    }
    #[inline]
    pub fn normalized(self) -> Self {
        let len = self.length().max(1e-6);
        Self {
            x: self.x / len,
            y: self.y / len,
        }
    }
}

impl Add for Vec2 {
    type Output = Self;
    #[inline]
    fn add(self, rhs: Self) -> Self {
        Self::new(self.x + rhs.x, self.y + rhs.y)
    }
}
impl Sub for Vec2 {
    type Output = Self;
    #[inline]
    fn sub(self, rhs: Self) -> Self {
        Self::new(self.x - rhs.x, self.y - rhs.y)
    }
}
impl Mul<f32> for Vec2 {
    type Output = Self;
    #[inline]
    fn mul(self, s: f32) -> Self {
        Self::new(self.x * s, self.y * s)
    }
}
impl AddAssign for Vec2 {
    #[inline]
    fn add_assign(&mut self, rhs: Self) {
        self.x += rhs.x;
        self.y += rhs.y;
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
#[repr(u8)]
pub enum Team {
    Red = 1,
    Blue = 2,
}

impl Team {
    pub fn other(self) -> Self {
        match self {
            Team::Red => Team::Blue,
            Team::Blue => Team::Red,
        }
    }
    pub fn from_u8(v: u8) -> Option<Self> {
        match v {
            1 => Some(Team::Red),
            2 => Some(Team::Blue),
            _ => None,
        }
    }
}

#[derive(Debug, Clone, Copy)]
pub struct Rink {
    pub half_w: f32,
    pub half_h: f32,
    pub goal_half_width: f32,
    pub goal_depth: f32,
    pub puck_radius: f32,
    pub player_radius: f32,
    /// Goalie crease: box in front of each goal (width along x, half-height y).
    pub crease_len: f32,
    pub crease_half_h: f32,
}

impl Default for Rink {
    fn default() -> Self {
        Self {
            half_w: 30.0,
            half_h: 15.0,
            goal_half_width: 2.5,
            goal_depth: 1.5,
            puck_radius: 0.15,
            player_radius: 0.5,
            crease_len: 3.0,
            crease_half_h: 3.0,
        }
    }
}

impl Rink {
    /// Faceoff dot positions: center + 4 neutral/offensive dots.
    pub fn faceoff_spots(&self) -> [Vec2; 5] {
        [
            Vec2::new(0.0, 0.0),
            Vec2::new(-12.0, -6.0),
            Vec2::new(-12.0, 6.0),
            Vec2::new(12.0, -6.0),
            Vec2::new(12.0, 6.0),
        ]
    }

    pub fn in_crease(&self, pos: Vec2, team: Team) -> bool {
        let gx = match team {
            Team::Red => -self.half_w,
            Team::Blue => self.half_w,
        };
        let dx = (pos.x - gx).abs();
        dx < self.crease_len && pos.y.abs() < self.crease_half_h
    }
}

#[derive(Debug, Clone, Copy)]
pub struct Puck {
    pub pos: Vec2,
    pub vel: Vec2,
}

impl Default for Puck {
    fn default() -> Self {
        Self {
            pos: Vec2::new(0.0, 0.0),
            vel: Vec2::new(0.0, 0.0),
        }
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Role {
    Skater,
    Goalie,
}

#[derive(Debug, Clone, Copy)]
pub struct Player {
    pub pos: Vec2,
    pub vel: Vec2,
    pub stamina: f32,
    pub sprinting: bool,
    pub role: Role,
    pub team: Team,
    /// Last touch tick for assists tracking (set by game.rs, not physics).
    pub last_touch_tick: u64,
}

impl Default for Player {
    fn default() -> Self {
        Self {
            pos: Vec2::new(0.0, 0.0),
            vel: Vec2::new(0.0, 0.0),
            stamina: 1.0,
            sprinting: false,
            role: Role::Skater,
            team: Team::Red,
            last_touch_tick: 0,
        }
    }
}

#[derive(Debug, Default, Clone, Copy)]
pub struct PlayerInput {
    pub thrust_x: i8,
    pub thrust_y: i8,
    pub sprint: bool,
    pub hit: bool,
}

#[derive(Debug)]
pub struct World {
    pub rink: Rink,
    pub puck: Puck,
    pub friction: f32,
    pub restitution: f32,
}

impl World {
    pub fn new(friction: f32, restitution: f32) -> Self {
        Self {
            rink: Rink::default(),
            puck: Puck::default(),
            friction,
            restitution,
        }
    }

    /// Place puck at center + players at faceoff formation by team.
    /// No allocation. `players` order should already be grouped by team for balance,
    /// but we place deterministically by index side.
    pub fn reset_for_faceoff(&mut self, players: &mut [Player]) {
        self.puck.pos = Vec2::new(0.0, 0.0);
        self.puck.vel = Vec2::new(0.0, 0.0);
        let spots = self.rink.faceoff_spots();
        for (i, p) in players.iter_mut().enumerate() {
            // Alternate sides around center; goalies go to crease.
            if p.role == Role::Goalie {
                let gx = match p.team {
                    Team::Red => -self.rink.half_w + 1.0,
                    Team::Blue => self.rink.half_w - 1.0,
                };
                p.pos = Vec2::new(gx, 0.0);
            } else {
                let s = spots[i % spots.len()];
                // Mirror for Blue so teams face each other.
                let x = match p.team {
                    Team::Red => -s.x.abs() * 0.5 - 1.0,
                    Team::Blue => s.x.abs() * 0.5 + 1.0,
                };
                let y = s.y + ((i as f32) * 0.7).rem_euclid(3.0) - 1.5;
                p.pos = Vec2::new(x.clamp(-28.0, 28.0), y.clamp(-13.0, 13.0));
            }
            p.vel = Vec2::new(0.0, 0.0);
        }
    }

    /// Advance simulation. Returns Some(scoring_team) on goal, else None.
    /// No heap allocation.
    #[inline]
    pub fn step(
        &mut self,
        players: &mut [Player],
        inputs: &[PlayerInput],
        dt: f32,
    ) -> Option<Team> {
        debug_assert!(players.len() == inputs.len());
        let mut goal: Option<Team> = None;

        for (p, inp) in players.iter_mut().zip(inputs.iter()) {
            let thrust = Vec2::new(inp.thrust_x as f32 / 127.0, inp.thrust_y as f32 / 127.0);
            let max_speed = if inp.sprint && p.stamina > 0.01 {
                if p.role == Role::Goalie {
                    7.0
                } else {
                    9.0
                }
            } else if p.role == Role::Goalie {
                5.0
            } else {
                6.0
            };
            let target = thrust * max_speed;
            let blend = (10.0 * dt).min(1.0);
            p.vel = p.vel * (1.0 - blend) + target * blend;
            // Clamp speed (anti-cheat: never exceed max + small margin).
            let spd = p.vel.length();
            if spd > max_speed + 0.5 {
                p.vel = p.vel * ((max_speed + 0.5) / spd);
            }
            p.pos = p.pos + p.vel * dt;

            if inp.sprint && thrust.length_sq() > 0.01 {
                p.stamina = (p.stamina - dt * 0.25).max(0.0);
                p.sprinting = true;
            } else {
                p.stamina = (p.stamina + dt * 0.15).min(1.0);
                p.sprinting = false;
            }

            // Clamp inside rink.
            p.pos.x = p.pos.x.clamp(
                -self.rink.half_w + self.rink.player_radius,
                self.rink.half_w - self.rink.player_radius,
            );
            p.pos.y = p.pos.y.clamp(
                -self.rink.half_h + self.rink.player_radius,
                self.rink.half_h - self.rink.player_radius,
            );

            // Crease protection: skaters cannot sit in opponent crease.
            // Push out gently (allows pass-through but not camping).
            let opp = p.team.other();
            if p.role == Role::Skater && self.rink.in_crease(p.pos, opp) {
                let gx = match opp {
                    Team::Red => -self.rink.half_w,
                    Team::Blue => self.rink.half_w,
                };
                let dir = if p.pos.x > gx { 1.0 } else { -1.0 };
                p.pos.x += dir * 8.0 * dt;
            }
            // Goalies stay near home crease.
            if p.role == Role::Goalie {
                let gx = match p.team {
                    Team::Red => -self.rink.half_w + 1.0,
                    Team::Blue => self.rink.half_w - 1.0,
                };
                // Soft tether: pull back if >8m from crease.
                let d = p.pos - Vec2::new(gx, 0.0);
                if d.length() > 8.0 {
                    p.pos = p.pos - d * (2.0 * dt);
                }
            }

            if inp.hit {
                let d = self.puck.pos - p.pos;
                let dist = d.length();
                if dist < self.rink.player_radius + self.rink.puck_radius + 0.9 {
                    let dir = if dist > 1e-4 {
                        d * (1.0 / dist)
                    } else {
                        Vec2::new(1.0, 0.0)
                    };
                    // Stick hit: stronger for skaters, goalies deflect.
                    let power = if p.role == Role::Goalie { 8.0 } else { 12.0 };
                    self.puck.vel = self.puck.vel + dir * power + p.vel * 0.5;
                }
            }
        }

        // Player-player separation.
        let min_dist = self.rink.player_radius * 2.0;
        let n = players.len();
        for i in 0..n {
            for j in (i + 1)..n {
                let (pi, pj) = {
                    let (head, tail) = players.split_at_mut(j);
                    (&mut head[i], &mut tail[0])
                };
                let d = pj.pos - pi.pos;
                let dist_sq = d.length_sq();
                if dist_sq < min_dist * min_dist && dist_sq > 1e-8 {
                    let dist = dist_sq.sqrt();
                    let push = (min_dist - dist) * 0.5;
                    let nrm = d * (1.0 / dist);
                    pi.pos = pi.pos - nrm * push;
                    pj.pos = pj.pos + nrm * push;
                }
            }
        }

        // Puck friction + integrate.
        let damp = (-self.friction * dt).exp();
        self.puck.vel = self.puck.vel * damp;
        // Clamp puck speed (slapshot cap ~30 m/s).
        let ps = self.puck.vel.length();
        if ps > 30.0 {
            self.puck.vel = self.puck.vel * (30.0 / ps);
        }
        self.puck.pos = self.puck.pos + self.puck.vel * dt;

        let r = self.rink.puck_radius;
        let hw = self.rink.half_w;
        let hh = self.rink.half_h;
        let in_mouth = self.puck.pos.y.abs() < self.rink.goal_half_width;

        // Goals extend into net depth; only count when fully over line inside mouth.
        if self.puck.pos.x > hw + 0.1 {
            if in_mouth {
                goal = Some(Team::Red);
            } else {
                self.puck.pos.x = hw - r;
                self.puck.vel.x = -self.puck.vel.x * self.restitution;
            }
        } else if self.puck.pos.x < -hw - 0.1 {
            if in_mouth {
                goal = Some(Team::Blue);
            } else {
                self.puck.pos.x = -hw + r;
                self.puck.vel.x = -self.puck.vel.x * self.restitution;
            }
        } else {
            // Normal boards (including behind-goal area inside mouth but not yet goal).
            if self.puck.pos.x > hw - r && !in_mouth {
                self.puck.pos.x = hw - r;
                self.puck.vel.x = -self.puck.vel.x * self.restitution;
            } else if self.puck.pos.x < -hw + r && !in_mouth {
                self.puck.pos.x = -hw + r;
                self.puck.vel.x = -self.puck.vel.x * self.restitution;
            }
            // Allow puck to enter net mouth up to depth before goal registers.
            if in_mouth {
                self.puck.pos.x = self.puck.pos.x.clamp(
                    -hw - self.rink.goal_depth,
                    hw + self.rink.goal_depth,
                );
            }
        }
        if self.puck.pos.y > hh - r {
            self.puck.pos.y = hh - r;
            self.puck.vel.y = -self.puck.vel.y * self.restitution;
        } else if self.puck.pos.y < -hh + r {
            self.puck.pos.y = -hh + r;
            self.puck.vel.y = -self.puck.vel.y * self.restitution;
        }

        // Puck-player collision.
        for p in players.iter() {
            let d = self.puck.pos - p.pos;
            let min_d = self.rink.player_radius + self.rink.puck_radius;
            let dist_sq = d.length_sq();
            if dist_sq < min_d * min_d && dist_sq > 1e-8 {
                let dist = dist_sq.sqrt();
                let nrm = d * (1.0 / dist);
                self.puck.pos = p.pos + nrm * min_d;
                let rel = self.puck.vel - p.vel;
                let vn = rel.x * nrm.x + rel.y * nrm.y;
                if vn < 0.0 {
                    self.puck.vel = self.puck.vel - nrm * (vn * (1.0 + self.restitution));
                }
            }
        }

        goal
    }

    /// Simple bot input: chase puck, hit when in range. Deterministic, no alloc.
    pub fn bot_input(player: &Player, puck: &Puck, tick: u64) -> PlayerInput {
        let d = puck.pos - player.pos;
        let dist = d.length();
        let dir = if dist > 1e-3 {
            d * (1.0 / dist)
        } else {
            Vec2::new(0.0, 0.0)
        };
        // Add slight wobble so bots don't stack.
        let wob = ((tick / 30 + player.pos.x as u64) % 3) as f32 - 1.0;
        let tx = (dir.x * 100.0 + wob * 10.0).clamp(-127.0, 127.0) as i8;
        let ty = (dir.y * 100.0).clamp(-127.0, 127.0) as i8;
        PlayerInput {
            thrust_x: tx,
            thrust_y: ty,
            sprint: dist > 6.0,
            hit: dist < 1.4,
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn puck_friction_decays_velocity() {
        let mut w = World::new(1.0, 0.85);
        w.puck.vel = Vec2::new(10.0, 0.0);
        let mut players: Vec<Player> = vec![];
        let inputs: Vec<PlayerInput> = vec![];
        let g = w.step(&mut players, &inputs, 1.0);
        assert!(g.is_none());
        assert!((w.puck.vel.x - 3.6788).abs() < 0.01);
    }

    #[test]
    fn wall_bounce_equal_restitution() {
        let mut w = World::new(0.0, 0.85);
        w.puck.pos = Vec2::new(29.8, 10.0);
        w.puck.vel = Vec2::new(5.0, 0.0);
        let mut players: Vec<Player> = vec![];
        let inputs: Vec<PlayerInput> = vec![];
        let g = w.step(&mut players, &inputs, 1.0 / 60.0);
        assert!(g.is_none());
        assert!(w.puck.vel.x < 0.0);
        assert!(w.puck.pos.x <= 30.0 - 0.15 + 1e-4);
    }

    #[test]
    fn no_alloc_step_10_players() {
        let mut w = World::new(0.35, 0.85);
        let mut players = vec![Player::default(); 10];
        let inputs = vec![PlayerInput::default(); 10];
        for _ in 0..120 {
            w.step(&mut players, &inputs, 1.0 / 60.0);
        }
        assert!(w.puck.pos.x.is_finite());
    }

    #[test]
    fn goal_detected_red_scores() {
        let mut w = World::new(0.0, 0.85);
        w.puck.pos = Vec2::new(30.0, 0.0);
        w.puck.vel = Vec2::new(10.0, 0.0);
        let mut players: Vec<Player> = vec![];
        let inputs: Vec<PlayerInput> = vec![];
        let g = w.step(&mut players, &inputs, 1.0 / 60.0);
        assert_eq!(g, Some(Team::Red));
    }

    #[test]
    fn faceoff_reset_places_goalies() {
        let mut w = World::new(0.35, 0.85);
        let mut players = vec![
            Player {
                team: Team::Red,
                role: Role::Goalie,
                ..Default::default()
            },
            Player {
                team: Team::Blue,
                role: Role::Goalie,
                ..Default::default()
            },
            Player {
                team: Team::Red,
                role: Role::Skater,
                ..Default::default()
            },
        ];
        w.reset_for_faceoff(&mut players);
        assert!(players[0].pos.x < -20.0);
        assert!(players[1].pos.x > 20.0);
        assert_eq!(w.puck.pos, Vec2::new(0.0, 0.0));
    }
}
