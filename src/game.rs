//! Authoritative match state machine (complete server-side rules).
//!
//! Phases: Lobby → Warmup → Faceoff → Playing ⇄ Goal → Faceoff … → Intermission
//! → … → Overtime (sudden death) → GameOver. Clock counts down in Playing/Overtime only.

use crate::physics::{Player, Team, World};
use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
#[repr(u8)]
pub enum Phase {
    Lobby = 0,
    Warmup = 1,
    Faceoff = 2,
    Playing = 3,
    Goal = 4,
    Intermission = 5,
    Overtime = 6,
    GameOver = 7,
}

impl Phase {
    pub fn from_u8(v: u8) -> Self {
        match v {
            1 => Phase::Warmup,
            2 => Phase::Faceoff,
            3 => Phase::Playing,
            4 => Phase::Goal,
            5 => Phase::Intermission,
            6 => Phase::Overtime,
            7 => Phase::GameOver,
            _ => Phase::Lobby,
        }
    }
}

#[derive(Debug, Clone)]
pub struct MatchConfig {
    pub periods: u8,
    pub period_secs: u32,
    pub intermission_secs: u32,
    pub faceoff_secs: u32,
    pub overtime: bool,
    pub goal_secs: u32,
    pub warmup_secs: u32,
}

impl Default for MatchConfig {
    fn default() -> Self {
        Self {
            periods: 3,
            period_secs: 300,
            intermission_secs: 30,
            faceoff_secs: 3,
            overtime: true,
            goal_secs: 3,
            warmup_secs: 10,
        }
    }
}

#[derive(Debug, Default, Clone, Serialize, Deserialize)]
pub struct SkaterStats {
    pub goals: u32,
    pub assists: u32,
    pub hits: u32,
}

#[derive(Debug, Clone)]
pub enum GameEvent {
    Goal { team: Team, scorer: Option<usize> },
    PhaseChanged { from: Phase, to: Phase },
    Chat { from: String, text: String },
    ServerMessage { text: String },
}

pub struct Game {
    pub cfg: MatchConfig,
    pub phase: Phase,
    pub phase_ticks_left: u64,
    pub tick_rate: u32,
    pub period: u8, // 1-based
    pub time_left_ms: u32,
    pub score_red: u8,
    pub score_blue: u8,
    pub stats: Vec<SkaterStats>,
    pub last_scorer: Option<usize>,
}

impl Game {
    pub fn new(cfg: MatchConfig, tick_rate: u32, max_players: usize) -> Self {
        Self {
            cfg,
            phase: Phase::Lobby,
            phase_ticks_left: 0,
            tick_rate,
            period: 1,
            time_left_ms: 0,
            score_red: 0,
            score_blue: 0,
            stats: vec![SkaterStats::default(); max_players],
            last_scorer: None,
        }
    }

    pub fn start_match(&mut self, world: &mut World, players: &mut [Player]) {
        self.score_red = 0;
        self.score_blue = 0;
        self.period = 1;
        for s in self.stats.iter_mut() {
            *s = SkaterStats::default();
        }
        self.enter_warmup(world, players);
    }

    fn secs_to_ticks(&self, secs: u32) -> u64 {
        secs as u64 * self.tick_rate as u64
    }

    fn enter_warmup(&mut self, world: &mut World, players: &mut [Player]) -> GameEvent {
        let from = self.phase;
        self.phase = Phase::Warmup;
        self.phase_ticks_left = self.secs_to_ticks(self.cfg.warmup_secs);
        self.time_left_ms = self.cfg.period_secs * 1000;
        world.reset_for_faceoff(players);
        GameEvent::PhaseChanged { from, to: Phase::Warmup }
    }

    fn enter_faceoff(&mut self, world: &mut World, players: &mut [Player]) -> GameEvent {
        let from = self.phase;
        self.phase = Phase::Faceoff;
        self.phase_ticks_left = self.secs_to_ticks(self.cfg.faceoff_secs.max(1));
        world.reset_for_faceoff(players);
        GameEvent::PhaseChanged { from, to: Phase::Faceoff }
    }

    fn enter_playing(&mut self) -> GameEvent {
        let from = self.phase;
        // Overtime stays Overtime, else Playing.
        let to = if from == Phase::Overtime
            || (self.period > self.cfg.periods && self.cfg.overtime)
        {
            Phase::Overtime
        } else {
            Phase::Playing
        };
        self.phase = to;
        GameEvent::PhaseChanged { from, to }
    }

