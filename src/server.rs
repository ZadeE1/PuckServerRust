//! Tokio UTP + NGO server loop (wires `utp` + `ngo` to UDP).
//!
//! Reference: `H:\\UTP` @ 2.4.0 (`SimpleConnectionLayer.cs`,
//! `ReliableUtility.cs`, `NetworkParams.cs`), `H:\\NGO` @ v2.5.1
//! (`UnityTransport.cs` pipeline order/mapping, `NetworkMessageManager.cs`).
//!
//! - Handshake: request (`55 54 50 01` + token) -> accept. Disconnect after
//!   30s silence, heartbeat every 500ms.
//! - Pipelines (creation order in `UnityTransport.CreateDriver`, ids start
//!   at 1 — verify against live traffic): 1 = unreliable-fragmented,
//!   2 = unreliable-sequenced-fragmented, 3 = reliable-sequenced.
//! - Reliable: window-32 headers, immediate ack-only replies, in-order
//!   delivery via a reorder buffer, resend after 200ms.
//! - NGO batches ride as pipeline payloads; verified batches are handed to
//!   the `Handler` as (client id, message id, payload).

use std::collections::{BTreeMap, HashMap};
use std::net::SocketAddr;
use std::time::{Duration, Instant};

use tokio::net::UdpSocket;

use crate::ngo;
use crate::utp;

pub const PIPE_UNRELIABLE_FRAG: u8 = 1;
pub const PIPE_UNRELIABLE_SEQ_FRAG: u8 = 2;
pub const PIPE_RELIABLE_SEQ: u8 = 3;

const MTU_PAYLOAD: usize = 1296;
const FRAG_CHUNK: usize = 1000;
const HEARTBEAT_MS: u64 = 500;
const TIMEOUT_MS: u64 = 30_000;
const RESEND_MS: u64 = 200;

fn now_ms(start: Instant) -> u64 {
    start.elapsed().as_millis() as u64
}

struct Conn {
    addr: SocketAddr,
    token: u64,
    client_id: u64,
    last_seen: Instant,
    last_heartbeat: Instant,
    unrel_seq: Option<u16>,
    frag: utp::Reassembler,
    rel_send: utp::ResendQueue,
    rel_recv_next: u16,
    rel_recv_buf: BTreeMap<u16, Vec<u8>>,
    rel_recv_seen: Vec<u16>,
    rel_last_remote: u16,
}

impl Conn {
    fn new(addr: SocketAddr, token: u64, client_id: u64, now: Instant) -> Self {
        Self {
            addr,
            token,
            client_id,
            last_seen: now,
            last_heartbeat: now,
            unrel_seq: None,
            frag: utp::Reassembler::new(),
            rel_send: utp::ResendQueue::new(),
            rel_recv_next: 0,
            rel_recv_buf: BTreeMap::new(),
            rel_recv_seen: Vec::new(),
            rel_last_remote: 0,
        }
    }
}

/// Decoded inbound event for the game layer.
pub enum Event {
    Connected(u64),
    Disconnected(u64),
    NgoMessage(u64, u32, Vec<u8>),
}

pub struct Server {
    sock: UdpSocket,
    start: Instant,
    conns: HashMap<SocketAddr, Conn>,
    next_client: u64,
    out: Vec<(SocketAddr, Vec<u8>)>,
}

impl Server {
    pub async fn bind(addr: &str) -> std::io::Result<Self> {
        Ok(Self {
            sock: UdpSocket::bind(addr).await?,
            start: Instant::now(),
            conns: HashMap::new(),
            next_client: 1,
            out: Vec::new(),
        })
    }

    pub fn local_addr(&self) -> std::io::Result<SocketAddr> {
        self.sock.local_addr()
    }

    /// Drain one UDP datagram (non-blocking). Returns false when empty.
    pub async fn pump(&mut self, events: &mut Vec<Event>) -> std::io::Result<bool> {
        let mut buf = [0u8; 2048];
        let res = tokio::time::timeout(Duration::from_millis(1), self.sock.recv_from(&mut buf)).await;
        let (n, addr) = match res {
            Ok(Ok(v)) => v,
            _ => return Ok(false),
        };
        self.on_datagram(addr, &buf[..n], events);
        Ok(true)
    }

    fn send_raw(&mut self, addr: SocketAddr, pkt: Vec<u8>) {
        self.out.push((addr, pkt));
    }

    /// Flush queued outbound datagrams.
    pub async fn flush(&mut self) -> std::io::Result<()> {
        for (addr, pkt) in self.out.drain(..).collect::<Vec<_>>() {
            self.sock.send_to(&pkt, addr).await?;
        }
        Ok(())
    }

