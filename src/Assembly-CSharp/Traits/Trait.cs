using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Logs;
using Maccima_Games.Util;
using Object_Pools;
using UnityEngine;
using UtilityScripts;

namespace Traits;

[Serializable]
public class Trait : IMoodModifier, ISavable, IContextMenuItem
{
	public string name;

	public string description;

	public string thoughtText;

	public TRAIT_TYPE type;

	public TRAIT_EFFECT effect;

	public List<INTERACTION_TYPE> advertisedInteractions;

	public int ticksDuration;

	public int moodEffect;

	public bool isHidden;

	public string[] mutuallyExclusive;

	public bool canBeTriggered;

	public ELEMENTAL_TYPE elementalType;

	public List<RESISTANCE> resistancesType;

	public List<float> resistancesValue;

	public List<string> traitOverrideFunctionIdentifiers { get; protected set; }

	public string localizedName => LocalizationManager.Instance.GetLocalizedValue("Traits_Table", name);

	public string localizedDescription => LocalizationManager.Instance.GetLocalizedValue("Traits_Table", name + "_Description") ?? "";

	public string persistentID { get; private set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Trait;

	public List<Character> responsibleCharacters { get; protected set; }

	public INTERACTION_TYPE gainedFromDoingType { get; protected set; }

	public bool isGainedFromDoingStealth { get; protected set; }

	public virtual Type serializedData => typeof(SaveDataTrait);

	public Character responsibleCharacter => responsibleCharacters?.FirstOrDefault();

	public string modifierName => name;

	public int moodModifier => moodEffect;

	public string descriptionInUI => GetDescriptionInUI();

	public virtual bool isPersistent => false;

	public virtual bool isSingleton => false;

	public virtual bool shouldBeLoadedInMainThread => false;

	public Sprite contextMenuIcon => null;

	public string contextMenuName => localizedName;

	public int contextMenuColumn => 1;

	public List<IContextMenuItem> subMenus => null;

	public virtual bool affectsNameIcon => false;

	public void InitializeInstancedTrait()
	{
		persistentID = Utilities.GetNewUniqueID();
		DatabaseManager.Instance.traitDatabase.RegisterTrait(this);
	}

	public virtual void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		persistentID = saveDataTrait.persistentID;
		gainedFromDoingType = saveDataTrait.gainedFromDoingType;
		isGainedFromDoingStealth = saveDataTrait.isGainedFromDoingStealth;
		DatabaseManager.Instance.traitDatabase.RegisterTrait(this);
	}

