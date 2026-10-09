# PuckServerRust

Rebuildable Puck dedicated server: decompiled C# game logic (`Puck.dll`),
Rust native plugins, and a Linux builder. The stock Windows install and the
decompilation are kept read-only; all work happens in `RustEdition/`.

## Layout

| Path | Purpose |
|---|---|
| `RealServerForBasis/` | Original Windows dedicated-server install. Reference only, do not edit. |
| `decompiledEdition/` | `ilspycmd` output for `Puck.dll`, fixed up to build with 0 errors. Regenerable, do not hand-edit. |
| `RustEdition/` | Conversion workspace, mirrored from `decompiledEdition/`. All code work happens here. |
| `docker/` | Linux dedicated-server builder (`Dockerfile.builder`) plus the copy-to-target scripts (`build.sh`, `run.sh`). |
| `vm/` | Lima test VM on a Windows host (`puck.yaml`, `deploy.ps1`, `puck@.service`, `README.md`). |

## Requirements

- Linux target: Docker with `linux/amd64` support (builder fetches Steam tool `3481440`, anonymous).
- Local Windows builds: .NET 10 SDK (`LangVersion 14.0` fails on older SDKs).
- Native plugin: Rust toolchain (`RustEdition/native/audio_curve/`).

## Quickstart — Linux target

Below, `your-server` is your target host (SSH). The bundle is built on your
machine, copied over, and run there.

```bash
# 1. Build the bundle (Mac or any Docker host)
./docker/build.sh ./puck-server

# 2. Copy it to the target — scp only
scp -r ./puck-server your-server:~/puck-server

# 3. Run it on the target
ssh your-server 'cd ~/puck-server && nohup ./run.sh > server.log 2>&1 < /dev/null & echo started'

# 4. Verify it booted (wait ~60s, then look for the ready line)
ssh your-server 'grep "ready to accept clients" ~/puck-server/server.log'
# Puck B1235 is ready to accept clients on <ip>:25565!
```

Edit `puck-server/server/server_config.json` before copying (or on the target
before launching) to set `port`, `name`, `password`, `maxPlayers`, `tickRate`.
`isPublic: false` keeps it unlisted.

Stop it (note: the player ignores SIGTERM, use SIGKILL):

```bash
ssh your-server 'pkill -9 -f puck-server/server/Puck'
```

Ports: game `port` from the config (default `25565`, TCP+UDP) plus UDP
`7777`/`7778`. Open them on the target firewall.

## Quickstart — Windows target

No Docker needed. `RustEdition/Puck.csproj` already bundles a runnable Windows
folder on every `dotnet build` (the `BundlePlayer` step):

```bash
# 1. Managed game logic (any OS with .NET 10 SDK)
dotnet build RustEdition/Puck.csproj -c Release

# 2. Native plugin — on the Windows machine (or skip: the server falls back
#    to managed code without it, minus the audio-curve speedup)
cd RustEdition/native/audio_curve && cargo build --release

# 3. Transplant 1–2 files onto a Windows dedicated-server install
#    (Steam app_update 3481440, or the BundlePlayer output folder):
#      Puck.dll          -> Puck_Data/Managed/Puck.dll
#      audio_curve.dll   -> next to Puck.exe
```

Run on the target:

```powershell
.\Puck.exe -batchmode -nographics
```

## How it works

`Puck.exe`/`Puck` is just the Unity bootstrapper (Unity `6000.3.14f1`, Mono).
Game logic is the IL assembly `Puck_Data/Managed/Puck.dll`, which is
OS-agnostic — the same build runs on both players. Going cross-platform only
means swapping the native shell around it: Linux needs `Puck` + `UnityPlayer.so`
+ `libaudio_curve.so` (from Steam tool `3481440` + this repo's builds), never a
recompiled exe. The Linux builder passes `-p:BundlePlayer=false` to skip the
Windows-only copy step; local builds keep the default.

## Conventions

- Profiler is opt-in: run with `PUCK_PROFILE=1`, output `profiler.jsonl`.
- Build native (`cargo build --release`) before `dotnet build` locally.
- A change is done when the rebuild succeeds and the server boots to
  `ready to accept clients` — see `AGENTS.md` for the binding work contracts.
