//! File persistence: bans.json, stats.json, example config.
//! JSON only (no native deps) so Mac + Linux + Windows builds stay trivial.

use crate::game::SkaterStats;
use std::collections::{HashMap, HashSet};
use std::net::IpAddr;
use std::path::{Path, PathBuf};

pub fn data_file(data_dir: &Path, name: &str) -> PathBuf {
    data_dir.join(name)
}

pub fn load_bans(data_dir: &Path) -> HashSet<IpAddr> {
    let p = data_file(data_dir, "bans.json");
    let Ok(data) = std::fs::read_to_string(&p) else {
        return HashSet::new();
    };
    let v: Vec<String> = serde_json::from_str(&data).unwrap_or_default();
    v.into_iter().filter_map(|s| s.parse().ok()).collect()
}

pub fn save_bans(data_dir: &Path, bans: &HashSet<IpAddr>) -> anyhow::Result<()> {
    std::fs::create_dir_all(data_dir)?;
    let v: Vec<String> = bans.iter().map(|ip| ip.to_string()).collect();
    std::fs::write(data_file(data_dir, "bans.json"), serde_json::to_string_pretty(&v)?)?;
    Ok(())
}

pub fn load_totals(data_dir: &Path) -> HashMap<String, SkaterStats> {
    let p = data_file(data_dir, "stats.json");
    let Ok(data) = std::fs::read_to_string(&p) else {
        return HashMap::new();
    };
    serde_json::from_str(&data).unwrap_or_default()
}

pub fn save_totals(data_dir: &Path, totals: &HashMap<String, SkaterStats>) -> anyhow::Result<()> {
    std::fs::create_dir_all(data_dir)?;
    std::fs::write(
        data_file(data_dir, "stats.json"),
        serde_json::to_string_pretty(totals)?,
    )?;
    Ok(())
}

pub fn write_example_config(data_dir: &Path) -> anyhow::Result<PathBuf> {
    std::fs::create_dir_all(data_dir)?;
    let p = data_file(data_dir, "puck-server.example.json");
    if !p.exists() {
        std::fs::write(&p, crate::config::Config::example_json())?;
    }
    Ok(p)
}
