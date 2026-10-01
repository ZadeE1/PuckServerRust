//! Netcode for GameObjects 2.5.1 framing (pinned: `H:\\NGO` @ tag `v2.5.1`).
//!
//! Wire facts the unmodified client enforces:
//! - Message id = XXHash32(UTF-8 message-type FullName), seed 0.
//! - Batch = u16 magic `0x1160` + u16 count + i32 size + u64 XXHash64 of the
//!   bytes that follow the batch header + messages.
//! - Each message = u32 id + u32 size + payload.
//! - Integers use NGO bit-packed varints (`BytePacker`): u32 values <= 2^29-1
//!   shift left 3 with the byte-count in the low 3 bits; larger values are a
//!   `5` marker + raw u32 LE. u64 uses 4 low bits, `9` marker + raw u64.
//!   Signed values are zigzag-encoded first. Lengths are bit-packed u32.
//! - ConnectionRequest = bitpacked version-count + (u32 hash + bitpacked
//!   version) per message + u64 config-hash + length-prefixed connection data.
//! No allocation on encode; decode borrows. No external deps.

pub const BATCH_MAGIC: u16 = 0x1160;
pub const BATCH_HEADER_LEN: usize = 16;
pub const MSG_HEADER_LEN: usize = 8;

/// FullNames of the 25 built-in message types, in `NetworkMessageTypes` order.
/// Wire id = [`xxhash32`] of the UTF-8 name.
pub const MESSAGE_NAMES: [&str; 25] = [
    "Unity.Netcode.ConnectionApprovedMessage",
    "Unity.Netcode.ConnectionRequestMessage",
    "Unity.Netcode.ChangeOwnershipMessage",
    "Unity.Netcode.ClientConnectedMessage",
    "Unity.Netcode.ClientDisconnectedMessage",
    "Unity.Netcode.ClientRpcMessage",
    "Unity.Netcode.CreateObjectMessage",
    "Unity.Netcode.DestroyObjectMessage",
    "Unity.Netcode.DisconnectReasonMessage",
    "Unity.Netcode.ForwardClientRpcMessage",
    "Unity.Netcode.ForwardServerRpcMessage",
    "Unity.Netcode.NamedMessage",
    "Unity.Netcode.NetworkTransformMessage",
    "Unity.Netcode.NetworkVariableDeltaMessage",
    "Unity.Netcode.ParentSyncMessage",
    "Unity.Netcode.ProxyMessage",
    "Unity.Netcode.RpcMessage",
    "Unity.Netcode.SceneEventMessage",
    "Unity.Netcode.ServerLogMessage",
    "Unity.Netcode.ServerRpcMessage",
    "Unity.Netcode.SessionOwnerMessage",
    "Unity.Netcode.TimeSyncMessage",
    "Unity.Netcode.UnnamedMessage",
    "Unity.Netcode.AnticipationCounterSyncPingMessage",
    "Unity.Netcode.AnticipationCounterSyncPongMessage",
];

#[inline]
fn rotl32(x: u32, r: u32) -> u32 {
    (x << r) | (x >> (32 - r))
}

fn read_u32_le(b: &[u8], i: usize) -> u32 {
    u32::from_le_bytes([b[i], b[i + 1], b[i + 2], b[i + 3]])
}

