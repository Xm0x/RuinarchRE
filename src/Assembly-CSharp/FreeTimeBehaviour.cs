using System.Collections.Generic;
using System.Linq;
using Characters.Villager_Wants;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UtilityScripts;

public class FreeTimeBehaviour : CharacterBehaviour
{
	private readonly string[] highTierClasses = new string[6] { "Knight", "Hunter", "Mage", "Barbarian", "Stalker", "Shaman" };

	private readonly TILE_OBJECT_TYPE[] dropItemExclusions = new TILE_OBJECT_TYPE[3]
	{
		TILE_OBJECT_TYPE.HEALING_POTION,
		TILE_OBJECT_TYPE.LUNCH_PACK,
		TILE_OBJECT_TYPE.CULTIST_KIT
	};

	public FreeTimeBehaviour()
	{
		base.priority = 9;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if ((character.moodComponent.moodState == MOOD_STATE.Bad || character.moodComponent.moodState == MOOD_STATE.Critical) && BadOrCriticalMoodBehaviour(character, ref log, out producedJob))
		{
			return true;
		}
		if (character.needsComponent.HasNeeds())
		{
			if (character.limiterComponent.canDoFullnessRecovery)
			{
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
			}
			if (character.limiterComponent.canDoHappinessRecovery)
			{
				if (character.needsComponent.isSulking)
				{
					if (CreateHappinessRecoveryJob(character, out producedJob))
					{
						return true;
					}
				}
				else if (character.needsComponent.isBored && GameUtilities.RollChance(20, ref log) && CreateHappinessRecoveryJob(character, out producedJob))
				{
					return true;
				}
			}
		}
		PartyLogic(character, ref log);
		if (TryMageGuildPersonalChangeClass(character, ref log, out producedJob))
		{
			return true;
		}
		if (WantedItemInInventoryBehaviour(character, ref log, out producedJob))
		{
			return true;
		}
		if (!character.trapStructure.IsTrapped() && ChanceData.RollChance(CHANCE_TYPE.Free_Time_Obtain_Want, ref log) && character.faction != null && character.faction.isMajorFaction && character.villagerWantsComponent != null)
		{
			LocationStructure p_chosenStructure;
			TileObject p_foundObject;
			VillagerWant topPriorityWant = character.villagerWantsComponent.GetTopPriorityWant(character, out p_chosenStructure, out p_foundObject);
			if (topPriorityWant is DwellingWant)
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Buy_Home, p_chosenStructure.tiles.First().tileObjectComponent.genericTileObject);
				producedJob = null;
				return true;
			}
			if (topPriorityWant is FoodWant)
			{
				if (p_foundObject != null)
				{
					if (character.jobComponent.CreateDropItemJob(JOB_TYPE.STOCKPILE_FOOD, p_foundObject, character.homeStructure, out producedJob))
					{
						return true;
					}
				}
				else if (character.jobComponent.TryCreateStockpileFood(character, p_chosenStructure, out producedJob))
				{
					return true;
				}
			}
			else if (topPriorityWant is FurnitureWant furnitureWant)
			{
				TILE_OBJECT_TYPE furnitureWanted = furnitureWant.GetFurnitureWanted(character);
				if (p_foundObject != null)
				{
					if (character.jobComponent.CreateUnbuiltFurnitureThenCraftUsingOwnedResource(furnitureWanted, character.homeStructure, p_foundObject, out producedJob))
					{
						return true;
					}
				}
				else if (character.jobComponent.CreateCraftFurniture(furnitureWanted, character.homeStructure, p_chosenStructure, out producedJob))
				{
					return true;
				}
			}
			else if (topPriorityWant is EquipmentWant equipmentWant)
			{
				Workshop workshop = p_chosenStructure as Workshop;
				CharacterClass characterClass = character.characterClass;
				List<TILE_OBJECT_TYPE> list = null;
				if (equipmentWant is WeaponWant)
				{
					list = characterClass.craftableWeapons;
				}
				else if (equipmentWant is ArmorWant)
				{
					list = characterClass.craftableArmors;
				}
				else if (equipmentWant is AccessoryWant)
				{
					list = characterClass.craftableAccessories;
				}
				if (list != null)
				{
					List<TileObject> list2 = RuinarchListPool<TileObject>.Claim();
					for (int i = 0; i < list.Count; i++)
					{
						TILE_OBJECT_TYPE type = list[i];
						List<TileObject> tileObjectsOfType = p_chosenStructure.GetTileObjectsOfType(type);
						if (tileObjectsOfType == null)
						{
							continue;
						}
						for (int j = 0; j < tileObjectsOfType.Count; j++)
						{
							TileObject tileObject = tileObjectsOfType[j];
							if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT)
							{
								list2.Add(tileObject);
							}
						}
					}
					if (list2.Count > 0)
					{
						TileObject randomElement = CollectionUtilities.GetRandomElement(list2);
						if (character.jobComponent.TryCreateBuyItem(character, randomElement, out producedJob))
						{
							return true;
						}
					}
					else if (!workshop.IsCharacterAlreadyHasRequest(character))
					{
						WorkShopRequestForm workShopRequestForm = new WorkShopRequestForm();
						workShopRequestForm.requestingCharacter = character;
						if (equipmentWant is WeaponWant)
						{
							workShopRequestForm.equipmentType = EQUIPMENT_TYPE.WEAPON;
						}
						else if (equipmentWant is ArmorWant)
						{
							workShopRequestForm.equipmentType = EQUIPMENT_TYPE.ARMOR;
						}
						else if (equipmentWant is AccessoryWant)
						{
							workShopRequestForm.equipmentType = EQUIPMENT_TYPE.ACCESSORY;
						}
						workshop.PostRequest(workShopRequestForm);
					}
				}
			}
			else if (topPriorityWant is HealingPotionWant)
			{
				TileObject tileObject2 = null;
				List<TileObject> list3 = RuinarchListPool<TileObject>.Claim();
				p_chosenStructure.PopulateTileObjectsOfType(list3, TILE_OBJECT_TYPE.HEALING_POTION);
				List<TileObject> list4 = RuinarchListPool<TileObject>.Claim();
				for (int k = 0; k < list3.Count; k++)
				{
					TileObject tileObject3 = list3[k];
					if (tileObject3.gridTileLocation != null && !tileObject3.HasJobTargetingThis(JOB_TYPE.BUY_ITEM))
					{
						list4.Add(tileObject3);
					}
				}
				if (list4.Count > 0)
				{
					tileObject2 = CollectionUtilities.GetRandomElement(list4);
				}
				RuinarchListPool<TileObject>.Release(list3);
				RuinarchListPool<TileObject>.Release(list4);
				if (tileObject2 != null && character.jobComponent.TryCreateBuyItem(character, tileObject2, out producedJob))
				{
					return true;
				}
			}
		}
		CharacterClassBehaviour classBehaviour = CharacterManager.Instance.GetClassBehaviour(character.characterClass.className);
		if (classBehaviour != null && classBehaviour.TryDoBehaviour(character, ref producedJob, ref log))
		{
			_ = producedJob;
			return true;
		}
		if (!HasHomeStructureOrTerritory(character))
		{
			return HomelessBehaviour(character, ref log, out producedJob);
		}
		if (character.isAtHomeStructure || character.IsInTerritory())
		{
			return AtHomeBehaviour(character, ref log, out producedJob);
		}
		if (character.currentStructure is Tavern)
		{
			return TavernBehaviour(character, ref log, out producedJob);
		}
		if (character.currentStructure is Hospice)
		{
			return HospiceBehaviour(character, ref log, out producedJob);
		}
		return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
	}

	private bool AtHomeBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.previousCharacterDataComponent.IsPreviousJobOrActionReturnHome())
		{
			if (TryCombatantPersonalChangeClass(character, ref log, out producedJob))
			{
				return true;
			}
			TileObject unoccupiedBuiltTileObject = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
			if (unoccupiedBuiltTileObject != null)
			{
				character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, unoccupiedBuiltTileObject, out producedJob);
			}
			else
			{
				character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
			}
			return true;
		}
		if (PartyMemberPreparationBehaviour(character, ref log, out producedJob))
		{
			return true;
		}
		if (character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick) == DAILY_SCHEDULE.Free_Time && character.dailyScheduleComponent.schedule.IsInFirstHourOfCurrentScheduleType(GameManager.Instance.currentTick) && !character.traitContainer.HasTrait("Agoraphobic") && !character.crimeComponent.IsWantedBy(character.faction))
		{
			if (ChanceData.RollChance(CHANCE_TYPE.Socialize_Chance, ref log) && !character.behaviourComponent.HasBehaviour(typeof(SocializingBehaviour)) && character.homeSettlement != null && character.homeSettlement.locationType == LOCATION_TYPE.VILLAGE)
			{
				LocationStructure p_targetStructure = ((!character.homeSettlement.HasStructure(STRUCTURE_TYPE.TAVERN)) ? character.homeSettlement.cityCenter : (GameUtilities.RollChance(50) ? character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN) : character.homeSettlement.cityCenter));
				character.behaviourComponent.GoSocializing(character, p_targetStructure);
				producedJob = null;
				return true;
			}
			if (ChanceData.RollChance(CHANCE_TYPE.Visit_Village_Chance, ref log) && !character.traitContainer.HasTrait("Enslaved") && !character.behaviourComponent.HasBehaviour(typeof(VisitVillageBehaviour)))
			{
				List<NPCSettlement> list = RuinarchListPool<NPCSettlement>.Claim();
				character.currentRegion.PopulateValidVillagesToVisit(character, list, character.faction);
				if (character.homeSettlement != null)
				{
					list.Remove(character.homeSettlement);
				}
				if (list.Count > 0)
				{
					NPCSettlement randomElement = CollectionUtilities.GetRandomElement(list);
					character.behaviourComponent.VisitVillage(character, randomElement);
					RuinarchListPool<NPCSettlement>.Release(list);
					producedJob = null;
					return true;
				}
				RuinarchListPool<NPCSettlement>.Release(list);
			}
		}
		if (character.homeStructure != null && character.homeStructure.settlementLocation is NPCSettlement nPCSettlement && character.homeStructure is ManMadeStructure { structureWalls: not null } manMadeStructure && manMadeStructure.structureObj != null && manMadeStructure.structureWalls.Count > 0 && manMadeStructure.ShouldBeRepaired() && ChanceData.RollChance(CHANCE_TYPE.Idle_Repair_Home))
		{
			JobQueueItem p_job = nPCSettlement.GetJob(JOB_TYPE.REPAIR, manMadeStructure.structureTileObject);
			if (p_job == null)
			{
				manMadeStructure.TryCreateSettlementRepairJob(out p_job);
			}
			if (p_job != null && p_job.assignedCharacter == null)
			{
				producedJob = p_job;
				return true;
			}
		}
		if (GameUtilities.RollChance(4, ref log))
		{
			TileObject unoccupiedTileObject = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
			if (unoccupiedTileObject != null && !character.traitContainer.HasTrait("Vampire") && character.limiterComponent.canDoTirednessRecovery)
			{
				character.PlanFixedJob(JOB_TYPE.IDLE_NAP, INTERACTION_TYPE.NAP, unoccupiedTileObject, out producedJob);
				return true;
			}
		}
		if (character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick) == DAILY_SCHEDULE.Free_Time && character.traitContainer.HasTrait("Aroused") && !character.traitContainer.HasTrait("Griefstricken") && !character.traitContainer.HasTrait("Heartbroken"))
		{
			Character latestValidArousedTarget = character.traitContainer.GetTraitOrStatus<Aroused>("Aroused").latestValidArousedTarget;
			if (latestValidArousedTarget != null)
			{
				if (character.relationshipContainer.IsLoverOrAffair(latestValidArousedTarget))
				{
					if (GameUtilities.RollChance(character.traitContainer.HasTrait("Drunk") ? 25 : 15, ref log) && character.jobComponent.TriggerMakeLoveJob(JOB_TYPE.IDLE, latestValidArousedTarget, out producedJob))
					{
						return true;
					}
				}
				else if (character.relationshipContainer.HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE.LOVER) || character.relationshipContainer.HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE.AFFAIR))
				{
					if (character.traitContainer.HasTrait("Unfaithful"))
					{
						if (GameUtilities.RollChance(character.traitContainer.HasTrait("Drunk") ? 25 : 15, ref log) && character.jobComponent.TriggerSeduce(latestValidArousedTarget, out producedJob))
						{
							return true;
						}
					}
					else if (GameUtilities.RollChance((!character.traitContainer.HasTrait("Drunk")) ? 1 : 5, ref log) && character.jobComponent.TriggerSeduce(latestValidArousedTarget, out producedJob))
					{
						return true;
					}
				}
				else if (GameUtilities.RollChance(character.traitContainer.HasTrait("Drunk") ? 25 : 15, ref log) && character.jobComponent.TriggerSeduce(latestValidArousedTarget, out producedJob))
				{
					return true;
				}
			}
		}
		if (TryMoveInWithSpouse(character, ref log, out producedJob))
		{
			return true;
		}
		if (character.traitContainer.HasTrait("Suicidal") && GameUtilities.RollChance(20, ref log))
		{
			if (GameUtilities.RollChance(65, ref log))
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, character, "", null, "Cry_Suicidal");
				producedJob = null;
				return true;
			}
			if (character.jobComponent.TriggerSuicideJob(out producedJob, "Suicide_Reason_Mental_Break"))
			{
				return true;
			}
		}
		if (character.traitContainer.HasTrait("Despairing") && GameUtilities.RollChance(15, ref log))
		{
			if (GameUtilities.RollChance(90, ref log))
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, character, "", null, "Cry_Despair");
				producedJob = null;
				return true;
			}
			if (character.jobComponent.TriggerSuicideJob(out producedJob, "Suicide_Reason_Despair"))
			{
				return true;
			}
		}
		if (GameUtilities.RollChance(30, ref log) && !character.trapStructure.IsTrapped() && !character.trapStructure.IsTrappedInArea())
		{
			Character disabledCharacterToVisit = GetDisabledCharacterToVisit(character);
			if (disabledCharacterToVisit != null && disabledCharacterToVisit.homeStructure != null)
			{
				character.PlanFixedJob(JOB_TYPE.CHECK_PARALYZED_FRIEND, INTERACTION_TYPE.VISIT, disabledCharacterToVisit, out producedJob, new OtherData[2]
				{
					new LocationStructureOtherData(disabledCharacterToVisit.homeStructure),
					new CharacterOtherData(disabledCharacterToVisit)
				});
				return true;
			}
		}
		if (character.currentSettlement != null && character.currentSettlement.HasHospiceClaimedByNonEnemyOrSelfAndNotBanned(character, out var foundStructure))
		{
			Hospice hospice = foundStructure as Hospice;
			if (character.traitContainer.HasTrait("Injured", "Burnt", "Poisoned", "Plagued") && !character.traitContainer.HasTrait("Plague Reservoir") && ChanceData.RollChance(CHANCE_TYPE.Plauged_Injured_Visit_Hospice))
			{
				BedClinic firstBedToRecuperate = hospice.GetFirstBedToRecuperate();
				if (firstBedToRecuperate != null && character.jobComponent.TryRecuperate(firstBedToRecuperate, out producedJob))
				{
					return true;
				}
			}
			if (ChanceData.RollChance(CHANCE_TYPE.Vampire_Lycan_Visit_Hospice, ref log) && character.currentStructure != foundStructure && hospice.HasWorkerWithLevel5HealingMagic())
			{
				_ = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.dislikedBeingVampire;
				if (character.lycanData != null)
				{
					_ = character.lycanData.dislikesBeingLycan;
				}
			}
		}
		if (character.HasItemOtherThan(dropItemExclusions) && character.homeStructure != null && GameUtilities.RollChance(10) && character.jobComponent.CreateDropItemJob(JOB_TYPE.DROP_ITEM, character.GetRandomItemThatIsNotOfType(dropItemExclusions), character.homeStructure, out producedJob))
		{
			return true;
		}
		if (character.traitContainer.HasTrait("Devout") && GameUtilities.RollChance(2, ref log))
		{
			character.jobComponent.TriggerPray(out producedJob, JOB_TYPE.IDLE_PRAY);
			return true;
		}
		if (character.traitContainer.HasTrait("Lazy") && GameUtilities.RollChance(4))
		{
			CreateCleanJob(character, ref log, out producedJob);
		}
		else
		{
			CreateCleanJob(character, ref log, out producedJob);
		}
		if (producedJob != null)
		{
			return true;
		}
		if (character.homeSettlement != null && character.homeSettlement.locationType == LOCATION_TYPE.VILLAGE && !character.traitContainer.HasTrait("Agoraphobia") && !character.crimeComponent.IsWantedBy(character.faction) && ChanceData.RollChance(CHANCE_TYPE.Socialize_Friend, ref log))
		{
			if (ChanceData.RollChance(CHANCE_TYPE.Socialize_Friend_Visit, ref log))
			{
				List<Character> list2 = RuinarchListPool<Character>.Claim();
				for (int i = 0; i < character.relationshipContainer.charactersWithOpinion.Count; i++)
				{
					Character character2 = character.relationshipContainer.charactersWithOpinion[i];
					if (character.relationshipContainer.IsFriendsWith(character2) && character.relationshipContainer.GetAwarenessState(character, character2) == AWARENESS_STATE.Available && character2.homeStructure != null && character2.homeSettlement == character.homeSettlement && character2.gridTileLocation != null && character2.gridTileLocation.structure == character2.homeStructure)
					{
						list2.Add(character2);
					}
				}
				if (list2.Count > 0)
				{
					Character randomElement2 = CollectionUtilities.GetRandomElement(list2);
					character.behaviourComponent.GoSocializing(character, randomElement2.homeStructure);
					RuinarchListPool<Character>.Release(list2);
					return true;
				}
				RuinarchListPool<Character>.Release(list2);
			}
			character.behaviourComponent.GoSocializing(character, character.homeSettlement.cityCenter);
			return true;
		}
		TileObject unoccupiedBuiltTileObject2 = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
		if (unoccupiedBuiltTileObject2 != null)
		{
			character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, unoccupiedBuiltTileObject2, out producedJob);
			return true;
		}
		character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
		return true;
	}

	private bool HomelessBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.homeSettlement != null && character.homeSettlement.locationType == LOCATION_TYPE.VILLAGE)
		{
			if (character.currentStructure != null && character.currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && character.currentStructure.structureType != STRUCTURE_TYPE.TAVERN)
			{
				if (GameUtilities.RollChance(25, ref log))
				{
					if (character.homeSettlement.HasStructure(STRUCTURE_TYPE.TAVERN) && !character.traitContainer.HasTrait("Agoraphobic"))
					{
						LocationStructure locationStructure = (GameUtilities.RollChance(50) ? character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN) : character.homeSettlement.cityCenter);
						if (locationStructure.passableTiles.Count > 0)
						{
							LocationGridTile randomElement = CollectionUtilities.GetRandomElement(locationStructure.passableTiles);
							return character.jobComponent.CreateGoToJob(JOB_TYPE.IDLE, randomElement, out producedJob);
						}
					}
					else
					{
						LocationStructure cityCenter = character.homeSettlement.cityCenter;
						if (cityCenter.passableTiles.Count > 0)
						{
							LocationGridTile randomElement2 = CollectionUtilities.GetRandomElement(cityCenter.passableTiles);
							return character.jobComponent.CreateGoToJob(JOB_TYPE.IDLE, randomElement2, out producedJob);
						}
					}
				}
				else
				{
					character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
					if (producedJob != null)
					{
						return true;
					}
				}
			}
			else
			{
				if (character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick) == DAILY_SCHEDULE.Free_Time && character.dailyScheduleComponent.schedule.IsInFirstHourOfCurrentScheduleType(GameManager.Instance.currentTick) && !character.traitContainer.HasTrait("Agoraphobic") && !character.crimeComponent.IsWantedBy(character.faction))
				{
					if (ChanceData.RollChance(CHANCE_TYPE.Socialize_Chance, ref log) && !character.behaviourComponent.HasBehaviour(typeof(SocializingBehaviour)) && character.homeSettlement != null && character.homeSettlement.locationType == LOCATION_TYPE.VILLAGE)
					{
						LocationStructure p_targetStructure = ((!character.homeSettlement.HasStructure(STRUCTURE_TYPE.TAVERN)) ? character.homeSettlement.cityCenter : (GameUtilities.RollChance(50) ? character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN) : character.homeSettlement.cityCenter));
						character.behaviourComponent.GoSocializing(character, p_targetStructure);
						producedJob = null;
						return true;
					}
					if (ChanceData.RollChance(CHANCE_TYPE.Visit_Village_Chance, ref log) && !character.traitContainer.HasTrait("Enslaved") && !character.behaviourComponent.HasBehaviour(typeof(VisitVillageBehaviour)))
					{
						List<NPCSettlement> list = RuinarchListPool<NPCSettlement>.Claim();
						character.currentRegion.PopulateValidVillagesToVisit(character, list, character.faction);
						if (list.Count > 0)
						{
							NPCSettlement randomElement3 = CollectionUtilities.GetRandomElement(list);
							character.behaviourComponent.VisitVillage(character, randomElement3);
							RuinarchListPool<NPCSettlement>.Release(list);
							producedJob = null;
							return true;
						}
						RuinarchListPool<NPCSettlement>.Release(list);
					}
				}
				if (TryCombatantPersonalChangeClass(character, ref log, out producedJob))
				{
					return true;
				}
				if (TryMoveInWithSpouse(character, ref log, out producedJob))
				{
					return true;
				}
				if (character.traitContainer.HasTrait("Suicidal") && GameUtilities.RollChance(20, ref log))
				{
					if (GameUtilities.RollChance(65, ref log))
					{
						character.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, character, "", null, "Cry_Suicidal");
						producedJob = null;
						return true;
					}
					if (character.jobComponent.TriggerSuicideJob(out producedJob, "Suicide_Reason_Mental_Break"))
					{
						return true;
					}
				}
				if (character.traitContainer.HasTrait("Despairing") && GameUtilities.RollChance(15, ref log))
				{
					if (GameUtilities.RollChance(90, ref log))
					{
						character.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, character, "", null, "Cry_Despair");
						producedJob = null;
						return true;
					}
					if (character.jobComponent.TriggerSuicideJob(out producedJob, "Suicide_Reason_Despair"))
					{
						return true;
					}
				}
				if (GameUtilities.RollChance(30, ref log) && !character.trapStructure.IsTrapped() && !character.trapStructure.IsTrappedInArea())
				{
					Character disabledCharacterToVisit = GetDisabledCharacterToVisit(character);
					if (disabledCharacterToVisit != null && disabledCharacterToVisit.homeStructure != null)
					{
						character.PlanFixedJob(JOB_TYPE.CHECK_PARALYZED_FRIEND, INTERACTION_TYPE.VISIT, disabledCharacterToVisit, out producedJob, new OtherData[2]
						{
							new LocationStructureOtherData(disabledCharacterToVisit.homeStructure),
							new CharacterOtherData(disabledCharacterToVisit)
						});
						return true;
					}
				}
				if (character.currentSettlement != null && character.currentSettlement.HasHospiceClaimedByNonEnemyOrSelfAndNotBanned(character, out var foundStructure))
				{
					Hospice hospice = foundStructure as Hospice;
					if (character.traitContainer.HasTrait("Injured", "Burnt", "Poisoned", "Plagued") && !character.traitContainer.HasTrait("Plague Reservoir") && ChanceData.RollChance(CHANCE_TYPE.Plauged_Injured_Visit_Hospice))
					{
						BedClinic firstBedToRecuperate = hospice.GetFirstBedToRecuperate();
						if (firstBedToRecuperate != null && character.jobComponent.TryRecuperate(firstBedToRecuperate, out producedJob))
						{
							return true;
						}
					}
					if (ChanceData.RollChance(CHANCE_TYPE.Vampire_Lycan_Visit_Hospice, ref log) && character.currentStructure != foundStructure && hospice.HasWorkerWithLevel5HealingMagic())
					{
						_ = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.dislikedBeingVampire;
						if (character.lycanData != null)
						{
							_ = character.lycanData.dislikesBeingLycan;
						}
					}
				}
				if (character.traitContainer.HasTrait("Devout") && GameUtilities.RollChance(2, ref log))
				{
					character.jobComponent.TriggerPray(out producedJob, JOB_TYPE.IDLE_PRAY);
					return true;
				}
			}
		}
		character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
		return true;
	}

	private bool TavernBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		GameManager.Instance.GetCurrentTimeInWordsOfTick(character).ToStringEnum();
		_ = 3;
		return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
	}

	private bool HospiceBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.traitContainer.HasTrait("Injured", "Burnt", "Poisoned", "Plagued") && !character.traitContainer.HasTrait("Plague Reservoir"))
		{
			Hospice hospice = character.currentStructure as Hospice;
			if (!hospice.IsBanned(character))
			{
				BedClinic firstBedToRecuperate = hospice.GetFirstBedToRecuperate();
				if (firstBedToRecuperate != null && character.jobComponent.TryRecuperate(firstBedToRecuperate, out producedJob))
				{
					return true;
				}
			}
		}
		if (!character.trapStructure.IsTrapped() && !character.trapStructure.IsTrappedInArea())
		{
			return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
		}
		return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
	}

	private bool WantedItemInInventoryBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.homeStructure != null)
		{
			for (int i = 0; i < character.items.Count; i++)
			{
				TileObject tileObject = character.items[i];
				if (character.villagerWantsComponent.CanItemSatisfyWant(tileObject) && character.jobComponent.CreateDropItemJob(JOB_TYPE.DROP_ITEM, tileObject, character.homeStructure, out producedJob))
				{
					return true;
				}
			}
		}
		producedJob = null;
		return false;
	}

	private bool BadOrCriticalMoodBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.traitContainer.HasTrait("Pyromaniac") && ChanceData.RollChance(CHANCE_TYPE.Pyromaniac_Arson_Bad_Mood))
		{
			List<TileObject> list = RuinarchListPool<TileObject>.Claim();
			if (character.currentStructure != null && (character.currentStructure.structureType.IsVillageStructure() || character.currentStructure.structureType.IsInterior()))
			{
				character.currentStructure.PopulateTileObjectsWithTraitThatActorCanReach(list, "Flammable", character);
			}
			if (list.Count <= 0 && character.currentSettlement != null)
			{
				character.currentSettlement.PopulateTileObjectsWithTraitThatActorCanReach("Flammable", list, character);
			}
			if (list.Count <= 0)
			{
				character.areaLocation.tileObjectComponent.PopulateTileObjectsWithTraitThatActorCanReach("Flammable", list, character);
			}
			TileObject randomElement = CollectionUtilities.GetRandomElement(list);
			RuinarchListPool<TileObject>.Release(list);
			if (randomElement != null && character.jobComponent.TriggerArson(randomElement, out producedJob))
			{
				return true;
			}
		}
		producedJob = null;
		return false;
	}

	private bool TryMoveInWithSpouse(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Spouse_Move_In))
		{
			Character firstCharacterWithRelationship = character.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
			if (firstCharacterWithRelationship != null && !firstCharacterWithRelationship.isDead && firstCharacterWithRelationship.homeSettlement == character.homeSettlement && firstCharacterWithRelationship.homeStructure != null && firstCharacterWithRelationship.homeStructure != character.homeStructure && !firstCharacterWithRelationship.homeStructure.HasReachedMaxResidentCapacity() && firstCharacterWithRelationship.homeStructure.CanBeResidentHere(character) && firstCharacterWithRelationship.homeStructure != character.previousCharacterDataComponent.previousHomeStructure)
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, firstCharacterWithRelationship);
				producedJob = null;
				return true;
			}
		}
		producedJob = null;
		return false;
	}

	private bool TryMageGuildPersonalChangeClass(Character character, ref string log, out JobQueueItem producedJob)
	{
		if ((character.characterClass.IsCombatant() || character.characterClass.className == "Noble") && !character.traitContainer.HasTrait("Enslaved") && GameUtilities.RollChance(50) && character.faction != null && character.faction.ideologyComponent.HasIdeology(FACTION_IDEOLOGY.Mage_Guild) && character.characterClass.className != "Mage" && character.jobComponent.TriggerPersonalChangeClassJob("Mage", out producedJob))
		{
			return true;
		}
		producedJob = null;
		return false;
	}

	private bool TryCombatantPersonalChangeClass(Character character, ref string log, out JobQueueItem producedJob)
	{
		if ((character.characterClass.IsCombatant() || character.characterClass.className == "Noble") && !character.traitContainer.HasTrait("Enslaved"))
		{
			bool flag = false;
			if ((character.faction == null || !character.faction.ideologyComponent.HasIdeology(FACTION_IDEOLOGY.Mage_Guild) || !(character.characterClass.className != "Mage")) && ChanceData.RollChance(CHANCE_TYPE.Personal_Combatant_Change_Class, ref log) && CanCharacterChangeToAHigherTierCombatClass(character))
			{
				List<string> list = RuinarchListPool<string>.Claim();
				if (character.characterClass.attackType == ATTACK_TYPE.MAGICAL && character.structureComponent.HasWorkPlaceStructure())
				{
					TryAddClassToChangeClassChoices(character, "Mage", list);
					TryAddClassToChangeClassChoices(character, "Shaman", list);
				}
				else
				{
					TryAddClassToChangeClassChoices(character, "Knight", list);
					TryAddClassToChangeClassChoices(character, "Hunter", list);
					TryAddClassToChangeClassChoices(character, "Mage", list);
					if (list.Count <= 0)
					{
						TryAddClassToChangeClassChoices(character, "Barbarian", list);
						TryAddClassToChangeClassChoices(character, "Stalker", list);
						TryAddClassToChangeClassChoices(character, "Shaman", list);
					}
				}
				if (list.Count > 0)
				{
					string randomElement = CollectionUtilities.GetRandomElement(list);
					if (character.jobComponent.TriggerPersonalChangeClassJob(randomElement, out producedJob))
					{
						return true;
					}
				}
			}
		}
		producedJob = null;
		return false;
	}

	private bool HasHomeStructureOrTerritory(Character character)
	{
		if (character.homeStructure == null || character.homeStructure.hasBeenDestroyed)
		{
			return character.HasTerritory();
		}
		return true;
	}

	private Character GetDisabledCharacterToVisit(Character p_character)
	{
		Character result = null;
		List<Character> charactersWithOpinion = p_character.relationshipContainer.charactersWithOpinion;
		if (charactersWithOpinion.Count > 0)
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			for (int i = 0; i < charactersWithOpinion.Count; i++)
			{
				Character character = charactersWithOpinion[i];
				if (p_character.homeSettlement == character.homeSettlement && p_character.homeStructure != character.homeStructure && character.traitContainer.HasTrait("Paralyzed", "Catatonic") && !character.isDead && p_character.relationshipContainer.GetAwarenessState(p_character, character) != AWARENESS_STATE.Missing && p_character.homeStructure != character.homeStructure && p_character.relationshipContainer.IsFriendsWith(character))
				{
					list.Add(character);
				}
			}
			if (list.Count > 0)
			{
				result = CollectionUtilities.GetRandomElement(list);
			}
			RuinarchListPool<Character>.Release(list);
		}
		return result;
	}

	private bool CreateHappinessRecoveryJob(Character p_character, out JobQueueItem producedJob)
	{
		return p_character.needsComponent.PlanHappinessRecoveryForFreeTime(out producedJob);
	}

	private void CreateCleanJob(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		character.currentStructure.PopulateTileObjectsListWithAllTileObjects(list);
		for (int i = 0; i < list.Count; i++)
		{
			TileObject tileObject = list[i];
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT && tileObject.traitContainer.HasTrait("Dirty", "Wet", "Burnt") && tileObject.tileObjectType != TILE_OBJECT_TYPE.WATER_WELL && !tileObject.HasJobTargetingThis(JOB_TYPE.IDLE_CLEAN))
			{
				character.jobComponent.TryCreateCleanItemJob(tileObject, out producedJob);
				if (producedJob != null)
				{
					break;
				}
			}
		}
	}

	private bool CanCharacterChangeToAHigherTierCombatClass(Character p_character)
	{
		if (!p_character.classComponent.canChangeClass)
		{
			return false;
		}
		if (p_character.faction != null && p_character.faction.ideologyComponent.HasIdeology(FACTION_IDEOLOGY.Mage_Guild))
		{
			if (p_character.characterClass.className != "Mage")
			{
				return true;
			}
			return false;
		}
		return !highTierClasses.Contains(p_character.characterClass.className);
	}

	private void TryAddClassToChangeClassChoices(Character p_character, string p_className, List<string> p_classChoices)
	{
		if (p_character.classComponent.ableClasses.Contains(p_className) && (!(p_className == "Stalker") || !p_character.traitContainer.HasTrait("Demon Cultist")))
		{
			p_classChoices.Add(p_className);
		}
	}
}
