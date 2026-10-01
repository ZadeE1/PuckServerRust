mod admin;
mod config;
mod game;
mod metrics;
mod net;
mod ngo;
mod persistence;
mod physics;
mod physx;
mod puck_net;
mod session;
mod server;
mod utp;

use std::net::SocketAddr;
use std::sync::atomic::{AtomicUsize, Ordering};
use std::sync::Arc;
use std::time::{Duration, Instant};

use admin::{serve_admin, AdminRequest};
use config::Config;
use game::{Game, GameEvent, MatchConfig, Phase};
use metrics::TickStats;
use net::{
    decode_ack, decode_chat, decode_hello, decode_heartbeat, decode_input, encode_disc_resp,
    encode_event, encode_heartbeat, encode_reject, encode_snapshot, encode_welcome, NetEvent,
    Snapshot, SnapshotPlayer, P_ACK, P_CHAT, P_DISCONNECT, P_DISC_QUERY, P_EVENT, P_HEARTBEAT,
    P_HELLO, P_INPUT, REJECT_BADPASS, REJECT_BANNED, REJECT_FULL,
};
use persistence::{load_bans, load_totals, save_bans, save_totals, write_example_config};
use physics::{Player, PlayerInput, Role, Team, World};
use session::{ChatAction, JoinError, SessionManager};
use tokio::net::{TcpListener, UdpSocket};
use tokio::sync::mpsc;
use tracing::{info, warn};

