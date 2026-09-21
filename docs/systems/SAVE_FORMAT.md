# Save / Load & Persistence

How Ruinarch persists a game. Summary up front:

- A save is a **`.zip`** containing a FullSerializer-encoded game state
  (`mainSave.sav`), a SQLite **log** database (`gameDB.db`), JSON metadata
  (`quickInfo.json`), and a thumbnail (`screen.png`).
- Game state is a tree of **`SaveData<T>`** objects rooted at
  **`SaveDataCurrentProgress`**, serialized via **BayatGames.SaveGameFree**
  (which uses **FullSerializer**).
- Load is **two-phase**: instantiate objects, then resolve `persistentID`
  string references.
- **SQLite stores only logs**, never game state.

Paths relative to `src/Assembly-CSharp/`.

## On-disk layout

```
<WorldType>-<Day>_<Time>.zip        (manual)   |   <timestamp>.zip (autosave)
├── mainSave.sav      FullSerializer binary of SaveDataCurrentProgress (~5-50 MB)
├── gameDB.db         SQLite3 event-log database (~1-5 MB)
├── quickInfo.json    metadata (Newtonsoft.Json): saveVersion, scenarioName,
│                     omnipotentMode, archetype, portalLevel, appliedMods
└── screen.png        screenshot thumbnail
```

Directories (`UtilityScripts/Utilities.cs`), all under
`Application.persistentDataPath`:
- `gameSavePath` = `…/Ruinarch Game Saves/`: manual saves.
- `autosavePath` = `…/Ruinarch Game Saves/Autosaves/`: autosaves (**max 3**,
  oldest rotated out).
- `tempPath` / `tempZipPath` = `…/Temp/`: staging for save packaging / load
  extraction.

> Under Proton these live in the prefix at
> `…/AppData/LocalLow/Maccima Games/Ruinarch/Ruinarch Game Saves/`.

## Data model

### `ISavable.cs`
Every persistable runtime object implements it:
- `persistentID` (string GUID via `Utilities.GetNewUniqueID()`): the primary key
  for all cross-references.
- `objectType` (`OBJECT_TYPE`): which hub it belongs to.
- `serializedData` (`Type`): reflection hint to its `SaveData<T>` subclass.

Implementers: `Character`, `Faction`, `TileObject`, `Party`, `PartyQuest`,
`ActualGoapNode`, …

### `SaveData.cs`: `SaveData<T> : BaseSaveData`
`[Serializable]` generic wrapper with virtual `Save(T data)`, `T Load()`,
`CleanUp()`. Instances are created by reflection:
```csharp
var sd = (SaveData<T>)Activator.CreateInstance(data.serializedData);
sd.Save(data);                       // copy runtime → save fields
```
Subclasses: `SaveDataCharacter`, `SaveDataFaction`, `SaveDataTileObject` (and
weapon/armor/resource/structure variants), `SaveDataParty`,
`SaveDataActualGoapNode`, …

### `SaveDataCurrentProgress.cs`: the root
Holds all game state:
- Scalars: `fileName`, `gameVersion` (`Application.version`), timestamp,
  language, game date (month/day/year/tick), `continuousDays`.
- `worldMapSave`, `worldSettingsData`, `familyTreeDatabase`, `playerSave`
  (`SaveDataPlayerGame`), `victoryCondition`, portrait availability, plague state.
- **`objectHub`** `Dictionary<OBJECT_TYPE, BaseSaveDataHub>`: the bulk store.
  Each hub is `Dictionary<persistentID, SaveData*>`. Keys include: `Character`,
  `Faction`, `Tile_Object`, `Action` (in-progress GOAP nodes), `Party`,
  `Party_Quest`, `Job`, `Crime`, `Interrupt`, `Trait`, `Gathering`,
  `Game_Alert`, `Shared_Opinion_Modifier`.

Example leaf (`SaveDataCharacter.cs`): 80+ fields (id, name, isDead, race,
gender, HP, position/rotation, …) plus nested component SaveData
(`classComponent`, `needsComponent`, `behaviourComponent`, `jobComponent`, …).
`Load()` builds the `Character` via `CharacterManager.CreateFromSave`;
`LoadReferences()` resolves faction/job/structure/territory links.

## Save pipeline: `SaveManager.cs`, `SaveCurrentProgressManager.cs`

`SaveManager` (singleton) owns `SavePlayerManager` + `SaveCurrentProgressManager`
and the directory setup. A save (`DoManualSave(fileName, callback, isAutosave)`)
runs a coroutine that fans serialization across threads:

1. `new SaveDataCurrentProgress()` → `Initialize()` (fileName, version,
   timestamp, empty `objectHub`).
2. `SaveDate`, `SaveWorldSettings`, `SavePlayer`.
3. `SaveFactions` (from `FactionManager.allFactions`), `SaveCharacters`
   (`CharacterManager.CreateNewSaveDataCharacter`), `SaveJobs`, `SaveActions`,
   `SaveVictoryCondition`.
