using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

[Serializable]
public class SkillUpgradeData
{
	public List<int> upgradeCosts;

	public List<int> manacostPerLevel;

	public List<int> chargesPerLevel;

	public int[] spiritEnergyCostPerLevel;

	[HideInInspector]
	public List<UPGRADE_BONUS> bonuses = new List<UPGRADE_BONUS>();

	[HideInInspector]
	public List<int> additionalDamagePerLevel = new List<int>();

	[HideInInspector]
	public List<float> additionalPiercePerLevel = new List<float>();

	[HideInInspector]
	public List<float> additionalHpPercentagePerLevel = new List<float>();

	[HideInInspector]
	public List<float> additionalAttackPercentagePerLevel = new List<float>();

	[HideInInspector]
	public List<int> additionalHpValuePerLevel = new List<int>();

	[HideInInspector]
	public List<int> additionalAttackValuePerLevel = new List<int>();

	[HideInInspector]
	public List<int> applyOnLevel = new List<int>();

	[HideInInspector]
	public List<float> additionalmanaReceivedPercentagePerLevel = new List<float>();

	[HideInInspector]
	public List<float> statsIncreasedPercentagePerLevel = new List<float>();

	[HideInInspector]
	public List<int> durationBonusPerLevel = new List<int>();

	[HideInInspector]
	public List<float> additionalMaxHPPercentagePerLevel = new List<float>();

	[HideInInspector]
	public List<int> additionalMaxHPActualPerLevel = new List<int>();

	[HideInInspector]
	public List<float> additionalChanceBonusPercentagePerLevel = new List<float>();

	[HideInInspector]
	public List<int> additionalTileRangeBonusPerLevel = new List<int>();

	[HideInInspector]
	public List<int> decreaseMovementSpeedPerLevel = new List<int>();

	[HideInInspector]
	public List<int> cooldownPerLevel = new List<int>();

	[HideInInspector]
	public List<int> skillMovementSpeed = new List<int>();

	[HideInInspector]
	public List<int> physicalResistance = new List<int>();

	[HideInInspector]
	public List<int> elementalResistance = new List<int>();

	[HideInInspector]
	public List<int> secondaryResistance = new List<int>();

	[HideInInspector]
	public List<int> mentalResistance = new List<int>();

	[HideInInspector]
	public List<POWER_ADDED_EFFECT> addedEffectsLevel0 = new List<POWER_ADDED_EFFECT>();

	[HideInInspector]
	public List<POWER_ADDED_EFFECT> addedEffectsLevel1 = new List<POWER_ADDED_EFFECT>();

	[HideInInspector]
	public List<POWER_ADDED_EFFECT> addedEffectsLevel2 = new List<POWER_ADDED_EFFECT>();

	[HideInInspector]
	public List<POWER_ADDED_EFFECT> addedEffectsLevel3 = new List<POWER_ADDED_EFFECT>();

	public int GetUpgradeCostBaseOnLevel(int p_currentLevel)
	{
		if (upgradeCosts == null || upgradeCosts.Count <= 0)
		{
			return 0;
		}
		if (p_currentLevel >= upgradeCosts.Count)
		{
			return upgradeCosts[upgradeCosts.Count - 1];
		}
		return SpellUtilities.GetModifiedSpellCost(upgradeCosts[p_currentLevel], WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease());
	}

	public int GetAdditionalDamageBaseOnLevel(int p_currentLevel)
	{
		if (additionalDamagePerLevel == null || additionalDamagePerLevel.Count <= 0)
		{
			return 0;
		}
		if (p_currentLevel >= additionalDamagePerLevel.Count)
		{
			return additionalDamagePerLevel[additionalDamagePerLevel.Count - 1];
		}
		return additionalDamagePerLevel[p_currentLevel];
	}

	public float GetAdditionalPiercePerLevelBaseOnLevel(int p_currentLevel)
	{
		if (additionalPiercePerLevel == null || additionalPiercePerLevel.Count <= 0)
		{
			return 0f;
		}
		if (p_currentLevel >= additionalPiercePerLevel.Count)
		{
			return additionalPiercePerLevel[additionalPiercePerLevel.Count - 1];
		}
		return additionalPiercePerLevel[p_currentLevel];
	}