#[tokio::main]
async fn main() -> anyhow::Result<()> {
    tracing_subscriber::fmt()
        .with_env_filter(tracing_subscriber::EnvFilter::from_default_env())
        .init();

    let cfg = Config::load_merged()?;
    info!(
        "puck-server-rust v1 starting '{}' mode={} udp={}:{} disc={} admin={} players={} tick={}Hz snap={}Hz",
        cfg.name,
        cfg.game_mode,
        cfg.bind,
        cfg.port,
        cfg.discovery_port,
        cfg.admin_port,
        cfg.max_players,
        cfg.tick_rate,
        cfg.snapshot_rate
    );
    std::fs::create_dir_all(&cfg.data_dir)?;
    let _ = write_example_config(&cfg.data_dir);

    let bind_addr = format!("{}:{}", cfg.bind, cfg.port);
    let socket = UdpSocket::bind(&bind_addr).await?;
    socket.set_broadcast(true)?;
    info!("game UDP bound on {} (LAN: connect from Windows via <mac-ip>:{} )", bind_addr, cfg.port);

    // Discovery socket (separate port, broadcast).
    let disc_socket = UdpSocket::bind(format!("0.0.0.0:{}", cfg.discovery_port)).await?;
    disc_socket.set_broadcast(true)?;
    let active_count = Arc::new(AtomicUsize::new(0));
    {
        let disc_socket = disc_socket;
        let active_count = active_count.clone();
        let name = cfg.name.clone();
        let port = cfg.port;
        let max = cfg.max_players;
        tokio::spawn(async move {
            let mut buf = vec![0u8; 256];
            loop {
                let Ok((n, src)) = disc_socket.recv_from(&mut buf).await else {
                    continue;
                };
                if n >= 2 && buf[0] == P_DISC_QUERY {
                    let players = active_count.load(Ordering::Relaxed) as u8;
                    let resp = encode_disc_resp(port, players, max as u8, &name);
                    let _ = disc_socket.send_to(&resp, src).await;
                }
            }
        });
    }

    // Admin TCP.
    let (admin_tx, mut admin_rx) = mpsc::unbounded_channel::<AdminRequest>();
    if cfg.admin_port != 0 {
        let listener = TcpListener::bind(format!("{}:{}", cfg.bind, cfg.admin_port)).await?;
        info!("admin TCP on {}:{} (nc + `auth <admin_password>`)\"", cfg.bind, cfg.admin_port);
        let tx = admin_tx.clone();
        let pw = cfg.admin_password.clone();
        tokio::spawn(async move {
            serve_admin(listener, pw, tx).await;
        });
    }

    // State.
    let mut world = World::new(cfg.puck_friction, cfg.restitution);
    let mut players: Vec<Player> = (0..cfg.max_players)
        .map(|i| Player {
            team: if i % 2 == 0 { Team::Red } else { Team::Blue },
            role: Role::Skater,
            ..Default::default()
        })
        .collect();
    // Two goalies: slot 0 red goalie, slot 1 blue goalie (if max>=2).
    if cfg.max_players >= 2 {
        players[0].role = Role::Goalie;
        players[0].team = Team::Red;
        players[1].role = Role::Goalie;
        players[1].team = Team::Blue;
    }
    let mut inputs: Vec<PlayerInput> = vec![PlayerInput::default(); cfg.max_players];
    let mut sessions = SessionManager::new(cfg.max_players, cfg.password.clone());
    sessions.bans = load_bans(&cfg.data_dir);
    info!("loaded {} bans", sessions.bans.len());
    let mut totals = load_totals(&cfg.data_dir);

    let match_cfg = MatchConfig {
        periods: cfg.periods,
        period_secs: cfg.period_secs,
        intermission_secs: cfg.intermission_secs,
        faceoff_secs: cfg.faceoff_secs,
        overtime: cfg.overtime,
        ..Default::default()
    };
    let mut game = Game::new(match_cfg, cfg.tick_rate, cfg.max_players);
    world.reset_for_faceoff(&mut players);

    // Bots at start if enabled.
    if cfg.bots {
        for _ in 0..(4.min(cfg.max_players)) {
            if let Some(id) = sessions.add_bot(0) {
                players[id].team = sessions.slots[id].team;
                if id < 2 {
                    players[id].role = Role::Goalie;
                }
            }
        }
        info!("bots enabled, {} active", sessions.active_count());
    }

    let tick_dt = 1.0 / cfg.tick_rate as f32;
    let snap_every = (cfg.tick_rate / cfg.snapshot_rate.max(1)).max(1) as u64;
    let mut tick: u64 = 0;
    let mut stats = TickStats::default();
    let mut buf = vec![0u8; 2048];
    let mut snap_buf = Vec::with_capacity(512);
    let mut ticker = tokio::time::interval(Duration::from_secs_f64(tick_dt as f64));
    ticker.set_missed_tick_behavior(tokio::time::MissedTickBehavior::Skip);
    let mut last_save = Instant::now();
    let mut gameover_tick: Option<u64> = None;
    let mut shutdown = false;

    info!("ready. Windows LAN client: direct-connect to <this-mac-ip>:{} (allow UDP firewall). Discovery broadcast on :{}.", cfg.port, cfg.discovery_port);

    loop {
        ticker.tick().await;
        if shutdown {
            break;
        }
        let t0 = Instant::now();
        tick += 1;

        // ---- admin commands (non-blocking) ----
        while let Ok(req) = admin_rx.try_recv() {
            let reply = handle_admin(
                &req.cmd,
                &req.args,
                &mut sessions,
                &mut players,
                &game,
                tick,
                &cfg,
            );
            if req.cmd == "stop" {
                let _ = req.resp.send(reply);
                shutdown = true;
                break;
            }
            let _ = req.resp.send(reply);
        }

        // ---- UDP inbound ----
        loop {
            match socket.try_recv_from(&mut buf) {
                Ok((n, src)) => {
                    if n == 0 {
                        continue;
                    }
                    handle_packet(
                        &buf[..n],
                        src,
                        tick,
                        &mut sessions,
                        &mut players,
                        &mut inputs,
                        &mut stats,
                        &socket,
                        &mut game,
                        &mut world,
                        &cfg,
                    )
                    .await;
                }
                Err(ref e) if e.kind() == std::io::ErrorKind::WouldBlock => break,
                Err(e) => {
                    warn!("udp recv: {}", e);
                    break;
                }
            }
        }

        // ---- bots fill inputs ----
        for i in 0..sessions.slots.len() {
            if sessions.slots[i].connected && sessions.slots[i].is_bot {
                inputs[i] = World::bot_input(&players[i], &world.puck, tick);
            }
        }

        // ---- auto-start match on first humans ----
        if game.phase == Phase::Lobby && sessions.active_count() >= 2 {
            let evs = start_match_now(&mut game, &mut world, &mut players);
            for ev in evs {
                queue_net_event(&mut sessions, &ev, &game, tick);
            }
            info!("match auto-started (warmup)");
        }

        // ---- physics + rules ----
        let goal = world.step(&mut players, &inputs, tick_dt);
        // Hits counter for stats (count hit inputs that connected).
        for (i, inp) in inputs.iter().enumerate() {
            if inp.hit && i < game.stats.len() && sessions.slots.get(i).map(|s| s.connected).unwrap_or(false) {
                // Count coarsely: only when puck near (avoid spam). Checked via last touch freshness.
                if tick.saturating_sub(players[i].last_touch_tick) < 5 {
                    game.stats[i].hits += 1;
                }
            }
        }
        let evs = game.tick(&mut world, &mut players, goal, tick);
        for ev in &evs {
            match ev {
                GameEvent::Goal { team, scorer } => {
                    info!("GOAL {:?} scorer={:?} {}-{}", team, scorer, game.score_red, game.score_blue);
                }
                GameEvent::PhaseChanged { from, to } => {
                    info!("phase {:?} -> {:?} period {} t={}ms", from, to, game.period, game.time_left_ms);
                }
                _ => {}
            }
            queue_net_event(&mut sessions, ev, &game, tick);
        }
        if game.phase == Phase::GameOver && gameover_tick.is_none() {
            gameover_tick = Some(tick);
            // Persist totals.
            for (i, s) in sessions.slots.iter().enumerate() {
                if s.connected && !s.is_bot && !s.name.is_empty() && i < game.stats.len() {
                    let e = totals.entry(s.name.clone()).or_insert(game::SkaterStats::default());
                    e.goals += game.stats[i].goals;
                    e.assists += game.stats[i].assists;
                    e.hits += game.stats[i].hits;
                }
            }
            let _ = save_totals(&cfg.data_dir, &totals);
            info!("game over {}-{} saved stats ({} players)", game.score_red, game.score_blue, totals.len());
        }
        // Auto-restart 15s after game over if players remain.
        if let Some(got) = gameover_tick {
            if tick.saturating_sub(got) > cfg.tick_rate as u64 * 15 {
                if sessions.active_count() >= 2 {
                    let evs = start_match_now(&mut game, &mut world, &mut players);
                    for ev in evs {
                        queue_net_event(&mut sessions, &ev, &game, tick);
                    }
                    info!("auto-restarted match");
                }
                gameover_tick = None;
            }
        }

        // ---- timeouts every tick (cheap) ----
        if tick % cfg.tick_rate as u64 == 0 {
            let timed = sessions.timeouts(tick, cfg.tick_rate);
            for id in timed {
                info!("client {} timed out", id);
            }
            active_count.store(sessions.active_count(), Ordering::Relaxed);
        }

        // ---- resend pending reliable events ----
        resend_pending(&mut sessions, &socket, tick, cfg.tick_rate, &mut stats).await;

        // ---- snapshots ----
        if tick % snap_every == 0 {
            let snap_players: Vec<SnapshotPlayer> = sessions
                .slots
                .iter()
                .enumerate()
                .filter(|(_, s)| s.connected)
                .map(|(i, _)| SnapshotPlayer::from_player(i, &players[i]))
                .collect();
            let snap = Snapshot {
                tick,
                phase: game.phase,
                score_red: game.score_red,
                score_blue: game.score_blue,
                period: game.period,
                time_left_ms: game.time_left_ms,
                puck: world.puck,
                players: snap_players,
            };
            encode_snapshot(&snap, &mut snap_buf);
            stats.snapshots += 1;
            for s in sessions.slots.iter().filter(|s| s.connected && !s.is_bot) {
                if let Some(a) = s.addr {
                    let _ = socket.send_to(&snap_buf, a).await;
                }
            }
        }

        // ---- periodic persistence ----
        if last_save.elapsed() > Duration::from_secs(60) {
            let _ = save_bans(&cfg.data_dir, &sessions.bans);
            let _ = save_totals(&cfg.data_dir, &totals);
            last_save = Instant::now();
        }

        stats.record(t0.elapsed().as_micros());
        if tick % (cfg.tick_rate as u64 * 5) == 0 {
            info!(
                "tick {} phase={:?} p{} {}-{} puck=({:.1},{:.1}) avg={:.1}µs max={}µs in={} snap={} ev={}",
                tick,
                game.phase,
                game.period,
                game.score_red,
                game.score_blue,
                world.puck.pos.x,
                world.puck.pos.y,
                stats.avg_micros(),
                stats.max_micros,
                stats.inputs,
                stats.snapshots,
                stats.events_sent
            );
        }
    }

    let _ = save_bans(&cfg.data_dir, &sessions.bans);
    let _ = save_totals(&cfg.data_dir, &totals);
    info!("server stopped cleanly");
    Ok(())
}