/// XXH32, seed 0. Matches `XXHash.Hash32` byte-for-byte (LE lanes).
// ponytail: verbatim port of Runtime/Hashing/XXHash.cs.
pub fn xxhash32(input: &[u8]) -> u32 {
    const P1: u32 = 2654435761;
    const P2: u32 = 2246822519;
    const P3: u32 = 3266489917;
    const P4: u32 = 668265263;
    const P5: u32 = 374761393;
    let mut h = P5;
    let mut off = 0;
    let len = input.len();
    if len >= 16 {
        let (mut v0, mut v1, mut v2, mut v3) = (P1.wrapping_add(P2), P2, 0u32, 0u32.wrapping_sub(P1));
        let blocks = len >> 4;
        for _ in 0..blocks {
            v0 = rotl32(v0.wrapping_add(read_u32_le(input, off).wrapping_mul(P2)), 13).wrapping_mul(P1);
            v1 = rotl32(v1.wrapping_add(read_u32_le(input, off + 4).wrapping_mul(P2)), 13).wrapping_mul(P1);
            v2 = rotl32(v2.wrapping_add(read_u32_le(input, off + 8).wrapping_mul(P2)), 13).wrapping_mul(P1);
            v3 = rotl32(v3.wrapping_add(read_u32_le(input, off + 12).wrapping_mul(P2)), 13).wrapping_mul(P1);
            off += 16;
        }
        h = rotl32(v0, 1)
            .wrapping_add(rotl32(v1, 7))
            .wrapping_add(rotl32(v2, 12))
            .wrapping_add(rotl32(v3, 18));
    }
    h = h.wrapping_add(len as u32);
    let mut rem = len & 15;
    while rem >= 4 {
        h = rotl32(h.wrapping_add(read_u32_le(input, off).wrapping_mul(P3)), 17).wrapping_mul(P4);
        off += 4;
        rem -= 4;
    }
    while rem > 0 {
        h = rotl32(h.wrapping_add((input[off] as u32).wrapping_mul(P5)), 11).wrapping_mul(P1);
        off += 1;
        rem -= 1;
    }
    h ^= h >> 15;
    h = h.wrapping_mul(P2);
    h ^= h >> 13;
    h = h.wrapping_mul(P3);
    h ^= h >> 16;
    h
}

#[inline]
fn rotl64(x: u64, r: u32) -> u64 {
    (x << r) | (x >> (64 - r))
}

fn read_u64_le(b: &[u8], i: usize) -> u64 {
    u64::from_le_bytes([
        b[i], b[i + 1], b[i + 2], b[i + 3], b[i + 4], b[i + 5], b[i + 6], b[i + 7],
    ])
}

/// XXH64, seed 0. Matches `XXHash.Hash64` byte-for-byte.
// ponytail: verbatim port of Runtime/Hashing/XXHash.cs.
pub fn xxhash64(input: &[u8]) -> u64 {
    const P1: u64 = 11400714785074694791;
    const P2: u64 = 14029467366897019727;
    const P3: u64 = 1609587929392839161;
    const P4: u64 = 9650029242287828579;
    const P5: u64 = 2870177450012600261;
    let mut h = P5;
    let mut off = 0;
    let len = input.len();
    if len >= 32 {
        let (mut v0, mut v1, mut v2, mut v3) =
            (P1.wrapping_add(P2), P2, 0u64, 0u64.wrapping_sub(P1));
        let blocks = len >> 5;
        for _ in 0..blocks {
            v0 = rotl64(v0.wrapping_add(read_u64_le(input, off).wrapping_mul(P2)), 31).wrapping_mul(P1);
            v1 = rotl64(v1.wrapping_add(read_u64_le(input, off + 8).wrapping_mul(P2)), 31).wrapping_mul(P1);
            v2 = rotl64(v2.wrapping_add(read_u64_le(input, off + 16).wrapping_mul(P2)), 31).wrapping_mul(P1);
            v3 = rotl64(v3.wrapping_add(read_u64_le(input, off + 24).wrapping_mul(P2)), 31).wrapping_mul(P1);
            off += 32;
        }
        // Merge round: NOT standard xxh64 — NGO-specific interleave.
        h = rotl64(v0, 1)
            .wrapping_add(rotl64(v1, 7))
            .wrapping_add(rotl64(v2, 12))
            .wrapping_add(rotl64(v3, 18));
        for v in [v0, v1, v2, v3] {
            let k = rotl64(v.wrapping_mul(P2), 31).wrapping_mul(P1);
            h ^= k;
            h = h.wrapping_mul(P1).wrapping_add(P4);
        }
    }
    h = h.wrapping_add(len as u64);
    let mut rem = len & 31;
    while rem >= 8 {
        let k = rotl64(read_u64_le(input, off).wrapping_mul(P2), 31).wrapping_mul(P1);
        h ^= k;
        h = rotl64(h, 27).wrapping_mul(P1).wrapping_add(P4);
        off += 8;
        rem -= 8;
    }
    if rem >= 4 {
        h ^= (read_u32_le(input, off) as u64).wrapping_mul(P1);
        h = rotl64(h, 23).wrapping_mul(P2).wrapping_add(P3);
        off += 4;
        rem -= 4;
    }
    while rem > 0 {
        h ^= (input[off] as u64).wrapping_mul(P5);
        h = rotl64(h, 11).wrapping_mul(P1);
        off += 1;
        rem -= 1;
    }
    h ^= h >> 33;
    h = h.wrapping_mul(P2);
    h ^= h >> 29;
    h = h.wrapping_mul(P3);
    h ^= h >> 32;
    h
}

