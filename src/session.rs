//! Sessions: slots, auth (password/bans), teams, timeouts, chat commands.
//!
//! First connected client becomes host (can /kick /ban). Bans are IP-based
//! with file persistence handled by persistence.rs.

use std::collections::{HashMap, HashSet, VecDeque};
use std::net::{IpAddr, SocketAddr};

use crate::physics::Team;

#[derive(Debug, Clone)]
pub struct Slot {
    pub id: usize,
    pub name: String,
    pub team: Team,
    pub addr: Option<SocketAddr>,
    pub is_bot: bool,
    pub connected: bool,
    pub last_seen_tick: u64,
    pub last_input_seq: u16,
    pub next_event_seq: u32,
    /// Reliable events awaiting ack: (seq, bytes, last_sent_tick).
    pub pending: VecDeque<(u32, Vec<u8>, u64)>,
}

impl Slot {
    pub fn empty(id: usize) -> Self {
        Self {
            id,
            name: String::new(),
            team: if id % 2 == 0 { Team::Red } else { Team::Blue },
            addr: None,
            is_bot: false,
            connected: false,
            last_seen_tick: 0,
            last_input_seq: 0,
            next_event_seq: 1,
            pending: VecDeque::new(),
        }
    }
}

pub struct SessionManager {
    pub slots: Vec<Slot>,
    pub addr_to_id: HashMap<SocketAddr, usize>,
    pub bans: HashSet<IpAddr>,
    pub password: String,
    pub host_id: Option<usize>,
}

#[derive(Debug)]
pub enum JoinError {
    Full,
    BadPassword,
    Banned,
    BadName,
}

impl SessionManager {
    pub fn new(max_players: usize, password: String) -> Self {
        Self {
            slots: (0..max_players).map(Slot::empty).collect(),
            addr_to_id: HashMap::new(),
            bans: HashSet::new(),
            password,
            host_id: None,
        }
    }

    pub fn active_count(&self) -> usize {
        self.slots.iter().filter(|s| s.connected).count()
    }

    fn team_counts(&self) -> (usize, usize) {
        let mut r = 0;
        let mut b = 0;
        for s in self.slots.iter().filter(|s| s.connected) {
            match s.team {
                Team::Red => r += 1,
                Team::Blue => b += 1,
            }
        }
        (r, b)
    }

    fn pick_team(&self, req: u8) -> Team {
        match req {
            1 => Team::Red,
            2 => Team::Blue,
            _ => {
                let (r, b) = self.team_counts();
                if r <= b {
                    Team::Red
                } else {
                    Team::Blue
                }
            }
        }
    }

    pub fn is_banned(&self, addr: &SocketAddr) -> bool {
        self.bans.contains(&addr.ip())
    }

    /// Join or rejoin. Returns client_id + team.
    pub fn join(
        &mut self,
        name: &str,
        team_req: u8,
        password: &str,
        addr: SocketAddr,
        tick: u64,
    ) -> Result<(usize, Team), JoinError> {
        if self.is_banned(&addr) {
            return Err(JoinError::Banned);
        }
        if !self.password.is_empty() && password != self.password {
            return Err(JoinError::BadPassword);
        }
        let clean = sanitize_name(name);
        if clean.is_empty() {
            return Err(JoinError::BadName);
        }
        // Rejoin by addr.
        if let Some(&id) = self.addr_to_id.get(&addr) {
            let s = &mut self.slots[id];
            s.last_seen_tick = tick;
            s.name = clean;
            return Ok((id, s.team));
        }
        // Find free slot.
        let free = self.slots.iter().position(|s| !s.connected);
        match free {
            None => Err(JoinError::Full),
            Some(id) => {
                let team = self.pick_team(team_req);
                let s = &mut self.slots[id];
                s.name = clean;
                s.team = team;
                s.addr = Some(addr);
                s.connected = true;
                s.is_bot = false;
                s.last_seen_tick = tick;
                s.pending.clear();
                self.addr_to_id.insert(addr, id);
                if self.host_id.is_none() {
                    self.host_id = Some(id);
                }
                Ok((id, team))
            }
        }
    }

