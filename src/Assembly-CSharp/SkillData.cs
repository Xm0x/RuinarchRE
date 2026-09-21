using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Maccima_Games.Util;
using Object_Pools;
using Traits;
using UnityEngine;
using UtilityScripts;

public class SkillData : IPlayerSkill
{
	public const int MAX_SPELL_LEVEL = 3;

	private string _localizedDescriptionKey = string.Empty;

	private string _currencyUIText = string.Empty;

	private string _currencyLevelUpUIText = string.Empty;

	private string _chargesNotCombinedUIText = string.Empty;

	private string _chargesCombinedUIText = string.Empty;

	private string _chargesCombinedIconFirstUIText = string.Empty;

	private string _maxChargesUIText = string.Empty;

	private string _maxChargesLevelUpUIText = string.Empty;

	public SPELL_TARGET[] targetTypes { get; protected set; }

	public int charges { get; private set; }

	public bool isInUse { get; private set; }

	public bool isTemporarilyInUse { get; private set; }

	public int currentCooldownTick { get; private set; }

	public int baseMaxCharges { get; private set; }

	public int baseManaCost { get; private set; }

	public int baseSpiritEnergyCost { get; private set; }

	public float basePierce { get; private set; }

	public int baseCooldown { get; private set; }

	public int bonusCharges { get; private set; }

	public int currentLevel { get; set; }

	public int remainingChaosOrbs { get; private set; }

	public bool isUnlockedBaseOnRequirements { get; set; }

	public bool isUsable { get; private set; }

	public string characterThatLockedSkillID { get; private set; }

	public virtual string localizedName => LocalizationManager.Instance.GetLocalizedValue("PlayerPowers_Table", name);

	public virtual string localizedDescription => GetLocalizedDescription();

	public int unlockCost { get; set; }

	public SkillEventDispatcher skillEventDispatcher { get; }

	public virtual PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.NONE;

	public virtual string name => string.Empty;

	public virtual string description => string.Empty;

	public virtual PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.NONE;

	public int maxCharges => baseMaxCharges;

	public int manaCost => baseManaCost;

	public int cooldown => baseCooldown;

	public int spiritEnergyCost => baseSpiritEnergyCost;

	public int threat => 0;

	public virtual int radius => 0;

	public int levelForDisplay => currentLevel + 1;

	public bool isMaxLevel => currentLevel >= 3;

	public bool hasCharges => baseMaxCharges != -1;

	public bool hasCooldown => baseCooldown != -1;

	public bool hasManaCost => baseManaCost != -1;

	public bool hasSpiritEnergyCost => baseSpiritEnergyCost != -1;

	public bool hasBonusCharges => bonusCharges > 0;

	public bool hasRemainingOrUnliChaosOrbs
	{
		get
		{
			if (!hasUnliChaosOrbs)
			{
				return hasRemainingChaosOrbs;
			}
			return true;
		}
	}

	public bool hasUnliChaosOrbs => remainingChaosOrbs == -1;

	public bool hasRemainingChaosOrbs => remainingChaosOrbs > 0;

	public int totalCharges => charges + bonusCharges;

	public virtual bool isInCooldown
	{
		get
		{
			if (hasCooldown)
			{
				return currentCooldownTick < cooldown;
			}
			return false;
		}
	}

	protected SkillData()
	{
		_localizedDescriptionKey = name + "_Description";
		ResetData();
	}

	public void LevelUp()
	{
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(type);
		currentLevel = Mathf.Clamp(++currentLevel, 0, 3);
		SetManaCost(scriptableObjPlayerSkillData.GetManaCostBaseOnLevel(currentLevel));
		SetBaseSpiritEnergyCost(scriptableObjPlayerSkillData.GetSpiritEnergyCostBaseOnLevel(currentLevel));
		SetMaxCharges(scriptableObjPlayerSkillData.GetMaxChargesBaseOnLevel(currentLevel));
		SetPierce(PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(type));
		SetCooldown(scriptableObjPlayerSkillData.GetCoolDownBaseOnLevel(currentLevel));
		SetCharges(maxCharges);
		FinishCooldown();
		ResetCurrencyUIText();
		ResetAllChargesUIText();
		scriptableObjPlayerSkillData.ResetAllBonusUIText();
		OnLevelUp();
		if (category == PLAYER_SKILL_CATEGORY.AFFLICTION)
		{
			Messenger.Broadcast(name + "LevelUp", this);
		}
		Messenger.Broadcast(PlayerSkillSignals.UPDATE_PLAYER_SKILL, this);
	}

