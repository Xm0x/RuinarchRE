using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Grid_Tile_Features;
using Inner_Maps.Location_Structures;
using Locations.Area_Features;
using UnityEngine;
using UtilityScripts;

public class VillageSpot
{
	public Area coreSpot { get; }

	public List<Area> reservedAreas { get; }

	public int lumberyardSpots { get; }

	public int miningSpots { get; }

	public List<string> linkedBeastDens { get; private set; }

	public Area migrationSpawningArea { get; private set; }

	public int[] areaDisableVotes { get; private set; }

	public bool isOccupied { get; private set; }

	public bool isDisabled
	{
		get
		{
			if (areaDisableVotes.Sum() <= 0 && !coreSpot.structureComponent.HasStructureInArea() && !coreSpot.IsNextToOrPartOfVillage())
			{
				return coreSpot.gridTileComponent.HasCorruption();
			}
			return true;
		}
	}

	public VillageSpot(Area p_spot, List<Area> p_areas, int p_lumberyardSpots, int p_miningSpots)
	{
		coreSpot = p_spot;
		reservedAreas = new List<Area>(p_areas);
		lumberyardSpots = p_lumberyardSpots;
		miningSpots = p_miningSpots;
		linkedBeastDens = new List<string>();
	}

	public VillageSpot(SaveDataVillageSpot p_data)
	{
		coreSpot = GameUtilities.GetHexTileGivenCoordinates(p_data.mainArea, GridMap.Instance.map);
		reservedAreas = GameUtilities.GetHexTilesGivenCoordinates(p_data.reservedAreas, GridMap.Instance.map);
		lumberyardSpots = p_data.lumberyardSpots;
		miningSpots = p_data.miningSpots;
		linkedBeastDens = p_data.linkedBeastDens;
		if (linkedBeastDens == null)
		{
			linkedBeastDens = new List<string>();
		}
		if (!reservedAreas.Contains(coreSpot))
		{
			reservedAreas.Add(coreSpot);
		}
		migrationSpawningArea = GameUtilities.GetHexTileGivenCoordinates(p_data.migrationSpawningArea, GridMap.Instance.map);
		areaDisableVotes = p_data.areaDisableVotes;
		isOccupied = p_data.isOccupied;
	}

	public override string ToString()
	{
		return coreSpot.ToString();
	}

	public void ColorCoreSpot()
	{
		Color p_color = Color.green;
		if (isDisabled)
		{
			p_color = Color.red;
		}
		p_color.a = 0.8f;
		ColorArea(coreSpot, p_color);
	}

	public void ColorWholeVillageSpot(Color p_color)
	{
		p_color.a = 0.8f;
		for (int i = 0; i < reservedAreas.Count; i++)
		{
			Area p_area = reservedAreas[i];
			ColorArea(p_area, p_color);
		}
		ColorCoreSpot();
	}

	public void ResetColorOnWholeVillageSpot()
	{
		for (int i = 0; i < reservedAreas.Count; i++)
		{
			Area p_area = reservedAreas[i];
			ResetAreaColor(p_area);
		}
		ColorCoreSpot();
	}

	private void ColorArea(Area p_area, Color p_color)
	{
		for (int i = 0; i < p_area.gridTileComponent.gridTiles.Count; i++)
		{
			LocationGridTile locationGridTile = p_area.gridTileComponent.gridTiles[i];
			locationGridTile.parentMap.perlinTilemap.SetTile(locationGridTile.localPlace, InnerMapManager.Instance.assetManager.grassTile);
			locationGridTile.parentMap.perlinTilemap.SetColor(locationGridTile.localPlace, p_color);
		}
	}

	private void ResetAreaColor(Area p_area)
	{
		for (int i = 0; i < p_area.gridTileComponent.gridTiles.Count; i++)
		{
			LocationGridTile locationGridTile = p_area.gridTileComponent.gridTiles[i];
			locationGridTile.parentMap.perlinTilemap.SetTile(locationGridTile.localPlace, null);
			locationGridTile.parentMap.perlinTilemap.SetColor(locationGridTile.localPlace, Color.white);
		}
	}

	public void AddWaterAreas(List<Area> p_areas)
	{
		reservedAreas.AddRange(p_areas);
	}

	public void AddCaveAreas(List<Area> p_areas)
	{
		reservedAreas.AddRange(p_areas);
	}

	public bool CanAccommodateFaction(FACTION_TYPE p_factionType)
	{
		switch (p_factionType)
		{
		case FACTION_TYPE.Elven_Kingdom:
			return lumberyardSpots > 0;
		case FACTION_TYPE.Human_Empire:
			return miningSpots > 0;
		case FACTION_TYPE.Vampire_Clan:
			if (lumberyardSpots <= 0)
			{
				return miningSpots > 0;
			}
			return true;
		case FACTION_TYPE.Lycan_Clan:
			if (lumberyardSpots <= 0)
			{
				return miningSpots > 0;
			}
			return true;
		case FACTION_TYPE.Demon_Cult:
			if (lumberyardSpots <= 0)
			{
				return miningSpots > 0;
			}
			return true;
		default:
			return true;
		}
	}

