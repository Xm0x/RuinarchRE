#!/usr/bin/env bash
# Extract Ruinarch's assets from YOUR OWN install using AssetRipper, driving its
# headless web API. Output goes to reference-assets/ (gitignored). Like
# decompile.sh, this reproduces content locally and never commits game assets.
#
# Usage:
#   tools/extract-assets.sh [primary|project] [output-dir]
#     primary  (default) - assets + decompiled Scripts (raw content tree)
#     project            - a reconstructable Unity project
#
# Requires AssetRipper (the free GUI build ships a headless mode + web API):
#   set ASSETRIPPER to the executable, or drop it at the default path below.
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/env.sh"

MODE="${1:-primary}"
OUT="${2:-$RUIN_PROJECT_DIR/reference-assets}"
ASSETRIPPER="${ASSETRIPPER:-$HOME/Desktop/Apps/AssetRipper/AssetRipper.GUI.Free}"
PORT="${ASSETRIPPER_PORT:-17654}"
BASE="http://127.0.0.1:$PORT"

case "$MODE" in
	primary) ENDPOINT="/Export/PrimaryContent" ;;
	project) ENDPOINT="/Export/UnityProject" ;;
	*) echo "unknown mode '$MODE' (use: primary | project)" >&2; exit 2 ;;
esac

[ -x "$ASSETRIPPER" ] || { echo "AssetRipper not found at: $ASSETRIPPER
Set ASSETRIPPER to the AssetRipper.GUI.Free executable (https://assetripper.github.io/)." >&2; exit 1; }
[ -d "$RUIN_GAME_DIR" ] || { echo "game dir not found: $RUIN_GAME_DIR (set RUIN_GAME_DIR)" >&2; exit 1; }

mkdir -p "$OUT"
echo ">>> Launching AssetRipper (headless) on port $PORT"
"$ASSETRIPPER" --headless --port "$PORT" >/dev/null 2>&1 &
AR_PID=$!
trap 'kill "$AR_PID" 2>/dev/null || true' EXIT

# Wait for the web server.
for i in $(seq 1 30); do
	curl -fsS "$BASE/" >/dev/null 2>&1 && break
	sleep 1
	[ "$i" = 30 ] && { echo "AssetRipper did not start" >&2; exit 1; }
done

echo ">>> Loading game: $RUIN_GAME_DIR"
curl -fsS -X POST "$BASE/LoadFolder" --data-urlencode "path=$RUIN_GAME_DIR" >/dev/null

echo ">>> Exporting ($MODE) -> $OUT  (this can take a few minutes)"
curl -fsS -X POST "$BASE$ENDPOINT" --data-urlencode "path=$OUT" >/dev/null

echo ">>> Done. $(find "$OUT" -type f | wc -l) files, $(du -sh "$OUT" | cut -f1) in $OUT"
echo "    (reference-assets/ is gitignored; never commit extracted assets.)"
