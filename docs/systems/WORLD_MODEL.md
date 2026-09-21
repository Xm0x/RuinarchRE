# World Model & Map

How Ruinarch represents its world — a **layered grid hierarchy** from the
overworld down to individual tiles and the objects on them. Paths relative to
`src/Assembly-CSharp/`.

## The hierarchy (top → bottom)

```
GridMap                     singleton; Area[,] 2D overworld grid
  └─ Region                 territory; owns a RegionInnerTileMap + List<Area>
       └─ Area              one overworld hex; owns a 14×14 block of tiles
            └─ InnerTileMap detailed LocationGridTile[,] for the region
                 └─ LocationGridTile        one cell (1 Unity unit)
                      └─ GridTileTileObjectComponent
                           └─ TileObject    item/decoration/resource on the tile

LocationStructure  spans many LocationGridTiles   (buildings/dungeons)
BaseSettlement     owns Areas + Structures
Faction            owns Settlements + Characters
```

Fixed dimension: **each Area = 14×14 LocationGridTiles** (196 tiles).
`InnerMapManager.AreaLocationGridTileSize = (14,14)`; structure placement grid
`BuildingSpotSize = (7,7)`.

## Layer 0 — `GridMap.cs`
Singleton world container. Holds `Area[,]` (the overworld grid), `width`/`height`.
`SetMap(Area[,])` populates it; `GetFirstPassableGridTile` /
`GetFirstPassableUnoccupiedGridTile` are common queries.

## Layer 1 — `Region.cs`
A territorial region aggregating Areas and holding the detailed map:
- `List<Area> areas`, `Area coreTile` (representative area).
- `RegionInnerTileMap _regionInnerTileMap` — the detailed grid.
- `Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures` — buildings by
  type (the primary structure query index).
- `List<BaseSettlement> settlementsInRegion`, `List<Faction> factionsHere`,
  residents / `charactersAtLocation`.

## Layer 2 — `Area.cs`
One overworld hex tile. `AreaData (x, y, ID)`, `Region region`,
`List<BaseSettlement> settlementsOnArea`. Behavior split into components:
- `AreaGridTileComponent` — its 14×14 tile block, `centerGridTile`, border tiles.
- `AreaStructureComponent` — structures occupying this area.
- `AreaTileObjectComponent`, `AreaBiomeComponent`, `AreaElevationComponent`,
  `AreaFeatureComponent`.
- `PopulateAreasInRange()` — neighborhood queries.

## Layer 3 — `Inner_Maps/InnerTileMap.cs` + `RegionInnerTileMap.cs`
`InnerTileMap` (abstract) holds `LocationGridTile[,] map`,
`List<LocationGridTile> allTiles`, the `GridGraph` pathfinding graph, and drives
Unity `Tilemap` rendering layers (ground/details/elevation/shore) plus Perlin
noise for elevation/precipitation and biome/temperature gradients.

`RegionInnerTileMap` (concrete) generation entry points are **coroutines**:
- `GenerateMap()` (new world) / `LoadMap()` (from save) — `IEnumerator`s.
- `GetInnerMapSizeGivenRegionDimensions()` = region area-span × (14×14).
- `PopulateNeededAreaDataAfterGridGeneration()` wires each `Area`'s
  `centerGridTile`.

## Layer 4 — `Inner_Maps/LocationGridTile.cs`
One cell. Key state:
- `localPlace` (Vector3Int), `worldLocation` / `centeredWorldLocation`.
- `tileType` (Empty / Wall — blocking), `tileState` (Empty / Occupied — affects
  pathfinding), `groundType` (Soil/Grass/Stone/Water_*/Sand/Magma/Corrupted/…),
  `mainBiomeType` (`BIOMES`), `elevationType` (`ELEVATION`).
- `structure` (owning `LocationStructure`), `parentMap`, `area`, `neighbourList`
  (8-dir), `possibleExits` (`Dictionary<GridNeighbourDirection,…>`).
- Components: `GridTileCorruptionComponent`, `GridTileMouseEventsComponent`,
  `GridTileTileObjectComponent`, `GridTileEventDispatcher`.

## Layer 5 — tile contents — `GridTileTileObjectComponent.cs`
Manages what sits on a tile:
- `objHere` (main occupant), `hiddenObjHere` (traps/fog), `genericTileObject`,
  `walls` (`List<ThinWall>`), trap flags (`hasLandmine`/`hasFreezingTrap`/
  `hasSnareTrap`).
- `SetObjectHere()` (place + update passability), `LoadObjectHere()` (from save),
  `RemoveObjectHere()` (destroy), `RemoveObjectHereWithoutDestroying()` (move).

## Structures — `Inner_Maps/Location_Structures/LocationStructure.cs`
A building/dungeon spanning many tiles:
- `structureType` (`STRUCTURE_TYPE`), `HashSet<LocationGridTile> tiles`,
  `List<LocationGridTile> passableTiles`.
- `groupedTileObjects` `Dictionary<TILE_OBJECT_TYPE, List<TileObject>>` — the
  in-structure object index.
- `settlementLocation` (owning `BaseSettlement`), `region`, `rooms`
  (`StructureRoom[]`), `maxHP`/`currentHP`, `residents`, `pointsOfInterest`.
