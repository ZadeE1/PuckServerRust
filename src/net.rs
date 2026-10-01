//! PuckRust UDP protocol v1 (complete, LAN-ready, Mac↔Windows).
//!
//! All integers little-endian. Max datagram < 1200B. First byte = packet type.
//! - 0x01 Hello, 0x02 Welcome, 0x03 Reject, 0x04 Input, 0x05 Snapshot,
//!   0x06 Event (reliable, acked), 0x07 Ack, 0x08 Heartbeat, 0x09 Disconnect,
//!   0x10 DiscoveryQuery, 0x11 DiscoveryResponse, 0x20 Chat.
//!
//! Backwards note: prototype 4B input replaced by 7B typed Input (0x04).

use crate::game::Phase;
use crate::physics::{Player, PlayerInput, Puck, Team, Vec2};

pub const PROTO_VER: u8 = 1;

pub const P_HELLO: u8 = 0x01;
pub const P_WELCOME: u8 = 0x02;
pub const P_REJECT: u8 = 0x03;
pub const P_INPUT: u8 = 0x04;
pub const P_SNAPSHOT: u8 = 0x05;
pub const P_EVENT: u8 = 0x06;
pub const P_ACK: u8 = 0x07;
pub const P_HEARTBEAT: u8 = 0x08;
pub const P_DISCONNECT: u8 = 0x09;
pub const P_DISC_QUERY: u8 = 0x10;
pub const P_DISC_RESP: u8 = 0x11;
pub const P_CHAT: u8 = 0x20;

pub const REJECT_FULL: u8 = 1;
pub const REJECT_BADPASS: u8 = 2;
pub const REJECT_BANNED: u8 = 3;

pub const EV_GOAL: u8 = 1;
pub const EV_PHASE: u8 = 2;
pub const EV_CHAT: u8 = 3;
pub const EV_SERVERMSG: u8 = 4;

#[derive(Debug, Clone)]
pub struct Hello {
    pub name: String,
    pub team_req: u8,
    pub password: String,
}

#[derive(Debug, Clone)]
pub struct Snapshot {
    pub tick: u64,
    pub phase: Phase,
    pub score_red: u8,
    pub score_blue: u8,
    pub period: u8,
    pub time_left_ms: u32,
    pub puck: Puck,
    pub players: Vec<SnapshotPlayer>,
}

#[derive(Debug, Clone)]
pub struct SnapshotPlayer {
    pub id: u8,
    pub team: u8,
    pub pos: Vec2,
    pub vel: Vec2,
    pub stamina_u8: u8,
    pub flags: u8, // bit0 sprinting
}

impl SnapshotPlayer {
    pub fn from_player(id: usize, p: &Player) -> Self {
        Self {
            id: id as u8,
            team: p.team as u8,
            pos: p.pos,
            vel: p.vel,
            stamina_u8: (p.stamina.clamp(0.0, 1.0) * 255.0) as u8,
            flags: if p.sprinting { 1 } else { 0 },
        }
    }
}

#[derive(Debug, Clone)]
pub struct InputPkt {
    pub client_id: u8,
    pub seq: u16,
    pub input: PlayerInput,
}

#[derive(Debug, Clone)]
pub enum NetEvent {
    Goal { team: Team, scorer: Option<usize>, score_red: u8, score_blue: u8 },
    Phase { from: Phase, to: Phase, period: u8, time_left_ms: u32 },
    Chat { from: String, text: String },
    ServerMsg { text: String },
}

fn push_str_capped(out: &mut Vec<u8>, s: &str, cap: usize) {
    let b = s.as_bytes();
    let n = b.len().min(cap);
    out.push(n as u8);
    out.extend_from_slice(&b[..n]);
}

fn read_str(buf: &[u8], pos: &mut usize, cap: usize) -> Option<String> {
    if *pos >= buf.len() {
        return None;
    }
    let n = buf[*pos] as usize;
    *pos += 1;
    if n > cap || *pos + n > buf.len() {
        return None;
    }
    let s = std::str::from_utf8(&buf[*pos..*pos + n]).ok()?.to_string();
    *pos += n;
    Some(s)
}

// ---------- encode ----------

pub fn encode_hello(h: &Hello) -> Vec<u8> {
    let mut o = Vec::with_capacity(64);
    o.push(P_HELLO);
    push_str_capped(&mut o, &h.name, 24);
    o.push(h.team_req);
    push_str_capped(&mut o, &h.password, 64);
    o
}

