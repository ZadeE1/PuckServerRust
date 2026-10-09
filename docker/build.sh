#!/bin/bash
# Build a copy-to-target Linux dedicated-server folder.
# Run on macOS or any Docker host. No Steam/Game files needed locally.
#
# Usage:
#   ./docker/build.sh [output-dir]        # default: ./puck-linux-server
#   PLATFORM=linux/amd64 ./docker/build.sh    # target arch (arm Mac -> amd64 node)
#   BUILDER_IMAGE=myreg/puck-builder:x ./docker/build.sh
#   STEAM_USER=u STEAM_PASSWORD=p ./docker/build.sh   # only if anon depot fetch fails
#
# Result:
#   <output-dir>/server/          complete runnable bundle (Puck, UnityPlayer.so, Puck_Data, ...)
#   <output-dir>/server/server_config.json   created from RealServerForBasis default if depot lacks one
#   <output-dir>/run.sh           copy of docker/run.sh — run it ON the target host
#
# Deploy: copy the whole output dir to the target (scp -r / rsync / usb),
# then on the target:  cd <output-dir> && ./run.sh
set -euo pipefail

PLATFORM="${PLATFORM:-linux/amd64}"
BUILDER_IMAGE="${BUILDER_IMAGE:-puck-builder:latest}"
OUT="${1:-./puck-linux-server}"

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$REPO_ROOT"

BUILD_ARGS=()
if [ -n "${STEAM_USER:-}" ]; then BUILD_ARGS+=(--build-arg "STEAM_USER=$STEAM_USER"); fi
if [ -n "${STEAM_PASSWORD:-}" ]; then BUILD_ARGS+=(--build-arg "STEAM_PASSWORD=$STEAM_PASSWORD"); fi

echo "==> building $BUILDER_IMAGE for $PLATFORM"
if [ "${#BUILD_ARGS[@]}" -eq 0 ]; then
  docker build --platform "$PLATFORM" -f docker/Dockerfile.builder \
    -t "$BUILDER_IMAGE" .
else
  docker build --platform "$PLATFORM" -f docker/Dockerfile.builder \
    -t "$BUILDER_IMAGE" "${BUILD_ARGS[@]}" .
fi

echo "==> extracting bundle to $OUT/server"
rm -rf "$OUT/server"
mkdir -p "$OUT"
CID="$(docker create --platform "$PLATFORM" "$BUILDER_IMAGE")"
docker cp "$CID:/out/server" "$OUT/server"
docker rm "$CID" > /dev/null

if [ ! -f "$OUT/server/server_config.json" ]; then
  echo "==> depot ships no server_config.json; seeding default"
  cp RealServerForBasis/server_config.json "$OUT/server/server_config.json"
fi

cp docker/run.sh "$OUT/run.sh"
chmod +x "$OUT/run.sh" "$OUT/server/Puck" "$OUT/server/start_server.sh"

echo "==> verifying bundle"
test -f "$OUT/server/Puck"
test -f "$OUT/server/UnityPlayer.so"
test -f "$OUT/server/Puck_Data/Managed/Puck.dll"
test -f "$OUT/server/libaudio_curve.so"
test -f "$OUT/server/server_config.json"

echo "OK: $OUT/server is ready."
echo "Copy '$OUT' to the target host, then run './run.sh' there."
