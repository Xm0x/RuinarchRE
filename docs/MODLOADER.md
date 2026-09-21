# Mod Loader

There are two ways to change Ruinarch in this project:

| Want to... | Use | Doc |
|---|---|---|
| Change the game's own code and recompile it | edit `src/`, `tools/deploy.sh` | [`MODDING.md`](MODDING.md) |
| Ship a **drop-in mod** that loads without recompiling the game | the mod loader below | this doc |

The mod loader is a first-class system baked into our `Assembly-CSharp` source
(`src/Assembly-CSharp/Modding/`). Because we own the source, the loader is part
of the game itself, with no external injector, no `winhttp.dll` proxy, no BepInEx.
Drop a DLL in `Mods/` and it loads.

## How it boots

The loader runs **before the first scene loads**, so any patches a mod applies
are in place before the game code they target ever executes.

- **Primary:** a C# **module initializer** (`[ModuleInitializer]` on
  `ModLoader.ModuleInit`). Roslyn emits the call into `<Module>.cctor`, which the
  Mono runtime runs the instant it loads `Assembly-CSharp`, the earliest
  managed entry point there is. This is what makes the loader fire even though
  it's injected post-build.
  - `net4.x` mscorlib predates `ModuleInitializerAttribute` (added in .NET 5), so
    we declare the attribute ourselves in
    `Modding/ModuleInitializerAttribute.cs`. Roslyn only cares about the
    fully-qualified name, so the compiler emits the call regardless.
- **Fallback:** `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` on
  `ModLoader.Initialize`, for the case where the game is rebuilt from a Unity
  project (which regenerates `RuntimeInitializeOnLoads.json`). `Initialize()` is
  idempotent, so both firing is harmless.

## The `Mods/` folder

Resolved to `<gameRoot>/Mods` (next to `Ruinarch.exe`), created if missing.
Layout:

```
Mods/
  0Harmony.dll            # shared dependency, resolved for every mod
  mods.log                # written fresh each launch (all mods append)
  ExampleMod/
    ExampleMod.dll        # the mod
    mod.json              # optional metadata
  AnotherMod.dll          # a bare DLL directly in Mods/ also works
```

Discovery scans every `*.dll` directly in `Mods/` **plus one level of
subfolders** (`Mods/MyMod/MyMod.dll`). A DLL that exposes no `IRuinarchMod`
implementation (e.g. `0Harmony.dll`) is inspected and skipped, so dependencies and
mods can share the folder.

**Isolation:** a mod that throws while loading is caught, logged, and skipped. It
never takes down the game or the other mods. An `AssemblyResolve` hook lets a mod
drop its own dependencies anywhere under `Mods/` and have them resolve.

## The mod API

Everything lives in the `Ruinarch.Modding` namespace.

### `IRuinarchMod`: the entry point

Implement on exactly one public, parameterless-constructible class. The loader
instantiates it and calls `OnLoad` once.

```csharp
public interface IRuinarchMod
{
    void OnLoad(ModContext context);
}
```

### `ModContext`: what you're handed

```csharp
public sealed class ModContext
{
    public ModInfo   Info        { get; }  // metadata from mod.json (or defaults)
    public string    ModDirectory { get; } // absolute path this DLL loaded from
    public string    ModsRoot     { get; } // absolute path to Mods/
    public ModLogger Logger       { get; } // scoped logger
}
```

### `ModInfo`: `mod.json`

Optional file next to the DLL. Any missing field falls back to a default derived
from the DLL name. Parsed with Unity's `JsonUtility` (no external JSON dep).

```json
{
  "id": "author.mymod",
  "name": "My Mod",
  "version": "1.0.0",
  "author": "you",
  "description": "what it does"
}
```

### `ModLogger`: logging

`Logger.Info/Warning/Error` write to Unity's `Player.log` (prefixed with the mod
id) **and** append to `Mods/mods.log`. Use `mods.log` for iteration: Unity
buffers `Player.log` heavily, but `mods.log` is flushed on every write, so it's
the reliable place to watch what your mod did.

## Harmony

`0Harmony.dll` (Harmony 2.x, `net472`, Mono-compatible) is bundled in `Mods/` and
resolved for every mod. Patch the game from `OnLoad`:

```csharp
using HarmonyLib;
using Ruinarch.Modding;

public class MyMod : IRuinarchMod
{
    public void OnLoad(ModContext context)
    {
        var harmony = new Harmony(context.Info.id);
        harmony.PatchAll(typeof(MyMod).Assembly);
    }
}

[HarmonyPatch(typeof(Character), nameof(Character.Death))]
static class Character_Death_Patch
{
    static void Postfix(Character __instance)
    {
        Debug.Log($"[mymod] {__instance.name} died");
    }
}
```

Harmony runtime detours are **verified working** under Proton/Wine + Mono
(Unity 2020.3). See the self-test in `examples/ExampleMod`, whose postfix fires
on a method it patches at load. The real game names are intact, so
`AccessTools.Method(typeof(SomeType), "SomeMethod")` and `[HarmonyPatch(...)]`
resolve directly against the decompiled source in `src/`.

## Writing and building a mod

`examples/ExampleMod/` is the reference. Copy it as a starting point.

```
tools/build-mod.sh examples/ExampleMod          # -> Mods/ExampleMod/ExampleMod.dll (+ 0Harmony.dll)
tools/build-mod.sh path/to/YourMod  DIR         # build into a specific game copy's Mods/
```

`build-mod.sh` compiles against the game's own assemblies **plus** our modding
API and Harmony, mirroring the main build's direct-`csc` approach. It copies
`0Harmony.dll` into the target `Mods/` alongside your DLL.

`tools/get-harmony.sh` fetches `0Harmony.dll` from NuGet into `lib/` (it's small
and MIT-licensed, but kept out of git so the repo stays self-contained via
script, not binary).

## Running

Deploy the modding-enabled build and drop mods in its `Mods/`:

```
tools/deploy.sh --run     # build our Assembly-CSharp (with loader) into testgame/ and launch
```

Then read the result:

```
cat testgame/Mods/mods.log
```

A healthy load looks like:

```
[example.mod] Hello from Example Mod v1.0.0!
[example.mod] Harmony attached to 2 method(s): ExampleMod.Probe, MainMenuManager.Start
[example.mod] >>> SELF-PATCH postfix fired - Harmony detours WORK in this runtime <<<
```

## Gotchas

- **Mono inlines trivial methods.** A Harmony patch on a tiny method can look
  like it "didn't fire" because the JIT inlined the callsite and never entered
  the patched body. This is a *false negative*, not a Harmony failure. If you're
  writing a self-test target, mark it `[MethodImpl(MethodImplOptions.NoInlining)]`
  (that's exactly what `ExampleMod.Probe` does). Real game methods (Unity
  messages like `Start`, virtual overrides, anything called by reflection) aren't
  inlined and patch normally.
- **`Player.log` is buffered; `mods.log` is not.** Watch `mods.log` when
  iterating.
- **Restart loop under the test harness.** Launching the `testgame/` copy while
  the Steam client is running can make Steamworks' `RestartAppIfNecessary` bounce
  the process (intro shows, closes, relaunches, Steam says "launching"), because
  the test copy shares appid `909320` with your installed game. This is a
  *launch-environment* artifact, not a loader problem; the mod loads on every
  boot regardless. To avoid it, launch with the Steam client closed, or test the
  loader against a build deployed with `tools/deploy.sh` and read `mods.log`.