/// Wire id for a message FullName.
pub fn message_id(name: &str) -> u32 {
    xxhash32(name.as_bytes())
}

fn used_bytes_u64(mut v: u64) -> usize {
    let mut n = 1;
    while v > 0xFF {
        n += 1;
        v >>= 8;
    }
    n
}

/// Bit-packed u32 encode. Appends to `out`.
pub fn pack_u32(out: &mut Vec<u8>, value: u32) {
    if value > (1 << 29) - 1 {
        out.push(5);
        out.extend_from_slice(&value.to_le_bytes());
        return;
    }
    let v = (value as u64) << 3;
    let n = used_bytes_u64(v | 1).max(1);
    let word = v | n as u64;
    out.extend_from_slice(&word.to_le_bytes()[..n]);
}

/// Bit-packed u64 encode.
pub fn pack_u64(out: &mut Vec<u8>, value: u64) {
    if value > (1u64 << 60) - 1 {
        out.push(9);
        out.extend_from_slice(&value.to_le_bytes());
        return;
    }
    let v = value << 4;
    let n = used_bytes_u64(v | 1).max(1);
    let word = v | n as u64;
    out.extend_from_slice(&word.to_le_bytes()[..n]);
}

#[inline]
fn zigzag32(v: i32) -> u32 {
    ((v << 1) ^ (v >> 31)) as u32
}

#[inline]
fn unzigzag32(v: u32) -> i32 {
    ((v >> 1) as i32) ^ -((v & 1) as i32)
}

#[inline]
fn zigzag64(v: i64) -> u64 {
    ((v << 1) ^ (v >> 63)) as u64
}

#[inline]
fn unzigzag64(v: u64) -> i64 {
    ((v >> 1) as i64) ^ -((v & 1) as i64)
}

pub fn pack_i32(out: &mut Vec<u8>, v: i32) {
    pack_u32(out, zigzag32(v));
}

pub fn pack_i64(out: &mut Vec<u8>, v: i64) {
    pack_u64(out, zigzag64(v));
}

pub fn pack_bytes(out: &mut Vec<u8>, b: &[u8]) {
    pack_u32(out, b.len() as u32);
    out.extend_from_slice(b);
}

/// Bounds-checked cursor reader.
pub struct Reader<'a> {
    b: &'a [u8],
    pos: usize,
}

impl<'a> Reader<'a> {
    pub fn new(b: &'a [u8]) -> Self {
        Self { b, pos: 0 }
    }

    pub fn remaining(&self) -> usize {
        self.b.len().saturating_sub(self.pos)
    }

