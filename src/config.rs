use clap::Parser;
use serde::{Deserialize, Serialize};
use std::path::PathBuf;

/// Full server configuration. CLI overrides file. Env: PUCK_* not yet – use CLI.
/// Mac native: binds 0.0.0.0 by default so Windows LAN clients can connect directly by IP.
#[derive(Debug, Clone, Parser, Serialize, Deserialize)]
#[command(name = "puck-server-rust", about = "Puck dedicated server (Rust, Mac-native, LAN-ready)")]
#[serde(default)]
pub struct Config {
    /// Server display name (shown in LAN discovery + snapshots)
    #[arg(long, default_value = "PuckRust LAN")]
    pub name: String,

    /// Game UDP bind address. Use 0.0.0.0 for LAN. 127.0.0.1 for local-only.
    #[arg(long, default_value = "0.0.0.0")]
    pub bind: String,

    /// Game UDP port (clients connect here). Matches Puck +port.
    #[arg(long, default_value_t = 7777)]
    pub port: u16,

    /// LAN discovery UDP port (broadcast query/response).
    #[arg(long, default_value_t = 7778)]
    pub discovery_port: u16,

    /// Admin TCP port (RCON line protocol). 0 = disabled.
    #[arg(long, default_value_t = 7779)]
    pub admin_port: u16,

    /// Game password (empty = open). Matches Puck +password.
    #[arg(long, default_value = "")]
    pub password: String,

    /// Admin password for TCP RCON kick/ban/say/stop.
    #[arg(long, default_value = "admin")]
    pub admin_password: String,

    /// Max players (1..16).
    #[arg(long, default_value_t = 10)]
    pub max_players: usize,

    /// Physics tick Hz (30..120).
    #[arg(long, default_value_t = 60)]
    pub tick_rate: u32,

    /// Snapshot broadcast Hz (<= tick).
    #[arg(long, default_value_t = 20)]
    pub snapshot_rate: u32,

    /// Game mode: standard | competitive | public
    #[arg(long, default_value = "standard")]
    pub game_mode: String,

    /// Regulation periods (1..5).
    #[arg(long, default_value_t = 3)]
    pub periods: u8,

    /// Period length seconds (60..1200).
    #[arg(long, default_value_t = 300)]
    pub period_secs: u32,

    /// Intermission seconds between periods.
    #[arg(long, default_value_t = 30)]
    pub intermission_secs: u32,

    /// Faceoff countdown seconds.
    #[arg(long, default_value_t = 3)]
    pub faceoff_secs: u32,

    /// Overtime enabled (sudden death).
    #[arg(long, default_value_t = true)]
    pub overtime: bool,

    /// Fill empty slots with bots (simple chase AI).
    #[arg(long, default_value_t = false)]
    pub bots: bool,

    /// Puck friction 1/s.
    #[arg(long, default_value_t = 0.35)]
    pub puck_friction: f32,

    /// Wall restitution 0..1.
    #[arg(long, default_value_t = 0.85)]
    pub restitution: f32,

    /// Data dir for bans.json / stats.json / puck-server.json
    #[arg(long, default_value = "data")]
    pub data_dir: PathBuf,

    /// Optional JSON config file to load first (CLI overrides).
    #[arg(long)]
    pub config: Option<PathBuf>,
}

impl Default for Config {
    fn default() -> Self {
        // Clap defaults via parse_from empty won't run; construct manually.
        Self {
            name: "PuckRust LAN".into(),
            bind: "0.0.0.0".into(),
            port: 7777,
            discovery_port: 7778,
            admin_port: 7779,
            password: String::new(),
            admin_password: "admin".into(),
            max_players: 10,
            tick_rate: 60,
            snapshot_rate: 20,
            game_mode: "standard".into(),
            periods: 3,
            period_secs: 300,
            intermission_secs: 30,
            faceoff_secs: 3,
            overtime: true,
            bots: false,
            puck_friction: 0.35,
            restitution: 0.85,
            data_dir: PathBuf::from("data"),
            config: None,
        }
    }
}