fn start_match_now(game: &mut Game, world: &mut World, players: &mut [Player]) -> Vec<GameEvent> {
    game.start_match(world, players);
    vec![GameEvent::PhaseChanged { from: Phase::Lobby, to: Phase::Warmup }]
}

fn queue_net_event(sessions: &mut SessionManager, ev: &GameEvent, game: &Game, tick: u64) {
    let net_ev = match ev {
        GameEvent::Goal { team, scorer } => NetEvent::Goal {
            team: *team,
            scorer: *scorer,
            score_red: game.score_red,
            score_blue: game.score_blue,
        },
        GameEvent::PhaseChanged { from, to } => NetEvent::Phase {
            from: *from,
            to: *to,
            period: game.period,
            time_left_ms: game.time_left_ms,
        },
        GameEvent::Chat { from, text } => NetEvent::Chat { from: from.clone(), text: text.clone() },
        GameEvent::ServerMessage { text } => NetEvent::ServerMsg { text: text.clone() },
    };
    // Per-client seq (reliable).
    for i in 0..sessions.slots.len() {
        if !sessions.slots[i].connected || sessions.slots[i].is_bot {
            continue;
        }
        let seq = sessions.slots[i].next_event_seq;
        sessions.slots[i].next_event_seq = seq.wrapping_add(1).max(1);
        let bytes = encode_event(seq, &net_ev);
        sessions.slots[i].pending.push_back((seq, bytes, tick));
        while sessions.slots[i].pending.len() > 32 {
            sessions.slots[i].pending.pop_front();
        }
    }
}

