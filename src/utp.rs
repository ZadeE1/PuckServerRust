//! UnityTransport UTP 2.4.0 wire layer (pinned: `H:\\UTP` @ tag `2.4.0`).
//!
//! Byte-exact framing for the three pipelines Puck's NGO driver creates
//! (`UnityTransport.CreateDriver`: unreliable-fragmented, unreliable-sequenced
//! fragmented, reliable-sequenced):
//! - Handshake: `55 54 50 01` + u64 token + u8 type (1=request, 2=accept).
//! - Data: u8 msg (1=data, 2=disconnect, 3=heartbeat) + u64 token + u8 pipeline.
//! - Unreliable-sequenced stage: u16 seq prefix.
//! - Fragmentation stage: u16 flags+seq (First=`1<<15`, Last=`1<<14`, 14b seq).
//! - Reliable stage (default window 32): u16 type (0=payload, 1=ack) +
//!   u16 processing-time + u16 seq + u16 acked-seq + u32 ack-mask.
//! No allocation on encode paths (fixed arrays); reassembler/resend own their
//! buffers. No external deps.

use std::collections::{BTreeMap, VecDeque};

pub const SIG: [u8; 4] = [0x55, 0x54, 0x50, 0x01]; // "UTP\x01" LE of 0x01505455
pub const TOKEN_LEN: usize = 8;

pub const MSG_DATA: u8 = 1;
pub const MSG_DISCONNECT: u8 = 2;
pub const MSG_HEARTBEAT: u8 = 3;

pub const HS_REQUEST: u8 = 1;
pub const HS_ACCEPT: u8 = 2;

pub const FRAG_FIRST: u16 = 1 << 15;
pub const FRAG_LAST: u16 = 1 << 14;
pub const FRAG_SEQ_MASK: u16 = FRAG_LAST - 1;

pub const REL_PAYLOAD: u16 = 0;
pub const REL_ACK: u16 = 1;
pub const REL_WINDOW: u32 = 64;

/// 13B handshake datagram: sig + u8 type + u64 token. Returns bytes
/// written (always 13).
pub fn encode_handshake(token: u64, accept: bool, out: &mut [u8; 13]) {
    out[0..4].copy_from_slice(&SIG);
    out[4] = if accept { HS_ACCEPT } else { HS_REQUEST };
    out[5..13].copy_from_slice(&token.to_le_bytes());
}

/// Returns (token, is_accept). None on bad signature/length/type.
pub fn decode_handshake(pkt: &[u8]) -> Option<(u64, bool)> {
    if pkt.len() < 13 || pkt[0..4] != SIG {
        return None;
    }
    let accept = match pkt[4] {
        HS_REQUEST => false,
        HS_ACCEPT => true,
        _ => return None,
    };
    let mut b = [0u8; 8];
    b.copy_from_slice(&pkt[5..13]);
    Some((u64::from_le_bytes(b), accept))
}

/// 10B data header: msg + token + pipeline id.
pub fn encode_data_header(msg: u8, token: u64, pipeline: u8) -> [u8; 10] {
    let mut out = [0u8; 10];
    out[0] = msg;
    out[1..9].copy_from_slice(&token.to_le_bytes());
    out[9] = pipeline;
    out
}

/// Returns (msg, token, pipeline). None on short buffer / unknown msg.
pub fn decode_data_header(pkt: &[u8]) -> Option<(u8, u64, u8)> {
    if pkt.len() < 10 || pkt[0] == 0 || pkt[0] > MSG_HEARTBEAT {
        return None;
    }
    let mut b = [0u8; 8];
    b.copy_from_slice(&pkt[1..9]);
    Some((pkt[0], u64::from_le_bytes(b), pkt[9]))
}

/// Split payload into fragments of at most `max_frag` bytes.
/// Header per fragment is added by the caller via [`frag_header`].
pub fn frag_split<'a>(payload: &'a [u8], max_frag: usize) -> Vec<&'a [u8]> {
    if payload.is_empty() {
        return vec![payload];
    }
    payload.chunks(max_frag.max(1)).collect()
}

