# RuinarchRE

DISCLAIMER: For %100 honesty, help of AI was used in this project.

A 100% reverse-engineering and source-restoration project for **Ruinarch**. The
goal is a clean, organized, documented, **recompilable** C# source tree that
rebuilds the game and drops back in.

> **Legal:** this repo ships no game binaries. It contains only reverse-
> engineering scripts, our own reconstructed and annotated source, and docs.
> You must own a legitimate copy of Ruinarch to regenerate the baseline.

## Quick start

```bash
tools/setup.sh        # ensure ilspycmd is installed
tools/decompile.sh    # regenerate reference/ from your local install
tools/seed-src.sh     # first time: seed src/ from the baseline
```

Edit `tools/env.sh` if your Ruinarch install path differs.

## Status

- [x] M0: reproducible raw baseline export
- [x] M1: `src/` compiles to DLLs, 0 errors *(build via `tools/build.sh`)*
- [x] M2: drop-in DLLs boot the game *(our build runs to main menu)*
- [x] M3: behavior-verified across a full scenario *(playtested, no issues)*
- [x] M4: all major systems documented ([docs/systems/](docs/systems/))
- [x] M5: structured control flow, edit/build/run dev loop ([docs/MODDING.md](docs/MODDING.md))
- [x] M6: first-class mod loader ([docs/MODLOADER.md](docs/MODLOADER.md))

| Path | What |
|---|---|
| `tools/` | decompile, build, and verify scripts |
| `src/` | organized, buildable game source (the deliverable) |
| `examples/` | reference mod for the built-in loader |
| `reference/` | raw ilspycmd baseline (gitignored; regenerate locally) |
| `docs/` | architecture and per-system deep dives |

## Modding

A mod loader is baked into the recompiled source, so there is no external
injector. Drop a DLL in `Mods/` and it loads before the first scene, with
[Harmony](https://github.com/pardeike/Harmony) runtime patching (verified under
Proton/Wine and Mono). Copy `examples/ExampleMod/` to start. Full guide:
[`docs/MODLOADER.md`](docs/MODLOADER.md).

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for the full strategy,
toolchain, and milestone definitions.
