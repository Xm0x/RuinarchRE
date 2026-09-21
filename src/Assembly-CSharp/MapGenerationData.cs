using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Tilemaps;
using UtilityScripts;

public class MapGenerationData
{
	public static int WorldMapTileGenerationBatches = 200;

	public static int InnerMapTileGenerationBatches = 500;

	public static int InnerMapSeamlessEdgeBatches = 300;

	public static int InnerMapElevationBatches = 300;

	public static int TileObjectLoadingBatches = 300;

	public static int LocationGridTileSecondaryWaveBatches = 300;

	public static int TileObjectCreationBatches = 10;

	public static int TileObjectCreationBatchesAfterWorldGeneration = 5;

	public static int TileObjectCreationBatchesWhileWaiting = 30;

	public static int LocationStructureSecondaryWaveBatches = 300;

	public const float XOffset = 2.56f;

	public const float YOffset = 1.93f;

	public WorldMapTemplate chosenWorldMapTemplate;

	private int AllowedHeightTolerance = 5;

	public List<VillageSpot> villageSpots { get; private set; }

	public List<Area> unreservedAreas { get; private set; }

	public List<Area> reservedAreas { get; private set; }

	public Dictionary<FactionTemplate, List<VillageSpot>> determinedVillages { get; private set; }

	public Dictionary<Area, List<LocationGridTile>> oceanBorderTilesCategorizedByArea { get; private set; }

	public Dictionary<Area, List<LocationGridTile>> caveBorderTilesCategorizedByArea { get; private set; }

	public TILE_OBJECT_TYPE[][] generatedMapPerlinDetailsMap { get; private set; }

	public bool isGeneratingTileObjects { get; private set; }

	public bool hasFinishedMapGenerationCoroutine { get; private set; }

	public List<StructureSetting> unplacedStructuresOnLastEnsuredStructurePlacementCall { get; private set; }

	public List<STRUCTURE_TYPE> LastPlacedStructureTypes { get; private set; }

	public Dictionary<NPCSettlement, int> missingFoodProducers { get; private set; }

	public Dictionary<NPCSettlement, int> missingBasicResourceProducers { get; private set; }

	public int width => chosenWorldMapTemplate.worldMapWidth;

	public int height => chosenWorldMapTemplate.worldMapHeight;

	public MapGenerationData()
	{
		villageSpots = new List<VillageSpot>();
		determinedVillages = new Dictionary<FactionTemplate, List<VillageSpot>>();
		oceanBorderTilesCategorizedByArea = new Dictionary<Area, List<LocationGridTile>>();
		caveBorderTilesCategorizedByArea = new Dictionary<Area, List<LocationGridTile>>();
		missingFoodProducers = new Dictionary<NPCSettlement, int>();
		missingBasicResourceProducers = new Dictionary<NPCSettlement, int>();
		LastPlacedStructureTypes = new List<STRUCTURE_TYPE>();
	}

	public void SetFinishedMapGenerationCoroutine(bool p_state)
	{
		hasFinishedMapGenerationCoroutine = p_state;
	}

	public VillageSpot AddVillageSpot(Area p_villageSpot, List<Area> p_areas, int p_lumberyardSpots, int p_miningSpots)
	{
		VillageSpot villageSpot = new VillageSpot(p_villageSpot, p_areas, p_lumberyardSpots, p_miningSpots);
		villageSpots.Add(villageSpot);
		return villageSpot;
	}

	public void RemoveVillageSpot(VillageSpot p_villageSpot)
	{
		villageSpots.Remove(p_villageSpot);
	}

	public void AddDeterminedVillage(FactionTemplate p_faction, VillageSpot p_spot)
	{
		if (!determinedVillages.ContainsKey(p_faction))
		{
			determinedVillages.Add(p_faction, new List<VillageSpot>());
		}
		determinedVillages[p_faction].Add(p_spot);
	}