/// Fragment header word for fragment index `seq`: First on 0, Last on final.
#[inline]
pub fn frag_header(seq: u16, is_last: bool) -> u16 {
    let mut h = seq & FRAG_SEQ_MASK;
    if seq == 0 {
        h |= FRAG_FIRST;
    }
    if is_last {
        h |= FRAG_LAST;
    }
    h
}

/// Ordered fragment reassembler. Out-of-order tolerant; completes when the
/// Last fragment and every index back to First are present.
#[derive(Default)]
pub struct Reassembler {
    parts: BTreeMap<u16, Vec<u8>>,
    last: Option<u16>,
}

impl Reassembler {
    pub fn new() -> Self {
        Self::default()
    }

    /// Push one fragment (header word + bytes). Returns the full message
    /// once complete. Stale state after completion resets automatically.
    pub fn push(&mut self, header: u16, data: &[u8]) -> Option<Vec<u8>> {
        let seq = header & FRAG_SEQ_MASK;
        if header & FRAG_LAST != 0 {
            self.last = Some(seq);
        }
        self.parts.insert(seq, data.to_vec());
        let last = self.last?;
        if self.parts.len() as u16 != last + 1 || !self.parts.contains_key(&0) {
            return None;
        }
        let mut out = Vec::new();
        for i in 0..=last {
            out.extend_from_slice(&self.parts[&i]);
        }
        self.parts.clear();
        self.last = None;
        Some(out)
    }
}

/// 16B reliable header (window-64 variant NGO uses): type + processing-time
/// + seq + acked-seq + u64 ack-mask, all LE.
pub fn encode_reliable_header(
    ack_only: bool,
    processing_ms: u16,
    seq: u16,
    acked_seq: u16,
    ack_mask: u64,
) -> [u8; 16] {
    let mut out = [0u8; 16];
    out[0..2].copy_from_slice(&(if ack_only { REL_ACK } else { REL_PAYLOAD }).to_le_bytes());
    out[2..4].copy_from_slice(&processing_ms.to_le_bytes());
    out[4..6].copy_from_slice(&seq.to_le_bytes());
    out[6..8].copy_from_slice(&acked_seq.to_le_bytes());
    out[8..16].copy_from_slice(&ack_mask.to_le_bytes());
    out
}

pub fn decode_reliable_header(pkt: &[u8]) -> Option<(bool, u16, u16, u16, u64)> {
    if pkt.len() < 16 {
        return None;
    }
    let ty = u16::from_le_bytes([pkt[0], pkt[1]]);
    if ty > REL_ACK {
        return None;
    }
    let mut m = [0u8; 8];
    m.copy_from_slice(&pkt[8..16]);
    Some((
        ty == REL_ACK,
        u16::from_le_bytes([pkt[2], pkt[3]]),
        u16::from_le_bytes([pkt[4], pkt[5]]),
        u16::from_le_bytes([pkt[6], pkt[7]]),
        u64::from_le_bytes(m),
    ))
}

/// Build the u64 ack-mask for `last_seq` given set of received seqs.
/// Bit 0 = `last_seq` itself (always set), bit N = `last_seq - N` received.
pub fn build_ack_mask(last_seq: u16, received: &[u16]) -> u64 {
    let mut mask = 1u64;
    for &s in received {
        let d = u32::from(last_seq.wrapping_sub(s));
        if d < REL_WINDOW {
            mask |= 1 << d;
        }
    }
    mask
}

/// Send-side resend tracker: unacked payloads keyed by seq with last-send
/// timestamp (ms). Caller retransmits whatever [`ResendQueue::due`] returns.
pub struct ResendQueue {
    next_seq: u16,
    unacked: VecDeque<(u16, Vec<u8>, u64)>,
}

impl ResendQueue {
    pub fn new() -> Self {
        Self {
            next_seq: 0,
            unacked: VecDeque::new(),
        }
    }

    /// Queue a payload for (re)transmission. Returns its seq.
    pub fn push(&mut self, payload: Vec<u8>, now_ms: u64) -> u16 {
        let seq = self.next_seq;
        self.next_seq = self.next_seq.wrapping_add(1);
        self.unacked.push_back((seq, payload, now_ms));
        seq
    }