	public virtual void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		if (p_saveDataTrait.responsibleCharacters != null)
		{
			responsibleCharacters = SaveUtilities.ConvertIDListToCharacters(p_saveDataTrait.responsibleCharacters);
		}
	}

	public virtual void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
	}

	public virtual void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
	}

	public void ApplyMoodEffects(ITraitable addedTo, GameDate expiryDate, Character characterResponsible)
	{
		if (addedTo is Character character && moodEffect != 0)
		{
			character.moodComponent.AddMoodEffect(moodEffect, this, expiryDate, characterResponsible);
		}
	}

	public void UnapplyMoodEffects(ITraitable removedFrom)
	{
		if (removedFrom is Character character && moodEffect != 0)
		{
			character.moodComponent.RemoveMoodEffect(-moodEffect, this);
		}
	}

	public virtual void OnAddTrait(ITraitable addedTo)
	{
		if (!(addedTo is Character))
		{
			return;
		}
		Character character = addedTo as Character;
		if (elementalType != ELEMENTAL_TYPE.Normal)
		{
			if (!character.equipmentComponent.HasEquips())
			{
				character.combatComponent.SetElementalType(elementalType);
			}
			character.combatComponent.elementalStatusWaitingList.Add(elementalType);
		}
	}

	public virtual void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		if (removedFrom is Character)
		{
			Character character = removedFrom as Character;
			if (elementalType != ELEMENTAL_TYPE.Normal)
			{
				character.combatComponent.elementalStatusWaitingList.Remove(elementalType);
				character.combatComponent.UpdateElementalType();
			}
		}
		if (TraitManager.Instance.IsInstancedTrait(name))
		{
			DatabaseManager.Instance.traitDatabase.UnRegisterTrait(this);
		}
	}

	public virtual bool OnDeath(Character character)
	{
		return false;
	}

	public virtual string GetTestingData(ITraitable traitable = null)
	{
		string text = string.Empty;
		if (responsibleCharacters != null)
		{
			text = "Responsible Characters: " + responsibleCharacters.ComafyList() + "\n";
		}
		return text + "Is gained from stealth: " + isGainedFromDoingStealth;
	}

	public virtual bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob)
	{
		return false;
	}

	public virtual bool OnCollideWith(IPointOfInterest collidedWith, IPointOfInterest owner)
	{
		return false;
	}

	public virtual void OnEnterGridTile(IPointOfInterest poiWhoEntered, IPointOfInterest owner)
	{
	}

	public virtual void OnInitiateMapObjectVisual(ITraitable traitable)
	{
	}

	public virtual void OnDestroyMapObjectVisual(ITraitable traitable)
	{
	}

	public virtual bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		return false;
	}

	public virtual void OnSeePOIEvenCannotWitness(IPointOfInterest targetPOI, Character character)
	{
	}

	public virtual void OnOwnerInitiallyPlaced(Character owner)
	{
	}

	public virtual bool PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		return false;
	}

	public virtual bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction)
	{
		return false;
	}

	public virtual void OnBeforeStartFlee(ITraitable traitable)
	{
	}

	public virtual void OnAfterExitingCombat(ITraitable traitable)
	{
	}

	public virtual void OnChangeElement(Character p_owner, ELEMENTAL_TYPE p_newElement, ELEMENTAL_TYPE p_oldElement)
	{
	}

	public virtual string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		if (character.trapStructure.IsTrapped())
		{
			character.trapStructure.ResetAllTrapStructures();
		}
		if (character.trapStructure.IsTrappedInArea())
		{
			character.trapStructure.ResetTrapArea();
		}
		return "flaw_effect";
	}

	public virtual bool CanFlawBeTriggered(Character character)
	{
		PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.TRIGGER_FLAW);
		int manaCost = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.TRIGGER_FLAW).manaCost;
		if (canBeTriggered && PlayerManager.Instance.player.currenciesComponent.mana >= manaCost && character.limiterComponent.canPerform && !character.traitContainer.IsBlessed())
		{
			return !character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld;
		}
		return false;
	}

	public virtual void OnTickStarted(ITraitable traitable)
	{
	}

	public virtual void OnTickEnded(ITraitable traitable)
	{
	}

	public virtual void OnHourStarted(ITraitable traitable)
	{
	}

	public virtual string GetNameInUI(ITraitable traitable)
	{
		return localizedName;
	}

	protected virtual string GetDescriptionInUI()
	{
		return localizedDescription;
	}

	public virtual void AfterDeath(Character character)
	{
	}

	public virtual string GetTriggerFlawEffectDescription(Character character, string key)
	{
		if (LocalizationManager.Instance.HasLocalizedValue("TraitTriggerFlaw_Table", name + " " + key))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "TraitTriggerFlaw_Table", name + " " + key, LOG_TAG.Player);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.FinalizeText();
			string logText = log.logText;
			LogPool.Release(log);
			return logText;
		}
		return string.Empty;
	}

	public void SetGainedFromDoingAction(INTERACTION_TYPE p_actionType, bool p_actionStealth)
	{
		gainedFromDoingType = p_actionType;
		isGainedFromDoingStealth = p_actionStealth;
		Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE, JOB_TYPE.REMOVE_STATUS);
	}

	public virtual void AddCharacterResponsibleForTrait(Character character)
	{
		if (character != null && !character.hasBeenCleanedUp)
		{
			if (responsibleCharacters == null)
			{
				responsibleCharacters = new List<Character>();
			}
			if (!responsibleCharacters.Contains(character))
			{
				responsibleCharacters.Add(character);
			}
		}
	}

	public void ClearResponsibleCharacters()
	{
		if (responsibleCharacters != null)
		{
			responsibleCharacters.Clear();
		}
	}

	public bool IsResponsibleForTrait(Character character)
	{
		if (character == null)
		{
			return false;
		}
		if (responsibleCharacter == character)
		{
			return true;
		}
		if (responsibleCharacters != null)
		{
			return responsibleCharacters.Contains(character);
		}
		return false;
	}

	public Trait GetBase()
	{
		return this;
	}

	public void AddTraitOverrideFunctionIdentifier(string identifier)
	{
		if (traitOverrideFunctionIdentifiers == null)
		{
			traitOverrideFunctionIdentifiers = new List<string>();
		}
		if (!traitOverrideFunctionIdentifiers.Contains(identifier))
		{
			traitOverrideFunctionIdentifiers.Add(identifier);
		}
	}

	public bool IsNeeds()
	{
		if (!(name == "Refreshed") && !(name == "Exhausted") && !(name == "Tired") && !(name == "Entertained") && !(name == "Bored") && !(name == "Sulking") && !(name == "Full") && !(name == "Hungry") && !(name == "Starving") && !(name == "Sprightly") && !(name == "Spent"))
		{
			return name == "Drained";
		}
		return true;
	}

	public PLAYER_SKILL_TYPE GetAfflictionSkillType()
	{
		return PlayerSkillManager.Instance.GetAfflictionTypeByTraitName(name);
	}

	public virtual void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost)
	{
	}

	public virtual void ExecuteActionPreEffects(INTERACTION_TYPE action, ActualGoapNode goapNode)
	{
	}

	public virtual void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode)
	{
	}

	public virtual void ExecuteActionAfterEffects(INTERACTION_TYPE action, Character actor, IPointOfInterest target, ACTION_CATEGORY category, ref bool isRemoved)
	{
	}

	public void OnPickAction()
	{
		if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character p_character && UIManager.Instance.contextMenuUIController.currentlyOpenedParentContextItem is PlayerAction playerAction)
		{
			if (playerAction.type == PLAYER_SKILL_TYPE.TRIGGER_FLAW)
			{
				ActivateTriggerFlawConfirmation(p_character);
			}
			else if (playerAction.type == PLAYER_SKILL_TYPE.REMOVE_BUFF)
			{
				(playerAction as RemoveBuffData).ActivateRemoveBuff(name, p_character);
			}
			else if (playerAction.type == PLAYER_SKILL_TYPE.REMOVE_FLAW)
			{
				(playerAction as RemoveFlawData).ActivateRemoveFlaw(name, p_character);
			}
		}
	}

	public bool CanBePickedRegardlessOfCooldown()
	{
		if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character character && UIManager.Instance.contextMenuUIController.currentlyOpenedParentContextItem is PlayerAction playerAction)
		{
			if (playerAction.type == PLAYER_SKILL_TYPE.TRIGGER_FLAW)
			{
				return CanFlawBeTriggered(character);
			}
			string cannotRemoveReason;
			if (playerAction.type == PLAYER_SKILL_TYPE.REMOVE_BUFF || playerAction.type == PLAYER_SKILL_TYPE.REMOVE_FLAW)
			{
				return CanBuffOrFlawBeRemoved(character, out cannotRemoveReason);
			}
		}
		return true;
	}

	public bool CanBuffOrFlawBeRemoved(Character targetCharacter, out string cannotRemoveReason)
	{
		if (targetCharacter.traitContainer.IsBlessed() && name != "Blessed")
		{
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("targetName", targetCharacter.visuals.GetCharacterNameWithIconAndColor());
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Remove_Buff_Blessed", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			cannotRemoveReason = localizedValue;
			return false;
		}
		if (this is Nullchild)
		{
			cannotRemoveReason = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Remove_Buff_Nullchild");
			return false;
		}
		if (targetCharacter.isDead)
		{
			cannotRemoveReason = string.Empty;
			return false;
		}
		cannotRemoveReason = string.Empty;
		return true;
	}

	public bool IsInCooldown()
	{
		return false;
	}

	public float GetCoverFillAmount()
	{
		return 0f;
	}

	public int GetCurrentRemainingCooldownTicks()
	{
		return 0;
	}

	private void ActivateTriggerFlawConfirmation(Character p_character)
	{
		string traitName = name;
		Trait trait = p_character.traitContainer.GetTraitOrStatus<Trait>(traitName);
		string question;
		string text;
		if (p_character.isInfoUnlocked)
		{
			question = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Flaw_Confirmation") + " " + trait.localizedName + "?";
			text = "<b>" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Effect_Title") + "</b>: " + trait.GetTriggerFlawEffectDescription(p_character, "flaw_effect");
		}
		else
		{
			question = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Flaw_Confirmation") + " ??????";
			text = "<b>" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Effect_Title") + "</b>: ?????";
		}
		string manaCost = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.TRIGGER_FLAW).manaCost + " " + Utilities.ManaIcon();
		UIManager.Instance.ShowTriggerFlawConfirmation(question, text, manaCost, delegate
		{
			TriggerFlawData.ActivateTriggerFlaw(trait, p_character);
		}, showCover: true, 26, pauseAndResume: true);
	}

	public int GetManaCost()
	{
		if (UIManager.Instance.contextMenuUIController.currentlyOpenedParentContextItem is PlayerAction playerAction && (playerAction.type == PLAYER_SKILL_TYPE.REMOVE_BUFF || playerAction.type == PLAYER_SKILL_TYPE.REMOVE_FLAW))
		{
			return 0;
		}
		return PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.TRIGGER_FLAW).manaCost;
	}

	public Log GetMoodEffectFlavorText(Character p_characterResponsible)
	{
		if (LocalizationManager.Instance.HasLocalizedValue("TraitMood_Table", name + " mood_effect"))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "TraitMood_Table", name + " mood_effect");
			GetMoodEffectFlavorTextActiveCharacterFiller(out var p_obj, out var p_name);
			if (p_obj == null && string.IsNullOrEmpty(p_name))
			{
				p_obj = p_characterResponsible;
				p_name = p_characterResponsible?.name;
			}
			if (p_obj != null || !string.IsNullOrEmpty(p_name))
			{
				log.AddToFillers(p_obj, p_name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			}
			else
			{
				log.AddToFillers(null, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Demons"), LOG_IDENTIFIER.ACTIVE_CHARACTER);
			}
			return log;
		}
		return null;
	}

	protected virtual void GetMoodEffectFlavorTextActiveCharacterFiller(out ILogFiller p_obj, out string p_name)
	{
		p_obj = null;
		p_name = string.Empty;
	}

	public void DispenseChaosOrbsForAffliction(Character p_character, PLAYER_SKILL_TYPE p_afflictionType, int p_amount)
	{
		LocationGridTile locationGridTile = p_character.gridTileLocation;
		if (p_character.isDead)
		{
			locationGridTile = p_character.deathTilePosition;
		}
		if (locationGridTile != null && PlayerSkillManager.Instance.GetSkillData(p_afflictionType).TryDecreaseRemainingChaosOrbs(ref p_amount))
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, locationGridTile.centeredWorldLocation, p_amount, locationGridTile.parentMap);
		}
	}

	public virtual void VillagerReactionToTileObjectTrait(TileObject owner, Character actor, ref string debugLog)
	{
	}

	protected void UpdateCharacterAwarenessStateOnAddTrait(Character p_character)
	{
		if (!p_character.hasBeenCleanedUp)
		{
			Messenger.Broadcast(CharacterSignals.UPDATE_CHARACTER_AWARENESS_STATE, p_character);
		}
	}

	protected void UpdateCharacterAwarenessStateOnRemoveTrait(Character p_character, Character p_removedBy)
	{
		if (!p_character.hasBeenCleanedUp)
		{
			LocationGridTile gridTileLocation = p_character.gridTileLocation;
			if (gridTileLocation != null && gridTileLocation.IsPartOfSettlement(out var settlement) && settlement.owner != null)
			{
				Messenger.Broadcast(CharacterSignals.UPDATE_CHARACTER_AWARENESS_STATE, p_character);
			}
			else if (p_removedBy != null && p_removedBy.faction != null)
			{
				p_removedBy.faction.charactersComponent.UpdateCharacterAwarenessData(p_character);
			}
		}
	}

	public override string ToString()
	{
		return name;
	}

	public virtual void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public virtual void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		if (responsibleCharacters != null)
		{
			responsibleCharacters.Contains(p_character);
		}
	}

	public virtual void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		if (responsibleCharacters != null)
		{
			responsibleCharacters.Remove(p_character);
		}
	}

	public virtual void CleanUp()
	{
		responsibleCharacters?.Clear();
	}
}