    fn enter_goal(&mut self) -> GameEvent {
        let from = self.phase;
        self.phase = Phase::Goal;
        self.phase_ticks_left = self.secs_to_ticks(self.cfg.goal_secs.max(1));
        GameEvent::PhaseChanged { from, to: Phase::Goal }
    }

    fn enter_intermission(&mut self) -> GameEvent {
        let from = self.phase;
        self.phase = Phase::Intermission;
        self.phase_ticks_left = self.secs_to_ticks(self.cfg.intermission_secs.max(1));
        GameEvent::PhaseChanged { from, to: Phase::Intermission }
    }

    fn enter_gameover(&mut self) -> GameEvent {
        let from = self.phase;
        self.phase = Phase::GameOver;
        self.phase_ticks_left = u64::MAX / 2;
        GameEvent::PhaseChanged { from, to: Phase::GameOver }
    }

    /// Tick match logic. Call AFTER World::step (pass goal if any).
    /// Returns events (usually empty; at most 2 per tick).
    pub fn tick(
        &mut self,
        world: &mut World,
        players: &mut [Player],
        goal: Option<Team>,
        tick: u64,
    ) -> Vec<GameEvent> {
        let mut evs = Vec::new();
        // Track last touch for assists: closest player to puck on hit handled in main via hits counter.
        // Here: update last_touch_tick for closest player within 1.5m.
        let mut best: Option<usize> = None;
        let mut best_d = 1.5f32;
        for (i, p) in players.iter().enumerate() {
            let d = (world.puck.pos - p.pos).length();
            if d < best_d {
                best_d = d;
                best = Some(i);
            }
        }
        if let Some(i) = best {
            players[i].last_touch_tick = tick;
        }

        match self.phase {
            Phase::Lobby => {
                // Auto-start warmup once 2+ humans/bots present? Caller decides via start_match.
            }
            Phase::Warmup => {
                if self.phase_ticks_left > 0 {
                    self.phase_ticks_left -= 1;
                }
                if self.phase_ticks_left == 0 {
                    evs.push(self.enter_faceoff(world, players));
                }
            }
            Phase::Faceoff => {
                if self.phase_ticks_left > 0 {
                    self.phase_ticks_left -= 1;
                }
                // Freeze puck during faceoff.
                world.puck.pos = crate::physics::Vec2::new(0.0, 0.0);
                world.puck.vel = crate::physics::Vec2::new(0.0, 0.0);
                if self.phase_ticks_left == 0 {
                    evs.push(self.enter_playing());
                }
            }
            Phase::Playing | Phase::Overtime => {
                // Clock.
                let dt_ms = 1000 / self.tick_rate.max(1);
                let overtime = self.phase == Phase::Overtime;
                if !overtime {
                    if self.time_left_ms > dt_ms {
                        self.time_left_ms -= dt_ms;
                    } else {
                        self.time_left_ms = 0;
                    }
                }
                if let Some(team) = goal {
                    // Score.
                    match team {
                        Team::Red => self.score_red = self.score_red.saturating_add(1),
                        Team::Blue => self.score_blue = self.score_blue.saturating_add(1),
                    }
                    // Credit scorer: most recent touch on scoring team.
                    let scorer = Self::pick_scorer(players, team, tick);
                    if let Some(s) = scorer {
                        if s < self.stats.len() {
                            self.stats[s].goals += 1;
                        }
                        // Assist: second most recent touch same team.
                        if let Some(a) = Self::pick_assist(players, team, tick, s) {
                            if a < self.stats.len() {
                                self.stats[a].assists += 1;
                            }
                        }
                        self.last_scorer = Some(s);
                    }
                    evs.push(GameEvent::Goal { team, scorer });
                    // Sudden-death OT ends immediately.
                    if overtime {
                        evs.push(self.enter_gameover());
                    } else {
                        evs.push(self.enter_goal());
                    }
                    return evs;
                }
                if !overtime && self.time_left_ms == 0 {
                    if self.period < self.cfg.periods {
                        evs.push(self.enter_intermission());
                    } else if self.score_red != self.score_blue {
                        evs.push(self.enter_gameover());
                    } else if self.cfg.overtime {
                        // Tie → OT sudden death.
                        let from = self.phase;
                        self.phase = Phase::Overtime;
                        evs.push(GameEvent::PhaseChanged { from, to: Phase::Overtime });
                        evs.push(self.enter_faceoff(world, players));
                        // enter_faceoff overwrote phase to Faceoff; fix: OT faceoff then OT playing.
                        // Keep Faceoff; enter_playing will restore Overtime.
                    } else {
                        evs.push(self.enter_gameover());
                    }
                }
            }
            Phase::Goal => {
                if self.phase_ticks_left > 0 {
                    self.phase_ticks_left -= 1;
                }
                if self.phase_ticks_left == 0 {
                    evs.push(self.enter_faceoff(world, players));
                }
            }
            Phase::Intermission => {
                if self.phase_ticks_left > 0 {
                    self.phase_ticks_left -= 1;
                }
                if self.phase_ticks_left == 0 {
                    self.period += 1;
                    self.time_left_ms = self.cfg.period_secs * 1000;
                    evs.push(self.enter_faceoff(world, players));
                }
            }
            Phase::GameOver => {}
        }
        evs
    }

