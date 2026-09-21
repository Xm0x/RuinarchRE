using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Object_Pools;
using Traits;
using UnityEngine;
using UtilityScripts;

public class ReactionComponent : CharacterComponent
{
	public List<Character> assumptionSuspects { get; private set; }

	public List<Character> charactersThatSawThisDead { get; private set; }

	public bool isHidden { get; private set; }

	public Character disguisedCharacter { get; private set; }

	public bool hasBeenShockedByDemonicStructure { get; private set; }

	public bool isDisguised => disguisedCharacter != null;

	public ReactionComponent()
	{
		assumptionSuspects = new List<Character>();
		charactersThatSawThisDead = new List<Character>();
	}

	public ReactionComponent(SaveDataReactionComponent data)
	{
		assumptionSuspects = new List<Character>();
		charactersThatSawThisDead = new List<Character>();
		isHidden = data.isHidden;
		hasBeenShockedByDemonicStructure = data.hasBeenShockedByDemonicStructure;
	}

	public void ReactTo(IPointOfInterest target, ref string debugLog)
	{
		Character character = base.owner;
		if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
		{
			Character character2 = target as Character;
			ReactTo(character, character2, ref debugLog);
			if (character2.carryComponent.carriedPOI is TileObject targetTileObject)
			{
				ReactToCarriedObject(character, targetTileObject, character2, ref debugLog);
			}
		}
		else if (target.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
		{
			ReactTo(character, target as TileObject, ref debugLog);
		}
		if (!character.isNormalCharacter || character.combatComponent.isInActualCombat)
		{
			return;
		}
		List<Trait> traitOverrideFunctions = character.traitContainer.GetTraitOverrideFunctions("See_Poi_Trait");
		if (traitOverrideFunctions != null)
		{
			for (int i = 0; i < traitOverrideFunctions.Count; i++)
			{
				traitOverrideFunctions[i].OnSeePOI(target, character);
			}
		}
	}

	public void ReactToDisguised(Character targetCharacter, Character copiedCharacter, ref string debugLog)
	{
		if (base.owner == copiedCharacter)
		{
			base.owner.combatComponent.Fight(targetCharacter, "Hostility");
			base.owner.interruptComponent.TriggerInterrupt(INTERRUPT.Surprised, targetCharacter, "", null, "Shocked_Copycat_Reason");
			if (targetCharacter.carryComponent.carriedPOI is TileObject targetTileObject)
			{
				ReactToCarriedObject(base.owner, targetTileObject, copiedCharacter, ref debugLog);
			}
			if (!base.owner.isNormalCharacter || base.owner.combatComponent.isInActualCombat)
			{
				return;
			}
			List<Trait> traitOverrideFunctions = base.owner.traitContainer.GetTraitOverrideFunctions("See_Poi_Trait");
			if (traitOverrideFunctions != null)
			{
				for (int i = 0; i < traitOverrideFunctions.Count; i++)
				{
					traitOverrideFunctions[i].OnSeePOI(copiedCharacter, base.owner);
				}
			}
		}
		else
		{
			ReactTo(targetCharacter, ref debugLog);
		}
	}

	public string ReactTo(IReactable reactable, REACTION_STATUS status, bool addLog = true)
	{
		if (!base.owner.isNormalCharacter)
		{
			return string.Empty;
		}
		if (reactable.awareCharacters.Contains(base.owner))
		{
			return "aware";
		}
		bool flag = true;
		if (status == REACTION_STATUS.WITNESSED && reactable.isStealth && !CanReactVigilant(base.owner, reactable) && reactable.target == base.owner)
		{
			flag = false;
		}
		if (flag)
		{
			reactable.AddAwareCharacter(base.owner);
		}
		if (reactable.GetReactableEffect(base.owner) == REACTABLE_EFFECT.Negative)
		{
			if (reactable is ActualGoapNode node)
			{
				base.owner.rumorComponent.AddAssumedWitnessedOrInformedNegativeInfo(node);
			}
			else if (reactable is Assumption assumption)
			{
				base.owner.rumorComponent.AddAssumedWitnessedOrInformedNegativeInfo(assumption.assumedAction);
			}
		}
		if (status == REACTION_STATUS.WITNESSED)
		{
			ReactToWitnessedReactable(reactable, addLog);
			return string.Empty;
		}
		return ReactToInformedReactable(reactable, addLog);
	}

	public string ReactToIntel(IReactable reactable)
	{
		if (!base.owner.isNormalCharacter)
		{
			return string.Empty;
		}
		if (reactable.awareCharacters.Contains(base.owner))
		{
			return "aware";
		}
		reactable.AddAwareCharacter(base.owner);
		if (reactable.GetReactableEffect(base.owner) == REACTABLE_EFFECT.Negative)
		{
			if (reactable is ActualGoapNode node)
			{
				base.owner.rumorComponent.AddAssumedWitnessedOrInformedNegativeInfo(node);
			}
			else if (reactable is Assumption assumption)
			{
				base.owner.rumorComponent.AddAssumedWitnessedOrInformedNegativeInfo(assumption.assumedAction);
			}
		}
		return ReactToInformedReactable(reactable, addLog: true);
	}

	private void ReactToWitnessedReactable(IReactable reactable, bool addLog)
	{
		if (base.owner.combatComponent.isInActualCombat)
		{
			return;
		}
		Character actor = reactable.actor;
		IPointOfInterest target = reactable.target;
		Character character = target as Character;
		if (actor.reactionComponent.disguisedCharacter != null)
		{
			actor = actor.reactionComponent.disguisedCharacter;
		}
		if (character != null)
		{
			if (character.hasBeenCleanedUp)
			{
				return;
			}
			if (character.reactionComponent.disguisedCharacter != null)
			{
				target = character.reactionComponent.disguisedCharacter;
			}
		}
		if ((base.owner.faction != null && actor.faction != null && base.owner.faction != actor.faction && base.owner.faction.IsHostileWith(actor.faction)) || reactable.informationLog == null || !reactable.informationLog.hasValue)
		{
			return;
		}
		if (actor != base.owner && target != base.owner)
		{
			string text = reactable.ReactionToActor(actor, target, base.owner, REACTION_STATUS.WITNESSED);
			text.HasEmotion();
			reactable.ReactionToTarget(actor, target, base.owner, REACTION_STATUS.WITNESSED).HasEmotion();
			if (addLog && text.HasEmotion() && (!(reactable is ActualGoapNode actualGoapNode) || (actualGoapNode.action.ShouldAddLogs(actualGoapNode) && CharacterManager.Instance.CanAddCharacterLogOrShowNotif(actualGoapNode.goapType))))
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "witness_event", LOG_TAG.Witnessed, reactable as ActualGoapNode);
				log.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.PARTY_1);
				log.AddToFillers(null, reactable.informationLog.unreplacedText, LOG_IDENTIFIER.APPEND);
				log.AddToFillers(reactable.informationLog.fillers);
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "emotions_reaction", LOG_TAG.Witnessed);
				log2.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log2.AddToFillers(null, Utilities.GetFirstFewEmotionsAndComafy(text, 2), LOG_IDENTIFIER.STRING_1);
				log.AddToFillers(null, log2.logText, LOG_IDENTIFIER.PARTY_2);
				log.AddLogToDatabase(releaseLogAfter: true);
				LogPool.Release(log2);
			}
		}
		else if (target == base.owner && (!reactable.isStealth || CanReactVigilant(target, reactable)))
		{
			string text2 = reactable.ReactionOfTarget(actor, target, REACTION_STATUS.WITNESSED);
			if (text2.HasEmotion())
			{
				Log log3 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "emotions_reaction", LOG_TAG.Life_Changes);
				log3.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log3.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log3.AddToFillers(null, Utilities.GetFirstFewEmotionsAndComafy(text2, 2), LOG_IDENTIFIER.STRING_1);
				log3.AddLogToDatabase(releaseLogAfter: true);
			}
		}
	}

	private string ReactToInformedReactable(IReactable reactable, bool addLog)
	{
		if (reactable.informationLog == null || !reactable.informationLog.hasValue)
		{
			return string.Empty;
		}
		Character actor = reactable.actor;
		IPointOfInterest target = reactable.target;
		if (actor.reactionComponent.disguisedCharacter != null)
		{
			actor = actor.reactionComponent.disguisedCharacter;
		}
		if (target is Character character && character.reactionComponent.disguisedCharacter != null)
		{
			target = character.reactionComponent.disguisedCharacter;
		}
		string text = string.Empty;
		if (actor != base.owner && target != base.owner)
		{
			string text2 = reactable.ReactionToActor(actor, target, base.owner, REACTION_STATUS.INFORMED);
			bool num = text2.HasEmotion();
			string text3 = reactable.ReactionToTarget(actor, target, base.owner, REACTION_STATUS.INFORMED);
			bool flag = text3.HasEmotion();
			text = ((!num && !string.IsNullOrEmpty(text2)) ? (text + text2) : ((flag || string.IsNullOrEmpty(text3)) ? (text + text2 + "/" + text3) : (text + text2)));
			if (addLog && text2.HasEmotion())
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "informed_event", LOG_TAG.Informed, reactable as ActualGoapNode);
				log.AddToFillers(reactable.informationLog.fillers);
				log.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.PARTY_1);
				log.AddToFillers(null, reactable.informationLog.unreplacedText, LOG_IDENTIFIER.APPEND);
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "emotions_reaction", LOG_TAG.Informed);
				log2.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log2.AddToFillers(null, Utilities.GetFirstFewEmotionsAndComafy(text2, 2), LOG_IDENTIFIER.STRING_1);
				log.AddToFillers(null, log2.logText, LOG_IDENTIFIER.PARTY_2);
				log.AddLogToDatabase(releaseLogAfter: true);
				LogPool.Release(log2);
			}
		}
		else if (target == base.owner && target is Character)
		{
			string text4 = reactable.ReactionOfTarget(actor, target, REACTION_STATUS.INFORMED);
			if (text4.HasEmotion())
			{
				Log log3 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "emotions_reaction", LOG_TAG.Informed);
				log3.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log3.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log3.AddToFillers(null, Utilities.GetFirstFewEmotionsAndComafy(text4, 2), LOG_IDENTIFIER.STRING_1);
				log3.AddLogToDatabase(releaseLogAfter: true);
			}
			text = text4;
		}
		return text;
	}

	private void ReactTo(Character actor, Character targetCharacter, ref string debugLog)
	{
		Character character = actor;
		Character character2 = targetCharacter;
		if (actor.reactionComponent.disguisedCharacter != null)
		{
			character = actor.reactionComponent.disguisedCharacter;
		}
		if (targetCharacter.reactionComponent.disguisedCharacter != null)
		{
			character2 = targetCharacter.reactionComponent.disguisedCharacter;
		}
		int chance = ChanceData.GetChance(CHANCE_TYPE.Tame_Beast);
		if (character2.characterClass.className == "Dragon" || character2.characterClass.className == "Unicorn")
		{
			chance = 3;
		}
		if (GameUtilities.RollChance(chance) && character.limiterComponent.canWitness && !character.petComponent.ownedPetsData.IsAtMaxCapacity() && character.faction != null && character2.faction != null && !character2.isDead && (character.characterClass.className == "Druid" || (character.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Beastmasters) && character.race.IsSapient())))
		{
			bool flag = false;
			if (character.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Wyvern_Tamers) && targetCharacter is Wyvern)
			{
				RaceManager.Instance.GetRaceData(character.race);
				if (character.race.IsSapient() && character.characterClass.IsCombatant() && !targetCharacter.petComponent.HasPetOwner() && targetCharacter.limiterComponent.canPerform && !character.petComponent.HasWyvernPet())
				{
					actor.interruptComponent.TriggerInterrupt(INTERRUPT.Tame_Beast, targetCharacter);
					flag = true;
				}
			}
			if (!flag && (character.faction == character2.faction || character2.faction.factionType.type == FACTION_TYPE.Wild_Monsters || character2.faction.factionType.type == FACTION_TYPE.Demons || character.faction.IsHostileWith(character2.faction)) && RaceManager.Instance.GetRaceData(character2.race).category == CHARACTER_CATEGORY.Beast && !character2.traitContainer.HasTrait("Resting") && !character2.traitContainer.HasTrait("Hibernating") && !character2.traitContainer.HasTrait("Temporal") && !character2.petComponent.HasPetOwner() && !(character2 is Animal) && !character2.movementComponent.isStationary)
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Tame_Beast, character2);
			}
		}
		bool flag2 = false;
		flag2 = ((character2 == targetCharacter || actor.faction != targetCharacter.faction) ? (character.IsHostileWith(character2) && character.IsLycanHostileWith(character2)) : (actor.IsHostileWith(targetCharacter) && actor.IsLycanHostileWith(targetCharacter)));
		if (flag2)
		{
			if (character.characterClass.className == "Noble" && character2.faction != null && character2.faction.isMajorNonPlayer && !character.partyComponent.isMemberThatJoinedQuest)
			{
				flag2 = false;
			}
			else if (character2.characterClass.className == "Noble" && character.faction != null && character.faction.isMajorNonPlayer && !character2.partyComponent.isMemberThatJoinedQuest)
			{
				flag2 = false;
			}
		}
		if (actor.behaviourComponent.HasBehaviour(typeof(DeMooderBehaviour)) || actor.behaviourComponent.HasBehaviour(typeof(DisablerBehaviour)) || actor.traitContainer.HasTrait("Dazed") || (actor is Summon summon && summon.ReactionToAnotherCharacter(actor, targetCharacter, character, character2, flag2, ref debugLog)))
		{
			return;
		}
		if (character2 is Dragon dragon && (!character2.limiterComponent.canMove || !character2.limiterComponent.canPerform) && actor.isNormalCharacter && !character2.isDead && !dragon.charactersThatAreWary.Contains(actor))
		{
			actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, dragon);
			dragon.AddCharacterThatWary(actor);
		}
		if (character2.characterClass.className == "Werewolf" && character.homeSettlement != null && character2.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(character.homeSettlement) && character2.lycanData != null && !character2.lycanData.DoesCharacterKnowThisLycan(character) && character.homeSettlement.eventManager.CanHaveEvents() && CrimeManager.Instance.GetCrimeSeverity(character, character2, character2, CRIME_TYPE.Werewolf).IsConsideredACrime() && !character.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Werewolf_Hunt) && ChanceData.RollChance(CHANCE_TYPE.Werewolf_Hunt_On_See_Werewolf, ref debugLog))
		{
			character.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Werewolf_Hunt);
		}
		if (actor.race == RACE.RATMAN)
		{
			Prisoner traitOrStatus = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
			if (traitOrStatus != null && targetCharacter.traitContainer.HasTrait("Restrained") && actor.faction == traitOrStatus.prisonerOfFaction && !actor.IsAtHome() && !actor.faction.isMajorNonPlayer)
			{
				LocationStructure locationStructure = null;
				if (actor.homeStructure != null)
				{
					if (!(actor.homeStructure is ThePortal))
					{
						locationStructure = actor.homeStructure;
					}
				}
				else if (actor.homeSettlement != null && actor.homeSettlement.mainStorage != null)
				{
					locationStructure = actor.homeSettlement.mainStorage;
				}
				if (locationStructure != null)
				{
					actor.jobComponent.CreateAbductJob(targetCharacter, locationStructure);
				}
				return;
			}
		}
		if (flag2)
		{
			HostileReactionToCharacter(actor, targetCharacter, character, character2, ref debugLog);
		}
		else if (!actor.combatComponent.isInActualCombat)
		{
			if (actor.traitContainer.HasTrait("Polymorphed"))
			{
				return;
			}
			NonHostileNotInCombatReactionToCharacter(actor, targetCharacter, character, character2, ref debugLog);
		}
		if (CanCharacterEatCorpseOf(actor, targetCharacter) && targetCharacter.isDead && targetCharacter.grave == null && actor.limiterComponent.canPerform && actor.limiterComponent.canMove && actor.limiterComponent.canDoFullnessRecovery && !actor.combatComponent.isInCombat && (actor.currentActionNode == null || actor.currentActionNode.action.goapType == INTERACTION_TYPE.ROAM || !actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.MONSTER_EAT_CORPSE)))
		{
			actor.jobComponent.TriggerEatCorpse(targetCharacter);
		}
		if (actor.petComponent.petOwner == character2)
		{
			PetReactToPetOwner(actor, character2, ref debugLog);
		}
		else if (character2.petComponent.HasPetOwner() && character2.petComponent.petOwner != actor)
		{
			ReactionToPet(actor, character2, ref debugLog);
		}
		targetCharacter.defaultCharacterTrait.AddCharacterThatHasReactedToThis(base.owner);
	}

	private bool CanCharacterEatCorpseOf(Character p_actor, Character p_target)
	{
		if (p_actor is Animal)
		{
			return false;
		}
		if (p_target is Animal)
		{
			if (p_actor is Summon)
			{
				Faction faction = p_actor.faction;
				if (faction != null && faction.factionType.type == FACTION_TYPE.Wild_Monsters)
				{
					return true;
				}
			}
			return p_actor.isConsideredRatman;
		}
		if (p_actor.isConsideredRatman)
		{
			return p_actor.needsComponent.isStarving;
		}
		return false;
	}

	private void ReactTo(Character actor, TileObject targetTileObject, ref string debugLog)
	{
		targetTileObject.GeneralReactionToTileObject(actor, ref debugLog);
		if (actor.isNormalCharacter)
		{
			targetTileObject.VillagerReactionToTileObject(actor, ref debugLog);
		}
	}

	private void ReactToCarriedObject(Character actor, TileObject targetTileObject, Character carrier, ref string debugLog)
	{
		if (targetTileObject is CultistKit)
		{
			Character character = carrier;
			if (carrier.reactionComponent.disguisedCharacter != null)
			{
				character = carrier.reactionComponent.disguisedCharacter;
			}
			if (!character.isDead && CrimeManager.Instance.IsConsideredACrimeByCharacter(actor, character, targetTileObject, CRIME_TYPE.Demon_Worship))
			{
				actor.assumptionComponent.CreateAndReactToNewAssumption(character, targetTileObject, INTERACTION_TYPE.IS_CULTIST, REACTION_STATUS.WITNESSED, !character.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship));
			}
		}
	}

	public void ReactToCombat(CombatState combat, IPointOfInterest poiHit)
	{
		Character character = combat.stateComponent.owner;
		Character character2 = base.owner;
		if (character2.combatComponent.isInCombat)
		{
			if (character2 == poiHit)
			{
				CombatState combatState = character2.stateComponent.currentState as CombatState;
				if (combatState.isAttacking && combatState.currentClosestHostile != null && combatState.currentClosestHostile != character && combatState.currentClosestHostile is Character character3 && character3.combatComponent.isInCombat && !(character3.stateComponent.currentState as CombatState).isAttacking)
				{
					character2.combatComponent.SetWillProcessCombat(state: true);
				}
			}
		}
		else
		{
			if (!base.owner.isNormalCharacter || base.owner.isDead || !base.owner.limiterComponent.canPerform || character2.IsHostileWith(character) || combat.DidCharacterAlreadyReactToThisCombat(character2) || (poiHit is Character otherCharacter && character2.IsHostileWith(otherCharacter)))
			{
				return;
			}
			combat.AddCharacterThatReactedToThisCombat(character2);
			if (poiHit is Character character4)
			{
				if (combat.currentClosestHostile != character4)
				{
					if (!character4.combatComponent.isInCombat)
					{
						CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, character2, character, REACTION_STATUS.WITNESSED);
					}
				}
				else
				{
					CombatData combatData = character.combatComponent.GetCombatData(character4);
					if (combatData != null && combatData.connectedAction != null && combatData.connectedAction.associatedJobType.IsApprehendTypeJob())
					{
						CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, character2, character, REACTION_STATUS.WITNESSED);
					}
					else if (character4 == character2)
					{
						if (character4.relationshipContainer.IsFriendsWith(character))
						{
							CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, character2, character, REACTION_STATUS.WITNESSED);
						}
						else if (character4.relationshipContainer.IsEnemiesWith(character))
						{
							CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, character2, character, REACTION_STATUS.WITNESSED);
						}
						else
						{
							CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, character2, character, REACTION_STATUS.WITNESSED);
						}
					}
					else if (character2.relationshipContainer.IsFriendsWith(character4))
					{
						if (character2.relationshipContainer.IsFriendsWith(character))
						{
							CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, character2, character, REACTION_STATUS.WITNESSED);
							CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, character2, character, REACTION_STATUS.WITNESSED);
						}
						else if (character2.relationshipContainer.IsEnemiesWith(character))
						{
							CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, character2, character, REACTION_STATUS.WITNESSED);
						}
						else
						{
							CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, character2, character, REACTION_STATUS.WITNESSED);
						}
					}
					else if (character2.relationshipContainer.IsEnemiesWith(character4))
					{
						if (character2.relationshipContainer.IsFriendsWith(character))
						{
							CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, character2, character, REACTION_STATUS.WITNESSED);
						}
						else if (character2.relationshipContainer.IsEnemiesWith(character))
						{
							CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, character2, character, REACTION_STATUS.WITNESSED);
						}
						else
						{
							CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, character2, character, REACTION_STATUS.WITNESSED);
						}
					}
					else
					{
						CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, character2, character, REACTION_STATUS.WITNESSED);
					}
				}
				if ((character2.faction == null || character2.faction != character.faction) && (character2.homeSettlement == null || character2.homeSettlement != character.homeSettlement))
				{
					return;
				}
				CombatData combatData2 = character.combatComponent.GetCombatData(character4);
				if (combatData2 == null || combatData2.attackBecauseOfCrime)
				{
					return;
				}
				if (combatData2.connectedAction != null)
				{
					ActualGoapNode connectedAction = combatData2.connectedAction;
					CRIME_TYPE crimeType = connectedAction.crimeType;
					if (crimeType != CRIME_TYPE.None && crimeType != CRIME_TYPE.Unset)
					{
						connectedAction.IncreaseReactionCounter();
						CrimeManager.Instance.ReactToCrime(character2, character, character4, character4.faction, crimeType, connectedAction, REACTION_STATUS.WITNESSED);
						connectedAction.DecreaseReactionCounter();
					}
				}
				else
				{
					ActualGoapNode actualGoapNode = InteractionManager.Instance.CreateNewIllusionAction(character, character4, INTERACTION_TYPE.ASSAULT, shouldLog: false);
					actualGoapNode.IncreaseReactionCounter();
					CrimeManager.Instance.ReactToCrime(character2, character, character4, character4.faction, actualGoapNode.crimeType, actualGoapNode, REACTION_STATUS.WITNESSED);
					actualGoapNode.DecreaseReactionCounter();
				}
			}
			else
			{
				if (!(poiHit is TileObject tileObject) || tileObject.IsOwnedBy(character))
				{
					return;
				}
				CombatData combatData3 = character.combatComponent.GetCombatData(tileObject);
				if (combatData3 == null || !(combatData3.connectedAction != null))
				{
					return;
				}
				ActualGoapNode connectedAction2 = combatData3.connectedAction;
				CRIME_TYPE crimeType2 = connectedAction2.crimeType;
				if (crimeType2 == CRIME_TYPE.None || crimeType2 == CRIME_TYPE.Unset)
				{
					return;
				}
				Faction faction = tileObject.factionOwner;
				if (faction == null)
				{
					BaseSettlement settlement = null;
					if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.IsPartOfSettlement(out settlement))
					{
						faction = settlement.owner;
					}
				}
				if (faction == null)
				{
					faction = character2.faction;
				}
				connectedAction2.IncreaseReactionCounter();
				CrimeManager.Instance.ReactToCrime(character2, character, tileObject, faction, crimeType2, connectedAction2, REACTION_STATUS.WITNESSED);
				connectedAction2.DecreaseReactionCounter();
			}
		}
	}

	public string ReactToIntel(IIntel intel)
	{
		string text = base.owner.reactionComponent.ReactToIntel(intel.reactable);
		if ((string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text)) && intel.actor != base.owner)
		{
			ActualGoapNode action = null;
			if (intel is ActionIntel actionIntel)
			{
				action = actionIntel.node;
			}
			text = CharacterManager.Instance.TriggerEmotion(EMOTION.Disinterest, base.owner, intel.actor, REACTION_STATUS.INFORMED, action);
		}
		return text;
	}

	private void HostileReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		if (actor.currentJob != null && actor.currentActionNode != null && actor.currentActionNode.avoidCombat && actor.currentActionNode.actionStatus == ACTION_STATUS.STARTED && !targetCharacter.isDead && targetCharacter.limiterComponent.canPerform && targetCharacter.combatComponent.combatMode != COMBAT_MODE.Passive)
		{
			actor.currentJob.CancelJob();
			actor.combatComponent.Flight(targetCharacter, "Encountered_Hostile");
			return;
		}
		LocationGridTile gridTileLocation = targetCharacter.gridTileLocation;
		bool flag = disguisedActor.isNormalCharacter && !disguisedActor.traitContainer.HasTrait("Enslaved") && targetCharacter.traitContainer.HasTrait("Enslaved") && disguisedActor.relationshipContainer.HasRelationshipWith(disguisedTarget) && !disguisedActor.relationshipContainer.IsEnemiesWith(disguisedTarget) && !targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Enslaved").IsResponsibleForTrait(disguisedActor) && disguisedActor.faction != targetCharacter.faction;
		bool flag2 = actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.currentParty.currentQuest is IRescuePartyQuest rescuePartyQuest && rescuePartyQuest.targetCharacter == targetCharacter && targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared", "Enslaved");
		bool num = disguisedActor.traitContainer.HasTrait("Necromancer") && targetCharacter.race == RACE.SKELETON && targetCharacter.faction == disguisedActor.prevFaction && disguisedActor.prevFaction != null;
		bool flag3 = actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.currentParty.currentQuest is RaidPartyQuest;
		bool isTargetInVillageOfOtherNonHostileFaction = false;
		bool isTargetInVillageOfSameFaction = false;
		if (gridTileLocation != null && gridTileLocation.structure.structureType != STRUCTURE_TYPE.WILDERNESS && gridTileLocation.IsPartOfSettlement(out var settlement) && settlement.owner != null && disguisedActor.faction != null && !disguisedActor.faction.IsHostileWith(settlement.owner))
		{
			if (settlement.owner == disguisedActor.faction)
			{
				isTargetInVillageOfSameFaction = true;
			}
			else
			{
				isTargetInVillageOfOtherNonHostileFaction = true;
			}
		}
		Prisoner traitOrStatus = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
		if (!num && !targetCharacter.isDead)
		{
			if (flag || flag2)
			{
				actor.jobComponent.TriggerReleaseJob(targetCharacter);
			}
			else if (disguisedActor is Troll && disguisedTarget.isNormalCharacter && disguisedActor.homeStructure != null)
			{
				TrollHostileReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
			}
			else if (disguisedActor.traitContainer.HasTrait("Demon Cultist") && (disguisedTarget.faction.isPlayerFaction || disguisedTarget.traitContainer.HasTrait("Demon Cultist")))
			{
				CultistHostileReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
			}
			else if (traitOrStatus != null && traitOrStatus.IsConsideredPrisonerOf(actor))
			{
				CombatPrisonerHostileReactionToCharacter(actor, targetCharacter, actor, disguisedTarget, traitOrStatus, ref debugLog);
			}
			else if (actor.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger) || flag3)
			{
				WarmongerKidnapCharacter(actor, targetCharacter, isTargetInVillageOfSameFaction, isTargetInVillageOfOtherNonHostileFaction, ref debugLog);
			}
			else if ((disguisedTarget.combatComponent.combatMode != COMBAT_MODE.Passive || targetCharacter.race == RACE.HARPY) && !targetCharacter.traitContainer.HasTrait("Hibernating") && disguisedActor.combatComponent.combatMode == COMBAT_MODE.Aggressive)
			{
				CombatHostileReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, isTargetInVillageOfOtherNonHostileFaction, traitOrStatus, ref debugLog);
			}
		}
	}

	private void TrollHostileReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		if (targetCharacter.currentStructure != disguisedActor.homeStructure)
		{
			if (!targetCharacter.limiterComponent.canPerform || !targetCharacter.limiterComponent.canMove)
			{
				if (!actor.combatComponent.isInCombat && !actor.jobQueue.HasJob(JOB_TYPE.CAPTURE_CHARACTER))
				{
					actor.jobComponent.TryTriggerCaptureCharacter(targetCharacter, disguisedActor.homeStructure, doNotRecalculate: true);
				}
			}
			else
			{
				if (actor.combatComponent.IsUnkillable(targetCharacter))
				{
					return;
				}
				CombatReaction fightOrFlightReaction = actor.combatComponent.GetFightOrFlightReaction(targetCharacter, "Hostility");
				if (fightOrFlightReaction.reaction == COMBAT_REACTION.None)
				{
					return;
				}
				if (fightOrFlightReaction.reaction == COMBAT_REACTION.Flight)
				{
					if (!targetCharacter.traitContainer.HasTrait("Restrained", "Resting"))
					{
						actor.combatComponent.FightOrFlight(targetCharacter, fightOrFlightReaction, null, isLethal: false);
					}
				}
				else
				{
					actor.combatComponent.FightOrFlight(targetCharacter, fightOrFlightReaction, null, isLethal: false);
				}
			}
		}
		else if (!targetCharacter.traitContainer.HasTrait("Restrained"))
		{
			actor.jobComponent.TriggerRestrainJob(targetCharacter, JOB_TYPE.CAPTURE_CHARACTER);
		}
	}

	private void CultistHostileReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		int num = Random.Range(0, 100);
		int num2 = 30;
		if (num < num2)
		{
			actor.interruptComponent.TriggerInterrupt(INTERRUPT.Inspired, targetCharacter);
		}
		else
		{
			actor.jobComponent.TriggerPray();
		}
	}

	private void CombatHostileReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, bool isTargetInVillageOfOtherNonHostileFaction, Prisoner targetPrisonerStatus, ref string debugLog)
	{
		if (targetCharacter.defaultCharacterTrait.hasBeenAbductedByWildMonster)
		{
			Faction faction = disguisedActor.faction;
			if (faction != null && faction.factionType.type == FACTION_TYPE.Wild_Monsters)
			{
				return;
			}
		}
		if (targetCharacter.defaultCharacterTrait.hasBeenAbductedByPlayerMonster && disguisedActor.faction.isPlayerFaction)
		{
			return;
		}
		if (disguisedActor is Kobold kobold)
		{
			if (!ShouldKoboldCombatReactToCharacter(kobold, targetCharacter, ref debugLog))
			{
				return;
			}
		}
		else if (disguisedActor is Centaur centaur && !ShouldCentaurCombatReactToCharacter(centaur, targetCharacter, ref debugLog))
		{
			return;
		}
		if (disguisedActor.faction.isPlayerFaction && targetCharacter.traitContainer.HasTrait("Dazed"))
		{
			return;
		}
		if (targetCharacter.traitContainer.HasTrait("Restrained"))
		{
			bool flag = false;
			if (targetPrisonerStatus != null)
			{
				if (targetPrisonerStatus.IsConsideredPrisonerOf(disguisedActor))
				{
					flag = true;
				}
				else
				{
					Character prisonerOfCharacter = targetPrisonerStatus.prisonerOfCharacter;
					if (prisonerOfCharacter != null && prisonerOfCharacter.faction == disguisedActor.faction && prisonerOfCharacter.faction != null)
					{
						flag = true;
					}
				}
			}
			if (!(isTargetInVillageOfOtherNonHostileFaction || flag) && actor.isNormalCharacter)
			{
				actor.jobComponent.TriggerFactionKidnapAndRestrainJob(targetCharacter);
			}
		}
		else
		{
			HostileFightOrFlightReaction(actor, targetCharacter, ref debugLog);
		}
	}

	private bool IsCarryingPOIForImportantJob(Character p_actor)
	{
		if (p_actor.carryComponent.isCarryingAnyPOI && p_actor.currentActionNode != null && p_actor.currentActionNode.associatedJobType.ShouldIgnoreHostilesIfCarryingPOI())
		{
			return true;
		}
		return false;
	}

	private void HostileFightOrFlightReaction(Character actor, Character targetCharacter, ref string debugLog)
	{
		bool flag = actor.combatComponent.ShouldCombatBeLethalAgainst(targetCharacter);
		if (!(!targetCharacter.traitContainer.HasTrait("Unconscious", "Restrained") || flag) || IsCarryingPOIForImportantJob(actor) || actor.combatComponent.IsUnkillable(targetCharacter))
		{
			return;
		}
		CombatReaction fightOrFlightReaction = actor.combatComponent.GetFightOrFlightReaction(targetCharacter, "Hostility");
		if (fightOrFlightReaction.reaction == COMBAT_REACTION.None)
		{
			return;
		}
		if (actor.traitContainer.HasTrait("Patrolling") && actor.homeSettlement != null && actor.IsInHomeSettlement() && targetCharacter.currentSettlement == actor.homeSettlement && actor.homeSettlement.locationType == LOCATION_TYPE.VILLAGE)
		{
			TIME_IN_WORDS currentTimeInWordsOfTick = GameManager.Instance.GetCurrentTimeInWordsOfTick();
			if (currentTimeInWordsOfTick == TIME_IN_WORDS.LATE_NIGHT || currentTimeInWordsOfTick == TIME_IN_WORDS.AFTER_MIDNIGHT)
			{
				actor.homeSettlement.AlertSleepingCombatantResidents(3);
			}
		}
		if (fightOrFlightReaction.reaction == COMBAT_REACTION.Flight)
		{
			if (!targetCharacter.traitContainer.HasTrait("Restrained", "Resting"))
			{
				actor.combatComponent.FightOrFlight(targetCharacter, fightOrFlightReaction, null, flag);
			}
		}
		else
		{
			actor.combatComponent.FightOrFlight(targetCharacter, fightOrFlightReaction, null, flag);
		}
	}

	private void CombatPrisonerHostileReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, Prisoner targetPrisonerStatus, ref string debugLog)
	{
		LocationStructure intendedPrisonAccordingTo = targetPrisonerStatus.GetIntendedPrisonAccordingTo(disguisedActor);
		if (targetCharacter.currentStructure == intendedPrisonAccordingTo)
		{
			if (!targetCharacter.needsComponent.isStarving)
			{
				return;
			}
			Vampire traitOrStatus = disguisedTarget.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
			if (traitOrStatus != null && traitOrStatus.DoesCharacterKnowThisVampire(disguisedActor))
			{
				if ((disguisedActor.traitContainer.HasTrait("Hemophiliac") || (disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget) == "Close Friend" && !disguisedActor.traitContainer.HasTrait("Hemophobic"))) && !targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.FEED, JOB_TYPE.OFFER_BLOOD))
				{
					actor.jobComponent.CreateFeedSelfToVampireJob(targetCharacter);
				}
			}
			else if (targetCharacter.race.IsSapient() && !targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.FEED) && (actor.currentJob == null || actor.currentJob.poiTarget != targetCharacter))
			{
				actor.jobComponent.TryTriggerFeed(targetCharacter);
			}
		}
		else
		{
			bool p_canApprehend = false;
			actor.jobComponent.TryCreateApprehend(targetCharacter, actor.homeSettlement, ref p_canApprehend, intendedPrisonAccordingTo);
		}
	}

	private bool ShouldKoboldCombatReactToCharacter(Kobold kobold, Character targetCharacter, ref string debugLog)
	{
		if (targetCharacter.HasJobTargetingThis(JOB_TYPE.CAPTURE_CHARACTER, RACE.KOBOLD))
		{
			return false;
		}
		return true;
	}

	private bool ShouldCentaurCombatReactToCharacter(Centaur centaur, Character targetCharacter, ref string debugLog)
	{
		if (targetCharacter.HasJobTargetingThis(JOB_TYPE.CAPTURE_CHARACTER, RACE.CENTAUR))
		{
			return false;
		}
		return true;
	}

	private void NonHostileNotInCombatReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		if (StalkerReactionToNonHostileVillager(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog))
		{
			return;
		}
		if (disguisedActor.isNormalCharacter)
		{
			if (!targetCharacter.isDead)
			{
				NonHostileAliveVillagerReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
			}
			else
			{
				Dead traitOrStatus = targetCharacter.traitContainer.GetTraitOrStatus<Dead>("Dead");
				if (!targetCharacter.reactionComponent.charactersThatSawThisDead.Contains(disguisedActor))
				{
					targetCharacter.reactionComponent.AddCharacterThatSawThisDead(disguisedActor);
					if (disguisedActor.traitContainer.HasTrait("Psychopath"))
					{
						if (targetCharacter.isNormalCharacter && (traitOrStatus == null || !traitOrStatus.IsResponsibleForTrait(actor)))
						{
							if (Random.Range(0, 2) == 0)
							{
								actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
							}
							else
							{
								actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
							}
						}
					}
					else
					{
						string opinionLabel = disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget);
						if (opinionLabel == "Friend" || opinionLabel == "Close Friend")
						{
							if (actor.traitContainer.HasTrait("Vampire") && targetCharacter.grave == null && GameUtilities.RollChance(15))
							{
								actor.jobComponent.CreateVampiricEmbraceJob(JOB_TYPE.VAMPIRIC_EMBRACE, targetCharacter);
								actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, disguisedTarget, "", null, "Saw_Dead");
							}
							else if (traitOrStatus == null || !traitOrStatus.IsResponsibleForTrait(actor))
							{
								if (Random.Range(0, 2) == 0)
								{
									actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, disguisedTarget, "", null, "Saw_Dead");
								}
								else
								{
									actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, disguisedTarget, "", null, "Saw_Dead");
								}
							}
						}
						else if (disguisedActor.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(disguisedTarget))
						{
							if (traitOrStatus == null || !traitOrStatus.IsResponsibleForTrait(actor))
							{
								if (Random.Range(0, 2) == 0)
								{
									actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, "", null, "Saw_Dead");
								}
								else
								{
									actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, targetCharacter, "", null, "Saw_Dead");
								}
							}
						}
						else if (opinionLabel == "Enemy")
						{
							if (traitOrStatus == null || !traitOrStatus.IsResponsibleForTrait(actor))
							{
								if (Random.Range(0, 100) < 25)
								{
									if (Random.Range(0, 2) == 0)
									{
										actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
									}
									else
									{
										actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
									}
								}
								else
								{
									actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, targetCharacter, "", null, "Shocked_Witness_Reason");
								}
							}
						}
						else if (opinionLabel == "Rival")
						{
							if (traitOrStatus == null || !traitOrStatus.IsResponsibleForTrait(actor))
							{
								if (Random.Range(0, 2) == 0)
								{
									actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
								}
								else
								{
									actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
								}
							}
						}
						else if (targetCharacter.isNormalCharacter && actor.relationshipContainer.HasRelationshipWith(targetCharacter) && (traitOrStatus == null || !traitOrStatus.IsResponsibleForTrait(actor)))
						{
							actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, targetCharacter, "", null, "Shocked_Witness_Reason");
						}
					}
					if ((bool)actor.marker && disguisedTarget.isNormalCharacter && traitOrStatus != null && traitOrStatus.gainedFromDoingType != INTERACTION_TYPE.EXECUTE && traitOrStatus.gainedFromDoingType != INTERACTION_TYPE.BURN_AT_STAKE && !disguisedTarget.crimeComponent.IsWantedBy(actor.faction))
					{
						LocationStructure structure = disguisedTarget.gridTileLocation.structure;
						if ((disguisedTarget.gridTileLocation == null || !structure.structureType.IsOpenSpace() || actor.relationshipContainer.HasRelationshipWith(disguisedTarget)) && structure is Dwelling)
						{
							assumptionSuspects.Clear();
							if (structure.residents.Contains(disguisedTarget) || disguisedTarget.previousCharacterDataComponent.previousHomeStructure == structure)
							{
								for (int i = 0; i < actor.marker.inVisionCharacters.Count; i++)
								{
									Character character = actor.marker.inVisionCharacters[i];
									if (IsCharacterValidMurderAssumptionSuspect(character))
									{
										assumptionSuspects.Add(character);
									}
								}
							}
							else if (structure.residents.Count > 0)
							{
								for (int j = 0; j < structure.residents.Count; j++)
								{
									Character character2 = structure.residents[j];
									if (character2 != actor && IsCharacterValidMurderAssumptionSuspect(character2))
									{
										assumptionSuspects.Add(character2);
									}
								}
							}
							if (assumptionSuspects.Count > 0)
							{
								Character character3 = assumptionSuspects[Random.Range(0, assumptionSuspects.Count)];
								actor.assumptionComponent.CreateAndReactToNewAssumption(character3, disguisedTarget, INTERACTION_TYPE.MURDER, REACTION_STATUS.WITNESSED, !traitOrStatus.IsResponsibleForTrait(character3));
							}
						}
					}
					if (disguisedTarget.traitContainer.HasTrait("Mangled") && disguisedActor.homeSettlement != null && disguisedActor.homeSettlement.eventManager.CanHaveEvents() && CrimeManager.Instance.GetCrimeSeverity(disguisedActor, disguisedTarget, disguisedTarget, CRIME_TYPE.Werewolf).IsConsideredACrime() && !disguisedActor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Werewolf_Hunt) && disguisedTarget.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(disguisedActor.homeSettlement) && ChanceData.RollChance(CHANCE_TYPE.Werewolf_Hunt_Mangled, ref debugLog))
					{
						disguisedActor.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Werewolf_Hunt);
					}
				}
			}
			if (!targetCharacter.marker || !targetCharacter.isNormalCharacter || !targetCharacter.carryComponent.isCarryingAnyPOI || !(targetCharacter.carryComponent.carriedPOI is Character character4))
			{
				return;
			}
			bool flag = true;
			if (actor.faction != null && character4.faction != null && actor.faction.IsHostileWith(character4.faction))
			{
				flag = false;
			}
			if (!flag)
			{
				return;
			}
			if (character4.traitContainer.HasTrait("Restrained", "Unconscious") && !character4.isDead && !character4.crimeComponent.IsWantedBy(actor.faction) && (targetCharacter.currentJob == null || targetCharacter.currentJob.jobType != JOB_TYPE.BLOOD_SACRIFICE))
			{
				bool flag2 = character4.traitContainer.IsResponsibleForTrait("Restrained", targetCharacter) || character4.traitContainer.IsResponsibleForTrait("Unconscious", targetCharacter);
				bool flag3 = true;
				if (character4.traitContainer.HasTrait("Prisoner") && character4.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner").IsConsideredPrisonerOf(actor))
				{
					flag3 = false;
				}
				if (flag3)
				{
					actor.assumptionComponent.CreateAndReactToNewAssumption(targetCharacter, character4, INTERACTION_TYPE.ASSAULT, REACTION_STATUS.WITNESSED, !flag2);
				}
			}
			else if (targetCharacter.currentJob != null && targetCharacter.currentJob.jobType == JOB_TYPE.BURY_SERIAL_KILLER_VICTIM)
			{
				actor.assumptionComponent.CreateAndReactToNewAssumption(targetCharacter, character4, INTERACTION_TYPE.MURDER, REACTION_STATUS.WITNESSED, isFabricated: false);
			}
		}
		else
		{
			if (!(actor is Summon summon))
			{
				return;
			}
			CHARACTER_CATEGORY category = RaceManager.Instance.GetRaceData(summon.race).category;
			Faction faction = disguisedActor.faction;
			if ((category != CHARACTER_CATEGORY.Humanoid && category != CHARACTER_CATEGORY.Demonic) || faction == null || faction != disguisedTarget.faction)
			{
				return;
			}
			bool flag4 = true;
			if ((faction.factionType.type == FACTION_TYPE.Wild_Monsters || faction.factionType.type == FACTION_TYPE.Undead) && actor.race != targetCharacter.race)
			{
				flag4 = false;
			}
			if (!flag4)
			{
				return;
			}
			Prisoner traitOrStatus2 = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
			if (traitOrStatus2 != null && traitOrStatus2.IsConsideredPrisonerOf(disguisedActor))
			{
				return;
			}
			bool flag5 = targetCharacter.traitContainer.HasTrait("Restrained");
			bool flag6 = targetCharacter.traitContainer.HasTrait("Ensnared");
			bool flag7 = targetCharacter.traitContainer.HasTrait("Frozen");
			bool flag8 = targetCharacter.traitContainer.HasTrait("Unconscious");
			if (!(flag5 || flag6 || flag7 || flag8) || disguisedTarget.crimeComponent.IsWantedBy(disguisedActor.faction))
			{
				return;
			}
			bool flag9 = flag5 && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Restrained").IsResponsibleForTrait(disguisedActor);
			bool flag10 = flag6 && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Ensnared").IsResponsibleForTrait(disguisedActor);
			bool flag11 = flag7 && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Frozen").IsResponsibleForTrait(disguisedActor);
			bool flag12 = flag8 && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Unconscious").IsResponsibleForTrait(disguisedActor);
			bool flag13 = !targetCharacter.HasJobTargetingThis(JOB_TYPE.REMOVE_STATUS);
			if (!flag13)
			{
				flag13 = !actor.HasCharacterInVisionWithJobAndTarget(JOB_TYPE.REMOVE_STATUS, targetCharacter, targetCharacter);
			}
			if (flag13)
			{
				if (flag5 && !flag9 && !targetCharacter.HasJobTargetingThis(JOB_TYPE.BLOOD_SACRIFICE))
				{
					actor.jobComponent.TriggerRemoveStatusTargetForMonster(targetCharacter, "Restrained");
				}
				if (flag6 && !flag10)
				{
					actor.jobComponent.TriggerRemoveStatusTargetForMonster(targetCharacter, "Ensnared");
				}
				if (flag7 && !flag11)
				{
					actor.jobComponent.TriggerRemoveStatusTargetForMonster(targetCharacter, "Frozen");
				}
				if (flag8 && !flag12 && !targetCharacter.HasJobTargetingThis(JOB_TYPE.BLOOD_SACRIFICE) && !targetCharacter.traitContainer.HasTrait("Berserked"))
				{
					actor.jobComponent.TriggerRemoveStatusTargetForMonster(targetCharacter, "Unconscious");
				}
			}
		}
		bool IsCharacterValidMurderAssumptionSuspect(Character character5)
		{
			if (character5 != targetCharacter && !character5.isDead && character5.relationshipContainer.IsEnemiesWith(disguisedTarget) && (character5.currentJob == null || character5.currentJob.jobType != JOB_TYPE.BURY))
			{
				return true;
			}
			return false;
		}
	}

	private void NonHostileAliveVillagerReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		NonHostileAliveVillagerChatReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
		NonHostileAliveVillagerWantedCriminalReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
		NonHostileAliveVillagerAccusedCriminalReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
		if (disguisedActor.faction != null && disguisedActor.homeSettlement != null && (disguisedActor.faction == disguisedTarget.faction || disguisedActor.homeSettlement == disguisedTarget.homeSettlement))
		{
			NonHostileAliveVillagerHomeOrFactionmateReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
		}
		else
		{
			NonHostileAliveVillagerNotHomeOrFactionmateReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
		}
		if (disguisedTarget.race == RACE.WOLF && disguisedTarget.traitContainer.HasTrait("Restrained") && disguisedActor.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves))
		{
			actor.jobComponent.TriggerReleaseJob(targetCharacter);
		}
		NonHostileAliveVillagerNotHomeOrFactionmateVampireReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
		if (disguisedTarget.traitContainer.HasTrait("Berserked") && disguisedTarget.limiterComponent.canPerform)
		{
			if (disguisedActor.characterClass.IsCombatant() && !disguisedActor.traitContainer.HasTrait("Coward"))
			{
				actor.jobComponent.TriggerKnockoutJob(targetCharacter);
			}
			else if (disguisedTarget.defaultCharacterTrait.HasReactedToThis(actor))
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, targetCharacter);
			}
		}
	}

	private bool StalkerReactionToNonHostileVillager(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		Prisoner traitOrStatus = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
		bool flag = false;
		if (traitOrStatus != null && traitOrStatus.isFactionPrisoner)
		{
			Faction prisonerOfFaction = traitOrStatus.prisonerOfFaction;
			if (targetCharacter.currentSettlement != null && targetCharacter.currentSettlement.owner == prisonerOfFaction)
			{
				flag = targetCharacter.currentSettlement.owner == prisonerOfFaction;
			}
		}
		if (disguisedActor.characterClass.className == "Stalker" && !targetCharacter.isDead && !flag)
		{
			bool flag2 = true;
			string opinionLabel = disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget);
			if (opinionLabel == "Close Friend")
			{
				flag2 = false;
			}
			else if (opinionLabel == "Friend")
			{
				flag2 = false;
			}
			if (flag2)
			{
				bool isTargetVampire = false;
				bool isTargetLycan = false;
				bool isTargetCultist = false;
				DoesStalkerConsidersCharacterAVampireLycanCultist(disguisedActor, disguisedTarget, ref isTargetVampire, ref isTargetLycan, ref isTargetCultist, ref debugLog);
				INTERACTION_TYPE iNTERACTION_TYPE = INTERACTION_TYPE.NONE;
				if (isTargetVampire)
				{
					iNTERACTION_TYPE = INTERACTION_TYPE.IS_VAMPIRE;
				}
				else if (isTargetLycan)
				{
					iNTERACTION_TYPE = INTERACTION_TYPE.IS_WEREWOLF;
				}
				else if (isTargetCultist)
				{
					iNTERACTION_TYPE = INTERACTION_TYPE.IS_CULTIST;
				}
				if (iNTERACTION_TYPE != INTERACTION_TYPE.NONE)
				{
					if (actor.TryGetTalentLevel(CHARACTER_TALENT.Martial_Arts) < 5)
					{
						if (!actor.assumptionComponent.HasAlreadyAssumedTo(iNTERACTION_TYPE, disguisedTarget, disguisedTarget))
						{
							actor.assumptionComponent.CreateAndReactToNewAssumption(disguisedTarget, disguisedTarget, iNTERACTION_TYPE, REACTION_STATUS.WITNESSED, isFabricated: false);
						}
						else
						{
							actor.classComponent.StalkerHunt(targetCharacter);
						}
						return true;
					}
					if (!actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.PURIFY))
					{
						return actor.jobComponent.TryCreateStalkerPurifyJob(targetCharacter);
					}
				}
			}
			else
			{
				bool isTargetVampire2 = false;
				bool isTargetLycan2 = false;
				bool isTargetCultist2 = false;
				DoesStalkerConsidersCharacterAVampireLycanCultist(disguisedActor, disguisedTarget, ref isTargetVampire2, ref isTargetLycan2, ref isTargetCultist2, ref debugLog);
				if ((isTargetVampire2 || isTargetLycan2 || isTargetCultist2) && actor.TryGetTalentLevel(CHARACTER_TALENT.Martial_Arts) >= 5 && !actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.PURIFY))
				{
					return actor.jobComponent.TryCreateStalkerPurifyJob(targetCharacter);
				}
			}
		}
		return false;
	}

	private void DoesStalkerConsidersCharacterAVampireLycanCultist(Character p_actor, Character p_target, ref bool isTargetVampire, ref bool isTargetLycan, ref bool isTargetCultist, ref string debugLog)
	{
		if (p_actor.classComponent.IsTargetRecognizedAsVampireByStalker(p_target))
		{
			if (CrimeManager.Instance.IsConsideredACrimeByCharacter(p_actor, p_target, p_target, CRIME_TYPE.Vampire))
			{
				isTargetVampire = true;
			}
		}
		else if (p_actor.classComponent.IsTargetRecognizedAsLycanByStalker(p_target))
		{
			if (CrimeManager.Instance.IsConsideredACrimeByCharacter(p_actor, p_target, p_target, CRIME_TYPE.Werewolf))
			{
				isTargetLycan = true;
			}
		}
		else if (p_actor.classComponent.IsTargetRecognizedAsCultistByStalker(p_target))
		{
			debugLog += "\n-Target is a Cultist";
			CRIME_TYPE crimeTypeByReligion = p_target.religionComponent.religion.GetCrimeTypeByReligion();
			if (CrimeManager.Instance.IsConsideredACrimeByCharacter(p_actor, p_target, p_target, crimeTypeByReligion))
			{
				isTargetCultist = true;
			}
		}
	}

	private void NonHostileAliveVillagerChatReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		if (!actor.isConversing && !targetCharacter.isConversing && (actor.currentActionNode == null || actor.currentActionNode.hasBeenReset || actor.currentActionNode.action.goapType != INTERACTION_TYPE.HAVE_AFFAIR || actor.currentActionNode.poiTarget != targetCharacter))
		{
			int num = Random.Range(0, 100);
			if (actor.nonActionEventsComponent.CanChat(targetCharacter) && num < 3)
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, targetCharacter);
			}
			else if (actor.behaviourComponent.canFlirtOnActivePartyQuest || !actor.partyComponent.isMemberThatJoinedQuest)
			{
				actor.nonActionEventsComponent.CheckForFlirtTrigger(disguisedActor, disguisedTarget, isOnSight: true, ref debugLog, out var _);
			}
		}
	}

	private void NonHostileAliveVillagerWantedCriminalReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		Prisoner traitOrStatus = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
		if (!disguisedTarget.isNormalCharacter || !disguisedActor.isNormalCharacter || disguisedActor.faction == null || !disguisedTarget.crimeComponent.IsWantedBy(disguisedActor.faction) || (targetCharacter.traitContainer.HasTrait("Restrained") && (traitOrStatus == null || !traitOrStatus.IsConsideredPrisonerOf(disguisedActor) || traitOrStatus.IsInIntendedPrisonAccordingTo(disguisedActor))))
		{
			return;
		}
		bool flag = false;
		if (actor.currentJob != null && actor.currentJob is GoapPlanJob goapPlanJob)
		{
			flag = goapPlanJob.jobType.IsApprehendTypeJob() && goapPlanJob.targetPOI == targetCharacter;
		}
		if (flag)
		{
			return;
		}
		string opinionLabel = disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget);
		if (opinionLabel == "Friend" || opinionLabel == "Close Friend" || disguisedActor.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(disguisedTarget))
		{
			Criminal traitOrStatus2 = disguisedTarget.traitContainer.GetTraitOrStatus<Criminal>("Criminal");
			if (traitOrStatus2 != null && !traitOrStatus2.HasCharacterThatIsAlreadyWorried(disguisedActor) && !disguisedTarget.defaultCharacterTrait.HasReactedToThis(disguisedActor))
			{
				traitOrStatus2.AddCharacterThatIsAlreadyWorried(disguisedActor);
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, targetCharacter);
			}
			return;
		}
		bool p_canApprehend = false;
		actor.jobComponent.TryCreateApprehend(targetCharacter, actor.homeSettlement, ref p_canApprehend);
		if (!p_canApprehend)
		{
			if (!disguisedTarget.defaultCharacterTrait.HasReactedToThis(disguisedActor))
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, targetCharacter);
			}
			BaseSettlement homeSettlement = actor.homeSettlement;
			if (homeSettlement != null && homeSettlement.locationType == LOCATION_TYPE.VILLAGE && homeSettlement is NPCSettlement nPCSettlement)
			{
				nPCSettlement.settlementJobTriggerComponent.TryCreateApprehend(targetCharacter);
			}
		}
	}

	private void NonHostileAliveVillagerAccusedCriminalReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		if (!disguisedTarget.isNormalCharacter || !disguisedActor.isNormalCharacter || disguisedActor.faction == null || disguisedTarget.crimeComponent.IsWantedBy(disguisedActor.faction) || !disguisedTarget.crimeComponent.IsAnActiveCrimeWitnessedBy(disguisedActor))
		{
			return;
		}
		string opinionLabel = disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget);
		if (opinionLabel == "Friend" || opinionLabel == "Close Friend" || disguisedActor.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(disguisedTarget))
		{
			Criminal traitOrStatus = disguisedTarget.traitContainer.GetTraitOrStatus<Criminal>("Criminal");
			if (traitOrStatus != null && !traitOrStatus.HasCharacterThatIsAlreadyWorried(disguisedActor) && !disguisedTarget.defaultCharacterTrait.HasReactedToThis(disguisedActor))
			{
				traitOrStatus.AddCharacterThatIsAlreadyWorried(disguisedActor);
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, targetCharacter);
			}
			return;
		}
		if (!disguisedTarget.defaultCharacterTrait.HasReactedToThis(disguisedActor))
		{
			bool flag = false;
			for (int i = 0; i < disguisedTarget.crimeComponent.activeCrimes.Count; i++)
			{
				CrimeData crimeData = disguisedTarget.crimeComponent.activeCrimes[i];
				if (CrimeManager.Instance.IsConsideredACrimeByCharacter(actor, targetCharacter, crimeData.target, crimeData.crimeType))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, targetCharacter);
			}
		}
		if (GameUtilities.RollChance(30) && disguisedActor.crimeComponent.HasUnreportedCrimeOf(disguisedTarget))
		{
			bool p_canApprehend = false;
			actor.jobComponent.TryCreateApprehend(targetCharacter, actor.homeSettlement, ref p_canApprehend);
		}
	}

	private void NonHostileAliveVillagerNotHomeOrFactionmateVampireReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		if (targetCharacter.limiterComponent.canPerform && !targetCharacter.partyComponent.isMemberThatJoinedQuest && !disguisedTarget.crimeComponent.IsCrimeAlreadyWitnessedBy(disguisedActor, CRIME_TYPE.Vampire) && GameManager.Instance.GetCurrentTimeInWordsOfTick() == TIME_IN_WORDS.AFTER_MIDNIGHT && disguisedActor.homeSettlement != null && disguisedActor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Vampire_Hunt) && !disguisedActor.homeSettlement.isUnderSiege && CrimeManager.Instance.GetCrimeSeverity(disguisedActor, disguisedTarget, disguisedTarget, CRIME_TYPE.Vampire).IsConsideredACrime())
		{
			Vampire traitOrStatus = disguisedTarget.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
			if ((traitOrStatus == null || !traitOrStatus.DoesCharacterKnowThisVampire(disguisedActor)) && !targetCharacter.isAtHomeStructure && targetCharacter.IsAtHome())
			{
				int num = 0;
				num = ((disguisedActor.moodComponent.moodState == MOOD_STATE.Critical) ? 100 : ((disguisedActor.moodComponent.moodState != MOOD_STATE.Bad) ? 50 : 75));
				if (GameUtilities.RollChance(num, ref debugLog))
				{
					actor.assumptionComponent.CreateAndReactToNewAssumption(disguisedTarget, disguisedTarget, INTERACTION_TYPE.IS_VAMPIRE, REACTION_STATUS.WITNESSED, traitOrStatus == null);
				}
			}
		}
		if (disguisedTarget.characterClass.className == "Vampire Lord" && !disguisedTarget.traitContainer.GetTraitOrStatus<Vampire>("Vampire").DoesCharacterKnowThisVampire(disguisedActor))
		{
			actor.assumptionComponent.CreateAndReactToNewAssumption(disguisedTarget, disguisedTarget, INTERACTION_TYPE.IS_VAMPIRE, REACTION_STATUS.WITNESSED, isFabricated: false);
		}
	}

	private void NonHostileAliveVillagerNotHomeOrFactionmateReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		bool flag = false;
		Prisoner traitOrStatus = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
		Lazy traitOrStatus2 = actor.traitContainer.GetTraitOrStatus<Lazy>("Lazy");
		if ((traitOrStatus2 == null || !traitOrStatus2.TryIgnoreUrgentTask(JOB_TYPE.RELEASE_CHARACTER)) && traitOrStatus != null && !traitOrStatus.IsConsideredPrisonerOf(disguisedActor) && targetCharacter.traitContainer.HasTrait("Restrained") && !disguisedActor.relationshipContainer.HasGrudgeAgainst(disguisedTarget))
		{
			Faction factionThatImprisoned = traitOrStatus.GetFactionThatImprisoned();
			if (factionThatImprisoned != null && disguisedActor.faction != factionThatImprisoned)
			{
				FactionRelationship relationshipWith = disguisedActor.faction.GetRelationshipWith(factionThatImprisoned);
				if (relationshipWith != null && relationshipWith.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile && !disguisedActor.relationshipContainer.IsEnemiesWith(disguisedTarget))
				{
					flag = ((!factionThatImprisoned.isPlayerFaction || !disguisedActor.traitContainer.HasTrait("Demon Cultist")) ? true : false);
				}
			}
			if (!flag && disguisedActor.relationshipContainer.IsFriendsWith(disguisedTarget))
			{
				flag = true;
			}
			if (flag)
			{
				actor.jobComponent.TriggerReleaseJob(targetCharacter);
			}
		}
		if (flag || !targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared") || targetCharacter.isDead || actor.faction == null || targetCharacter.faction == null || targetCharacter.currentStructure == null)
		{
			return;
		}
		bool flag2 = actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.currentParty.currentQuest is RaidPartyQuest;
		RELIGION p_religion2;
		if (targetCharacter.currentSettlement is NPCSettlement nPCSettlement && targetCharacter.currentStructure == nPCSettlement.prison)
		{
			if (actor.traitContainer.IsReligiousCultist(out var p_religion) && targetCharacter.traitContainer.IsReligiousCultist(p_religion) && !disguisedActor.relationshipContainer.HasGrudgeAgainst(disguisedTarget))
			{
				if (!actor.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(p_religion.GetCrimeTypeByReligion()) && !disguisedActor.relationshipContainer.IsEnemiesWith(disguisedTarget))
				{
					if (!targetCharacter.crimeComponent.IsWantedBy(actor.faction))
					{
						actor.jobComponent.TriggerReleaseJob(targetCharacter);
					}
					else if (p_religion.GetFactionTypeForReligion() != actor.faction.factionType.type)
					{
						actor.jobComponent.TriggerReleaseJob(targetCharacter);
					}
				}
			}
			else if ((actor.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger) || flag2) && !actor.IsTargetOfCurrentAction(targetCharacter) && !targetCharacter.limiterComponent.isTargetedByDemonicSnatch && GameUtilities.RollChance(ChanceData.GetChance(CHANCE_TYPE.Kidnap_Chance)))
			{
				actor.jobComponent.TriggerFactionKidnapAndRestrainJob(targetCharacter);
			}
		}
		else if (actor.traitContainer.IsReligiousCultist(out p_religion2) && !disguisedActor.relationshipContainer.HasGrudgeAgainst(disguisedTarget))
		{
			if (!targetCharacter.traitContainer.IsReligiousCultist(p_religion2))
			{
				if (actor.hasMarker)
				{
					if (actor.marker.CanDoStealthCrimeToTarget(targetCharacter, actor.religionComponent.religion.GetCrimeTypeByReligion()) && targetCharacter.traitContainer.HasTrait("Restrained", "Ensnared"))
					{
						actor.jobComponent.TryCreateLiberateJob(targetCharacter);
					}
					else if (!targetCharacter.crimeComponent.IsWantedBy(actor.faction) && !targetCharacter.traitContainer.HasTrait("Berserked") && !disguisedActor.relationshipContainer.IsEnemiesWith(disguisedTarget) && !disguisedActor.traitContainer.HasTrait("Evil", "Psychopath") && !disguisedActor.relationshipContainer.HasGrudgeAgainst(disguisedTarget))
					{
						actor.jobComponent.TriggerReleaseJob(targetCharacter);
					}
				}
			}
			else if (!disguisedActor.relationshipContainer.IsEnemiesWith(disguisedTarget))
			{
				if (!targetCharacter.crimeComponent.IsWantedBy(actor.faction))
				{
					actor.jobComponent.TriggerReleaseJob(targetCharacter);
				}
				else if (p_religion2.GetFactionTypeForReligion() != actor.faction.factionType.type)
				{
					actor.jobComponent.TriggerReleaseJob(targetCharacter);
				}
			}
		}
		else if (actor.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger) || flag2)
		{
			if (!actor.IsTargetOfCurrentAction(targetCharacter) && !targetCharacter.limiterComponent.isTargetedByDemonicSnatch && GameUtilities.RollChance(ChanceData.GetChance(CHANCE_TYPE.Kidnap_Chance)))
			{
				actor.jobComponent.TriggerFactionKidnapAndRestrainJob(targetCharacter);
			}
		}
		else if (!targetCharacter.crimeComponent.IsWantedBy(actor.faction) && !targetCharacter.traitContainer.HasTrait("Berserked") && !disguisedActor.relationshipContainer.IsEnemiesWith(disguisedTarget) && !disguisedActor.traitContainer.HasTrait("Evil", "Psychopath") && !disguisedActor.relationshipContainer.HasGrudgeAgainst(disguisedTarget))
		{
			actor.jobComponent.TriggerReleaseJob(targetCharacter);
		}
	}

	private void WarmongerKidnapCharacter(Character actor, Character targetCharacter, bool isTargetInVillageOfSameFaction, bool isTargetInVillageOfOtherNonHostileFaction, ref string debugLog)
	{
		if (targetCharacter.limiterComponent.isTargetedByDemonicSnatch || targetCharacter.HasJobTargetingThis(JOB_TYPE.FACTION_KIDNAP, actor.faction))
		{
			return;
		}
		if (targetCharacter.traitContainer.HasTrait("Restrained"))
		{
			if (isTargetInVillageOfSameFaction || isTargetInVillageOfOtherNonHostileFaction)
			{
				return;
			}
			if (targetCharacter.combatComponent.combatMode == COMBAT_MODE.Passive)
			{
				if (GameUtilities.RollChance(5))
				{
					actor.jobComponent.TriggerFactionKidnapAndRestrainJob(targetCharacter);
				}
			}
			else if (GameUtilities.RollChance(ChanceData.GetChance(CHANCE_TYPE.Kidnap_Chance)))
			{
				actor.jobComponent.TriggerFactionKidnapAndRestrainJob(targetCharacter);
			}
			else if (targetCharacter.combatComponent.combatMode != COMBAT_MODE.Passive)
			{
				actor.combatComponent.Fight(targetCharacter, "Hostility");
			}
		}
		else if (targetCharacter.traitContainer.HasTrait("Unconscious", "Frozen", "Ensnared"))
		{
			if (isTargetInVillageOfOtherNonHostileFaction)
			{
				return;
			}
			if (targetCharacter.combatComponent.combatMode == COMBAT_MODE.Passive)
			{
				if (GameUtilities.RollChance(5))
				{
					actor.jobComponent.TriggerFactionKidnapAndRestrainJob(targetCharacter);
				}
			}
			else if (GameUtilities.RollChance(ChanceData.GetChance(CHANCE_TYPE.Kidnap_Chance)))
			{
				actor.jobComponent.TriggerFactionKidnapAndRestrainJob(targetCharacter);
			}
			else if (targetCharacter.combatComponent.combatMode != COMBAT_MODE.Passive)
			{
				actor.combatComponent.Fight(targetCharacter, "Hostility");
			}
		}
		else if (!targetCharacter.isDead && (targetCharacter.combatComponent.combatMode != COMBAT_MODE.Passive || targetCharacter.race == RACE.HARPY) && !targetCharacter.traitContainer.HasTrait("Hibernating") && actor.combatComponent.combatMode == COMBAT_MODE.Aggressive)
		{
			HostileFightOrFlightReaction(actor, targetCharacter, ref debugLog);
		}
	}

	private void NonHostileAliveVillagerHomeOrFactionmateReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		if (disguisedActor.relationshipContainer.IsEnemiesWith(disguisedTarget) || disguisedActor.relationshipContainer.HasGrudgeAgainst(disguisedTarget))
		{
			HomeOrFactionmateEnemyReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
		}
		else if (!disguisedActor.traitContainer.HasTrait("Psychopath"))
		{
			HomeOrFactionmateNonPsychopathAndEnemyReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
		}
		if (disguisedActor.homeSettlement == null || !disguisedActor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event))
		{
			return;
		}
		Lethargic traitOrStatus = disguisedTarget.traitContainer.GetTraitOrStatus<Lethargic>("Lethargic");
		if (traitOrStatus != null && !traitOrStatus.IsResponsibleForTrait(disguisedActor) && !disguisedActor.defaultCharacterTrait.IsAwareOfTrait(disguisedActor, traitOrStatus))
		{
			disguisedActor.defaultCharacterTrait.BecomeAwareOfTrait(disguisedTarget, traitOrStatus);
			if (GameUtilities.RollChance(25) && !disguisedActor.relationshipContainer.IsFriendsWith(disguisedTarget))
			{
				disguisedActor.assumptionComponent.CreateAndReactToNewAssumption(disguisedTarget, disguisedTarget, INTERACTION_TYPE.IS_PLAGUED, REACTION_STATUS.WITNESSED, !disguisedTarget.traitContainer.HasTrait("Plagued"));
			}
		}
	}

	private void HomeOrFactionmateEnemyReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		if ((!targetCharacter.limiterComponent.canMove || !targetCharacter.limiterComponent.canPerform) && !targetCharacter.defaultCharacterTrait.HasReactedToThis(base.owner) && !targetCharacter.traitContainer.HasTrait("Resting"))
		{
			if (Random.Range(0, 2) == 0)
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
			}
			else
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
			}
		}
	}

	private void HomeOrFactionmateNonPsychopathAndEnemyReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		bool flag = targetCharacter.traitContainer.HasTrait("Paralyzed", "Ensnared");
		bool flag2 = targetCharacter.traitContainer.HasTrait("Quarantined");
		bool flag3 = targetCharacter.traitContainer.HasTrait("Restrained") && disguisedTarget.traitContainer.HasTrait("Criminal");
		bool flag4 = targetCharacter.traitContainer.HasTrait("Catatonic");
		bool flag5 = targetCharacter.crimeComponent.IsWantedBy(actor.faction);
		bool flag6 = disguisedTarget.traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.DoesCharacterKnowThisVampire(disguisedActor) ?? false;
		bool flag7 = disguisedTarget.isLycanthrope && disguisedTarget.lycanData.DoesCharacterKnowThisLycan(disguisedActor);
		if (flag || flag3 || flag4 || flag2)
		{
			if ((targetCharacter.needsComponent.isHungry || targetCharacter.needsComponent.isStarving) && !flag6 && targetCharacter.race.IsSapient())
			{
				if (!targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.FEED))
				{
					actor.jobComponent.TryTriggerFeed(targetCharacter);
				}
			}
			else if ((targetCharacter.needsComponent.isTired || targetCharacter.needsComponent.isExhausted) && flag && !flag2 && !flag5)
			{
				if (disguisedTarget.homeStructure != null && disguisedTarget.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED) is Bed bed && bed.gridTileLocation != targetCharacter.gridTileLocation && !targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.MOVE_CHARACTER) && targetCharacter.currentActionNode == null)
				{
					actor.jobComponent.TryTriggerMoveCharacter(targetCharacter, disguisedTarget.homeStructure, bed.gridTileLocation);
				}
			}
			else if ((targetCharacter.needsComponent.isBored || targetCharacter.needsComponent.isSulking) && targetCharacter.traitContainer.HasTrait("Paralyzed") && !flag2 && !flag5)
			{
				if (Random.Range(0, 2) == 0 && disguisedTarget.homeStructure != null)
				{
					if (targetCharacter.currentStructure != disguisedTarget.homeStructure && !targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.MOVE_CHARACTER) && targetCharacter.currentActionNode == null)
					{
						actor.jobComponent.TryTriggerMoveCharacter(targetCharacter, disguisedTarget.homeStructure);
					}
				}
				else if (!targetCharacter.currentStructure.structureType.IsOpenSpace() && !targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.MOVE_CHARACTER) && targetCharacter.currentActionNode == null)
				{
					BaseSettlement baseSettlement = targetCharacter.currentSettlement;
					if (baseSettlement == null)
					{
						baseSettlement = actor.homeSettlement;
					}
					if (baseSettlement == null)
					{
						baseSettlement = targetCharacter.homeSettlement;
					}
					if (baseSettlement != null)
					{
						actor.jobComponent.TryTriggerMoveCharacter(targetCharacter, baseSettlement);
					}
				}
			}
		}
		if (flag6)
		{
			HomeOrFactionmateNonPsychopathAndEnemyKnownVampireReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
		}
		if (flag7)
		{
			HomeOrFactionmateNonPsychopathAndEnemyKnownWerewolfReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
		}
		HomeOrFactionmateNonPsychopathAndEnemyRemoveStatusReactionToCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, ref debugLog);
	}

	private void HomeOrFactionmateNonPsychopathAndEnemyKnownVampireReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		if (disguisedActor.characterClass.className == "Shaman" && disguisedActor.relationshipContainer.IsFriendsWith(disguisedTarget) && !disguisedActor.traitContainer.HasTrait("Hemophiliac") && disguisedActor.faction != null && !disguisedActor.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Vampires))
		{
			actor.jobComponent.TriggerCureMagicalAffliction(disguisedTarget, "Vampire");
		}
		else if (disguisedActor.traitContainer.HasTrait("Hemophobic"))
		{
			actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, targetCharacter);
		}
		else if (targetCharacter.needsComponent.isStarving && (disguisedActor.traitContainer.HasTrait("Hemophiliac") || disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget) == "Close Friend") && !targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.FEED, JOB_TYPE.OFFER_BLOOD))
		{
			actor.jobComponent.CreateFeedSelfToVampireJob(targetCharacter);
		}
	}

	private void HomeOrFactionmateNonPsychopathAndEnemyKnownWerewolfReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		if (disguisedActor.characterClass.className == "Shaman" && disguisedActor.relationshipContainer.IsFriendsWith(disguisedTarget) && !disguisedActor.traitContainer.HasTrait("Lycanphiliac") && disguisedActor.faction != null && !disguisedActor.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves))
		{
			actor.jobComponent.TriggerCureMagicalAffliction(disguisedTarget, "Lycanthrope");
		}
		else if (disguisedActor.traitContainer.HasTrait("Lycanphobic"))
		{
			actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, targetCharacter);
		}
	}

	private void HomeOrFactionmateNonPsychopathAndEnemyRemoveStatusReactionToCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, ref string debugLog)
	{
		Prisoner traitOrStatus = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
		Lazy traitOrStatus2 = actor.traitContainer.GetTraitOrStatus<Lazy>("Lazy");
		if ((traitOrStatus2 != null && traitOrStatus2.TryIgnoreUrgentTask(JOB_TYPE.REMOVE_STATUS)) || (traitOrStatus != null && traitOrStatus.IsConsideredPrisonerOf(disguisedActor) && (traitOrStatus.IsPersonalPrisonerOf(disguisedActor) || targetCharacter.faction != actor.faction)))
		{
			return;
		}
		bool flag = targetCharacter.traitContainer.HasTrait("Restrained");
		bool flag2 = targetCharacter.traitContainer.HasTrait("Ensnared");
		bool flag3 = targetCharacter.traitContainer.HasTrait("Frozen");
		bool flag4 = targetCharacter.traitContainer.HasTrait("Unconscious");
		bool flag5 = targetCharacter.traitContainer.HasTrait("Enslaved");
		bool flag6 = flag5 && disguisedActor.relationshipContainer.HasRelationshipWith(disguisedTarget) && !disguisedActor.relationshipContainer.IsEnemiesWith(disguisedTarget);
		bool flag7 = actor.isAlliedWithPlayer && targetCharacter.limiterComponent.isTargetedByDemonicSnatch;
		if (!disguisedActor.isNormalCharacter || !(((disguisedTarget.isNormalCharacter || disguisedTarget.faction == disguisedActor.faction) && (flag || flag2 || flag3 || flag4)) || flag6) || disguisedTarget.crimeComponent.IsWantedBy(disguisedActor.faction) || flag7)
		{
			return;
		}
		bool flag8 = flag && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Restrained").IsResponsibleForTrait(disguisedActor);
		bool flag9 = flag2 && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Ensnared").IsResponsibleForTrait(disguisedActor);
		bool flag10 = flag3 && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Frozen").IsResponsibleForTrait(disguisedActor);
		bool flag11 = flag4 && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Unconscious").IsResponsibleForTrait(disguisedActor);
		bool flag12 = flag5 && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Enslaved").IsResponsibleForTrait(disguisedActor);
		bool flag13 = false;
		if (!disguisedActor.traitContainer.HasTrait("Enslaved") && flag5 && !flag12 && disguisedActor.faction != targetCharacter.faction && !targetCharacter.isDead)
		{
			flag13 = actor.jobComponent.TriggerReleaseJob(targetCharacter);
		}
		if (flag13)
		{
			return;
		}
		bool flag14 = !targetCharacter.HasJobTargetingThis(JOB_TYPE.REMOVE_STATUS);
		if (!flag14)
		{
			flag14 = !actor.HasCharacterInVisionWithJobAndTarget(JOB_TYPE.REMOVE_STATUS, targetCharacter, targetCharacter);
		}
		if (flag14)
		{
			if (flag && !targetCharacter.HasJobTargetingThis(JOB_TYPE.BLOOD_SACRIFICE))
			{
				actor.jobComponent.TriggerRemoveStatusTarget(targetCharacter, "Restrained");
			}
			if (flag2 && !flag9)
			{
				actor.jobComponent.TriggerRemoveStatusTarget(targetCharacter, "Ensnared");
			}
			if (flag3 && !flag10)
			{
				actor.jobComponent.TriggerRemoveStatusTarget(targetCharacter, "Frozen");
			}
			if (flag4 && !flag11 && !targetCharacter.HasJobTargetingThis(JOB_TYPE.BLOOD_SACRIFICE) && !targetCharacter.traitContainer.HasTrait("Berserked"))
			{
				actor.jobComponent.TriggerRemoveStatusTarget(targetCharacter, "Unconscious");
			}
		}
		else if (!flag8 && !flag9 && !flag10 && !flag11 && !disguisedTarget.defaultCharacterTrait.HasReactedToThis(disguisedActor))
		{
			if (GameUtilities.RollChance(35))
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, targetCharacter);
			}
			else
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, targetCharacter, "", null, "Shocked_In_A_Bind");
			}
		}
	}

	public void AddCharacterThatSawThisDead(Character character)
	{
		charactersThatSawThisDead.Add(character);
	}

	public void SetIsHidden(bool state)
	{
		if (isHidden == state)
		{
			return;
		}
		isHidden = state;
		base.owner.OnSetIsHidden();
		UpdateHiddenState();
		if (isHidden || !base.owner.marker)
		{
			return;
		}
		for (int i = 0; i < base.owner.marker.inVisionCharacters.Count; i++)
		{
			Character character = base.owner.marker.inVisionCharacters[i];
			if (character.hasMarker)
			{
				character.marker.AddUnprocessedPOI(base.owner);
			}
		}
	}

	public void UpdateHiddenState()
	{
		if ((bool)base.owner.marker)
		{
			if (isHidden)
			{
				base.owner.marker.SetVisualAlpha(0.5f);
			}
			else
			{
				base.owner.marker.SetVisualAlpha(1f);
			}
		}
	}

	public void SetDisguisedCharacter(Character character)
	{
		if (disguisedCharacter == character)
		{
			return;
		}
		disguisedCharacter = character;
		if (disguisedCharacter != null)
		{
			base.owner.visuals.UpdateAllVisuals(base.owner);
			Messenger.Broadcast(CharacterSignals.CHARACTER_DISGUISED, base.owner, character);
			return;
		}
		base.owner.visuals.UpdateAllVisuals(base.owner);
		if (base.owner.isDead || !base.owner.marker)
		{
			return;
		}
		for (int i = 0; i < base.owner.marker.inVisionCharacters.Count; i++)
		{
			Character character2 = base.owner.marker.inVisionCharacters[i];
			if (!character2.isDead && (bool)character2.marker)
			{
				character2.marker.AddUnprocessedPOI(base.owner);
			}
		}
	}

	public void SetHasBeedShockedByDemonicStructure(bool p_state)
	{
		hasBeenShockedByDemonicStructure = p_state;
	}

	public void ResistRuinarchPower()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "General", "PlayerAlerts_Table", "resist_ruinarch_power", LOG_TAG.Major);
		log.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
		LogPool.Release(log);
		PlayResistVFXandSFX();
	}

	public void PlayResistVFXandSFX()
	{
		GameManager.Instance.CreateParticleEffectAt(base.owner, PARTICLE_EFFECT.Resist).GetComponent<ResistEffect>().PlayEffect(base.owner.marker.usedSprite);
		AkSoundEngine.PostEvent("Play_Resist_Power", base.owner.marker.gameObject);
	}

	private void PetReactToPetOwner(Character p_actor, Character p_target, ref string debugLog)
	{
		if (p_target.traitContainer.HasTrait("Restrained"))
		{
			p_actor.jobComponent.TriggerReleaseJob(p_target);
			return;
		}
		bool num = p_target.traitContainer.HasTrait("Ensnared");
		bool flag = p_target.traitContainer.HasTrait("Frozen");
		bool flag2 = p_target.traitContainer.HasTrait("Unconscious");
		if (num)
		{
			p_actor.jobComponent.TriggerRemoveStatusTarget(p_target, "Ensnared");
		}
		if (flag)
		{
			p_actor.jobComponent.TriggerRemoveStatusTarget(p_target, "Frozen");
		}
		if (flag2 && !p_target.HasJobTargetingThis(JOB_TYPE.BLOOD_SACRIFICE) && !p_target.traitContainer.HasTrait("Berserked"))
		{
			p_actor.jobComponent.TriggerRemoveStatusTarget(p_target, "Unconscious");
		}
	}

	private void ReactionToPet(Character p_actor, Character p_target, ref string debugLog)
	{
		Character petOwner = p_target.petComponent.petOwner;
		if (p_actor.characterClass.IsCombatant() && petOwner.crimeComponent.IsWantedBy(p_actor.faction) && InteractionManager.Instance.CanCharacterTakeApprehendJob(p_actor, petOwner) && (!p_target.marker.IsPOIInVision(petOwner) || !petOwner.limiterComponent.canPerform))
		{
			base.owner.combatComponent.Fight(p_target, "Slay_Target");
		}
	}

	private bool CanReactVigilant(IPointOfInterest p_reactor, IReactable p_reactable)
	{
		if (p_reactor.traitContainer.HasTrait("Vigilant"))
		{
			if (p_reactor.traitContainer.HasTrait("Hemophiliac") && p_reactable.crimeType == CRIME_TYPE.Vampire)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void LoadReferences(SaveDataReactionComponent data)
	{
		for (int i = 0; i < data.charactersThatSawThisDead.Count; i++)
		{
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(data.charactersThatSawThisDead[i]);
			if (characterByPersistentID != null)
			{
				charactersThatSawThisDead.Add(characterByPersistentID);
			}
		}
		if (!string.IsNullOrEmpty(data.disguisedCharacter))
		{
			disguisedCharacter = CharacterManager.Instance.GetCharacterByPersistentID(data.disguisedCharacter);
		}
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		assumptionSuspects.Remove(p_character);
		charactersThatSawThisDead.Remove(p_character);
		if (disguisedCharacter == p_character)
		{
			SetDisguisedCharacter(null);
			base.owner.CancelAllJobs();
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		assumptionSuspects.Contains(p_character);
		charactersThatSawThisDead.Contains(p_character);
		_ = disguisedCharacter;
	}
}
