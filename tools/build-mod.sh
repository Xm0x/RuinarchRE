#!/usr/bin/env bash
# Compile a mod against the game's own assemblies + our modding API + Harmony,
# and drop it into a game copy's Mods/ folder ready to run.
#
# Usage:
#   tools/build-mod.sh examples/ExampleMod            -> into testgame/Mods/
#   tools/build-mod.sh examples/ExampleMod <game-dir> -> into <game-dir>/Mods/
#
# References:
#   - the game's Managed/*.dll  (minus the stock Assembly-CSharp.dll)
#   - build/Assembly-CSharp.dll (OUR build - it contains the modding API)
#   - lib/0Harmony.dll          (runtime patching)
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/env.sh"

moddir="${1:?usage: build-mod.sh <mod-source-dir> [game-dir]}"
target_dir="${2:-$RUIN_TESTGAME_DIR}"
[ -d "$moddir" ] || { echo "no such mod dir: $moddir" >&2; exit 1; }
name="$(basename "$moddir")"

gamecs="$RUIN_PROJECT_DIR/build/Assembly-CSharp.dll"
harmony="$RUIN_PROJECT_DIR/lib/0Harmony.dll"
[ -f "$gamecs" ]  || { echo "missing build/Assembly-CSharp.dll - run tools/build.sh first" >&2; exit 1; }
[ -f "$harmony" ] || { echo "missing lib/0Harmony.dll - run tools/get-harmony.sh" >&2; exit 1; }

managed_root="$target_dir/Ruinarch_Data/Managed"
[ -d "$managed_root" ] || { echo "not a Ruinarch dir: $target_dir" >&2; exit 1; }

CSC="$(ls -1 /usr/lib64/dotnet/sdk/*/Roslyn/bincore/csc.dll 2>/dev/null | sort -V | tail -1)"
[ -n "$CSC" ] || CSC="$(ls -1 "$(dirname "$(command -v dotnet)")"/sdk/*/Roslyn/bincore/csc.dll 2>/dev/null | sort -V | tail -1)"
[ -n "$CSC" ] || { echo "csc.dll not found under the dotnet SDK" >&2; exit 1; }

modsroot="$target_dir/Mods"
out="$modsroot/$name"
mkdir -p "$out"

rsp="$out/$name.rsp"
{
  echo "-nostdlib"
  echo "-noconfig"
  echo "-target:library"
  echo "-langversion:latest"
  echo "-nowarn:0169,0649,0067,0414,0219,0162,0168,1701,1702"
  echo "-out:$out/$name.dll"
  # Game base libraries + engine, minus the stock Assembly-CSharp (we use ours).
  for dll in "$managed_root"/*.dll; do
    [ "$(basename "$dll")" = "Assembly-CSharp.dll" ] && continue
    echo "-r:$dll"
  done
  echo "-r:$gamecs"
  echo "-r:$harmony"
  find "$moddir" -name '*.cs' -print
} > "$rsp"

echo ">>> Building mod '$name'"
dotnet exec "$CSC" "@$rsp"

# Ship the manifest next to the DLL and the shared Harmony lib in Mods/.
[ -f "$moddir/mod.json" ] && cp -f "$moddir/mod.json" "$out/mod.json"
cp -f "$harmony" "$modsroot/0Harmony.dll"

echo "OK -> $out/$name.dll"
echo "     $modsroot/0Harmony.dll"
