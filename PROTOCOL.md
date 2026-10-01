# PuckRust UDP Protocol v1 (LAN: Mac server ↔ Windows client)

Transport: UDP. All integers little-endian. Max datagram < 1200B. First byte = type.

## Discovery (port 7778 default, broadcast)
- Query `client → 255.255.255.255:7778`: `[0x10, 0x01]`
- Response `server → client`: `[0x11, ver:u8, port:u16, players:u8, max:u8, name_len:u8, name:u8[]]`

## Session
- Hello `→ server:7777`: `[0x01, name_len:u8, name[≤24 UTF-8 alnum/_/-/space], team_req:u8 (0 auto,1 red,2 blue), pass_len:u8, pass[≤64]]`
- Welcome `←`: `[0x02, client_id:u8, tick:u64, team:u8]` — store `client_id`, include in every Input.
- Reject `←`: `[0x03, reason:u8]` — 1 full, 2 bad password, 3 banned.
- Heartbeat both ways: `[0x08, tick:u64]` every ~2s; server echoes. Timeout 10s → slot freed.
- Disconnect `→`: `[0x09, client_id:u8]`.

## Gameplay
- Input `→` at tick rate (recommend 60Hz, min 20Hz): `[0x04, client_id:u8, seq:u16, thrust_x:i8, thrust_y:i8, flags:u8]` — thrust −127..127, bit0 sprint, bit1 hit. Server drops stale seq + spoofed addr.
- Snapshot `←` at snapshot rate (default 20Hz): `[0x05, tick:u64, phase:u8, score_red:u8, score_blue:u8, period:u8, time_left_ms:u32, puck_x:f32, puck_y:f32, puck_vx:f32, puck_vy:f32, count:u8, pad:3, per-player 20B: id:u8, team:u8, x:f32, y:f32, vx:f32, vy:f32, stamina:u8 (0-255), flags:u8 (bit0 sprinting)]`
- Phases: 0 Lobby, 1 Warmup, 2 Faceoff, 3 Playing, 4 Goal, 5 Intermission, 6 Overtime, 7 GameOver. Teams: 1 Red (defends x=−30), 2 Blue (x=+30). Rink 60×30m, goals `|y|<2.5` on ±x.

## Reliable events (server → client, acked)
- Event: `[0x06, seq:u32, kind:u8, payload...]` — client MUST reply `[0x07, ack_seq:u32]` per event; server resends every 500ms until ack (cap 32 backlog).
- Kind 1 Goal: `[team:u8, scorer:u8 (255 none), score_red:u8, score_blue:u8]`
- Kind 2 Phase: `[from:u8, to:u8, period:u8, time_left_ms:u32]`
- Kind 3 Chat: `[from_len:u8, from[], msg_len:u8, msg[≤128]]`
- Kind 4 ServerMsg: `[len:u8, msg[≤128]]`

## Chat (client → server)
- `[0x20, len:u8, msg[≤128]]` — plain text broadcasts to all; `/help /status /team red|blue|auto` anyone; `/kick <id> /ban <id>` host (first joiner) only.

## Admin TCP (port 7779, line protocol)
`nc <mac-ip> 7779` → `auth <admin_password>` → `status | say <msg> | kick <id> | ban <id|ip> | unban <ip> | bots on|off | stop | help`.

## LAN checklist (Mac host + Windows client)
1. Mac: `cargo run -- --bind 0.0.0.0 --port 7777 --name "LAN"` (not 127.0.0.1).
2. Mac IP: System Settings → Wi-Fi → IP (e.g. 192.168.1.50).
3. Same Wi-Fi/network, no client isolation. Windows firewall: allow UDP out + in for Python/client on 7777/7778.
4. Windows: `python client_example.py --discover` then `--host 192.168.1.50 --name Zed`.
5. If no reply: ping Mac IP, check `RUST_LOG=info` server shows `join`, try `--bind 0.0.0.0`, disable VPNs.