pub fn encode_welcome(client_id: u8, tick: u64, team: u8) -> [u8; 11] {
    let mut o = [0u8; 11];
    o[0] = P_WELCOME;
    o[1] = client_id;
    o[2..10].copy_from_slice(&tick.to_le_bytes());
    o[10] = team;
    o
}

pub fn encode_reject(reason: u8) -> [u8; 2] {
    [P_REJECT, reason]
}

pub fn encode_input(p: &InputPkt) -> [u8; 7] {
    let mut flags = 0u8;
    if p.input.sprint {
        flags |= 0x01;
    }
    if p.input.hit {
        flags |= 0x02;
    }
    [
        P_INPUT,
        p.client_id,
        (p.seq & 0xFF) as u8,
        (p.seq >> 8) as u8,
        p.input.thrust_x as u8,
        p.input.thrust_y as u8,
        flags,
    ]
}

pub fn encode_snapshot(s: &Snapshot, out: &mut Vec<u8>) {
    out.clear();
    out.reserve(37 + s.players.len() * 20);
    out.push(P_SNAPSHOT);
    out.extend_from_slice(&s.tick.to_le_bytes());
    out.push(s.phase as u8);
    out.push(s.score_red);
    out.push(s.score_blue);
    out.push(s.period);
    out.extend_from_slice(&s.time_left_ms.to_le_bytes());
    for v in [s.puck.pos.x, s.puck.pos.y, s.puck.vel.x, s.puck.vel.y] {
        out.extend_from_slice(&v.to_le_bytes());
    }
    out.push(s.players.len() as u8);
    out.extend_from_slice(&[0u8; 3]);
    for p in &s.players {
        out.push(p.id);
        out.push(p.team);
        for v in [p.pos.x, p.pos.y, p.vel.x, p.vel.y] {
            out.extend_from_slice(&v.to_le_bytes());
        }
        out.push(p.stamina_u8);
        out.push(p.flags);
    }
}

pub fn encode_event(seq: u32, ev: &NetEvent) -> Vec<u8> {
    let mut o = Vec::with_capacity(64);
    o.push(P_EVENT);
    o.extend_from_slice(&seq.to_le_bytes());
    match ev {
        NetEvent::Goal { team, scorer, score_red, score_blue } => {
            o.push(EV_GOAL);
            o.push(*team as u8);
            o.push(scorer.map(|s| s as u8).unwrap_or(255));
            o.push(*score_red);
            o.push(*score_blue);
        }
        NetEvent::Phase { from, to, period, time_left_ms } => {
            o.push(EV_PHASE);
            o.push(*from as u8);
            o.push(*to as u8);
            o.push(*period);
            o.extend_from_slice(&time_left_ms.to_le_bytes());
        }
        NetEvent::Chat { from, text } => {
            o.push(EV_CHAT);
            push_str_capped(&mut o, from, 24);
            push_str_capped(&mut o, text, 128);
        }
        NetEvent::ServerMsg { text } => {
            o.push(EV_SERVERMSG);
            push_str_capped(&mut o, text, 128);
        }
    }
    o
}

pub fn encode_ack(seq: u32) -> [u8; 5] {
    let mut o = [0u8; 5];
    o[0] = P_ACK;
    o[1..5].copy_from_slice(&seq.to_le_bytes());
    o
}

pub fn encode_heartbeat(tick: u64) -> [u8; 9] {
    let mut o = [0u8; 9];
    o[0] = P_HEARTBEAT;
    o[1..9].copy_from_slice(&tick.to_le_bytes());
    o
}

pub fn encode_chat(text: &str) -> Vec<u8> {
    let mut o = Vec::with_capacity(132);
    o.push(P_CHAT);
    push_str_capped(&mut o, text, 128);
    o
}

pub fn encode_disc_query() -> [u8; 2] {
    [P_DISC_QUERY, PROTO_VER]
}

pub fn encode_disc_resp(port: u16, players: u8, max: u8, name: &str) -> Vec<u8> {
    let mut o = Vec::with_capacity(32);
    o.push(P_DISC_RESP);
    o.push(PROTO_VER);
    o.extend_from_slice(&port.to_le_bytes());
    o.push(players);
    o.push(max);
    push_str_capped(&mut o, name, 48);
    o
}

// ---------- decode ----------