	public void SetUnreservedAreas(List<Area> p_areas)
	{
		unreservedAreas = p_areas;
	}

	public void SetReservedAreas(List<Area> p_areas)
	{
		reservedAreas = p_areas;
	}

	public void AddOceanBorderTile(Area p_area, LocationGridTile p_tile)
	{
		if (!oceanBorderTilesCategorizedByArea.ContainsKey(p_area))
		{
			oceanBorderTilesCategorizedByArea.Add(p_area, RuinarchListPool<LocationGridTile>.Claim());
		}
		oceanBorderTilesCategorizedByArea[p_area].Add(p_tile);
	}

	public void RemoveOceanBorderTile(Area p_area, LocationGridTile p_tile)
	{
		if (oceanBorderTilesCategorizedByArea.ContainsKey(p_area))
		{
			oceanBorderTilesCategorizedByArea[p_area].Remove(p_tile);
			if (oceanBorderTilesCategorizedByArea[p_area].Count == 0)
			{
				oceanBorderTilesCategorizedByArea.Remove(p_area);
			}
		}
	}

	public LocationGridTile GetFirstUnoccupiedNonEdgeOceanTile(Area p_area)
	{
		if (oceanBorderTilesCategorizedByArea.ContainsKey(p_area))
		{
			List<LocationGridTile> list = oceanBorderTilesCategorizedByArea[p_area];
			for (int i = 0; i < list.Count; i++)
			{
				LocationGridTile locationGridTile = list[i];
				if (locationGridTile.tileObjectComponent.objHere == null && locationGridTile.structure is Ocean && !locationGridTile.IsAtEdgeOfMap() && locationGridTile.HasDifferentStructureNeighbour(useFourNeighbours: true))
				{
					return locationGridTile;
				}
			}
		}
		return null;
	}

	public void AddCaveBorderTile(Area p_area, LocationGridTile p_tile)
	{
		if (!caveBorderTilesCategorizedByArea.ContainsKey(p_area))
		{
			caveBorderTilesCategorizedByArea.Add(p_area, RuinarchListPool<LocationGridTile>.Claim());
		}
		caveBorderTilesCategorizedByArea[p_area].Add(p_tile);
	}

	public void RemoveCaveBorderTile(Area p_area, LocationGridTile p_tile)
	{
		if (caveBorderTilesCategorizedByArea.ContainsKey(p_area))
		{
			caveBorderTilesCategorizedByArea[p_area].Remove(p_tile);
			if (caveBorderTilesCategorizedByArea[p_area].Count == 0)
			{
				caveBorderTilesCategorizedByArea.Remove(p_area);
			}
		}
	}

	public LocationGridTile GetFirstUnoccupiedNonEdgeCaveTile(Area p_area, MapGenerationData p_data)
	{
		if (caveBorderTilesCategorizedByArea.ContainsKey(p_area))
		{
			List<LocationGridTile> list = caveBorderTilesCategorizedByArea[p_area];
			for (int i = 0; i < list.Count; i++)
			{
				LocationGridTile locationGridTile = list[i];
				if (IsTileValidOreVeinTarget(locationGridTile, p_data))
				{
					return locationGridTile;
				}
			}
		}
		return null;
	}

	public LocationGridTile GetFirstUnoccupiedNonEdgeCaveTileThatIsFacingVillageSpot(Area p_area, MapGenerationData p_data, Area p_villageSpot)
	{
		if (caveBorderTilesCategorizedByArea.ContainsKey(p_area))
		{
			List<LocationGridTile> p_choices = caveBorderTilesCategorizedByArea[p_area];
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			PopulateTilesFacingArea(list, p_choices, p_villageSpot);
			for (int i = 0; i < list.Count; i++)
			{
				LocationGridTile locationGridTile = list[i];
				bool flag = false;
				List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
				locationGridTile.PopulateTilesInRadius(list2, 1, 0, includeCenterTile: false, includeTilesInDifferentStructure: true);
				for (int j = 0; j < list2.Count; j++)
				{
					if (list2[j].area == p_villageSpot)
					{
						flag = true;
						break;
					}
				}
				RuinarchListPool<LocationGridTile>.Release(list2);
				bool flag2 = IsTileValidOreVeinTarget(locationGridTile, p_data);
				if (!flag && flag2)
				{
					RuinarchListPool<LocationGridTile>.Release(list);
					return locationGridTile;
				}
			}
			RuinarchListPool<LocationGridTile>.Release(list);
		}
		return null;
	}

