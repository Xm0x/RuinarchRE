# System deep-dives

Reverse-engineered documentation of Ruinarch's major systems. Each maps the real
decompiled types in `src/Assembly-CSharp/` to how the system actually works.

| Doc | System |
|---|---|
| [CORE_LOOP.md](CORE_LOOP.md) | Tick/time model, `Messenger` event bus, per-entity fan-out, GOAP entry, boot order |
| [CHARACTER_AI_JOBS.md](CHARACTER_AI_JOBS.md) | Behaviour selection → Jobs → GOAP planning → action execution; Interrupts & Traits |
| [WORLD_MODEL.md](WORLD_MODEL.md) | GridMap → Region → Area → InnerTileMap → LocationGridTile → TileObject; structures, settlements, factions |
| [SAVE_FORMAT.md](SAVE_FORMAT.md) | `.zip` save layout, `SaveData<T>` tree, two-phase load, FullSerializer + SQLite-for-logs |

Cross-cutting facts:
- Everything the AI targets is an `IPointOfInterest` (`POI_TYPE`): characters,
  tile objects, structures.
- Every persistable object is an `ISavable` with a string `persistentID` — the
  key that stitches the whole save graph back together.
- Content is enum-driven: `JOB_TYPE` (229), `INTERACTION_TYPE` (~250),
  `TILE_OBJECT_TYPE` (300+), `STRUCTURE_TYPE` (85+), `FACTION_TYPE`.
