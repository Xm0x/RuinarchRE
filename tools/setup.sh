#!/usr/bin/env bash
# One-time toolchain setup for RuinarchRE.
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/env.sh"

echo "Checking toolchain..."
command -v dotnet   >/dev/null 2>&1 || { echo "MISSING: dotnet SDK"; exit 1; }
command -v mono     >/dev/null 2>&1 || echo "WARN: mono not found (optional; used for monodis inspection)"

if ! command -v ilspycmd >/dev/null 2>&1; then
  echo "Installing ilspycmd (ICSharpCode.Decompiler CLI)..."
  dotnet tool install -g ilspycmd
fi

echo "OK. ilspycmd: $(ilspycmd --version 2>&1 | head -1)"
echo "Game dir: $RUIN_GAME_DIR"
[ -f "$RUIN_MANAGED_DIR/Assembly-CSharp.dll" ] \
  && echo "Found Assembly-CSharp.dll ($(du -h "$RUIN_MANAGED_DIR/Assembly-CSharp.dll" | cut -f1))" \
  || echo "WARN: Assembly-CSharp.dll not found — check RUIN_GAME_DIR in tools/env.sh"
