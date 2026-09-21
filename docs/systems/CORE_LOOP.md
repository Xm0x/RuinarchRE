# Core Loop & Simulation Spine

How Ruinarch advances its world. This is the backbone every other system hangs
off of: a **tick-based simulation** driven from a single Unity `Update()`, fanned
out to entities through a **static event bus**, with per-character AI planned by
**GOAP**.

Source paths are relative to `src/Assembly-CSharp/`.

## 1. Time model

Fixed integer time, not wall-clock:

| Unit | Value | Notes |
|---|---|---|
| tick | base unit | smallest sim step |
| hour | 20 ticks | `ticksPerHour` |
| day | 480 ticks | `ticksPerDay` (= 24 × 20) |

`GameDate.cs` is the immutable time value object `(month, day, year, tick)`,
ordered by `GameDateComparer`. `GameManager.Today()` mints a fresh `GameDate`
each call.

## 2. Frame → tick driver — `GameManager.cs`

`GameManager` is the singleton at the center. Its Unity **`Update()`** runs a
real-time timer whose interval is the *progression speed*:

- X1 / X2 / X4 speeds map to per-tick real durations (~0.8s down to ~0.3s).
- Pause state gates the timer.

When the timer elapses, one **tick** fires:

```
GameManager.Update()  ── timer elapsed ──►  TickStarted()
                                              ├─ advance GameDate
                                              ├─ Messenger.Broadcast(Signals.TICK_STARTED)
                                              ├─ (hour/day rollovers: HOUR_STARTED, DAY_STARTED, MONTH_START)
                                              └─ Messenger.Broadcast(Signals.CHECK_SCHEDULES)
                                            ...work spread across frames...
                                            TickEnded()
                                              └─ Messenger.Broadcast(Signals.TICK_ENDED)
```

So a "tick" is not one frame — it's a logical step whose per-entity work is
**amortized across several render frames** (see §4) to avoid stutter.

## 3. Event bus — `Messenger.cs` + `Signals.cs`

Everything decouples through a **static** typed event bus:

- `Messenger.AddListener<T…>(string key, callback)` / `Broadcast<T…>(key, args)`.
- Backing store: `eventTable` `Dictionary<string, Delegate>`. Supports 0–7
  typed args (generic overloads).
- `MarkAsPermanent(key)` protects a listener from `Cleanup()` (used for
  cross-scene/global listeners so a scene teardown doesn't drop them).

Signal keys live in `Signals.cs` (core lifecycle) plus subsystem-specific signal
classes. Core lifecycle signals:

```
GAME_STARTED, GAME_LOADED, PROGRESSION_LOADED
TICK_STARTED, TICK_ENDED
HOUR_STARTED, DAY_STARTED, MONTH_START
CHECK_SCHEDULES
```

`SchedulingManager.cs` listens for `CHECK_SCHEDULES` and fires any actions queued
for the current `GameDate` — this is how timed jobs/events land on the right tick.

## 4. Per-entity fan-out — `CharacterTickManager.cs`

The tick doesn't call every character inline. `CharacterTickManager` **spreads
character updates across frames** with a per-frame budget:

- On `TICK_STARTED`, characters are marked ready.
- Its own `Update()` drains the ready set at N characters/frame (≈10 at X1,
  20 at X2, 30 at X4) under a time budget, calling each
  `character.TickStarted()` then later `character.TickEnded()`.

This is the key perf trick: hundreds of agents, but only a slice tick per frame.

## 5. Per-character logic — `Character.cs`

Each character owns a component bag (needs, behaviour, mood, interrupt, state,
combat, job) plus a `GoapPlanner` and `JobQueue`.

- **`TickStarted` → `OnTickStarted`**: needs decay, then
  `PerStartTickActionPlanning()`.
- **`TickEnded` → `OnTickEnded`**: job processing, mood, combat, trait ticks.

`PerStartTickActionPlanning()` is the decision point:

```
if CanPlanGoap()            // no active plan, no active job, not interrupted
    BehaviourComponent.RunBehaviour()   // pick highest-priority behaviour → goal
else
    continue current GOAP action / job
```

(Method ranges in the decompile: `Character.cs` ~3927–3984 and ~4119–4180.)

## 6. Behaviour → goal → plan — `BehaviourComponent.cs` + `Goap/`

- `BehaviourComponent.RunBehaviour()` walks `currentBehaviourComponents`
  (`CharacterBehaviour` subclasses — e.g. `ArsonistBehaviour`,
  `AttackVillageBehaviour`, `PatrolBehaviour`) by `priority` and runs the
  winner, which produces a goal / job.
- `Goap/` implements Goal-Oriented Action Planning:
  - `GoapPlanner.cs` builds a `GoapPlan` (ordered `GoapAction` sequence) for the
    chosen goal.
  - `ActualGoapNode.cs` = a concrete action instance being executed.
  - `GoapPlanJob.cs` ties a plan into the job queue.

So the AI chain per acting character is:

```
tick → RunBehaviour() → CharacterBehaviour (priority) → goal
     → GoapPlanner builds GoapPlan → GoapActions execute over subsequent ticks
     → JobQueue drives execution; TickEnded applies results (mood/combat/traits)
```

## 7. Bootstrap / init order

Scene start is orchestrated so singletons come up in dependency order:

- `StartupManager.cs` — scene bootstrap entry. Calls
  `Initializer.InitializeDataBeforeWorldCreation{MainThread,OtherThread}`, then
  `mapGenerator.InitializeWorld()` **or** `LoadGame()`, then broadcasts
  `GAME_LOADED` / progression-loaded signals.
- `Initializer.cs` — the actual manager boot sequence:
  - **Pre-world (main thread):** LocalizationManager, GameManager,
    DatabaseManager, CharacterManager, TraitManager, PlayerManager,
    InnerMapManager, UIManager, WorldEventManager.
  - **Background thread:** RaceManager, LandmarkManager, CrimeManager,
    JobManager, CombatManager.
  - **Post-world:** LightingManager, QuestManager, AudioManager.
- `LevelLoaderManager.cs` — async scene loads (main menu ↔ game) with progress.

## 8. Notable

- **Threading**: some init runs off the main thread (`Initializer` background
  phase; see also the `Threads/` namespace). World generation is partially
  threaded; sim ticking is main-thread.
- **Save hooks**: `StartupManager` branches to `LoadGame()`; `GAME_LOADED` /
  `PROGRESSION_LOADED` are the signals other systems key off to restore state.
  Save format itself is documented separately (`docs/systems/SAVE_FORMAT.md`).
- **DatabaseManager** boots early (pre-world) — the SQLite layer
  (`System.Data.SQLite`) is available before the world exists, consistent with
  logs/localization being DB-backed.

## Map (one glance)

```
GameManager.Update() [timer]
   └─► TickStarted ──Messenger──► TICK_STARTED ─► CharacterTickManager (spread/frame)
        │                                              └─► Character.TickStarted ─► PerStartTickActionPlanning
        │                                                     └─► BehaviourComponent.RunBehaviour ─► Goap plan
        ├─► CHECK_SCHEDULES ─► SchedulingManager (dated actions)
        └─► TickEnded ──Messenger──► TICK_ENDED ─► Character.TickEnded (jobs/mood/combat/traits)
```
