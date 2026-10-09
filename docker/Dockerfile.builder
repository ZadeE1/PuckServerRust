# Builder image: produces a complete Linux dedicated-server bundle at /out/server.
# Dedicated server ONLY (Steam tool 3481440). Never touches RealServerForBasis/
# (Windows) or decompiledEdition/ (read-only).
#
# Build (Linux x86_64 host, repo root as context):
#   docker build -f docker/Dockerfile.builder -t puck-builder .
#   docker build -f docker/Dockerfile.builder -t puck-builder \
#     --build-arg STEAM_USER=<user> --build-arg STEAM_PASSWORD=<pass> .
# (anonymous download is tried by default; if 3481440 denies it, pass a Steam login)
# Inspect output without running the game:
#   docker create --name puck-out puck-builder
#   docker cp puck-out:/out/server ./puck-linux
#   docker rm puck-out

# ---- Stage 1: fetch official Linux dedicated-server player (downloaded, never built)
# DepotDownloader ships a self-contained 64-bit binary (no dotnet needed) and
# works under amd64 emulation, unlike steamcmd's 32-bit bootstrap.
# Official Linux layout (Steam tool 3481440, depot 3481441): executable is
# `Puck` (ELF, no .x86_64 suffix), player `UnityPlayer.so`, plugins flat in
# `Puck_Data/Plugins/*.so`, Mono in `Puck_Data/MonoBleedingEdge/x86_64/`.
# Launch upstream is `./start_server.sh` (= LD_LIBRARY_PATH=CWD, exec Puck).
FROM ubuntu:22.04 AS fetcher
ARG STEAM_USER=""
ARG STEAM_PASSWORD=""
ARG DEDICATED_APP_ID=3481440
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update && apt-get install -y --no-install-recommends \
      ca-certificates curl unzip \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /dd /server \
    && curl -sqL "https://github.com/SteamRE/DepotDownloader/releases/latest/download/DepotDownloader-linux-x64.zip" -o /tmp/dd.zip \
    && unzip -q /tmp/dd.zip -d /dd && rm /tmp/dd.zip \
    && test -x /dd/DepotDownloader \
    && if [ -n "$STEAM_USER" ]; then \
         /dd/DepotDownloader -app "$DEDICATED_APP_ID" -os linux -osarch 64 \
           -dir /server -username "$STEAM_USER" -password "$STEAM_PASSWORD"; \
       else \
         /dd/DepotDownloader -app "$DEDICATED_APP_ID" -os linux -osarch 64 \
           -dir /server; \
       fi \
    && test -f /server/Puck \
    && test -f /server/UnityPlayer.so \
    && chmod +x /server/Puck

# ---- Stage 2: build managed Puck.dll (IL, OS-agnostic) against the LINUX depot's refs
# Puck.csproj HintPaths point at ../RealServerForBasis/Puck_Data/Managed/,
# so we recreate that relative layout from the Linux depot (correct refs for Linux).
# Puck.csproj pins LangVersion 14.0 (C# 14), which needs the .NET 10 SDK.
# Target stays netstandard2.1, so the IL still runs on the Unity Mono player.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS managed
COPY RustEdition/*.cs RustEdition/*.csproj /src/RustEdition/
COPY RustEdition/Properties/ /src/RustEdition/Properties/
COPY RustEdition/__GEN/ /src/RustEdition/__GEN/
COPY RustEdition/UnityEngine.InputSystem.Composites/ /src/RustEdition/UnityEngine.InputSystem.Composites/
COPY RustEdition/UI/ /src/RustEdition/UI/
COPY --from=fetcher /server/Puck_Data/Managed/ /src/RealServerForBasis/Puck_Data/Managed/
WORKDIR /src/RustEdition
# BundlePlayer=false: skip the Windows-only post-build copy (Puck.exe,
# UnityPlayer.dll, D3D12/, audio_curve.dll); this builder assembles its own
# Linux bundle from the official Linux player in the final stage.
RUN dotnet build Puck.csproj -c Release --nologo -p:BundlePlayer=false \
    && test -f bin/Release/netstandard2.1/Puck.dll

# ---- Stage 3: build native plugin as Linux .so (must be Linux x86_64)
FROM rust:1-bookworm AS native
COPY RustEdition/native/audio_curve/ /src/audio_curve/
WORKDIR /src/audio_curve
RUN cargo build --release \
    && test -f target/release/libaudio_curve.so

# ---- Stage 4: assemble complete bundle at /out/server (this image IS the builder output)
FROM ubuntu:22.04 AS builder-out
COPY --from=fetcher /server/ /out/server/
# Transplant 1: managed game logic (only file overwritten in Managed/)
COPY --from=managed \
  /src/RustEdition/bin/Release/netstandard2.1/Puck.dll \
  /out/server/Puck_Data/Managed/Puck.dll
# Transplant 2: native plugin (both probe paths; DllImport("audio_curve") -> libaudio_curve.so)
COPY --from=native \
  /src/audio_curve/target/release/libaudio_curve.so \
  /out/server/libaudio_curve.so
RUN mkdir -p /out/server/Puck_Data/Plugins \
    && cp /out/server/libaudio_curve.so /out/server/Puck_Data/Plugins/libaudio_curve.so \
    && chmod +x /out/server/Puck \
    && test -f /out/server/Puck \
    && test -f /out/server/UnityPlayer.so \
    && test -f /out/server/Puck_Data/Managed/Puck.dll \
    && test -f /out/server/libaudio_curve.so