    pub fn add_bot(&mut self, tick: u64) -> Option<usize> {
        let free = self.slots.iter().position(|s| !s.connected)?;
        let team = self.pick_team(0);
        let s = &mut self.slots[free];
        s.name = format!("Bot{}", free);
        s.team = team;
        s.addr = None;
        s.connected = true;
        s.is_bot = true;
        s.last_seen_tick = tick;
        Some(free)
    }

    pub fn remove_bot(&mut self) -> Option<usize> {
        let pos = self.slots.iter().position(|s| s.connected && s.is_bot)?;
        self.slots[pos].connected = false;
        self.slots[pos].pending.clear();
        Some(pos)
    }

    pub fn disconnect(&mut self, id: usize) {
        if id < self.slots.len() {
            if let Some(a) = self.slots[id].addr.take() {
                self.addr_to_id.remove(&a);
            }
            self.slots[id].connected = false;
            self.slots[id].pending.clear();
        }
    }

    pub fn kick(&mut self, id: usize) -> bool {
        if id < self.slots.len() && self.slots[id].connected {
            self.disconnect(id);
            true
        } else {
            false
        }
    }

    pub fn ban_ip(&mut self, ip: IpAddr) {
        self.bans.insert(ip);
        // Disconnect matching slots.
        let ids: Vec<usize> = self
            .slots
            .iter()
            .enumerate()
            .filter(|(_, s)| s.addr.map(|a| a.ip() == ip).unwrap_or(false))
            .map(|(i, _)| i)
            .collect();
        for id in ids {
            self.disconnect(id);
        }
    }

    pub fn unban_ip(&mut self, ip: &IpAddr) -> bool {
        self.bans.remove(ip)
    }

    pub fn touch(&mut self, id: usize, tick: u64, seq: u16) -> bool {
        if id >= self.slots.len() || !self.slots[id].connected {
            return false;
        }
        // Accept newer seq (allow wrap: seq greater in u16 sense, or first packet).
        let last = self.slots[id].last_input_seq;
        let fresh = seq.wrapping_sub(last) < 0x8000 && seq != last
            || self.slots[id].last_seen_tick == 0;
        // Always update heartbeat even on duplicate (packet loss / resend).
        self.slots[id].last_seen_tick = tick;
        if fresh {
            self.slots[id].last_input_seq = seq;
            true
        } else {
            false
        }
    }

    pub fn heartbeat(&mut self, id: usize, tick: u64) {
        if id < self.slots.len() {
            self.slots[id].last_seen_tick = tick;
        }
    }

    /// Timeout after 10s of silence. Returns timed-out ids.
    pub fn timeouts(&mut self, tick: u64, tick_rate: u32) -> Vec<usize> {
        let limit = tick_rate as u64 * 10;
        let mut out = Vec::new();
        for s in self.slots.iter() {
            if s.connected && !s.is_bot && tick.saturating_sub(s.last_seen_tick) > limit {
                out.push(s.id);
            }
        }
        for id in &out {
            self.disconnect(*id);
        }
        out
    }

    pub fn is_host(&self, id: usize) -> bool {
        self.host_id == Some(id)
    }

    pub fn ack(&mut self, id: usize, ack_seq: u32) {
        if id >= self.slots.len() {
            return;
        }
        while let Some((seq, _, _)) = self.slots[id].pending.front() {
            if *seq <= ack_seq || ack_seq.wrapping_sub(*seq) < 0x8000_0000 && *seq != ack_seq {
                // Remove all <= ack (handle wrap conservatively: exact + older).
                if *seq == ack_seq {
                    self.slots[id].pending.pop_front();
                    break;
                } else if seq_wraps_before(*seq, ack_seq) {
                    self.slots[id].pending.pop_front();
                } else {
                    break;
                }
            } else {
                break;
            }
        }
    }

