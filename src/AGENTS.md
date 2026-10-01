# src/ — AGENTS.md

## Purpose
- Complete authoritative PuckRust dedicated server (Mac-native, LAN-ready).
- `config.rs`: CLI + JSON file (`--config`), LAN bind/ports/password/match tuning.
- `physics.rs`: deterministic hockey core (teams, crease, faceoff, goals, bots).
- `game.rs`: match state machine (Lobby/Warmup/Faceoff/Playing/Goal/Intermission/Overtime/GameOver), clock, scores, skater stats.
- `net.rs`: UDP protocol v1 (Hello/Welcome/Input/Snapshot/Event+Ack/Heartbeat/Discovery/Chat).
- `session.rs`: slots, password/bans, team balance, host rights, timeouts, chat commands.
- `persistence.rs`: `data/bans.json`, `data/stats.json`, example config.
- `admin.rs`: TCP RCON (`auth/status/say/kick/ban/unban/bots/stop/help`).
- `metrics.rs`: tick timing + net counters. `main.rs`: tokio loop wiring all.

## Ownership
- Owned by root `puck-server-rust` binary. No lib target; all modules `mod` in `main.rs`.
- Public client contract: `PROTOCOL.md` + `client_example.py` (Windows stdlib client).

## Local Contracts
- `physics.rs`:
  - `Team { Red=1, Blue=2 }`, `Rink 60×30 goal_half 2.5 crease 3×3`, `World::step(&mut [Player], &[PlayerInput], dt) -> Option<Team>` (goal), `reset_for_faceoff`, `bot_input`. No alloc, dependency-free.
- `game.rs`: `Phase u8 0..7`, `MatchConfig`, `Game::tick(world, players, goal, tick) -> Vec<GameEvent>`, OT sudden death, auto-restart handled in main.
- `net.rs`: v1 types `0x01..0x20` (see PROTOCOL.md). Snapshot `37+20N` LE. Input 7B. Events reliable with seq/ack.
- `session.rs`: `SessionManager::join` (password/ban/balance), host = first join, 10s timeout, `/help /status /team /kick /ban`.
- `config.rs`: `Config::load_merged()` (JSON file + CLI), `validate()`, bind `0.0.0.0:7777` disc `:7778` admin `:7779`.
- `main.rs`: binds game+disc+admin, drains UDP via `try_recv`, steps full `players` array, snapshots connected only, resends events 500ms, saves bans/stats, `RUST_LOG=info` logs every 5s.
- `admin.rs`: `serve_admin`, `parse_line`.
- `persistence.rs`: JSON files only.

## Work Guidance
- Physics/rules first with unit tests; protocol changes require PROTOCOL.md + client_example.py + net tests update.
- Keep `physics.rs` dependency-free. Keep hot loop single-threaded, `MissedTickBehavior::Skip`.
- Never commit `data/*.json` with real bans, Steam downloads, or `target/`.

## Verification
- `cargo test` – 17 tests (physics/game/net/session/admin) must pass.
- Live LAN: `RUST_LOG=info cargo run -- --bind 0.0.0.0 --port 7777` then `python3 client_example.py --host 127.0.0.1` → `welcome` + snapshots; `--discover` finds server; `nc 127.0.0.1 7779` + `auth admin` + `status` → OK.
- Windows: same client on LAN via Mac IP; allow UDP firewall.

## Child DOX Index
- None (leaf modules, no subfolders).