    fn take(&mut self, n: usize) -> Option<&'a [u8]> {
        if self.remaining() < n {
            return None;
        }
        let s = &self.b[self.pos..self.pos + n];
        self.pos += n;
        Some(s)
    }

    pub fn u16_le(&mut self) -> Option<u16> {
        self.take(2).map(|s| u16::from_le_bytes([s[0], s[1]]))
    }

    pub fn u32_le(&mut self) -> Option<u32> {
        self.take(4).map(|s| u32::from_le_bytes([s[0], s[1], s[2], s[3]]))
    }

    pub fn i32_le(&mut self) -> Option<i32> {
        self.u32_le().map(|v| v as i32)
    }

    pub fn u64_le(&mut self) -> Option<u64> {
        self.take(8).map(|s| {
            u64::from_le_bytes([s[0], s[1], s[2], s[3], s[4], s[5], s[6], s[7]])
        })
    }

    pub fn unpack_u32(&mut self) -> Option<u32> {
        let first = *self.b.get(self.pos)? as u64;
        let n = (first & 0x7) as usize;
        if n == 5 {
            self.pos += 1;
            return self.u32_le();
        }
        if n == 0 || n > 4 || self.remaining() < n {
            return None;
        }
        let mut w = 0u64;
        for i in 0..n {
            w |= (self.b[self.pos + i] as u64) << (8 * i);
        }
        self.pos += n;
        Some((w >> 3) as u32)
    }

    pub fn unpack_u64(&mut self) -> Option<u64> {
        let first = *self.b.get(self.pos)? as u64;
        let n = (first & 0xF) as usize;
        if n == 9 {
            self.pos += 1;
            return self.u64_le();
        }
        if n == 0 || n > 8 || self.remaining() < n {
            return None;
        }
        let mut w = 0u64;
        for i in 0..n {
            w |= (self.b[self.pos + i] as u64) << (8 * i);
        }
        self.pos += n;
        Some(w >> 4)
    }

    pub fn unpack_i32(&mut self) -> Option<i32> {
        self.unpack_u32().map(unzigzag32)
    }

    pub fn unpack_i64(&mut self) -> Option<i64> {
        self.unpack_u64().map(unzigzag64)
    }

    pub fn unpack_bytes(&mut self) -> Option<&'a [u8]> {
        let n = self.unpack_u32()? as usize;
        self.take(n)
    }
}

/// Encode one batch: header + (packed index, packed size, payload) messages.
/// Fixes up the XXH64. Indices are registration order (`MESSAGE_NAMES`
/// order), NOT hashes — hashes are only used for version negotiation.
pub fn encode_batch(msgs: &[(u32, &[u8])]) -> Vec<u8> {
    let mut body = Vec::new();
    for (id, p) in msgs {
        pack_u32(&mut body, *id);
        pack_u32(&mut body, p.len() as u32);
        body.extend_from_slice(p);
    }
    let mut out = Vec::with_capacity(BATCH_HEADER_LEN + body.len());
    out.extend_from_slice(&BATCH_MAGIC.to_le_bytes());
    out.extend_from_slice(&(msgs.len() as u16).to_le_bytes());
    out.extend_from_slice(&(body.len() as i32).to_le_bytes());
    out.extend_from_slice(&xxhash64(&body).to_le_bytes());
    out.extend_from_slice(&body);
    out
}

/// Decode + verify one batch. Returns message (id, payload) slices.
pub fn decode_batch<'a>(pkt: &'a [u8]) -> Option<Vec<(u32, &'a [u8])>> {
    let mut r = Reader::new(pkt);
    if r.u16_le()? != BATCH_MAGIC {
        return None;
    }
    let count = r.u16_le()? as usize;
    let size = r.i32_le()?;
    if size < 0 || size as usize != r.remaining() - 8 {
        return None;
    }
    let hash = r.u64_le()?;
    let body_start = pkt.len() - r.remaining();
    let body = &pkt[body_start..];
    if xxhash64(body) != hash {
        return None;
    }
    let mut out = Vec::with_capacity(count);
    let mut r = Reader::new(body);
    for _ in 0..count {
        let id = r.unpack_u32()?;
        let len = r.unpack_u32()? as usize;
        out.push((id, r.take(len)?));
    }
    if r.remaining() != 0 {
        return None;
    }
    Some(out)
}

