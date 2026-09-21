#!/usr/bin/env bash
# First-time seed of the working src/ tree from the raw reference/ baseline.
# Safe: refuses to overwrite an existing src/<assembly> (cleanup work lives there).
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/env.sh"

mkdir -p "$RUIN_SRC_DIR"
for asm in "${RUIN_GAME_ASSEMBLIES[@]}"; do
  name="${asm%.dll}"
  ref="$RUIN_REF_DIR/$name"
  dst="$RUIN_SRC_DIR/$name"
  if [ ! -d "$ref" ]; then
    echo "SKIP $name: no baseline. Run tools/decompile.sh first." >&2
    continue
  fi
  if [ -d "$dst" ]; then
    echo "SKIP $name: src/$name already exists (not clobbering your work)." >&2
    continue
  fi
  echo ">>> Seeding src/$name from reference/$name"
  cp -r "$ref" "$dst"
  rm -f "$dst/.decompile.log"
done
echo "Done. src/ ready for Track A cleanup."