	private bool IsTileValidOreVeinTarget(LocationGridTile p_tile, MapGenerationData p_data)
	{
		if ((p_tile.tileObjectComponent.objHere is BlockWall || p_data.GetGeneratedObjectOnTile(p_tile) == TILE_OBJECT_TYPE.BLOCK_WALL) && !p_tile.IsAtEdgeOfMap())
		{
			if (p_tile.GetCountOfNeighboursThatHasTileObjectOfType(TILE_OBJECT_TYPE.ORE_VEIN) > 0)
			{
				return false;
			}
			int num = p_tile.FourNeighbours().Count((LocationGridTile t) => t.structure is Wilderness);
			if (num == 1 || num == 2)
			{
				int num2 = p_tile.neighbourList.Count((LocationGridTile t) => t.structure is Wilderness);
				if (num2 == 3 || num2 == 2)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void PopulateTilesFacingArea(List<LocationGridTile> p_listToPopulate, List<LocationGridTile> p_choices, Area p_area)
	{
		LocationGridTile centerGridTile = p_area.gridTileComponent.centerGridTile;
		for (int i = 0; i < p_choices.Count; i++)
		{
			LocationGridTile locationGridTile = p_choices[i];
			GridNeighbourDirection dir = GridNeighbourDirection.North;
			if (locationGridTile.localPlace.x < centerGridTile.localPlace.x)
			{
				int num = Mathf.Abs(locationGridTile.localPlace.y - centerGridTile.localPlace.y);
				dir = ((locationGridTile.localPlace.y < centerGridTile.localPlace.y && num > AllowedHeightTolerance) ? GridNeighbourDirection.North_East : ((locationGridTile.localPlace.y <= centerGridTile.localPlace.y || num <= AllowedHeightTolerance) ? GridNeighbourDirection.East : GridNeighbourDirection.South_East));
			}
			else if (locationGridTile.localPlace.x > centerGridTile.localPlace.x)
			{
				int num2 = Mathf.Abs(locationGridTile.localPlace.y - centerGridTile.localPlace.y);
				dir = ((locationGridTile.localPlace.y < centerGridTile.localPlace.y && num2 > AllowedHeightTolerance) ? GridNeighbourDirection.North_West : ((locationGridTile.localPlace.y <= centerGridTile.localPlace.y || num2 <= AllowedHeightTolerance) ? GridNeighbourDirection.West : GridNeighbourDirection.South_West));
			}
			else if (locationGridTile.localPlace.x == centerGridTile.localPlace.x)
			{
				dir = ((locationGridTile.localPlace.y >= centerGridTile.localPlace.y) ? ((locationGridTile.localPlace.y > centerGridTile.localPlace.y) ? GridNeighbourDirection.South : GridNeighbourDirection.North) : GridNeighbourDirection.North);
			}
			LocationGridTile neighbourAtDirection = locationGridTile.GetNeighbourAtDirection(dir);
			if (neighbourAtDirection != null && neighbourAtDirection.structure.structureType == STRUCTURE_TYPE.WILDERNESS)
			{
				p_listToPopulate.Add(locationGridTile);
			}
		}
	}

	public void InitializeGeneratedMapPerlinDetails(int width, int height)
	{
		TILE_OBJECT_TYPE[][] array = new TILE_OBJECT_TYPE[width][];
		for (int i = 0; i < width; i++)
		{
			array[i] = new TILE_OBJECT_TYPE[height];
			for (int j = 0; j < height; j++)
			{
				array[i][j] = TILE_OBJECT_TYPE.NONE;
			}
		}
		generatedMapPerlinDetailsMap = array;
	}

	public void SetGeneratedMapPerlinDetails(LocationGridTile tileLocation, TILE_OBJECT_TYPE p_type)
	{
		generatedMapPerlinDetailsMap[tileLocation.localPlace.x][tileLocation.localPlace.y] = p_type;
		if (p_type == TILE_OBJECT_TYPE.NONE)
		{
			tileLocation.parentMap.detailsTilemap.SetTile(tileLocation.localPlace, null);
			return;
		}
		TileBase tileBaseToUse = InnerMapManager.Instance.GetTileObjectScriptableObject<TileObjectScriptableObject>(p_type).GetTileBaseToUse(tileLocation.mainBiomeType);
		tileLocation.parentMap.detailsTilemap.SetTile(tileLocation.localPlace, tileBaseToUse);
	}

	public void SetGeneratingTileObjectsState(bool p_state)
	{
		isGeneratingTileObjects = p_state;
	}

	public TILE_OBJECT_TYPE GetGeneratedObjectOnTile(LocationGridTile p_tile)
	{
		return generatedMapPerlinDetailsMap[p_tile.localPlace.x][p_tile.localPlace.y];
	}

	public void AddLastPlacedStructureTypes(STRUCTURE_TYPE p_types)
	{
		LastPlacedStructureTypes.Add(p_types);
	}

	public void ClearLastPlacedVillageStructures()
	{
		LastPlacedStructureTypes.Clear();
	}

	public void SetLastUnplacedStructures(List<StructureSetting> p_settings)
	{
		unplacedStructuresOnLastEnsuredStructurePlacementCall = p_settings;
	}

	public void SetMissingFoodProducers(NPCSettlement p_settlement, int p_count)
	{
		if (!missingFoodProducers.ContainsKey(p_settlement))
		{
			missingFoodProducers.Add(p_settlement, 0);
		}
		missingFoodProducers[p_settlement] = p_count;
	}

	public void SetMissingBasicResourceProducers(NPCSettlement p_settlement, int p_count)
	{
		if (!missingBasicResourceProducers.ContainsKey(p_settlement))
		{
			missingBasicResourceProducers.Add(p_settlement, 0);
		}
		missingBasicResourceProducers[p_settlement] = p_count;
	}

	public int GetTotalMissingProductionStructures(NPCSettlement p_settlement)
	{
		int num = 0;
		if (missingFoodProducers.ContainsKey(p_settlement))
		{
			num += missingFoodProducers[p_settlement];
		}
		if (missingBasicResourceProducers.ContainsKey(p_settlement))
		{
			num += missingBasicResourceProducers[p_settlement];
		}
		return num;
	}

	public void CleanUpAfterMapGeneration()
	{
		foreach (KeyValuePair<Area, List<LocationGridTile>> item in oceanBorderTilesCategorizedByArea)
		{
			RuinarchListPool<LocationGridTile>.Release(item.Value);
		}
		foreach (KeyValuePair<Area, List<LocationGridTile>> item2 in caveBorderTilesCategorizedByArea)
		{
			RuinarchListPool<LocationGridTile>.Release(item2.Value);
		}
		oceanBorderTilesCategorizedByArea.Clear();
		caveBorderTilesCategorizedByArea.Clear();
		oceanBorderTilesCategorizedByArea = null;
		caveBorderTilesCategorizedByArea = null;
		generatedMapPerlinDetailsMap = null;
		unplacedStructuresOnLastEnsuredStructurePlacementCall = null;
		LastPlacedStructureTypes = null;
		missingFoodProducers = null;
		missingBasicResourceProducers = null;
	}
}