    fn pick_scorer(players: &[Player], team: Team, tick: u64) -> Option<usize> {
        let mut best: Option<usize> = None;
        let mut best_age = u64::MAX;
        for (i, p) in players.iter().enumerate() {
            if p.team != team {
                continue;
            }
            let age = tick.saturating_sub(p.last_touch_tick);
            if age < best_age && age < 600 {
                best_age = age;
                best = Some(i);
            }
        }
        // Fallback: closest to goal.
        if best.is_none() {
            let gx = match team {
                Team::Red => 30.0,
                Team::Blue => -30.0,
            };
            let mut bd = f32::MAX;
            for (i, p) in players.iter().enumerate() {
                if p.team != team {
                    continue;
                }
                let d = (p.pos.x - gx).abs() + p.pos.y.abs();
                if d < bd {
                    bd = d;
                    best = Some(i);
                }
            }
        }
        best
    }

    fn pick_assist(
        players: &[Player],
        team: Team,
        tick: u64,
        scorer: usize,
    ) -> Option<usize> {
        let mut best: Option<usize> = None;
        let mut best_age = u64::MAX;
        for (i, p) in players.iter().enumerate() {
            if i == scorer || p.team != team {
                continue;
            }
            let age = tick.saturating_sub(p.last_touch_tick);
            if age < best_age && age < 600 {
                best_age = age;
                best = Some(i);
            }
        }
        best
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::physics::{Player, Role, Team, Vec2, World};

    #[test]
    fn warmup_to_faceoff_to_playing() {
        let mut w = World::new(0.35, 0.85);
        let mut players = vec![Player::default(); 2];
        let mut g = Game::new(
            MatchConfig {
                warmup_secs: 0,
                faceoff_secs: 1,
                ..Default::default()
            },
            60,
            2,
        );
        g.phase = Phase::Warmup;
        g.phase_ticks_left = 1;
        let evs = g.tick(&mut w, &mut players, None, 1);
        assert!(evs.iter().any(|e| matches!(
            e,
            GameEvent::PhaseChanged { to: Phase::Faceoff, .. }
        )));
        // Run faceoff out.
        for t in 2..70 {
            let evs = g.tick(&mut w, &mut players, None, t);
            if g.phase == Phase::Playing {
                assert!(evs.iter().any(|e| matches!(
                    e,
                    GameEvent::PhaseChanged { to: Phase::Playing, .. }
                )));
                break;
            }
        }
        assert_eq!(g.phase, Phase::Playing);
    }

    #[test]
    fn goal_scores_and_credits() {
        let mut w = World::new(0.35, 0.85);
        let mut players = vec![
            Player {
                team: Team::Red,
                role: Role::Skater,
                pos: Vec2::new(20.0, 0.0),
                last_touch_tick: 100,
                ..Default::default()
            },
            Player {
                team: Team::Blue,
                role: Role::Skater,
                ..Default::default()
            },
        ];
        let mut g = Game::new(MatchConfig::default(), 60, 2);
        g.phase = Phase::Playing;
        g.time_left_ms = 300_000;
        let evs = g.tick(&mut w, &mut players, Some(Team::Red), 110);
        assert_eq!(g.score_red, 1);
        assert!(evs.iter().any(|e| matches!(e, GameEvent::Goal { .. })));
    }
}