4. `SaveTileObjectsMultithread`: TileObjects batched **200 per thread**;
   `SaveDestroyedTileObjects` on its own thread.
5. Each object goes through `AddToSaveHub<T>()`: reflect `SaveData<T>`,
   `Save(data)`, store in `objectHub[objectType]`.
6. `RuinarchSQLDatabase.SaveInMemoryDatabaseToFile(gameDB.db)` backs up the log DB.
7. `SaveGame.Save(mainSave.sav, root)`: BayatGames.SaveGameFree → FullSerializer
   binary.
8. `ZipFile.CreateFromDirectory(tempZipPath, out)` packages the `.zip`; move to
   `gameSavePath` (manual) or `autosavePath` (autosave, rotate to 3).

## Load pipeline: two phases

`LoadSaveDataCurrentProgress(...)` extracts the ZIP to `tempPath` and queues
`ReadSaveDataFileInOtherThread()`.

**Phase 0, deserialize:** `SaveGame.Load<SaveDataCurrentProgress>(mainSave.sav)`
→ FullSerializer rebuilds the SaveData graph (`objectHub` populated). Log DB
restored via `RuinarchSQLDatabase.LoadDatabaseFromFileToMemory(gameDB.db)`.

**Phase 1, instantiate (`Load`):** `SaveDataCurrentProgress` calls
`LoadCharacters` / `LoadFactions` / `LoadTileObjects` / `LoadParties` /
`LoadActions` / … Each `SaveData.Load()` builds a runtime object (e.g.
`CharacterManager.CreateFromSave`). After this, objects exist but cross-refs are
still **null** (only `persistentID` strings held).

**Phase 2, resolve (`LoadReferences`):** `LoadCharacterReferences` /
`LoadFactionReferences` / … iterate instantiated objects and call
`obj.LoadReferences(saveData)`, turning IDs into object refs, e.g.:
```csharp
faction = FactionManager.Instance.GetFactionBasedOnPersistentID(data.faction);
```
Deferred waves: `LoadTraitsSecondWave` (instanced traits) and
`LoadCharactersCurrentAction` (reattach the in-progress `ActualGoapNode`).

This two-phase split is what lets the save format use flat `persistentID`
strings with no foreign-key constraints. All graph edges are rebuilt after every
node exists.

## SQLite, logs only: `Databases/SQLDatabase/RuinarchSQLDatabase.cs`

`System.Data.SQLite`, an **in-memory** DB (`Data Source=:memory:;`). One `Logs`
table (persistentID PK, dates, `logText`/`rawText`, category, key, actionID,
`involvedObjects`, `isIntel`; extra columns added per `LOG_TAG` /
`LOG_IDENTIFIER` via `ALTER TABLE`).
- `InsertLog` / `InsertLogUsingMultiThread` (batched via `MultiThreadPool`) /
  `InsertLogAndDeleteOldest` (caps at `SettingsManager.logLimit`).
- `PopulateLogsThatMatchCriteria`: filtered, paginated queries for the Logs UI.
- Persisted with `SQLiteConnection.BackupDatabase()` in both directions
  (memory ↔ `gameDB.db`).

**Orthogonal to game state**: `gameDB.db` corruption loses logs only; game
recovers from `mainSave.sav`. Save corruption loses the game; logs are
unaffected. (Note: `DatabaseManager` boots pre-world, see
`docs/systems/CORE_LOOP.md` §7, so logging/localization are DB-backed before the
world exists.)

## Player-level data: `SavePlayerManager.cs`
`SaveDataPlayer` (settlement, unlocked skills, tutorials) is stored **separately**
from the run: `gameSavePath + SAVED_PLAYER_DATA_2` via `SaveGame.Save/Load`. It is
not inside the per-game `.zip`.

## Versioning: `SaveUtilities.cs`, `SaveDataQuickInfo.cs`
- Version captured at save time in `SaveDataCurrentProgress.gameVersion`,
  `SaveDataQuickInfo.saveVersion`, and `SaveDataPlayer.gameVersion`
  (= `Application.version`).
- `IsSaveFileVersionCompatible(v)`: `v == Application.version` **or** `v` in the
  hard-coded `compatibleSaveFileVersions` (`"1.05","1.1","1.101","1.102"`).
- `IsSaveFileValid(zip)` extracts `mainSave.sav`, reads `gameVersion` via
  Newtonsoft.Json, checks compatibility. `AreAppliedModsCompatible()` guards
  mod-mismatch corruption.
- **No auto-migration**: incompatible versions are rejected at load, not
  converted.

## Formats at a glance
| File | Serializer | Human-readable |
|---|---|---|
| `mainSave.sav` | FullSerializer (BayatGames.SaveGameFree) | no |
| `gameDB.db` | SQLite3 | via sqlite tools |
| `quickInfo.json` | Newtonsoft.Json | yes |

> RE tip: because `mainSave.sav` is FullSerializer's JSON-compatible format, a
> save can be inspected by decoding it with the same `FullSerializer.dll` shipped
> in `Managed/`, a natural next tool for save-editing/analysis.
