# AGENTS.md

## Read-only reference folders — DO NOT EDIT
- `RealServerForBasis/` — original Unity dedicated-server install (player, data, managed DLLs).
  Never modify game files here. Only exception: `server_config.json` when asked.
  Original DLL is backed up at `Puck_Data/Managed/Puck.dll.orig` (untracked).
- `decompiledEdition/` — `ilspycmd` (icsharpcode/ILSpy) output for `Puck.dll`, plus the minimal
  fixes needed to build with 0 errors (HintPaths, `UnityEngine.dll` reference, Netcode
  `__getTypeName`/`__rpc_exec_stage` patterns, `BundlePlayer` target). Regenerable; do not
  hand-edit game logic here. Rebuild: `dotnet build decompiledEdition/Puck.csproj`.

## Work here
- `RustEdition/` — Rust conversion workspace, mirrored from `decompiledEdition/` (sources only,
  no `bin/`/`obj/`). All conversion work happens here.
- `docker/` — Linux dedicated-server builder only (`docker/Dockerfile.builder`,
  Steam tool `3481440`). Builder fetches the official Linux player and rebuilds
  `Puck.dll` (IL) + `libaudio_curve.so` into `/out/server`.
  Copy-to-target flow: `docker/build.sh [dir]` (any Docker host) produces
  `<dir>/server/` + `<dir>/run.sh`; copy `<dir>` to the target and run `./run.sh`.
  Never bakes in Windows player files.

## Work in progress (parked, not abandoned)
- Sync-pipeline perf (`SynchronizedObject*`, 33 types): profiler freezes live play when these
  are instrumented; `PUCK_PROFILE_SKIP` prefix exclusions bisect it (`*` = writer only).
  Status: `SynchronizedObject`-excluded plays fine; full set freezes. Next: split the 33
  into halves and play-test each. Compression cache for unmoved objects is implemented
  (RustEdition) but unverified live for the same reason.

## Conventions
- Profiler (opt-in): run with `PUCK_PROFILE=1`, output `profiler.jsonl` (JSON lines, 1/sec).
- Native plugins: `cargo build --release` in `RustEdition/native/audio_curve/` BEFORE
  `dotnet build` (the csproj copies the cdylib into the bundle, it does not build it).
- Linux docker builder passes `-p:BundlePlayer=false` (`RustEdition/Puck.csproj:116`); local
  Windows builds keep the default bundling behavior.
- Verify by execution: rebuild must succeed and the bundled server must boot to
  "ready to accept clients" before any commit.