	public float GetAdditionalHpPercentagePerLevelBaseOnLevel(int p_currentLevel)
	{
		if (additionalHpPercentagePerLevel == null || additionalHpPercentagePerLevel.Count <= 0)
		{
			return 0f;
		}
		if (p_currentLevel >= additionalHpPercentagePerLevel.Count)
		{
			return additionalHpPercentagePerLevel[additionalHpPercentagePerLevel.Count - 1];
		}
		return additionalHpPercentagePerLevel[p_currentLevel];
	}

	public int GetAdditionalMaxHpActualPerLevelBaseOnLevel(int p_currentLevel)
	{
		if (additionalMaxHPActualPerLevel == null || additionalMaxHPActualPerLevel.Count <= 0)
		{
			return 0;
		}
		if (p_currentLevel >= additionalMaxHPActualPerLevel.Count)
		{
			return additionalMaxHPActualPerLevel[additionalMaxHPActualPerLevel.Count - 1];
		}
		return additionalMaxHPActualPerLevel[p_currentLevel];
	}

	public float GetAdditionalMaxHpPercentagePerLevelBaseOnLevel(int p_currentLevel)
	{
		if (additionalMaxHPPercentagePerLevel == null || additionalMaxHPPercentagePerLevel.Count <= 0)
		{
			return 0f;
		}
		if (p_currentLevel >= additionalMaxHPPercentagePerLevel.Count)
		{
			return additionalMaxHPPercentagePerLevel[additionalMaxHPPercentagePerLevel.Count - 1];
		}
		return additionalMaxHPPercentagePerLevel[p_currentLevel];
	}

	public int GetAdditionalHpActualPerLevelBaseOnLevel(int p_currentLevel)
	{
		if (additionalHpValuePerLevel == null || additionalHpValuePerLevel.Count <= 0)
		{
			return 0;
		}
		if (p_currentLevel >= additionalHpValuePerLevel.Count)
		{
			return additionalHpValuePerLevel[additionalHpValuePerLevel.Count - 1];
		}
		return additionalHpValuePerLevel[p_currentLevel];
	}

	public float GetAdditionalAttackPercentagePerLevelBaseOnLevel(int p_currentLevel)
	{
		if (additionalAttackPercentagePerLevel == null || additionalAttackPercentagePerLevel.Count <= 0)
		{
			return 0f;
		}
		if (p_currentLevel >= additionalAttackPercentagePerLevel.Count)
		{
			return additionalAttackPercentagePerLevel[additionalAttackPercentagePerLevel.Count - 1];
		}
		return additionalAttackPercentagePerLevel[p_currentLevel];
	}

	public int GetAdditionalAttackActualPerLevelBaseOnLevel(int p_currentLevel)
	{
		if (additionalAttackValuePerLevel == null || additionalAttackValuePerLevel.Count <= 0)
		{
			return 0;
		}
		if (p_currentLevel >= additionalAttackValuePerLevel.Count)
		{
			return additionalAttackValuePerLevel[additionalAttackValuePerLevel.Count - 1];
		}
		return additionalAttackValuePerLevel[p_currentLevel];
	}

	public float GetIncreaseStatsPercentagePerLevel(int p_currentLevel)
	{
		if (statsIncreasedPercentagePerLevel == null || statsIncreasedPercentagePerLevel.Count <= 0)
		{
			return 0f;
		}
		if (p_currentLevel >= statsIncreasedPercentagePerLevel.Count)
		{
			return statsIncreasedPercentagePerLevel[statsIncreasedPercentagePerLevel.Count - 1];
		}
		return statsIncreasedPercentagePerLevel[p_currentLevel];
	}

	public int GetDurationBonusPerLevel(int p_currentLevel)
	{
		if (durationBonusPerLevel == null || durationBonusPerLevel.Count <= 0)
		{
			return 0;
		}
		if (p_currentLevel >= durationBonusPerLevel.Count)
		{
			return durationBonusPerLevel[durationBonusPerLevel.Count - 1];
		}
		return durationBonusPerLevel[p_currentLevel];
	}

