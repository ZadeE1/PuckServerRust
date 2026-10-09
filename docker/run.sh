#!/bin/bash
# Run the Puck Linux dedicated server on the TARGET host.
# This file ships inside the build.sh output folder next to server/.
# Usage:  cd <output-dir> && ./run.sh [extra Unity args...]
# Config: edit server/server_config.json (port, name, password, tickRate...).
# Foreground process: use systemd / tmux / nohup to keep it up. Ctrl-C stops it.
set -euo pipefail

cd "$(dirname "$0")/server"
exec ./start_server.sh -batchmode -nographics "$@"