    /// Handle in-game chat text. Returns (reply_to_sender, broadcast_text) actions.
    /// Commands start with '/'. Host-only: kick, ban.
    pub fn handle_chat(&mut self, from_id: usize, text: &str) -> ChatAction {
        let t = text.trim();
        if t.starts_with("/kick ") && self.is_host(from_id) {
            let arg = t.trim_start_matches("/kick ").trim();
            if let Ok(id) = arg.parse::<usize>() {
                if self.kick(id) {
                    return ChatAction::Broadcast(format!("Player {} kicked by host", id));
                }
                return ChatAction::Reply("Kick failed: bad id".into());
            }
            return ChatAction::Reply("Usage: /kick <id>".into());
        }
        if t.starts_with("/ban ") && self.is_host(from_id) {
            let arg = t.trim_start_matches("/ban ").trim();
            if let Ok(id) = arg.parse::<usize>() {
                if id < self.slots.len() {
                    if let Some(a) = self.slots[id].addr {
                        let ip = a.ip();
                        self.ban_ip(ip);
                        return ChatAction::Broadcast(format!("Player {} banned ({})", id, ip));
                    }
                }
                return ChatAction::Reply("Ban failed: bad id".into());
            }
            return ChatAction::Reply("Usage: /ban <id>".into());
        }
        if t == "/help" {
            return ChatAction::Reply(
                "Commands: /help /status /team red|blue|auto /kick <id> (host) /ban <id> (host)".into(),
            );
        }
        if t == "/status" {
            let (r, b) = self.team_counts();
            return ChatAction::Reply(format!(
                "Players {}/{} (red {} blue {})",
                self.active_count(),
                self.slots.len(),
                r,
                b
            ));
        }
        if t.starts_with("/team ") {
            let arg = t.trim_start_matches("/team ").trim();
            let team = match arg {
                "red" => Some(Team::Red),
                "blue" => Some(Team::Blue),
                "auto" => Some(self.pick_team(0)),
                _ => None,
            };
            if let Some(tm) = team {
                if from_id < self.slots.len() {
                    self.slots[from_id].team = tm;
                    return ChatAction::Broadcast(format!(
                        "{} switched to {:?}",
                        self.slots[from_id].name, tm
                    ));
                }
            }
            return ChatAction::Reply("Usage: /team red|blue|auto".into());
        }
        // Normal chat → broadcast.
        let name = if from_id < self.slots.len() {
            self.slots[from_id].name.clone()
        } else {
            "???".into()
        };
        ChatAction::BroadcastChat { from: name, text: t.to_string() }
    }
}

fn seq_wraps_before(a: u32, b: u32) -> bool {
    b.wrapping_sub(a) < 0x8000_0000 && a != b
}

#[derive(Debug)]
pub enum ChatAction {
    Reply(String),
    Broadcast(String),
    BroadcastChat { from: String, text: String },
    None,
}

pub fn sanitize_name(raw: &str) -> String {
    let t = raw.trim();
    let mut out = String::new();
    for ch in t.chars().take(24) {
        if ch.is_alphanumeric() || ch == '_' || ch == '-' || ch == ' ' {
            out.push(ch);
        }
    }
    out.trim().to_string()
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::net::{IpAddr, Ipv4Addr};

    fn addr(n: u8) -> SocketAddr {
        SocketAddr::new(IpAddr::V4(Ipv4Addr::new(127, 0, 0, n)), 5000)
    }

    #[test]
    fn join_balances_teams() {
        let mut m = SessionManager::new(4, String::new());
        let (a, ta) = m.join("A", 0, "", addr(1), 1).unwrap();
        let (b, tb) = m.join("B", 0, "", addr(2), 1).unwrap();
        assert_ne!((a, ta), (b, tb));
        assert!(ta != tb);
    }

    #[test]
    fn password_and_ban() {
        let mut m = SessionManager::new(4, "secret".into());
        assert!(matches!(
            m.join("A", 0, "wrong", addr(1), 1),
            Err(JoinError::BadPassword)
        ));
        m.bans.insert(IpAddr::V4(Ipv4Addr::new(127, 0, 0, 9)));
        assert!(matches!(
            m.join("B", 0, "secret", addr(9), 1),
            Err(JoinError::Banned)
        ));
    }

    #[test]
    fn timeout_removes_idle() {
        let mut m = SessionManager::new(4, String::new());
        m.join("A", 0, "", addr(1), 0).unwrap();
        let out = m.timeouts(60 * 10 + 1, 60);
        assert_eq!(out.len(), 1);
    }
}
