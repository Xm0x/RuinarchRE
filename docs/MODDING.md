# Working on the game

The source in `src/` is the real, recompilable game code. This is the loop for
changing it and seeing your change in-game.

## The loop

```
edit src/Assembly-CSharp/**.cs
  -> tools/deploy.sh          # builds both DLLs, swaps them into testgame/
  -> tools/deploy.sh --run    # ...and launches that copy through Proton
```

`deploy.sh` only ever touches `Assembly-CSharp.dll` and
`Assembly-CSharp-firstpass.dll` in the target's `Ruinarch_Data/Managed/`. The
first deploy backs up the stock DLLs as `*.dll.orig`.

### Targets

| command | target |
|---|---|
| `tools/deploy.sh` | `testgame/` — the safe copy (default) |
| `tools/deploy.sh --target DIR` | any Ruinarch dir you point at |
| `tools/deploy.sh --install` | your **real** Steam install (asks for `yes`) |
| `tools/deploy.sh --restore` | put the stock DLLs back, remove ours |

**Always iterate against `testgame/`.** A build with a mistake in it won't touch
the game you actually play. Deploy to `--install` only once a change is proven in
the test copy.

## Build only (no swap)

```
tools/build.sh                       # Assembly-CSharp -> build/Assembly-CSharp.dll
tools/build.sh Assembly-CSharp-firstpass
```

The build calls Roslyn `csc` directly and references the game's own
`Managed/*.dll` (mirroring how Unity compiled it). It does **not** use the .NET
SDK targeting pack, whose `mscorlib`/`System.Core` reference assemblies collide
with Unity's and break `ExtensionAttribute` resolution. Warnings are expected;
`exit 0` with no `error CS` lines means success.

## Launching manually

`deploy.sh --run` does this for you, but to run a copy by hand:

```bash
echo 909320 > testgame/steam_appid.txt        # lets Steamworks init standalone
STEAM_COMPAT_DATA_PATH="$PWD/testprefix" \
STEAM_COMPAT_CLIENT_INSTALL_PATH="$HOME/.local/share/Steam" \
  "$HOME/.local/share/Steam/steamapps/common/Proton 10.0/proton" \
  run testgame/Ruinarch.exe
```

The Proton prefix lives in `testprefix/` (gitignored). Steam must be running so
Steamworks can attach.

### Watching for problems

The game log is written inside the prefix:

```
testprefix/pfx/drive_c/users/steamuser/AppData/LocalLow/Maccima Games/Ruinarch/Player.log
```

`grep -iE 'exception|NullReference' <that path>` after a run surfaces managed-code
crashes. A healthy fully-loaded game sits around ~1.5-1.8 GB RSS and burns CPU
(the simulation thread) — that, plus a menu on screen, is "it works".

## Making a change — worked example

1. Find the code. Types keep their original names, so `grep` works well, and the
   [`docs/systems/`](systems/) deep-dives map each subsystem to its files.
   For example, arsonist AI lives in `src/Assembly-CSharp/ArsonistBehaviour.cs`.
2. Edit the C#.
3. `tools/deploy.sh --run` and verify in-game.
4. When happy, commit. If you want it in your own play install:
   `tools/deploy.sh --install`.

## Naming cleanup as you go

Locals still carry decompiler-generic names (`num`, `flag`, `text`, `list`).
Rename them **per method as you touch it** — that keeps diffs meaningful and
avoids a risky project-wide sweep. Real type/method/field names are already
intact, so navigation doesn't need it.

## Regenerating from scratch

Nothing here depends on committed binaries. On a fresh clone with the game
installed:

```
tools/setup.sh       # checks toolchain, sees the install
tools/decompile.sh   # raw baseline -> reference/  (gitignored)
tools/seed-src.sh    # seed src/ from reference/   (only if src/ is empty)
tools/build.sh       # compile
```

`src/` is committed, so a normal clone skips straight to `tools/build.sh`.
