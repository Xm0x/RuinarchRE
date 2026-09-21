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
- [x] M6: mod loader shipped as its own project ([RuinarchModLoader](https://github.com/Xm0x/RuinarchModLoader))

| Path | What |
|---|---|
| `tools/` | decompile, build, and verify scripts |
| `src/` | organized, buildable game source (the deliverable) |
| `docs/` | architecture and per-system deep dives |
| `reference/` | raw ilspycmd baseline (gitignored; regenerate locally) |
| `tools/extract-assets.sh` | optional asset extraction (run on your own install) |

## Modding

Modding lives in a separate project, **[RuinarchModLoader](https://github.com/Xm0x/RuinarchModLoader)**.
It ships a patcher that installs a mod loader into your own `Assembly-CSharp.dll`
(stock game or this rebuild), with [Harmony](https://github.com/pardeike/Harmony)
runtime patching. Keeping it separate keeps this repo focused on faithful source
restoration. Because the game's names are intact here, this source is the
reference a modder reads to find exactly what to patch.

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for the full strategy,
toolchain, and milestone definitions.