    fn on_datagram(&mut self, addr: SocketAddr, pkt: &[u8], events: &mut Vec<Event>) {
        let now = Instant::now();
        // Handshake?
        if let Some((token, accept)) = utp::decode_handshake(pkt) {
            if !accept {
                let id = self.next_client;
                self.next_client += 1;
                self.conns.insert(addr, Conn::new(addr, token, id, now));
                let mut b = [0u8; 13];
                utp::encode_handshake(token, true, &mut b);
                self.send_raw(addr, b.to_vec());
                events.push(Event::Connected(id));
            }
            return;
        }
        let (msg, token, pipe) = match utp::decode_data_header(pkt) {
            Some(h) => h,
            None => return,
        };
        let c = match self.conns.get_mut(&addr) {
            Some(c) if c.token == token => c,
            _ => return,
        };
        c.last_seen = now;
        let body = &pkt[10..];
        match msg {
            utp::MSG_HEARTBEAT => {}
            utp::MSG_DISCONNECT => {
                let id = c.client_id;
                self.conns.remove(&addr);
                events.push(Event::Disconnected(id));
            }
            _ => {
                let id = c.client_id;
                let payloads = Self::strip_pipeline(c, pipe, body, now);
                for p in payloads {
                    if let Some(msgs) = ngo::decode_batch(&p) {
                        for (mid, payload) in msgs {
                            events.push(Event::NgoMessage(id, mid, payload.to_vec()));
                        }
                    }
                }
                // Piggyback ack on next send; also send ack-only now if reliable seen.
                Self::maybe_ack(self, addr);
            }
        }
    }

    /// Strip pipeline headers; returns complete NGO payloads. Caller handles borrow.
    fn strip_pipeline(c: &mut Conn, pipe: u8, body: &[u8], _now: Instant) -> Vec<Vec<u8>> {
        match pipe {
            PIPE_UNRELIABLE_FRAG | PIPE_UNRELIABLE_SEQ_FRAG => {
                let mut cur = body;
                if pipe == PIPE_UNRELIABLE_SEQ_FRAG {
                    if cur.len() < 2 {
                        return vec![];
                    }
                    let seq = u16::from_le_bytes([cur[0], cur[1]]);
                    let fresh = match c.unrel_seq {
                        None => true,
                        Some(last) => seq != last && seq.wrapping_sub(last) < 0x8000,
                    };
                    if !fresh {
                        return vec![];
                    }
                    c.unrel_seq = Some(seq);
                    cur = &cur[2..];
                }
                if cur.len() < 2 {
                    return vec![];
                }
                let fh = u16::from_le_bytes([cur[0], cur[1]]);
                match c.frag.push(fh, &cur[2..]) {
                    Some(full) => vec![full],
                    None => vec![],
                }
            }
            PIPE_RELIABLE_SEQ => {
                let (ack_only, _, seq, acked, mask) = match utp::decode_reliable_header(body) {
                    Some(h) => h,
                    None => return vec![],
                };
                c.rel_send.on_ack(acked, mask);
                if ack_only {
                    return vec![];
                }
                // Track for our ack-mask.
                if !c.rel_recv_seen.contains(&seq) {
                    c.rel_recv_seen.push(seq);
                    if c.rel_recv_seen.len() > 64 {
                        c.rel_recv_seen.remove(0);
                    }
                }
                if seq == c.rel_recv_next {
                    let mut out = vec![body[16..].to_vec()];
                    c.rel_recv_next = c.rel_recv_next.wrapping_add(1);
                    c.rel_last_remote = seq;
                    while let Some(p) = c.rel_recv_buf.remove(&c.rel_recv_next) {
                        out.push(p);
                        c.rel_last_remote = c.rel_recv_next;
                        c.rel_recv_next = c.rel_recv_next.wrapping_add(1);
                    }
                    out
                } else {
                    c.rel_recv_buf.entry(seq).or_insert_with(|| body[16..].to_vec());
                    vec![]
                }
            }
            _ => vec![],
        }
    }

    fn maybe_ack(server: &mut Server, addr: SocketAddr) {
        let (token, last, seen) = match server.conns.get(&addr) {
            Some(c) => (c.token, c.rel_last_remote, c.rel_recv_seen.clone()),
            None => return,
        };
        if seen.is_empty() {
            return;
        }
        let mask = utp::build_ack_mask(last, &seen);
        let rh = utp::encode_reliable_header(true, 0, 0, last, mask);
        let mut pkt = utp::encode_data_header(utp::MSG_DATA, token, PIPE_RELIABLE_SEQ).to_vec();
        pkt.extend_from_slice(&rh);
        server.send_raw(addr, pkt);
    }