	public bool IsOccupiedByVillage(out NPCSettlement p_settlement)
	{
		p_settlement = coreSpot.GetFirstNPCSettlementOnArea();
		return p_settlement != null;
	}

	public void OccupyVillageSpot()
	{
		if (isOccupied)
		{
			return;
		}
		isOccupied = true;
		for (int i = 0; i < reservedAreas.Count; i++)
		{
			Area area = reservedAreas[i];
			for (int j = 0; j < GridMap.Instance.mainRegion.villageSpots.Count; j++)
			{
				VillageSpot villageSpot = GridMap.Instance.mainRegion.villageSpots[j];
				if (villageSpot != this && villageSpot.reservedAreas.Contains(area))
				{
					villageSpot.VoteToDisableArea(area);
				}
			}
		}
	}

	public void VacateVillageSpot()
	{
		if (!isOccupied)
		{
			return;
		}
		isOccupied = false;
		for (int i = 0; i < reservedAreas.Count; i++)
		{
			Area area = reservedAreas[i];
			for (int j = 0; j < GridMap.Instance.mainRegion.villageSpots.Count; j++)
			{
				VillageSpot villageSpot = GridMap.Instance.mainRegion.villageSpots[j];
				if (villageSpot != this && villageSpot.reservedAreas.Contains(area))
				{
					villageSpot.VoteToEnableArea(area);
				}
			}
		}
	}

	public void PopulateBorderTiles(List<Area> p_areas)
	{
		for (int i = 0; i < reservedAreas.Count; i++)
		{
			Area area = reservedAreas[i];
			for (int j = 0; j < area.neighbourComponent.neighbours.Count; j++)
			{
				Area item = area.neighbourComponent.neighbours[j];
				if (!reservedAreas.Contains(item))
				{
					p_areas.Add(area);
					break;
				}
			}
		}
	}

