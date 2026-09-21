using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Events.World_Events;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Managers;
using Pathfinding;
using UnityEngine;
using UtilityScripts;

public class MapGenerationFinalization : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Finalizing_World");
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(FinalizeInnerMaps());
		stopwatch.Stop();
		AddLog("FinalizeInnerMaps took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
		stopwatch.Reset();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(RegionalItemGeneration());
		stopwatch.Stop();
		AddLog("RegionalItemGeneration took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
		stopwatch.Reset();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(LandmarkItemGeneration());
		stopwatch.Stop();
		AddLog("LandmarkItemGeneration took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
		stopwatch.Reset();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(CaveItemGeneration());
		stopwatch.Stop();
		AddLog("CaveItemGeneration took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
		stopwatch.Reset();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(LoadSettlementItems());
		stopwatch.Stop();
		AddLog("LoadSettlementItems took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
		stopwatch.Reset();
		stopwatch.Start();
		yield return MapGenerator.Instance.StartCoroutine(CreateWorldEvents());
		stopwatch.Stop();
		AddLog("CreateWorldEvents took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
	}

	private IEnumerator CreateWorldEvents()
	{
		if (WorldSettings.Instance.worldSettingsData.villageSettings.migrationSpeed != MIGRATION_SPEED.None)
		{
			if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Eradication)
			{
				WorldEventManager.Instance.AddActiveEvent(new EradicationVillagerMigration());
			}
			else
			{
				WorldEventManager.Instance.AddActiveEvent(new AutomaticVillagerMigrationEvent());
			}
		}
		yield return null;
	}

	private IEnumerator LoadWorldEvents(SaveDataCurrentProgress saveData)
	{
		for (int i = 0; i < saveData.worldMapSave.worldEventSaves.Count; i++)
		{
			WorldEvent worldEvent = saveData.worldMapSave.worldEventSaves[i].Load();
			WorldEventManager.Instance.LoadEvent(worldEvent);
		}
		yield return null;
	}

	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Finalizing_World");
		yield return MapGenerator.Instance.StartCoroutine(FinalizeInnerMaps());
		yield return MapGenerator.Instance.StartCoroutine(FinalizePerTileMinimapVisual());
		yield return MapGenerator.Instance.StartCoroutine(ExecuteLoadedFeatureInitialActions());
		yield return MapGenerator.Instance.StartCoroutine(LoadWorldEvents(saveData));
		yield return null;
	}

	private IEnumerator ExecuteLoadedFeatureInitialActions()
	{
		for (int i = 0; i < GridMap.Instance.allAreas.Count; i++)
		{
			Area area = GridMap.Instance.allAreas[i];
			for (int j = 0; j < area.featureComponent.features.Count; j++)
			{
				area.featureComponent.features[j].LoadedGameStartActions(area);
			}
			yield return null;
		}
	}

	private IEnumerator FinalizeInnerMaps()
	{
		for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++)
		{
			InnerTileMap map = InnerMapManager.Instance.innerMaps[i];
			yield return MapGenerator.Instance.StartCoroutine(map.CreateSeamlessEdges());
			foreach (Progress item in AstarPath.active.ScanAsync(new NavGraph[2] { map.pathfindingGraph, map.unwalkableGraph }))
			{
				_ = item;
			}
			yield return null;
		}
		for (int j = 0; j < GridMap.Instance.mainRegion.villageSpots.Count; j++)
		{
			GridMap.Instance.mainRegion.villageSpots[j].DetermineMigrationSpawningArea();
		}
	}

	private IEnumerator FinalizePerTileMinimapVisual()
	{
		for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++)
		{
			InnerTileMap map = InnerMapManager.Instance.innerMaps[i];
			int batchCount = 0;
			for (int j = 0; j < map.allTiles.Count; j++)
			{
				LocationGridTile locationGridTile = map.allTiles[j];
				locationGridTile.UpdateMinimapVisual(locationGridTile.structure);
				batchCount++;
				if (batchCount == MapGenerationData.InnerMapSeamlessEdgeBatches)
				{
					batchCount = 0;
					yield return null;
				}
			}
		}
	}

	private IEnumerator LoadSettlementItems()
	{
		for (int i = 0; i < LandmarkManager.Instance.allNonPlayerSettlements.Count; i++)
		{
			NPCSettlement nPCSettlement = LandmarkManager.Instance.allNonPlayerSettlements[i];
			if (nPCSettlement.locationType != LOCATION_TYPE.DUNGEON)
			{
				InnerMapManager.Instance.LoadInitialSettlementItems(nPCSettlement);
				yield return null;
			}
		}
	}

	private IEnumerator RegionalItemGeneration()
	{
		Region mainRegion = GridMap.Instance.mainRegion;
		LocationStructure wilderness = mainRegion.wilderness;
		List<LocationGridTile> locationChoices = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < wilderness.unoccupiedTiles.Count; i++)
		{
			LocationGridTile locationGridTile = wilderness.unoccupiedTiles[i];
			if (!locationGridTile.area.HasSettlementOnArea() && locationGridTile.elevationType == ELEVATION.PLAIN)
			{
				locationChoices.Add(locationGridTile);
			}
		}
		if (locationChoices.Count > 0)
		{
			RandomRegionalItemGeneration(mainRegion, ref locationChoices);
			if (locationChoices.Count > 0)
			{
				for (int j = 0; j < 7; j++)
				{
					if (locationChoices.Count == 0)
					{
						break;
					}
					LocationGridTile randomElement = CollectionUtilities.GetRandomElement(locationChoices);
					randomElement.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.TREASURE_CHEST), randomElement);
					locationChoices.Remove(randomElement);
				}
			}
		}
		RuinarchListPool<LocationGridTile>.Release(locationChoices);
		yield return null;
	}

	private void RandomRegionalItemGeneration(Region region, ref List<LocationGridTile> locationChoices)
	{
		List<ItemSetting> itemChoicesForBiome = WorldConfigManager.Instance.worldWideItemGenerationSetting.GetItemChoicesForBiome();
		if (itemChoicesForBiome == null)
		{
			return;
		}
		CollectionUtilities.GetRandomElement(itemChoicesForBiome);
		int num = Random.Range(1, 5);
		for (int i = 0; i < num; i++)
		{
			if (locationChoices.Count == 0)
			{
				break;
			}
			TILE_OBJECT_TYPE itemType = CollectionUtilities.GetRandomElement(itemChoicesForBiome).itemType;
			LocationGridTile randomElement = CollectionUtilities.GetRandomElement(locationChoices);
			randomElement.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(itemType), randomElement);
			locationChoices.Remove(randomElement);
		}
	}

	private IEnumerator LandmarkItemGeneration()
	{
		List<LocationStructure> allSpecialStructures = RuinarchListPool<LocationStructure>.Claim();
		LandmarkManager.Instance.PopulateAllSpecialStructures(allSpecialStructures);
		for (int i = 0; i < allSpecialStructures.Count; i++)
		{
			LocationStructure locationStructure = allSpecialStructures[i];
			if (locationStructure.structureType == STRUCTURE_TYPE.CAVE || locationStructure is AnimalDen)
			{
				continue;
			}
			StructureData structureData = LandmarkManager.Instance.GetStructureData(locationStructure.structureType);
			if (!(structureData.itemGenerationSetting != null))
			{
				continue;
			}
			List<ItemSetting> itemChoicesForBiome = structureData.itemGenerationSetting.GetItemChoicesForBiome();
			if (itemChoicesForBiome == null)
			{
				continue;
			}
			int num = structureData.itemGenerationSetting.iterations.Random();
			for (int j = 0; j < num; j++)
			{
				int num2 = CollectionUtilities.GetRandomElement(itemChoicesForBiome).minMaxRange.Random();
				for (int k = 0; k < num2; k++)
				{
					TILE_OBJECT_TYPE itemType = CollectionUtilities.GetRandomElement(itemChoicesForBiome).itemType;
					locationStructure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(itemType));
				}
			}
			yield return null;
		}
		RuinarchListPool<LocationStructure>.Release(allSpecialStructures);
	}

	private IEnumerator CaveItemGeneration()
	{
		StructureData structureData = LandmarkManager.Instance.GetStructureData(STRUCTURE_TYPE.CAVE);
		Region mainRegion = GridMap.Instance.mainRegion;
		if (mainRegion.HasStructure(STRUCTURE_TYPE.CAVE))
		{
			List<LocationStructure> structuresAtLocation = mainRegion.GetStructuresAtLocation(STRUCTURE_TYPE.CAVE);
			List<ItemSetting> itemChoicesForBiome = structureData.itemGenerationSetting.GetItemChoicesForBiome();
			for (int i = 0; i < structuresAtLocation.Count; i++)
			{
				LocationStructure locationStructure = structuresAtLocation[i];
				int num = GetHexTileCountOfCave(locationStructure) - 1;
				for (int j = 0; j < num; j++)
				{
					ItemSetting randomElement = CollectionUtilities.GetRandomElement(itemChoicesForBiome);
					int num2 = randomElement.minMaxRange.Random();
					for (int k = 0; k < num2; k++)
					{
						locationStructure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(randomElement.itemType));
					}
				}
			}
		}
		yield return null;
	}

	private int GetHexTileCountOfCave(LocationStructure caveStructure)
	{
		List<Area> list = new List<Area>();
		for (int i = 0; i < caveStructure.unoccupiedTiles.Count; i++)
		{
			LocationGridTile locationGridTile = caveStructure.unoccupiedTiles.ElementAt(i);
			if (!list.Contains(locationGridTile.area))
			{
				list.Add(locationGridTile.area);
			}
		}
		return list.Count;
	}

	public static void ItemGenerationAfterPickingLoadout()
	{
		GenerateArtifacts();
	}

	private static void GenerateArtifacts()
	{
		int num = 1;
		List<TILE_OBJECT_TYPE> list = new List<TILE_OBJECT_TYPE>(WorldConfigManager.Instance.initialArtifactChoices);
		for (int i = 0; i < num; i++)
		{
			if (list.Count == 0)
			{
				break;
			}
			LocationStructure randomStructureThatIsInADungeonAndHasPassableTiles = GridMap.Instance.mainRegion.GetRandomStructureThatIsInADungeonAndHasPassableTiles();
			if (randomStructureThatIsInADungeonAndHasPassableTiles != null)
			{
				TILE_OBJECT_TYPE randomElement = CollectionUtilities.GetRandomElement(list);
				if (randomElement.IsArtifact(out var artifactType))
				{
					Artifact poi = InnerMapManager.Instance.CreateNewArtifact(artifactType);
					randomStructureThatIsInADungeonAndHasPassableTiles.AddPOI(poi);
					list.Remove(randomElement);
				}
				else
				{
					TileObject poi2 = InnerMapManager.Instance.CreateNewTileObject<TileObject>(randomElement);
					randomStructureThatIsInADungeonAndHasPassableTiles.AddPOI(poi2);
					list.Remove(randomElement);
				}
			}
		}
	}
}