- Subclasses: `ManMadeStructure`, `DemonicStructure`, `NaturalStructure`,
  `Wilderness`, + 80+ concrete types (Tavern, Farm, Mine, Barracks, MageTower,
  VampireCastle, NecromancerLair, …).

## Objects — `TileObject.cs`
Items/decorations/resources on tiles:
- `tileObjectType` (`TILE_OBJECT_TYPE`, 300+ values), `gridTileLocation`,
  `maxHP`/`currentHP`, `characterOwner`, `isPreplaced`, `persistentID`,
  `state` (`POI_STATE`; `poiType = TILE_OBJECT`).
- Components: `ResourceStorageComponent` (food/materials), `partyComponent`
  (spawners), `traitContainer` (Flammable/Burning…), construction component.
- Subclasses: `ResourcePile`, `GenericTileObject` (default placeholder),
  `StructureObject`, `MonsterSpawner`, trap objects, etc.

Note: `TileObject`, characters, and structures are all **points of interest**
(`IPointOfInterest` / `POI_TYPE`) — a unifying interface the AI targets.

## Settlements — `Locations/Settlements/BaseSettlement.cs`
- `locationType` (`LOCATION_TYPE`), `owner` (`Faction`, null = neutral),
  `List<Area> areas` (can span several), `structures` dict, `allStructures`,
  `residents`, `parties`.
- `AddResident`/`RemoveResident`, `HasSettlementOnArea()`, `IsPartOfVillage()`.
- Subclasses: `PlayerSettlement` (the player's demonic base), `NPCSettlement`
  (village/dungeon variants: Human_Village, Elven_Hamlet, …).

## Factions — `Faction.cs`
- `factionType` (`FACTION_TYPE`), `race` (`RACE`),
  `List<BaseSettlement> ownedSettlements`, `List<Character> characters`,
  `Dictionary<Faction, FactionRelationship> relationships` (war/peace/alliance).
- Components: `FactionIdeologyComponent`, `FactionCrimeComponent`,
  `FactionOpinionComponent`; faction-specific `pathfindingTag` /
  `pathfindingDoorTag`.
- `JoinFaction()` / `LeaveFaction()` and ideology/banning join checks.

## Managers
- **`LandmarkManager.cs`** — world structure creation + queries.
  `GenerateRegionMap(Region, MapGenerationComponent)` coroutine:
  instantiate `RegionInnerTileMap` → `Initialize()` (Perlin seeds) →
  `region.GenerateStructures()` → `innerTileMap.GenerateMap()` →
  `InnerMapManager.OnCreateInnerMap()`.
  Also `CreateNewSettlement(...)`, `GetStructuresOfType(...)`,
  `wildernessMonsterSpawnerData` (`Dictionary<BIOMES, WildernessMonsterSpawnerData[]>`).
- **`Inner_Maps/InnerMapManager.cs`** — display/input layer.
  `currentlyShowingMap`/`currentlyShowingLocation`, `innerMaps` list,
  pathfinding tag map (Ground=1, Obstacle=2, faction doors 4–16, Roads=17,
  Caves=18, Special_Structures=19), `GetTileFromMousePosition()`,
  `OnClickMapObject()`.

## Content enums (drive most of the game's data)
- `LOCATION_TYPE`: VILLAGE, DEMONIC_INTRUSION, DUNGEON, EMPTY, PSEUDO_VILLAGE.
- `STRUCTURE_TYPE`: 85+ (TAVERN=1, WAREHOUSE=2, DWELLING=3, WILDERNESS=5,
  CEMETERY=8, PRISON=9, CITY_CENTER=11, BARRACKS=13, MONSTER_LAIR=22,
  MAGE_TOWER=25, CAVE=27, NECROMANCER_LAIR=78, LICH_GRAVEYARD=85, …).
- `TILE_OBJECT_TYPE`: 300+ (WOOD_PILE=0, TABLE=6, BED=7, ORE=8, TOMBSTONE=12,
  BRAZIER=34, CAMPFIRE=35, TRAINING_DUMMY=59, TREASURE_CHEST=87,
  TORTURE_CHAMBERS_TILE_OBJECT=180, KENNEL_TILE_OBJECT=182, MONSTER_SPAWNER=264,
  …).
- `FACTION_TYPE`: Human_Empire, Demon_Cult, Undead, Elven_Kingdom, Lycan_Clan,
  Divine_Church, Wiccans, Vampire_Clan, Bandits, Ratmen, Vagrants.
- `BIOMES`: GRASSLAND, FOREST, SNOW, DESERT. `ELEVATION`: PLAIN, HILLS, MOUNTAINS.

## Generation → placement → query (one glance)
```
LandmarkManager.GenerateRegionMap(Region)
  → RegionInnerTileMap.GenerateMap()  → LocationGridTile[,]
  → Region.GenerateStructures()       → LocationStructure.tiles
  → GridTileTileObjectComponent.SetObjectHere(TileObject)  (updates passability)
  → InnerMapManager registers for render
query: GetTileFromMousePosition() → LocationGridTile
        → .structure / .tileObjectComponent.objHere
```
