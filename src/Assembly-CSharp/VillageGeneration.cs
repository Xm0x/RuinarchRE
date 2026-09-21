using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Pathfinding;
using UnityEngine;
using UtilityScripts;

public class VillageGeneration : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Creating_Settlements");
		yield return MapGenerator.Instance.StartCoroutine(InitialInnerMapScan());
		yield return MapGenerator.Instance.StartCoroutine(CreateSettlements(GridMap.Instance.mainRegion, data));
		if (succeess)
		{
			ApplyPreGeneratedCharacterRelationships(data);
			for (int i = 0; i < DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count; i++)
			{
				DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[i].migrationComponent.ForceRandomizePerHourIncrement();
			}
			yield return null;
		}
	}

	private IEnumerator CreateSettlements(Region region, MapGenerationData data)
	{
		List<NPCSettlement> createdSettlements = RuinarchListPool<NPCSettlement>.Claim();
		List<VillageSetting> villageSettings = RuinarchListPool<VillageSetting>.Claim();
		foreach (KeyValuePair<FactionTemplate, List<VillageSpot>> setting in data.determinedVillages)
		{
			FactionTemplate factionTemplate = setting.Key;
			RACE race = ((factionTemplate.factionType == FACTION_TYPE.Elven_Kingdom) ? RACE.ELVES : ((factionTemplate.factionType == FACTION_TYPE.Human_Empire) ? RACE.HUMANS : ((!GameUtilities.RollChance(50)) ? RACE.HUMANS : RACE.ELVES)));
			Faction faction = FactionManager.Instance.CreateNewFaction(factionTemplate.factionType, factionTemplate.name, factionTemplate.factionEmblem, race);
			faction.factionType.SetAsDefault(faction);
			LOCATION_TYPE locationType = GetLocationTypeForRace(faction.race);
			for (int i = 0; i < setting.Value.Count; i++)
			{
				VillageSpot villageSpot = setting.Value[i];
				Area coreSpot = villageSpot.coreSpot;
				VillageSetting item = factionTemplate.villageSettings[i];
				NPCSettlement nPCSettlement = LandmarkManager.Instance.CreateNewSettlement(region, locationType, coreSpot);
				nPCSettlement.SetOccupiedVillageSpot(villageSpot);
				createdSettlements.Add(nPCSettlement);
				villageSettings.Add(item);
				nPCSettlement.SetName(item.villageName);
				LandmarkManager.Instance.OwnSettlement(faction, nPCSettlement);
				SETTLEMENT_TYPE settlementTypeForFaction = LandmarkManager.Instance.GetSettlementTypeForFaction(faction);
				nPCSettlement.SetSettlementType(settlementTypeForFaction);
				List<StructureSetting> structureSettings = GenerateCityCenter(faction);
				yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, faction.factionType.type, structureSettings, nPCSettlement, data));
				RuinarchListPool<StructureSetting>.Release(structureSettings);
			}
		}
		for (int i = 0; i < createdSettlements.Count; i++)
		{
			NPCSettlement npcSettlement = createdSettlements[i];
			int neededBasicResourceProducingStructures = villageSettings[i].GetBasicResourceProducingStructureCount();
			int missingBasicResourceProducers = 0;
			for (int j = 0; j < neededBasicResourceProducingStructures; j++)
			{
				List<StructureSetting> structureSettings = RuinarchListPool<StructureSetting>.Claim();
				StructureSetting item2;
				if (npcSettlement.owner.factionType.type == FACTION_TYPE.Human_Empire)
				{
					item2 = npcSettlement.owner.factionType.CreateStructureSettingForStructure(STRUCTURE_TYPE.MINE, npcSettlement);
				}
				else if (npcSettlement.owner.factionType.type == FACTION_TYPE.Elven_Kingdom)
				{
					item2 = npcSettlement.owner.factionType.CreateStructureSettingForStructure(STRUCTURE_TYPE.LUMBERYARD, npcSettlement);
				}
				else if (GameUtilities.RollChance(50))
				{
					List<StructureConnector> list = RuinarchListPool<StructureConnector>.Claim();
					npcSettlement.PopulateStructureConnectorsForStructureType(list, STRUCTURE_TYPE.MINE);
					bool flag = list.Count > 0;
					RuinarchListPool<StructureConnector>.Release(list);
					item2 = npcSettlement.owner.factionType.CreateStructureSettingForStructure(flag ? STRUCTURE_TYPE.MINE : STRUCTURE_TYPE.LUMBERYARD, npcSettlement);
				}
				else
				{
					List<StructureConnector> list2 = RuinarchListPool<StructureConnector>.Claim();
					npcSettlement.PopulateStructureConnectorsForStructureType(list2, STRUCTURE_TYPE.LUMBERYARD);
					bool flag2 = list2.Count > 0;
					RuinarchListPool<StructureConnector>.Release(list2);
					item2 = npcSettlement.owner.factionType.CreateStructureSettingForStructure(flag2 ? STRUCTURE_TYPE.LUMBERYARD : STRUCTURE_TYPE.MINE, npcSettlement);
				}
				structureSettings.Add(item2);
				yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, npcSettlement.owner.factionType.type, structureSettings, npcSettlement, data));
				if (data.unplacedStructuresOnLastEnsuredStructurePlacementCall != null && data.unplacedStructuresOnLastEnsuredStructurePlacementCall.Count > 0)
				{
					missingBasicResourceProducers += data.unplacedStructuresOnLastEnsuredStructurePlacementCall.Count;
					Debug.Log($"{npcSettlement.name} was unable to place {data.unplacedStructuresOnLastEnsuredStructurePlacementCall.ComafyList()}. Missing Basic Resource Structures are: {missingBasicResourceProducers}");
				}
				RuinarchListPool<StructureSetting>.Release(structureSettings);
				data.SetMissingBasicResourceProducers(npcSettlement, missingBasicResourceProducers);
			}
		}
		for (int i = 0; i < createdSettlements.Count; i++)
		{
			NPCSettlement npcSettlement = createdSettlements[i];
			VillageSetting p_villageSetting = villageSettings[i];
			List<StructureSetting> structureSettings = GenerateDwellings(npcSettlement.owner, p_villageSetting, npcSettlement);
			int missingBasicResourceProducers = structureSettings.Count;
			yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, npcSettlement.owner.factionType.type, structureSettings, npcSettlement, data));
			RuinarchListPool<StructureSetting>.Release(structureSettings);
			if (npcSettlement.GetNumberOfStructures(STRUCTURE_TYPE.DWELLING) < missingBasicResourceProducers)
			{
				succeess = false;
				yield break;
			}
		}
		for (int k = 0; k < createdSettlements.Count; k++)
		{
			NPCSettlement nPCSettlement2 = createdSettlements[k];
			if (nPCSettlement2.structures.ContainsKey(STRUCTURE_TYPE.DWELLING))
			{
				int count = nPCSettlement2.structures[STRUCTURE_TYPE.DWELLING].Count;
				List<Character> list3 = GenerateSettlementResidents(count, nPCSettlement2, nPCSettlement2.owner, data);
				RandomizeCharacterClassesBasedOnTalents(nPCSettlement2, list3);
				List<TileObject> list4 = RuinarchListPool<TileObject>.Claim();
				nPCSettlement2.PopulateTileObjectsFromStructures<TileObject>(list4, STRUCTURE_TYPE.DWELLING);
				for (int l = 0; l < list4.Count; l++)
				{
					list4[l].UpdateOwners();
				}
				RuinarchListPool<TileObject>.Release(list4);
				CharacterManager.Instance.PlaceInitialCharacters(list3, nPCSettlement2);
			}
		}
		for (int i = 0; i < createdSettlements.Count; i++)
		{
			NPCSettlement npcSettlement = createdSettlements[i];
			int missingBasicResourceProducers = villageSettings[i].GetFoodProducingStructureCount();
			int neededBasicResourceProducingStructures = 0;
			for (int j = 0; j < missingBasicResourceProducers; j++)
			{
				bool wasStructurePlaced = false;
				List<StructureSetting> structureSettings = RuinarchListPool<StructureSetting>.Claim();
				if (ShouldBuildFishery(npcSettlement))
				{
					StructureSetting item3 = new StructureSetting(STRUCTURE_TYPE.FISHERY, RESOURCE.NONE);
					structureSettings.Add(item3);
					yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, npcSettlement.owner.factionType.type, structureSettings, npcSettlement, data));
					if (data.unplacedStructuresOnLastEnsuredStructurePlacementCall.Count <= 0)
					{
						wasStructurePlaced = true;
					}
				}
				if (!wasStructurePlaced && ShouldBuildButcher(npcSettlement))
				{
					structureSettings.Clear();
					StructureSetting item3 = new StructureSetting(STRUCTURE_TYPE.BUTCHERS_SHOP, RESOURCE.NONE);
					structureSettings.Add(item3);
					yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, npcSettlement.owner.factionType.type, structureSettings, npcSettlement, data));
					if (data.unplacedStructuresOnLastEnsuredStructurePlacementCall.Count <= 0)
					{
						wasStructurePlaced = true;
					}
				}
				if (!wasStructurePlaced)
				{
					structureSettings.Clear();
					StructureSetting item3 = new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.NONE);
					structureSettings.Add(item3);
					yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, npcSettlement.owner.factionType.type, structureSettings, npcSettlement, data));
					if (data.unplacedStructuresOnLastEnsuredStructurePlacementCall.Count <= 0)
					{
						wasStructurePlaced = true;
					}
				}
				if (!wasStructurePlaced)
				{
					neededBasicResourceProducingStructures++;
				}
				RuinarchListPool<StructureSetting>.Release(structureSettings);
			}
			data.SetMissingFoodProducers(npcSettlement, neededBasicResourceProducingStructures);
		}
		List<STRUCTURE_TYPE> specialStructureTypes = RuinarchListPool<STRUCTURE_TYPE>.Claim();
		for (int i = 0; i < createdSettlements.Count; i++)
		{
			NPCSettlement npcSettlement = createdSettlements[i];
			int specialStructureCount = villageSettings[i].GetSpecialStructureCount();
			int totalMissingProductionStructures = data.GetTotalMissingProductionStructures(npcSettlement);
			int neededBasicResourceProducingStructures = specialStructureCount + totalMissingProductionStructures;
			specialStructureTypes.Clear();
			specialStructureTypes.Add(STRUCTURE_TYPE.PRISON);
			specialStructureTypes.Add(STRUCTURE_TYPE.CEMETERY);
			specialStructureTypes.Add(STRUCTURE_TYPE.TAVERN);
			specialStructureTypes.Add(STRUCTURE_TYPE.HOSPICE);
			specialStructureTypes.Add(STRUCTURE_TYPE.WORKSHOP);
			for (int missingBasicResourceProducers = 0; missingBasicResourceProducers < neededBasicResourceProducingStructures; missingBasicResourceProducers++)
			{
				if (specialStructureTypes.Count <= 0)
				{
					break;
				}
				STRUCTURE_TYPE randomElement = CollectionUtilities.GetRandomElement(specialStructureTypes);
				specialStructureTypes.Remove(randomElement);
				List<StructureSetting> structureSettings = RuinarchListPool<StructureSetting>.Claim();
				StructureSetting item4 = npcSettlement.owner.factionType.CreateStructureSettingForStructure(randomElement, npcSettlement);
				structureSettings.Add(item4);
				yield return MapGenerator.Instance.StartCoroutine(EnsuredStructurePlacement(region, npcSettlement.owner.factionType.type, structureSettings, npcSettlement, data));
				RuinarchListPool<StructureSetting>.Release(structureSettings);
			}
		}
		for (int i = 0; i < createdSettlements.Count; i++)
		{
			NPCSettlement nPCSettlement3 = createdSettlements[i];
			yield return MapGenerator.Instance.StartCoroutine(nPCSettlement3.PlaceInitialObjectsForWorldGenCoroutine());
		}
		RuinarchListPool<NPCSettlement>.Release(createdSettlements);
		RuinarchListPool<VillageSetting>.Release(villageSettings);
	}

	private bool ShouldBuildFishery(NPCSettlement p_settlement)
	{
		if (p_settlement.HasStructure(STRUCTURE_TYPE.FISHERY))
		{
			return false;
		}
		if (p_settlement.owner != null && p_settlement.owner.factionType.IsActionConsideredACrime(CRIME_TYPE.Animal_Killing))
		{
			return false;
		}
		if (!p_settlement.occupiedVillageSpot.HasUnusedFishingSpot())
		{
			return false;
		}
		if (!p_settlement.HasResidentThatIsOrCanBecomeClass("Fisher"))
		{
			return false;
		}
		return true;
	}

	private bool ShouldBuildButcher(NPCSettlement p_settlement)
	{
		if (p_settlement.HasStructure(STRUCTURE_TYPE.BUTCHERS_SHOP))
		{
			return false;
		}
		if (p_settlement.owner != null && p_settlement.owner.factionType.IsActionConsideredACrime(CRIME_TYPE.Animal_Killing))
		{
			return false;
		}
		if (!p_settlement.occupiedVillageSpot.HasAccessToButcherAnimals())
		{
			return false;
		}
		if (!p_settlement.HasResidentThatIsOrCanBecomeClass("Butcher"))
		{
			return false;
		}
		return true;
	}

	private List<StructureSetting> GenerateCityCenter(Faction p_faction)
	{
		List<StructureSetting> list = RuinarchListPool<StructureSetting>.Claim();
		list.Add(new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, p_faction.factionType.mainResource));
		return list;
	}

	private List<StructureSetting> GenerateDwellings(Faction p_faction, VillageSetting p_villageSetting, NPCSettlement p_settlement)
	{
		List<StructureSetting> list = RuinarchListPool<StructureSetting>.Claim();
		int randomDwellingCount = p_villageSetting.GetRandomDwellingCount();
		StructureSetting item = p_faction.factionType.CreateStructureSettingForStructure(STRUCTURE_TYPE.DWELLING, p_settlement);
		for (int i = 0; i < randomDwellingCount; i++)
		{
			list.Add(item);
		}
		return list;
	}

	private IEnumerator EnsuredStructurePlacement(Region region, FACTION_TYPE p_factionType, List<StructureSetting> structureSettings, NPCSettlement npcSettlement, MapGenerationData p_data)
	{
		List<StructureSetting> unplacedStructures = new List<StructureSetting>();
		List<StructureSetting> structuresToPlace = new List<StructureSetting>(structureSettings);
		if (!npcSettlement.HasStructure(STRUCTURE_TYPE.CITY_CENTER))
		{
			StructureSetting cityCenter = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, npcSettlement.owner.factionType.mainResource);
			yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceFirstStructureForSettlement(npcSettlement.owner.factionType.type, npcSettlement, region.innerMap, cityCenter));
			structuresToPlace.Remove(cityCenter);
		}
		for (int i = 0; i < 4; i++)
		{
			yield return MapGenerator.Instance.StartCoroutine(PlaceStructures(region, p_factionType, structuresToPlace, npcSettlement, p_data));
			unplacedStructures.Clear();
			unplacedStructures.AddRange(structuresToPlace);
			for (int j = 0; j < p_data.LastPlacedStructureTypes.Count; j++)
			{
				STRUCTURE_TYPE sTRUCTURE_TYPE = p_data.LastPlacedStructureTypes[j];
				for (int k = 0; k < unplacedStructures.Count; k++)
				{
					if (sTRUCTURE_TYPE == unplacedStructures[k].structureType)
					{
						unplacedStructures.RemoveAt(k);
						break;
					}
				}
			}
			if (unplacedStructures.Count == 0)
			{
				break;
			}
			structuresToPlace.Clear();
			structuresToPlace.AddRange(unplacedStructures);
		}
		p_data.SetLastUnplacedStructures(unplacedStructures);
	}

	private IEnumerator InitialInnerMapScan()
	{
		for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++)
		{
			InnerTileMap innerTileMap = InnerMapManager.Instance.innerMaps[i];
			foreach (Progress item in AstarPath.active.ScanAsync(new NavGraph[2] { innerTileMap.pathfindingGraph, innerTileMap.unwalkableGraph }))
			{
				_ = item;
			}
			yield return null;
		}
	}

	private IEnumerator PlaceStructures(Region region, FACTION_TYPE p_factionType, List<StructureSetting> structureSettings, NPCSettlement npcSettlement, MapGenerationData p_data)
	{
		p_data.ClearLastPlacedVillageStructures();
		for (int i = 0; i < structureSettings.Count; i++)
		{
			StructureSetting structureToPlace = structureSettings[i];
			if (structureToPlace.structureType != STRUCTURE_TYPE.CITY_CENTER)
			{
				yield return MapGenerator.Instance.StartCoroutine(PlaceStructure(region, p_factionType, structureToPlace, npcSettlement, p_data));
			}
		}
		yield return null;
	}

	private IEnumerator PlaceStructure(Region region, FACTION_TYPE p_factionType, StructureSetting structureToPlace, NPCSettlement npcSettlement, MapGenerationData p_data)
	{
		List<StructureConnector> list = RuinarchListPool<StructureConnector>.Claim();
		npcSettlement.PopulateStructureConnectorsForStructureType(list, structureToPlace.structureType);
		StructureConnector structureConnector;
		LocationGridTile targetTile;
		string structurePrefabName;
		LocationGridTile connectorTile;
		string functionLog;
		if (structureToPlace.structureType == STRUCTURE_TYPE.MINE || structureToPlace.structureType == STRUCTURE_TYPE.FISHERY)
		{
			list = list.OrderBy((StructureConnector c) => Vector2.Distance(c.transform.position, npcSettlement.cityCenter.tiles.ElementAt(0).centeredWorldLocation)).ToList();
			structureConnector = LandmarkManager.Instance.CanPlaceStructureBlueprintMine(p_factionType, npcSettlement, structureToPlace, list, out targetTile, out structurePrefabName, out var _, out connectorTile, out var _, out functionLog);
		}
		else
		{
			CollectionUtilities.Shuffle(list);
			structureConnector = LandmarkManager.Instance.CanPlaceStructureBlueprintDefault(npcSettlement.owner.factionType.type, npcSettlement, structureToPlace, list, out targetTile, out structurePrefabName, out var _, out connectorTile, out var _, out functionLog);
		}
		if (structureConnector != null)
		{
			GameObject originalObjectFromPool = ObjectPoolManager.Instance.GetOriginalObjectFromPool(structurePrefabName);
			LocationStructure locationStructure = LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(npcSettlement, region.innerMap, originalObjectFromPool, targetTile);
			if (locationStructure is ManMadeStructure manMadeStructure)
			{
				manMadeStructure.OnUseStructureConnector(connectorTile);
			}
			p_data.AddLastPlacedStructureTypes(locationStructure.structureType);
		}
		RuinarchListPool<StructureConnector>.Release(list);
		yield return null;
	}

	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}

	private void GenerateResidentConfiguration(int providedCitizenCount, int dwellingCount, out int coupleCharacters, out int singleCharacters)
	{
		singleCharacters = 0;
		coupleCharacters = 0;
		if (providedCitizenCount != -1 && dwellingCount < providedCitizenCount)
		{
			int num = providedCitizenCount;
			int num2 = dwellingCount;
			for (int i = 0; i < dwellingCount; i++)
			{
				if (num2 >= num)
				{
					singleCharacters++;
					num--;
				}
				else
				{
					coupleCharacters++;
					num -= 2;
				}
				num2--;
			}
			return;
		}
		if (dwellingCount == 2)
		{
			coupleCharacters = 2;
			singleCharacters = 0;
			return;
		}
		for (int j = 0; j < dwellingCount; j++)
		{
			if (GameUtilities.RollChance(35))
			{
				coupleCharacters++;
			}
			else
			{
				singleCharacters++;
			}
			if (providedCitizenCount > 0 && singleCharacters + coupleCharacters * 2 >= providedCitizenCount)
			{
				break;
			}
		}
	}

	private List<Character> GenerateSettlementResidents(int dwellingCount, NPCSettlement npcSettlement, Faction faction, MapGenerationData data, int providedCitizenCount = -1)
	{
		GenerateResidentConfiguration(providedCitizenCount, dwellingCount, out var coupleCharacters, out var singleCharacters);
		List<Character> createdCharacters = new List<Character>();
		int citizenCount = 0;
		for (int i = 0; i < coupleCharacters; i++)
		{
			List<Dwelling> availableDwellingsAtSettlement = GetAvailableDwellingsAtSettlement(npcSettlement);
			if (availableDwellingsAtSettlement.Count == 0)
			{
				break;
			}
			Dwelling randomElement = CollectionUtilities.GetRandomElement(availableDwellingsAtSettlement);
			CreateCouple(npcSettlement, faction, data, randomElement, ref createdCharacters, ref citizenCount);
		}
		for (int j = 0; j < singleCharacters; j++)
		{
			List<Dwelling> availableDwellingsAtSettlement2 = GetAvailableDwellingsAtSettlement(npcSettlement);
			if (availableDwellingsAtSettlement2.Count == 0)
			{
				break;
			}
			Dwelling randomElement2 = CollectionUtilities.GetRandomElement(availableDwellingsAtSettlement2);
			CreateSingleCharacter(npcSettlement, faction, data, randomElement2, ref createdCharacters, ref citizenCount);
		}
		if (GameUtilities.RollChance(10))
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			list.AddRange(createdCharacters);
			int index = GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1);
			Character character = list[index];
			Character character2 = null;
			list.RemoveAt(index);
			while (character2 == null && list.Count > 0)
			{
				int index2 = GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1);
				character2 = list[index2];
				if (character.homeStructure == character2.homeStructure)
				{
					character2 = null;
					list.RemoveAt(index2);
				}
			}
			if (character2 != null)
			{
				character.traitContainer.BecomeObsessWith(character, character2);
			}
		}
		return createdCharacters;
	}

	private void CreateSingleCharacter(NPCSettlement npcSettlement, Faction faction, MapGenerationData data, Dwelling dwelling, ref List<Character> createdCharacters, ref int citizenCount, string providedClass = "")
	{
		PreCharacterData availableSingleCharacterForSettlement = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
		if (availableSingleCharacterForSettlement != null)
		{
			createdCharacters.Add(SpawnCharacter(availableSingleCharacterForSettlement, string.IsNullOrEmpty(providedClass) ? "Farmer" : providedClass, dwelling, faction, npcSettlement));
			citizenCount++;
			return;
		}
		Debug.LogWarning("Could not find any more characters to spawn");
		FamilyTree familyTree = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
		DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(familyTree);
		availableSingleCharacterForSettlement = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
		createdCharacters.Add(SpawnCharacter(availableSingleCharacterForSettlement, string.IsNullOrEmpty(providedClass) ? "Farmer" : providedClass, dwelling, faction, npcSettlement));
		citizenCount++;
	}

	private void CreateCouple(NPCSettlement npcSettlement, Faction faction, MapGenerationData data, Dwelling dwelling, ref List<Character> createdCharacters, ref int citizenCount, string providedClass1 = "", string providedClass2 = "")
	{
		List<Couple> availableCouplesToBeSpawned = GetAvailableCouplesToBeSpawned(faction.race, data);
		if (availableCouplesToBeSpawned.Count > 0)
		{
			Couple randomElement = CollectionUtilities.GetRandomElement(availableCouplesToBeSpawned);
			createdCharacters.AddRange(SpawnCouple(randomElement, dwelling, faction, npcSettlement, providedClass1, providedClass2));
			citizenCount += 2;
			return;
		}
		List<Couple> availableSiblingCouplesToBeSpawned = GetAvailableSiblingCouplesToBeSpawned(faction.race, data);
		if (availableSiblingCouplesToBeSpawned.Count > 0)
		{
			Couple randomElement2 = CollectionUtilities.GetRandomElement(availableSiblingCouplesToBeSpawned);
			createdCharacters.AddRange(SpawnCouple(randomElement2, dwelling, faction, npcSettlement, providedClass1, providedClass2));
			citizenCount += 2;
			return;
		}
		PreCharacterData availableSingleCharacterForSettlement = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
		if (availableSingleCharacterForSettlement != null)
		{
			createdCharacters.Add(SpawnCharacter(availableSingleCharacterForSettlement, string.IsNullOrEmpty(providedClass1) ? "Farmer" : providedClass1, dwelling, faction, npcSettlement));
			citizenCount++;
			return;
		}
		Debug.LogWarning("Could not find any more characters to spawn. Generating a new family tree.");
		FamilyTree familyTree = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
		DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(familyTree);
		availableSingleCharacterForSettlement = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
		createdCharacters.Add(SpawnCharacter(availableSingleCharacterForSettlement, string.IsNullOrEmpty(providedClass1) ? "Farmer" : providedClass1, dwelling, faction, npcSettlement));
		citizenCount++;
	}

	private List<Couple> GetAvailableCouplesToBeSpawned(RACE race, MapGenerationData data)
	{
		List<Couple> list = new List<Couple>();
		List<FamilyTree> list2 = DatabaseManager.Instance.familyTreeDatabase.allFamilyTreesDictionary[race];
		for (int i = 0; i < list2.Count; i++)
		{
			FamilyTree familyTree = list2[i];
			for (int j = 0; j < familyTree.allFamilyMembers.Count; j++)
			{
				PreCharacterData preCharacterData = familyTree.allFamilyMembers[j];
				if (preCharacterData.hasBeenSpawned)
				{
					continue;
				}
				PreCharacterData characterWithRelationship = preCharacterData.GetCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER, DatabaseManager.Instance.familyTreeDatabase);
				if (characterWithRelationship != null && !characterWithRelationship.hasBeenSpawned)
				{
					Couple item = new Couple(preCharacterData, characterWithRelationship);
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
		}
		return list;
	}

	private List<Couple> GetAvailableSiblingCouplesToBeSpawned(RACE race, MapGenerationData data)
	{
		List<Couple> list = new List<Couple>();
		List<FamilyTree> list2 = DatabaseManager.Instance.familyTreeDatabase.allFamilyTreesDictionary[race];
		for (int i = 0; i < list2.Count; i++)
		{
			FamilyTree familyTree = list2[i];
			if (familyTree.children == null || familyTree.children.Count < 2)
			{
				continue;
			}
			List<PreCharacterData> list3 = familyTree.children.Where((PreCharacterData x) => !x.hasBeenSpawned).ToList();
			if (list3.Count >= 2)
			{
				PreCharacterData randomElement = CollectionUtilities.GetRandomElement(list3);
				list3.Remove(randomElement);
				PreCharacterData randomElement2 = CollectionUtilities.GetRandomElement(list3);
				Couple item = new Couple(randomElement, randomElement2);
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	private PreCharacterData GetAvailableSingleCharacterForSettlement(RACE race, MapGenerationData data, NPCSettlement npcSettlement)
	{
		List<PreCharacterData> list = new List<PreCharacterData>();
		List<FamilyTree> list2 = DatabaseManager.Instance.familyTreeDatabase.allFamilyTreesDictionary[race];
		for (int i = 0; i < list2.Count; i++)
		{
			FamilyTree familyTree = list2[i];
			for (int j = 0; j < familyTree.allFamilyMembers.Count; j++)
			{
				PreCharacterData preCharacterData = familyTree.allFamilyMembers[j];
				if (!preCharacterData.hasBeenSpawned)
				{
					PreCharacterData characterWithRelationship = preCharacterData.GetCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER, DatabaseManager.Instance.familyTreeDatabase);
					if (characterWithRelationship == null || !characterWithRelationship.hasBeenSpawned || CharacterManager.Instance.GetCharacterByID(characterWithRelationship.id)?.homeSettlement != npcSettlement)
					{
						list.Add(preCharacterData);
					}
				}
			}
		}
		if (list.Count > 0)
		{
			return CollectionUtilities.GetRandomElement(list);
		}
		return null;
	}

	private List<Dwelling> GetAvailableDwellingsAtSettlement(NPCSettlement npcSettlement)
	{
		List<Dwelling> list = new List<Dwelling>();
		if (npcSettlement.structures.ContainsKey(STRUCTURE_TYPE.DWELLING))
		{
			List<LocationStructure> list2 = npcSettlement.structures[STRUCTURE_TYPE.DWELLING];
			for (int i = 0; i < list2.Count; i++)
			{
				Dwelling dwelling = list2[i] as Dwelling;
				if (dwelling.residents.Count == 0)
				{
					list.Add(dwelling);
				}
			}
		}
		return list;
	}

	private List<Character> SpawnCouple(Couple couple, Dwelling dwelling, Faction faction, NPCSettlement npcSettlement, string className1 = "", string className2 = "")
	{
		return new List<Character>
		{
			SpawnCharacter(couple.character1, string.IsNullOrEmpty(className1) ? "Farmer" : className1, dwelling, faction, npcSettlement),
			SpawnCharacter(couple.character2, string.IsNullOrEmpty(className2) ? "Farmer" : className2, dwelling, faction, npcSettlement)
		};
	}

	private Character SpawnCharacter(PreCharacterData data, string className, Dwelling dwelling, Faction faction, NPCSettlement npcSettlement)
	{
		return CharacterManager.Instance.CreateNewCharacter(data, className, faction, npcSettlement, dwelling, delegate(Character character)
		{
			AfterCharacterInitializationProcess(character, faction);
		});
	}

	private void AfterCharacterInitializationProcess(Character p_character, Faction p_faction)
	{
		if (p_faction.factionType.type == FACTION_TYPE.Demon_Cult)
		{
			p_character.religionComponent.ChangeReligion(RELIGION.Demon_Worship);
		}
	}

	private void RandomizeCharacterClassesBasedOnTalents(NPCSettlement npcSettlement, List<Character> spawnedCharacters)
	{
		List<string> list = RuinarchListPool<string>.Claim();
		string text = "Food Producer";
		string text2 = "Basic Resource Producer";
		string text3 = "Special Civilian";
		list.Add(text);
		list.Add(text2);
		list.Add(text3);
		List<Character> list2 = RuinarchListPool<Character>.Claim();
		list2.AddRange(spawnedCharacters);
		int numberOfNeededCombatants = SettlementClassComponent.GetNumberOfNeededCombatants(spawnedCharacters.Count);
		for (int i = 0; i < numberOfNeededCombatants; i++)
		{
			Character randomElement = CollectionUtilities.GetRandomElement(list2);
			list2.Remove(randomElement);
			string className;
			if (ChanceData.RollChance(CHANCE_TYPE.Noble_Chance))
			{
				className = "Noble";
			}
			else
			{
				List<string> list3 = RuinarchListPool<string>.Claim();
				randomElement.classComponent.PopulateAbleCombatantClasses(list3);
				className = CollectionUtilities.GetRandomElement(list3);
				RuinarchListPool<string>.Release(list3);
			}
			randomElement.classComponent.AssignClass(className, isInitial: true);
			randomElement.classComponent.OnUpdateCharacterClass();
		}
		int num = spawnedCharacters.Count - numberOfNeededCombatants;
		int num2 = 0;
		for (int j = 0; j < num; j++)
		{
			Character randomElement2 = CollectionUtilities.GetRandomElement(list2);
			list2.Remove(randomElement2);
			string text4 = list[num2];
			num2++;
			if (num2 >= list.Count)
			{
				num2 = 0;
			}
			List<string> list4 = RuinarchListPool<string>.Claim();
			if (text4 == text)
			{
				randomElement2.classComponent.PopulateAbleFoodProducerClasses(list4);
			}
			else if (text4 == text2)
			{
				randomElement2.classComponent.PopulateBasicProducerClasses(list4, npcSettlement.owner.factionType.type);
			}
			else if (text4 == text3)
			{
				randomElement2.classComponent.PopulateAbleSpecialCivilianClasses(list4);
			}
			string randomElement3 = CollectionUtilities.GetRandomElement(list4);
			randomElement2.classComponent.AssignClass(randomElement3, isInitial: true);
			randomElement2.classComponent.OnUpdateCharacterClass();
			RuinarchListPool<string>.Release(list4);
		}
		RuinarchListPool<Character>.Release(list2);
		RuinarchListPool<string>.Release(list);
	}

	private void ApplyPreGeneratedCharacterRelationships(MapGenerationData data)
	{
		foreach (KeyValuePair<RACE, List<FamilyTree>> item in DatabaseManager.Instance.familyTreeDatabase.allFamilyTreesDictionary)
		{
			for (int i = 0; i < item.Value.Count; i++)
			{
				FamilyTree familyTree = item.Value[i];
				for (int j = 0; j < familyTree.allFamilyMembers.Count; j++)
				{
					PreCharacterData preCharacterData = familyTree.allFamilyMembers[j];
					if (preCharacterData.hasBeenSpawned)
					{
						Character characterByID = CharacterManager.Instance.GetCharacterByID(preCharacterData.id);
						RelationshipManager.Instance.ApplyPreGeneratedRelationships(preCharacterData, characterByID);
					}
				}
			}
		}
	}

	private LOCATION_TYPE GetLocationTypeForRace(RACE race)
	{
		if ((uint)(race - 1) <= 1u)
		{
			return LOCATION_TYPE.VILLAGE;
		}
		throw new Exception("There was no location type provided for race " + race.ToStringEnum());
	}
}
