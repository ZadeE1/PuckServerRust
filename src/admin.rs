//! TCP admin (RCON) line protocol for LAN ops.
//!
//! Connect: `nc <mac-ip> 7779`, then `auth <admin_password>`.
//! Commands: status | say <msg> | kick <id> | ban <ip> | unban <ip> | stop | help

use tokio::io::{AsyncBufReadExt, AsyncWriteExt, BufReader};
use tokio::net::TcpListener;
use tokio::sync::{mpsc, oneshot};

#[derive(Debug)]
pub struct AdminRequest {
    pub cmd: String,
    pub args: String,
    pub resp: oneshot::Sender<String>,
}

pub fn parse_line(line: &str) -> Option<(String, String)> {
    let t = line.trim();
    if t.is_empty() {
        return None;
    }
    match t.find(' ') {
        None => Some((t.to_lowercase(), String::new())),
        Some(i) => Some((t[..i].to_lowercase(), t[i + 1..].trim().to_string())),
    }
}

pub async fn serve_admin(
    listener: TcpListener,
    admin_password: String,
    tx: mpsc::UnboundedSender<AdminRequest>,
) {
    loop {
        let Ok((stream, peer)) = listener.accept().await else {
            continue;
        };
        let tx = tx.clone();
        let admin_password = admin_password.clone();
        tokio::spawn(async move {
            let (r, mut w) = stream.into_split();
            let mut lines = BufReader::new(r).lines();
            // Auth first.
            let Ok(Some(first)) = lines.next_line().await else {
                return;
            };
            let (cmd, args) = match parse_line(&first) {
                Some(v) => v,
                None => return,
            };
            if cmd != "auth" || args != admin_password {
                let _ = w.write_all(b"ERR bad auth\n").await;
                return;
            }
            let _ = w.write_all(b"OK authed. type help\n").await;
            tracing::info!("admin authed from {}", peer);
            loop {
                let Ok(line) = lines.next_line().await else {
                    return;
                };
                let Some(text) = line else { return };
                let (cmd, args) = match parse_line(&text) {
                    Some(v) => v,
                    None => continue,
                };
                if cmd.is_empty() {
                    continue;
                }
                let (resp_tx, resp_rx) = oneshot::channel();
                let req = AdminRequest { cmd: cmd.clone(), args, resp: resp_tx };
                if tx.send(req).is_err() {
                    let _ = w.write_all(b"ERR server busy\n").await;
                    return;
                }
                match tokio::time::timeout(std::time::Duration::from_secs(5), resp_rx).await {
                    Ok(Ok(msg)) => {
                        let _ = w.write_all(msg.as_bytes()).await;
                        let _ = w.write_all(b"\n").await;
                        if cmd == "stop" {
                            return;
                        }
                    }
                    _ => {
                        let _ = w.write_all(b"ERR timeout\n").await;
                    }
                }
            }
        });
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parse_commands() {
        assert_eq!(
            parse_line("kick 3").unwrap(),
            ("kick".into(), "3".into())
        );
        assert_eq!(parse_line("STATUS").unwrap(), ("status".into(), "".into()));
        assert!(parse_line("   ").is_none());
    }
}
