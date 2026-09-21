using System;
using System.Collections.Generic;
using Characters.Components;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Object_Pools;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class CharacterTrait : Trait, CharacterEventDispatcher.ITraitListener
{
	private Dictionary<Character, List<string>> _traitsFromOtherCharacterThatThisIsAwareOf;

	public List<TileObject> alreadyInspectedTileObjects { get; private set; }

	public List<Character> charactersAlreadySawForHope { get; private set; }

	public HashSet<Character> charactersThatHaveReactedToThis { get; private set; }

	public List<TileObject> alreadyReactedToFoodPiles { get; private set; }

	public Character owner { get; private set; }

	public bool hasBeenAbductedByWildMonster { get; private set; }

	public bool hasBeenAbductedByPlayerMonster { get; private set; }

	public bool hasSeenFire { get; protected set; }

	public bool hasSeenWet { get; protected set; }

	public bool hasSeenPoisoned { get; protected set; }

	public override Type serializedData => typeof(SaveDataCharacterTrait);

	public Dictionary<Character, List<string>> traitsFromOtherCharacterThatThisIsAwareOf => _traitsFromOtherCharacterThatThisIsAwareOf;

	public CharacterTrait()
	{
		name = "Character Trait";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		isHidden = true;
		hasBeenAbductedByWildMonster = false;
		hasBeenAbductedByPlayerMonster = false;
		alreadyInspectedTileObjects = new List<TileObject>();
		charactersAlreadySawForHope = new List<Character>();
		charactersThatHaveReactedToThis = new HashSet<Character>();
		alreadyReactedToFoodPiles = new List<TileObject>();
		_traitsFromOtherCharacterThatThisIsAwareOf = new Dictionary<Character, List<string>>();
		AddTraitOverrideFunctionIdentifier("Start_Perform_Trait");
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataCharacterTrait saveDataCharacterTrait = p_saveDataTrait as SaveDataCharacterTrait;
		alreadyInspectedTileObjects = SaveUtilities.ConvertIDListToTileObjects(saveDataCharacterTrait.alreadyInspectedTileObjects);
		charactersAlreadySawForHope.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataCharacterTrait.charactersAlreadySawForHope));
		charactersThatHaveReactedToThis = new HashSet<Character>(SaveUtilities.ConvertIDListToCharacters(saveDataCharacterTrait.charactersThatHaveReactedToThis));
		if (saveDataCharacterTrait.alreadyReactedFoodPiles != null)
		{
			alreadyReactedToFoodPiles = SaveUtilities.ConvertIDListToTileObjects(saveDataCharacterTrait.alreadyReactedFoodPiles);
		}
		hasBeenAbductedByPlayerMonster = saveDataCharacterTrait.hasBeenAbductedByPlayerMonster;
		hasBeenAbductedByWildMonster = saveDataCharacterTrait.hasBeenAbductedByWildMonster;
		if (saveDataCharacterTrait.traitsFromOtherCharacterThatThisIsAwareOf == null)
		{
			return;
		}
		foreach (KeyValuePair<string, List<string>> item in saveDataCharacterTrait.traitsFromOtherCharacterThatThisIsAwareOf)
		{
			Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(item.Key);
			if (characterByPersistentID != null)
			{
				_traitsFromOtherCharacterThatThisIsAwareOf.Add(characterByPersistentID, item.Value);
				characterByPersistentID.eventDispatcher.SubscribeToCharacterLostTrait(this);
			}
		}
	}

	public void AddAlreadyInspectedObject(TileObject to)
	{
		if (!alreadyInspectedTileObjects.Contains(to))
		{
			alreadyInspectedTileObjects.Add(to);
		}
	}

	public bool HasAlreadyInspectedObject(TileObject to)
	{
		return alreadyInspectedTileObjects.Contains(to);
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		owner = addTo as Character;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		owner = addedTo as Character;
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		owner = null;
		charactersAlreadySawForHope?.Clear();
		charactersThatHaveReactedToThis?.Clear();
		_traitsFromOtherCharacterThatThisIsAwareOf?.Clear();
	}

	public void AddCharacterThatHasReactedToThis(Character character)
	{
		if (!charactersThatHaveReactedToThis.Contains(character))
		{
			charactersThatHaveReactedToThis.Add(character);
		}
	}

	public void RemoveCharacterThatHasReactedToThis(Character character)
	{
		charactersThatHaveReactedToThis.Remove(character);
	}

	public bool HasReactedToThis(Character character)
	{
		return charactersThatHaveReactedToThis.Contains(character);
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is TileObject tileObject && characterThatWillDoJob.limiterComponent.canMove)
		{
			if (tileObject is TreasureChest)
			{
				if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.OPEN_CHEST, tileObject) && !characterThatWillDoJob.traitContainer.HasTrait("Suspicious"))
				{
					characterThatWillDoJob.jobComponent.CreateOpenChestJob(tileObject);
				}
			}
			else if (tileObject is CultistKit && !characterThatWillDoJob.traitContainer.HasTrait("Demon Cultist"))
			{
				if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.DESTROY, tileObject))
				{
					CRIME_SEVERITY p_crimeSeverity = CRIME_SEVERITY.None;
					if (characterThatWillDoJob.faction != null)
					{
						p_crimeSeverity = characterThatWillDoJob.faction.factionType.GetCrimeSeverity(CRIME_TYPE.Demon_Worship);
					}
					if (p_crimeSeverity.IsConsideredACrime())
					{
						characterThatWillDoJob.jobComponent.TriggerDestroy(tileObject, "Destroy_Cultist_Kit");
					}
				}
			}
			else if (ShouldInspectItem(characterThatWillDoJob, tileObject))
			{
				if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.INSPECT, tileObject) && !characterThatWillDoJob.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.INSPECT))
				{
					characterThatWillDoJob.jobComponent.TriggerInspect(tileObject);
				}
			}
			else if (tileObject.traitContainer.HasTrait("Edible") && characterThatWillDoJob.needsComponent.isStarving && !characterThatWillDoJob.traitContainer.HasTrait("Vampire") && !characterThatWillDoJob.traitContainer.HasTrait("Paralyzed") && characterThatWillDoJob.previousCharacterDataComponent.previousJobType != JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT)
			{
				characterThatWillDoJob.jobComponent.CreateFullnessRecoveryOnSight(tileObject);
			}
			else if (!characterThatWillDoJob.IsInventoryAtFullCapacity() && tileObject.traitContainer.HasTrait("Treasure") && !characterThatWillDoJob.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.TAKE_ITEM_ON_SIGHT) && !characterThatWillDoJob.traitContainer.HasTrait("Suspicious"))
			{
				if (tileObject.CanBePickedUpNormallyUponVisionBy(characterThatWillDoJob) && !characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.TAKE_ITEM_ON_SIGHT))
				{
					int num = 100;
					if (characterThatWillDoJob.HasItem(tileObject.name) || characterThatWillDoJob.HasOwnedItemInHomeStructure(tileObject.name))
					{
						num = 10;
						if (characterThatWillDoJob.GetItemCount(tileObject.name) + characterThatWillDoJob.GetNumOfOwnedItemsInHomeStructure(tileObject.name) >= 2)
						{
							num = 0;
						}
					}
					if (UnityEngine.Random.Range(0, 100) < num)
					{
						if (tileObject.characterOwner != null && !tileObject.IsOwnedBy(characterThatWillDoJob))
						{
							characterThatWillDoJob.jobComponent.CreateStealItemJob(JOB_TYPE.TAKE_ITEM_ON_SIGHT, tileObject);
						}
						else
						{
							characterThatWillDoJob.jobComponent.CreateTakeItemOnSightJob(tileObject);
						}
						return true;
					}
				}
			}
			else if (tileObject.tileObjectType.IsDemonicStructureTileObject() && tileObject.gridTileLocation?.structure is DemonicStructure demonicStructure)
			{
				bool flag = false;
				if (WorldSettings.Instance.worldSettingsData.IsRetaliationAllowed() && !PlayerManager.Instance.player.retaliationComponent.isRetaliating && !PlayerManager.Instance.player.HasAlreadyReportedADemonicStructure(characterThatWillDoJob) && characterThatWillDoJob.limiterComponent.canWitness && !characterThatWillDoJob.behaviourComponent.isAttackingDemonicStructure && characterThatWillDoJob.homeSettlement != null && characterThatWillDoJob.necromancerTrait == null && characterThatWillDoJob.race.IsSapient() && characterThatWillDoJob.hasMarker && characterThatWillDoJob.carryComponent.IsNotBeingCarried() && !characterThatWillDoJob.isAlliedWithPlayer && (!characterThatWillDoJob.partyComponent.hasParty || !characterThatWillDoJob.partyComponent.currentParty.isActive || (characterThatWillDoJob.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Counterattack && !(characterThatWillDoJob.partyComponent.currentParty.currentQuest is IRescuePartyQuest))) && characterThatWillDoJob.faction != null && characterThatWillDoJob.faction.isMajorNonPlayer)
				{
					if (!characterThatWillDoJob.faction.isAwareOfPlayer)
					{
						flag = characterThatWillDoJob.jobComponent.CreateReportDemonicStructure(demonicStructure);
						if (flag)
						{
							AkSoundEngine.PostEvent("Play_Location_Discovered", InnerMapCameraMove.Instance.gameObject);
							Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "structure_discovered", LOG_TAG.Player, LOG_TAG.Major);
							log.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
							log.AddToFillers(demonicStructure, demonicStructure.GetNameRelativeTo(null), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
							log.AddLogToDatabase();
							PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
						}
					}
					else if (ChanceData.RollChance(CHANCE_TYPE.Counterattack) && !characterThatWillDoJob.faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Counterattack))
					{
						characterThatWillDoJob.faction.partyQuestBoard.CreateCounterattackPartyQuest(characterThatWillDoJob, characterThatWillDoJob.homeSettlement);
					}
				}
				if (!flag && !characterThatWillDoJob.movementComponent.hasMovedOnCorruption && characterThatWillDoJob.limiterComponent.canWitness && !characterThatWillDoJob.behaviourComponent.isAttackingDemonicStructure && !characterThatWillDoJob.reactionComponent.hasBeenShockedByDemonicStructure && !PlayerManager.Instance.player.HasAlreadyReportedADemonicStructure(characterThatWillDoJob) && (!characterThatWillDoJob.partyComponent.hasParty || !characterThatWillDoJob.partyComponent.currentParty.isActive || (characterThatWillDoJob.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Counterattack && !(characterThatWillDoJob.partyComponent.currentParty.currentQuest is IRescuePartyQuest) && characterThatWillDoJob.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Heirloom_Hunt)) && !characterThatWillDoJob.isAlliedWithPlayer && characterThatWillDoJob.necromancerTrait == null && !characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.REPORT_CORRUPTED_STRUCTURE) && characterThatWillDoJob.isNormalCharacter)
				{
					characterThatWillDoJob.reactionComponent.SetHasBeedShockedByDemonicStructure(p_state: true);
					Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Reason_Table", "Saw_Demonic_Structure", LOG_TAG.Life_Changes);
					log2.AddToFillers(demonicStructure, demonicStructure.GetNameRelativeTo(null), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
					string logText = log2.logText;
					LogPool.Release(log2);
					characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, characterThatWillDoJob, "", null, logText);
				}
			}
		}
		if (targetPOI is Character character)
		{
			if (characterThatWillDoJob.limiterComponent.canMove && characterThatWillDoJob.limiterComponent.canPerform)
			{
				if (owner.partyComponent.hasParty && owner.partyComponent.currentParty.isActive && owner.partyComponent.currentParty.currentQuest is IRescuePartyQuest rescuePartyQuest && rescuePartyQuest.targetCharacter == character)
				{
					if (!character.isDead)
					{
						if (character.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared", "Enslaved"))
						{
							if (owner.jobComponent.TriggerReleaseJob(character))
							{
								rescuePartyQuest.SetIsReleasing(state: true);
							}
						}
						else
						{
							rescuePartyQuest.SetIsReleasing(state: false);
							rescuePartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Safe"));
							if (character.traitContainer.HasTrait("Paralyzed") && !character.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.MOVE_CHARACTER))
							{
								owner.jobComponent.TryTriggerMoveCharacter(character);
							}
						}
					}
					else
					{
						rescuePartyQuest.SetIsReleasing(state: false);
						rescuePartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Dead"));
					}
				}
				if (!character.isDead)
				{
					if (!character.isNormalCharacter)
					{
						string opinionLabel = characterThatWillDoJob.relationshipContainer.GetOpinionLabel(character);
						if (opinionLabel == "Friend")
						{
							if (!charactersAlreadySawForHope.Contains(character))
							{
								charactersAlreadySawForHope.Add(character);
								characterThatWillDoJob.needsComponent.AdjustHope(-5f);
							}
						}
						else if (opinionLabel == "Close Friend" && !charactersAlreadySawForHope.Contains(character))
						{
							charactersAlreadySawForHope.Add(character);
							characterThatWillDoJob.needsComponent.AdjustHope(-10f);
						}
					}
				}
				else
				{
					Obsessed traitOrStatus = owner.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed");
					BaseSettlement settlement;
					if (character.traitContainer.HasTrait("Mummified") && character.grave == null && GameUtilities.RollChance(40) && (traitOrStatus == null || traitOrStatus.targetCharacter != character))
					{
						owner.jobComponent.TriggerCheckOut(character);
					}
					else if (owner.isNormalCharacter && character.isNormalCharacter && owner.race != RACE.RATMAN && character.race != RACE.RATMAN && character.gridTileLocation != null && (!character.gridTileLocation.IsPartOfSettlement() || (character.gridTileLocation.IsPartOfSettlement(out settlement) && settlement.locationType != LOCATION_TYPE.VILLAGE)) && owner.relationshipContainer.GetOpinionLabel(character) != "Rival" && (traitOrStatus == null || traitOrStatus.targetCharacter != character))
					{
						if (owner.partyComponent.isMemberThatJoinedQuest)
						{
							owner.jobComponent.TriggerPersonalBuryInActivePartyJob(character);
						}
						else if (owner.traitContainer.HasTrait("Necromancer"))
						{
							if (owner.faction.factionType.type != FACTION_TYPE.Undead)
							{
								owner.jobComponent.TriggerPersonalOutsideVillageBuryJob(character);
							}
						}
						else
						{
							owner.jobComponent.TriggerPersonalOutsideVillageBuryJob(character);
						}
					}
				}
			}
			if (!character.isDead && owner.isNormalCharacter && character.isNormalCharacter && owner.faction != character.faction && owner.faction != null && character.currentStructure != null && character.currentStructure.isInterior && character.currentStructure.settlementLocation != null && character.currentStructure.settlementLocation.owner == owner.faction)
			{
				bool flag2 = true;
				if (character.currentStructure.structureType.IsSpecialStructure())
				{
					flag2 = false;
				}
				else
				{
					switch (character.currentStructure.structureType)
					{
					case STRUCTURE_TYPE.TAVERN:
					case STRUCTURE_TYPE.CEMETERY:
					case STRUCTURE_TYPE.CITY_CENTER:
					case STRUCTURE_TYPE.BARRACKS:
					case STRUCTURE_TYPE.HOSPICE:
					case STRUCTURE_TYPE.FARM:
					case STRUCTURE_TYPE.LUMBERYARD:
					case STRUCTURE_TYPE.MINE:
					case STRUCTURE_TYPE.CULT_TEMPLE:
					case STRUCTURE_TYPE.WORKSHOP:
					case STRUCTURE_TYPE.FISHERY:
					case STRUCTURE_TYPE.MAGIC_ACADEMY:
					case STRUCTURE_TYPE.WYVERN_COOP:
					case STRUCTURE_TYPE.ARROW_TOWER:
					case STRUCTURE_TYPE.LIGHTNING_TOWER:
					case STRUCTURE_TYPE.BEAST_PEN:
						flag2 = false;
						break;
					}
				}
				if (flag2 && character.currentActionNode != null && character.currentActionNode.targetStructure != character.currentStructure)
				{
					flag2 = false;
				}
				if (flag2 && owner.hasMarker && !character.traitContainer.HasTrait("Restrained", "Unconscious"))
				{
					bool flag3 = true;
					if (character.traitContainer.HasTrait("Prisoner") && character.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner").IsConsideredPrisonerOf(owner))
					{
						flag3 = false;
					}
					if (flag3 && character.characterClass.className == "Noble" && !character.partyComponent.isMemberThatJoinedQuest)
					{
						flag3 = false;
					}
					if (flag3 && character.isVagrant)
					{
						flag3 = false;
					}
					if (flag3 && character.traitContainer.HasTrait("Demon Cultist") && owner.traitContainer.HasTrait("Demon Cultist"))
					{
						flag3 = false;
					}
					if (flag3 && owner.relationshipContainer.IsFriendsWith(character))
					{
						flag3 = false;
					}
					if (flag3 && character.currentActionNode != null && character.currentActionNode.isPerformingActualAction && character.currentActionNode.action.goapType == INTERACTION_TYPE.VAMPIRIC_EMBRACE && character.currentActionNode.target == owner)
					{
						flag3 = false;
					}
					if (flag3)
					{
						owner.assumptionComponent.CreateAndReactToNewAssumption(character, owner, INTERACTION_TYPE.TRESPASSING, REACTION_STATUS.WITNESSED, isFabricated: false);
					}
				}
			}
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	private bool ShouldInspectItem(Character characterThatWillDoJob, TileObject item)
	{
		if (item is Excalibur excalibur)
		{
			if (characterThatWillDoJob.isNormalCharacter)
			{
				return !excalibur.HasInspectedThis(characterThatWillDoJob);
			}
			return false;
		}
		if (item is BerserkOrb)
		{
			return true;
		}
		if (item is GorgonEye)
		{
			return true;
		}
		if (item is HeartOfTheWind)
		{
			return true;
		}
		return false;
	}

	private void HerbPlantProcessing(Character actor, TileObject herbPlant)
	{
		NPCSettlement homeSettlement = actor.homeSettlement;
		if (homeSettlement == null)
		{
			return;
		}
		LocationStructure firstStructureOfType = homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
		if (firstStructureOfType != null && herbPlant.gridTileLocation != null && herbPlant.gridTileLocation.structure.structureType != STRUCTURE_TYPE.CITY_CENTER && herbPlant.gridTileLocation.structure.structureType != STRUCTURE_TYPE.HOSPICE)
		{
			int numberOfTileObjects = firstStructureOfType.GetNumberOfTileObjects(TILE_OBJECT_TYPE.HERB_PLANT);
			int numberOfJobsThatTargetsTileObjectOfType = homeSettlement.GetNumberOfJobsThatTargetsTileObjectOfType(TILE_OBJECT_TYPE.HERB_PLANT);
			if (numberOfTileObjects + numberOfJobsThatTargetsTileObjectOfType < 4)
			{
				homeSettlement.settlementJobTriggerComponent.TryCreateHaulJobForItems(herbPlant, firstStructureOfType);
			}
		}
	}

	public override bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction)
	{
		if (node.poiTarget.traitContainer.HasTrait("Booby Trapped"))
		{
			bool num = node.poiTarget.traitContainer.GetTraitOrStatus<BoobyTrapped>("Booby Trapped").OnPerformGoapAction(node, ref willStillContinueAction);
			if (num && !node.hasBeenReset && node.actor.jobQueue.jobsInQueue.Count > 0)
			{
				node.actor.jobQueue.jobsInQueue[0].CancelJob();
			}
			return num;
		}
		return false;
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		charactersAlreadySawForHope.Remove(p_character);
		charactersThatHaveReactedToThis.Remove(p_character);
		_traitsFromOtherCharacterThatThisIsAwareOf.Remove(p_character);
	}

	public void SetHasBeenAbductedByPlayerMonster(bool state)
	{
		hasBeenAbductedByPlayerMonster = state;
	}

	public void SetHasBeenAbductedByWildMonster(bool state)
	{
		hasBeenAbductedByWildMonster = state;
	}

	public void BecomeAwareOfTrait(Character p_character, Trait p_trait)
	{
		if (!traitsFromOtherCharacterThatThisIsAwareOf.ContainsKey(p_character))
		{
			traitsFromOtherCharacterThatThisIsAwareOf.Add(p_character, new List<string>());
			p_character.eventDispatcher.SubscribeToCharacterLostTrait(this);
		}
		traitsFromOtherCharacterThatThisIsAwareOf[p_character].Add(p_trait.name);
	}

	public bool IsAwareOfTrait(Character p_character, Trait p_trait)
	{
		if (traitsFromOtherCharacterThatThisIsAwareOf.ContainsKey(p_character))
		{
			return traitsFromOtherCharacterThatThisIsAwareOf[p_character].Contains(p_trait.name);
		}
		return false;
	}

	public void OnCharacterGainedTrait(Character p_character, Trait p_gainedTrait)
	{
	}

	public void OnCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy)
	{
		if (traitsFromOtherCharacterThatThisIsAwareOf.ContainsKey(p_character) && traitsFromOtherCharacterThatThisIsAwareOf[p_character].Remove(p_lostTrait.name) && traitsFromOtherCharacterThatThisIsAwareOf[p_character].Count == 0)
		{
			traitsFromOtherCharacterThatThisIsAwareOf.Remove(p_character);
			p_character.eventDispatcher.UnsubscribeToCharacterLostTrait(this);
		}
	}

	public void AddFoodPileAsReactedTo(FoodPile p_foodPile)
	{
		alreadyReactedToFoodPiles.Add(p_foodPile);
	}

	public void RemoveFoodPileAsReactedTo(FoodPile p_foodPile)
	{
		alreadyReactedToFoodPiles.Remove(p_foodPile);
	}

	public bool HasAlreadyReactedToFoodPile(FoodPile p_foodPile)
	{
		return alreadyReactedToFoodPiles.Contains(p_foodPile);
	}

	public void SetHasSeenFire(bool state)
	{
		hasSeenFire = state;
	}

	public void SetHasSeenWet(bool state)
	{
		hasSeenWet = state;
	}

	public void SetHasSeenPoisoned(bool state)
	{
		hasSeenPoisoned = state;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
		charactersAlreadySawForHope.Contains(p_character);
		charactersThatHaveReactedToThis.Contains(p_character);
		_traitsFromOtherCharacterThatThisIsAwareOf.ContainsKey(p_character);
	}

	public override void CleanUp()
	{
		base.CleanUp();
		charactersAlreadySawForHope?.Clear();
		charactersThatHaveReactedToThis?.Clear();
		_traitsFromOtherCharacterThatThisIsAwareOf?.Clear();
	}
}
