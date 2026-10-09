# Linux test VM on a Windows host (Lima)

Run the Linux Puck dedicated server in a real Ubuntu VM on Windows 11 Home,
where Hyper-V is unavailable and WSL2 is too limited (NAT'd networking,
no reliable UDP port forwarding for game traffic, no persistent services).

## Why Lima

- **vs WSL2**: Lima is a real VM (QEMU + WHPX acceleration) with declarative
  TCP **and** UDP port forwarding, a static config file, and systemd services
  that survive reboots. WSL2's mirrored-NAT networking drops game UDP traffic.
- **vs VirtualBox**: lighter, scriptable (`limactl`), config-as-code
  (`puck.yaml`). (A VirtualBox `PuckVM` was tried first and parked.)

## Layout

| File | Purpose |
|---|---|
| `puck.yaml` | VM definition: 1 GiB RAM, 2 CPUs, Ubuntu 24.04 LTS, game port forwards, first-boot provisioning (swap, ufw, systemd unit) |
| `puck@.service` | systemd template that runs the server; `systemctl enable --now puck@$(whoami)` |
| `deploy.ps1` | Host script: `docker/build.sh` → stream bundle into the VM → start service → check the ready line |

## Setup (Windows host)

```powershell
winget install --id SoftwareFreedomConservancy.QEMU -e --accept-source-agreements --accept-package-agreements --silent
winget install --id Lima.Lima -e --accept-source-agreements --accept-package-agreements --silent
```

Add `C:\Program Files\qemu` and the winget `Lima.Lima*` `bin` dir to `PATH`.
Keep the VM data off the system drive — Lima honors `LIMA_HOME` (e.g. `D:\lima`, set as a
User env var; the instance then lives at `<LIMA_HOME>\puck`):

```powershell
[Environment]::SetEnvironmentVariable("LIMA_HOME", "D:\lima", "User")
$env:LIMA_HOME = "D:\lima"
```

## Daily use

```powershell
limactl start ./vm/puck.yaml   # first boot downloads the Ubuntu cloud image + provisions
limactl shell puck             # SSH into the VM
.\vm\deploy.ps1                # rebuild bundle, copy in, (re)start server, verify boot
limactl stop --force puck      # --force: the Unity player ignores SIGTERM and stalls graceful stop
```

Game client on the host connects to `127.0.0.1:25565` (TCP+UDP forwarded;
UDP `7777`/`7778` forward on demand when the guest binds them).
Host TCP check: `(New-Object Net.Sockets.TcpClient).Connect("127.0.0.1", 25565)`.

Config note: `mounts: []` in `puck.yaml` means **no host folders are shared**
into the guest (verified via guest `mount`: only `fusectl`). The guest's home
is `/home/<user>.guest`, not `/home/<user>` — the systemd unit resolves it
via `$HOME` instead of hardcoding.

## Gotchas (all hit during setup)

- `limactl copy` (scp) is broken on Windows hosts — stream files with
  `tar -czf - … | limactl shell puck -- tar -xzf - …` under Git Bash.
- Git Bash mangles absolute guest paths (`/home/…` → `C:\Program Files\Git\…`)
  for native binaries: `export MSYS2_ARG_CONV_EXCL="*"`.
- Guest port `22` can't be in `portForwards` (reserved for Lima's own SSH);
  use `limactl shell` / `limactl show-ssh`.
- `limactl stop --force` can leave `qemu-system-x86_64.exe` / `limactl.exe`
  alive holding the disk and log files — kill leftovers before moving or
  restarting, or the next `start` fails on locked files.
- Force-killing QEMU discards writeback-cached disk writes: after an unclean
  stop, re-apply the systemd unit + bundle files and `sync` before trusting
  the disk. One guest kernel panic (`init` SIGSEGV under WHPX) was observed
  once; the instance has been stable since.
- `docker/*.sh` must stay LF (they run on Linux/WSL): enforced by
  `.gitattributes` (`*.sh text eol=lf`); with `core.autocrlf=true` checkouts
  they otherwise break with `set: pipefail: invalid option`.
- The service needs `LD_LIBRARY_PATH=$PWD` (mirrors `start_server.sh`), or
  `steamclient.so` fails to load and Steam init retries forever.