async fn resend_pending(
    sessions: &mut SessionManager,
    socket: &UdpSocket,
    tick: u64,
    tick_rate: u32,
    stats: &mut TickStats,
) {
    // Resend reliable events older than 500ms.
    let limit = (tick_rate / 2).max(1) as u64;
    for i in 0..sessions.slots.len() {
        if !sessions.slots[i].connected || sessions.slots[i].is_bot {
            continue;
        }
        let Some(addr) = sessions.slots[i].addr else {
            continue;
        };
        // Collect due items.
        let mut due: Vec<Vec<u8>> = Vec::new();
        let mut newest_sent = None;
        for (seq, bytes, last) in sessions.slots[i].pending.iter() {
            if tick.saturating_sub(*last) >= limit {
                due.push(bytes.clone());
                let _ = seq;
            }
            newest_sent = Some(*last);
        }
        // Also send brand-new events immediately (last == tick means just queued).
        // The limit check above already covers them when limit==0? Force: if any item
        // has last == tick, send it now.
        if due.is_empty() {
            for (_, bytes, last) in sessions.slots[i].pending.iter() {
                if *last == tick {
                    due.push(bytes.clone());
                }
            }
        }
        for bytes in due {
            let _ = socket.send_to(&bytes, addr).await;
            stats.events_sent += 1;
        }
        if !sessions.slots[i].pending.is_empty() {
            // Refresh timestamps for sent items to avoid tight loop; only if we sent.
            // We sent due+new; mark all as sent now (conservative, ack-driven).
            let any_due = sessions.slots[i].pending.iter().any(|(_, _, l)| tick.saturating_sub(*l) >= limit || *l == tick);
            if any_due {
                for (_, _, last) in sessions.slots[i].pending.iter_mut() {
                    *last = tick;
                }
            }
        }
        let _ = newest_sent;
    }
}

