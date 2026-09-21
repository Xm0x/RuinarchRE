using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

namespace Traits;

public class LycanthropeData
{
	public Character activeForm { get; private set; }

	public Character limboForm { get; private set; }

	public Character lycanthropeForm { get; private set; }

	public Character originalForm { get; private set; }

	public bool dislikesBeingLycan { get; private set; }

	public bool isMaster { get; private set; }

	public List<Character> awareCharacters { get; private set; }

	public bool isInWerewolfForm { get; private set; }

	public Character plainWolf { get; private set; }

	public Character direWolf { get; private set; }

	public LycanthropeData(Character originalForm)
	{
		this.originalForm = originalForm;
		isMaster = false;
		CreatePlainWolfForm();
		UpdateLycanForm();
		activeForm = originalForm;
		limboForm = lycanthropeForm;
		originalForm.SetLycanthropeData(this);
		originalForm.traitContainer.AddTrait(originalForm, "Lycanthrope");
		awareCharacters = new List<Character>();
		DetermineIfDesireOrDislike(originalForm);
		Messenger.AddListener<SkillData>("LycanthropyLevelUp", OnLycanthropyLevelUp);
	}

	public LycanthropeData(Character originalForm, Character lycanthropeForm, Character activeForm, Character limboForm, Character plainWolf, Character direWolf)
	{
		this.originalForm = originalForm;
		this.lycanthropeForm = lycanthropeForm;
		this.activeForm = activeForm;
		this.limboForm = limboForm;
		this.plainWolf = plainWolf;
		this.direWolf = direWolf;
		originalForm.SetLycanthropeData(this);
		plainWolf?.SetLycanthropeData(this);
		direWolf?.SetLycanthropeData(this);
		Messenger.AddListener<SkillData>("LycanthropyLevelUp", OnLycanthropyLevelUp);
	}

	private void CreatePlainWolfForm()
	{
		plainWolf = CharacterManager.Instance.CreateNewLimboSummon(SUMMON_TYPE.Wolf, FactionManager.Instance.wildMonsterFaction);
		plainWolf.ConstructInitialGoapAdvertisementActions();
		plainWolf.SetFirstName(originalForm.name);
		plainWolf.SetLycanthropeData(this);
		plainWolf.traitContainer.AddTrait(plainWolf, "Lycanthrope");
	}

	private void CreateDireWolfForm()
	{
		direWolf = CharacterManager.Instance.CreateNewLimboSummon(SUMMON_TYPE.Dire_Wolf, FactionManager.Instance.wildMonsterFaction);
		direWolf.ConstructInitialGoapAdvertisementActions();
		direWolf.SetFirstName(originalForm.name);
		direWolf.SetLycanthropeData(this);
		direWolf.traitContainer.AddTrait(direWolf, "Lycanthrope");
	}

