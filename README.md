# RuinarchRE

Ruinarch's managed game logic, decompiled into readable, buildable C#. Use this
repository to trace character AI, world state and save/load behavior before
writing a mod.

[Systems](docs/systems/README.md) · [Architecture](docs/ARCHITECTURE.md) · [Runtime mods](https://github.com/Xm0x/RuinarchModLoader)

## Start with the behavior

| Investigating | Read first |
|---|---|
| Time, signals or startup | [Core loop](docs/systems/CORE_LOOP.md) |
| A character's choice or interrupted job | [Character AI and jobs](docs/systems/CHARACTER_AI_JOBS.md) |
| Tiles, buildings, settlements or factions | [World model](docs/systems/WORLD_MODEL.md) |
| A save that loads differently | [Save format](docs/systems/SAVE_FORMAT.md) |
| Prefabs, sprites or scene data | [Assets](docs/systems/ASSETS.md) |

Each guide maps the system to real decompiled types under `src/Assembly-CSharp/`. Names of types, methods, fields and namespaces survive in the original metadata; local variable names often remain decompiler-generated.

## What is restored

The documented baseline contains **4,980 code types**, a compilable source tree and major-system documentation. Its recorded milestones include a rebuilt DLL reaching the main menu and a played behavior-verification scenario.

That is the project's defined code-restoration scope, **not proof of every possible gameplay path**. Art, audio and serialized content live in the game's `.assets` files and are not part of the recovered C# tree. See the [milestone definitions](docs/ARCHITECTURE.md) for the exact meaning of completion.

## Work from your own installation

Requires your own Ruinarch installation and the toolchain described in [Architecture](docs/ARCHITECTURE.md). Override the install path when needed:

```bash
export RUIN_GAME_DIR="/path/to/Ruinarch"
tools/setup.sh        # install/check ilspycmd
tools/decompile.sh    # regenerate the raw reference/ baseline
tools/seed-src.sh     # seed src/ only where it does not already exist
tools/build.sh       # compile Assembly-CSharp into build/
```

`tools/env.sh` resolves paths. For IDE use, `tools/fix-csproj-paths.sh` connects the project files to your local `Managed/` directory. Keep your restored-source changes in `src/`; `reference/` is the regenerable, gitignored baseline.

| Path | Purpose |
|---|---|
| `src/` | Organized, buildable game source |
| `docs/` | Architecture, workflow and system guides |
| `tools/` | Decompile, build and verification scripts |
| `reference/` | Raw local ILSpy export |
| `tools/extract-assets.sh` | Optional extraction from your own game files |

<details>
<summary>Recorded restoration milestones</summary>

## Status

- [x] M0: reproducible raw baseline export
- [x] M1: `src/` compiles to DLLs, 0 errors *(build via `tools/build.sh`)*
- [x] M2: drop-in DLLs boot the game *(our build runs to main menu)*
- [x] M3: behavior-verified across a full scenario *(playtested, no issues)*
- [x] M4: all major systems documented ([docs/systems/](docs/systems/))
- [x] M5: structured control flow, edit/build/run dev loop ([docs/MODDING.md](docs/MODDING.md))
- [x] M6: mod loader shipped as its own project ([RuinarchModLoader](https://github.com/Xm0x/RuinarchModLoader))

</details>

## Runtime modding belongs next door

Use **[RuinarchModLoader](https://github.com/Xm0x/RuinarchModLoader)** to install the loader startup call into your own game assembly, then apply Harmony patches at runtime. **[RuinarchMods](https://github.com/Xm0x/RuinarchMods)** contains the gameplay packages. Their normal deployment uses the stock game, not a replacement rebuilt game DLL.

This repository stays focused on source restoration and serves as the reference for choosing patch targets. The separate [restoration edit/build/run workflow](docs/MODDING.md) documents source-development experiments.

## Project notes

AI assistance was used in this project. Ruinarch's original code and assets remain the property of their respective owners; this is an independent reverse-engineering project, not an official source release.
