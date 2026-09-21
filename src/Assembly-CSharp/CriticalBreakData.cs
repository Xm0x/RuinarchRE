using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Maccima_Games.Util;
using UtilityScripts;

public class CriticalBreakData : PlayerAction
{
	private List<CRITICAL_BREAK_ACTION> _criticalBreakActionChoices;

	private CRITICAL_BREAK_ACTION[] _criticalBreakActions;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CRITICAL_BREAK;

	public override string name => "Critical Break";

	public override string description => "This Ability will force the Villager to perform a critical action. A list of up to three possible actions will be provided to the player. This can only be triggered on Villagers in Critical Mood.\nActivating Critical Break produces 2 Chaos Orbs and will give the Villager Catharsis.";

	public CriticalBreakData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
		_criticalBreakActions = (CRITICAL_BREAK_ACTION[])Enum.GetValues(typeof(CRITICAL_BREAK_ACTION));
		_criticalBreakActionChoices = new List<CRITICAL_BREAK_ACTION>();
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character p_actor)
		{
			ShowCriticalBreakChoices(p_actor);
		}
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		bool flag = base.IsValid(target);
		if (flag && target is Character character && (character.isDead || !character.isNormalCharacter))
		{
			return false;
		}
		return flag;
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		bool flag = base.CanPerformAbilityTowards(targetCharacter);
		if (flag)
		{
			if (targetCharacter.moodComponent.isInCriticalBreak)
			{
				return false;
			}
			if (!targetCharacter.moodComponent.isInCriticalMood)
			{
				return false;
			}
			if (targetCharacter.limiterComponent.IsIncapacitated())
			{
				return false;
			}
		}
		return flag;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.moodComponent.isInCriticalBreak)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Critical_Break", targetCharacter) + "|";
		}
		if (!targetCharacter.moodComponent.isInCriticalMood)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Not_Critical_Mood", targetCharacter) + "|";
		}
		if (targetCharacter.limiterComponent.IsIncapacitated())
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Target_Incapacitated") + "|";
		}
		return text;
	}

	private void PopulateViableCriticalBreakActions(Character p_actor)
	{
		_criticalBreakActionChoices.Clear();
		for (int i = 0; i < _criticalBreakActions.Length; i++)
		{
			CRITICAL_BREAK_ACTION cRITICAL_BREAK_ACTION = _criticalBreakActions[i];
			if (CanDoCriticalBreak(cRITICAL_BREAK_ACTION, p_actor))
			{
				_criticalBreakActionChoices.Add(cRITICAL_BREAK_ACTION);
			}
		}
		while (_criticalBreakActionChoices.Count > 3)
		{
			_criticalBreakActionChoices.RemoveAt(GameUtilities.RandomBetweenTwoNumbers(0, _criticalBreakActionChoices.Count - 1));
		}
	}

	private bool CanDoCriticalBreak(CRITICAL_BREAK_ACTION p_action, Character p_actor)
	{
		switch (p_action)
		{
		case CRITICAL_BREAK_ACTION.Break_Up:
			return p_actor.relationshipContainer.HasRelationshipWithAliveCharacter(RELATIONSHIP_TYPE.LOVER);
		case CRITICAL_BREAK_ACTION.Abandon_Faction:
			if (p_actor.faction != null && !p_actor.faction.isPlayerFaction)
			{
				return p_actor.faction.factionType.type != FACTION_TYPE.Vagrants;
			}
			return false;
		case CRITICAL_BREAK_ACTION.Kill_Target:
			if (p_actor.characterClass.IsCombatant())
			{
				List<Area> list = RuinarchListPool<Area>.Claim(50);
				p_actor.areaLocation.PopulateAreasInRange(list, 6, includeCenterTile: true);
				for (int i = 0; i < p_actor.relationshipContainer.charactersWithOpinion.Count; i++)
				{
					Character character = p_actor.relationshipContainer.charactersWithOpinion[i];
					if (!character.isDead && p_actor.relationshipContainer.IsEnemiesWith(character) && list.Contains(p_actor.areaLocation))
					{
						RuinarchListPool<Area>.Release(list);
						return true;
					}
				}
				RuinarchListPool<Area>.Release(list);
			}
			return false;
		case CRITICAL_BREAK_ACTION.Become_Cannibal:
			return !p_actor.traitContainer.HasTrait("Cannibal");
		case CRITICAL_BREAK_ACTION.Lightning_Storm:
			return p_actor.characterClass.attackType == ATTACK_TYPE.MAGICAL;
		case CRITICAL_BREAK_ACTION.Expel:
			if (p_actor.isFactionLeader)
			{
				for (int j = 0; j < p_actor.relationshipContainer.charactersWithOpinion.Count; j++)
				{
					Character character2 = p_actor.relationshipContainer.charactersWithOpinion[j];
					if (!character2.isDead && p_actor.relationshipContainer.IsEnemiesWith(character2) && character2.faction == p_actor.faction)
					{
						return true;
					}
				}
			}
			return false;
		case CRITICAL_BREAK_ACTION.Commit_Suicide:
			return !p_actor.traitContainer.HasTrait("Paralyzed");
		case CRITICAL_BREAK_ACTION.Become_Bandit:
			return FactionManager.Instance.banditFaction == null;
		case CRITICAL_BREAK_ACTION.Become_Evil:
			return !p_actor.traitContainer.HasTrait("Evil");
		case CRITICAL_BREAK_ACTION.Destroy_Structure:
			if (p_actor.homeSettlement != null && p_actor.homeSettlement.locationType == LOCATION_TYPE.VILLAGE)
			{
				return !p_actor.traitContainer.HasTrait("Pyrophobic");
			}
			return false;
		default:
			throw new ArgumentOutOfRangeException("p_action", p_action, null);
		}
	}

	public object GetCriticalBreakTarget(CRITICAL_BREAK_ACTION p_actionType, Character p_actor)
	{
		switch (p_actionType)
		{
		case CRITICAL_BREAK_ACTION.Break_Up:
		{
			List<Character> list4 = RuinarchListPool<Character>.Claim();
			p_actor.relationshipContainer.PopulateAliveCharactersWithRelationship(list4, RELATIONSHIP_TYPE.LOVER);
			Character randomElement = CollectionUtilities.GetRandomElement(list4);
			RuinarchListPool<Character>.Release(list4);
			return randomElement;
		}
		case CRITICAL_BREAK_ACTION.Kill_Target:
		{
			List<Area> list2 = RuinarchListPool<Area>.Claim(50);
			p_actor.areaLocation.PopulateAreasInRange(list2, 6, includeCenterTile: true);
			List<Character> list3 = RuinarchListPool<Character>.Claim(p_actor.relationshipContainer.charactersWithOpinion.Count);
			list3.AddRange(p_actor.relationshipContainer.charactersWithOpinion);
			list3.Shuffle();
			Character result2 = null;
			for (int j = 0; j < p_actor.relationshipContainer.charactersWithOpinion.Count; j++)
			{
				Character character = p_actor.relationshipContainer.charactersWithOpinion[j];
				if (!character.isDead && p_actor.relationshipContainer.IsEnemiesWith(character) && list2.Contains(p_actor.areaLocation))
				{
					result2 = character;
					break;
				}
			}
			RuinarchListPool<Character>.Release(list3);
			RuinarchListPool<Area>.Release(list2);
			return result2;
		}
		case CRITICAL_BREAK_ACTION.Expel:
		{
			List<Character> list5 = RuinarchListPool<Character>.Claim(p_actor.relationshipContainer.charactersWithOpinion.Count);
			list5.AddRange(p_actor.relationshipContainer.charactersWithOpinion);
			list5.Shuffle();
			Character result3 = null;
			for (int k = 0; k < p_actor.relationshipContainer.charactersWithOpinion.Count; k++)
			{
				Character character2 = p_actor.relationshipContainer.charactersWithOpinion[k];
				if (!character2.isDead && p_actor.relationshipContainer.IsEnemiesWith(character2) && character2.faction == p_actor.faction)
				{
					result3 = character2;
					break;
				}
			}
			RuinarchListPool<Character>.Release(list5);
			return result3;
		}
		case CRITICAL_BREAK_ACTION.Destroy_Structure:
		{
			BaseSettlement homeSettlement = p_actor.homeSettlement;
			LocationStructure result = null;
			List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim(homeSettlement.allStructures.Count);
			list.AddRange(homeSettlement.allStructures);
			list.Shuffle();
			for (int i = 0; i < list.Count; i++)
			{
				LocationStructure locationStructure = list[i];
				if (locationStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && locationStructure is ManMadeStructure manMadeStructure && !locationStructure.hasBeenDestroyed)
				{
					result = manMadeStructure;
				}
			}
			RuinarchListPool<LocationStructure>.Release(list);
			return result;
		}
		case CRITICAL_BREAK_ACTION.Abandon_Faction:
		case CRITICAL_BREAK_ACTION.Become_Cannibal:
		case CRITICAL_BREAK_ACTION.Lightning_Storm:
		case CRITICAL_BREAK_ACTION.Commit_Suicide:
		case CRITICAL_BREAK_ACTION.Become_Bandit:
		case CRITICAL_BREAK_ACTION.Become_Evil:
			return p_actor;
		default:
			throw new ArgumentOutOfRangeException("p_actionType", p_actionType, null);
		}
	}

	public void ShowCriticalBreakChoices(Character p_actor)
	{
		UIManager.Instance.HideContextMenu();
		PopulateViableCriticalBreakActions(p_actor);
		if (_criticalBreakActionChoices.Count > 0)
		{
			Messenger.Broadcast(UISignals.SHOW_CRITICAL_BREAK_UI, _criticalBreakActionChoices, p_actor);
			return;
		}
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", "Critical Break_No_Action_Title");
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("source", p_actor.name);
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", "Critical Break_No_Action", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		PlayerUI.Instance.ShowGeneralConfirmation(localizedValue, localizedValue2);
	}

	public void TryActivateCriticalBreak(CRITICAL_BREAK_ACTION p_actionType, Character p_actor, object p_target, string p_actionDescription)
	{
		if (ActivateCriticalBreak(p_actionType, p_actor, p_target))
		{
			if (p_actor.currentActionNode != null && p_actor.currentActionNode.associatedJob != null)
			{
				p_actor.currentActionNode.associatedJob.CancelJob();
			}
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Powers", "PlayerActions_Table", "Critical Break Triggered", LOG_TAG.Player);
			log.AddToFillers(p_actor, p_actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, p_actionDescription, LOG_IDENTIFIER.STRING_1);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			p_actor.moodComponent.OnCriticalBreakStarted(p_actionType, p_target);
			OnExecutePlayerSkill();
			Messenger.Broadcast(PlayerSignals.CRITICAL_BREAK_TRIGGERED, p_actor);
		}
	}

	private bool ActivateCriticalBreak(CRITICAL_BREAK_ACTION p_actionType, Character p_actor, object p_target)
	{
		return p_actionType switch
		{
			CRITICAL_BREAK_ACTION.Break_Up => ActivateBreakUp(p_actor, p_target), 
			CRITICAL_BREAK_ACTION.Abandon_Faction => ActivateAbandonFaction(p_actor), 
			CRITICAL_BREAK_ACTION.Kill_Target => ActivateKillTarget(p_actor, p_target), 
			CRITICAL_BREAK_ACTION.Become_Cannibal => ActivateBecomeCannibal(p_actor), 
			CRITICAL_BREAK_ACTION.Lightning_Storm => ActivateLightningStorm(p_actor), 
			CRITICAL_BREAK_ACTION.Expel => ActivateExpelTarget(p_actor, p_target), 
			CRITICAL_BREAK_ACTION.Commit_Suicide => ActivateSuicide(p_actor), 
			CRITICAL_BREAK_ACTION.Become_Bandit => ActivateBecomeBandit(p_actor), 
			CRITICAL_BREAK_ACTION.Become_Evil => ActivateTurnEvil(p_actor), 
			CRITICAL_BREAK_ACTION.Destroy_Structure => ActivateDestroyStructure(p_actor, p_target), 
			_ => throw new ArgumentOutOfRangeException("p_actionType", p_actionType, null), 
		};
	}

	private bool ActivateBecomeCannibal(Character p_actor)
	{
		p_actor.traitContainer.AddTrait(p_actor, "Cannibal");
		return true;
	}

	private bool ActivateKillTarget(Character p_actor, object p_target)
	{
		Character p_character = p_target as Character;
		p_actor.jobQueue.CancelAllJobs();
		p_actor.behaviourComponent.StartNonInstantCriticalBreak(p_character);
		return true;
	}

	private bool ActivateAbandonFaction(Character p_actor)
	{
		p_actor.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, p_actor, "left_faction_normal");
		return true;
	}

	private bool ActivateBreakUp(Character p_actor, object p_target)
	{
		Character targetPOI = p_target as Character;
		p_actor.interruptComponent.TriggerInterrupt(INTERRUPT.Break_Up, targetPOI);
		return true;
	}

	private bool ActivateLightningStorm(Character p_actor)
	{
		List<AOESpellTileObject> activeSpellsOnTile = p_actor.gridTileLocation.parentMap.region.regionSpellsComponent.GetActiveSpellsOnTile(p_actor.gridTileLocation);
		ElectricStormTileObject electricStormTileObject = null;
		if (activeSpellsOnTile != null)
		{
			for (int i = 0; i < activeSpellsOnTile.Count; i++)
			{
				if (activeSpellsOnTile[i] is ElectricStormTileObject electricStormTileObject2)
				{
					electricStormTileObject = electricStormTileObject2;
					break;
				}
			}
		}
		if (electricStormTileObject != null)
		{
			electricStormTileObject.ResetElectricStormDuration();
		}
		else
		{
			ElectricStormTileObject poi = InnerMapManager.Instance.CreateNewTileObject<ElectricStormTileObject>(TILE_OBJECT_TYPE.ELECTRIC_STORM_TILE_OBJECT);
			p_actor.gridTileLocation.structure.AddPOI(poi, p_actor.gridTileLocation);
		}
		return true;
	}

	private bool ActivateExpelTarget(Character p_actor, object p_target)
	{
		Character character = p_target as Character;
		character.faction.KickOutCharacterByPlayer(character);
		return true;
	}

	private bool ActivateSuicide(Character p_actor)
	{
		p_actor.behaviourComponent.StartNonInstantCriticalBreak();
		return true;
	}

	private bool ActivateBecomeBandit(Character p_actor)
	{
		p_actor.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, p_actor, "create_bandits");
		return true;
	}

	private bool ActivateTurnEvil(Character p_actor)
	{
		if (p_actor.traitContainer.AddTrait(p_actor, "Evil"))
		{
			PlayerManager.Instance.player.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_MAKE_VILLAGER_EVIL);
			return true;
		}
		return false;
	}

	private bool ActivateDestroyStructure(Character p_actor, object p_target)
	{
		ManMadeStructure p_structure = p_target as ManMadeStructure;
		p_actor.behaviourComponent.StartNonInstantCriticalBreak(p_structure);
		return true;
	}

	public void SpawnChaosOrbs(Character p_actor, int p_amount, LocationGridTile p_tile)
	{
		if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.CRITICAL_BREAK).TryDecreaseRemainingChaosOrbs(ref p_amount))
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_tile.centeredWorldLocation, p_amount, p_tile.parentMap);
		}
	}
}
