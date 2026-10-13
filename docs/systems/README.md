# System deep-dives

**A navigation guide to Ruinarch's major systems.** Start with the observed behavior, then follow the named types in `src/Assembly-CSharp/`.

[Repository](../../README.md) · [Architecture](../ARCHITECTURE.md) · [Modding](../MODDING.md)

## Choose an entry point

| Doc | System |
|---|---|
| [CORE_LOOP.md](CORE_LOOP.md) | Tick/time model, `Messenger` event bus, per-entity fan-out, GOAP entry, boot order |
| [CHARACTER_AI_JOBS.md](CHARACTER_AI_JOBS.md) | Behaviour selection → Jobs → GOAP planning → action execution; Interrupts & Traits |
| [WORLD_MODEL.md](WORLD_MODEL.md) | GridMap → Region → Area → InnerTileMap → LocationGridTile → TileObject; structures, settlements, factions |
| [SAVE_FORMAT.md](SAVE_FORMAT.md) | `.zip` save layout, `SaveData<T>` tree, two-phase load, FullSerializer + SQLite-for-logs |
| [ASSETS.md](ASSETS.md) | Where content lives (assets/resources/scenes) and how to extract it from your own install |

## Identities that cross system boundaries

- **Targets:** the AI acts on `IPointOfInterest` objects (`POI_TYPE`), including characters, tile objects and structures.
- **Persistence:** an `ISavable` object's string `persistentID` joins it back to the saved object graph.
- **Content:** enum-driven identifiers connect data to behaviors: `JOB_TYPE` (229), `INTERACTION_TYPE` (about 250), `TILE_OBJECT_TYPE` (300+), `STRUCTURE_TYPE` (85+) and `FACTION_TYPE`. These are baseline counts, not a modding extension contract.

For a job that stops unexpectedly, begin with **Character AI and jobs**; inspect the tick and signal lifecycle in **Core loop**, then the object's ownership in **World model**. For a reload issue, start with **Save format** before assuming the live cache is durable saved state.