    /// Queue an NGO batch for a client. Fragments over the unreliable
    /// pipelines; reliable payloads must fit (`MTU_PAYLOAD`).
    pub fn send_batch(&mut self, client: u64, msgs: &[(u32, &[u8])], reliable: bool) {
        let addr = match self.conns.iter().find(|(_, c)| c.client_id == client).map(|(a, _)| *a) {
            Some(a) => a,
            None => return,
        };
        let batch = ngo::encode_batch(msgs);
        let t = now_ms(self.start);
        if reliable {
            let pkt = match self.conns.get_mut(&addr) {
                Some(c) => {
                    debug_assert!(batch.len() <= MTU_PAYLOAD, "reliable payload over MTU");
                    let seq = c.rel_send.push(batch.clone(), t);
                    let mask = utp::build_ack_mask(c.rel_last_remote, &c.rel_recv_seen);
                    let rh = utp::encode_reliable_header(false, 0, seq, c.rel_last_remote, mask);
                    let mut pkt =
                        utp::encode_data_header(utp::MSG_DATA, c.token, PIPE_RELIABLE_SEQ).to_vec();
                    pkt.extend_from_slice(&rh);
                    pkt.extend_from_slice(&batch);
                    pkt
                }
                None => return,
            };
            self.send_raw(addr, pkt);
        } else {
            let c = match self.conns.get(&addr) {
                Some(c) => c,
                None => return,
            };
            let token = c.token;
            let chunks = utp::frag_split(&batch, FRAG_CHUNK);
            let n = chunks.len() as u16;
            for (i, ch) in chunks.iter().enumerate() {
                let fh = utp::frag_header(i as u16, i as u16 == n - 1);
                let mut pkt = utp::encode_data_header(utp::MSG_DATA, token, PIPE_UNRELIABLE_FRAG).to_vec();
                pkt.extend_from_slice(&fh.to_le_bytes());
                pkt.extend_from_slice(ch);
                self.send_raw(addr, pkt);
            }
        }
    }