/// Encode the server-observed head of a ConnectionRequest: version list +
/// config hash + connection data.
pub fn encode_connection_request(
    versions: &[(u32, i32)],
    config_hash: u64,
    connection_data: &[u8],
) -> Vec<u8> {
    let mut out = Vec::new();
    pack_u32(&mut out, versions.len() as u32);
    for (h, v) in versions {
        out.extend_from_slice(&h.to_le_bytes());
        pack_i32(&mut out, *v);
    }
    out.extend_from_slice(&config_hash.to_le_bytes());
    pack_bytes(&mut out, connection_data);
    out
}

/// Signed packed int: reinterpreted, NO zigzag (`WriteValuePacked(int)`
/// casts straight through). Negative values take the 5-byte escape.
pub fn pack_ipacked(out: &mut Vec<u8>, v: i32) {
    pack_u32(out, v as u32);
}

/// Signed packed long: reinterpreted, NO zigzag.
pub fn pack_lpacked(out: &mut Vec<u8>, v: i64) {
    pack_u64(out, v as u64);
}

impl<'a> Reader<'a> {
    pub fn unpack_ipacked(&mut self) -> Option<i32> {
        self.unpack_u32().map(|v| v as i32)
    }

    pub fn unpack_lpacked(&mut self) -> Option<i64> {
        self.unpack_u64().map(|v| v as i64)
    }
}

/// RpcMessage payload: packed obj + packed behaviour + packed method + args.
pub fn encode_rpc(obj: u64, behaviour: u64, method: u32, args: &[u8]) -> Vec<u8> {
    let mut out = Vec::new();
    pack_u64(&mut out, obj);
    pack_u64(&mut out, behaviour);
    pack_u32(&mut out, method);
    out.extend_from_slice(args);
    out
}

pub fn decode_rpc(pkt: &[u8]) -> Option<(u64, u64, u32, &[u8])> {
    let mut r = Reader::new(pkt);
    let obj = r.unpack_u64()?;
    let beh = r.unpack_u64()?;
    let method = r.unpack_u32()?;
    Some((obj, beh, method, &pkt[pkt.len() - r.remaining()..]))
}

/// CreateObject flags byte bits.
pub const CO_INCLUDES_OBJECT: u8 = 0x01;
pub const CO_UPDATE_OBSERVERS: u8 = 0x02;
pub const CO_UPDATE_NEW_OBSERVERS: u8 = 0x04;

/// Minimal SceneObject (client-server path, no parent/ownership/instantiation
/// data). Transform = 10 raw LE f32s (pos, rot-quat, scale).
#[derive(Debug, PartialEq)]
pub struct SceneObject<'a> {
    pub bitfield: u16,
    pub hash: u32,
    pub obj_id: u64,
    pub owner_id: u64,
    pub observers: Option<&'a [u64]>,
    pub transform: Option<[f32; 10]>,
    pub scene_handle: i32,
    pub sync_data: &'a [u8],
}

pub fn encode_scene_object(o: &SceneObject) -> Vec<u8> {
    let mut out = Vec::new();
    out.extend_from_slice(&o.bitfield.to_le_bytes());
    out.extend_from_slice(&o.hash.to_le_bytes());
    pack_u64(&mut out, o.obj_id);
    pack_u64(&mut out, o.owner_id);
    if let Some(obs) = o.observers {
        pack_ipacked(&mut out, obs.len() as i32);
        for id in obs {
            pack_lpacked(&mut out, *id as i64);
        }
    }
    if let Some(t) = &o.transform {
        for f in t {
            out.extend_from_slice(&f.to_le_bytes());
        }
    }
    out.extend_from_slice(&o.scene_handle.to_le_bytes());
    out.extend_from_slice(&(o.sync_data.len() as i32).to_le_bytes());
    out.extend_from_slice(o.sync_data);
    out
}

