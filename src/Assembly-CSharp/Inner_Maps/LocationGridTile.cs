using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using PathFind;
using Pathfinding;
using Traits;
using UnityEngine;
using UnityEngine.Tilemaps;
using UtilityScripts;

namespace Inner_Maps;

public class LocationGridTile : IHasNeighbours<LocationGridTile>, ISavable
{
	public enum Tile_Type
	{
		Empty,
		Wall
	}

	public enum Tile_State
	{
		Empty,
		Occupied
	}

	public enum Ground_Type
	{
		Soil,
		Grass,
		Stone,
		Snow,
		Tundra,
		Cobble,
		Wood,
		Snow_Dirt,
		Cave,
		Corrupted,
		Desert_Grass,
		Sand,
		Desert_Stone,
		Bone,
		Demon_Stone,
		Flesh,
		Structure_Stone,
		Ruined_Stone,
		Magma,
		Blight,
		Water_Deep,
		Water_Mid,
		Water_Shallow,
		Water_Shore
	}

	private static readonly GridNeighbourDirection[] gridNeighbourDirections = CollectionUtilities.GetEnumValues<GridNeighbourDirection>();

	private Dictionary<GridNeighbourDirection, Point> possibleExits = new Dictionary<GridNeighbourDirection, Point>
	{
		{
			GridNeighbourDirection.North,
			new Point(0, 1)
		},
		{
			GridNeighbourDirection.South,
			new Point(0, -1)
		},
		{
			GridNeighbourDirection.West,
			new Point(-1, 0)
		},
		{
			GridNeighbourDirection.East,
			new Point(1, 0)
		},
		{
			GridNeighbourDirection.North_West,
			new Point(-1, 1)
		},
		{
			GridNeighbourDirection.North_East,
			new Point(1, 1)
		},
		{
			GridNeighbourDirection.South_West,
			new Point(-1, -1)
		},
		{
			GridNeighbourDirection.South_East,
			new Point(1, -1)
		}
	};

	private PointFloat[] nodePoints = new PointFloat[4]
	{
		new PointFloat(0.25f, 0.25f),
		new PointFloat(-0.25f, 0.25f),
		new PointFloat(0.25f, -0.25f),
		new PointFloat(-0.25f, -0.25f)
	};

	private Dictionary<GridNeighbourDirection, LocationGridTile> _neighbours;

	private Dictionary<GridNeighbourDirection, LocationGridTile> _fourNeighbours;

	private List<LocationGridTile> _fourNeighboursList;

	private int _walkedOnCount;

	public string persistentID { get; }

	public InnerTileMap parentMap { get; private set; }

	public Tilemap parentTileMap { get; private set; }

	public Area area { get; }

	public Vector3Int localPlace { get; }

	public Vector3 worldLocation { get; private set; }

	public Vector3 centeredWorldLocation { get; private set; }

	public Vector3 localLocation { get; }

	public Vector3 centeredLocalLocation { get; }

	public Tile_Type tileType { get; private set; }

	public Tile_State tileState { get; private set; }

	public Ground_Type groundType { get; private set; }

	public BIOMES mainBiomeType { get; private set; }

	public ELEVATION elevationType { get; private set; }

	public LocationStructure structure { get; private set; }

	public List<LocationGridTile> neighbourList { get; private set; }

	public List<Character> charactersHere { get; private set; }

	public bool hasBlueprint { get; private set; }

	public int connectorsOnTile { get; private set; }

	public float floorSample { get; private set; }

	public int nonDefaultCounter { get; private set; }

	public Ground_Type initialGroundType { get; private set; }

	public Biome_Tile_Type specificBiomeTileType { get; private set; }

	public string groundTileMapAssetName { get; private set; }

	public string wallTileMapAssetName { get; private set; }

	public string shoreTileMapAssetName { get; set; }

	public int blueprintTileReferenceXPos { get; private set; }

	public int blueprintTileReferenceYPos { get; private set; }

	public GameDate scheduleToRevert { get; private set; }

	public GridTileCorruptionComponent corruptionComponent { get; private set; }

	public GridTileMouseEventsComponent mouseEventsComponent { get; private set; }

	public GridTileTileObjectComponent tileObjectComponent { get; private set; }

	public GridTileEventDispatcher eventDispatcher { get; private set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Gridtile;

	public Type serializedData => typeof(SaveDataLocationGridTile);

	public bool isOccupied => tileState == Tile_State.Occupied;

	public int walkedOnCount => _walkedOnCount;

	public bool isDefault => nonDefaultCounter <= 0;

	public int meteorCount { get; private set; }

	public int nonPlayerMeteorCount { get; private set; }

	private Dictionary<GridNeighbourDirection, LocationGridTile> FourNeighboursDictionary()
	{
		return _fourNeighbours;
	}

	public LocationGridTile(int x, int y, Tilemap tilemap, InnerTileMap p_parentMap, Area p_area)
	{
		persistentID = Guid.NewGuid().ToString();
		parentMap = p_parentMap;
		parentTileMap = tilemap;
		area = p_area;
		localPlace = new Vector3Int(x, y, 0);
		worldLocation = tilemap.CellToWorld(localPlace);
		localLocation = tilemap.CellToLocal(localPlace);
		centeredLocalLocation = new Vector3(localLocation.x + 0.5f, localLocation.y + 0.5f, localLocation.z);
		centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
		tileType = Tile_Type.Empty;
		tileState = Tile_State.Empty;
		charactersHere = new List<Character>(10);
		_fourNeighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>(4);
		_neighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>(9);
		neighbourList = new List<LocationGridTile>(9);
		nonDefaultCounter = 0;
		connectorsOnTile = 0;
		mainBiomeType = BIOMES.NONE;
		elevationType = ELEVATION.PLAIN;
		corruptionComponent = new GridTileCorruptionComponent();
		corruptionComponent.SetOwner(this);
		mouseEventsComponent = new GridTileMouseEventsComponent();
		mouseEventsComponent.SetOwner(this);
		tileObjectComponent = new GridTileTileObjectComponent();
		tileObjectComponent.SetOwner(this);
		eventDispatcher = new GridTileEventDispatcher();
		eventDispatcher.SetOwner(this);
		DatabaseManager.Instance.locationGridTileDatabase.RegisterTile(this);
	}

	public LocationGridTile(SaveDataLocationGridTile data, Tilemap tilemap, InnerTileMap p_parentMap, Area p_area)
	{
		persistentID = data.persistentID;
		parentMap = p_parentMap;
		parentTileMap = tilemap;
		area = p_area;
		groundTileMapAssetName = data.groundTileMapAssetName;
		wallTileMapAssetName = data.wallTileMapAssetName;
		shoreTileMapAssetName = data.shoreTileMapAssetName;
		localPlace = new Vector3Int((int)data.localPlace.x, (int)data.localPlace.y, 0);
		worldLocation = data.worldLocation;
		localLocation = data.localLocation;
		centeredLocalLocation = data.centeredLocalLocation;
		centeredWorldLocation = data.centeredWorldLocation;
		tileType = data.tileType;
		tileState = data.tileState;
		charactersHere = new List<Character>();
		_fourNeighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>(4);
		_neighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>(9);
		neighbourList = new List<LocationGridTile>(9);
		nonDefaultCounter = data.nonDefaultCounter;
		connectorsOnTile = data.connectorsCount;
		corruptionComponent = data.corruptionComponent.Load();
		corruptionComponent.SetOwner(this);
		mouseEventsComponent = data.mouseEventsComponent.Load();
		mouseEventsComponent.SetOwner(this);
		tileObjectComponent = data.tileObjectComponent.Load();
		tileObjectComponent.SetOwner(this);
		eventDispatcher = new GridTileEventDispatcher();
		eventDispatcher.SetOwner(this);
		elevationType = data.elevation;
		_walkedOnCount = data.walkedOnCount;
		scheduleToRevert = data.scheduleToRevert;
		meteorCount = data.meteorCount;
		nonPlayerMeteorCount = data.nonPlayerMeteorCount;
		DatabaseManager.Instance.locationGridTileDatabase.RegisterTile(this);
	}

	public void LoadSecondWave(SaveDataLocationGridTile saveDataLocationGridTile)
	{
		for (int i = 0; i < meteorCount; i++)
		{
			AddMeteorFromSave();
		}
		for (int j = 0; j < nonPlayerMeteorCount; j++)
		{
			AddNonPlayerMeteorFromSave();
		}
		corruptionComponent.LoadSecondWave();
		tileObjectComponent.LoadSecondWave();
		if (scheduleToRevert.hasValue)
		{
			SchedulingManager.Instance.AddEntry(scheduleToRevert, RevertTileToOriginalGround, null);
		}
	}

	public void SetTileType(Tile_Type tileType)
	{
		this.tileType = tileType;
	}

	public void SetGroundType(Ground_Type newGroundType, bool isInitial = false)
	{
		Ground_Type ground_Type = groundType;
		groundType = newGroundType;
		if (tileObjectComponent.genericTileObject != null)
		{
			switch (newGroundType)
			{
			case Ground_Type.Grass:
			case Ground_Type.Wood:
			case Ground_Type.Desert_Grass:
			case Ground_Type.Sand:
			case Ground_Type.Demon_Stone:
			case Ground_Type.Structure_Stone:
			case Ground_Type.Ruined_Stone:
				tileObjectComponent.genericTileObject.traitContainer.AddTrait(tileObjectComponent.genericTileObject, "Flammable");
				break;
			case Ground_Type.Snow:
				tileObjectComponent.genericTileObject.traitContainer.RemoveTrait(tileObjectComponent.genericTileObject, "Flammable");
				break;
			default:
				tileObjectComponent.genericTileObject.traitContainer.RemoveTrait(tileObjectComponent.genericTileObject, "Flammable");
				break;
			}
		}
		if (GameManager.Instance.gameHasStarted && ground_Type == Ground_Type.Snow && newGroundType == Ground_Type.Tundra)
		{
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(UnityEngine.Random.Range(1, 4)));
			SchedulingManager.Instance.AddEntry(gameDate, RevertBackToSnow, this);
		}
		if (!isInitial && initialGroundType != newGroundType)
		{
			SetIsDefault(state: false);
		}
		if (area != null)
		{
			area.gridTileComponent.EvaluatePassabilityOfTile(this);
		}
	}

