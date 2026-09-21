using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;
using UtilityScripts;

public class TriggerGrudgeData : PlayerAction
{
	private WeightedDictionary<MonsterMigrationBiomeAtomizedData> _monsterWeightedDictionary;

	private List<TRIGGER_GRUDGE_ACTION> _grudgeActionChoices;

	private TRIGGER_GRUDGE_ACTION[] _grudgeActions;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TRIGGER_GRUDGE;

	public override string name => "Trigger Grudge";

	public override string description => "This Ability will consume a Grudge and force the Villager to act against their Grudge target.\nActivating Trigger Grudge produces a Chaos Orb. If the Villager successfully performs a task related to the Grudge, it will produce additional 2 Chaos Orbs.";

	public TriggerGrudgeData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
		_grudgeActions = (TRIGGER_GRUDGE_ACTION[])Enum.GetValues(typeof(TRIGGER_GRUDGE_ACTION));
		_grudgeActionChoices = new List<TRIGGER_GRUDGE_ACTION>();
		ConstructSummonEphemeralBeastsDictionary();
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (!targetCharacter.limiterComponent.canPerform || !targetCharacter.limiterComponent.canMove)
			{
				return false;
			}
			if (targetCharacter.partyComponent.isMemberThatJoinedQuest)
			{
				return false;
			}
			return !targetCharacter.isDead;
		}
		return false;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character character)
		{
			if (!character.isNormalCharacter || character.isConsideredRatman)
			{
				return false;
			}
			if (!character.relationshipContainer.HasGrudgeAgainstAliveCharactersWithPath(character))
			{
				return false;
			}
		}
		return base.IsValid(target);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (!targetCharacter.limiterComponent.canPerform || !targetCharacter.limiterComponent.canMove)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Target_Incapacitated") + "|";
		}
		if (targetCharacter.partyComponent.isMemberThatJoinedQuest)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Target_Character_In_Quest") + "|";
		}
		if (targetCharacter.isDead)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Dead") + "|";
		}
		return text;
	}

	protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems)
	{
		if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character character)
		{
			p_contextMenuItems.Clear();
			for (int i = 0; i < character.relationshipContainer.charactersWithOpinion.Count; i++)
			{
				Character character2 = character.relationshipContainer.charactersWithOpinion[i];
				IRelationshipData relationshipDataWith = character.relationshipContainer.GetRelationshipDataWith(character2);
				if (relationshipDataWith != null && relationshipDataWith.hasGrudge && !character2.isDead)
				{
					p_contextMenuItems.Add(character2);
				}
			}
			return p_contextMenuItems;
		}
		return null;
	}

	public void ShowGrudgeChoices(Character p_actor, Character p_target)
	{
		UIManager.Instance.HideContextMenu();
		PopulateViableGrudgeActions(p_actor, p_target);
		if (_grudgeActionChoices.Count > 0)
		{
			Messenger.Broadcast(UISignals.SHOW_GRUDGE_UI, _grudgeActionChoices, p_actor, p_target);
			return;
		}
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Grudge_Failed");
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("actorName", p_actor.name);
		dictionary.Add("targetName", p_target.name);
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Grudge_Failed_Description", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		PlayerUI.Instance.ShowGeneralConfirmation(localizedValue, localizedValue2);
	}

	public void TryActivateGrudge(TRIGGER_GRUDGE_ACTION p_grudgeActionType, Character p_actor, Character p_target)
	{
		if (!ActivateGrudge(p_grudgeActionType, p_actor, p_target))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Powers", "PlayerPowerAlerts_Table", name + " fail_trigger", LOG_TAG.Player);
			log.AddToFillers(p_actor, p_actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(p_target, p_target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			return;
		}
		Messenger.Broadcast(PlayerSignals.TRIGGERED_GRUDGE, p_grudgeActionType, p_actor, p_target);
		OnExecutePlayerSkill();
		p_actor.relationshipContainer.SetHasGrudgeAgainst(p_actor, p_target, p_state: false);
		if (p_actor.gridTileLocation != null)
		{
			SpawnChaosOrbs(1, p_actor.gridTileLocation);
		}
	}

	private void PopulateViableGrudgeActions(Character p_actor, Character p_target)
	{
		_grudgeActionChoices.Clear();
		for (int i = 0; i < _grudgeActions.Length; i++)
		{
			TRIGGER_GRUDGE_ACTION tRIGGER_GRUDGE_ACTION = _grudgeActions[i];
			if (CanDoGrudgeAction(tRIGGER_GRUDGE_ACTION, p_actor, p_target))
			{
				_grudgeActionChoices.Add(tRIGGER_GRUDGE_ACTION);
			}
		}
		while (_grudgeActionChoices.Count > 3)
		{
			_grudgeActionChoices.RemoveAt(GameUtilities.RandomBetweenTwoNumbers(0, _grudgeActionChoices.Count - 1));
		}
	}

	private bool CanDoGrudgeAction(TRIGGER_GRUDGE_ACTION p_grudgeActionType, Character p_actor, Character p_target)
	{
		return p_grudgeActionType switch
		{
			TRIGGER_GRUDGE_ACTION.Attack_Kill => CanDoGrudgeActionAttackKill(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Destroy_Home => CanDoGrudgeActionDestroyHome(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Fabricate_Crime => CanDoGrudgeActionFabricateCrime(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Booby_Trap => CanDoGrudgeActionBoobyTrap(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Summon_Monsters => CanDoGrudgeActionSummonMonsters(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Expel_Party => CanDoGrudgeActionExpelParty(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Leave_Party => CanDoGrudgeActionLeaveParty(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Expel_Faction => CanDoGrudgeActionExpelFaction(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Turn_Evil => CanDoTurnEvil(p_actor), 
			_ => false, 
		};
	}

	private bool CanDoGrudgeActionAttackKill(Character p_actor, Character p_target)
	{
		if (p_actor.hasMarker && p_target.hasMarker && p_actor.gridTileLocation != null && p_target.gridTileLocation != null && p_actor.movementComponent.HasPathToEvenIfDiffRegion(p_target.gridTileLocation) && !p_actor.isDead && !p_target.isDead && (p_actor.marker.IsPOIInVision(p_target) || p_actor.gridTileLocation.area.IsNearbyTo(p_target.gridTileLocation.area)))
		{
			return true;
		}
		return false;
	}

	private bool CanDoGrudgeActionDestroyHome(Character p_actor, Character p_target)
	{
		if (p_target.homeStructure != null && p_target.homeStructure is Dwelling dwelling && p_actor.homeStructure != p_target.homeStructure && !p_actor.jobQueue.HasJob(JOB_TYPE.GRUDGE) && p_actor.movementComponent.HasPathToEvenIfDiffRegion(dwelling.structureTileObject.gridTileLocation))
		{
			return true;
		}
		return false;
	}

	private bool CanDoGrudgeActionFabricateCrime(Character p_actor, Character p_target)
	{
		if (p_actor.faction != null && !p_target.crimeComponent.IsWantedBy(p_actor.faction) && ((!p_target.isSettlementRuler && !p_target.isFactionLeader) || p_actor.faction != p_target.faction))
		{
			if ((!p_actor.isFactionLeader && !p_actor.isSettlementRuler) || p_actor.faction != p_target.faction)
			{
				return GetRandomFabricateCrimeInformationReceiver(p_actor, p_target) != null;
			}
			return true;
		}
		return false;
	}

	private bool CanDoGrudgeActionBoobyTrap(Character p_actor, Character p_target)
	{
		if (p_target.homeStructure != null && p_target.homeStructure is Dwelling dwelling)
		{
			for (int i = 0; i < dwelling.pointsOfInterest.Count; i++)
			{
				if (dwelling.pointsOfInterest.ElementAt(i) is TileObject { mapObjectState: MAP_OBJECT_STATE.BUILT } tileObject && tileObject.OccupiesTile() && !tileObject.isHidden && !tileObject.traitContainer.HasTrait("Booby Trapped") && tileObject.tileObjectType != TILE_OBJECT_TYPE.BLOCK_WALL && tileObject.tileObjectType != TILE_OBJECT_TYPE.ICE_BLOCK_WALL && tileObject.tileObjectType != TILE_OBJECT_TYPE.THIN_WALL && tileObject.gridTileLocation != null && p_actor.movementComponent.HasPathToEvenIfDiffRegion(tileObject.gridTileLocation))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CanDoGrudgeActionSummonMonsters(Character p_actor, Character p_target)
	{
		if (p_actor.hasMarker && p_target.hasMarker && p_actor.gridTileLocation != null && p_target.gridTileLocation != null && !p_target.isDead && !p_actor.jobQueue.HasJob(JOB_TYPE.GRUDGE) && p_actor.characterClass.attackType == ATTACK_TYPE.MAGICAL && p_actor.movementComponent.HasPathToEvenIfDiffRegion(p_target.gridTileLocation) && (p_actor.marker.IsPOIInVision(p_target) || p_actor.gridTileLocation.area.IsNearbyTo(p_target.gridTileLocation.area)))
		{
			return true;
		}
		return false;
	}

	private bool CanDoGrudgeActionExpelParty(Character p_actor, Character p_target)
	{
		if (p_actor.partyComponent.hasParty && p_actor.partyComponent.currentParty == p_target.partyComponent.currentParty && p_actor.partyComponent.currentParty.partyLeader == p_actor)
		{
			return true;
		}
		return false;
	}

	private bool CanDoGrudgeActionLeaveParty(Character p_actor, Character p_target)
	{
		if (p_actor.partyComponent.hasParty && p_actor.partyComponent.currentParty == p_target.partyComponent.currentParty && p_actor.partyComponent.currentParty.partyLeader != p_actor)
		{
			return true;
		}
		return false;
	}

	private bool CanDoGrudgeActionExpelFaction(Character p_actor, Character p_target)
	{
		if (p_actor.faction != null && p_target.faction != null && p_actor.faction == p_target.faction && p_actor.isFactionLeader)
		{
			return true;
		}
		return false;
	}

	private bool CanDoTurnEvil(Character p_actor)
	{
		if (!p_actor.traitContainer.HasTrait("Evil"))
		{
			return true;
		}
		return false;
	}

	private bool ActivateGrudge(TRIGGER_GRUDGE_ACTION p_grudgeActionType, Character p_actor, Character p_target)
	{
		return p_grudgeActionType switch
		{
			TRIGGER_GRUDGE_ACTION.Attack_Kill => ActivateGrudgeActionAttackKill(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Destroy_Home => ActivateGrudgeActionDestroyHome(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Fabricate_Crime => ActivateGrudgeActionFabricateCrime(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Booby_Trap => ActivateGrudgeActionBoobyTrap(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Summon_Monsters => ActivateGrudgeActionSummonMonsters(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Expel_Party => ActivateGrudgeActionExpelParty(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Leave_Party => ActivateGrudgeActionLeaveParty(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Expel_Faction => ActivateGrudgeActionExpelFaction(p_actor, p_target), 
			TRIGGER_GRUDGE_ACTION.Turn_Evil => ActivateTurnEvil(p_actor), 
			_ => false, 
		};
	}

	private bool ActivateGrudgeActionAttackKill(Character p_actor, Character p_target)
	{
		p_actor.jobQueue.CancelAllJobs();
		p_actor.combatComponent.Fight(p_target, "Grudge");
		if (p_actor.gridTileLocation != null)
		{
			SpawnChaosOrbs(2, p_actor.gridTileLocation);
		}
		return true;
	}

	private bool ActivateGrudgeActionDestroyHome(Character p_actor, Character p_target)
	{
		if (p_target.homeStructure is Dwelling dwelling && p_actor.jobComponent.TriggerDestroyHome(JOB_TYPE.GRUDGE, dwelling.structureTileObject))
		{
			return true;
		}
		return false;
	}

	private bool ActivateGrudgeActionFabricateCrime(Character p_actor, Character p_target)
	{
		if ((p_actor.isFactionLeader || p_actor.isSettlementRuler) && p_actor.faction == p_target.faction)
		{
			ActualGoapNode actualGoapNode = FabricateGrudgeCrime(p_actor, p_target);
			if (actualGoapNode != null)
			{
				actualGoapNode.descriptionLog?.AddTag(LOG_TAG.Witnessed);
				actualGoapNode.IncreaseReactionCounter();
				p_actor.reactionComponent.ReactTo(actualGoapNode, REACTION_STATUS.WITNESSED, addLog: false);
				actualGoapNode.DecreaseReactionCounter();
				SpawnChaosOrbs(2, p_actor.gridTileLocation);
				return true;
			}
		}
		return SpreadFabricatedCrime(p_actor, p_target);
	}

	private bool ActivateGrudgeActionBoobyTrap(Character p_actor, Character p_target)
	{
		if (p_actor.jobComponent.CreatePlaceTrapOnAnyHomeItemJob(p_target, JOB_TYPE.GRUDGE))
		{
			return true;
		}
		return false;
	}

	private bool ActivateGrudgeActionSummonMonsters(Character p_actor, Character p_target)
	{
		if (p_actor.jobComponent.TriggerSummonEphemeralBeasts(JOB_TYPE.GRUDGE, p_target))
		{
			return true;
		}
		return false;
	}

	private bool ActivateGrudgeActionExpelParty(Character p_actor, Character p_target)
	{
		if (p_target.interruptComponent.TriggerInterrupt(INTERRUPT.Removed_From_Party, p_target, "", null, "Removed_From_Party_Grudge"))
		{
			SpawnChaosOrbs(2, p_actor.gridTileLocation);
			return true;
		}
		return false;
	}

	private bool ActivateGrudgeActionLeaveParty(Character p_actor, Character p_target)
	{
		if (p_actor.interruptComponent.TriggerInterrupt(INTERRUPT.Left_Party, p_actor, "", null, "Left_Party_Grudgeful_With_Member"))
		{
			SpawnChaosOrbs(2, p_actor.gridTileLocation);
			return true;
		}
		return false;
	}

	private bool ActivateGrudgeActionExpelFaction(Character p_actor, Character p_target)
	{
		if (p_target.faction != null)
		{
			p_target.faction.KickOutCharacterByPlayer(p_target);
			SpawnChaosOrbs(2, p_actor.gridTileLocation);
			return true;
		}
		return false;
	}

	private bool ActivateTurnEvil(Character p_actor)
	{
		if (p_actor.traitContainer.AddTrait(p_actor, "Evil"))
		{
			SpawnChaosOrbs(2, p_actor.gridTileLocation);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Powers", "TriggerGrudge_Table", "Turn_Evil_Success", LOG_TAG.Player);
			log.AddToFillers(p_actor, p_actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_MAKE_VILLAGER_EVIL);
			return true;
		}
		return false;
	}

	private bool SpreadFabricatedCrime(Character p_fabricator, Character p_crimeActor)
	{
		ActualGoapNode actualGoapNode = FabricateGrudgeCrime(p_fabricator, p_crimeActor);
		if (actualGoapNode != null)
		{
			Character randomFabricateCrimeInformationReceiver = GetRandomFabricateCrimeInformationReceiver(p_fabricator, p_crimeActor);
			if (randomFabricateCrimeInformationReceiver != null)
			{
				return p_fabricator.jobComponent.CreateSpreadNegativeInfoJob(JOB_TYPE.GRUDGE, randomFabricateCrimeInformationReceiver, actualGoapNode);
			}
		}
		return false;
	}

	private Character GetRandomFabricateCrimeInformationReceiver(Character p_fabricator, Character p_crimeActor)
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < p_fabricator.faction.characters.Count; i++)
		{
			Character character = p_fabricator.faction.characters[i];
			if (!character.isDead && character.IsAtHome() && character.limiterComponent.canPerform && character.limiterComponent.canMove && character != p_fabricator && character != p_crimeActor && character.homeSettlement == p_fabricator.homeSettlement && character.homeSettlement != null && !character.partyComponent.isMemberThatJoinedQuest && (character.isFactionLeader || character.isSettlementRuler))
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	private ActualGoapNode FabricateGrudgeCrime(Character p_fabricator, Character p_crimeActor)
	{
		List<CRIME_TYPE> list = RuinarchListPool<CRIME_TYPE>.Claim();
		List<INTERACTION_TYPE> list2 = RuinarchListPool<INTERACTION_TYPE>.Claim();
		p_crimeActor.faction.factionType.PopulateCrimeTypesBySeverity(list, CRIME_SEVERITY.Serious);
		list.Remove(CRIME_TYPE.Murder);
		InteractionManager.Instance.PopulateActionTypesBasedOnCrimeType(list2, p_crimeActor, list);
		INTERACTION_TYPE iNTERACTION_TYPE = INTERACTION_TYPE.NONE;
		if (list2.Count > 0)
		{
			iNTERACTION_TYPE = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
		}
		RuinarchListPool<INTERACTION_TYPE>.Release(list2);
		RuinarchListPool<CRIME_TYPE>.Release(list);
		ActualGoapNode actualGoapNode = null;
		if (iNTERACTION_TYPE != INTERACTION_TYPE.NONE)
		{
			if (!InteractionManager.Instance.goapActionData[iNTERACTION_TYPE].isTargetSelf)
			{
				Character randomAliveNonLeaderCharacterWithOpinion = p_crimeActor.relationshipContainer.GetRandomAliveNonLeaderCharacterWithOpinion(p_crimeActor);
				if (randomAliveNonLeaderCharacterWithOpinion != null)
				{
					actualGoapNode = InteractionManager.Instance.CreateNewIllusionAction(p_crimeActor, randomAliveNonLeaderCharacterWithOpinion, iNTERACTION_TYPE, shouldLog: false);
					actualGoapNode.SetIsFabricated(p_state: true);
				}
			}
			else
			{
				actualGoapNode = InteractionManager.Instance.CreateNewIllusionAction(p_crimeActor, p_crimeActor, iNTERACTION_TYPE, shouldLog: false);
				actualGoapNode.SetIsFabricated(p_state: true);
			}
		}
		return actualGoapNode;
	}

	private void ConstructSummonEphemeralBeastsDictionary()
	{
		_monsterWeightedDictionary = new WeightedDictionary<MonsterMigrationBiomeAtomizedData>();
		_monsterWeightedDictionary.AddElement(new MonsterMigrationBiomeAtomizedData
		{
			monsterType = SUMMON_TYPE.Skeleton,
			minRange = 2,
			maxRange = 3,
			weight = 10
		}, 10);
		_monsterWeightedDictionary.AddElement(new MonsterMigrationBiomeAtomizedData
		{
			monsterType = SUMMON_TYPE.Wolf,
			minRange = 2,
			maxRange = 3,
			weight = 10
		}, 10);
		_monsterWeightedDictionary.AddElement(new MonsterMigrationBiomeAtomizedData
		{
			monsterType = SUMMON_TYPE.Bear,
			minRange = 2,
			maxRange = 3,
			weight = 10
		}, 10);
		_monsterWeightedDictionary.AddElement(new MonsterMigrationBiomeAtomizedData
		{
			monsterType = SUMMON_TYPE.Boar,
			minRange = 2,
			maxRange = 3,
			weight = 10
		}, 10);
		_monsterWeightedDictionary.AddElement(new MonsterMigrationBiomeAtomizedData
		{
			monsterType = SUMMON_TYPE.Chicken,
			minRange = 3,
			maxRange = 5,
			weight = 10
		}, 10);
		_monsterWeightedDictionary.AddElement(new MonsterMigrationBiomeAtomizedData
		{
			monsterType = SUMMON_TYPE.Rabbit,
			minRange = 3,
			maxRange = 5,
			weight = 10
		}, 10);
		_monsterWeightedDictionary.AddElement(new MonsterMigrationBiomeAtomizedData
		{
			monsterType = SUMMON_TYPE.Sheep,
			minRange = 3,
			maxRange = 5,
			weight = 10
		}, 10);
		_monsterWeightedDictionary.AddElement(new MonsterMigrationBiomeAtomizedData
		{
			monsterType = SUMMON_TYPE.Pig,
			minRange = 3,
			maxRange = 5,
			weight = 10
		}, 10);
	}

	public MonsterMigrationBiomeAtomizedData GetRandomMonsterForSummoning()
	{
		return _monsterWeightedDictionary.PickRandomElementGivenWeights();
	}

	private void SpawnChaosOrbs(int p_amount, LocationGridTile p_tile)
	{
		if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.TRIGGER_GRUDGE).TryDecreaseRemainingChaosOrbs(ref p_amount))
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_tile.centeredWorldLocation, p_amount, p_tile.parentMap);
		}
	}
}
