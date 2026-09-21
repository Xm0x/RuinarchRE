#!/usr/bin/env bash
# Fetch Lib.Harmony (0Harmony.dll) from NuGet into lib/. Harmony is MIT-licensed
# runtime-patching library used by mods. Run once; the DLL is small and can be
# committed so the repo is self-contained for modders.
set -euo pipefail
source "$(dirname "${BASH_SOURCE[0]}")/env.sh"

VER="${1:-2.2.2}"
dest="$RUIN_PROJECT_DIR/lib/0Harmony.dll"
mkdir -p "$RUIN_PROJECT_DIR/lib"

tmp="$(mktemp -d)"
trap 'rm -rf "$tmp"' EXIT
cat > "$tmp/h.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Lib.Harmony" Version="$VER" />
  </ItemGroup>
</Project>
EOF

echo ">>> Restoring Lib.Harmony $VER ..."
dotnet restore "$tmp/h.csproj" --packages "$tmp/pkgs" >/dev/null

src="$(find "$tmp/pkgs/lib.harmony/$VER/lib/net472" -iname '0Harmony.dll' | head -1)"
[ -n "$src" ] || src="$(find "$tmp/pkgs/lib.harmony/$VER/lib" -iname '0Harmony.dll' | head -1)"
[ -n "$src" ] || { echo "0Harmony.dll not found in package" >&2; exit 1; }

cp -f "$src" "$dest"
echo "OK -> lib/0Harmony.dll ($(du -h "$dest" | cut -f1), Harmony $VER)"