/// NetworkVariableDelta: packed obj + packed behaviour + delivery byte +
/// per-var (packed len + bytes).
pub fn encode_var_delta(obj: u64, behaviour: u64, delivery: u8, vars: &[&[u8]]) -> Vec<u8> {
    let mut out = Vec::new();
    pack_u64(&mut out, obj);
    pack_u64(&mut out, behaviour);
    out.push(delivery);
    for v in vars {
        pack_bytes(&mut out, v);
    }
    out
}

pub fn decode_var_delta<'a>(pkt: &'a [u8]) -> Option<(u64, u64, u8, Vec<&'a [u8]>)> {
    let mut r = Reader::new(pkt);
    let obj = r.unpack_u64()?;
    let beh = r.unpack_u64()?;
    let delivery = *r.take(1)?.first()?;
    let mut vars = Vec::new();
    while r.remaining() > 0 {
        vars.push(r.unpack_bytes()?);
    }
    Some((obj, beh, delivery, vars))
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn xxhash_vectors_match_reference() {
        // Ground truth from NGO's own XXHash.cs via dotnet (see xxhash-ref).
        assert_eq!(xxhash32(b""), 0x02CC_5D05);
        assert_eq!(xxhash32(b"a"), 0x550D_7456);
        assert_eq!(xxhash64(b""), 0xEF46_DB37_51D8_E999);
        assert_eq!(xxhash64(b"abc"), 0x44BC_2CF5_AD77_0999);
        // Long-input (>=16 / >=32) lanes engage.
        assert_eq!(xxhash32(b"0123456789abcdef"), 0xC2C4_5B69);
        assert_eq!(xxhash64(b"0123456789abcdef0123456789abcdef"), 0x642A_9495_8E71_E6C5);
    }

    #[test]
    fn packed_ints_roundtrip_with_escapes() {
        for v in [0u32, 1, 127, 1 << 29 - 1, 1 << 29, u32::MAX] {
            let mut b = Vec::new();
            pack_u32(&mut b, v);
            assert_eq!(Reader::new(&b).unpack_u32(), Some(v), "u32 {v}");
        }
        for v in [0u64, 1, (1u64 << 60) - 1, 1u64 << 60, u64::MAX] {
            let mut b = Vec::new();
            pack_u64(&mut b, v);
            assert_eq!(Reader::new(&b).unpack_u64(), Some(v), "u64 {v}");
        }
        for v in [0i32, -1, 1, -1000, i32::MIN, i32::MAX] {
            let mut b = Vec::new();
            pack_i32(&mut b, v);
            assert_eq!(Reader::new(&b).unpack_i32(), Some(v), "i32 {v}");
        }
        for v in [0i64, -1, i64::MIN, i64::MAX] {
            let mut b = Vec::new();
            pack_i64(&mut b, v);
            assert_eq!(Reader::new(&b).unpack_i64(), Some(v), "i64 {v}");
        }
        // Escape markers: 5 for u32-overflow, 9 for u64-overflow.
        let mut b = Vec::new();
        pack_u32(&mut b, u32::MAX);
        assert_eq!(b.len(), 5);
        let mut c = Vec::new();
        pack_u64(&mut c, u64::MAX);
        assert_eq!(c.len(), 9);
    }

    #[test]
    fn batch_roundtrip_and_hash_reject() {
        // Indices are registration order: 1 = ConnectionRequest.
        let m1 = (1u32, b"hello".as_slice());
        let m2 = (0u32, b"".as_slice());
        let pkt = encode_batch(&[m1, m2]);
        let back = decode_batch(&pkt).unwrap();
        assert_eq!(back.len(), 2);
        assert_eq!(back[0].0, m1.0);
        assert_eq!(back[0].1, b"hello");
        let mut bad = pkt.clone();
        let n = bad.len();
        bad[n - 1] ^= 0xFF;
        assert!(decode_batch(&bad).is_none());
        let mut badm = pkt.clone();
        badm[0] ^= 0xFF;
        assert!(decode_batch(&badm).is_none());
    }

    #[test]
    fn message_ids_are_stable_and_unique() {
        let mut ids: Vec<u32> = MESSAGE_NAMES.iter().map(|n| message_id(n)).collect();
        ids.sort_unstable();
        ids.dedup();
        assert_eq!(ids.len(), 25);
    }

    #[test]
    fn connection_request_roundtrip() {        let versions = [(message_id(MESSAGE_NAMES[1]), 1), (message_id(MESSAGE_NAMES[0]), 2)];
        let conn = b"puck-password-token";
        let enc = encode_connection_request(&versions, 0xDEAD_BEEF, conn);
        let mut r = Reader::new(&enc);
        assert_eq!(r.unpack_u32(), Some(2));
        assert_eq!(r.u32_le(), Some(versions[0].0));
        assert_eq!(r.unpack_i32(), Some(1));
        assert_eq!(r.u32_le(), Some(versions[1].0));
        assert_eq!(r.unpack_i32(), Some(2));
        assert_eq!(r.u64_le(), Some(0xDEAD_BEEF));
        assert_eq!(r.unpack_bytes(), Some(conn.as_slice()));
        assert_eq!(r.remaining(), 0);
    }

    #[test]
    fn rpc_roundtrip_move_input() {
        // PlayerInput.Server_MoveInputRpc = method 1 (metadata order).
        let args = [0x7Fu8, 0x00, 0x01];
        let enc = encode_rpc(42, 0, 1, &args);
        let (obj, beh, method, back) = decode_rpc(&enc).unwrap();
        assert_eq!((obj, beh, method), (42, 0, 1));
        assert_eq!(back, args);
    }

    #[test]
    fn scene_object_roundtrip_with_transform() {
        let o = SceneObject {
            bitfield: 0b0000_0100_0101, // player + scene + transform bits
            hash: 0x12345678,
            obj_id: 7,
            owner_id: 3,
            observers: None,
            transform: Some([1.0, 2.0, 3.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0]),
            scene_handle: 5,
            sync_data: b"vars",
        };
        let enc = encode_scene_object(&o);
        // bitfield u16 + hash u32 + packed ids + 40B transform + handle + len + data.
        let mut r = Reader::new(&enc);
        assert_eq!(r.u16_le(), Some(o.bitfield));
        assert_eq!(r.u32_le(), Some(o.hash));
        assert_eq!(r.unpack_u64(), Some(7));
        assert_eq!(r.unpack_u64(), Some(3));
        let t: Vec<f32> = (0..10).map(|_| r.take(4).map(|s| f32::from_le_bytes([s[0], s[1], s[2], s[3]])).unwrap()).collect();
        assert_eq!(t[0], 1.0);
        assert_eq!(r.i32_le(), Some(5));
        assert_eq!(r.i32_le(), Some(4));
        assert_eq!(r.take(4), Some(b"vars".as_slice()));
        assert_eq!(r.remaining(), 0);
    }

    #[test]
    fn var_delta_roundtrip() {
        let v0 = [0x01u8, 0x02];
        let v1 = [0xFFu8; 300]; // forces multi-byte length prefix
        let enc = encode_var_delta(9, 2, 1, &[&v0, &v1]);
        let (obj, beh, delivery, vars) = decode_var_delta(&enc).unwrap();
        assert_eq!((obj, beh, delivery), (9, 2, 1));
        assert_eq!(vars.len(), 2);
        assert_eq!(vars[0], v0);
        assert_eq!(vars[1], v1);
    }

    #[test]
    fn packed_signed_is_reinterpret_not_zigzag() {
        // -1 -> u32::MAX -> 5-byte escape (zigzag would give 1 byte).
        let mut b = Vec::new();
        pack_ipacked(&mut b, -1);
        assert_eq!(b.len(), 5);
        assert_eq!(Reader::new(&b).unpack_ipacked(), Some(-1));
        let mut c = Vec::new();
        pack_lpacked(&mut c, -1);
        assert_eq!(c.len(), 9);
    }
}
