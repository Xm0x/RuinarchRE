using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using UtilityScripts;

[CreateAssetMenu(fileName = "New Player Skill Data", menuName = "Scriptable Objects/Player Skills/Player Skill Data")]
public class PlayerSkillData : ScriptableObject
{
	public PLAYER_SKILL_TYPE skill;

	public int cheatedLevel;

	public int bonusChargeWhenUnlocked;

	public int unlockChargeOnPortalUpgrade = 1;

	public int chaosOrbLimit;

	public Sprite buttonSprite;

	public VideoClip tooltipVideoClip;

	public Sprite tooltipImage;

	public Sprite skillIcon;

	public Sprite actionItemIcon;

	[SerializeField]
	private int unlockCost;

	public int tier;

	public int baseLoadoutWeight;

	public RESISTANCE resistanceType;

	public PLAYER_ARCHETYPE archetypeWeightedBonus;

	[Tooltip("If true, this power will show up in the release ability menu even if it is already learned by the player")]
	public bool canBeReleasedEvenIfLearned;

	[Header("Context Menu")]
	public Sprite contextMenuIcon;

	public int contextMenuColumn;

	public bool isNonUpgradeable;

	public bool isAffliction;

	public bool isLockedBaseOnRequirements;

	[Space]
	[Header("--------------Upgrade Related---------------")]
	public RequirementData requirementData;

	public SkillUpgradeData skillUpgradeData;

	public AfflictionUpgradeData afflictionUpgradeData;

	private string _upgradeBonusUIText = string.Empty;

	private string _afflictionBonusUIText = string.Empty;

	private string _chargesBonusUIText = string.Empty;

	private string _resistanceBonusUIText = string.Empty;

	private string _powerAddedEffectsUIText = string.Empty;

	private string _upgradeBonusLevelUpUIText = string.Empty;

	private string _afflictionBonusLevelUpUIText = string.Empty;

	private string _powerAddedEffectsLevelUpUIText = string.Empty;

	public int GetUnlockCost()
	{
		return SpellUtilities.GetModifiedSpellCost(unlockCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease());
	}

	public int GetManaCostBaseOnLevel(int level)
	{
		return SpellUtilities.GetModifiedSpellCost(skillUpgradeData.GetManaCostPerLevel(level), WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
	}

	public int GetSpiritEnergyCostBaseOnLevel(int level)
	{
		if (!WorldSettings.Instance.worldSettingsData.IsSpiritEnergyEnabledBasedOnVictoryCondition())
		{
			return 0;
		}
		if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification() == 0f)
		{
			return 0;
		}
		return skillUpgradeData.GetSpiritEnergyCostPerLevel(level);
	}

