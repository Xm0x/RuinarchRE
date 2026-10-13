# System deep-dives

Find the code behind an observed Ruinarch behavior. The guides below connect
runtime systems to named types in `src/Assembly-CSharp/`.

[Repository](../../README.md) · [Architecture](../ARCHITECTURE.md) · [Modding](../MODDING.md)

## Start from the symptom

| Investigating | Guide and scope |
|---|---|
| Startup, time or signal delivery | [Core loop](CORE_LOOP.md): tick/time model, `Messenger`, per-entity fan-out, GOAP entry and boot order |
| A character's choice or interrupted job | [Character AI and jobs](CHARACTER_AI_JOBS.md): behavior selection, Jobs, GOAP planning, action execution, interrupts and traits |
| A tile, building, settlement or faction | [World model](WORLD_MODEL.md): `GridMap`, `Region`, `Area`, `InnerTileMap`, `LocationGridTile`, `TileObject` and ownership |
| A save that loads differently | [Save format](SAVE_FORMAT.md): ZIP layout, `SaveData<T>`, two-phase load, FullSerializer and SQLite logs |
| Missing or unexpected visual content | [Assets](ASSETS.md): assets, resources, scenes and extraction from your own installation |

## Identities that cross system boundaries

- **Targets:** the AI acts on `IPointOfInterest` objects (`POI_TYPE`), including characters, tile objects and structures.
- **Persistence:** an `ISavable` object's string `persistentID` joins it back to the saved object graph.
- **Content:** enum-driven identifiers connect data to behaviors: `JOB_TYPE` (229), `INTERACTION_TYPE` (about 250), `TILE_OBJECT_TYPE` (300+), `STRUCTURE_TYPE` (85+) and `FACTION_TYPE`. These are baseline counts, not a modding extension contract.

Follow a stopped job from **Character AI and jobs** to the tick and signal
lifecycle in **Core loop**, then to ownership in **World model**. For reload
differences, read **Save format** before treating a live cache as durable state.