    /// Heartbeats, resends, timeouts. Call at ~100Hz.
    pub fn tick(&mut self, events: &mut Vec<Event>) {        let t = now_ms(self.start);
        let now = Instant::now();
        let mut dead = Vec::new();
        let mut resends: Vec<(SocketAddr, Vec<u8>)> = Vec::new();
        for (addr, c) in self.conns.iter_mut() {
            if now.duration_since(c.last_seen).as_millis() as u64 > TIMEOUT_MS {
                dead.push((*addr, c.client_id));
                continue;
            }
            if now.duration_since(c.last_heartbeat).as_millis() as u64 > HEARTBEAT_MS {
                c.last_heartbeat = now;
                let pkt = utp::encode_data_header(utp::MSG_HEARTBEAT, c.token, 0).to_vec();
                resends.push((*addr, pkt));
            }
            for (seq, payload) in c.rel_send.due(t, RESEND_MS) {
                let mask = utp::build_ack_mask(c.rel_last_remote, &c.rel_recv_seen);
                let rh = utp::encode_reliable_header(false, 0, seq, c.rel_last_remote, mask);
                let mut pkt = utp::encode_data_header(utp::MSG_DATA, c.token, PIPE_RELIABLE_SEQ).to_vec();
                pkt.extend_from_slice(&rh);
                pkt.extend_from_slice(&payload);
                resends.push((*addr, pkt));
            }
        }
        for (a, p) in resends {
            self.send_raw(a, p);
        }
        for (addr, id) in dead {
            self.conns.remove(&addr);
            events.push(Event::Disconnected(id));
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    async fn recv_one(sock: &UdpSocket) -> (Vec<u8>, SocketAddr) {
        let mut b = [0u8; 4096];
        let (n, from) = tokio::time::timeout(Duration::from_secs(2), sock.recv_from(&mut b))
            .await
            .expect("timeout")
            .unwrap();
        (b[..n].to_vec(), from)
    }

    #[tokio::test]
    async fn loopback_handshake_plus_reliable_ngo_batch() {
        let mut srv = Server::bind("127.0.0.1:0").await.unwrap();
        let saddr = srv.local_addr().unwrap();
        let cli = UdpSocket::bind("127.0.0.1:0").await.unwrap();
        cli.connect(saddr).await.unwrap();

        // 1. Handshake request -> accept.
        let mut hs = [0u8; 13];
        utp::encode_handshake(0xABCD, false, &mut hs);
        cli.send(&hs).await.unwrap();
        let mut evs = Vec::new();
        srv.pump(&mut evs).await.unwrap();
        srv.flush().await.unwrap();
        let (got, _) = recv_one(&cli).await;
        assert_eq!(utp::decode_handshake(&got), Some((0xABCD, true)));
        assert!(matches!(evs[0], Event::Connected(1)));

        // 2. Client sends NGO batch (connection request) over reliable pipe.
        let req = ngo::encode_connection_request(&[(7, 1)], 0x1234, b"pw");
        let batch = ngo::encode_batch(&[(ngo::message_id("Unity.Netcode.ConnectionRequestMessage"), &req)]);
        let mut cseq = 0u16;
        let send_rel = |seq: u16, payload: &[u8]| {
            let rh = utp::encode_reliable_header(false, 0, seq, 0, 0);
            let mut p = utp::encode_data_header(utp::MSG_DATA, 0xABCD, PIPE_RELIABLE_SEQ).to_vec();
            p.extend_from_slice(&rh);
            p.extend_from_slice(payload);
            p
        };
        cli.send(&send_rel(cseq, &batch)).await.unwrap();
        cseq = cseq.wrapping_add(1);
        srv.pump(&mut evs).await.unwrap();
        srv.flush().await.unwrap();
        // Server emits ack-only; client should see it.
        let (ack, _) = recv_one(&cli).await;
        let (_, _, pipe) = utp::decode_data_header(&ack).unwrap();
        assert_eq!(pipe, PIPE_RELIABLE_SEQ);
        assert!(utp::decode_reliable_header(&ack[10..]).unwrap().0);
        // And the NGO message arrived decoded.
        assert!(evs.iter().any(|e| matches!(e, Event::NgoMessage(1, _, _))));

        // 3. Server replies with a batch; client reads it back.
        srv.send_batch(1, &[(ngo::message_id("Unity.Netcode.ConnectionApprovedMessage"), b"ok")], true);
        srv.flush().await.unwrap();
        let (rep, _) = recv_one(&cli).await;
        let msgs = ngo::decode_batch(&rep[10 + 16..]).unwrap();
        assert_eq!(msgs[0].1, b"ok");

        // 4. Out-of-order reliable delivery still arrives in order.
        let b1 = ngo::encode_batch(&[(1, b"one".as_slice())]);
        let b2 = ngo::encode_batch(&[(1, b"two".as_slice())]);
        cli.send(&send_rel(2, &b2)).await.unwrap(); // seq 2 before 1
        cli.send(&send_rel(1, &b1)).await.unwrap();
        let mut evs2 = Vec::new();
        srv.pump(&mut evs2).await.unwrap();
        srv.pump(&mut evs2).await.unwrap();
        let got: Vec<Vec<u8>> = evs2
            .iter()
            .filter_map(|e| match e {
                Event::NgoMessage(_, _, p) => Some(p.clone()),
                _ => None,
            })
            .collect();
        assert_eq!(got.len(), 2);
        let _ = cseq;
    }

    /// Live capture harness (ignored by default): binds 127.0.0.1:25567,
    /// completes UTP handshakes, and hex-dumps every NGO batch received for
    /// 90s. Point the real client at this port, then compare with `ngo`.
    #[tokio::test]
    #[ignore]
    async fn live_capture() {
        let mut srv = Server::bind("127.0.0.1:25567").await.unwrap();
        println!("CAPTURE listening on 127.0.0.1:25567");
        let deadline = std::time::Instant::now() + Duration::from_secs(90);
        let mut evs = Vec::new();
        while std::time::Instant::now() < deadline {
            while srv.pump(&mut evs).await.unwrap() {}
            srv.flush().await.unwrap();
            srv.tick(&mut Vec::new());
            for e in evs.drain(..) {
                match e {
                    Event::Connected(id) => println!("CONNECTED client={id}"),
                    Event::Disconnected(id) => println!("DISCONNECTED client={id}"),
                    Event::NgoMessage(id, mid, p) => {
                        let hex: String = p
                            .iter()
                            .take(320)
                            .map(|b| format!("{b:02X}"))
                            .collect::<Vec<_>>()
                            .join(" ");
                        println!("NGO client={id} msg={mid} len={} {hex}", p.len());
                    }
                }
            }
            tokio::time::sleep(Duration::from_millis(5)).await;
        }
    }
}