	public int GetTileRangeBonusPerLevel(int p_currentLevel)
	{
		if (additionalTileRangeBonusPerLevel == null || additionalTileRangeBonusPerLevel.Count <= 0)
		{
			return 0;
		}
		if (p_currentLevel >= additionalTileRangeBonusPerLevel.Count)
		{
			return additionalTileRangeBonusPerLevel[additionalTileRangeBonusPerLevel.Count - 1];
		}
		return additionalTileRangeBonusPerLevel[p_currentLevel];
	}

	public float GetChanceBonusPerLevel(int p_currentLevel)
	{
		if (additionalChanceBonusPercentagePerLevel == null || additionalChanceBonusPercentagePerLevel.Count <= 0)
		{
			return 0f;
		}
		if (p_currentLevel >= additionalChanceBonusPercentagePerLevel.Count)
		{
			return additionalChanceBonusPercentagePerLevel[additionalChanceBonusPercentagePerLevel.Count - 1];
		}
		return additionalChanceBonusPercentagePerLevel[p_currentLevel];
	}

	public int GetManaCostPerLevel(int p_currentLevel)
	{
		if (manacostPerLevel == null || manacostPerLevel.Count <= 0)
		{
			return -1;
		}
		if (p_currentLevel >= manacostPerLevel.Count)
		{
			return manacostPerLevel[manacostPerLevel.Count - 1];
		}
		return manacostPerLevel[p_currentLevel];
	}

	public int GetSpiritEnergyCostPerLevel(int p_currentLevel)
	{
		if (spiritEnergyCostPerLevel == null || spiritEnergyCostPerLevel.Length == 0)
		{
			return -1;
		}
		if (p_currentLevel >= spiritEnergyCostPerLevel.Length)
		{
			return spiritEnergyCostPerLevel[spiritEnergyCostPerLevel.Length - 1];
		}
		return spiritEnergyCostPerLevel[p_currentLevel];
	}

	public int GetCoolDownPerLevel(int p_currentLevel)
	{
		if (cooldownPerLevel == null || cooldownPerLevel.Count <= 0)
		{
			return -1;
		}
		if (p_currentLevel >= cooldownPerLevel.Count)
		{
			return cooldownPerLevel[cooldownPerLevel.Count - 1];
		}
		return cooldownPerLevel[p_currentLevel];
	}

	public int GetChargesBaseOnLevel(int p_currentLevel)
	{
		if (chargesPerLevel == null || chargesPerLevel.Count <= 0)
		{
			return -1;
		}
		if (p_currentLevel >= chargesPerLevel.Count)
		{
			return chargesPerLevel[chargesPerLevel.Count - 1];
		}
		return chargesPerLevel[p_currentLevel];
	}

	public int GetSkillMovementSpeedPerLevel(int p_currentLevel)
	{
		if (skillMovementSpeed == null || skillMovementSpeed.Count <= 0)
		{
			return 0;
		}
		if (p_currentLevel >= skillMovementSpeed.Count)
		{
			return skillMovementSpeed[skillMovementSpeed.Count - 1];
		}
		return skillMovementSpeed[p_currentLevel];
	}

	public string GetDescriptionBaseOnFirstBonus(int p_currentLevel)
	{
		if (bonuses == null || bonuses.Count <= 0)
		{
			return "n/a";
		}
		return GetDescription(bonuses[0], p_currentLevel);
	}

	public int GetPhysicalResistanceBaseOnLevel(int p_currentLevel)
	{
		if (physicalResistance == null || physicalResistance.Count <= 0)
		{
			return 0;
		}
		if (p_currentLevel >= physicalResistance.Count)
		{
			return physicalResistance[physicalResistance.Count - 1];
		}
		return physicalResistance[p_currentLevel];
	}

	public int GetElementalResistanceBaseOnLevel(int p_currentLevel)
	{
		if (elementalResistance == null || elementalResistance.Count <= 0)
		{
			return 0;
		}
		if (p_currentLevel >= elementalResistance.Count)
		{
			return elementalResistance[elementalResistance.Count - 1];
		}
		return elementalResistance[p_currentLevel];
	}

