#!/usr/bin/env bash
# Build a game assembly directly with Roslyn csc, referencing ONLY the game's
# own Managed/ DLLs (as Unity did). Avoids the .NET SDK net40 targeting pack,
# whose reference-assembly mscorlib/System.Core conflict with Unity's and break
# ExtensionAttribute resolution.
#
# Usage: tools/build.sh [Assembly-CSharp|Assembly-CSharp-firstpass]   (default: Assembly-CSharp)
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/env.sh"

target="${1:-Assembly-CSharp}"
srcdir="$RUIN_SRC_DIR/$target"
outdir="$RUIN_PROJECT_DIR/build"
[ -d "$srcdir" ] || { echo "No src/$target — run tools/seed-src.sh" >&2; exit 1; }
mkdir -p "$outdir"

# Locate Roslyn csc from the installed SDK.
CSC="$(ls -1 /usr/lib64/dotnet/sdk/*/Roslyn/bincore/csc.dll 2>/dev/null | sort -V | tail -1)"
[ -n "$CSC" ] || CSC="$(ls -1 "$(dirname "$(command -v dotnet)")"/sdk/*/Roslyn/bincore/csc.dll 2>/dev/null | sort -V | tail -1)"
[ -n "$CSC" ] || { echo "csc.dll not found under the dotnet SDK" >&2; exit 1; }

rsp="$outdir/$target.rsp"
{
  echo "-nostdlib"
  echo "-noconfig"
  echo "-unsafe"
  echo "-target:library"
  echo "-langversion:latest"
  echo "-nowarn:0169,0649,0067,0414,0219,0162,0168,0184,0472,0693,1701,1702"
  echo "-out:$outdir/$target.dll"
  # Reference every Managed DLL except the assembly we're building.
  for dll in "$RUIN_MANAGED_DIR"/*.dll; do
    base="$(basename "$dll" .dll)"
    [ "$base" = "$target" ] && continue
    echo "-r:$dll"
  done
  # All decompiled sources for this assembly.
  find "$srcdir" -name '*.cs' -print
} > "$rsp"

echo ">>> Building $target ($(grep -c '^-r:' "$rsp") refs, $(grep -c '\.cs$' "$rsp") sources)"
dotnet exec "$CSC" "@$rsp"
echo "OK -> build/$target.dll"