pub fn decode_hello(buf: &[u8]) -> Option<Hello> {
    if buf.first() != Some(&P_HELLO) {
        return None;
    }
    let mut pos = 1;
    let name = read_str(buf, &mut pos, 24)?;
    if pos >= buf.len() {
        return None;
    }
    let team_req = buf[pos];
    pos += 1;
    let password = read_str(buf, &mut pos, 64)?;
    if team_req > 2 {
        return None;
    }
    Some(Hello { name, team_req, password })
}

pub fn decode_input(buf: &[u8]) -> Option<InputPkt> {
    if buf.len() < 7 || buf[0] != P_INPUT {
        return None;
    }
    let client_id = buf[1];
    let seq = u16::from_le_bytes([buf[2], buf[3]]);
    Some(InputPkt {
        client_id,
        seq,
        input: PlayerInput {
            thrust_x: buf[4] as i8,
            thrust_y: buf[5] as i8,
            sprint: buf[6] & 0x01 != 0,
            hit: buf[6] & 0x02 != 0,
        },
    })
}

pub fn decode_ack(buf: &[u8]) -> Option<u32> {
    if buf.len() < 5 || buf[0] != P_ACK {
        return None;
    }
    Some(u32::from_le_bytes([buf[1], buf[2], buf[3], buf[4]]))
}

pub fn decode_chat(buf: &[u8]) -> Option<String> {
    if buf.first() != Some(&P_CHAT) {
        return None;
    }
    let mut pos = 1;
    read_str(buf, &mut pos, 128)
}

pub fn decode_heartbeat(buf: &[u8]) -> Option<u64> {
    if buf.len() < 9 || buf[0] != P_HEARTBEAT {
        return None;
    }
    Some(u64::from_le_bytes(buf[1..9].try_into().ok()?))
}

pub fn decode_event(buf: &[u8]) -> Option<(u32, NetEventKind)> {
    if buf.len() < 6 || buf[0] != P_EVENT {
        return None;
    }
    let seq = u32::from_le_bytes([buf[1], buf[2], buf[3], buf[4]]);
    let kind = buf[5];
    Some((seq, NetEventKind(kind)))
}

#[derive(Debug, Clone, Copy)]
pub struct NetEventKind(pub u8);

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn hello_roundtrip() {
        let h = Hello { name: "Zed".into(), team_req: 0, password: "pw".into() };
        let b = encode_hello(&h);
        let d = decode_hello(&b).unwrap();
        assert_eq!(d.name, "Zed");
        assert_eq!(d.team_req, 0);
    }

    #[test]
    fn input_roundtrip() {
        let p = InputPkt {
            client_id: 3,
            seq: 1234,
            input: PlayerInput { thrust_x: -100, thrust_y: 50, sprint: true, hit: false },
        };
        let b = encode_input(&p);
        let d = decode_input(&b).unwrap();
        assert_eq!(d.client_id, 3);
        assert_eq!(d.seq, 1234);
        assert_eq!(d.input.thrust_x, -100);
        assert!(d.input.sprint);
    }

    #[test]
    fn snapshot_size() {
        let s = Snapshot {
            tick: 99,
            phase: Phase::Playing,
            score_red: 1,
            score_blue: 2,
            period: 1,
            time_left_ms: 1000,
            puck: Puck::default(),
            players: vec![
                SnapshotPlayer { id: 0, team: 1, pos: Vec2::new(0.0, 0.0), vel: Vec2::new(0.0, 0.0), stamina_u8: 255, flags: 0 },
                SnapshotPlayer { id: 1, team: 2, pos: Vec2::new(0.0, 0.0), vel: Vec2::new(0.0, 0.0), stamina_u8: 255, flags: 0 },
            ],
        };
        let mut buf = Vec::new();
        encode_snapshot(&s, &mut buf);
        assert_eq!(buf.len(), 37 + 2 * 20);
        assert_eq!(buf[0], P_SNAPSHOT);
    }

    #[test]
    fn event_encode_goal() {
        let ev = NetEvent::Goal { team: Team::Red, scorer: Some(2), score_red: 1, score_blue: 0 };
        let b = encode_event(7, &ev);
        let (seq, kind) = decode_event(&b).unwrap();
        assert_eq!(seq, 7);
        assert_eq!(kind.0, EV_GOAL);
    }

    #[test]
    fn chat_roundtrip() {
        let b = encode_chat("hello team");
        assert_eq!(decode_chat(&b).unwrap(), "hello team");
    }

    #[test]
    fn reject_bad_type() {
        assert!(decode_input(&[0xFF, 0, 0, 0, 0, 0, 0]).is_none());
        assert!(decode_hello(&[P_HELLO]).is_none());
    }
}