    /// Drop everything the peer's ack-mask confirms.
    pub fn on_ack(&mut self, acked_seq: u16, ack_mask: u64) {
        self.unacked.retain(|(s, _, _)| {
            let d = u32::from(acked_seq.wrapping_sub(*s));
            d >= REL_WINDOW || (ack_mask & (1 << d)) == 0
        });
    }

    /// Seqs older than `timeout_ms`. Updates their timestamps.
    pub fn due(&mut self, now_ms: u64, timeout_ms: u64) -> Vec<(u16, Vec<u8>)> {
        let mut out = Vec::new();
        for (s, p, t) in self.unacked.iter_mut() {
            if now_ms.saturating_sub(*t) >= timeout_ms {
                *t = now_ms;
                out.push((*s, p.clone()));
            }
        }
        out
    }

    pub fn pending(&self) -> usize {
        self.unacked.len()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn handshake_roundtrip() {
        let mut b = [0u8; 13];
        encode_handshake(0x0102030405060708, false, &mut b);
        assert_eq!(&b[0..4], &SIG);
        assert_eq!(decode_handshake(&b), Some((0x0102030405060708, false)));
        encode_handshake(99, true, &mut b);
        assert_eq!(decode_handshake(&b), Some((99, true)));
        b[0] ^= 0xFF;
        assert!(decode_handshake(&b).is_none());
        assert!(decode_handshake(&b[..5]).is_none());
    }

    #[test]
    fn data_header_roundtrip() {
        let h = encode_data_header(MSG_DATA, 1234, 2);
        assert_eq!(decode_data_header(&h), Some((MSG_DATA, 1234, 2)));
        assert!(decode_data_header(&h[..9]).is_none());
        let mut bad = h;
        bad[0] = 9;
        assert!(decode_data_header(&bad).is_none());
    }

    #[test]
    fn frag_reassemble_out_of_order() {
        let payload: Vec<u8> = (0..5000u32).map(|i| (i % 251) as u8).collect();
        let frags = frag_split(&payload, 1200);
        assert_eq!(frags.len(), 5);
        let n = frags.len() as u16;
        let mut r = Reassembler::new();
        // Deliver last, then first, then the rest.
        let mut order: Vec<u16> = (0..n).collect();
        order.reverse();
        let mut done = None;
        for i in order {
            let h = frag_header(i, i == n - 1);
            let mut hb = [0u8; 2];
            hb.copy_from_slice(&h.to_le_bytes());
            done = r.push(h, frags[i as usize]);
        }
        assert_eq!(done.unwrap(), payload);
    }

    #[test]
    fn reliable_header_roundtrip_window64() {
        let h = encode_reliable_header(false, 12, 1000, 999, 0b101);
        // 16B header: NGO uses window 64 on its reliable pipeline.
        assert_eq!(h.len(), 16);
        assert_eq!(decode_reliable_header(&h), Some((false, 12, 1000, 999, 0b101)));
        let a = encode_reliable_header(true, 0, 0, 500, 1);
        assert_eq!(decode_reliable_header(&a).unwrap().0, true);
        assert!(decode_reliable_header(&h[..15]).is_none());
    }

    #[test]
    fn ack_mask_and_resend_flow() {
        assert_eq!(build_ack_mask(10, &[10, 9, 7]), 0b1 | 0b10 | 0b1000);
        let mut q = ResendQueue::new();
        q.push(vec![1], 0);
        q.push(vec![2], 0);
        assert_eq!(q.pending(), 2);
        // Peer acks seq 1 with mask covering 0..=1.
        q.on_ack(1, build_ack_mask(1, &[1, 0]));
        assert_eq!(q.pending(), 0);
        q.push(vec![3], 100);
        assert!(q.due(150, 200).is_empty());
        assert_eq!(q.due(350, 200).len(), 1); // timed out -> resend
        q.on_ack(2, build_ack_mask(2, &[2]));
        assert_eq!(q.pending(), 0);
    }
}