	public bool HasUnusedFishingSpot()
	{
		for (int i = 0; i < reservedAreas.Count; i++)
		{
			Area area = reservedAreas[i];
			if (!area.elevationComponent.HasElevation(ELEVATION.WATER))
			{
				continue;
			}
			for (int j = 0; j < area.tileObjectComponent.itemsInArea.Count; j++)
			{
				if (area.tileObjectComponent.itemsInArea[j] is FishingSpot fishingSpot && fishingSpot.structureConnector != null && fishingSpot.structureConnector.isOpen)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasAccessToSkinnerAnimals()
	{
		for (int i = 0; i < reservedAreas.Count; i++)
		{
			if (reservedAreas[i].structureComponent.HasStructureInArea(GameUtilities.skinnerStructures))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAccessToButcherAnimals()
	{
		for (int i = 0; i < reservedAreas.Count; i++)
		{
			Area area = reservedAreas[i];
			if (area.featureComponent.HasFeature(AreaFeatureDB.Game_Feature) || area.structureComponent.HasStructureInArea(STRUCTURE_TYPE.RABBIT_HOLE))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasUnusedMiningSpots()
	{
		for (int i = 0; i < reservedAreas.Count; i++)
		{
			Area area = reservedAreas[i];
			if (!area.elevationComponent.HasElevation(ELEVATION.MOUNTAIN))
			{
				continue;
			}
			for (int j = 0; j < area.structureComponent.structureConnectors.Count; j++)
			{
				StructureConnector structureConnector = area.structureComponent.structureConnectors[j];
				if (structureConnector.tileLocation.structure is Cave && !structureConnector.isPartOfLocationStructureObject)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasUnusedMiningSpotsThatSettlementHasNotYetConnectedTo(NPCSettlement p_settlement)
	{
		for (int i = 0; i < reservedAreas.Count; i++)
		{
			Area area = reservedAreas[i];
			if (!area.elevationComponent.HasElevation(ELEVATION.MOUNTAIN))
			{
				continue;
			}
			for (int j = 0; j < area.structureComponent.structureConnectors.Count; j++)
			{
				StructureConnector structureConnector = area.structureComponent.structureConnectors[j];
				if (structureConnector.tileLocation.structure is Cave cave && !structureConnector.isPartOfLocationStructureObject && !cave.IsConnectedToSettlement(p_settlement))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasUnusedLumberyardSpots()
	{
		BigTreeSpotFeature feature = GridMap.Instance.mainRegion.gridTileFeatureComponent.GetFeature<BigTreeSpotFeature>();
		SmallTreeSpotFeature feature2 = GridMap.Instance.mainRegion.gridTileFeatureComponent.GetFeature<SmallTreeSpotFeature>();
		for (int i = 0; i < reservedAreas.Count; i++)
		{
			Area p_area = reservedAreas[i];
			List<LocationGridTile> featureTilesInArea = feature.GetFeatureTilesInArea(p_area);
			List<LocationGridTile> featureTilesInArea2 = feature2.GetFeatureTilesInArea(p_area);
			if (featureTilesInArea != null)
			{
				for (int j = 0; j < featureTilesInArea.Count; j++)
				{
					if (featureTilesInArea[j].tileObjectComponent.objHere is TreeObject treeObject && treeObject.structureConnector != null && treeObject.structureConnector.isOpen)
					{
						return true;
					}
				}
			}
			if (featureTilesInArea2 == null)
			{
				continue;
			}
			for (int k = 0; k < featureTilesInArea2.Count; k++)
			{
				if (featureTilesInArea2[k].tileObjectComponent.objHere is TreeObject treeObject2 && treeObject2.structureConnector != null && treeObject2.structureConnector.isOpen)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddLinkedBeastDen(LocationStructure p_structure)
	{
		if (!linkedBeastDens.Contains(p_structure.persistentID))
		{
			linkedBeastDens.Add(p_structure.persistentID);
		}
	}

	public LocationStructure GetRandomLinkedAliveBeastDen()
	{
		LocationStructure result = null;
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		for (int i = 0; i < linkedBeastDens.Count; i++)
		{
			LocationStructure structureByPersistentIDSafe = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(linkedBeastDens[i]);
			if (structureByPersistentIDSafe == null)
			{
				continue;
			}
			if (!structureByPersistentIDSafe.hasBeenDestroyed)
			{
				if (structureByPersistentIDSafe.GetFirstTileWithObject().tileObjectComponent.objHere is AnimalBurrow animalBurrow && animalBurrow.HasAliveSpawnedMonster())
				{
					list.Add(structureByPersistentIDSafe);
				}
			}
			else
			{
				linkedBeastDens.RemoveAt(i);
				i--;
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	public string GetLinkedBeastDensSummary()
	{
		string text = string.Empty;
		for (int i = 0; i < linkedBeastDens.Count; i++)
		{
			if (i > 0)
			{
				text += ",";
			}
			text += DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(linkedBeastDens[i])?.name;
		}
		return text;
	}

	public void DetermineMigrationSpawningArea()
	{
		Area area = null;
		float num = float.MaxValue;
		for (int i = 0; i < GridMap.Instance.edgeAreas.Count; i++)
		{
			Area area2 = GridMap.Instance.edgeAreas[i];
			if (area2.elevationComponent.elevationType == ELEVATION.PLAIN && PathfindingManager.Instance.HasPath(area2.gridTileComponent.centerGridTile, coreSpot.gridTileComponent.centerGridTile))
			{
				float num2 = Vector2.Distance(area2.gridTileComponent.centerGridTile.centeredLocalLocation, coreSpot.gridTileComponent.centerGridTile.centeredLocalLocation);
				if (num2 < num)
				{
					area = area2;
					num = num2;
				}
			}
		}
		if (area != null)
		{
			migrationSpawningArea = area;
		}
		else
		{
			migrationSpawningArea = coreSpot;
		}
	}

	public LocationGridTile GetRandomMigrationSpawningTile()
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < migrationSpawningArea.gridTileComponent.borderTiles.Count; i++)
		{
			LocationGridTile locationGridTile = migrationSpawningArea.gridTileComponent.borderTiles[i];
			if (locationGridTile.IsAtEdgeOfMap() && !locationGridTile.corruptionComponent.isCorrupted && locationGridTile.IsPassable() && !locationGridTile.structure.isInterior)
			{
				list.Add(locationGridTile);
			}
		}
		if (list.Count <= 0)
		{
			if (migrationSpawningArea.gridTileComponent.passableTiles.Count > 0)
			{
				list.AddRange(migrationSpawningArea.gridTileComponent.passableTiles);
			}
			else
			{
				list.AddRange(migrationSpawningArea.gridTileComponent.gridTiles);
			}
		}
		LocationGridTile randomElement = CollectionUtilities.GetRandomElement(list);
		RuinarchListPool<LocationGridTile>.Release(list);
		return randomElement;
	}

	public void ConstructAreaDisableVotesArray()
	{
		areaDisableVotes = new int[reservedAreas.Count];
		for (int i = 0; i < areaDisableVotes.Length; i++)
		{
			areaDisableVotes[i] = 0;
		}
	}

	private void VoteToDisableArea(Area p_area)
	{
		_ = isDisabled;
		int num = reservedAreas.IndexOf(p_area);
		areaDisableVotes[num]++;
	}

	private void VoteToEnableArea(Area p_area)
	{
		_ = isDisabled;
		int num = reservedAreas.IndexOf(p_area);
		areaDisableVotes[num]--;
	}

	[Conditional("DEBUG_LOG")]
	private void LogChangeInEnabledState(bool wasDisabled)
	{
		if ((!wasDisabled || isDisabled) && !wasDisabled)
		{
			_ = isDisabled;
		}
	}
}
