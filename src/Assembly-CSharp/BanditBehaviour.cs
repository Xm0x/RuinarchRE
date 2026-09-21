using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class BanditBehaviour : CharacterBehaviour
{
	public BanditBehaviour()
	{
		base.priority = 9;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		LocationStructure homeStructure = character.homeStructure;
		Region mainRegion = GridMap.Instance.mainRegion;
		if (homeStructure == null)
		{
			LocationStructure structureOfTypeWithLowestResidentCountOwnedBy = mainRegion.GetStructureOfTypeWithLowestResidentCountOwnedBy(STRUCTURE_TYPE.BANDIT_CAMP, FactionManager.Instance.banditFaction);
			if (structureOfTypeWithLowestResidentCountOwnedBy != null)
			{
				character.MigrateHomeStructureTo(structureOfTypeWithLowestResidentCountOwnedBy);
				return true;
			}
			structureOfTypeWithLowestResidentCountOwnedBy = mainRegion.GetStructureOfTypeWithNoResidentAndIsUnowned(STRUCTURE_TYPE.BANDIT_CAMP);
			if (structureOfTypeWithLowestResidentCountOwnedBy != null)
			{
				character.MigrateHomeStructureTo(structureOfTypeWithLowestResidentCountOwnedBy);
				return true;
			}
			if (TryBuildBanditCamp(character, ref log, ref producedJob))
			{
				return true;
			}
			if (!character.HasHome() && character.currentRegion != null)
			{
				Area randomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage = character.currentRegion.GetRandomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage(character);
				if (randomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage != null)
				{
					character.SetTerritory(randomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage);
					producedJob = null;
					return false;
				}
			}
		}
		else if (homeStructure.structureType == STRUCTURE_TYPE.BANDIT_CAMP)
		{
			PartyLogic(character, ref log);
			int numberOfResidentsThatIsAlive = homeStructure.GetNumberOfResidentsThatIsAlive();
			if (character.tileObjectComponent.primaryBed == null || character.tileObjectComponent.primaryBed.gridTileLocation == null || character.tileObjectComponent.primaryBed.gridTileLocation.structure != homeStructure)
			{
				ClaimABed(character, homeStructure);
			}
			bool isAtHomeStructure = character.isAtHomeStructure;
			if (isAtHomeStructure)
			{
				int numberOfResidentsThatIsAliveVillager = homeStructure.GetNumberOfResidentsThatIsAliveVillager();
				DAILY_SCHEDULE scheduleType = character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick);
				if (TryDoScheduledNeedsAtHomeBehaviour(character, scheduleType, ref log, ref producedJob) && producedJob != null)
				{
					return true;
				}
				if (character.characterClass.className == "Crafter" && TryDoScheduledWorkAtHomeForCraftersBehaviour(character, scheduleType, homeStructure, numberOfResidentsThatIsAliveVillager, ref log, ref producedJob) && producedJob != null)
				{
					return true;
				}
				if (TryDoScheduledNormalWorkAtHomeBehaviour(character, scheduleType, homeStructure, ref log, ref producedJob) && producedJob != null)
				{
					return true;
				}
				switch (scheduleType)
				{
				case DAILY_SCHEDULE.Work:
				{
					int totalResourceInStructure = homeStructure.GetTotalResourceInStructure(RESOURCE.FOOD);
					int num = numberOfResidentsThatIsAliveVillager * 20;
					if (totalResourceInStructure < num)
					{
						if (TryFindNearestFoodNearby(character, homeStructure, ref log, ref producedJob) && producedJob != null)
						{
							return true;
						}
						if (TryFindNearestFoodInForageStructures(character, homeStructure, ref log, ref producedJob) && producedJob != null)
						{
							return true;
						}
					}
					break;
				}
				case DAILY_SCHEDULE.Free_Time:
					if (VisitBanditCamps(character, ref log, ref producedJob))
					{
						return true;
					}
					break;
				}
			}
			else if (GameUtilities.RollChance(90, ref log) && character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob))
			{
				return true;
			}
			if (character.moodComponent.moodState != MOOD_STATE.Bad && character.moodComponent.moodState != MOOD_STATE.Critical && character.behaviourComponent.PlanSettlementOrFactionWorkActions(out producedJob))
			{
				return true;
			}
			if (numberOfResidentsThatIsAlive >= 10 && GameUtilities.RollChance(40, ref log))
			{
				LocationStructure structureOfTypeWithLowestResidentCountOwnedBy2 = mainRegion.GetStructureOfTypeWithLowestResidentCountOwnedBy(STRUCTURE_TYPE.BANDIT_CAMP, FactionManager.Instance.banditFaction);
				if (structureOfTypeWithLowestResidentCountOwnedBy2 != null && structureOfTypeWithLowestResidentCountOwnedBy2.GetNumberOfResidentsThatIsAlive() < 10)
				{
					character.MigrateHomeStructureTo(structureOfTypeWithLowestResidentCountOwnedBy2);
					return true;
				}
				structureOfTypeWithLowestResidentCountOwnedBy2 = mainRegion.GetStructureOfTypeWithNoResidentAndIsUnowned(STRUCTURE_TYPE.BANDIT_CAMP);
				if (structureOfTypeWithLowestResidentCountOwnedBy2 != null)
				{
					character.MigrateHomeStructureTo(structureOfTypeWithLowestResidentCountOwnedBy2);
					return true;
				}
				if (TryBuildBanditCamp(character, ref log, ref producedJob))
				{
					return true;
				}
				if (!character.HasHome() && character.currentRegion != null)
				{
					Area randomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage2 = character.currentRegion.GetRandomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage(character);
					if (randomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage2 != null)
					{
						character.SetTerritory(randomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage2);
						producedJob = null;
						return false;
					}
				}
			}
			if (isAtHomeStructure && TryDoIdleBehaviour(character, ref log, ref producedJob) && producedJob != null)
			{
				return true;
			}
		}
		return character.jobComponent.TriggerRoamAroundTile(out producedJob);
	}

	private void ClaimABed(Character character, LocationStructure homeStructure)
	{
		character.tileObjectComponent.SetPrimaryBed(homeStructure.GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsUnowned<Bed>(character));
		if (character.tileObjectComponent.primaryBed != null)
		{
			character.tileObjectComponent.primaryBed.SetCharacterOwner(character);
		}
	}

	private Character GetFirstHostileIntruderOf(Character actor, LocationStructure p_structure)
	{
		return p_structure.GetFirstCharacterInsideStructureThatIsAliveHostileThatHasPathTo(actor);
	}

	private bool TryBuildBanditCamp(Character character, ref string log, ref JobQueueItem producedJob)
	{
		if (character.raceSetting.category != CHARACTER_CATEGORY.Villager && character.raceSetting.category != CHARACTER_CATEGORY.Humanoid && character.raceSetting.category != CHARACTER_CATEGORY.Demonic)
		{
			return false;
		}
		if (character.faction != null && character.faction.HasMemberWithJob(JOB_TYPE.BUILD_BANDIT_CAMP, character))
		{
			return false;
		}
		Area area = character.gridTileLocation?.GetNearestAreaForNecromancerSpawnLairOrBuildingBanditCamp(character);
		if (area != null)
		{
			LocationGridTile centerGridTile = area.gridTileComponent.centerGridTile;
			if (character.jobComponent.TriggerBuildBanditCamp(centerGridTile, out producedJob))
			{
				return true;
			}
		}
		return false;
	}

	private bool TryDoScheduledNeedsAtHomeBehaviour(Character character, DAILY_SCHEDULE currentScheduleType, ref string log, ref JobQueueItem producedJob)
	{
		switch (currentScheduleType)
		{
		case DAILY_SCHEDULE.Sleep:
			if (!character.needsComponent.doesNotGetTired)
			{
				return character.needsComponent.PlanTirednessRecoveryActionsForSleepBehaviour(out producedJob);
			}
			break;
		case DAILY_SCHEDULE.Free_Time:
			if (!character.needsComponent.HasNeeds())
			{
				break;
			}
			if (character.needsComponent.isStarving)
			{
				if (character.traitContainer.HasTrait("Vampire"))
				{
					if (character.needsComponent.PlanFullnessRecoveryActionsVampire(out producedJob))
					{
						return true;
					}
				}
				else if (character.needsComponent.PlanFullnessRecoveryActionsForFreeTime(out producedJob))
				{
					return true;
				}
			}
			else if (character.needsComponent.isHungry && GameUtilities.RollChance(20, ref log))
			{
				if (character.traitContainer.HasTrait("Vampire"))
				{
					if (character.needsComponent.PlanFullnessRecoveryActionsVampire(out producedJob))
					{
						return true;
					}
				}
				else if (character.needsComponent.PlanFullnessRecoveryActionsForFreeTime(out producedJob))
				{
					return true;
				}
			}
			if (character.needsComponent.isSulking)
			{
				if (CreateHappinessRecoveryJob(character, ref producedJob))
				{
					return true;
				}
			}
			else if (character.needsComponent.isBored && GameUtilities.RollChance(20, ref log) && CreateHappinessRecoveryJob(character, ref producedJob))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private bool CreateHappinessRecoveryJob(Character p_character, ref JobQueueItem producedJob)
	{
		if (!p_character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), p_character, p_character);
			JobUtilities.PopulatePriorityLocationsForHappinessRecovery(p_character, goapPlanJob);
			goapPlanJob.SetDoNotRecalculate(state: true);
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	private bool TryFindNearestFoodNearby(Character character, LocationStructure homeStructure, ref string log, ref JobQueueItem producedJob)
	{
		Area areaLocation = character.areaLocation;
		if (areaLocation != null)
		{
			LocationGridTile gridTileLocation = character.gridTileLocation;
			List<Area> list = RuinarchListPool<Area>.Claim();
			List<TileObject> list2 = RuinarchListPool<TileObject>.Claim();
			areaLocation.PopulateAreasInRange(list, 6, includeCenterTile: true);
			for (int i = 0; i < list.Count; i++)
			{
				Area area = list[i];
				TileObject randomTileObjectOfTypeForBanditForage = area.tileObjectComponent.GetRandomTileObjectOfTypeForBanditForage<FoodPile>();
				if (randomTileObjectOfTypeForBanditForage != null)
				{
					list2.Add(randomTileObjectOfTypeForBanditForage);
					continue;
				}
				randomTileObjectOfTypeForBanditForage = area.tileObjectComponent.GetRandomTileObjectOfTypeForBanditForage<BerryShrub>();
				if (randomTileObjectOfTypeForBanditForage != null)
				{
					list2.Add(randomTileObjectOfTypeForBanditForage);
				}
			}
			TileObject tileObject = null;
			float num = 0f;
			if (list2.Count > 0)
			{
				for (int j = 0; j < list2.Count; j++)
				{
					TileObject tileObject2 = list2[j];
					LocationGridTile gridTileLocation2 = tileObject2.gridTileLocation;
					if (gridTileLocation2 != null)
					{
						float distanceTo = gridTileLocation.GetDistanceTo(gridTileLocation2);
						if (tileObject == null || distanceTo < num)
						{
							tileObject = tileObject2;
							num = distanceTo;
						}
					}
				}
			}
			RuinarchListPool<Area>.Release(list);
			RuinarchListPool<TileObject>.Release(list2);
			if (tileObject != null && character.jobComponent.TryCreateForageFoodJob(tileObject, homeStructure, out producedJob))
			{
				return true;
			}
		}
		return false;
	}

	private bool TryFindNearestFoodInForageStructures(Character character, LocationStructure homeStructure, ref string log, ref JobQueueItem producedJob)
	{
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		List<TileObject> list2 = RuinarchListPool<TileObject>.Claim();
		GridMap.Instance.mainRegion.PopulateNearbyStructuresFrom(list, character, STRUCTURE_TYPE.MUSHROOM_HAVEN, STRUCTURE_TYPE.BERRY_GARDEN);
		for (int i = 0; i < list.Count; i++)
		{
			LocationStructure locationStructure = list[i];
			TileObject randomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombineAndHasTrait = locationStructure.GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombineAndHasTrait<FoodPile>("Edible");
			if (randomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombineAndHasTrait != null)
			{
				list2.Add(randomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombineAndHasTrait);
				continue;
			}
			randomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombineAndHasTrait = locationStructure.GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombineAndHasTrait<BerryShrub>("Edible");
			if (randomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombineAndHasTrait != null)
			{
				list2.Add(randomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombineAndHasTrait);
			}
		}
		TileObject tileObject = null;
		float num = 0f;
		if (list2.Count > 0)
		{
			LocationGridTile gridTileLocation = character.gridTileLocation;
			if (gridTileLocation != null)
			{
				for (int j = 0; j < list2.Count; j++)
				{
					TileObject tileObject2 = list2[j];
					LocationGridTile gridTileLocation2 = tileObject2.gridTileLocation;
					if (gridTileLocation2 != null)
					{
						float distanceTo = gridTileLocation.GetDistanceTo(gridTileLocation2);
						if (tileObject == null || distanceTo < num)
						{
							tileObject = tileObject2;
							num = distanceTo;
						}
					}
				}
			}
		}
		RuinarchListPool<LocationStructure>.Release(list);
		RuinarchListPool<TileObject>.Release(list2);
		if (tileObject != null && character.jobComponent.TryCreateForageFoodJob(tileObject, homeStructure, out producedJob))
		{
			return true;
		}
		return false;
	}

	private bool TryDoIdleBehaviour(Character character, ref string log, ref JobQueueItem producedJob)
	{
		if (!character.traitContainer.HasTrait("Lazy"))
		{
			CreateCleanJob(character, ref log, ref producedJob);
			if (producedJob != null)
			{
				return true;
			}
		}
		if (GameUtilities.RollChance(50, ref log))
		{
			TileObject unoccupiedBuiltTileObject = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
			if (unoccupiedBuiltTileObject != null)
			{
				character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, unoccupiedBuiltTileObject, out producedJob);
				if (producedJob != null)
				{
					return true;
				}
			}
		}
		if (character.limiterComponent.canDoTirednessRecovery && GameUtilities.RollChance(50, ref log))
		{
			TileObject primaryBed = character.tileObjectComponent.primaryBed;
			if (primaryBed != null && primaryBed.currentStructure == character.homeStructure)
			{
				character.PlanFixedJob(JOB_TYPE.IDLE_NAP, INTERACTION_TYPE.NAP, primaryBed, out producedJob);
				if (producedJob != null)
				{
					return true;
				}
			}
		}
		character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
		if (producedJob != null)
		{
			return true;
		}
		return false;
	}

	private void CreateCleanJob(Character character, ref string log, ref JobQueueItem producedJob)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		character.currentStructure.PopulateTileObjectsListWithAllTileObjects(list);
		for (int i = 0; i < list.Count; i++)
		{
			TileObject tileObject = list[i];
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT && (tileObject.traitContainer.HasTrait("Dirty") || tileObject.traitContainer.HasTrait("Wet") || tileObject.traitContainer.HasTrait("Burnt")) && tileObject.tileObjectType != TILE_OBJECT_TYPE.WATER_WELL && !tileObject.HasJobTargetingThis(JOB_TYPE.IDLE_CLEAN))
			{
				character.jobComponent.TryCreateCleanItemJob(tileObject, out producedJob);
				if (producedJob != null)
				{
					break;
				}
			}
		}
	}

	private bool TryDoScheduledWorkAtHomeForCraftersBehaviour(Character character, DAILY_SCHEDULE currentScheduleType, LocationStructure homeStructure, int numberOfAliveResidents, ref string log, ref JobQueueItem producedJob)
	{
		if (currentScheduleType == DAILY_SCHEDULE.Work)
		{
			TILE_OBJECT_TYPE tILE_OBJECT_TYPE = TILE_OBJECT_TYPE.BED;
			if (homeStructure.GetNumberOfTileObjects(tILE_OBJECT_TYPE) < numberOfAliveResidents)
			{
				int craftResourceCost = TileObjectDB.GetTileObjectData(tILE_OBJECT_TYPE).craftResourceCost;
				ResourcePile resourcePileObjectWithEnoughAmount = homeStructure.GetResourcePileObjectWithEnoughAmount<WoodPile>(craftResourceCost);
				if (resourcePileObjectWithEnoughAmount == null)
				{
					resourcePileObjectWithEnoughAmount = homeStructure.GetResourcePileObjectWithEnoughAmount<StonePile>(craftResourceCost);
				}
				if (resourcePileObjectWithEnoughAmount == null)
				{
					resourcePileObjectWithEnoughAmount = homeStructure.GetResourcePileObjectWithEnoughAmount<MetalPile>(craftResourceCost);
				}
				if (resourcePileObjectWithEnoughAmount != null)
				{
					character.jobComponent.CreateUnbuiltFurnitureThenCraftUsingOwnedResource(tILE_OBJECT_TYPE, homeStructure, resourcePileObjectWithEnoughAmount, out producedJob);
					if (producedJob != null)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool TryDoScheduledNormalWorkAtHomeBehaviour(Character character, DAILY_SCHEDULE currentScheduleType, LocationStructure homeStructure, ref string log, ref JobQueueItem producedJob)
	{
		if (currentScheduleType == DAILY_SCHEDULE.Work)
		{
			List<TileObject> list = RuinarchListPool<TileObject>.Claim();
			PopulateWoodPileListInsideStructureForCombining(list, homeStructure);
			if (list.Count > 1)
			{
				character.jobComponent.TryCreateCombineStockpile(list[1] as ResourcePile, list[0] as ResourcePile, out producedJob);
				if (producedJob != null)
				{
					RuinarchListPool<TileObject>.Release(list);
					return true;
				}
			}
			list.Clear();
			PopulateStonePileListInsideStructureForCombining(list, homeStructure);
			if (list.Count > 1)
			{
				character.jobComponent.TryCreateCombineStockpile(list[1] as ResourcePile, list[0] as ResourcePile, out producedJob);
				if (producedJob != null)
				{
					RuinarchListPool<TileObject>.Release(list);
					return true;
				}
			}
			RuinarchListPool<TileObject>.Release(list);
		}
		return false;
	}

	private void PopulateWoodPileListInsideStructureForCombining(List<TileObject> builtPilesInSideStructure, LocationStructure structure)
	{
		List<TileObject> tileObjectsOfType = structure.GetTileObjectsOfType(TILE_OBJECT_TYPE.WOOD_PILE);
		if (tileObjectsOfType == null)
		{
			return;
		}
		for (int i = 0; i < tileObjectsOfType.Count; i++)
		{
			TileObject tileObject = tileObjectsOfType[i];
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT && tileObject.characterOwner == null && !tileObject.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				builtPilesInSideStructure.Add(tileObject);
			}
		}
	}

	private void PopulateStonePileListInsideStructureForCombining(List<TileObject> builtPilesInSideStructure, LocationStructure structure)
	{
		List<TileObject> tileObjectsOfType = structure.GetTileObjectsOfType(TILE_OBJECT_TYPE.STONE_PILE);
		if (tileObjectsOfType == null)
		{
			return;
		}
		for (int i = 0; i < tileObjectsOfType.Count; i++)
		{
			TileObject tileObject = tileObjectsOfType[i];
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT && tileObject.characterOwner == null && !tileObject.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				builtPilesInSideStructure.Add(tileObject);
			}
		}
	}

	private bool VisitBanditCamps(Character character, ref string log, ref JobQueueItem producedJob)
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Visit_Village_Chance, ref log) && !character.traitContainer.HasTrait("Enslaved") && !character.behaviourComponent.HasBehaviour(typeof(VisitVillageBehaviour)))
		{
			NPCSettlement nPCSettlement = null;
			List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
			character.currentRegion.PopulateValidBanditCampsToVisit(character, list);
			if (list.Count > 0 && CollectionUtilities.GetRandomElement(list).settlementLocation is NPCSettlement nPCSettlement2)
			{
				nPCSettlement = nPCSettlement2;
			}
			RuinarchListPool<LocationStructure>.Release(list);
			if (nPCSettlement != null)
			{
				character.behaviourComponent.VisitVillage(character, nPCSettlement);
				producedJob = null;
				return true;
			}
		}
		return false;
	}
}