	public int GetCoolDownBaseOnLevel(int level)
	{
		int num = 1;
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			num = PlayerManager.Instance.player.playerSkillComponent.allPowersCooldownMultiplier;
		}
		return SpellUtilities.GetModifiedSpellCost(skillUpgradeData.GetCoolDownPerLevel(level), WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCooldownSpeedModification() * (float)num);
	}

	public int GetMaxChargesBaseOnLevel(int level)
	{
		int num = skillUpgradeData.GetChargesBaseOnLevel(level);
		if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled && skill != PLAYER_SKILL_TYPE.BRAINWASH && skill != PLAYER_SKILL_TYPE.TORTURE && skill != PLAYER_SKILL_TYPE.DEMON_ENVY && skill != PLAYER_SKILL_TYPE.DEMON_GLUTTONY && skill != PLAYER_SKILL_TYPE.DEMON_GREED && skill != PLAYER_SKILL_TYPE.DEMON_LUST && skill != PLAYER_SKILL_TYPE.DEMON_PRIDE && skill != PLAYER_SKILL_TYPE.DEMON_SLOTH && skill != PLAYER_SKILL_TYPE.DEMON_WRATH && num < 3 && num > 0)
		{
			num = 3;
		}
		return SpellUtilities.GetModifiedSpellCost(num, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetChargeModificationBasedOnSkillType(skill));
	}

	public float GetAdditionalPiercePerLevelBaseOnLevel(int p_currentLevel)
	{
		return skillUpgradeData.GetAdditionalPiercePerLevelBaseOnLevel(p_currentLevel);
	}

	public int GetDurationBonusPerLevel(int p_currentLevel)
	{
		return skillUpgradeData.GetDurationBonusPerLevel(p_currentLevel);
	}

	[ContextMenu("Reset requirements except portal")]
	public void ResetAllRequirements()
	{
		requirementData.requiredArchetypes = new List<PLAYER_ARCHETYPE>();
		requirementData.requiredSkills.Clear();
		requirementData.actionCount = 0;
		requirementData.afflictionCount = 0;
		requirementData.spellsCount = 0;
		requirementData.tier1Count = 0;
		requirementData.tier2Count = 0;
		requirementData.tier3Count = 0;
		requirementData.isOR = false;
	}

	public string GetUpgradeBonusUIText(int p_level)
	{
		if (string.IsNullOrEmpty(_upgradeBonusUIText))
		{
			List<UPGRADE_BONUS> list = RuinarchListPool<UPGRADE_BONUS>.Claim();
			list.AddRange(skillUpgradeData.bonuses.OrderBy((UPGRADE_BONUS u) => u.GetUpgradeOrderInTooltip()));
			for (int num = 0; num < list.Count; num++)
			{
				UPGRADE_BONUS uPGRADE_BONUS = list[num];
				if (uPGRADE_BONUS != UPGRADE_BONUS.Added_Effect_Level_0 && uPGRADE_BONUS != UPGRADE_BONUS.Added_Effect_Level_1 && uPGRADE_BONUS != UPGRADE_BONUS.Added_Effect_Level_2 && uPGRADE_BONUS != UPGRADE_BONUS.Added_Effect_Level_3 && GetFormattedBonusString(uPGRADE_BONUS, this, p_level, out var formatted))
				{
					_upgradeBonusUIText = _upgradeBonusUIText + formatted + "\n";
				}
			}
			RuinarchListPool<UPGRADE_BONUS>.Release(list);
			_upgradeBonusUIText = _upgradeBonusUIText.TrimEnd();
		}
		return _upgradeBonusUIText;
	}

	public void ResetUpgradeBonusUIText()
	{
		_upgradeBonusUIText = string.Empty;
	}

	public string GetUpgradeBonusLevelUpUIText(int p_level)
	{
		if (string.IsNullOrEmpty(_upgradeBonusLevelUpUIText))
		{
			List<UPGRADE_BONUS> list = RuinarchListPool<UPGRADE_BONUS>.Claim();
			list.AddRange(skillUpgradeData.bonuses.OrderBy((UPGRADE_BONUS u) => u.GetUpgradeOrderInTooltip()));
			for (int num = 0; num < list.Count; num++)
			{
				UPGRADE_BONUS uPGRADE_BONUS = list[num];
				if (uPGRADE_BONUS == UPGRADE_BONUS.Added_Effect_Level_0 || uPGRADE_BONUS == UPGRADE_BONUS.Added_Effect_Level_1 || uPGRADE_BONUS == UPGRADE_BONUS.Added_Effect_Level_2 || uPGRADE_BONUS == UPGRADE_BONUS.Added_Effect_Level_3)
				{
					continue;
				}
				string formatted;
				bool formattedBonusString = GetFormattedBonusString(uPGRADE_BONUS, this, p_level, out formatted);
				if (!string.IsNullOrEmpty(formatted))
				{
					string bonusDifferenceString = GetBonusDifferenceString(uPGRADE_BONUS, this, p_level, p_level + 1);
					if (!string.IsNullOrEmpty(bonusDifferenceString))
					{
						formatted = formatted + " " + Utilities.UpgradeArrowIcon() + " " + Utilities.ColorizeUpgradeText(bonusDifferenceString ?? "");
					}
					if (formattedBonusString || !string.IsNullOrEmpty(bonusDifferenceString))
					{
						_upgradeBonusLevelUpUIText = _upgradeBonusLevelUpUIText + formatted + "\n";
					}
				}
			}
			RuinarchListPool<UPGRADE_BONUS>.Release(list);
			_upgradeBonusLevelUpUIText = _upgradeBonusLevelUpUIText.TrimEnd();
		}
		return _upgradeBonusLevelUpUIText;
	}

	public void ResetUpgradeBonusLevelUpUIText()
	{
		_upgradeBonusLevelUpUIText = string.Empty;
	}

	public string GetChargesBonusUIText()
	{
		if (string.IsNullOrEmpty(_chargesBonusUIText))
		{
			int chargesBaseOnLevel = skillUpgradeData.GetChargesBaseOnLevel(0);
			if (chargesBaseOnLevel > 0)
			{
				_chargesBonusUIText = string.Format("{0} {1}/{2}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Charges") + ":"), chargesBaseOnLevel, chargesBaseOnLevel);
			}
		}
		return _chargesBonusUIText;
	}

	public void ResetChargesBonusUIText()
	{
		_chargesBonusUIText = string.Empty;
	}

	public string GetResistanceBonusUIText()
	{
		if (string.IsNullOrEmpty(_resistanceBonusUIText) && resistanceType != RESISTANCE.None)
		{
			_resistanceBonusUIText = _resistanceBonusUIText + Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Resistances") + ":") + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", resistanceType.ToStringEnumWithIcon());
		}
		return _resistanceBonusUIText;
	}

	public void ResetResistancesBonusUIText()
	{
		_resistanceBonusUIText = string.Empty;
	}

	public string GetAfflictionBonusUIText(int p_level)
	{
		if (string.IsNullOrEmpty(_afflictionBonusUIText))
		{
			for (int i = 0; i < afflictionUpgradeData.bonuses.Count; i++)
			{
				AFFLICTION_UPGRADE_BONUS p_afflictionUpgradeBonus = afflictionUpgradeData.bonuses[i];
				string formattedAfflictionBonusString = GetFormattedAfflictionBonusString(p_afflictionUpgradeBonus, this, p_level);
				if (!string.IsNullOrEmpty(formattedAfflictionBonusString))
				{
					_afflictionBonusUIText = _afflictionBonusUIText + formattedAfflictionBonusString + "\n";
				}
			}
			_afflictionBonusUIText = _afflictionBonusUIText.TrimEnd();
		}
		return _afflictionBonusUIText;
	}

	public void ResetAfflictionBonusUIText()
	{
		_afflictionBonusUIText = string.Empty;
	}

	public string GetAfflictionBonusesWithLevelUpDetailsString(int p_level)
	{
		if (string.IsNullOrEmpty(_afflictionBonusLevelUpUIText))
		{
			for (int i = 0; i < afflictionUpgradeData.bonuses.Count; i++)
			{
				AFFLICTION_UPGRADE_BONUS p_afflictionUpgradeBonus = afflictionUpgradeData.bonuses[i];
				string formattedAfflictionBonusString = GetFormattedAfflictionBonusString(p_afflictionUpgradeBonus, this, p_level);
				int p_nextLevel = p_level + 1;
				if (!string.IsNullOrEmpty(formattedAfflictionBonusString))
				{
					_afflictionBonusLevelUpUIText += formattedAfflictionBonusString;
					string afflictionBonusDifferenceString = GetAfflictionBonusDifferenceString(p_afflictionUpgradeBonus, this, p_level, p_nextLevel);
					if (!string.IsNullOrEmpty(afflictionBonusDifferenceString))
					{
						_afflictionBonusLevelUpUIText = _afflictionBonusLevelUpUIText + Utilities.UpgradeArrowIcon() + " " + Utilities.ColorizeUpgradeText(afflictionBonusDifferenceString) + "\n";
					}
					else
					{
						_afflictionBonusLevelUpUIText += "\n";
					}
				}
			}
			_afflictionBonusLevelUpUIText = _afflictionBonusLevelUpUIText.TrimEnd();
		}
		return _afflictionBonusLevelUpUIText;
	}

	public void ResetAfflictionBonusLevelUpUIText()
	{
		_afflictionBonusLevelUpUIText = string.Empty;
	}

	public string GetPowerAddedEffectsUIText(int p_level)
	{
		if (string.IsNullOrEmpty(_powerAddedEffectsUIText))
		{
			List<POWER_ADDED_EFFECT> list = RuinarchListPool<POWER_ADDED_EFFECT>.Claim();
			PopulatePowerAddedEffects(list, this, p_level);
			for (int i = 0; i < list.Count; i++)
			{
				POWER_ADDED_EFFECT p_type = list[i];
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("PlayerPowers_Table", p_type.ToStringEnum());
				_powerAddedEffectsUIText = _powerAddedEffectsUIText + localizedValue + "\n";
			}
			if (list.Count > 0)
			{
				_powerAddedEffectsUIText = Utilities.ColorizeSpellTitle(_powerAddedEffectsUIText.TrimEnd());
			}
			RuinarchListPool<POWER_ADDED_EFFECT>.Release(list);
		}
		return _powerAddedEffectsUIText;
	}

	public void ResetPlayerActionBonusUIText()
	{
		_powerAddedEffectsUIText = string.Empty;
	}

	public string GetPowerAddedEffectsWithLevelUpUIText(int p_level)
	{
		if (string.IsNullOrEmpty(_powerAddedEffectsLevelUpUIText))
		{
			List<POWER_ADDED_EFFECT> list = RuinarchListPool<POWER_ADDED_EFFECT>.Claim();
			PopulatePowerAddedEffects(list, this, p_level);
			for (int i = 0; i < list.Count; i++)
			{
				POWER_ADDED_EFFECT p_type = list[i];
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("PlayerPowers_Table", p_type.ToStringEnum());
				_powerAddedEffectsLevelUpUIText = _powerAddedEffectsLevelUpUIText + Utilities.ColorizeSpellTitle(localizedValue) + "\n";
			}
			int count = list.Count;
			int p_level2 = p_level + 1;
			PopulatePowerAddedEffects(list, this, p_level2);
			for (int j = count; j < list.Count; j++)
			{
				POWER_ADDED_EFFECT p_type2 = list[j];
				string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("PlayerPowers_Table", p_type2.ToStringEnum());
				_powerAddedEffectsLevelUpUIText = _powerAddedEffectsLevelUpUIText + Utilities.ColorizeUpgradeText(localizedValue2) + "\n";
			}
			RuinarchListPool<POWER_ADDED_EFFECT>.Release(list);
			_powerAddedEffectsLevelUpUIText = _powerAddedEffectsLevelUpUIText.TrimEnd();
		}
		return _powerAddedEffectsLevelUpUIText;
	}

	public void ResetPlayerActionBonusLevelUpUIText()
	{
		_powerAddedEffectsLevelUpUIText = string.Empty;
	}

	public void ResetAllBonusUIText()
	{
		ResetUpgradeBonusUIText();
		ResetUpgradeBonusLevelUpUIText();
		ResetChargesBonusUIText();
		ResetResistancesBonusUIText();
		ResetAfflictionBonusUIText();
		ResetAfflictionBonusLevelUpUIText();
		ResetPlayerActionBonusUIText();
		ResetPlayerActionBonusLevelUpUIText();
	}

	public bool GetFormattedBonusString(UPGRADE_BONUS bonus, PlayerSkillData p_data, int p_level, out string formatted)
	{
		if (bonus == UPGRADE_BONUS.Cooldown)
		{
			formatted = Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cooldown_No_Icon") + ": ");
		}
		else
		{
			formatted = Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", bonus.ToString() + "_Title") + ": ");
		}
		switch (bonus)
		{
		case UPGRADE_BONUS.Damage:
			formatted += p_data.skillUpgradeData.GetAdditionalDamageBaseOnLevel(p_level);
			break;
		case UPGRADE_BONUS.Pierce:
		{
			float additionalPiercePerLevelBaseOnLevel = p_data.GetAdditionalPiercePerLevelBaseOnLevel(p_level);
			formatted = Utilities.ColorizeSpellTitle(formatted + Utilities.PiercingIcon()) + " " + additionalPiercePerLevelBaseOnLevel;
			if (additionalPiercePerLevelBaseOnLevel == 0f)
			{
				return false;
			}
			break;
		}
		case UPGRADE_BONUS.Duration:
		{
			int durationBonusPerLevel = p_data.GetDurationBonusPerLevel(p_level);
			string empty = string.Empty;
			empty = ((!isAffliction || durationBonusPerLevel != 0) ? GameManager.ConvertTicksToWholeTime(durationBonusPerLevel) : LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Permanent"));
			formatted += empty;
			break;
		}
		case UPGRADE_BONUS.Tile_Range:
		{
			int tileRangeBonusPerLevel = p_data.skillUpgradeData.GetTileRangeBonusPerLevel(p_level);
			formatted += GetTileRangeDisplay(tileRangeBonusPerLevel);
			break;
		}
		case UPGRADE_BONUS.Movement_Speed:
			formatted += p_data.skillUpgradeData.GetSkillMovementSpeedPerLevel(p_level);
			break;
		case UPGRADE_BONUS.Cooldown:
		{
			int coolDownBaseOnLevel = p_data.GetCoolDownBaseOnLevel(p_level);
			switch (coolDownBaseOnLevel)
			{
			case 0:
				formatted += LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No Cooldown");
				break;
			case -1:
				formatted += LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No Cooldown");
				break;
			default:
				formatted += GameManager.ConvertTicksToWholeTime(coolDownBaseOnLevel);
				break;
			}
			break;
		}
		case UPGRADE_BONUS.Atk_Percentage:
			formatted = formatted + p_data.skillUpgradeData.GetAdditionalAttackPercentagePerLevelBaseOnLevel(p_level).ToString(CultureInfo.InvariantCulture) + "%";
			break;
		case UPGRADE_BONUS.Physical_Resistance:
		{
			int physicalResistanceBaseOnLevel = p_data.skillUpgradeData.GetPhysicalResistanceBaseOnLevel(p_level);
			formatted += physicalResistanceBaseOnLevel;
			if (physicalResistanceBaseOnLevel == 0)
			{
				return false;
			}
			break;
		}
		case UPGRADE_BONUS.Elemental_Resistance:
		{
			int elementalResistanceBaseOnLevel = p_data.skillUpgradeData.GetElementalResistanceBaseOnLevel(p_level);
			formatted += elementalResistanceBaseOnLevel;
			if (elementalResistanceBaseOnLevel == 0)
			{
				return false;
			}
			break;
		}
		case UPGRADE_BONUS.Secondary_Resistance:
		{
			int secondaryResistanceBaseOnLevel = p_data.skillUpgradeData.GetSecondaryResistanceBaseOnLevel(p_level);
			formatted += secondaryResistanceBaseOnLevel;
			if (secondaryResistanceBaseOnLevel == 0)
			{
				return false;
			}
			break;
		}
		case UPGRADE_BONUS.Mental_Resistance:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetMentalResistanceBaseOnLevel(p_level);
			formatted += mentalResistanceBaseOnLevel;
			if (mentalResistanceBaseOnLevel == 0)
			{
				return false;
			}
			break;
		}
		default:
			formatted = string.Empty;
			return false;
		}
		return true;
	}

	public string GetBonusDifferenceString(UPGRADE_BONUS bonus, PlayerSkillData p_data, int p_level, int p_nextLevel)
	{
		string result = string.Empty;
		switch (bonus)
		{
		case UPGRADE_BONUS.Damage:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetAdditionalDamageBaseOnLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetAdditionalDamageBaseOnLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel2 - mentalResistanceBaseOnLevel > 0)
			{
				result = mentalResistanceBaseOnLevel2.ToString() ?? "";
			}
			break;
		}
		case UPGRADE_BONUS.Pierce:
		{
			float additionalPiercePerLevelBaseOnLevel = p_data.GetAdditionalPiercePerLevelBaseOnLevel(p_level);
			float additionalPiercePerLevelBaseOnLevel2 = p_data.GetAdditionalPiercePerLevelBaseOnLevel(p_nextLevel);
			if (additionalPiercePerLevelBaseOnLevel2 - additionalPiercePerLevelBaseOnLevel > 0f)
			{
				result = additionalPiercePerLevelBaseOnLevel2.ToString() ?? "";
			}
			break;
		}
		case UPGRADE_BONUS.Duration:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetDurationBonusPerLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetDurationBonusPerLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel != mentalResistanceBaseOnLevel2)
			{
				string empty = string.Empty;
				empty = ((!isAffliction || mentalResistanceBaseOnLevel2 != 0) ? GameManager.ConvertTicksToWholeTime(mentalResistanceBaseOnLevel2) : LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Permanent"));
				result = empty ?? "";
			}
			break;
		}
		case UPGRADE_BONUS.Tile_Range:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetTileRangeBonusPerLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetTileRangeBonusPerLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel2 - mentalResistanceBaseOnLevel > 0)
			{
				result = GetTileRangeDisplay(mentalResistanceBaseOnLevel2) ?? "";
			}
			break;
		}
		case UPGRADE_BONUS.Movement_Speed:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetSkillMovementSpeedPerLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetSkillMovementSpeedPerLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel2 - mentalResistanceBaseOnLevel > 0)
			{
				result = $"{mentalResistanceBaseOnLevel2}";
			}
			break;
		}
		case UPGRADE_BONUS.Cooldown:
		{
			int mentalResistanceBaseOnLevel = p_data.GetCoolDownBaseOnLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.GetCoolDownBaseOnLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel != mentalResistanceBaseOnLevel2)
			{
				result = mentalResistanceBaseOnLevel2 switch
				{
					0 => LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Instant"), 
					-1 => LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No Cooldown"), 
					_ => GameManager.ConvertTicksToWholeTime(mentalResistanceBaseOnLevel2) ?? "", 
				};
			}
			break;
		}
		case UPGRADE_BONUS.Atk_Percentage:
		{
			float additionalPiercePerLevelBaseOnLevel = p_data.skillUpgradeData.GetAdditionalAttackPercentagePerLevelBaseOnLevel(p_level);
			float additionalPiercePerLevelBaseOnLevel2 = p_data.skillUpgradeData.GetAdditionalAttackPercentagePerLevelBaseOnLevel(p_nextLevel);
			if (additionalPiercePerLevelBaseOnLevel2 - additionalPiercePerLevelBaseOnLevel > 0f)
			{
				result = additionalPiercePerLevelBaseOnLevel2 + "%";
			}
			break;
		}
		case UPGRADE_BONUS.Physical_Resistance:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetPhysicalResistanceBaseOnLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetPhysicalResistanceBaseOnLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel != mentalResistanceBaseOnLevel2)
			{
				result = mentalResistanceBaseOnLevel2.ToString();
			}
			break;
		}
		case UPGRADE_BONUS.Elemental_Resistance:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetElementalResistanceBaseOnLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetElementalResistanceBaseOnLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel != mentalResistanceBaseOnLevel2)
			{
				result = mentalResistanceBaseOnLevel2.ToString();
			}
			break;
		}
		case UPGRADE_BONUS.Secondary_Resistance:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetSecondaryResistanceBaseOnLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetSecondaryResistanceBaseOnLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel != mentalResistanceBaseOnLevel2)
			{
				result = mentalResistanceBaseOnLevel2.ToString();
			}
			break;
		}
		case UPGRADE_BONUS.Mental_Resistance:
		{
			int mentalResistanceBaseOnLevel = p_data.skillUpgradeData.GetMentalResistanceBaseOnLevel(p_level);
			int mentalResistanceBaseOnLevel2 = p_data.skillUpgradeData.GetMentalResistanceBaseOnLevel(p_nextLevel);
			if (mentalResistanceBaseOnLevel != mentalResistanceBaseOnLevel2)
			{
				result = mentalResistanceBaseOnLevel2.ToString();
			}
			break;
		}
		default:
			result = string.Empty;
			break;
		}
		return result;
	}

	private string GetFormattedAfflictionBonusString(AFFLICTION_UPGRADE_BONUS p_afflictionUpgradeBonus, PlayerSkillData p_data, int p_level)
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", p_afflictionUpgradeBonus.ToString() + "_Title");
		switch (p_afflictionUpgradeBonus)
		{
		case AFFLICTION_UPGRADE_BONUS.Crowd_Number:
			return Utilities.ColorizeSpellTitle(localizedValue + ":") + " " + p_data.afflictionUpgradeData.crowdNumber[p_level];
		case AFFLICTION_UPGRADE_BONUS.Hunger_Rate:
			return Utilities.ColorizeSpellTitle(localizedValue + ":") + " +" + p_data.afflictionUpgradeData.hungerRate[p_level] + "%";
		case AFFLICTION_UPGRADE_BONUS.Trigger_Rate:
			return Utilities.ColorizeSpellTitle(localizedValue + ":") + " " + p_data.afflictionUpgradeData.rateChance[p_level] + "%";
		case AFFLICTION_UPGRADE_BONUS.Naps_Duration:
		{
			int ticks = p_data.afflictionUpgradeData.napsDuration[p_level];
			return Utilities.ColorizeSpellTitle(localizedValue + ":") + " " + GameManager.ConvertTicksToWholeTime(ticks);
		}
		case AFFLICTION_UPGRADE_BONUS.Number_Criteria:
			return Utilities.ColorizeSpellTitle(localizedValue + ":") + " " + p_data.afflictionUpgradeData.numberOfCriteria[p_level];
		case AFFLICTION_UPGRADE_BONUS.Criteria:
		{
			string text2 = Utilities.ColorizeSpellTitle(localizedValue + ": ");
			List<LIST_OF_CRITERIA> list2 = RuinarchListPool<LIST_OF_CRITERIA>.Claim();
			p_data.afflictionUpgradeData.PopulateAvailableCriteriaForLevel(list2, p_level);
			string text3 = string.Empty;
			for (int j = 0; j < list2.Count; j++)
			{
				LIST_OF_CRITERIA lIST_OF_CRITERIA = list2[j];
				string text4 = "Capital_" + lIST_OF_CRITERIA.ToStringEnum();
				if (lIST_OF_CRITERIA == LIST_OF_CRITERIA.Race)
				{
					text4 = lIST_OF_CRITERIA.ToStringEnum();
				}
				string text5 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", text4);
				if (string.IsNullOrEmpty(text5))
				{
					text5 = text4;
				}
				text3 += text5;
				if (!list2.IsLastIndex(j))
				{
					text3 += ", ";
				}
			}
			text2 += text3;
			RuinarchListPool<LIST_OF_CRITERIA>.Release(list2);
			return text2;
		}
		case AFFLICTION_UPGRADE_BONUS.Trigger_Opinion:
		{
			string text = Utilities.ColorizeSpellTitle(localizedValue + ": ");
			List<OPINIONS> list = RuinarchListPool<OPINIONS>.Claim();
			p_data.afflictionUpgradeData.PopulateOpinionTriggerForLevel(list, p_level);
			if (list.Contains(OPINIONS.Everyone))
			{
				text += LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Everyone");
			}
			else
			{
				for (int i = 0; i < list.Count; i++)
				{
					OPINIONS oPINIONS = list[i];
					if (oPINIONS != OPINIONS.NoOne)
					{
						string key = oPINIONS.ToStringEnum();
						text += LocalizationManager.Instance.GetLocalizedValue("Relationships_Table", key);
						if (!list.IsLastIndex(i))
						{
							text += ", ";
						}
					}
				}
			}
			RuinarchListPool<OPINIONS>.Release(list);
			return text;
		}
		default:
			return string.Empty;
		}
	}

	private void PopulatePowerAddedEffects(List<POWER_ADDED_EFFECT> p_bonuses, PlayerSkillData p_data, int p_level)
	{
		List<POWER_ADDED_EFFECT> addedEffectUpgradeBonusesByLevel = p_data.GetAddedEffectUpgradeBonusesByLevel(p_level);
		for (int i = 0; i < addedEffectUpgradeBonusesByLevel.Count; i++)
		{
			POWER_ADDED_EFFECT item = addedEffectUpgradeBonusesByLevel[i];
			if (!p_bonuses.Contains(item))
			{
				p_bonuses.Add(item);
			}
		}
	}

	private string GetAfflictionBonusDifferenceString(AFFLICTION_UPGRADE_BONUS p_afflictionUpgradeBonus, PlayerSkillData p_data, int p_level, int p_nextLevel)
	{
		switch (p_afflictionUpgradeBonus)
		{
		case AFFLICTION_UPGRADE_BONUS.Crowd_Number:
		{
			int num = p_data.afflictionUpgradeData.crowdNumber[p_level];
			int num2 = p_data.afflictionUpgradeData.crowdNumber[p_nextLevel];
			if (num2 != num)
			{
				return num2.ToString();
			}
			return string.Empty;
		}
		case AFFLICTION_UPGRADE_BONUS.Hunger_Rate:
			return "+" + p_data.afflictionUpgradeData.hungerRate[p_nextLevel] + "%";
		case AFFLICTION_UPGRADE_BONUS.Trigger_Rate:
			return p_data.afflictionUpgradeData.rateChance[p_nextLevel] + "%";
		case AFFLICTION_UPGRADE_BONUS.Naps_Duration:
			return GameManager.ConvertTicksToWholeTime(p_data.afflictionUpgradeData.napsDuration[p_nextLevel]) ?? "";
		case AFFLICTION_UPGRADE_BONUS.Number_Criteria:
			return p_data.afflictionUpgradeData.numberOfCriteria[p_nextLevel].ToString() ?? "";
		case AFFLICTION_UPGRADE_BONUS.Criteria:
		{
			List<LIST_OF_CRITERIA> list2 = RuinarchListPool<LIST_OF_CRITERIA>.Claim();
			p_data.afflictionUpgradeData.PopulateAvailableCriteriaForLevel(list2, p_nextLevel);
			string text2 = string.Empty;
			for (int j = 0; j < list2.Count; j++)
			{
				LIST_OF_CRITERIA lIST_OF_CRITERIA = list2[j];
				string text3 = "Capital_" + lIST_OF_CRITERIA.ToStringEnum();
				if (lIST_OF_CRITERIA == LIST_OF_CRITERIA.Race)
				{
					text3 = lIST_OF_CRITERIA.ToStringEnum();
				}
				string text4 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", text3);
				if (string.IsNullOrEmpty(text4))
				{
					text4 = text3;
				}
				text2 += text4;
				if (!list2.IsLastIndex(j))
				{
					text2 += ", ";
				}
			}
			return text2;
		}
		case AFFLICTION_UPGRADE_BONUS.Trigger_Opinion:
		{
			string text = string.Empty;
			List<OPINIONS> list = RuinarchListPool<OPINIONS>.Claim();
			p_data.afflictionUpgradeData.PopulateOpinionTriggerForLevel(list, p_nextLevel);
			if (list.Contains(OPINIONS.Everyone))
			{
				text += LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Everyone");
			}
			else
			{
				for (int i = 0; i < list.Count; i++)
				{
					OPINIONS oPINIONS = list[i];
					if (oPINIONS != OPINIONS.NoOne)
					{
						string key = oPINIONS.ToStringEnum();
						text += LocalizationManager.Instance.GetLocalizedValue("Relationships_Table", key);
						if (!list.IsLastIndex(i))
						{
							text += ", ";
						}
					}
				}
			}
			RuinarchListPool<OPINIONS>.Release(list);
			return text;
		}
		default:
			return string.Empty;
		}
	}

	public static string GetFormattedAfflictionSpecificBehaviour(POWER_ADDED_EFFECT p_behaviour)
	{
		return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", p_behaviour.ToStringEnum());
	}

	private string GetTileRangeDisplay(int radius)
	{
		int num = radius * 2 + 1;
		return num + "x" + num;
	}

	public List<POWER_ADDED_EFFECT> GetAddedEffectUpgradeBonusesByLevel(int p_level)
	{
		return p_level switch
		{
			0 => skillUpgradeData.addedEffectsLevel0, 
			1 => skillUpgradeData.addedEffectsLevel1, 
			2 => skillUpgradeData.addedEffectsLevel2, 
			3 => skillUpgradeData.addedEffectsLevel3, 
			_ => null, 
		};
	}

	public List<POWER_ADDED_EFFECT> GetAddedEffectUpgradeBonusesByEnum(UPGRADE_BONUS p_bonusType)
	{
		return p_bonusType switch
		{
			UPGRADE_BONUS.Added_Effect_Level_0 => skillUpgradeData.addedEffectsLevel0, 
			UPGRADE_BONUS.Added_Effect_Level_1 => skillUpgradeData.addedEffectsLevel1, 
			UPGRADE_BONUS.Added_Effect_Level_2 => skillUpgradeData.addedEffectsLevel2, 
			UPGRADE_BONUS.Added_Effect_Level_3 => skillUpgradeData.addedEffectsLevel3, 
			_ => null, 
		};
	}

	public bool HasAddedEffectByLevel(POWER_ADDED_EFFECT p_effect, int p_level)
	{
		return GetAddedEffectUpgradeBonusesByLevel(p_level)?.Contains(p_effect) ?? false;
	}
}