impl Config {
    /// Load JSON file + apply CLI overrides (CLI wins when non-default).
    /// Simple approach: if --config given, load file then re-apply explicitly-set CLI args.
    /// For simplicity we load file first, then parse CLI and merge non-default scalar fields.
    pub fn load_merged() -> anyhow::Result<Self> {
        // Peek raw args for --config path without full parse.
        let raw: Vec<String> = std::env::args().collect();
        let mut file_cfg: Option<Config> = None;
        let mut i = 0;
        while i < raw.len() {
            if raw[i] == "--config" && i + 1 < raw.len() {
                let p = PathBuf::from(&raw[i + 1]);
                let data = std::fs::read_to_string(&p)?;
                file_cfg = Some(serde_json::from_str(&data)?);
                break;
            }
            if let Some(v) = raw[i].strip_prefix("--config=") {
                let data = std::fs::read_to_string(v)?;
                file_cfg = Some(serde_json::from_str(&data)?);
                break;
            }
            i += 1;
        }
        let cli = Config::parse();
        if let Some(mut base) = file_cfg {
            // CLI overrides: only override when arg differs from clap default OR was explicitly passed.
            // We detect explicit presence in raw args.
            let has = |flag: &str| raw.iter().any(|a| a == flag || a.starts_with(&format!("{}=", flag)));
            if has("--name") {
                base.name = cli.name;
            }
            if has("--bind") {
                base.bind = cli.bind;
            }
            if has("--port") {
                base.port = cli.port;
            }
            if has("--discovery-port") {
                base.discovery_port = cli.discovery_port;
            }
            if has("--admin-port") {
                base.admin_port = cli.admin_port;
            }
            if has("--password") {
                base.password = cli.password;
            }
            if has("--admin-password") {
                base.admin_password = cli.admin_password;
            }
            if has("--max-players") {
                base.max_players = cli.max_players;
            }
            if has("--tick-rate") {
                base.tick_rate = cli.tick_rate;
            }
            if has("--snapshot-rate") {
                base.snapshot_rate = cli.snapshot_rate;
            }
            if has("--game-mode") {
                base.game_mode = cli.game_mode;
            }
            if has("--periods") {
                base.periods = cli.periods;
            }
            if has("--period-secs") {
                base.period_secs = cli.period_secs;
            }
            if has("--intermission-secs") {
                base.intermission_secs = cli.intermission_secs;
            }
            if has("--faceoff-secs") {
                base.faceoff_secs = cli.faceoff_secs;
            }
            if has("--overtime") {
                base.overtime = cli.overtime;
            }
            if has("--bots") {
                base.bots = cli.bots;
            }
            if has("--puck-friction") {
                base.puck_friction = cli.puck_friction;
            }
            if has("--restitution") {
                base.restitution = cli.restitution;
            }
            if has("--data-dir") {
                base.data_dir = cli.data_dir;
            }
            base.config = cli.config;
            base.validate()?;
            Ok(base)
        } else {
            cli.validate()?;
            Ok(cli)
        }
    }

    pub fn validate(&self) -> anyhow::Result<()> {
        if !(1..=16).contains(&self.max_players) {
            anyhow::bail!("max-players must be 1..16");
        }
        if !(30..=120).contains(&self.tick_rate) {
            anyhow::bail!("tick-rate must be 30..120");
        }
        if self.snapshot_rate == 0 || self.snapshot_rate > self.tick_rate {
            anyhow::bail!("snapshot-rate must be 1..=tick-rate");
        }
        if !(1..=5).contains(&self.periods) {
            anyhow::bail!("periods must be 1..5");
        }
        if !(60..=1200).contains(&self.period_secs) {
            anyhow::bail!("period-secs must be 60..1200");
        }
        match self.game_mode.as_str() {
            "standard" | "competitive" | "public" => {}
            _ => anyhow::bail!("game-mode must be standard|competitive|public"),
        }
        Ok(())
    }

    pub fn example_json() -> String {
        let ex = Config::default();
        serde_json::to_string_pretty(&ex).unwrap_or_default()
    }
}