	private void UpdateLycanForm()
	{
		lycanthropeForm = plainWolf;
		AfflictData afflictionData = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.LYCANTHROPY);
		if (afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Transform_Master_Werewolf))
		{
			SetIsMaster(p_state: true);
		}
		if (afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Becomes_Stronger_Dire_Wolf))
		{
			if (direWolf == null)
			{
				CreateDireWolfForm();
			}
			lycanthropeForm = direWolf;
		}
	}

	private void UpdateLycanFormName()
	{
		lycanthropeForm.SetFirstName(originalForm.name);
	}

	public void Transform(Character character)
	{
		if (character == originalForm)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Transform_To_Wolf, character);
		}
		else if (character == lycanthropeForm)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Revert_To_Normal, character);
		}
	}

	public void TurnToWolf()
	{
		if (UIManager.Instance.characterInfoUI.activeCharacter == activeForm)
		{
			UIManager.Instance.characterInfoUI.CloseMenu();
		}
		if (UIManager.Instance.monsterInfoUI.activeMonster == activeForm)
		{
			UIManager.Instance.monsterInfoUI.CloseMenu();
		}
		UpdateLycanForm();
		activeForm.traitContainer.RemoveTrait(activeForm, "Transforming");
		activeForm = lycanthropeForm;
		limboForm = originalForm;
		LocationGridTile gridTileLocation = originalForm.gridTileLocation;
		Region homeRegion = originalForm.homeRegion;
		PutToLimbo(originalForm);
		UpdateLycanFormName();
		ReleaseFromLimbo(lycanthropeForm, gridTileLocation, homeRegion);
		CopyImportantTraits(originalForm, lycanthropeForm);
		lycanthropeForm.needsComponent.ResetFullnessMeter();
		lycanthropeForm.needsComponent.ResetTirednessMeter();
		lycanthropeForm.needsComponent.ResetHappinessMeter();
		lycanthropeForm.needsComponent.ResetStaminaMeter();
		lycanthropeForm.needsComponent.ResetHopeMeter();
		lycanthropeForm.traitContainer.AddTrait(lycanthropeForm, "Transitioning");
		if (UIManager.Instance.IsContextMenuShowingForTarget(originalForm))
		{
			UIManager.Instance.RefreshPlayerActionContextMenuWithNewTarget(lycanthropeForm);
		}
		Messenger.Broadcast(CharacterSignals.ON_SWITCH_FROM_LIMBO, originalForm, lycanthropeForm);
		activeForm.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(activeForm);
	}

	public void RevertToNormal()
	{
		if (UIManager.Instance.characterInfoUI.activeCharacter == activeForm)
		{
			UIManager.Instance.characterInfoUI.CloseMenu();
		}
		if (UIManager.Instance.monsterInfoUI.activeMonster == activeForm)
		{
			UIManager.Instance.monsterInfoUI.CloseMenu();
		}
		activeForm.traitContainer.RemoveTrait(activeForm, "Transforming");
		activeForm = originalForm;
		limboForm = lycanthropeForm;
		LocationGridTile gridTileLocation = lycanthropeForm.gridTileLocation;
		Region homeRegion = lycanthropeForm.homeRegion;
		UpdateLycanFormName();
		PutToLimbo(lycanthropeForm);
		ReleaseFromLimbo(originalForm, gridTileLocation, homeRegion);
		CopyImportantTraits(lycanthropeForm, originalForm);
		lycanthropeForm.traitContainer.RemoveTrait(lycanthropeForm, "Transitioning");
		if (UIManager.Instance.IsContextMenuShowingForTarget(lycanthropeForm))
		{
			UIManager.Instance.RefreshPlayerActionContextMenuWithNewTarget(originalForm);
		}
		Messenger.Broadcast(CharacterSignals.ON_SWITCH_FROM_LIMBO, lycanthropeForm, originalForm);
		activeForm.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(activeForm);
	}

	private void CopyImportantTraits(Character p_copyFrom, Character p_copyTo)
	{
		if (p_copyFrom.traitContainer.HasTrait("Restrained"))
		{
			Trait traitOrStatus = p_copyFrom.traitContainer.GetTraitOrStatus<Trait>("Restrained");
			TraitManager.Instance.CopyTraitOrStatus(traitOrStatus, p_copyFrom, p_copyTo);
		}
		if (p_copyFrom.traitContainer.HasTrait("Prisoner"))
		{
			Trait traitOrStatus2 = p_copyFrom.traitContainer.GetTraitOrStatus<Trait>("Prisoner");
			TraitManager.Instance.CopyTraitOrStatus(traitOrStatus2, p_copyFrom, p_copyTo);
		}
	}

	private void PutToLimbo(Character form)
	{
		CharacterManager.Instance.PutCharacterIntoLimbo(form);
		form.currentRegion?.RemoveCharacterFromLocation(form);
		form.homeRegion?.RemoveResident(form);
	}

	private void ReleaseFromLimbo(Character form, LocationGridTile tileLocation, Region homeRegion)
	{
		homeRegion?.AddResident(form);
		CharacterManager.Instance.ReleaseCharacterFromLimbo(form, tileLocation);
		form.needsComponent.CheckExtremeNeeds();
	}

	public void EraseThisDataWhenTraitIsRemoved(Character form)
	{
		if (form == activeForm)
		{
			if (form == lycanthropeForm)
			{
				originalForm.traitContainer.RemoveTrait(originalForm, "Lycanthrope");
				RevertToNormal();
			}
			plainWolf?.homeSettlement?.RemoveResident(plainWolf);
			plainWolf?.faction?.LeaveFaction(plainWolf);
			direWolf?.homeSettlement?.RemoveResident(direWolf);
			direWolf?.faction?.LeaveFaction(direWolf);
			if (plainWolf != null)
			{
				CharacterManager.Instance.RemoveLimboCharacter(plainWolf);
			}
			if (direWolf != null)
			{
				CharacterManager.Instance.RemoveLimboCharacter(direWolf);
			}
			originalForm.SetLycanthropeData(null);
			plainWolf?.SetLycanthropeData(null);
			direWolf?.SetLycanthropeData(null);
			Messenger.RemoveListener<SkillData>("LycanthropyLevelUp", OnLycanthropyLevelUp);
		}
	}

	public void LycanDies(Character form, string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] multipleDeathLogFillers = null, LogFiller p_singleDeathLogFillers = null, object deathSource = null)
	{
		if (form == activeForm)
		{
			originalForm.traitContainer.RemoveTrait(originalForm, "Lycanthrope");
			if (form == lycanthropeForm)
			{
				RevertToNormal();
				originalForm.SetLycanthropeData(null);
				plainWolf?.SetLycanthropeData(null);
				direWolf?.SetLycanthropeData(null);
				Messenger.RemoveListener<SkillData>("LycanthropyLevelUp", OnLycanthropyLevelUp);
				originalForm.Death(cause, deathFromAction, responsibleCharacter, _deathLog, multipleDeathLogFillers, p_singleDeathLogFillers, null, isPlayerSource: false, deathSource);
			}
			if (isMaster)
			{
				PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<WolfClanEvent>().AdjustNumberOfAliveMasterLycan(-1);
			}
		}
	}

	private void OnLycanthropyLevelUp(SkillData p_skill)
	{
		if (p_skill.currentLevel >= 2)
		{
			SetIsMaster(p_state: true);
		}
	}

	public void SetIsInWerewolfForm(bool state)
	{
		isInWerewolfForm = state;
	}

	public bool CanTransformIntoWerewolf()
	{
		if (!activeForm.traitContainer.HasTrait("Polymorphed"))
		{
			return !activeForm.mountComponent.IsMounting();
		}
		return false;
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		awareCharacters.Remove(p_character);
	}

	public void SetDislikesBeingLycan(bool state)
	{
		dislikesBeingLycan = state;
	}

	public void SetIsMaster(bool p_state)
	{
		if (isMaster != p_state)
		{
			isMaster = p_state;
			if (isMaster)
			{
				PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<WolfClanEvent>().AdjustNumberOfAliveMasterLycan(1);
			}
			else
			{
				PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<WolfClanEvent>().AdjustNumberOfAliveMasterLycan(-1);
			}
		}
	}

	private void DetermineIfDesireOrDislike(Character character)
	{
		if (character.traitContainer.HasTrait("Lycanphobic", "Chaste"))
		{
			SetDislikesBeingLycan(state: true);
		}
		else if (character.traitContainer.HasTrait("Lycanphiliac"))
		{
			SetDislikesBeingLycan(state: false);
		}
		else if (character.traitContainer.HasTrait("Demon Cultist") && GameUtilities.RollChance(75))
		{
			SetDislikesBeingLycan(state: false);
		}
		else if (character.characterClass.className == "Hero" && GameUtilities.RollChance(75))
		{
			SetDislikesBeingLycan(state: true);
		}
		else if (character.characterClass.className == "Shaman" && GameUtilities.RollChance(80))
		{
			SetDislikesBeingLycan(state: true);
		}
		else if (character.traitContainer.HasTrait("Vampire") && GameUtilities.RollChance(80))
		{
			SetDislikesBeingLycan(state: true);
		}
		else if (character.traitContainer.HasTrait("Evil", "Treacherous") && GameUtilities.RollChance(75))
		{
			SetDislikesBeingLycan(state: false);
		}
		else if (character.race == RACE.ELVES && GameUtilities.RollChance(75))
		{
			SetDislikesBeingLycan(state: false);
		}
		else
		{
			SetDislikesBeingLycan(GameUtilities.RollChance(50));
		}
	}

	public void LoadIsMaster(bool p_isMaster)
	{
		isMaster = p_isMaster;
	}

	public void LoadDislikesBeingLycan(bool p_dislikesBeingLycan)
	{
		dislikesBeingLycan = p_dislikesBeingLycan;
	}

	public void LoadIsInWerewolfForm(bool p_state)
	{
		isInWerewolfForm = p_state;
	}

	public void AddAwareCharacter(Character character)
	{
		if (!awareCharacters.Contains(character))
		{
			awareCharacters.Add(character);
			if (character.traitContainer.HasTrait("Lycanphiliac"))
			{
				character.traitContainer.GetTraitOrStatus<Lycanphiliac>("Lycanphiliac").OnBecomeAwareOfLycan(originalForm);
			}
			else if (character.traitContainer.HasTrait("Lycanphobic"))
			{
				character.traitContainer.GetTraitOrStatus<Lycanphobic>("Lycanphobic").OnBecomeAwareOfLycan(originalForm);
			}
		}
	}

	public void LoadAwareCharacters(List<Character> characters)
	{
		awareCharacters = new List<Character>();
		if (characters.Count > 0)
		{
			awareCharacters.AddRange(characters);
		}
	}

	public bool DoesCharacterKnowThisLycan(Character character)
	{
		return awareCharacters.Contains(character);
	}

	public bool DoesFactionKnowThisLycan(Faction faction, bool includeDeadMembersInChecking = true)
	{
		for (int i = 0; i < faction.characters.Count; i++)
		{
			Character character = faction.characters[i];
			if (character != originalForm && (includeDeadMembersInChecking || !character.isDead) && DoesCharacterKnowThisLycan(character))
			{
				return true;
			}
		}
		return false;
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = activeForm;
		_ = limboForm;
		_ = lycanthropeForm;
		_ = originalForm;
		awareCharacters.Contains(p_character);
		_ = plainWolf;
		_ = direWolf;
	}
}
