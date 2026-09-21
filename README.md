# RuinarchRE

DISCLAIMER: For %100 honesty, help of AI was used in this project.

A 100% reverse-engineering / source-restoration project for **Ruinarch**
(Maya Games), a Unity **Mono** title. Because the game ships .NET CIL with full
metadata intact, the goal is a clean, organized, documented, **recompilable**
C# source tree that reproduces the game — the managed-code analog of the GTA SA
decompilation projects.

> **Legal:** this repo ships no game binaries. It contains only reverse-
> engineering scripts, our own reconstructed/annotated source, and docs. You
> must own a legitimate copy of Ruinarch to regenerate the baseline.

## Quick start

```bash
tools/setup.sh        # ensure ilspycmd is installed
tools/decompile.sh    # regenerate reference/ from your local install
tools/seed-src.sh     # first time: seed src/ from the baseline
```

Edit `tools/env.sh` if your Ruinarch install path differs.

## Status

- [x] M0 — reproducible raw baseline export
- [x] M1 — `src/` compiles to DLLs, 0 errors *(build via `tools/build.sh`)*
- [x] M2 — drop-in DLLs boot the game *(verified: our build runs to main menu)*
- [x] M3 — behavior-verified across a full scenario *(playtested on our build, no issues)*
- [x] M4 — all major systems documented ([docs/systems/](docs/systems/): core loop, AI/jobs, world, saves)
- [x] M5 — structured control flow (no goto artifacts) + edit→build→run dev loop ([docs/MODDING.md](docs/MODDING.md))
- [x] M6 — first-class mod loader: drop-in DLLs + Harmony ([docs/MODLOADER.md](docs/MODLOADER.md))

| Path | What |
|---|---|
| `tools/` | reproducible decompile + build + verify scripts |
| `src/` | organized, cleaned, buildable game source (the deliverable) |
| `examples/` | reference mod for the built-in loader |
| `reference/` | raw ilspycmd baseline (gitignored; regenerate locally) |
| `docs/` | architecture + per-system deep dives |
| `native/` | Ghidra notes (low priority; game is 100% managed) |

## Modding

Because we own the recompiled source, a mod loader is baked straight into the
game — no external injector. Drop a DLL in `Mods/`, it loads before the first
scene, and [Harmony](https://github.com/pardeike/Harmony) runtime patching works
(verified under Proton/Wine + Mono). Copy `examples/ExampleMod/` to start. Full
guide: [`docs/MODLOADER.md`](docs/MODLOADER.md).

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for the full strategy,
toolchain rationale, and milestone definitions.
