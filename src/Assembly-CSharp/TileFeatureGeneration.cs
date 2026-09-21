using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Inner_Maps;
using Locations.Area_Features;
using UtilityScripts;

public class TileFeatureGeneration : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating_Tile_Features");
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(DetermineVillageSpots(data));
		stopwatch.Stop();
		AddLog("DetermineVillageSpots took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
		stopwatch.Reset();
		stopwatch.Start();
		succeess = TryAssignSettlementTiles(data);
		stopwatch.Stop();
		AddLog("TryAssignSettlementTiles took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
	}

	private IEnumerator DetermineVillageSpots(MapGenerationData p_data)
	{
		List<Area> list = RuinarchListPool<Area>.Claim();
		List<Area> list2 = RuinarchListPool<Area>.Claim();
		list2.AddRange(GridMap.Instance.allAreas);
		List<Area> list3 = RuinarchListPool<Area>.Claim(GridMap.Instance.allAreas.Count);
		list3.AddRange(GridMap.Instance.allAreas);
		list3.Shuffle();
		for (int i = 0; i < list3.Count; i++)
		{
			Area area = list3[i];
			bool flag = false;
			for (int j = 0; j < area.neighbourComponent.cardinalNeighbours.Count; j++)
			{
				Area item = area.neighbourComponent.cardinalNeighbours[j];
				if (list.Contains(item))
				{
					flag = true;
					break;
				}
			}
			if (!flag && IsAreaValidVillageSpotCandidate(area, p_data))
			{
				list.Add(area);
			}
		}
		int maxVillagesForMapSize = WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxVillagesForMapSize();
		while (p_data.villageSpots.Count < maxVillagesForMapSize && list.Count != 0)
		{
			Area randomElement = CollectionUtilities.GetRandomElement(list);
			list.Remove(randomElement);
			List<Area> list4 = RuinarchListPool<Area>.Claim();
			list4.Add(randomElement);
			List<Area> list5 = RuinarchListPool<Area>.Claim();
			List<Area> list6 = RuinarchListPool<Area>.Claim();
			List<Area> list7 = RuinarchListPool<Area>.Claim();
			List<Area> list8 = RuinarchListPool<Area>.Claim();
			list5.AddRange(randomElement.neighbourComponent.cardinalNeighbours);
			list6.Add(randomElement);
			int num = 0;
			int num2 = 0;
			int num3 = GameUtilities.RandomBetweenTwoNumbers(10, 14);
			while (list5.Count > 0 && list4.Count < num3)
			{
				Area area2 = list5[0];
				if (area2.elevationComponent.elevationType == ELEVATION.PLAIN)
				{
					if (area2.tileObjectComponent.GetNumberOfTileObjectsInHexTile(TILE_OBJECT_TYPE.SMALL_TREE_OBJECT, TILE_OBJECT_TYPE.BIG_TREE_OBJECT, p_data) >= 8)
					{
						num++;
					}
					list4.Add(area2);
					for (int k = 0; k < area2.neighbourComponent.cardinalNeighbours.Count; k++)
					{
						Area item2 = area2.neighbourComponent.cardinalNeighbours[k];
						if (!list6.Contains(item2) && !list5.Contains(item2))
						{
							list5.Add(item2);
						}
					}
				}
				else
				{
					list.Remove(area2);
				}
				if (area2.elevationComponent.HasElevation(ELEVATION.WATER) && !list7.Contains(area2) && list7.Count < 2)
				{
					ReservedWaterAreaFishSourceHandling(p_data, area2);
					list7.Add(area2);
				}
				if (area2.elevationComponent.HasElevation(ELEVATION.MOUNTAIN) && area2.elevationComponent.elevationDictionary[ELEVATION.MOUNTAIN] > 5 && !list8.Contains(area2))
				{
					ReservedCaveAreaMetalSourceProcessing(p_data, area2, randomElement);
					list8.Add(area2);
					num2++;
				}
				list5.Remove(area2);
				list6.Add(area2);
			}
			if (list4.Count >= 6)
			{
				VillageSpot villageSpot = p_data.AddVillageSpot(randomElement, list4, num, num2);
				if (list7.Count < 2 || list8.Count < 2)
				{
					for (int l = 0; l < villageSpot.reservedAreas.Count; l++)
					{
						if (list7.Count >= 2 && list8.Count >= 2)
						{
							break;
						}
						Area area3 = villageSpot.reservedAreas[l];
						for (int m = 0; m < area3.neighbourComponent.cardinalNeighbours.Count; m++)
						{
							if (list7.Count >= 2 && list8.Count >= 2)
							{
								break;
							}
							Area area4 = area3.neighbourComponent.cardinalNeighbours[m];
							if (area4.elevationComponent.elevationType == ELEVATION.MOUNTAIN)
							{
								if (list8.Count < 2 && !list8.Contains(area4))
								{
									ReservedCaveAreaMetalSourceProcessing(p_data, area4, randomElement);
									list8.Add(area4);
								}
							}
							else if (area4.elevationComponent.elevationType == ELEVATION.WATER && list7.Count < 2 && !list7.Contains(area4))
							{
								ReservedWaterAreaFishSourceHandling(p_data, area4);
								list7.Add(area4);
							}
						}
					}
				}
				villageSpot.AddWaterAreas(list7);
				villageSpot.AddCaveAreas(list8);
				list.ListRemoveRange(list4);
				list.ListRemoveRange(randomElement.neighbourComponent.neighbours);
				list2.ListRemoveRange(list4);
				list2.ListRemoveRange(list7);
				list2.ListRemoveRange(list8);
				villageSpot.ConstructAreaDisableVotesArray();
			}
			RuinarchListPool<Area>.Release(list5);
			RuinarchListPool<Area>.Release(list6);
			RuinarchListPool<Area>.Release(list4);
			RuinarchListPool<Area>.Release(list8);
			RuinarchListPool<Area>.Release(list7);
		}
		RuinarchListPool<Area>.Release(list);
		p_data.SetUnreservedAreas(list2);
		GridMap.Instance.mainRegion.SetVillageSpots(p_data.villageSpots);
		yield return null;
	}

	private void ReservedCaveAreaMetalSourceProcessing(MapGenerationData p_data, Area currentAreaBeingChecked, Area p_mainSpotCandidate)
	{
		if (currentAreaBeingChecked.tileObjectComponent.HasTileObjectOfTypeInHexTile(TILE_OBJECT_TYPE.ORE_VEIN))
		{
			return;
		}
		int num = GameUtilities.RandomBetweenTwoNumbers(5, 7);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			LocationGridTile firstUnoccupiedNonEdgeCaveTileThatIsFacingVillageSpot = p_data.GetFirstUnoccupiedNonEdgeCaveTileThatIsFacingVillageSpot(currentAreaBeingChecked, p_data, p_mainSpotCandidate);
			if (firstUnoccupiedNonEdgeCaveTileThatIsFacingVillageSpot != null)
			{
				p_data.SetGeneratedMapPerlinDetails(firstUnoccupiedNonEdgeCaveTileThatIsFacingVillageSpot, TILE_OBJECT_TYPE.NONE);
				currentAreaBeingChecked.region.innerMap.CreateOreVein(firstUnoccupiedNonEdgeCaveTileThatIsFacingVillageSpot);
				num2++;
			}
		}
		if (num2 > 0)
		{
			return;
		}
		for (int j = 0; j < 2; j++)
		{
			LocationGridTile firstUnoccupiedNonEdgeCaveTile = p_data.GetFirstUnoccupiedNonEdgeCaveTile(currentAreaBeingChecked, p_data);
			if (firstUnoccupiedNonEdgeCaveTile != null)
			{
				p_data.SetGeneratedMapPerlinDetails(firstUnoccupiedNonEdgeCaveTile, TILE_OBJECT_TYPE.NONE);
				currentAreaBeingChecked.region.innerMap.CreateOreVein(firstUnoccupiedNonEdgeCaveTile);
			}
		}
	}

	private void ReservedWaterAreaFishSourceHandling(MapGenerationData p_data, Area currentAreaBeingChecked)
	{
		if (!currentAreaBeingChecked.tileObjectComponent.HasTileObjectOfTypeInHexTile(TILE_OBJECT_TYPE.FISHING_SPOT))
		{
			LocationGridTile firstUnoccupiedNonEdgeOceanTile = p_data.GetFirstUnoccupiedNonEdgeOceanTile(currentAreaBeingChecked);
			currentAreaBeingChecked.region.innerMap.CreateFishingSpot(firstUnoccupiedNonEdgeOceanTile);
		}
	}

	private bool IsAreaValidVillageSpotCandidate(Area p_area, MapGenerationData p_mapGenerationData)
	{
		if (p_area.elevationComponent.elevationType == ELEVATION.PLAIN && p_area.elevationComponent.IsFully(ELEVATION.PLAIN) && (p_area.neighbourComponent.HasCardinalNeighbourWithElevationThatIsNotReservedByOtherVillage(ELEVATION.WATER, p_mapGenerationData.villageSpots) || p_area.neighbourComponent.HasCardinalNeighbourWithElevationThatIsNotReservedByOtherVillage(ELEVATION.MOUNTAIN, p_mapGenerationData.villageSpots)))
		{
			return true;
		}
		return false;
	}

	private bool TryAssignSettlementTiles(MapGenerationData data)
	{
		int num = 0;
		int currentTotalVillageCountBasedOnFactions = WorldSettings.Instance.worldSettingsData.factionSettings.GetCurrentTotalVillageCountBasedOnFactions();
		if (data.villageSpots.Count < currentTotalVillageCountBasedOnFactions)
		{
			return false;
		}
		for (int i = 0; i < WorldSettings.Instance.worldSettingsData.factionSettings.factionTemplates.Count; i++)
		{
			FactionTemplate factionTemplate = WorldSettings.Instance.worldSettingsData.factionSettings.factionTemplates[i];
			for (int j = 0; j < factionTemplate.villageSettings.Count; j++)
			{
				if (data.villageSpots.Count == 0)
				{
					return false;
				}
				VillageSpot villageSpot = null;
				List<VillageSpot> list = RuinarchListPool<VillageSpot>.Claim();
				for (int k = 0; k < data.villageSpots.Count; k++)
				{
					VillageSpot villageSpot2 = data.villageSpots[k];
					if (!villageSpot2.isDisabled && villageSpot2.CanAccommodateFaction(factionTemplate.factionType))
					{
						list.Add(villageSpot2);
					}
				}
				if (list.Count > 0)
				{
					villageSpot = CollectionUtilities.GetRandomElement(list);
					villageSpot.OccupyVillageSpot();
					data.AddDeterminedVillage(factionTemplate, villageSpot);
					data.RemoveVillageSpot(villageSpot);
					num++;
					continue;
				}
				return false;
			}
		}
		return num == currentTotalVillageCountBasedOnFactions;
	}

	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		SaveDataArea[,] savedMap = saveData.worldMapSave.GetSaveDataMap();
		for (int x = 0; x < data.width; x++)
		{
			for (int y = 0; y < data.height; y++)
			{
				SaveDataArea saveDataArea = savedMap[x, y];
				Area area = GridMap.Instance.map[x, y];
				List<SaveDataAreaFeature> tileFeatureSaveData = saveDataArea.tileFeatureSaveData;
				if (tileFeatureSaveData != null && tileFeatureSaveData.Count > 0)
				{
					for (int i = 0; i < saveDataArea.tileFeatureSaveData.Count; i++)
					{
						AreaFeature feature = saveDataArea.tileFeatureSaveData[i].Load();
						area.featureComponent.AddFeature(feature, area);
					}
				}
				yield return null;
			}
		}
	}
}
