using System.Collections.Generic;
using Characters.Villager_Wants;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class VisitVillageBehaviour : CharacterBehaviour
{
	public VisitVillageBehaviour()
	{
		base.priority = 800;
		attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[1] { BEHAVIOUR_COMPONENT_ATTRIBUTE.STOPS_BEHAVIOUR_LOOP };
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		NPCSettlement targetVisitVillage = character.behaviourComponent.targetVisitVillage;
		if ((character.behaviourComponent.visitVillageEndTime.hasValue && character.behaviourComponent.visitVillageEndTime.IsBefore(GameManager.Instance.Today())) || (targetVisitVillage.owner != null && character.crimeComponent.IsWantedBy(targetVisitVillage.owner)))
		{
			character.behaviourComponent.ClearOutVisitVillageBehaviour();
			producedJob = null;
			return false;
		}
		if (character.currentSettlement == targetVisitVillage)
		{
			LocationStructure p_chosenStructure = null;
			TileObject p_foundObject = null;
			if (character.behaviourComponent.visitVillageIntent == VISIT_VILLAGE_INTENT.Socialize)
			{
				LocationStructure targetVisitVillageStructure = character.behaviourComponent.targetVisitVillageStructure;
				if (targetVisitVillageStructure == null)
				{
					character.behaviourComponent.OnCharacterArrivedAtTargetVillageSettlement(character, targetVisitVillage);
					producedJob = null;
					return false;
				}
				if (targetVisitVillageStructure.hasBeenDestroyed)
				{
					character.behaviourComponent.ClearOutVisitVillageBehaviour();
					producedJob = null;
					return false;
				}
				if (targetVisitVillageStructure.structureType == STRUCTURE_TYPE.TAVERN)
				{
					if (GameUtilities.RollChance(15, ref log))
					{
						List<TileObject> tileObjectsOfType = targetVisitVillageStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.TABLE);
						if (tileObjectsOfType != null)
						{
							for (int i = 0; i < tileObjectsOfType.Count; i++)
							{
								Table table = tileObjectsOfType[i] as Table;
								if (table.mapObjectState == MAP_OBJECT_STATE.BUILT && table.CanAccommodateCharacter(character) && character.jobComponent.TriggerDrinkJob(JOB_TYPE.SOCIALIZE, table, out producedJob))
								{
									return true;
								}
							}
						}
					}
				}
				else if (targetVisitVillageStructure.structureType == STRUCTURE_TYPE.CITY_CENTER && GameUtilities.RollChance(15, ref log))
				{
					List<TileObject> tileObjectsOfType2 = targetVisitVillageStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.WATER_WELL);
					if (tileObjectsOfType2 != null)
					{
						for (int j = 0; j < tileObjectsOfType2.Count; j++)
						{
							WaterWell waterWell = tileObjectsOfType2[j] as WaterWell;
							if (waterWell.mapObjectState == MAP_OBJECT_STATE.BUILT)
							{
								return character.jobComponent.TriggerDrinkWaterJob(waterWell, out producedJob);
							}
						}
					}
				}
				if (GameUtilities.RollChance(20, ref log) && ChatBehaviour(character, ref log, out producedJob))
				{
					return true;
				}
				TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
				if (ChanceData.RollChance(CHANCE_TYPE.Change_Intent, ref log))
				{
					bool flag = false;
					if (character.villagerWantsComponent.GetTopPriorityWant(character, out p_chosenStructure, out p_foundObject) is FoodWant && !character.HasItem<ResourcePile>() && ChanceData.RollChance(CHANCE_TYPE.Change_Intent_Purchase, ref log))
					{
						character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Purchase);
						flag = true;
					}
					if (!flag && character.characterClass.className == "Noble" && character.currentSettlement is NPCSettlement { locationType: LOCATION_TYPE.VILLAGE, owner: not null } nPCSettlement && nPCSettlement.owner != character.faction && nPCSettlement.ruler?.currentSettlement == nPCSettlement && !nPCSettlement.owner.IsFriendlyWith(character.faction) && ChanceData.RollChance(CHANCE_TYPE.Change_Intent_Noble, ref log))
					{
						character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Enhance_Relationship);
						flag = true;
					}
					if (!flag && character.traitContainer.HasTrait("Kleptomaniac") && ChanceData.RollChance(CHANCE_TYPE.Change_Intent_Kleptomania, ref log))
					{
						character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Steal);
						LocationStructure randomStructure = targetVisitVillage.GetRandomStructure();
						character.behaviourComponent.SetTargetVisitVillageStructure(randomStructure);
						flag = true;
					}
					if (!flag && character.traitContainer.HasTrait("Vampire") && (currentTimeInWordsOfTick == TIME_IN_WORDS.EARLY_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT) && ChanceData.RollChance(CHANCE_TYPE.Change_Intent_Vampire, ref log))
					{
						character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Drink_Blood);
						flag = true;
					}
					if (!flag && character.traitContainer.HasTrait("Demon Cultist") && ChanceData.RollChance(CHANCE_TYPE.Change_Intent_Cultist, ref log))
					{
						character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Preach);
						flag = true;
					}
					if (!flag && character.traitContainer.HasTrait("Pyromaniac") && ChanceData.RollChance(CHANCE_TYPE.Change_Intent_Pyromaniac, ref log))
					{
						character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Arson);
					}
				}
				else
				{
					if (character.currentStructure != targetVisitVillageStructure)
					{
						LocationGridTile tile = ((targetVisitVillageStructure.passableTiles.Count > 0) ? CollectionUtilities.GetRandomElement(targetVisitVillageStructure.passableTiles) : CollectionUtilities.GetRandomElement(targetVisitVillageStructure.tiles));
						return character.jobComponent.CreateGoToJob(JOB_TYPE.VISIT_DIFFERENT_VILLAGE, tile, out producedJob);
					}
					if (character.jobComponent.TriggerRoamAroundStructure(JOB_TYPE.VISIT_DIFFERENT_VILLAGE, out producedJob))
					{
						return true;
					}
				}
			}
			if (character.behaviourComponent.visitVillageIntent == VISIT_VILLAGE_INTENT.Steal)
			{
				if (character.behaviourComponent.targetVisitVillageStructure == null)
				{
					character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Socialize);
					producedJob = null;
					return true;
				}
				if (character.currentStructure != character.behaviourComponent.targetVisitVillageStructure)
				{
					LocationGridTile tile2 = ((character.behaviourComponent.targetVisitVillageStructure.passableTiles.Count > 0) ? CollectionUtilities.GetRandomElement(character.behaviourComponent.targetVisitVillageStructure.passableTiles) : CollectionUtilities.GetRandomElement(character.behaviourComponent.targetVisitVillageStructure.tiles));
					return character.jobComponent.CreateGoToJob(JOB_TYPE.VISIT_STRUCTURE, tile2, out producedJob);
				}
				character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Socialize);
				if (character.jobComponent.TriggerRobLocation(character.currentStructure, INTERACTION_TYPE.STEAL_ANYTHING, out producedJob))
				{
					return true;
				}
			}
			else if (character.behaviourComponent.visitVillageIntent == VISIT_VILLAGE_INTENT.Drink_Blood)
			{
				character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Socialize);
				List<Character> list = RuinarchListPool<Character>.Claim();
				targetVisitVillage.PopulateResidentsCurrentlyInsideVillage(list);
				if (list.Count > 0)
				{
					Character randomElement = CollectionUtilities.GetRandomElement(list);
					if (character.jobComponent.CreateDrinkBloodJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, randomElement, out producedJob))
					{
						RuinarchListPool<Character>.Release(list);
						return true;
					}
				}
				RuinarchListPool<Character>.Release(list);
			}
			else if (character.behaviourComponent.visitVillageIntent == VISIT_VILLAGE_INTENT.Preach)
			{
				character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Socialize);
				if (character.jobComponent.TryGetValidEvangelizeTargetInsideVillage(out var targetCharacter, targetVisitVillage))
				{
					return character.jobComponent.TryCreateEvangelizeJob(targetCharacter, out producedJob);
				}
			}
			else if (character.behaviourComponent.visitVillageIntent == VISIT_VILLAGE_INTENT.Enhance_Relationship)
			{
				character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Socialize);
				if (character.currentSettlement is NPCSettlement { ruler: not null } nPCSettlement2)
				{
					character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Enhance_Relationship);
					if (character.jobComponent.CreateEnhanceRelationshipJob(nPCSettlement2.ruler, out producedJob))
					{
						return true;
					}
				}
			}
			else if (character.behaviourComponent.visitVillageIntent == VISIT_VILLAGE_INTENT.Arson)
			{
				character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Socialize);
				if (character.currentSettlement != null)
				{
					List<TileObject> list2 = RuinarchListPool<TileObject>.Claim();
					character.currentSettlement.PopulateTileObjectsWithTraitThatActorCanReach("Flammable", list2, character);
					TileObject randomElement2 = CollectionUtilities.GetRandomElement(list2);
					RuinarchListPool<TileObject>.Release(list2);
					if (randomElement2 != null && character.jobComponent.TriggerArson(randomElement2, out producedJob))
					{
						return true;
					}
				}
			}
			else if (character.behaviourComponent.visitVillageIntent == VISIT_VILLAGE_INTENT.Purchase)
			{
				if (p_foundObject != null)
				{
					if (character.jobComponent.CreateDropItemJob(JOB_TYPE.STOCKPILE_FOOD, p_foundObject, character.homeStructure, out producedJob))
					{
						character.behaviourComponent.ClearOutVisitVillageBehaviour();
						return true;
					}
				}
				else if (p_chosenStructure != null && character.jobComponent.TryCreateStockpileFood(character, p_chosenStructure, out producedJob))
				{
					character.behaviourComponent.ClearOutVisitVillageBehaviour();
					return true;
				}
			}
			if (character.jobComponent.TriggerRoamAroundStructure(JOB_TYPE.VISIT_DIFFERENT_VILLAGE, out producedJob))
			{
				return true;
			}
			producedJob = null;
			return true;
		}
		LocationStructure locationStructure = targetVisitVillage.cityCenter;
		if (locationStructure == null)
		{
			locationStructure = targetVisitVillage.mainStorage ?? targetVisitVillage.GetRandomStructure();
		}
		LocationGridTile locationGridTile = ((locationStructure.passableTiles.Count > 0) ? CollectionUtilities.GetRandomElement(locationStructure.passableTiles) : CollectionUtilities.GetRandomElement(locationStructure.tiles));
		if (character.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile))
		{
			return character.jobComponent.CreateGoToJob(JOB_TYPE.VISIT_DIFFERENT_VILLAGE, locationGridTile, out producedJob);
		}
		character.behaviourComponent.ClearOutVisitVillageBehaviour();
		producedJob = null;
		return false;
	}

	private bool ChatBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.isNormalCharacter && character.hasMarker && character.marker.inVisionCharacters.Count > 0 && CharacterManager.Instance.HasCharacterNotConversedInMinutes(character, 9))
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			character.marker.PopulateCharactersThatIsNotDeadVillagerAndNotConversedInMinutes(list, 9);
			Character character2 = null;
			if (list.Count > 0)
			{
				character2 = CollectionUtilities.GetRandomElement(list);
			}
			RuinarchListPool<Character>.Release(list);
			if (character2 != null)
			{
				if (character.nonActionEventsComponent.CanChat(character2) && GameUtilities.RollChance(50, ref log))
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, character2);
					producedJob = null;
					return true;
				}
				if (character.nonActionEventsComponent.CheckForFlirtTrigger(character, character2, isOnSight: false, ref log, out producedJob))
				{
					return true;
				}
			}
		}
		producedJob = null;
		return false;
	}
}
