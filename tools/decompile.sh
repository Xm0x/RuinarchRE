#!/usr/bin/env bash
# Reproducible raw decompile of Ruinarch's managed game code.
#
# Produces a whole-project (.csproj + per-type .cs, namespace-nested) export
# under reference/ for each game assembly. reference/ is gitignored: it is the
# untouched baseline we diff our cleaned src/ tree against.
#
# Requires: ilspycmd (dotnet tool). Run tools/setup.sh once if missing.
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/env.sh"

if ! command -v ilspycmd >/dev/null 2>&1; then
  echo "ilspycmd not found. Install with: dotnet tool install -g ilspycmd" >&2
  exit 1
fi

mkdir -p "$RUIN_REF_DIR"

for asm in "${RUIN_GAME_ASSEMBLIES[@]}"; do
  name="${asm%.dll}"
  out="$RUIN_REF_DIR/$name"
  src="$RUIN_MANAGED_DIR/$asm"
  echo ">>> Decompiling $asm -> reference/$name"
  rm -rf "$out"
  mkdir -p "$out"
  ilspycmd "$src" \
    -p \
    --nested-directories \
    -o "$out" \
    -r "$RUIN_MANAGED_DIR" \
    2> "$out/.decompile.log" || {
      echo "!!! ilspycmd reported issues for $asm (see $out/.decompile.log)" >&2
    }
  echo "    types: $(find "$out" -name '*.cs' | wc -l) files"
done

echo "Done. Raw baseline in reference/. Seed src/ with: tools/seed-src.sh"
