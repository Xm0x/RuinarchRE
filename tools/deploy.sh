#!/usr/bin/env bash
# Edit -> build -> run dev loop.
#
# Builds both assemblies from src/ and swaps them into a game copy's Managed/
# dir, keeping a one-time backup of the originals. By default it targets the
# safe testgame/ copy, NEVER your real Steam install, so a broken build can't
# corrupt the game you actually play.
#
# Usage:
#   tools/deploy.sh                 build + swap into testgame/
#   tools/deploy.sh --run           ...then launch it through Proton
#   tools/deploy.sh --target DIR    swap into an arbitrary game dir
#   tools/deploy.sh --install       target the REAL Steam install (asks first)
#   tools/deploy.sh --restore       put the original DLLs back, remove ours
#
# Only Assembly-CSharp[.-firstpass].dll are touched; everything else is the
# stock game copy.
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/env.sh"

target_dir="$RUIN_TESTGAME_DIR"
do_run=0
do_restore=0

while [ $# -gt 0 ]; do
  case "$1" in
    --run)     do_run=1 ;;
    --restore) do_restore=1 ;;
    --target)  target_dir="$2"; shift ;;
    --install)
      target_dir="$RUIN_GAME_DIR"
      echo "!!! Targeting your REAL install: $RUIN_GAME_DIR"
      read -rp "    Type 'yes' to continue: " ok
      [ "$ok" = "yes" ] || { echo "aborted."; exit 1; }
      ;;
    *) echo "unknown arg: $1" >&2; exit 1 ;;
  esac
  shift
done

managed="$target_dir/Ruinarch_Data/Managed"
[ -d "$managed" ] || { echo "Not a Ruinarch dir (no Ruinarch_Data/Managed): $target_dir" >&2; exit 1; }

restore_one() {
  local name="$1" bak="$managed/$1.orig"
  if [ -f "$bak" ]; then
    rm -f "$managed/$name"
    mv "$bak" "$managed/$name"
    echo "  restored $name"
  else
    echo "  no backup for $name (nothing to restore)"
  fi
}

if [ "$do_restore" = 1 ]; then
  echo ">>> Restoring stock DLLs in $managed"
  for a in "${RUIN_GAME_ASSEMBLIES[@]}"; do restore_one "$a"; done
  echo "Done."
  exit 0
fi

# 1. Build both assemblies from src/.
for a in Assembly-CSharp Assembly-CSharp-firstpass; do
  "$RUIN_PROJECT_DIR/tools/build.sh" "$a"
done

# 2. Swap into the target, backing up the originals exactly once.
echo ">>> Deploying to $managed"
for a in "${RUIN_GAME_ASSEMBLIES[@]}"; do
  built="$RUIN_PROJECT_DIR/build/$a"
  [ -f "$built" ] || { echo "missing build/$a" >&2; exit 1; }
  # Back up the stock DLL once (only if the current one isn't already ours).
  if [ ! -f "$managed/$a.orig" ]; then
    cp -f "$managed/$a" "$managed/$a.orig"
    echo "  backed up $a -> $a.orig"
  fi
  # rm first: the copy may be a hardlink into the real install (shared inode).
  rm -f "$managed/$a"
  cp -f "$built" "$managed/$a"
  echo "  deployed $a ($(du -h "$built" | cut -f1))"
done

# 3. Optionally launch through Proton as a standalone (Steamworks-tolerant) run.
if [ "$do_run" = 1 ]; then
  echo ">>> Launching via Proton"
  echo "$RUIN_APPID" > "$target_dir/steam_appid.txt"
  prefix="$RUIN_PROJECT_DIR/testprefix"; mkdir -p "$prefix"
  STEAM_COMPAT_DATA_PATH="$prefix" \
  STEAM_COMPAT_CLIENT_INSTALL_PATH="$RUIN_STEAM_DIR" \
    "$RUIN_PROTON_DIR/proton" run "$target_dir/Ruinarch.exe"
fi

echo "Done."
