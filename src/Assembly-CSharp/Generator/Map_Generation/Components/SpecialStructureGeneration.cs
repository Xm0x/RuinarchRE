using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Area_Features;
using UnityEngine;
using UtilityScripts;

namespace Generator.Map_Generation.Components;

public class SpecialStructureGeneration : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		_ = string.Empty;
		int specialStructuresToCreate = WorldSettings.Instance.worldSettingsData.mapSettings.GetSpecialStructuresToCreate();
		bool isUsingUnreservedAreas = true;
		List<Area> locationChoices = RuinarchListPool<Area>.Claim();
		for (int i = 0; i < data.unreservedAreas.Count; i++)
		{
			Area area = data.unreservedAreas[i];
			if (IsAreaValidForSpecialStructurePlacement(area) && !area.HasNeigbouringAreaThatIsReservedByVillageSpot())
			{
				locationChoices.Add(area);
			}
		}
		Dictionary<BIOMES, List<STRUCTURE_TYPE>> structureChoicesPerBiome = new Dictionary<BIOMES, List<STRUCTURE_TYPE>>();
		BIOMES[] enumValues = CollectionUtilities.GetEnumValues<BIOMES>();
		foreach (BIOMES bIOMES in enumValues)
		{
			List<STRUCTURE_TYPE> list = RuinarchListPool<STRUCTURE_TYPE>.Claim();
			for (int k = 0; k < WorldConfigManager.Instance.worldGenSpecialStructureChoices.Length; k++)
			{
				STRUCTURE_TYPE sTRUCTURE_TYPE = WorldConfigManager.Instance.worldGenSpecialStructureChoices[k];
				if (sTRUCTURE_TYPE.CanSpecialStructureBePlacedOnBiome(bIOMES))
				{
					list.Add(sTRUCTURE_TYPE);
				}
			}
			if (list.Count > 0)
			{
				structureChoicesPerBiome.Add(bIOMES, list);
				continue;
			}
			RemoveAreasWithBiomeFromLocationChoices(locationChoices, bIOMES);
			RuinarchListPool<STRUCTURE_TYPE>.Release(list);
		}
		for (int l = 0; l < specialStructuresToCreate; l++)
		{
			if (locationChoices.Count == 0)
			{
				if (!isUsingUnreservedAreas)
				{
					break;
				}
				isUsingUnreservedAreas = false;
				PopulateListWithAreasThatAreFarFromVillageSpotCores(locationChoices);
			}
			if (locationChoices.Count == 0)
			{
				break;
			}
			Area chosenArea = CollectionUtilities.GetRandomElement(locationChoices);
			List<STRUCTURE_TYPE> list2 = structureChoicesPerBiome[chosenArea.biomeType];
			if (list2.Count <= 0)
			{
				continue;
			}
			STRUCTURE_TYPE chosenStructureType = CollectionUtilities.GetRandomElement(list2);
			yield return MapGenerator.Instance.StartCoroutine(TryCreateSpecialStructure(chosenStructureType, chosenArea));
			locationChoices.Remove(chosenArea);
			for (int m = 0; m < chosenArea.neighbourComponent.neighbours.Count; m++)
			{
				locationChoices.Remove(chosenArea.neighbourComponent.neighbours[m]);
			}
			if (!chosenStructureType.ShouldSpecialStructureBeUnique())
			{
				continue;
			}
			foreach (KeyValuePair<BIOMES, List<STRUCTURE_TYPE>> item in structureChoicesPerBiome)
			{
				BIOMES key = item.Key;
				structureChoicesPerBiome[key].Remove(chosenStructureType);
				if (structureChoicesPerBiome[key].Count < 0)
				{
					RemoveAreasWithBiomeFromLocationChoices(locationChoices, key);
					structureChoicesPerBiome.Remove(key);
				}
			}
		}
		RuinarchListPool<Area>.Release(locationChoices);
		AdditionalResourceCreation();
	}

	private void RemoveAreasWithBiomeFromLocationChoices(List<Area> p_locationChoices, BIOMES p_biome)
	{
		List<Area> list = RuinarchListPool<Area>.Claim(p_locationChoices.Count);
		list.AddRange(p_locationChoices);
		for (int i = 0; i < list.Count; i++)
		{
			Area area = list[i];
			if (area.biomeType == p_biome)
			{
				p_locationChoices.Remove(area);
			}
		}
		RuinarchListPool<Area>.Release(list);
	}

	private void PopulateListWithAreasThatAreFarFromVillageSpotCores(List<Area> p_locationChoices)
	{
		List<Area> list = RuinarchListPool<Area>.Claim(GridMap.Instance.allAreas.Count);
		list.AddRange(GridMap.Instance.allAreas);
		for (int i = 0; i < GridMap.Instance.mainRegion.villageSpots.Count; i++)
		{
			VillageSpot villageSpot = GridMap.Instance.mainRegion.villageSpots[i];
			List<Area> list2 = RuinarchListPool<Area>.Claim();
			villageSpot.coreSpot.PopulateAreasInRange(list2, 2, includeCenterTile: true);
			list.ListRemoveRange(list2);
			RuinarchListPool<Area>.Release(list2);
		}
		for (int j = 0; j < list.Count; j++)
		{
			Area area = list[j];
			if (IsAreaValidForSpecialStructurePlacement(area) && !area.neighbourComponent.HasNeighbourSpecialStructure())
			{
				p_locationChoices.Add(area);
			}
		}
	}

	private bool IsAreaValidForSpecialStructurePlacement(Area p_area)
	{
		if (p_area.elevationComponent.IsFully(ELEVATION.PLAIN) && p_area.featureComponent.features.Count == 0 && p_area.primaryStructureInArea is Wilderness)
		{
			return true;
		}
		return false;
	}

	private IEnumerator TryCreateSpecialStructure(STRUCTURE_TYPE p_structureType, Area p_area)
	{
		NPCSettlement p_settlement = LandmarkManager.Instance.CreateNewSettlement(p_area.region, LOCATION_TYPE.DUNGEON, p_area);
		yield return MapGenerator.Instance.StartCoroutine(CreateSpecialStructure(p_structureType, p_area.region, p_area, p_settlement));
	}

	private IEnumerator CreateSpecialStructure(STRUCTURE_TYPE p_structureType, Region p_region, Area p_area, NPCSettlement p_settlement)
	{
		if (p_structureType == STRUCTURE_TYPE.MONSTER_LAIR)
		{
			LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(p_region, p_structureType, p_settlement);
			yield return MapGenerator.Instance.StartCoroutine(GenerateMonsterLair(p_area, structure));
		}
		else
		{
			yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceBuiltSpecialStructure(p_settlement, p_region.innerMap, RESOURCE.NONE, p_structureType));
		}
	}

	private IEnumerator GenerateMonsterLair(Area hexTile, LocationStructure structure)
	{
		List<LocationGridTile> locationGridTiles = new List<LocationGridTile>(hexTile.gridTileComponent.gridTiles);
		LocationStructure wilderness = hexTile.region.wilderness;
		InnerMapManager.Instance.MonsterLairCellAutomata(locationGridTiles, structure, hexTile.region, wilderness);
		structure.SetOccupiedArea(hexTile);
		yield return null;
	}

	private void AdditionalResourceCreation()
	{
		int count = GridMap.Instance.mainRegion.villageSpots.Count;
		List<Area> list = RuinarchListPool<Area>.Claim();
		List<Area> list2 = RuinarchListPool<Area>.Claim();
		for (int i = 0; i < GridMap.Instance.allAreas.Count; i++)
		{
			Area area = GridMap.Instance.allAreas[i];
			if (area.GetOccupyingVillageSpot() == null)
			{
				List<Area> list3 = RuinarchListPool<Area>.Claim();
				area.PopulateAreasInRange(list3, 1);
				if (list3.All((Area a) => a.GetOccupyingVillageSpot() == null))
				{
					list.Add(area);
				}
				RuinarchListPool<Area>.Release(list3);
			}
			else
			{
				list2.Add(area);
			}
		}
		List<string> list4 = RuinarchListPool<string>.Claim();
		list4.Add("BOAR_DEN");
		list4.Add("WOLF_DEN");
		list4.Add("BEAR_DEN");
		list4.Add("RABBIT_HOLE");
		list4.Add("Game Feature");
		list4.Add("MINK_HOLE");
		list4.Add("MOONCRAWLER_HOLE");
		for (int num = 0; num < count; num++)
		{
			string randomElement = CollectionUtilities.GetRandomElement(list4);
			switch (randomElement)
			{
			case "Game Feature":
			case "RABBIT_HOLE":
			case "MINK_HOLE":
			case "MOONCRAWLER_HOLE":
				if (list2.Count > 0)
				{
					Area randomElement3 = CollectionUtilities.GetRandomElement(list2);
					list2.Remove(randomElement3);
					if (randomElement == "Game Feature")
					{
						randomElement3.featureComponent.AddFeature(AreaFeatureDB.Game_Feature, randomElement3);
					}
					else
					{
						CreateMonsterDen(randomElement, randomElement3);
					}
				}
				break;
			default:
				if (list.Count > 0)
				{
					Area randomElement2 = CollectionUtilities.GetRandomElement(list);
					list.Remove(randomElement2);
					CreateMonsterDen(randomElement, randomElement2);
				}
				break;
			}
		}
		RuinarchListPool<string>.Release(list4);
		RuinarchListPool<Area>.Release(list2);
		RuinarchListPool<Area>.Release(list);
	}

	private void CreateMonsterDen(string randomType, Area randomArea)
	{
		STRUCTURE_TYPE structureType = (STRUCTURE_TYPE)Enum.Parse(typeof(STRUCTURE_TYPE), randomType);
		GameObject firstStructurePrefabForStructure = InnerMapManager.Instance.GetFirstStructurePrefabForStructure(FACTION_TYPE.None, new StructureSetting(structureType, RESOURCE.NONE));
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < randomArea.gridTileComponent.gridTiles.Count; i++)
		{
			LocationGridTile locationGridTile = randomArea.gridTileComponent.gridTiles[i];
			if (locationGridTile.structure is Wilderness && locationGridTile.tileObjectComponent.objHere == null && locationGridTile.IsPassable())
			{
				List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
				locationGridTile.PopulateTilesInRadius(list2, 2, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
				if (!list2.Any((LocationGridTile t) => t.structure.structureType != STRUCTURE_TYPE.WILDERNESS || t.IsAtEdgeOfMap() || !t.IsPassable() || (t.area.GetOccupyingVillageSpot() != null && t.area.GetOccupyingVillageSpot().coreSpot == t.area)))
				{
					list.Add(locationGridTile);
				}
				RuinarchListPool<LocationGridTile>.Release(list2);
			}
		}
		if (list.Count > 0)
		{
			LocationGridTile randomElement = CollectionUtilities.GetRandomElement(list);
			NPCSettlement settlement = LandmarkManager.Instance.CreateNewSettlement(randomArea.region, LOCATION_TYPE.DUNGEON, randomArea);
			LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(settlement, GridMap.Instance.mainRegion.innerMap, firstStructurePrefabForStructure, randomElement);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}
}
