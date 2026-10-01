# PuckServerRust — AGENTS.md (Root DOX Rail)

## Purpose
- High-performance Rust remake of Puck (NS7, Steam App 2994020) dedicated server (App 3481440) – complete server-side, Mac-native.
- Original: Unity + Mono + Netcode/UTP, Linux x86_64 `Puck` + `start_server.sh` (password/bans, periods/OT/faceoff/intermission, chat commands, bots, TCP admin, mods).
- This repo: fixed-timestep 2D hockey + authoritative match machine + UDP v1 + TCP RCON, no GC, no alloc in hot loop. Runs natively on Mac ARM64/x86_64; Windows connects over LAN via IP or broadcast discovery.
- Lima test result (2026-10-01): native ARM64 Lima → `Exec format error`; qemu-user runs `--help` but full Unity server segfaults (SIGSEGV MAPERR). Use Rust server instead.

## Ownership
- Owner: local prototype (single-crate binary `puck-server-rust`).
- Original game IP/assets belong to NS7. This is a clean-room server prototype – do not copy Unity assets, `Puck_Data/`, or Steamworks binaries.

## Local Contracts
- UDP protocol v1 (see `PROTOCOL.md`, `src/net.rs`): Hello/Welcome/Reject, 7B Input, Snapshot `37+20N`, reliable Events + Ack, Heartbeat, Discovery `:7778`, Chat.
- Physics tick default 60Hz, snapshot default 20Hz, game `:7777` + admin TCP `:7779` (matches Puck `+port 7777` / `+password`).
- `World::step(&mut [Player], &[PlayerInput], dt) -> Option<Team>` must remain allocation-free.
- Linux x86_64 original download cache (not in repo): `/var/folders/.../T/opencode/puck-dl` via `steamcmd +@sSteamCmdForcePlatformType linux +app_update 3481440`.

## Work Guidance
- Rust 1.98+, edition 2021, tokio full, clap derive, tracing, serde.
- Keep `physics.rs` dependency-free (no tokio/serde) for benching and determinism.
- Add game rules (faceoff, penalties, goals) in `physics.rs`, netcode in `net.rs`, runtime in `main.rs`.
- Do not commit `Puck_Data/`, `*.so`, Steam downloads, or Lima images.

## Verification
- `cargo test` – 17 tests (physics/game/net/session/admin) must pass.
- `cargo run -- --port 7777 --tick-rate 60` – binds UDP, logs `tick ... avg_tick=..µs` every 5s.
- LAN: `python3 client_example.py --host <mac-ip>` → welcome + snapshots; `--discover` finds `:7778`; `nc <mac-ip> 7779` + `auth` + `status` → OK.
- Manual Lima re-test: `limactl shell main -- file /tmp/puck-dl/puck-dl/Puck` → x86-64; `./Puck` → Exec format error.

## Child DOX Index
- `src/AGENTS.md` – module contracts for config/physics/game/net/session/persistence/admin/metrics/main.
- `PROTOCOL.md` – UDP v1 + TCP admin contract for Windows client implementers (update with any `src/net.rs` change).
- `client_example.py` – stdlib Windows/Linux/Mac test client (keep in sync with `PROTOCOL.md`).
