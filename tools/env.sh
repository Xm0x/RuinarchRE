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

# The two assemblies that contain all game logic.
export RUIN_GAME_ASSEMBLIES=(
  "Assembly-CSharp.dll"
  "Assembly-CSharp-firstpass.dll"
)
