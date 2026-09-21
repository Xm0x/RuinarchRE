#!/usr/bin/env bash
# Central path config for the RuinarchRE project.
# Source this from other scripts: `source "$(dirname "$0")/env.sh"`

# --- Game install (source of truth, never committed) ---
export RUIN_GAME_DIR="/home/user/.local/share/Steam/steamapps/common/Ruinarch"
export RUIN_DATA_DIR="$RUIN_GAME_DIR/Ruinarch_Data"
export RUIN_MANAGED_DIR="$RUIN_DATA_DIR/Managed"

# --- Toolchain ---
export GHIDRA_DIR="/home/user/Desktop/Apps/ghidra_12.1.3_PUBLIC"
export PATH="$PATH:$HOME/.dotnet/tools"   # ilspycmd

# --- Project layout ---
export RUIN_PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export RUIN_SRC_DIR="$RUIN_PROJECT_DIR/src"
export RUIN_REF_DIR="$RUIN_PROJECT_DIR/reference"

# --- Dev drop-in target (safe copy; never the real install by default) ---
export RUIN_TESTGAME_DIR="$RUIN_PROJECT_DIR/testgame"
export RUIN_PROTON_DIR="$RUIN_GAME_DIR/../Proton 10.0"
export RUIN_STEAM_DIR="$HOME/.local/share/Steam"
export RUIN_APPID="909320"

# The two assemblies that contain all game logic.
export RUIN_GAME_ASSEMBLIES=(
  "Assembly-CSharp.dll"
  "Assembly-CSharp-firstpass.dll"
)