	private void RevertBackToSnow()
	{
		SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.snowTile, updateEdges: true);
	}

	public void UpdateWorldLocation()
	{
		worldLocation = parentTileMap.CellToWorld(localPlace);
		centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
	}

	public void SetInitialGroundType(Ground_Type groundType)
	{
		initialGroundType = groundType;
	}

	public void SetFloorSample(float floorSample)
	{
		this.floorSample = floorSample;
	}

	private Ground_Type GetGroundTypeBasedOnCurrentAsset()
	{
		Sprite sprite = parentMap.groundTilemap.GetSprite(localPlace);
		Sprite sprite2 = parentMap.elevationTilemap.GetSprite(localPlace);
		if ((object)sprite2 == null)
		{
			sprite2 = parentMap.shoreTilemap.GetSprite(localPlace);
		}
		if ((object)sprite2 != null)
		{
			string text = sprite2.name.ToLower();
			if (text.Contains("laid"))
			{
				return Ground_Type.Flesh;
			}
			if (text.Contains("dungeon") || text.Contains("cave"))
			{
				return Ground_Type.Cave;
			}
			if (text.Contains("water") || text.Contains("pond") || text.Contains("shore"))
			{
				if (text.Contains("deep"))
				{
					return Ground_Type.Water_Deep;
				}
				if (text.Contains("mid"))
				{
					return Ground_Type.Water_Mid;
				}
				if (text.Contains("shallow"))
				{
					return Ground_Type.Water_Shallow;
				}
				if (text.Contains("shore"))
				{
					return Ground_Type.Water_Shore;
				}
				return Ground_Type.Water_Deep;
			}
		}
		if ((object)sprite != null)
		{
			string text2 = sprite.name.ToLower();
			if (text2.Contains("desert"))
			{
				if (text2.Contains("grass"))
				{
					return Ground_Type.Desert_Grass;
				}
				if (text2.Contains("sand"))
				{
					return Ground_Type.Sand;
				}
				if (text2.Contains("rocks"))
				{
					return Ground_Type.Desert_Stone;
				}
			}
			else
			{
				if (text2.Contains("corruption") || text2.Contains("corrupted"))
				{
					return Ground_Type.Corrupted;
				}
				if (text2.Contains("bone"))
				{
					return Ground_Type.Bone;
				}
				if (text2.Contains("structure floor") || text2.Contains("wood") || text2.Contains("elven"))
				{
					return Ground_Type.Wood;
				}
				if (text2.Contains("cobble"))
				{
					return Ground_Type.Cobble;
				}
				if (text2.Contains("dirt") || text2.Contains("soil") || text2.Contains("outside") || text2.Contains("snow"))
				{
					if (text2.Contains("dirtsnow"))
					{
						return Ground_Type.Snow_Dirt;
					}
					if (text2.Contains("snow"))
					{
						return Ground_Type.Snow;
					}
					if (text2.Contains("tundra"))
					{
						return Ground_Type.Tundra;
					}
					return Ground_Type.Soil;
				}
				if (text2.Contains("stone") || text2.Contains("road"))
				{
					if (text2.Contains("demon"))
					{
						return Ground_Type.Demon_Stone;
					}
					if (text2.Contains("floor"))
					{
						return Ground_Type.Structure_Stone;
					}
					return Ground_Type.Stone;
				}
				if (text2.Contains("ruins"))
				{
					return Ground_Type.Ruined_Stone;
				}
				if (text2.Contains("grass") || text2.Contains("forest"))
				{
					return Ground_Type.Grass;
				}
				if (text2.Contains("tundra"))
				{
					return Ground_Type.Tundra;
				}
				if (text2.Contains("flesh"))
				{
					return Ground_Type.Flesh;
				}
				if (text2.Contains("fiery") || text2.Contains("magma"))
				{
					return Ground_Type.Magma;
				}
				if (text2.Contains("blight"))
				{
					return Ground_Type.Blight;
				}
				if (text2.Contains("divine"))
				{
					return Ground_Type.Structure_Stone;
				}
				if (text2.Contains("nature"))
				{
					return Ground_Type.Structure_Stone;
				}
			}
		}
		throw new Exception($"There is no ground type for ground asset: {sprite} or structure asset: {sprite2}");
	}

	public void UpdateGroundTypeBasedOnAsset()
	{
		Ground_Type groundTypeBasedOnCurrentAsset = GetGroundTypeBasedOnCurrentAsset();
		SetGroundType(groundTypeBasedOnCurrentAsset);
	}

	public void InitialUpdateGroundTypeBasedOnAsset()
	{
		Ground_Type groundTypeBasedOnCurrentAsset = GetGroundTypeBasedOnCurrentAsset();
		SetGroundType(groundTypeBasedOnCurrentAsset, isInitial: true);
		SetInitialGroundType(groundTypeBasedOnCurrentAsset);
	}

	public void SetGroundTilemapVisual(TileBase tileBase, bool updateEdges = false)
	{
		SetGroundTilemapTileAsset(tileBase);
		if (tileObjectComponent.genericTileObject.mapObjectVisual != null && tileObjectComponent.genericTileObject.mapObjectVisual.usedSprite != null)
		{
			tileObjectComponent.genericTileObject.mapObjectVisual.SetVisual(parentMap.groundTilemap.GetSprite(localPlace));
		}
		UpdateGroundTypeBasedOnAsset();
		if (updateEdges)
		{
			CreateSeamlessEdgesForSelfAndNeighbours();
		}
	}

	public void SetStructureTilemapVisual(TileBase tileBase)
	{
		SetElevationTilemapTileAsset(tileBase);
		UpdateGroundTypeBasedOnAsset();
	}

	public void SetGroundTilemapTileAsset(TileBase tileBase)
	{
		parentMap.groundTilemap.SetTile(localPlace, tileBase);
		UpdateGroundTileMapAssetName();
	}

	public void SetElevationTilemapTileAsset(TileBase tileBase)
	{
		parentMap.elevationTilemap.SetTile(localPlace, tileBase);
		UpdateWallTileMapAssetName();
	}

	public void UpdateGroundTileMapAssetNameForBatchedTileSetting()
	{
		UpdateGroundTileMapAssetName();
	}

	public void UpdateWallTileMapAssetNameForBatchedTileSetting()
	{
		UpdateWallTileMapAssetName();
	}

	private void UpdateGroundTileMapAssetName()
	{
		groundTileMapAssetName = parentMap.groundTilemap.GetTile(localPlace)?.name ?? string.Empty;
	}

	private void UpdateWallTileMapAssetName()
	{
		wallTileMapAssetName = parentMap.elevationTilemap.GetTile(localPlace)?.name ?? string.Empty;
	}

	public void UpdateShoreTileMapAssetName()
	{
		shoreTileMapAssetName = parentMap.shoreTilemap.GetTile(localPlace)?.name ?? string.Empty;
	}

	public void CreateSeamlessEdgesForSelfAndNeighbours()
	{
		CreateSeamlessEdgesForTile(parentMap);
		for (int i = 0; i < neighbourList.Count; i++)
		{
			neighbourList[i].CreateSeamlessEdgesForTile(parentMap);
		}
	}

	private int GetGroundTypeLayer(Ground_Type p_groundType)
	{
		switch (p_groundType)
		{
		case Ground_Type.Water_Deep:
			return 16;
		case Ground_Type.Water_Mid:
			return 15;
		case Ground_Type.Water_Shallow:
			return 14;
		case Ground_Type.Water_Shore:
			return 13;
		case Ground_Type.Corrupted:
			return 12;
		case Ground_Type.Snow:
			return 11;
		case Ground_Type.Tundra:
			return 10;
		case Ground_Type.Snow_Dirt:
			return 9;
		case Ground_Type.Grass:
			return 8;
		case Ground_Type.Desert_Grass:
			return 7;
		case Ground_Type.Sand:
			return 6;
		case Ground_Type.Stone:
		case Ground_Type.Cave:
		case Ground_Type.Desert_Stone:
		case Ground_Type.Demon_Stone:
		case Ground_Type.Magma:
			return 5;
		case Ground_Type.Cobble:
		case Ground_Type.Wood:
		case Ground_Type.Structure_Stone:
		case Ground_Type.Ruined_Stone:
			return 4;
		case Ground_Type.Blight:
			return 3;
		case Ground_Type.Bone:
		case Ground_Type.Flesh:
			return 2;
		case Ground_Type.Soil:
			return 1;
		default:
			return 0;
		}
	}

	public void CreateSeamlessEdgesForTile(InnerTileMap map)
	{
		_ = string.Empty;
		if (HasCardinalNeighbourOfDifferentGroundType())
		{
			BIOMES biomeOfGroundType = GetBiomeOfGroundType(groundType);
			{
				foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> item in FourNeighboursDictionary())
				{
					LocationGridTile value = item.Value;
					bool flag = false;
					BIOMES biomeOfGroundType2 = GetBiomeOfGroundType(value.groundType);
					if ((value.tileType == Tile_Type.Wall && (value.groundType != Ground_Type.Flesh || value.structure.structureType == STRUCTURE_TYPE.BEAST_LAIR)) || (value.IsWater() && !IsWater()))
					{
						flag = false;
					}
					else if (biomeOfGroundType != biomeOfGroundType2 && biomeOfGroundType != BIOMES.NONE && biomeOfGroundType2 != BIOMES.NONE)
					{
						switch (biomeOfGroundType)
						{
						case BIOMES.SNOW:
							flag = true;
							break;
						case BIOMES.GRASSLAND:
							if (biomeOfGroundType2 == BIOMES.DESERT)
							{
								flag = true;
							}
							break;
						}
					}
					else
					{
						int groundTypeLayer = GetGroundTypeLayer(groundType);
						int groundTypeLayer2 = GetGroundTypeLayer(value.groundType);
						if (groundTypeLayer > groundTypeLayer2)
						{
							flag = true;
						}
					}
					Tilemap tilemap = item.Key switch
					{
						GridNeighbourDirection.North => map.northEdgeTilemap, 
						GridNeighbourDirection.South => map.southEdgeTilemap, 
						GridNeighbourDirection.West => map.westEdgeTilemap, 
						GridNeighbourDirection.East => map.eastEdgeTilemap, 
						_ => null, 
					};
					if (flag)
					{
						if (InnerMapManager.Instance.assetManager.edgeAssets.ContainsKey(groundType) && InnerMapManager.Instance.assetManager.edgeAssets[groundType].Count > (int)item.Key)
						{
							tilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.edgeAssets[groundType][(int)item.Key]);
						}
						else
						{
							tilemap.SetTile(localPlace, null);
						}
					}
					else
					{
						tilemap.SetTile(localPlace, null);
					}
				}
				return;
			}
		}
		ClearAllSeamlessEdges(map);
	}

	public void ClearAllSeamlessEdges(InnerTileMap map)
	{
		map.northEdgeTilemap.SetTile(localPlace, null);
		map.southEdgeTilemap.SetTile(localPlace, null);
		map.westEdgeTilemap.SetTile(localPlace, null);
		map.eastEdgeTilemap.SetTile(localPlace, null);
	}

	private BIOMES GetBiomeOfGroundType(Ground_Type groundType)
	{
		switch (groundType)
		{
		case Ground_Type.Desert_Grass:
		case Ground_Type.Sand:
		case Ground_Type.Desert_Stone:
			return BIOMES.DESERT;
		case Ground_Type.Snow:
		case Ground_Type.Tundra:
		case Ground_Type.Snow_Dirt:
			return BIOMES.SNOW;
		case Ground_Type.Soil:
		case Ground_Type.Grass:
			return BIOMES.GRASSLAND;
		default:
			return BIOMES.NONE;
		}
	}

	public void RevertTileToOriginalPerlin()
	{
		bool isCorrupted = corruptionComponent.isCorrupted;
		TileBase groundAssetForTile = InnerMapManager.Instance.assetManager.GetGroundAssetForTile(this);
		SetGroundTilemapVisual(groundAssetForTile);
		if (isCorrupted && !corruptionComponent.isCorrupted)
		{
			PlayerManager.Instance.player.playerSettlement.RemoveCorruptedTile(this);
		}
	}

	public void DetermineNextGroundTypeAfterDestruction()
	{
		TileBase tileBase;
		switch (groundType)
		{
		case Ground_Type.Water_Deep:
		case Ground_Type.Water_Mid:
		case Ground_Type.Water_Shallow:
		case Ground_Type.Water_Shore:
			tileBase = InnerMapManager.Instance.assetManager.soilTile;
			break;
		case Ground_Type.Snow:
		case Ground_Type.Snow_Dirt:
			tileBase = InnerMapManager.Instance.assetManager.tundraTile;
			break;
		case Ground_Type.Cobble:
		case Ground_Type.Wood:
		case Ground_Type.Cave:
		case Ground_Type.Bone:
		case Ground_Type.Demon_Stone:
		case Ground_Type.Flesh:
		case Ground_Type.Structure_Stone:
		case Ground_Type.Ruined_Stone:
		case Ground_Type.Blight:
			tileBase = InnerTileMap.GetGroundAssetPerlin(floorSample, mainBiomeType);
			break;
		case Ground_Type.Desert_Grass:
		case Ground_Type.Sand:
			tileBase = InnerMapManager.Instance.assetManager.desertStoneGroundTile;
			break;
		default:
			tileBase = null;
			break;
		}
		if (tileBase != null)
		{
			SetGroundTilemapVisual(tileBase);
			CreateSeamlessEdgesForSelfAndNeighbours();
		}
	}

	public void ScheduleRevertTileToOriginalGround()
	{
		if (!scheduleToRevert.hasValue)
		{
			scheduleToRevert = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(24));
			SetIsDefault(state: false);
			SchedulingManager.Instance.AddEntry(scheduleToRevert, RevertTileToOriginalGround, null);
		}
	}

	private void RevertTileToOriginalGround()
	{
		GameDate gameDate = scheduleToRevert;
		gameDate.hasValue = false;
		scheduleToRevert = gameDate;
		SetIsDefault(state: true);
		if (structure.structureType == STRUCTURE_TYPE.WILDERNESS && !corruptionComponent.isCorrupted)
		{
			RevertTileToOriginalPerlin();
			CreateSeamlessEdgesForSelfAndNeighbours();
			UpdateMinimapVisual(GridMap.Instance.mainRegion.wilderness);
		}
	}

	public void SetStructure(LocationStructure p_structure)
	{
		LocationStructure locationStructure = structure;
		locationStructure?.RemoveTile(this);
		structure = p_structure;
		structure.AddTile(this);
		if (tileObjectComponent.objHere != null)
		{
			LocationAwarenessUtility.RemoveFromAwarenessList(tileObjectComponent.objHere);
			LocationAwarenessUtility.AddToAwarenessList(tileObjectComponent.objHere, this);
			structure.OnlyAddPOIToList(tileObjectComponent.objHere);
		}
		if (locationStructure != structure)
		{
			for (int i = 0; i < charactersHere.Count; i++)
			{
				Character character = charactersHere[i];
				structure.AddCharacterAtLocation(character);
			}
		}
		eventDispatcher.ExecuteTileChangedStructureEvent(structure, locationStructure, this);
	}

	public void SetTileState(Tile_State state)
	{
		if (structure != null)
		{
			if (tileState == Tile_State.Empty && state == Tile_State.Occupied)
			{
				structure.RemoveUnoccupiedTile(this);
			}
			else if (tileState == Tile_State.Occupied && state == Tile_State.Empty)
			{
				structure.AddUnoccupiedTile(this);
			}
		}
		tileState = state;
	}

	public void InstantPlaceDemonicStructure(StructureSetting p_structureSetting)
	{
		GameObject randomElement = CollectionUtilities.GetRandomElement(InnerMapManager.Instance.GetStructurePrefabsForStructure(FACTION_TYPE.Demons, p_structureSetting.structureType, p_structureSetting.resource));
		tileObjectComponent.genericTileObject.InstantPlaceStructure(randomElement.name, PlayerManager.Instance.player.playerSettlement);
	}

	public void PlaceSelfBuildingStructure(StructureSetting p_structureSetting, int p_buildingTimeInTicks)
	{
		if (p_buildingTimeInTicks > 0)
		{
			GameObject randomElement = CollectionUtilities.GetRandomElement(InnerMapManager.Instance.GetStructurePrefabsForStructure(FACTION_TYPE.Demons, p_structureSetting.structureType, p_structureSetting.resource));
			tileObjectComponent.genericTileObject.PlaceSelfBuildingStructure(randomElement.name, PlayerManager.Instance.player.playerSettlement, p_buildingTimeInTicks);
		}
		else
		{
			InstantPlaceDemonicStructure(p_structureSetting);
		}
	}

	public void AddCharacterHere(Character character)
	{
		if (!charactersHere.Contains(character))
		{
			eventDispatcher.ExecuteCharacterEnteredTile(character, this);
		}
		charactersHere.Add(character);
		if (tileObjectComponent.genericTileObject != null)
		{
			List<Trait> traitOverrideFunctions = tileObjectComponent.genericTileObject.traitContainer.GetTraitOverrideFunctions("Enter_Grid_Tile_Trait");
			if (traitOverrideFunctions != null)
			{
				for (int i = 0; i < traitOverrideFunctions.Count; i++)
				{
					traitOverrideFunctions[i].OnEnterGridTile(character, tileObjectComponent.genericTileObject);
				}
			}
		}
		if (tileObjectComponent.hasLandmine && tileObjectComponent.genericTileObject != null)
		{
			Landmined traitOrStatus = tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Landmined>("Landmined");
			if (traitOrStatus != null && !tileObjectComponent.IsImmuneToLandmine(character, traitOrStatus))
			{
				GameManager.Instance.StartCoroutine(tileObjectComponent.TriggerLandmine(character));
			}
		}
		if (!character.movementComponent.cameFromWurmHole)
		{
			if (tileObjectComponent.objHere != null && tileObjectComponent.objHere is WurmHole wurmHole)
			{
				bool flag = true;
				if (character.carryComponent.isBeingCarriedBy != null)
				{
					if (character.carryComponent.isBeingCarriedBy.movementComponent.isFlying)
					{
						flag = false;
					}
				}
				else if (character.movementComponent.isFlying)
				{
					flag = false;
				}
				if (flag && wurmHole.wurmHoleConnection.gridTileLocation != null)
				{
					wurmHole.TravelThroughWurmHole(character);
					return;
				}
			}
		}
		else
		{
			character.movementComponent.SetCameFromWurmHole(state: false);
		}
		if (tileObjectComponent.objHere != null && tileObjectComponent.objHere is Rug rug && rug.traitContainer.HasTrait("Booby Trapped") && (!(character.currentActionNode != null) || character.currentActionNode.target != rug || character.currentActionNode.goapType != INTERACTION_TYPE.REMOVE_TRAP))
		{
			BoobyTrapped traitOrStatus2 = rug.traitContainer.GetTraitOrStatus<BoobyTrapped>("Booby Trapped");
			if (!traitOrStatus2.IsImmuneToTrap(character))
			{
				traitOrStatus2.DamageTargetByTrap(character, rug);
			}
		}
		if (tileObjectComponent.genericTileObject != null)
		{
			if (tileObjectComponent.hasFreezingTrap)
			{
				FreezingTrapped traitOrStatus3 = tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<FreezingTrapped>("Freezing Trapped");
				if (traitOrStatus3 != null && !tileObjectComponent.IsImmuneToFreezingTrap(character, traitOrStatus3))
				{
					tileObjectComponent.TriggerFreezingTrap(character);
				}
			}
			if (tileObjectComponent.hasSnareTrap)
			{
				SnareTrapped traitOrStatus4 = tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<SnareTrapped>("Snare Trapped");
				if (traitOrStatus4 != null && !tileObjectComponent.IsImmuneToSnareTrap(character, traitOrStatus4))
				{
					tileObjectComponent.TriggerSnareTrap(character);
				}
			}
		}
		if (GetCharacterWithRace(RACE.SCORPION) is Scorpion { isDead: false } scorpion && scorpion.limiterComponent.canPerform && scorpion.limiterComponent.canMove && scorpion != character && scorpion.heldCharacter == null && scorpion.isHidden && character.canBeTargetedByLandActions && !scorpion.hasPulledForTheDay)
		{
			scorpion.SetHasPulledForTheDay(p_state: true);
			scorpion.SetHeldCharacter(character);
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Pulled_Down, scorpion);
		}
		if (corruptionComponent.isCorrupted)
		{
			if (character.currentActionNode != null && character.currentActionNode.action.actionLocationType == ACTION_LOCATION_TYPE.ON_REACH_CORRUPTION)
			{
				if (character.hasMarker)
				{
					character.marker.pathfindingAI.ClearAllCurrentPathData();
				}
				character.PerformGoapAction();
			}
			if (!character.isDead && character.limiterComponent.canMove && character.limiterComponent.canPerform && !character.movementComponent.hasMovedOnCorruption && (bool)character.marker && character.marker.hasFleePath && character.isNormalCharacter && character.gridTileLocation != null)
			{
				character.movementComponent.SetHasMovedOnCorruption(state: true);
				character.marker.AddAvoidPositions(character.gridTileLocation.area.gridTileComponent.centerGridTile.worldLocation);
			}
		}
		else
		{
			character.movementComponent.SetHasMovedOnCorruption(state: false);
		}
		if (character.isNormalCharacter && character.hasMarker && character.marker.isMoving && !character.traitContainer.HasTrait("Wet") && tileObjectComponent.genericTileObject.traitContainer.HasTrait("Wet") && GameUtilities.RollChance(1.5f))
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Stumble_Knockout, character);
		}
	}

	public Character GetCharacterWithRace(RACE p_lookUprace)
	{
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			if (character.race == p_lookUprace)
			{
				return character;
			}
		}
		return null;
	}

	public void RemoveCharacterHere(Character character)
	{
		if (charactersHere.Remove(character))
		{
			eventDispatcher.ExecuteCharacterLeftTile(character, this);
		}
	}

	public bool IsInHomeOf(Character character)
	{
		if (character.homeSettlement != null)
		{
			return IsPartOfSettlement(character.homeSettlement);
		}
		if (character.homeStructure != null)
		{
			return structure == character.homeStructure;
		}
		if (character.HasTerritory())
		{
			return area == character.territory;
		}
		return false;
	}

	public LocationGridTile GetNeighbourAtDirection(GridNeighbourDirection dir)
	{
		if (_neighbours.ContainsKey(dir))
		{
			return _neighbours[dir];
		}
		return null;
	}

	public bool TryGetNeighbourDirection(LocationGridTile tile, out GridNeighbourDirection dir)
	{
		foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> neighbour in _neighbours)
		{
			if (neighbour.Value == tile)
			{
				dir = neighbour.Key;
				return true;
			}
		}
		dir = GridNeighbourDirection.East;
		return false;
	}

	public bool IsAtEdgeOfMap()
	{
		for (int i = 0; i < gridNeighbourDirections.Length; i++)
		{
			if (!_neighbours.ContainsKey(gridNeighbourDirections[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasDifferentDwellingOrOutsideNeighbour()
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			if (neighbourList[i].structure != structure)
			{
				return true;
			}
		}
		return false;
	}

	public override string ToString()
	{
		return localPlace.ToString();
	}

	public float GetDistanceTo(LocationGridTile tile)
	{
		return Vector2.Distance(localLocation, tile.localLocation);
	}

	public void FindNeighbours(LocationGridTile[,] map)
	{
		int upperBound = map.GetUpperBound(0);
		int upperBound2 = map.GetUpperBound(1);
		Point otherPoint = new Point(localPlace.x, localPlace.y);
		foreach (KeyValuePair<GridNeighbourDirection, Point> possibleExit in possibleExits)
		{
			GridNeighbourDirection key = possibleExit.Key;
			Point point = possibleExit.Value.Sum(otherPoint);
			if (Utilities.IsInRange(point.X, 0, upperBound + 1) && Utilities.IsInRange(point.Y, 0, upperBound2 + 1))
			{
				LocationGridTile locationGridTile = map[point.X, point.Y];
				_neighbours.Add(key, locationGridTile);
				neighbourList.Add(locationGridTile);
				if (key.IsCardinalDirection())
				{
					_fourNeighbours.Add(key, locationGridTile);
				}
			}
		}
	}

	public bool HasUnoccupiedNeighbour(bool sameStructureOnly = false)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (!locationGridTile.isOccupied && (!sameStructureOnly || locationGridTile.structure == structure))
			{
				return true;
			}
		}
		return false;
	}

	public void PopulateUnoccupiedNeighbours(List<LocationGridTile> neighbours, bool sameStructureOnly = false)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (!locationGridTile.isOccupied && (!sameStructureOnly || locationGridTile.structure == structure))
			{
				neighbours.Add(locationGridTile);
			}
		}
	}

	public void PopulateUnoccupiedNeighboursWithNoCharactersInSameAreaAndStructure(List<LocationGridTile> neighbours)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (!locationGridTile.isOccupied && locationGridTile.charactersHere.Count <= 0 && locationGridTile.structure == structure && locationGridTile.area == area)
			{
				neighbours.Add(locationGridTile);
			}
		}
	}

	public void PopulateNeighboursWithNoCharacters(List<LocationGridTile> neighbours, bool sameStructureOnly = false)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (locationGridTile.charactersHere.Count <= 0 && (!sameStructureOnly || locationGridTile.structure == structure))
			{
				neighbours.Add(locationGridTile);
			}
		}
	}

	public void PopulateUnoccupiedNeighboursThatIsSameStructureAs(List<LocationGridTile> neighbours, LocationStructure structure)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (!locationGridTile.isOccupied && locationGridTile.structure == structure)
			{
				neighbours.Add(locationGridTile);
			}
		}
	}

	public void PopulateNeighbours(List<LocationGridTile> neighbours)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile item = neighbourList[i];
			neighbours.Add(item);
		}
	}

	public bool HasNeighbourOfElevation(ELEVATION elevation, bool useFourNeighbours = false)
	{
		Dictionary<GridNeighbourDirection, LocationGridTile> dictionary = _neighbours;
		if (useFourNeighbours)
		{
			dictionary = FourNeighboursDictionary();
		}
		for (int i = 0; i < dictionary.Values.Count; i++)
		{
			if (_neighbours.Values.ElementAt(i).area.elevationType == elevation)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNeighbourOfType(Tile_Type type, bool useFourNeighbours = false)
	{
		Dictionary<GridNeighbourDirection, LocationGridTile> dictionary = _neighbours;
		if (useFourNeighbours)
		{
			dictionary = FourNeighboursDictionary();
		}
		for (int i = 0; i < dictionary.Values.Count; i++)
		{
			if (_neighbours.Values.ElementAt(i).tileType == type)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNeighbourThatIsWater(bool useFourNeighbours = false)
	{
		Dictionary<GridNeighbourDirection, LocationGridTile> dictionary = _neighbours;
		if (useFourNeighbours)
		{
			dictionary = FourNeighboursDictionary();
		}
		for (int i = 0; i < dictionary.Values.Count; i++)
		{
			if (_neighbours.Values.ElementAt(i).IsWater())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNeighbourOfType(Ground_Type type, bool useFourNeighbours = false)
	{
		Dictionary<GridNeighbourDirection, LocationGridTile> dictionary = _neighbours;
		if (useFourNeighbours)
		{
			dictionary = FourNeighboursDictionary();
		}
		for (int i = 0; i < dictionary.Values.Count; i++)
		{
			if (_neighbours.Values.ElementAt(i).groundType == type)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNeighbourNotInList(List<LocationGridTile> list, bool useFourNeighbours = false)
	{
		Dictionary<GridNeighbourDirection, LocationGridTile> dictionary = _neighbours;
		if (useFourNeighbours)
		{
			dictionary = FourNeighboursDictionary();
		}
		for (int i = 0; i < dictionary.Values.Count; i++)
		{
			if (!list.Contains(_neighbours.Values.ElementAt(i)))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasDifferentStructureNeighbour(bool useFourNeighbours = false)
	{
		Dictionary<GridNeighbourDirection, LocationGridTile> dictionary = _neighbours;
		if (useFourNeighbours)
		{
			dictionary = FourNeighboursDictionary();
		}
		for (int i = 0; i < dictionary.Values.Count; i++)
		{
			if (dictionary.Values.ElementAt(i).structure != structure)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNeighbouringWalledStructure()
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (locationGridTile.structure != null && !locationGridTile.structure.structureType.IsOpenSpace())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasVillageOrSpecialStructureNeighbour()
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (locationGridTile.structure.structureType != STRUCTURE_TYPE.WILDERNESS && !(locationGridTile.structure is DemonicStructure))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNeighbourStructure(STRUCTURE_TYPE p_type)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			if (neighbourList[i].structure.structureType == p_type)
			{
				return true;
			}
		}
		return false;
	}

	public int GetCountNeighboursOfType(Tile_Type type, bool useFourNeighbours = false)
	{
		int num = 0;
		Dictionary<GridNeighbourDirection, LocationGridTile> dictionary = _neighbours;
		if (useFourNeighbours)
		{
			dictionary = FourNeighboursDictionary();
		}
		for (int i = 0; i < dictionary.Values.Count; i++)
		{
			if (_neighbours.Values.ElementAt(i).tileType == type)
			{
				num++;
			}
		}
		return num;
	}

	public bool IsNeighbour(LocationGridTile tile, bool sameStructureOnly = false)
	{
		if (sameStructureOnly)
		{
			for (int i = 0; i < neighbourList.Count; i++)
			{
				if (neighbourList[i] == tile && ((structure.structureType.IsOpenSpace() && tile.structure.structureType.IsOpenSpace()) || structure == tile.structure))
				{
					return true;
				}
			}
		}
		else
		{
			for (int j = 0; j < neighbourList.Count; j++)
			{
				if (neighbourList[j] == tile)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAdjacentTo(Type type)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (locationGridTile.tileObjectComponent.objHere != null && locationGridTile.tileObjectComponent.objHere.GetType() == type)
			{
				return true;
			}
		}
		return false;
	}

	public List<LocationGridTile> FourNeighbours()
	{
		TryPopulateFourNeighboursList();
		return _fourNeighboursList;
	}

	public void PopulateFourNeighboursThatHasTileObjectOfType(List<LocationGridTile> neighbours, TILE_OBJECT_TYPE p_type)
	{
		List<LocationGridTile> list = FourNeighbours();
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			TileObject objHere = locationGridTile.tileObjectComponent.objHere;
			if (objHere != null && objHere.tileObjectType == p_type)
			{
				neighbours.Add(locationGridTile);
			}
		}
	}

	public void PopulateFourNeighboursValidTiles(List<LocationGridTile> neighbours)
	{
		List<LocationGridTile> list = FourNeighbours();
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			if (locationGridTile.tileType == Tile_Type.Empty)
			{
				neighbours.Add(locationGridTile);
			}
		}
	}

	public void PopulateNeighboursInSameStructureThatHasNoNeighbourInDifferentStructure(List<LocationGridTile> p_neighbours)
	{
		List<LocationGridTile> list = FourNeighbours();
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			if (locationGridTile.structure == structure && !locationGridTile.HasDifferentStructureNeighbour(useFourNeighbours: true))
			{
				p_neighbours.Add(locationGridTile);
			}
		}
	}

	public int GetCountOfFourNeighboursInStructureType(STRUCTURE_TYPE p_type)
	{
		int num = 0;
		List<LocationGridTile> list = FourNeighbours();
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].structure.structureType == p_type)
				{
					num++;
				}
			}
		}
		return num;
	}

	private void TryPopulateFourNeighboursList()
	{
		if (_fourNeighboursList == null)
		{
			if (_fourNeighboursList == null)
			{
				_fourNeighboursList = new List<LocationGridTile>(4);
			}
			for (int i = 0; i < _fourNeighbours.Values.Count; i++)
			{
				_fourNeighboursList.Add(_fourNeighbours.Values.ElementAt(i));
			}
		}
	}

	public int GetCountOfNeighboursInStructureType(STRUCTURE_TYPE p_type)
	{
		int num = 0;
		if (neighbourList != null)
		{
			for (int i = 0; i < neighbourList.Count; i++)
			{
				if (neighbourList[i].structure.structureType == p_type)
				{
					num++;
				}
			}
		}
		return num;
	}

	public int GetCountOfNeighboursThatHasTileObjectOfType(TILE_OBJECT_TYPE p_type)
	{
		int num = 0;
		if (neighbourList != null)
		{
			for (int i = 0; i < neighbourList.Count; i++)
			{
				TileObject objHere = neighbourList[i].tileObjectComponent.objHere;
				if (objHere != null && objHere.tileObjectType == p_type)
				{
					num++;
				}
			}
		}
		return num;
	}

	public LocationGridTile GetNeareastTileFromThisThatIsPassableOrHasNoWallsAndIsNotInOcean(List<LocationGridTile> checkedTiles)
	{
		if (!checkedTiles.Contains(this))
		{
			checkedTiles.Add(this);
			if ((IsPassable() || !tileObjectComponent.HasWalls()) && structure.structureType != STRUCTURE_TYPE.OCEAN)
			{
				return this;
			}
		}
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (!checkedTiles.Contains(locationGridTile))
			{
				checkedTiles.Add(locationGridTile);
				if ((locationGridTile.IsPassable() || !locationGridTile.tileObjectComponent.HasWalls()) && locationGridTile.structure.structureType != STRUCTURE_TYPE.OCEAN)
				{
					return locationGridTile;
				}
			}
		}
		for (int j = 0; j < neighbourList.Count; j++)
		{
			LocationGridTile neareastTileFromThisThatIsPassableOrHasNoWallsAndIsNotInOcean = neighbourList[j].GetNeareastTileFromThisThatIsPassableOrHasNoWallsAndIsNotInOcean(checkedTiles);
			if (neareastTileFromThisThatIsPassableOrHasNoWallsAndIsNotInOcean != null)
			{
				return neareastTileFromThisThatIsPassableOrHasNoWallsAndIsNotInOcean;
			}
		}
		return null;
	}

	public LocationStructure GetNearestInteriorStructureFromThisExcept(List<LocationStructure> exclusions)
	{
		LocationStructure locationStructure = null;
		if (structure != null && structure.region.allStructures.Count > 0)
		{
			float num = 99999f;
			for (int i = 0; i < structure.region.allStructures.Count; i++)
			{
				LocationStructure locationStructure2 = structure.region.allStructures[i];
				if (locationStructure2 == structure || !locationStructure2.isInterior || (exclusions != null && exclusions.Contains(locationStructure2)))
				{
					continue;
				}
				LocationGridTile randomPassableTile = locationStructure2.GetRandomPassableTile();
				if (randomPassableTile != null && PathfindingManager.Instance.HasPath(this, randomPassableTile))
				{
					float num2 = Vector2.Distance(randomPassableTile.localLocation, localLocation);
					if (locationStructure == null || num2 < num)
					{
						locationStructure = locationStructure2;
						num = num2;
					}
				}
			}
		}
		return locationStructure;
	}

	public LocationStructure GetNearestVillageStructureFromThisWithResidents(Character relativeTo = null)
	{
		LocationStructure locationStructure = null;
		if (structure != null && structure.region.allStructures.Count > 0)
		{
			float num = 99999f;
			for (int i = 0; i < structure.region.allStructures.Count; i++)
			{
				LocationStructure locationStructure2 = structure.region.allStructures[i];
				if (locationStructure2 == structure || locationStructure2.settlementLocation == null || locationStructure2.settlementLocation is PlayerSettlement || locationStructure2.settlementLocation.owner == null || locationStructure2.settlementLocation.residents.Count <= 0 || !locationStructure2.settlementLocation.owner.isMajorNonPlayer)
				{
					continue;
				}
				LocationGridTile randomPassableTile = locationStructure2.GetRandomPassableTile();
				if (randomPassableTile != null && ((relativeTo != null && relativeTo.movementComponent.HasPathTo(randomPassableTile)) || PathfindingManager.Instance.HasPath(this, randomPassableTile)))
				{
					float num2 = Vector2.Distance(randomPassableTile.localLocation, localLocation);
					if (locationStructure == null || num2 < num)
					{
						locationStructure = locationStructure2;
						num = num2;
					}
				}
			}
		}
		return locationStructure;
	}

	public LocationGridTile GetNearestUnoccupiedTileFromThisWithStructure(STRUCTURE_TYPE structureType)
	{
		LocationGridTile result = null;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		PopulateUnoccupiedNeighbours(list, sameStructureOnly: true);
		if (list.Count == 0)
		{
			if (structure != null)
			{
				LocationGridTile locationGridTile = null;
				float num = 99999f;
				for (int i = 0; i < structure.unoccupiedTiles.Count; i++)
				{
					LocationGridTile locationGridTile2 = structure.unoccupiedTiles.ElementAt(i);
					if (locationGridTile2 != this && !locationGridTile2.IsWater() && locationGridTile2.structure != null && locationGridTile2.structure.structureType == structureType)
					{
						float num2 = Vector2.Distance(locationGridTile2.localLocation, localLocation);
						if (num2 < num)
						{
							locationGridTile = locationGridTile2;
							num = num2;
						}
					}
				}
				result = locationGridTile;
			}
		}
		else
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetNearestEdgeTileFromThis()
	{
		if (IsAtEdgeOfWalkableMap())
		{
			return this;
		}
		LocationGridTile locationGridTile = null;
		List<LocationGridTile> list = neighbourList;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsAtEdgeOfWalkableMap())
			{
				locationGridTile = list[i];
				break;
			}
		}
		if (locationGridTile == null)
		{
			float num = -999f;
			for (int j = 0; j < parentMap.allEdgeTiles.Count; j++)
			{
				LocationGridTile locationGridTile2 = parentMap.allEdgeTiles[j];
				float num2 = Vector2.Distance(locationGridTile2.localLocation, localLocation);
				if (num == -999f || num2 < num)
				{
					locationGridTile = locationGridTile2;
					num = num2;
				}
			}
		}
		return locationGridTile;
	}

	public LocationGridTile GetRandomUnoccupiedNeighbor()
	{
		LocationGridTile result = null;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		PopulateUnoccupiedNeighbours(list, sameStructureOnly: true);
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetFirstNoObjectNeighbor(bool thisStructureOnly = false)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if ((!thisStructureOnly || locationGridTile.structure == structure) && locationGridTile.tileObjectComponent.objHere == null && locationGridTile.IsPassable())
			{
				return locationGridTile;
			}
		}
		return null;
	}

	public LocationGridTile GetFirstNearestTileFromThisWithNoObject(bool thisStructureOnly = false, LocationGridTile exception = null)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		LocationGridTile firstNearestTileFromThisWithNoObjectBase = GetFirstNearestTileFromThisWithNoObjectBase(thisStructureOnly, list, exception);
		RuinarchListPool<LocationGridTile>.Release(list);
		return firstNearestTileFromThisWithNoObjectBase;
	}

	private LocationGridTile GetFirstNearestTileFromThisWithNoObjectBase(bool thisStructureOnly, List<LocationGridTile> checkedTiles, LocationGridTile exception)
	{
		if (!checkedTiles.Contains(this))
		{
			checkedTiles.Add(this);
			if (tileObjectComponent.objHere == null && IsPassable() && this != exception)
			{
				return this;
			}
			LocationGridTile firstNoObjectNeighbor = GetFirstNoObjectNeighbor(thisStructureOnly);
			if (firstNoObjectNeighbor != null)
			{
				return firstNoObjectNeighbor;
			}
			for (int i = 0; i < neighbourList.Count; i++)
			{
				LocationGridTile locationGridTile = neighbourList[i];
				if (locationGridTile != exception && (!thisStructureOnly || locationGridTile.structure == structure))
				{
					firstNoObjectNeighbor = locationGridTile.GetFirstNearestTileFromThisWithNoObjectBase(thisStructureOnly, checkedTiles, exception);
					if (firstNoObjectNeighbor != null)
					{
						return firstNoObjectNeighbor;
					}
				}
			}
		}
		return null;
	}

	public LocationGridTile GetRandomNeighborWithoutCharacters()
	{
		LocationGridTile result = null;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		PopulateNeighboursWithNoCharacters(list, sameStructureOnly: true);
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		return result;
	}

	public LocationGridTile GetFirstNeighbor(bool sameStructure = true)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (!sameStructure || structure == locationGridTile.structure)
			{
				return locationGridTile;
			}
		}
		return null;
	}

	public LocationGridTile GetFirstNeighborThatIsPassableAndNoObjectAndSameAreaAs(Area p_area)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (locationGridTile.tileObjectComponent.objHere == null && locationGridTile.IsPassable() && locationGridTile.area == p_area)
			{
				return locationGridTile;
			}
		}
		return null;
	}

	public LocationGridTile GetFirstNeighborThatIsPassableAndNoObject()
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (locationGridTile.tileObjectComponent.objHere == null && locationGridTile.IsPassable())
			{
				return locationGridTile;
			}
		}
		return null;
	}

	public LocationGridTile GetFirstNeighborThatIsPassable()
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (locationGridTile.IsPassable())
			{
				return locationGridTile;
			}
		}
		return null;
	}

	public LocationGridTile GetFirstNeighborThatIsPassableAndSameStructureAs(LocationStructure p_structure)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (locationGridTile.structure == p_structure && locationGridTile.IsPassable())
			{
				return locationGridTile;
			}
		}
		return null;
	}

	public bool IsAtEdgeOfWalkableMap()
	{
		if ((localPlace.y == 0 && localPlace.x >= 0 && localPlace.x <= parentMap.width - 1) || (localPlace.y == parentMap.height - 1 && localPlace.x >= 0 && localPlace.x <= parentMap.width - 1) || (localPlace.x == 0 && localPlace.y >= 0 && localPlace.y <= parentMap.height - 1) || (localPlace.x == parentMap.width - 1 && localPlace.y >= 0 && localPlace.y <= parentMap.height - 1))
		{
			return true;
		}
		return false;
	}

	private bool HasCardinalNeighbourOfDifferentGroundType()
	{
		List<LocationGridTile> list = FourNeighbours();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].groundType != groundType)
			{
				return true;
			}
		}
		return false;
	}

	public void PopulateTraitablesOnTile(List<ITraitable> traitables)
	{
		traitables.Add(tileObjectComponent.genericTileObject);
		for (int i = 0; i < tileObjectComponent.walls.Count; i++)
		{
			ThinWall item = tileObjectComponent.walls[i];
			traitables.Add(item);
		}
		for (int j = 0; j < charactersHere.Count; j++)
		{
			Character character = charactersHere[j];
			if (!(tileObjectComponent.objHere is Tombstone tombstone) || tombstone.character != character)
			{
				traitables.Add(character);
			}
		}
		if (tileObjectComponent.objHere != null && tileObjectComponent.objHere.mapObjectState == MAP_OBJECT_STATE.BUILT)
		{
			traitables.Add(tileObjectComponent.objHere);
			if (tileObjectComponent.objHere is BaseBed { users: not null } baseBed && baseBed.users.Length != 0)
			{
				for (int k = 0; k < baseBed.users.Length; k++)
				{
					Character character2 = baseBed.users[k];
					if (character2 != null && !charactersHere.Contains(character2))
					{
						traitables.Add(character2);
					}
				}
			}
		}
		Messenger.Broadcast(GridTileSignals.POPULATE_TRAITABLES_ON_TILE, this, traitables);
	}

	public void PopulateAliveTraitablesOnTile(List<ITraitable> traitables)
	{
		traitables.Add(tileObjectComponent.genericTileObject);
		for (int i = 0; i < tileObjectComponent.walls.Count; i++)
		{
			ThinWall item = tileObjectComponent.walls[i];
			traitables.Add(item);
		}
		for (int j = 0; j < charactersHere.Count; j++)
		{
			Character character = charactersHere[j];
			if ((!(tileObjectComponent.objHere is Tombstone tombstone) || tombstone.character != character) && !character.isDead)
			{
				traitables.Add(character);
			}
		}
		if (tileObjectComponent.objHere != null && tileObjectComponent.objHere.mapObjectState == MAP_OBJECT_STATE.BUILT)
		{
			traitables.Add(tileObjectComponent.objHere);
			if (tileObjectComponent.objHere is BaseBed { users: not null } baseBed && baseBed.users.Length != 0)
			{
				for (int k = 0; k < baseBed.users.Length; k++)
				{
					Character character2 = baseBed.users[k];
					if (character2 != null && !charactersHere.Contains(character2))
					{
						traitables.Add(character2);
					}
				}
			}
		}
		Messenger.Broadcast(GridTileSignals.POPULATE_ALIVE_TRAITABLES_ON_TILE, this, traitables);
	}

	public void PopulateTraitablesOnTileThatCanHaveElementalTrait(List<ITraitable> traitables, string traitName, bool bypassElementalChance, float piercing, ELEMENTAL_TYPE elementalType)
	{
		if (GameUtilities.RollChance(tileObjectComponent.genericTileObject.traitContainer.GetElementalTraitChanceToBeAdded(traitName, tileObjectComponent.genericTileObject, bypassElementalChance, null, piercing, elementalType)) && tileObjectComponent.genericTileObject.CanBeAffectedByElementalStatus(traitName))
		{
			traitables.Add(tileObjectComponent.genericTileObject);
		}
		for (int i = 0; i < tileObjectComponent.walls.Count; i++)
		{
			ThinWall thinWall = tileObjectComponent.walls[i];
			if (thinWall.traitContainer.GetElementalTraitChanceToBeAdded(traitName, thinWall, bypassElementalChance, null, piercing, elementalType) > 0)
			{
				traitables.Add(thinWall);
			}
		}
		if (tileObjectComponent.objHere != null && tileObjectComponent.objHere.mapObjectState == MAP_OBJECT_STATE.BUILT && GameUtilities.RollChance(tileObjectComponent.objHere.traitContainer.GetElementalTraitChanceToBeAdded(traitName, tileObjectComponent.objHere, bypassElementalChance, null, piercing, elementalType)) && tileObjectComponent.objHere.CanBeAffectedByElementalStatus(traitName))
		{
			traitables.Add(tileObjectComponent.objHere);
		}
		for (int j = 0; j < charactersHere.Count; j++)
		{
			Character character = charactersHere[j];
			if (GameUtilities.RollChance(character.traitContainer.GetElementalTraitChanceToBeAdded(traitName, character, bypassElementalChance, null, piercing, elementalType)))
			{
				traitables.Add(character);
			}
		}
	}

	public void PopulatePOIsOnTile(List<IPointOfInterest> pois)
	{
		pois.Add(tileObjectComponent.genericTileObject);
		if (tileObjectComponent.objHere != null && tileObjectComponent.objHere.mapObjectState == MAP_OBJECT_STATE.BUILT)
		{
			pois.Add(tileObjectComponent.objHere);
		}
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character item = charactersHere[i];
			pois.Add(item);
		}
	}

	public void AddTraitToAllPOIsOnTile(string traitName, ELEMENTAL_TYPE elementalType = ELEMENTAL_TYPE.Normal)
	{
		tileObjectComponent.genericTileObject.traitContainer.AddTrait(tileObjectComponent.genericTileObject, traitName, null, bypassElementalChance: false, -1, 0f, elementalType);
		if (tileObjectComponent.objHere != null && tileObjectComponent.objHere.mapObjectState == MAP_OBJECT_STATE.BUILT)
		{
			tileObjectComponent.objHere.traitContainer.AddTrait(tileObjectComponent.objHere, traitName, null, bypassElementalChance: false, -1, 0f, elementalType);
		}
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			character.traitContainer.AddTrait(character, traitName);
		}
	}

	public int GetDifferentElevationNeighboursCount()
	{
		int num = 0;
		for (int i = 0; i < neighbourList.Count; i++)
		{
			if (neighbourList[i].elevationType != elevationType)
			{
				num++;
			}
		}
		return num;
	}

	private BaseSettlement GetSettlementLocationBasedOnStructure()
	{
		if (structure != null && structure.settlementLocation != null)
		{
			return structure.settlementLocation;
		}
		return area.GetSettlementOnArea();
	}

	public bool IsPartOfSettlement(out BaseSettlement settlement)
	{
		BaseSettlement settlementLocationBasedOnStructure = GetSettlementLocationBasedOnStructure();
		if (settlementLocationBasedOnStructure != null)
		{
			settlement = settlementLocationBasedOnStructure;
			return true;
		}
		settlement = null;
		return false;
	}

	public bool IsPartOfSettlement(BaseSettlement settlement)
	{
		return GetSettlementLocationBasedOnStructure() == settlement;
	}

	public bool IsPartOfSettlement()
	{
		return GetSettlementLocationBasedOnStructure() != null;
	}

	public bool IsNextToSettlement(out BaseSettlement settlement)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			if (neighbourList[i].IsPartOfSettlement(out settlement))
			{
				return true;
			}
		}
		settlement = null;
		return false;
	}

	public bool IsNextToSettlement(BaseSettlement settlement)
	{
		for (int i = 0; i < neighbourList.Count; i++)
		{
			if (neighbourList[i].IsPartOfSettlement(settlement))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsNextToSettlementArea(BaseSettlement settlement)
	{
		Area area = this.area;
		for (int i = 0; i < area.neighbourComponent.neighbours.Count; i++)
		{
			if (area.neighbourComponent.neighbours[i].HasSettlementOnArea(settlement))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsNextToOrPartOfSettlement(out BaseSettlement settlement)
	{
		if (!IsPartOfSettlement(out settlement))
		{
			return IsNextToSettlement(out settlement);
		}
		return true;
	}

	public bool IsNextToOrPartOfSettlement(BaseSettlement settlement)
	{
		if (!IsPartOfSettlement(settlement))
		{
			return IsNextToSettlement(settlement);
		}
		return true;
	}

	public bool IsNextToSettlementAreaOrPartOfSettlement(BaseSettlement settlement)
	{
		if (!IsPartOfSettlement(settlement))
		{
			return IsNextToSettlementArea(settlement);
		}
		return true;
	}

	public void PopulateTilesInRadius(List<LocationGridTile> tiles, int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false, bool includeImpassable = true, bool includeTilesWithObject = true)
	{
		int upperBound = parentMap.map.GetUpperBound(0);
		int upperBound2 = parentMap.map.GetUpperBound(1);
		int x = localPlace.x;
		int y = localPlace.y;
		if (includeCenterTile)
		{
			tiles.Add(this);
		}
		int num = x - radiusLimit;
		int num2 = x + radiusLimit;
		int num3 = y - radiusLimit;
		int num4 = y + radiusLimit;
		for (int i = x - radius; i <= x + radius; i++)
		{
			for (int j = y - radius; j <= y + radius; j++)
			{
				if (i >= 0 && i <= upperBound && j >= 0 && j <= upperBound2 && (i != x || j != y) && (radiusLimit <= 0 || i <= num || i >= num2 || j <= num3 || j >= num4))
				{
					LocationGridTile locationGridTile = parentMap.map[i, j];
					if (locationGridTile.structure != null && (includeTilesWithObject || locationGridTile.tileObjectComponent.objHere == null) && (includeTilesInDifferentStructure || locationGridTile.structure == structure || (locationGridTile.structure.structureType.IsOpenSpace() && structure.structureType.IsOpenSpace())) && (includeImpassable || locationGridTile.IsPassable()))
					{
						tiles.Add(locationGridTile);
					}
				}
			}
		}
	}

	public bool IsPassable()
	{
		if (IsWater())
		{
			return false;
		}
		if (tileObjectComponent.objHere != null && tileObjectComponent.objHere.IsUnpassable())
		{
			return false;
		}
		return true;
	}

	public bool IsWater()
	{
		if (groundType != Ground_Type.Water_Deep && groundType != Ground_Type.Water_Mid && groundType != Ground_Type.Water_Shallow)
		{
			return groundType == Ground_Type.Water_Shore;
		}
		return true;
	}

	public LocationGridTile GetNearestTileWithWalkableNode(bool includeThis = true)
	{
		if (includeThis && HasWalkableNode())
		{
			return this;
		}
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		list.Add(this);
		LocationGridTile nearestTileWithWalkableNodeRecursively = GetNearestTileWithWalkableNodeRecursively(list);
		RuinarchListPool<LocationGridTile>.Release(list);
		return nearestTileWithWalkableNodeRecursively;
	}

	public LocationGridTile GetNearestTileWithWalkableNodeRecursively(List<LocationGridTile> alreadyCheckedTiles)
	{
		alreadyCheckedTiles.Add(this);
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (locationGridTile.HasWalkableNode())
			{
				return locationGridTile;
			}
		}
		for (int j = 0; j < neighbourList.Count; j++)
		{
			LocationGridTile locationGridTile2 = neighbourList[j];
			if (!alreadyCheckedTiles.Contains(locationGridTile2))
			{
				LocationGridTile nearestTileWithWalkableNodeRecursively = locationGridTile2.GetNearestTileWithWalkableNodeRecursively(alreadyCheckedTiles);
				if (nearestTileWithWalkableNodeRecursively != null)
				{
					return nearestTileWithWalkableNodeRecursively;
				}
			}
		}
		return null;
	}

	public Area GetNearestAreaWithinRegion()
	{
		if (this.area.elevationType != ELEVATION.WATER && this.area.elevationType != ELEVATION.MOUNTAIN)
		{
			return this.area;
		}
		Area area = null;
		float num = 0f;
		for (int i = 0; i < this.area.region.areas.Count; i++)
		{
			Area area2 = this.area.region.areas[i];
			if (area2.elevationType != ELEVATION.WATER && area2.elevationType != ELEVATION.MOUNTAIN)
			{
				float distanceTo = GetDistanceTo(area2.gridTileComponent.centerGridTile);
				if (area == null || distanceTo < num)
				{
					area = area2;
					num = distanceTo;
				}
			}
		}
		return area;
	}

	public Area GetNearestAreaWithinRegionThatCharacterHasPathTo(Character p_character)
	{
		if (p_character.movementComponent.HasPathTo(this.area))
		{
			return this.area;
		}
		Area area = null;
		float num = 0f;
		for (int i = 0; i < this.area.region.areas.Count; i++)
		{
			Area area2 = this.area.region.areas[i];
			if (p_character.movementComponent.HasPathTo(area2))
			{
				float distanceTo = GetDistanceTo(area2.gridTileComponent.centerGridTile);
				if (area == null || distanceTo < num)
				{
					area = area2;
					num = distanceTo;
				}
			}
		}
		return area;
	}

	public Area GetNearestAreaWithinRegionThatIsNotMountainAndWaterAndHasNoSettlement()
	{
		if (this.area.elevationType != ELEVATION.MOUNTAIN && this.area.elevationType != ELEVATION.WATER && !this.area.HasSettlementOnArea())
		{
			return this.area;
		}
		Area area = null;
		float num = 0f;
		for (int i = 0; i < this.area.region.areas.Count; i++)
		{
			Area area2 = this.area.region.areas[i];
			if (area2.elevationType != ELEVATION.MOUNTAIN && area2.elevationType != ELEVATION.WATER && !area2.HasSettlementOnArea())
			{
				float distanceTo = GetDistanceTo(area2.gridTileComponent.centerGridTile);
				if (area == null || distanceTo < num)
				{
					area = area2;
					num = distanceTo;
				}
			}
		}
		return area;
	}

	public Area GetNearestAreaForNecromancerSpawnLairOrBuildingBanditCamp(Character p_builder)
	{
		if (IsAreaSuitableForBanditCamp(p_builder))
		{
			return this.area;
		}
		Area area = null;
		float num = 9999f;
		for (int i = 0; i < this.area.region.areas.Count; i++)
		{
			Area area2 = this.area.region.areas[i];
			if (IsAreaSuitableForBanditCamp(p_builder))
			{
				float distanceTo = GetDistanceTo(area2.gridTileComponent.centerGridTile);
				if (area == null || distanceTo < num)
				{
					area = area2;
					num = distanceTo;
				}
			}
		}
		return area;
	}

	public bool IsAreaSuitableForBanditCamp(Character p_builder)
	{
		if (area.elevationComponent.IsFully(ELEVATION.PLAIN) && !area.structureComponent.HasStructureInArea() && !area.HasSettlementOnArea() && !area.IsNextToOrPartOfVillage() && p_builder.movementComponent.HasPathTo(area) && !area.HasBlueprintOnTile() && !area.neighbourComponent.IsAtEdgeOfMap())
		{
			for (int i = 0; i < area.gridTileComponent.borderTiles.Count; i++)
			{
				LocationGridTile p_tile = area.gridTileComponent.borderTiles[i];
				if (!LocationStructureObject.AreTilesInRadiusTileValidForStructurePlacement(STRUCTURE_TYPE.BANDIT_CAMP, p_tile, out var _))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public bool HasPathOutside()
	{
		Vector3 positionWithinTileThatIsOnAWalkableNode = GetPositionWithinTileThatIsOnAWalkableNode();
		for (int i = 0; i < area.neighbourComponent.neighbours.Count; i++)
		{
			LocationGridTile firstPassableWithWalkableNodeTile = area.neighbourComponent.neighbours[i].gridTileComponent.GetFirstPassableWithWalkableNodeTile();
			if (firstPassableWithWalkableNodeTile != null)
			{
				Vector3 positionWithinTileThatIsOnAWalkableNode2 = firstPassableWithWalkableNodeTile.GetPositionWithinTileThatIsOnAWalkableNode();
				if (!positionWithinTileThatIsOnAWalkableNode.Equals(Vector3.positiveInfinity) && !positionWithinTileThatIsOnAWalkableNode.Equals(Vector3.negativeInfinity) && !positionWithinTileThatIsOnAWalkableNode2.Equals(Vector3.positiveInfinity) && !positionWithinTileThatIsOnAWalkableNode2.Equals(Vector3.negativeInfinity) && PathfindingManager.Instance.HasPath(positionWithinTileThatIsOnAWalkableNode, positionWithinTileThatIsOnAWalkableNode2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SetHasBlueprint(bool hasBlueprint, int p_blueprintTileRefXPos, int p_blueprintTileRefYPos)
	{
		this.hasBlueprint = hasBlueprint;
		blueprintTileReferenceXPos = p_blueprintTileRefXPos;
		blueprintTileReferenceYPos = p_blueprintTileRefYPos;
		if (hasBlueprint)
		{
			area.AddBlueprint();
		}
		else
		{
			area.RemoveBlueprint();
		}
	}

	public void AddStructureConnector(StructureConnector p_connector)
	{
		connectorsOnTile++;
		area.structureComponent.AddStructureConnector(p_connector);
	}

	public void RemoveStructureConnector(StructureConnector p_connector)
	{
		connectorsOnTile--;
		area.structureComponent.RemoveStructureConnector(p_connector);
	}

	public void AddPlaguedRats(bool p_randomizedPosition = false, bool isFromSpell = false)
	{
		Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Rat, PlayerManager.Instance.player.playerFaction, null, parentMap.region);
		summon.OnSummonAsPlayerMonster();
		CharacterManager.Instance.PlaceSummonInitially(summon, this);
		if (isFromSpell)
		{
			summon.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
		}
		if (p_randomizedPosition)
		{
			Vector3 position = summon.mapObjectVisual.transform.position;
			position.x += UnityEngine.Random.Range(-1f, 1f);
			position.y += UnityEngine.Random.Range(-1f, 1f);
			summon.mapObjectVisual.transform.position = position;
		}
		BaseSettlement settlement = null;
		if (structure.structureType != STRUCTURE_TYPE.WILDERNESS && structure.structureType != STRUCTURE_TYPE.OCEAN && IsPartOfSettlement(out settlement) && settlement.locationType != LOCATION_TYPE.VILLAGE)
		{
			summon.MigrateHomeStructureTo(structure);
		}
		else
		{
			summon.SetTerritory(area, returnHome: false);
		}
		summon.jobQueue.CancelAllJobs();
	}

	public void AddNecronomicon()
	{
		Artifact poi = InnerMapManager.Instance.CreateNewArtifact(ARTIFACT_TYPE.Necronomicon);
		structure.AddPOI(poi, this);
	}

	public void AddMeteor()
	{
		SetIsDefault(state: false);
		meteorCount++;
		GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Meteor_Strike);
	}

	public void AddMeteorFromSave()
	{
		GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Meteor_Strike);
	}

	public void RemoveMeteor()
	{
		meteorCount--;
		SetIsDefault(state: true);
	}

	public void AddNonPlayerMeteor()
	{
		SetIsDefault(state: false);
		nonPlayerMeteorCount++;
		GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Non_Player_Meteor_Strike);
	}

	public void AddNonPlayerMeteorFromSave()
	{
		GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Non_Player_Meteor_Strike);
	}

	public void RemoveNonPlayerMeteor()
	{
		nonPlayerMeteorCount--;
		SetIsDefault(state: true);
	}

	public void SetIndividualBiomeType(BIOMES p_biome)
	{
		BiomeDivision biomeDivision = parentMap.region.biomeDivisionComponent.GetBiomeDivision(mainBiomeType);
		biomeDivision?.RemoveTile(this);
		mainBiomeType = p_biome;
		parentMap.region.biomeDivisionComponent.GetBiomeDivision(p_biome)?.AddTile(this);
		if (biomeDivision != null)
		{
			area.biomeComponent.OnTileInAreaChangedBiome(this, biomeDivision.biome);
		}
	}

	public void SetSpecificBiomeType(Biome_Tile_Type p_type)
	{
		specificBiomeTileType = p_type;
	}

	public void SetElevation(ELEVATION p_elevation)
	{
		ELEVATION p_oldElevation = elevationType;
		elevationType = p_elevation;
		area.elevationComponent.OnTileInAreaChangedElevation(this, p_oldElevation);
	}

	public GraphNode GetGridNodeByWorldPosition(Vector3 p_worldPos)
	{
		for (int i = 0; i < nodePoints.Length; i++)
		{
			if (GetNodePointWorldLocation(nodePoints[i]).Equals(p_worldPos))
			{
				return GetGridNodeByNodePointIndex(i);
			}
		}
		return null;
	}

	private Vector3 GetNodePointWorldLocation(PointFloat p_point)
	{
		float x = centeredWorldLocation.x + p_point.X;
		float y = centeredWorldLocation.y + p_point.Y;
		return new Vector3(x, y, centeredWorldLocation.z);
	}

	private Vector3 GetNodePointWorldLocation(GridNeighbourDirection p_direction)
	{
		int nodePointIndexByDirection = GetNodePointIndexByDirection(p_direction);
		float x = centeredWorldLocation.x + nodePoints[nodePointIndexByDirection].X;
		float y = centeredWorldLocation.y + nodePoints[nodePointIndexByDirection].Y;
		return new Vector3(x, y, centeredWorldLocation.z);
	}

	private int GetNodePointIndexByDirection(GridNeighbourDirection p_direction)
	{
		return p_direction switch
		{
			GridNeighbourDirection.North_East => 0, 
			GridNeighbourDirection.North_West => 1, 
			GridNeighbourDirection.South_East => 2, 
			GridNeighbourDirection.South_West => 3, 
			_ => -1, 
		};
	}

	private GridNeighbourDirection GetDirectionByNodePointIndex(int p_index)
	{
		return p_index switch
		{
			0 => GridNeighbourDirection.North_East, 
			1 => GridNeighbourDirection.North_West, 
			2 => GridNeighbourDirection.South_East, 
			3 => GridNeighbourDirection.South_West, 
			_ => GridNeighbourDirection.North, 
		};
	}

	public GraphNode GetGridNodeByNodePointIndex(int p_index)
	{
		GridNeighbourDirection directionByNodePointIndex = GetDirectionByNodePointIndex(p_index);
		Vector3 nodePointWorldLocation = GetNodePointWorldLocation(directionByNodePointIndex);
		return AstarPath.active.GetNearest(nodePointWorldLocation, GridMap.Instance.mainRegion.innerMap.onlyUnwalkableGraph).node;
	}

	public GraphNode GetGridNodeByNodePointIndexPathfindingGraph(int p_index)
	{
		GridNeighbourDirection directionByNodePointIndex = GetDirectionByNodePointIndex(p_index);
		Vector3 nodePointWorldLocation = GetNodePointWorldLocation(directionByNodePointIndex);
		return AstarPath.active.GetNearest(nodePointWorldLocation, GridMap.Instance.mainRegion.innerMap.onlyPathfindingGraph).node;
	}

	public GraphNode GetNearestGridNodeByWorldPos(Vector3 pos)
	{
		return AstarPath.active.GetNearest(pos, GridMap.Instance.mainRegion.innerMap.onlyUnwalkableGraph).node;
	}

	public Vector3 GetPositionWithinTileThatIsOnAWalkableNode()
	{
		for (int i = 0; i < nodePoints.Length; i++)
		{
			if (GetGridNodeByNodePointIndex(i).Walkable)
			{
				return GetNodePointWorldLocation(nodePoints[i]);
			}
		}
		return centeredWorldLocation;
	}

	public void CollisionCheckGraphNodes()
	{
		for (int i = 0; i < nodePoints.Length; i++)
		{
			GridNeighbourDirection directionByNodePointIndex = GetDirectionByNodePointIndex(i);
			Vector3 nodePointWorldLocation = GetNodePointWorldLocation(directionByNodePointIndex);
			InnerMapManager.Instance.currentlyShowingMap.unwalkableGraph.collision.LogCollisionCheck(nodePointWorldLocation);
		}
	}

	public bool IsPositionInWalkableNode(Vector3 pos)
	{
		return AstarPath.active.GetNearest(pos, GridMap.Instance.mainRegion.innerMap.onlyUnwalkableGraph).node.Walkable;
	}

	public bool HasUnwalkableNode()
	{
		for (int i = 0; i < nodePoints.Length; i++)
		{
			if (!GetGridNodeByNodePointIndex(i).Walkable)
			{
				return true;
			}
		}
		return false;
	}

	public string GetWalkableNodeSummary()
	{
		string text = "Walkable node summary: ";
		for (int i = 0; i < nodePoints.Length; i++)
		{
			GraphNode gridNodeByNodePointIndex = GetGridNodeByNodePointIndex(i);
			GraphNode gridNodeByNodePointIndexPathfindingGraph = GetGridNodeByNodePointIndexPathfindingGraph(i);
			text = $"{text}\n\t- {i} - {gridNodeByNodePointIndex.Walkable.ToString()} - {gridNodeByNodePointIndexPathfindingGraph.Walkable.ToString()} - {gridNodeByNodePointIndexPathfindingGraph.Tag.ToString()}";
		}
		return text;
	}

	public bool HasWalkableNode()
	{
		for (int i = 0; i < nodePoints.Length; i++)
		{
			if (GetGridNodeByNodePointIndex(i).Walkable)
			{
				return true;
			}
		}
		return false;
	}

	public Vector3 GetUnoccupiedWalkablePositionInTileThatIsInLineOfSightWith(IPointOfInterest p_target, float p_distanceLimit, Vector3 p_originPos, RaycastHit2D[] p_lineOfSightObjects)
	{
		for (int i = 0; i < nodePoints.Length; i++)
		{
			Vector3 nodePointWorldLocation = GetNodePointWorldLocation(nodePoints[i]);
			if (Vector2.Distance(nodePointWorldLocation, p_originPos) <= p_distanceLimit)
			{
				GraphNode gridNodeByNodePointIndex = GetGridNodeByNodePointIndex(i);
				if (gridNodeByNodePointIndex.Walkable && !IsGridNodeOccupiedByActiveCharacter(gridNodeByNodePointIndex) && GameUtilities.IsInLineOfSight(p_target, nodePointWorldLocation, 5f, GameUtilities.Line_Of_Sight_Layer_Mask, p_lineOfSightObjects))
				{
					return nodePointWorldLocation;
				}
			}
		}
		return Vector3.positiveInfinity;
	}

	public bool IsGridNodeOccupiedByActiveCharacter(GraphNode p_gridNode)
	{
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			if (!character.isDead && character.limiterComponent.canPerform && character.limiterComponent.canMove && AstarPath.active.GetNearest(character.worldPosition, GridMap.Instance.mainRegion.innerMap.onlyUnwalkableGraph).node == p_gridNode)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsGridNodeOccupiedByNonRepositioningActiveCharacterOtherThan(Character p_character)
	{
		GraphNode node = AstarPath.active.GetNearest(p_character.worldPosition, GridMap.Instance.mainRegion.innerMap.onlyUnwalkableGraph).node;
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			if (p_character != character && !character.isDead && character.limiterComponent.canPerform && character.limiterComponent.canMove && (!character.combatComponent.isInCombat || !(character.stateComponent.currentState is CombatState { isRepositioning: not false } combatState) || combatState.repositioningTo == node) && AstarPath.active.GetNearest(character.worldPosition, GridMap.Instance.mainRegion.innerMap.onlyUnwalkableGraph).node == node)
			{
				return true;
			}
		}
		return false;
	}

	public void SetIsDefault(bool state)
	{
		if (!state)
		{
			nonDefaultCounter++;
		}
		else
		{
			nonDefaultCounter--;
		}
	}

	public bool IsTileConsideredProtected(int p_radius)
	{
		return GetStructureProtectingTile(p_radius) != null;
	}

	public LocationStructure GetStructureProtectingTile(int p_radius)
	{
		LocationStructure primaryStructureInArea = area.primaryStructureInArea;
		if (structure.isProtected || (primaryStructureInArea != null && primaryStructureInArea.isProtected))
		{
			return structure;
		}
		if (p_radius > 0)
		{
			LocationStructure result = null;
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			PopulateTilesInRadius(list, p_radius, 0, includeCenterTile: false, includeTilesInDifferentStructure: true);
			for (int i = 0; i < list.Count; i++)
			{
				LocationGridTile locationGridTile = list[i];
				if (locationGridTile.structure.isProtected || (locationGridTile.area.primaryStructureInArea != null && locationGridTile.area.primaryStructureInArea.isProtected))
				{
					result = locationGridTile.structure;
					break;
				}
			}
			RuinarchListPool<LocationGridTile>.Release(list);
			return result;
		}
		return null;
	}

	public void CleanUp()
	{
		parentMap = null;
		parentTileMap = null;
		structure = null;
		_neighbours?.Clear();
		_neighbours = null;
		_fourNeighbours?.Clear();
		_fourNeighbours = null;
		_fourNeighboursList?.Clear();
		_fourNeighboursList = null;
		neighbourList?.Clear();
		neighbourList = null;
		charactersHere?.Clear();
		tileObjectComponent.CleanUp();
	}

	public void UpdateMinimapVisual(LocationStructure p_structure)
	{
		if (p_structure != null && p_structure.TryGetMinimapColorForTileInStructure(this, out var p_color))
		{
			parentMap.SetMinimapTileColor(localPlace, p_color);
			return;
		}
		if (corruptionComponent.isCorrupted)
		{
			parentMap.SetMinimapTileColor(localPlace, GameUtilities.CorruptedMinimapColor);
			return;
		}
		p_color = InnerMapManager.Instance.assetManager.GetColorForBiomeType(specificBiomeTileType);
		parentMap.SetMinimapTileColor(localPlace, p_color);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = structure;
		corruptionComponent.CheckIfStructureIsStillReferenced(p_structure);
		mouseEventsComponent.CheckIfStructureIsStillReferenced(p_structure);
		tileObjectComponent.CheckIfStructureIsStillReferenced(p_structure);
		eventDispatcher.CheckIfStructureIsStillReferenced(p_structure);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		charactersHere.Contains(p_character);
		corruptionComponent?.CheckIfCharacterIsStillReferenced(p_character);
		mouseEventsComponent?.CheckIfCharacterIsStillReferenced(p_character);
		tileObjectComponent?.CheckIfCharacterIsStillReferenced(p_character);
		eventDispatcher?.CheckIfCharacterIsStillReferenced(p_character);
	}
}