fn handle_admin(
    cmd: &str,
    args: &str,
    sessions: &mut SessionManager,
    players: &mut [Player],
    game: &Game,
    tick: u64,
    cfg: &Config,
) -> String {
    match cmd {
        "status" => format!(
            "OK tick={} phase={:?} period={} {}-{} players={}/{}",
            tick,
            game.phase,
            game.period,
            game.score_red,
            game.score_blue,
            sessions.active_count(),
            sessions.slots.len()
        ),
        "say" => {
            let ev = GameEvent::ServerMessage { text: format!("[ADMIN] {}", args) };
            queue_net_event(sessions, &ev, game, tick);
            "OK said".into()
        }
        "kick" => match args.parse::<usize>() {
            Ok(id) if sessions.kick(id) => format!("OK kicked {}", id),
            _ => "ERR usage: kick <id>".into(),
        },
        "ban" => {
            if let Ok(ip) = args.parse::<std::net::IpAddr>() {
                sessions.ban_ip(ip);
                let _ = save_bans(&cfg.data_dir, &sessions.bans);
                format!("OK banned {}", ip)
            } else if let Ok(id) = args.parse::<usize>() {
                if id < sessions.slots.len() {
                    if let Some(a) = sessions.slots[id].addr {
                        sessions.ban_ip(a.ip());
                        let _ = save_bans(&cfg.data_dir, &sessions.bans);
                        return format!("OK banned id {} ({})", id, a.ip());
                    }
                }
                "ERR bad id/ip".into()
            } else {
                "ERR usage: ban <id|ip>".into()
            }
        }
        "unban" => match args.parse::<std::net::IpAddr>() {
            Ok(ip) if sessions.unban_ip(&ip) => {
                let _ = save_bans(&cfg.data_dir, &sessions.bans);
                format!("OK unbanned {}", ip)
            }
            _ => "ERR usage: unban <ip>".into(),
        },
        "bots" => match args {
            "on" => {
                let mut n = 0;
                for _ in 0..4 {
                    if let Some(id) = sessions.add_bot(tick) {
                        players[id].team = sessions.slots[id].team;
                        n += 1;
                    }
                }
                format!("OK added {} bots", n)
            }
            "off" => {
                let mut n = 0;
                while sessions.remove_bot().is_some() {
                    n += 1;
                }
                format!("OK removed {} bots", n)
            }
            _ => "ERR usage: bots on|off".into(),
        },
        "stop" => "OK stopping".into(),
        "help" => "OK cmds: status say <msg> kick <id> ban <id|ip> unban <ip> bots on|off stop help".into(),
        _ => "ERR unknown. try help".into(),
    }
}