	public int GetSecondaryResistanceBaseOnLevel(int p_currentLevel)
	{
		if (secondaryResistance == null || secondaryResistance.Count <= 0)
		{
			return 0;
		}
		if (p_currentLevel >= secondaryResistance.Count)
		{
			return secondaryResistance[secondaryResistance.Count - 1];
		}
		return secondaryResistance[p_currentLevel];
	}

	public int GetMentalResistanceBaseOnLevel(int p_currentLevel)
	{
		if (mentalResistance == null || mentalResistance.Count <= 0)
		{
			return 0;
		}
		if (p_currentLevel >= mentalResistance.Count)
		{
			return mentalResistance[mentalResistance.Count - 1];
		}
		return mentalResistance[p_currentLevel];
	}

	private string GetDescription(UPGRADE_BONUS p_upgradeBonus, int p_currentLevel)
	{
		switch (p_upgradeBonus)
		{
		case UPGRADE_BONUS.Pierce:
			if (additionalPiercePerLevel.Count <= 0)
			{
				return "Pierce: No Data";
			}
			if (p_currentLevel >= additionalPiercePerLevel.Count)
			{
				p_currentLevel = additionalPiercePerLevel.Count - 1;
			}
			return additionalPiercePerLevel[p_currentLevel] + "%";
		case UPGRADE_BONUS.Damage:
			if (additionalDamagePerLevel.Count <= 0)
			{
				return "Damage: No Data";
			}
			if (p_currentLevel >= additionalDamagePerLevel.Count)
			{
				p_currentLevel = additionalDamagePerLevel.Count - 1;
			}
			return additionalDamagePerLevel[p_currentLevel].ToString();
		case UPGRADE_BONUS.Duration:
			if (durationBonusPerLevel.Count <= 0)
			{
				return "Duration: No Data";
			}
			if (p_currentLevel >= durationBonusPerLevel.Count)
			{
				p_currentLevel = durationBonusPerLevel.Count - 1;
			}
			return durationBonusPerLevel[p_currentLevel].ToString();
		case UPGRADE_BONUS.Max_HP_Percentage:
			if (additionalMaxHPPercentagePerLevel.Count <= 0)
			{
				return "Max HP: No Data";
			}
			if (p_currentLevel >= additionalMaxHPPercentagePerLevel.Count)
			{
				p_currentLevel = additionalMaxHPPercentagePerLevel.Count - 1;
			}
			return additionalMaxHPPercentagePerLevel[p_currentLevel] + "%";
		case UPGRADE_BONUS.Atk_Percentage:
			if (additionalAttackPercentagePerLevel.Count <= 0)
			{
				return "Atk: No Data";
			}
			if (p_currentLevel >= additionalAttackPercentagePerLevel.Count)
			{
				p_currentLevel = additionalAttackPercentagePerLevel.Count - 1;
			}
			return additionalAttackPercentagePerLevel[p_currentLevel] + "%";
		case UPGRADE_BONUS.Amplify_Effect_By_Percentage:
			if (statsIncreasedPercentagePerLevel.Count <= 0)
			{
				return "Effect: No Data";
			}
			if (p_currentLevel >= statsIncreasedPercentagePerLevel.Count)
			{
				p_currentLevel = statsIncreasedPercentagePerLevel.Count - 1;
			}
			return statsIncreasedPercentagePerLevel[p_currentLevel] + "%";
		case UPGRADE_BONUS.Tile_Range:
			if (additionalTileRangeBonusPerLevel.Count <= 0)
			{
				return "Tiles: No Data";
			}
			if (p_currentLevel >= additionalTileRangeBonusPerLevel.Count)
			{
				p_currentLevel = additionalTileRangeBonusPerLevel.Count - 1;
			}
			return additionalTileRangeBonusPerLevel[p_currentLevel] + "%";
		case UPGRADE_BONUS.Cooldown:
			if (cooldownPerLevel.Count <= 0)
			{
				return "Tiles: No Data";
			}
			if (p_currentLevel >= cooldownPerLevel.Count)
			{
				p_currentLevel = cooldownPerLevel.Count - 1;
			}
			return cooldownPerLevel[p_currentLevel] + "%";
		default:
			return "n/a";
		}
	}
}