	protected virtual string GetLocalizedDescription()
	{
		return LocalizationManager.Instance.GetLocalizedValue("PlayerPowers_Table", _localizedDescriptionKey) ?? "";
	}

	public virtual void OnSetAsCurrentActiveSpell()
	{
	}

	public virtual void OnNoLongerCurrentActiveSpell()
	{
	}

	public virtual void ActivateAbility(IPointOfInterest targetPOI)
	{
		OnExecutePlayerSkill();
	}

	public virtual void ActivateAbility(LocationGridTile targetTile)
	{
		OnExecutePlayerSkill();
	}

	public virtual void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter)
	{
		OnExecutePlayerSkill();
	}

	public virtual void ActivateAbility(Area targetArea)
	{
		OnExecutePlayerSkill();
	}

	public virtual void ActivateAbility(LocationStructure targetStructure)
	{
		OnExecutePlayerSkill();
	}

	public virtual void ActivateAbility(StructureRoom room)
	{
		OnExecutePlayerSkill();
	}

	public virtual void ActivateAbility(BaseSettlement targetSettlement)
	{
		OnExecutePlayerSkill();
	}

	public virtual void ActivateAbility(int p_numberOfTimesToBeExecuted)
	{
		OnExecutePlayerSkill(p_numberOfTimesToBeExecuted);
	}

	public virtual string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = string.Empty;
		if (!isUsable)
		{
			text = AppendLockedReason(text);
		}
		LocationGridTile gridTileLocation = targetCharacter.gridTileLocation;
		if (gridTileLocation != null)
		{
			LocationStructure structureProtectingTile = gridTileLocation.GetStructureProtectingTile(radius);
			if (structureProtectingTile != null)
			{
				if (structureProtectingTile is MageTower)
				{
					return text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Mage_Tower_Blocked") + "|";
				}
				return text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Character_In_Protected_Structure") + "|";
			}
		}
		return text;
	}

	public virtual string GetReasonsWhyCannotPerformAbilityTowards(TileObject targetTileObject)
	{
		string text = string.Empty;
		if (!isUsable)
		{
			text = AppendLockedReason(text);
		}
		LocationGridTile gridTileLocation = targetTileObject.gridTileLocation;
		if (gridTileLocation != null)
		{
			LocationStructure structureProtectingTile = gridTileLocation.GetStructureProtectingTile(radius);
			if (structureProtectingTile != null)
			{
				if (structureProtectingTile is MageTower)
				{
					return text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Mage_Tower_Blocked") + "|";
				}
				return text + GetLocalizedReasonWhyCannotPerformAbilityTowards("TileObject_In_Protected_Structure") + "|";
			}
		}
		return text;
	}

	public virtual string GetReasonsWhyCannotPerformAbilityTowards(BaseSettlement p_targetSettlement)
	{
		string text = string.Empty;
		if (!isUsable)
		{
			text = AppendLockedReason(text);
		}
		return text;
	}

	public virtual string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure p_targetStructure)
	{
		string text = string.Empty;
		if (!isUsable)
		{
			text = AppendLockedReason(text);
		}
		if (p_targetStructure.isProtected)
		{
			if (p_targetStructure is MageTower)
			{
				return text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Mage_Tower_Blocked") + "|";
			}
			return text + GetLocalizedReasonWhyCannotPerformAbilityTowards("In_Protected_Structure") + "|";
		}
		return text;
	}

	public virtual bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (targetCharacter.traitContainer.IsBlessed())
		{
			return false;
		}
		LocationGridTile gridTileLocation = targetCharacter.gridTileLocation;
		if (gridTileLocation != null && gridTileLocation.IsTileConsideredProtected(radius))
		{
			return false;
		}
		return CanPerformAbility();
	}

	public virtual bool CanPerformAbilityTowards(TileObject tileObject)
	{
		LocationGridTile gridTileLocation = tileObject.gridTileLocation;
		if (gridTileLocation != null && gridTileLocation.IsTileConsideredProtected(radius))
		{
			if (type == PLAYER_SKILL_TYPE.SNATCH_VILLAGER || type == PLAYER_SKILL_TYPE.SNATCH_OBJECT)
			{
				return CanPerformAbility();
			}
			return false;
		}
		return CanPerformAbility();
	}

	public virtual bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		o_cannotPerformReason = string.Empty;
		if (targetTile.IsTileConsideredProtected(radius))
		{
			if (type == PLAYER_SKILL_TYPE.SNATCH_VILLAGER || type == PLAYER_SKILL_TYPE.SNATCH_OBJECT)
			{
				return CanPerformAbility();
			}
			return false;
		}
		return CanPerformAbility();
	}

	public virtual bool CanPerformAbilityTowards(Area targetArea)
	{
		return CanPerformAbility();
	}

	public virtual bool CanPerformAbilityTowards(LocationStructure targetStructure)
	{
		if (targetStructure.isProtected)
		{
			return false;
		}
		return CanPerformAbility();
	}

	public virtual bool CanPerformAbilityTowards(StructureRoom room)
	{
		return CanPerformAbility();
	}

	public virtual bool CanPerformAbilityTowards(BaseSettlement targetSettlement)
	{
		return CanPerformAbility();
	}

	public virtual bool IsValid(IPlayerActionTarget target)
	{
		return true;
	}

	public virtual bool IsValid()
	{
		return true;
	}

	public virtual string GetReasonsWhyInvalid()
	{
		string text = string.Empty;
		if (!isUsable)
		{
			text = AppendLockedReason(text);
		}
		return text;
	}

	public virtual void ShowValidHighlight(LocationGridTile tile)
	{
	}

	public virtual void UnhighlightAffectedTiles()
	{
		TileHighlighter.Instance.HideHighlight();
	}

	public virtual bool ShowInvalidHighlight(LocationGridTile tile, ref string invalidText)
	{
		return false;
	}

	protected virtual void OnLevelUp()
	{
	}

	public virtual string GetBonusUIText()
	{
		return string.Empty;
	}

	public virtual string GetBonusLevelUpUIText()
	{
		return string.Empty;
	}

	public void ResetDataExceptLevel()
	{
		int num = currentLevel;
		ResetData();
		currentLevel = num;
	}

	public void ResetData()
	{
		charges = -1;
		baseManaCost = -1;
		baseCooldown = -1;
		baseMaxCharges = -1;
		baseSpiritEnergyCost = -1;
		currentCooldownTick = cooldown;
		currentLevel = 0;
		ResetIsInUse();
		isTemporarilyInUse = false;
		isUsable = true;
	}

	public bool CanPerformAbilityTowards(IPointOfInterest poi)
	{
		if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
		{
			return CanPerformAbilityTowards(poi as Character);
		}
		if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
		{
			return CanPerformAbilityTowards(poi as TileObject);
		}
		return CanPerformAbility();
	}

	public bool CanPerformAbility()
	{
		bool flag = (((!hasCharges || charges > 0) && isInUse) || hasBonusCharges) && (!hasManaCost || PlayerManager.Instance.player.currenciesComponent.mana >= manaCost) && (!hasSpiritEnergyCost || PlayerManager.Instance.player.currenciesComponent.spiritEnergy >= spiritEnergyCost) && isUsable;
		if (!flag)
		{
			if (type == PLAYER_SKILL_TYPE.SCHEME)
			{
				if (!isUsable)
				{
					return false;
				}
				return true;
			}
			if (type == PLAYER_SKILL_TYPE.RAID)
			{
				if (!isUsable)
				{
					return false;
				}
				return true;
			}
		}
		else if (category == PLAYER_SKILL_CATEGORY.SCHEME)
		{
			if (PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SCHEME).charges <= 0)
			{
				return false;
			}
		}
		else if (category == PLAYER_SKILL_CATEGORY.RAID)
		{
			return true;
		}
		return flag;
	}

	public bool CanTarget(IPointOfInterest poi, ref string hoverText)
	{
		if (poi.traitContainer.IsBlessed())
		{
			hoverText = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cannot_Target_Blessed");
			return false;
		}
		if (poi is Character && !(poi as Character).race.IsSapient())
		{
			return false;
		}
		hoverText = string.Empty;
		return CanPerformAbilityTowards(poi);
	}

	public bool CanTarget(LocationGridTile tile)
	{
		string o_cannotPerformReason;
		return CanPerformAbilityTowards(tile, out o_cannotPerformReason);
	}

	public bool CanTarget(LocationGridTile tile, out string p_cannotTargetText)
	{
		return CanPerformAbilityTowards(tile, out p_cannotTargetText);
	}

	public bool CanTarget(Area p_area)
	{
		return CanPerformAbilityTowards(p_area);
	}

	public bool CanTarget(BaseSettlement p_targetSettlement)
	{
		return CanPerformAbilityTowards(p_targetSettlement);
	}

	protected void IncreaseThreatForEveryCharacterThatSeesPOI(IPointOfInterest poi, int amount)
	{
		Messenger.Broadcast(CharacterSignals.INCREASE_THREAT_THAT_SEES_POI, poi, amount);
	}

	public void OnLoadSpell()
	{
		Messenger.Broadcast(PlayerSkillSignals.CHARGES_UPDATED, this, 0);
		if (hasCooldown && ((hasCharges && charges < maxCharges) || currentCooldownTick < cooldown))
		{
			Messenger.Broadcast(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, this);
			Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
		}
	}

	public void OnExecutePlayerSkill()
	{
		if (!PlayerSkillManager.Instance.unlimitedCast)
		{
			if (hasBonusCharges)
			{
				AdjustBonusCharges(-1);
			}
			else if (hasCharges)
			{
				if (charges > 0 && !WorldSettings.Instance.worldSettingsData.playerSkillSettings.PowerHasUnlimitedCharges(type))
				{
					AdjustCharges(-1);
				}
			}
			else if (category == PLAYER_SKILL_CATEGORY.SCHEME || category == PLAYER_SKILL_CATEGORY.RAID)
			{
				StartCooldown();
			}
			if (hasManaCost)
			{
				PlayerManager.Instance.player.currenciesComponent.AdjustMana(-manaCost);
			}
			if (hasSpiritEnergyCost)
			{
				PlayerManager.Instance.player.currenciesComponent.AdjustSpiritEnergy(-spiritEnergyCost);
			}
		}
		PlayerManager.Instance.player.threatComponent.AdjustThreat(threat);
		if (category == PLAYER_SKILL_CATEGORY.PLAYER_ACTION)
		{
			Messenger.Broadcast(PlayerSkillSignals.ON_EXECUTE_PLAYER_ACTION, this as PlayerAction);
		}
		else if (category == PLAYER_SKILL_CATEGORY.AFFLICTION)
		{
			Messenger.Broadcast(PlayerSkillSignals.ON_EXECUTE_AFFLICTION, this);
		}
		else
		{
			Messenger.Broadcast(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, this);
		}
	}

	public void OnExecutePlayerSkill(int p_numberOfTimesToExecute)
	{
		if (!PlayerSkillManager.Instance.unlimitedCast)
		{
			if (hasBonusCharges)
			{
				int num = bonusCharges;
				if (p_numberOfTimesToExecute < bonusCharges)
				{
					num = p_numberOfTimesToExecute;
				}
				AdjustBonusCharges(-num);
				int num2 = p_numberOfTimesToExecute - num;
				if (num2 > 0 && charges > 0 && !WorldSettings.Instance.worldSettingsData.playerSkillSettings.PowerHasUnlimitedCharges(type))
				{
					if (charges < num2)
					{
						num2 = charges;
					}
					AdjustCharges(-num2);
				}
			}
			else if (hasCharges)
			{
				if (charges > 0 && !WorldSettings.Instance.worldSettingsData.playerSkillSettings.PowerHasUnlimitedCharges(type))
				{
					if (charges < p_numberOfTimesToExecute)
					{
						p_numberOfTimesToExecute = charges;
					}
					AdjustCharges(-p_numberOfTimesToExecute);
				}
			}
			else if (category == PLAYER_SKILL_CATEGORY.SCHEME || category == PLAYER_SKILL_CATEGORY.RAID)
			{
				StartCooldown();
			}
			if (hasManaCost)
			{
				PlayerManager.Instance.player.currenciesComponent.AdjustMana(-manaCost * p_numberOfTimesToExecute);
			}
			if (hasSpiritEnergyCost)
			{
				PlayerManager.Instance.player.currenciesComponent.AdjustSpiritEnergy(-spiritEnergyCost * p_numberOfTimesToExecute);
			}
		}
		PlayerManager.Instance.player.threatComponent.AdjustThreat(threat * p_numberOfTimesToExecute);
		if (category == PLAYER_SKILL_CATEGORY.PLAYER_ACTION)
		{
			Messenger.Broadcast(PlayerSkillSignals.ON_EXECUTE_PLAYER_ACTION, this as PlayerAction);
		}
		else if (category == PLAYER_SKILL_CATEGORY.AFFLICTION)
		{
			Messenger.Broadcast(PlayerSkillSignals.ON_EXECUTE_AFFLICTION, this);
		}
		else
		{
			Messenger.Broadcast(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, this);
		}
	}

	public bool HasValidCharges()
	{
		if (PlayerSkillManager.Instance.unlimitedCast)
		{
			return true;
		}
		if (!hasCharges)
		{
			return true;
		}
		if (hasBonusCharges)
		{
			return true;
		}
		if (hasCharges)
		{
			if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.PowerHasUnlimitedCharges(type))
			{
				return true;
			}
			if (charges > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasValidCharges(int p_minimumAmount)
	{
		if (PlayerSkillManager.Instance.unlimitedCast)
		{
			return true;
		}
		if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.PowerHasUnlimitedCharges(type))
		{
			return true;
		}
		if (!hasCharges)
		{
			return true;
		}
		return bonusCharges + charges >= p_minimumAmount;
	}

	public bool HasEnoughSpiritEnergy(int p_minimumAmount)
	{
		if (PlayerSkillManager.Instance.unlimitedCast)
		{
			return true;
		}
		if (!hasSpiritEnergyCost)
		{
			return true;
		}
		if (WorldSettings.Instance.worldSettingsData.IsSpiritEnergyEnabledBasedOnVictoryCondition())
		{
			return true;
		}
		int num = spiritEnergyCost * p_minimumAmount;
		return PlayerManager.Instance.player.currenciesComponent.spiritEnergy >= num;
	}

	public void StartCooldown()
	{
		if (hasCooldown && currentCooldownTick == cooldown)
		{
			SetCurrentCooldownTick(0);
			Messenger.Broadcast(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, this);
			if (cooldown > 0)
			{
				Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
			}
			else
			{
				PerTickCooldown();
			}
		}
	}

	protected virtual void PerTickCooldown()
	{
		currentCooldownTick++;
		if (currentCooldownTick < cooldown)
		{
			return;
		}
		currentCooldownTick = cooldown;
		FinishCooldown();
		if (hasCharges && charges < maxCharges)
		{
			AdjustCharges(1);
		}
		else if (category == PLAYER_SKILL_CATEGORY.SCHEME)
		{
			SkillData playerActionData = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SCHEME);
			if (playerActionData.hasCharges && playerActionData.charges < playerActionData.maxCharges)
			{
				playerActionData.AdjustCharges(1);
			}
		}
		Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
	}

	public virtual void FinishCooldown()
	{
		Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
		Messenger.Broadcast(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, this);
	}

	public bool HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT p_effect)
	{
		return PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(type).HasAddedEffectByLevel(p_effect, currentLevel);
	}

	protected string GetLocalizedReasonWhyCannotPerformAbilityTowards(string p_key)
	{
		return LocalizationManager.Instance.GetLocalizedValue("PlayerPowerReasons_Table", p_key);
	}

	protected string GetLocalizedReasonWhyCannotPerformAbilityTowards(string p_key, Character p_targetCharacter)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Player", "PlayerPowerReasons_Table", p_key);
		log.AddToFillers(p_targetCharacter, p_targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		string logText = log.logText;
		LogPool.Release(log);
		return logText;
	}

	protected string GetLocalizedReasonWhyCannotPerformAbilityTowards(string p_key, BaseSettlement p_settlement)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Player", "PlayerPowerReasons_Table", p_key);
		log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
		string logText = log.logText;
		LogPool.Release(log);
		return logText;
	}

	protected string GetLocalizedReasonWhyCannotPerformAbilityTowards(string p_key, LocationStructure p_structure)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Player", "PlayerPowerReasons_Table", p_key);
		log.AddToFillers(p_structure, p_structure.name, LOG_IDENTIFIER.LANDMARK_1);
		string logText = log.logText;
		LogPool.Release(log);
		return logText;
	}

	private string AppendLockedReason(string reasons)
	{
		Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(characterThatLockedSkillID);
		if (characterByPersistentID != null)
		{
			bool flag = false;
			if (characterByPersistentID.traitContainer.HasTrait("Cleric") && characterByPersistentID.traitContainer.GetTraitOrStatus<Cleric>("Cleric").lockedSkill == type)
			{
				reasons = reasons + GetLocalizedReasonWhyCannotPerformAbilityTowards("Power_Locked", characterByPersistentID) + "|";
				flag = true;
			}
			if (!flag && characterByPersistentID.traitContainer.HasTrait("Nullchild") && characterByPersistentID.traitContainer.GetTraitOrStatus<Nullchild>("Nullchild").lockedSkills.Contains(type))
			{
				reasons = ((!characterByPersistentID.isInfoUnlocked) ? (reasons + GetLocalizedReasonWhyCannotPerformAbilityTowards("Power_Locked_Secret") + "|") : (reasons + GetLocalizedReasonWhyCannotPerformAbilityTowards("Power_Locked", characterByPersistentID) + "|"));
				flag = true;
			}
			if (!flag)
			{
				reasons = reasons + GetLocalizedReasonWhyCannotPerformAbilityTowards("Power_Locked_Default", characterByPersistentID) + "|";
			}
		}
		return reasons;
	}

	public void SetIsUnlockBaseOnRequirements(bool p_isUnlocked)
	{
		isUnlockedBaseOnRequirements = p_isUnlocked;
	}

	public void SetMaxCharges(int amount)
	{
		baseMaxCharges = amount;
	}

	public void AdjustMaxCharges(int amount)
	{
		baseMaxCharges += amount;
	}

	public void SetCharges(int amount)
	{
		charges = amount;
	}

	public void SetBonusCharges(int amount)
	{
		bonusCharges = amount;
	}

	public void AdjustCharges(int amount)
	{
		charges += amount;
		ResetAllChargesUIText();
		Messenger.Broadcast(PlayerSkillSignals.CHARGES_UPDATED, this, amount);
		if (charges < maxCharges)
		{
			StartCooldown();
		}
	}

	public void AdjustBonusCharges(int amount)
	{
		bonusCharges += amount;
		bonusCharges = Mathf.Max(bonusCharges, 0);
		ResetAllChargesUIText();
		Messenger.Broadcast(PlayerSkillSignals.BONUS_CHARGES_ADJUSTED, this);
	}

	public void SetPierce(float amount)
	{
		basePierce = amount;
	}

	public void SetUnlockCost(int amount)
	{
		unlockCost = amount;
	}

	public void SetManaCost(int amount)
	{
		baseManaCost = amount;
	}

	public void SetBaseSpiritEnergyCost(int p_amount)
	{
		baseSpiritEnergyCost = p_amount;
	}

	public void SetCooldown(int amount)
	{
		baseCooldown = amount;
		currentCooldownTick = cooldown;
	}

	public void SetBaseCooldownOnly(int amount)
	{
		baseCooldown = amount;
	}

	public void SetCurrentCooldownTick(int amount)
	{
		currentCooldownTick = amount;
	}

	public void SetRemainingChaosOrbs(int amount)
	{
		remainingChaosOrbs = amount;
	}

	public void SetIsInUse(bool state)
	{
		if (isInUse == state)
		{
			return;
		}
		isInUse = state;
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(type);
		if (isInUse)
		{
			SetIsTemporarilyInUse(p_state: false);
			if (scriptableObjPlayerSkillData != null)
			{
				PlayerManager.Instance.player.playerSkillComponent.AddTierCount(scriptableObjPlayerSkillData);
			}
		}
		else if (scriptableObjPlayerSkillData != null)
		{
			PlayerManager.Instance.player.playerSkillComponent.RemoveTierCount(scriptableObjPlayerSkillData);
		}
	}

	public void ResetIsInUse()
	{
		isInUse = false;
	}

	public void SetIsTemporarilyInUse(bool p_state)
	{
		isTemporarilyInUse = p_state;
	}

	public void SetCurrentLevel(int amount)
	{
		currentLevel = amount;
	}

	public void DecreaseRemainingChaosOrbs(int p_amount)
	{
		if (!hasUnliChaosOrbs && remainingChaosOrbs > 0)
		{
			remainingChaosOrbs -= p_amount;
			if (remainingChaosOrbs <= 0)
			{
				remainingChaosOrbs = 0;
				ShowExhaustedChaosOrbsNotif();
			}
		}
	}

	public void DecreaseRemainingChaosOrbs(ref int p_amount)
	{
		if (!hasUnliChaosOrbs)
		{
			if (p_amount > remainingChaosOrbs)
			{
				p_amount = remainingChaosOrbs;
			}
			DecreaseRemainingChaosOrbs(p_amount);
		}
	}

	public bool TryDecreaseRemainingChaosOrbs(ref int p_amount)
	{
		if (!hasRemainingOrUnliChaosOrbs)
		{
			return false;
		}
		DecreaseRemainingChaosOrbs(ref p_amount);
		return true;
	}

	public bool TryDecreaseRemainingChaosOrbs(int p_amount)
	{
		if (!hasRemainingOrUnliChaosOrbs)
		{
			return false;
		}
		DecreaseRemainingChaosOrbs(p_amount);
		return true;
	}

	private void ShowExhaustedChaosOrbsNotif()
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		string localizedValue = localizedName;
		if (type == PLAYER_SKILL_TYPE.BRAINWASH)
		{
			localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cultists");
		}
		dictionary.Add("powerName", localizedValue);
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("PlayerPowerAlerts_Table", "Exhausted_Chaos_Orbs", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		PopUpNotificationUI.Instance.ShowPlayerPoppingTextNotif(Utilities.YellowDotIcon() + localizedValue2, 5, PlayerUI.Instance.popUpDisplayPoint);
	}

	public void EnableSkill()
	{
		SetIsUsable(p_state: true);
		SetCharacterThatLockedSkill(string.Empty);
		Messenger.Broadcast(PlayerSkillSignals.SKILL_ENABLED, this);
	}

	public void DisableSkill(Character p_character)
	{
		SetIsUsable(p_state: false);
		SetCharacterThatLockedSkill(p_character.persistentID);
		Messenger.Broadcast(PlayerSkillSignals.SKILL_DISABLED, this);
	}

	public void SetIsUsable(bool p_state)
	{
		isUsable = p_state;
	}

	public void SetCharacterThatLockedSkill(string p_id)
	{
		characterThatLockedSkillID = p_id;
	}

	public string GetCurrencyUIText()
	{
		if (string.IsNullOrEmpty(_currencyUIText))
		{
			if (manaCost > 0)
			{
				_currencyUIText = $"{_currencyUIText}{Utilities.ManaIcon()}{manaCost}    ";
			}
			if (spiritEnergyCost > 0)
			{
				_currencyUIText = $"{_currencyUIText}{Utilities.SpiritEnergyIcon()}{spiritEnergyCost}    ";
			}
			string chargesNotCombinedUIText = GetChargesNotCombinedUIText();
			if (!string.IsNullOrEmpty(chargesNotCombinedUIText))
			{
				_currencyUIText += chargesNotCombinedUIText;
			}
		}
		return _currencyUIText;
	}

	public string GetCurrencyLevelUpUIText(PlayerSkillData playerSkillData)
	{
		if (string.IsNullOrEmpty(_currencyLevelUpUIText))
		{
			int num = manaCost;
			int manaCostBaseOnLevel = playerSkillData.GetManaCostBaseOnLevel(currentLevel + 1);
			if (num != manaCostBaseOnLevel)
			{
				_currencyLevelUpUIText = $"{_currencyLevelUpUIText}{Utilities.ManaIcon()}{num} {Utilities.UpgradeArrowIcon()} {Utilities.ColorizeUpgradeText(manaCostBaseOnLevel.ToString())}    ";
			}
			else
			{
				_currencyLevelUpUIText = $"{_currencyLevelUpUIText}{Utilities.ManaIcon()}{num}    ";
			}
			string chargesNotCombinedUIText = GetChargesNotCombinedUIText();
			if (!string.IsNullOrEmpty(chargesNotCombinedUIText))
			{
				_currencyLevelUpUIText += chargesNotCombinedUIText;
			}
		}
		return _currencyLevelUpUIText;
	}

	public void ResetCurrencyUIText()
	{
		_currencyUIText = string.Empty;
		_currencyLevelUpUIText = string.Empty;
	}

	public string GetChargesNotCombinedUIText()
	{
		if (string.IsNullOrEmpty(_chargesNotCombinedUIText))
		{
			_chargesNotCombinedUIText = SpellUtilities.GetDisplayOfCurrentChargesWithBonusChargesNotCombined(charges, maxCharges, bonusCharges, hasCharges && isInUse);
		}
		return _chargesNotCombinedUIText;
	}

	public void ResetChargesNotCombinedUIText()
	{
		_chargesNotCombinedUIText = string.Empty;
		ResetCurrencyUIText();
	}

	public string GetChargesCombinedUIText()
	{
		if (string.IsNullOrEmpty(_chargesCombinedUIText) && (hasCharges || hasBonusCharges))
		{
			_chargesCombinedUIText = $"{(hasBonusCharges ? Utilities.BonusChargesIcon() : Utilities.ChargesIcon())}{(isInUse ? totalCharges : bonusCharges)}";
		}
		return _chargesCombinedUIText;
	}

	public void ResetChargesCombinedUIText()
	{
		_chargesCombinedUIText = string.Empty;
	}

	public string GetChargesCombinedIconFirstUIText()
	{
		if (string.IsNullOrEmpty(_chargesCombinedIconFirstUIText) && (hasCharges || hasBonusCharges))
		{
			_chargesCombinedIconFirstUIText = $"{(hasBonusCharges ? Utilities.BonusChargesIcon() : Utilities.ChargesIcon())}{(isInUse ? totalCharges : bonusCharges)}";
		}
		return _chargesCombinedIconFirstUIText;
	}

	public void ResetChargesCombinedIconFirstUIText()
	{
		_chargesCombinedIconFirstUIText = string.Empty;
	}

	public void ResetAllChargesUIText()
	{
		ResetChargesNotCombinedUIText();
		ResetChargesCombinedUIText();
		ResetChargesCombinedIconFirstUIText();
		ResetMaxChargesUIText();
	}

	public string GetMaxChargesUIText()
	{
		if (string.IsNullOrEmpty(_maxChargesUIText) && maxCharges > 0)
		{
			_maxChargesUIText = string.Format("{0} {1}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Max_Charges_No_Icon") + ":"), maxCharges);
		}
		return _maxChargesUIText;
	}

	public string GetMaxChargesBonusLevelUpUIText(PlayerSkillData p_skillData)
	{
		if (string.IsNullOrEmpty(_maxChargesLevelUpUIText))
		{
			int p_currentLevel = currentLevel + 1;
			if (maxCharges > 0)
			{
				_maxChargesLevelUpUIText = GetMaxChargesUIText();
				int num = maxCharges;
				int chargesBaseOnLevel = p_skillData.skillUpgradeData.GetChargesBaseOnLevel(p_currentLevel);
				if (chargesBaseOnLevel - num > 0)
				{
					_maxChargesLevelUpUIText = _maxChargesLevelUpUIText + " " + Utilities.UpgradeArrowIcon() + " " + Utilities.ColorizeUpgradeText($"{chargesBaseOnLevel}");
				}
			}
		}
		return _maxChargesLevelUpUIText;
	}

	public void ResetMaxChargesUIText()
	{
		_maxChargesUIText = string.Empty;
		_maxChargesLevelUpUIText = string.Empty;
	}

	public string GetManaCostChargesCooldownStr()
	{
		return string.Concat(string.Concat("Mana Cost: " + manaCost, "\nCharges: ", charges.ToString()), "\nCooldown: ", cooldown.ToString());
	}
}
