# RuinarchRE: Reverse Engineering Architecture

## 0. Target

Ruinarch is a Unity Mono game. All gameplay logic ships as .NET CIL bytecode
with **full metadata intact** (type, method, field, and namespace names, verified
unobfuscated across 4980 types). So this is a **source-restoration** project:
recover a clean, organized, documented, and ultimately **recompilable** C# tree
that reproduces the game's behavior.

### Where the code lives

| Binary | Nature | RE priority |
|---|---|---|
| `Ruinarch_Data/Managed/Assembly-CSharp.dll` (5.3 MB, ~4980 types) | **all game logic** | PRIMARY |
| `Ruinarch_Data/Managed/Assembly-CSharp-firstpass.dll` (433 KB) | early-compiled game code (plugins/third-party integrated first) | PRIMARY |
| `UnityPlayer.dll` (native, 26.8 MB) | stock Unity engine | skip (not game-specific) |
| `Ruinarch_Data/Plugins/x86_64/*` | stock SQLite / Wwise / Steamworks | skip |
| Unity*/System*/third-party managed DLLs | engine + libraries | reference only, not reversed |

Data assets (`*.assets`, `sharedassets*`, `resources.assets`) hold prefabs,
sprites, scriptable-object data. Reversing those (asset extraction) is a
separate, later track (see §5).

## 1. Toolchain

- **ilspycmd** (ICSharpCode.Decompiler 9.x): primary. Whole-project export to
  `.csproj` + namespace-nested `.cs`. Reproducible, scriptable.
- **dnSpy / ILSpy GUI**: interactive inspection, live debugging of the running
  game, on-the-fly edits to validate a hypothesis.
- **monodis** (mono): low-level metadata/table inspection when the decompiler is
  ambiguous.

Rationale: with intact metadata, a disassembler-first workflow would throw away
the free names and structure that make this game 95%-recovered on the first
export. We lean on the CIL decompiler and spend human effort on the small
fraction it mangles, plus comprehension.

## 2. Repository model (copyright-safe)

The repo **never** contains game binaries or the raw baseline export. It ships:

- `tools/`: scripts that regenerate the baseline from a legit local install.
- `src/`: our organized, cleaned, buildable C# tree (the deliverable).
- `docs/`: system documentation.

`reference/` (raw ilspycmd output) and every `*.dll/*.exe/*.assets` are
gitignored. Workflow for a fresh clone:

```
tools/setup.sh       # ensure ilspycmd present
tools/decompile.sh   # regenerate reference/ from your Ruinarch install
tools/seed-src.sh    # first-time: copy reference -> src for cleanup work
```

## 3. Two tracks

### Track A: Mechanical (make it build)
1. Whole-project export (`tools/decompile.sh`).
2. Seed `src/` from the baseline.
3. Iterate to a **clean compile**: fix decompiler artifacts the C# compiler
   rejects (see §4). Reference the game's own Managed/ DLLs for the exact
   assembly set the original linked against.
4. **Drop-in verify:** build our DLL, swap it into a copy of the game, confirm
   the game boots and behaves. This is the "100%" proof.

Progress metric: `types compiling / total types`.

### Track B: Comprehension (understand it)
Per-system deep dives in `docs/systems/`, driven by curiosity and by whatever
Track A surfaces. Candidate systems, roughly in dependency order:
- Core loop: `GameManager`, `Messenger` event bus, tick/scheduling.
- Character AI: `*Behaviour` classes, `behaviourComponent`, job system
  (`JobQueueItem`, `jobComponent`), GOAP.
- World model: `Area`, `Region`, `Inner_Maps`, `LocationGridTile`,
  settlements/factions.
- Persistence: `SaveData*` types, the SQLite `System.Data.SQLite` usage,
  save/load format (high-value), documented in `docs/systems/SAVE_FORMAT.md`.
- Data/content: how scriptable objects and `StreamingAssets` feed the sim.

Comprehension work may rename locals, add XML-doc comments, and restructure,
always keeping behavior identical and diffable against `reference/`.

## 4. Known decompiler artifacts to fix (Track A)

- Compiler-generated names (`<>c__DisplayClass`, `<Method>b__0`): legal CIL,
  sometimes illegal C#; rename or restructure.
- `yield`/`async` state machines re-emitted verbatim: usually recompile fine
  under ILSpy's high-level output, but watch for `ref`-struct edge cases.
- `ref`/`out` + `in` param modreqs, `readonly struct`, function pointers.
- Explicit interface impls and unspeakable names.
- `[assembly:]` attributes / InternalsVisibleTo: restore from metadata.
- Init-only + `record` lowering on newer C#: pin `<LangVersion>` in the csproj.

## 5. Later tracks (post-build)

- **Asset extraction** (AssetStudio/AssetRipper) to recover prefabs, sprites,
  ScriptableObject data, reconstructing the *content* the code drives.

## 6. Milestone ladder ("100%" defined)

- **M0: Baseline** ✅ reproducible raw export of both assemblies.
- **M1: Compiles**: `src/Assembly-CSharp` + `firstpass` build to DLLs with 0
  errors, then push to GitHub.
- **M2: Drop-in runs**: our DLLs replace the originals; game boots to main menu
  and starts a world.
- **M3: Behavior-verified**: play a full scenario (spawn, jobs, combat,
  save/load) with no divergence from stock.
- **M4: Documented**: every major system in §3 has a `docs/systems/*.md`.
- **M5: Clean source**: names/comments restored to human-authored quality; tree
  reads like the original repo would.
- **M6: Mod loader**: shipped as its own project,
  [RuinarchModLoader](https://github.com/Xm0x/RuinarchModLoader) (a patcher that
  installs a Harmony-based loader into the game's own assembly).

"100% RE" = **M3 + M4** (functionally-equivalent buildable source plus full
system docs). M5 is the polish pass; M6 (a separate project) makes it moddable.