#[allow(clippy::too_many_arguments)]
async fn handle_packet(
    buf: &[u8],
    src: SocketAddr,
    tick: u64,
    sessions: &mut SessionManager,
    players: &mut [Player],
    inputs: &mut [PlayerInput],
    stats: &mut TickStats,
    socket: &UdpSocket,
    game: &mut Game,
    _world: &mut World,
    cfg: &Config,
) {
    if buf.is_empty() {
        return;
    }
    match buf[0] {
        P_HELLO => {
            let Some(h) = decode_hello(buf) else {
                return;
            };
            match sessions.join(&h.name, h.team_req, &h.password, src, tick) {
                Ok((id, team)) => {
                    stats.joins += 1;
                    players[id].team = team;
                    let w = encode_welcome(id as u8, tick, team as u8);
                    let _ = socket.send_to(&w, src).await;
                    // Welcome message reliably.
                    let ev = GameEvent::ServerMessage {
                        text: format!("Welcome {}! You are {:?} #{}", h.name, team, id),
                    };
                    queue_net_event(sessions, &ev, game, tick);
                    info!("join id={} name='{}' team={:?} from {}", id, h.name, team, src);
                }
                Err(e) => {
                    stats.rejects += 1;
                    let reason = match e {
                        JoinError::Full => REJECT_FULL,
                        JoinError::BadPassword => REJECT_BADPASS,
                        JoinError::Banned => REJECT_BANNED,
                        JoinError::BadName => REJECT_FULL,
                    };
                    let _ = socket.send_to(&encode_reject(reason), src).await;
                }
            }
        }
        P_INPUT => {
            let Some(pkt) = decode_input(buf) else {
                return;
            };
            let id = pkt.client_id as usize;
            // Anti-spoof: addr must own this id.
            match sessions.addr_to_id.get(&src) {
                Some(&owner) if owner == id => {}
                _ => return,
            }
            if sessions.touch(id, tick, pkt.seq) {
                if id < inputs.len() {
                    inputs[id] = pkt.input;
                    stats.inputs += 1;
                }
            }
            // Heartbeat implicit.
        }
        P_HEARTBEAT => {
            if let Some(hb) = decode_heartbeat(buf) {
                let _ = hb;
                if let Some(&id) = sessions.addr_to_id.get(&src) {
                    sessions.heartbeat(id, tick);
                    // Echo back so client can measure RTT.
                    let _ = socket.send_to(&encode_heartbeat(tick), src).await;
                }
            }
        }
        P_ACK => {
            if let Some(seq) = decode_ack(buf) {
                if let Some(&id) = sessions.addr_to_id.get(&src) {
                    sessions.ack(id, seq);
                    stats.events_acked += 1;
                }
            }
        }
        P_CHAT => {
            let Some(text) = decode_chat(buf) else {
                return;
            };
            let Some(&id) = sessions.addr_to_id.get(&src) else {
                return;
            };
            sessions.heartbeat(id, tick);
            match sessions.handle_chat(id, &text) {
                ChatAction::Reply(msg) => {
                    let ev = GameEvent::ServerMessage { text: msg };
                    // Reply only to sender.
                    if id < sessions.slots.len() && !sessions.slots[id].is_bot {
                        let seq = sessions.slots[id].next_event_seq;
                        sessions.slots[id].next_event_seq = seq.wrapping_add(1).max(1);
                        let net_ev = NetEvent::ServerMsg {
                            text: match &ev {
                                GameEvent::ServerMessage { text } => text.clone(),
                                _ => String::new(),
                            },
                        };
                        let bytes = encode_event(seq, &net_ev);
                        sessions.slots[id].pending.push_back((seq, bytes, tick));
                    }
                }
                ChatAction::Broadcast(msg) => {
                    let ev = GameEvent::ServerMessage { text: msg };
                    queue_net_event(sessions, &ev, game, tick);
                }
                ChatAction::BroadcastChat { from, text } => {
                    let ev = GameEvent::Chat { from, text };
                    queue_net_event(sessions, &ev, game, tick);
                }
                ChatAction::None => {}
            }
        }
        P_DISCONNECT => {
            if buf.len() >= 2 {
                let id = buf[1] as usize;
                if sessions.addr_to_id.get(&src) == Some(&id) {
                    sessions.disconnect(id);
                    info!("client {} disconnected", id);
                }
            }
        }
        P_EVENT => {
            // Client should not send events; ignore (but keep heartbeat fresh).
            if let Some(&id) = sessions.addr_to_id.get(&src) {
                sessions.heartbeat(id, tick);
            }
        }
        _ => {}
    }
    // Reliable events are sent by resend_pending() each tick (500ms cadence +
    // immediate for newly queued). No bulk resend here to avoid 60Hz storms.
    let _ = cfg;
}
