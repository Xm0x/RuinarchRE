using System;
using UtilityScripts;

[Serializable]
public class PrimordialStatsData
{
	public int lvlStr;

	public int lvlInt;

	public int lvlPiercing;

	public int lvlMentalResistance;

	public int lvlPhysicalResistance;

	public int lvlElementalResistance;

	public int lvlSecondaryResistance;

	public float[] upgradePerLevelStrength = new float[6] { 0f, 5f, 5f, 5f, 5f, 5f };

	public float[] upgradePerLevelIntelligence = new float[6] { 0f, 5f, 5f, 5f, 5f, 5f };

	public float[] upgradePerLevelPiercing = new float[6] { 0f, 20f, 20f, 20f, 20f, 20f };

	public float[] upgradePerLevelMentalResistance = new float[6] { 0f, 10f, 10f, 10f, 10f, 10f };

	public float[] upgradePerLevelPhysicalResistance = new float[6] { 0f, 10f, 10f, 10f, 10f, 10f };

	public float[] upgradePerLevelElementalResistance = new float[6] { 0f, 10f, 10f, 10f, 10f, 10f };

	public float[] upgradePerLevelSecondaryResistance = new float[6] { 0f, 10f, 10f, 10f, 10f, 10f };

	public float[] costPerLevelStrength = new float[6] { 10f, 15f, 20f, 25f, 30f, -1f };

	public float[] costPerLevelIntelligence = new float[6] { 10f, 15f, 20f, 25f, 30f, -1f };

	public float[] costPerLevelPiercing = new float[6] { 10f, 25f, 50f, 75f, 100f, -1f };

	public float[] costPerLevelMentalResistance = new float[6] { 10f, 15f, 20f, 25f, 30f, -1f };

	public float[] costPerLevelPhysicalResistance = new float[6] { 20f, 30f, 40f, 50f, 60f, -1f };

	public float[] costPerLevelElementalResistance = new float[6] { 20f, 30f, 40f, 50f, 60f, -1f };

	public float[] costPerLevelSecondaryResistance = new float[6] { 10f, 15f, 20f, 25f, 30f, -1f };

	public void LevelUp(PRIMORDIAL_STATS_BONUS p_bonus)
	{
		switch (p_bonus)
		{
		case PRIMORDIAL_STATS_BONUS.Str:
			lvlStr++;
			break;
		case PRIMORDIAL_STATS_BONUS.Int:
			lvlInt++;
			break;
		case PRIMORDIAL_STATS_BONUS.Piercing:
			lvlPiercing++;
			break;
		case PRIMORDIAL_STATS_BONUS.Mental_Res:
			lvlMentalResistance++;
			break;
		case PRIMORDIAL_STATS_BONUS.Physical_Res:
			lvlPhysicalResistance++;
			break;
		case PRIMORDIAL_STATS_BONUS.Elemental_Res:
			lvlElementalResistance++;
			break;
		case PRIMORDIAL_STATS_BONUS.Secondary_Res:
			lvlSecondaryResistance++;
			break;
		default:
			throw new ArgumentOutOfRangeException("p_bonus", p_bonus, null);
		}
	}

	public float GetCurrentUpgradeCost(PRIMORDIAL_STATS_BONUS p_bonus)
	{
		return SpellUtilities.GetModifiedSpellCost(p_bonus switch
		{
			PRIMORDIAL_STATS_BONUS.Str => costPerLevelStrength[lvlStr], 
			PRIMORDIAL_STATS_BONUS.Int => costPerLevelIntelligence[lvlInt], 
			PRIMORDIAL_STATS_BONUS.Piercing => costPerLevelPiercing[lvlPiercing], 
			PRIMORDIAL_STATS_BONUS.Mental_Res => costPerLevelMentalResistance[lvlMentalResistance], 
			PRIMORDIAL_STATS_BONUS.Physical_Res => costPerLevelPhysicalResistance[lvlPhysicalResistance], 
			PRIMORDIAL_STATS_BONUS.Elemental_Res => costPerLevelElementalResistance[lvlElementalResistance], 
			PRIMORDIAL_STATS_BONUS.Secondary_Res => costPerLevelSecondaryResistance[lvlSecondaryResistance], 
			_ => throw new ArgumentOutOfRangeException("p_bonus", p_bonus, null), 
		}, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease());
	}

	public bool CheckIfUpgradeable(int p_chaosOrbs, PRIMORDIAL_STATS_BONUS p_bonus)
	{
		float currentUpgradeCost = GetCurrentUpgradeCost(p_bonus);
		switch (p_bonus)
		{
		case PRIMORDIAL_STATS_BONUS.Str:
			if ((float)p_chaosOrbs >= currentUpgradeCost)
			{
				return true;
			}
			break;
		case PRIMORDIAL_STATS_BONUS.Int:
			if ((float)p_chaosOrbs >= currentUpgradeCost)
			{
				return true;
			}
			break;
		case PRIMORDIAL_STATS_BONUS.Piercing:
			if ((float)p_chaosOrbs >= currentUpgradeCost)
			{
				return true;
			}
			break;
		case PRIMORDIAL_STATS_BONUS.Mental_Res:
			if ((float)p_chaosOrbs >= currentUpgradeCost)
			{
				return true;
			}
			break;
		case PRIMORDIAL_STATS_BONUS.Physical_Res:
			if ((float)p_chaosOrbs >= currentUpgradeCost)
			{
				return true;
			}
			break;
		case PRIMORDIAL_STATS_BONUS.Elemental_Res:
			if ((float)p_chaosOrbs >= currentUpgradeCost)
			{
				return true;
			}
			break;
		case PRIMORDIAL_STATS_BONUS.Secondary_Res:
			if ((float)p_chaosOrbs >= currentUpgradeCost)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public float GetCurrentBonus(PRIMORDIAL_STATS_BONUS p_statsBonus)
	{
		return p_statsBonus switch
		{
			PRIMORDIAL_STATS_BONUS.Str => upgradePerLevelStrength[lvlStr], 
			PRIMORDIAL_STATS_BONUS.Int => upgradePerLevelIntelligence[lvlInt], 
			PRIMORDIAL_STATS_BONUS.Piercing => upgradePerLevelPiercing[lvlPiercing], 
			PRIMORDIAL_STATS_BONUS.Mental_Res => upgradePerLevelMentalResistance[lvlMentalResistance], 
			PRIMORDIAL_STATS_BONUS.Physical_Res => upgradePerLevelPhysicalResistance[lvlPhysicalResistance], 
			PRIMORDIAL_STATS_BONUS.Elemental_Res => upgradePerLevelElementalResistance[lvlElementalResistance], 
			PRIMORDIAL_STATS_BONUS.Secondary_Res => upgradePerLevelSecondaryResistance[lvlSecondaryResistance], 
			_ => 0f, 
		};
	}

	public float GetCurrentBonusForTextDisplay(PRIMORDIAL_STATS_BONUS p_statsBonus)
	{
		return p_statsBonus switch
		{
			PRIMORDIAL_STATS_BONUS.Str => upgradePerLevelStrength[lvlStr] * (float)lvlStr, 
			PRIMORDIAL_STATS_BONUS.Int => upgradePerLevelIntelligence[lvlInt] * (float)lvlInt, 
			PRIMORDIAL_STATS_BONUS.Piercing => upgradePerLevelPiercing[lvlPiercing] * (float)lvlPiercing, 
			PRIMORDIAL_STATS_BONUS.Mental_Res => upgradePerLevelMentalResistance[lvlMentalResistance] * (float)lvlMentalResistance, 
			PRIMORDIAL_STATS_BONUS.Physical_Res => upgradePerLevelPhysicalResistance[lvlPhysicalResistance] * (float)lvlPhysicalResistance, 
			PRIMORDIAL_STATS_BONUS.Elemental_Res => upgradePerLevelElementalResistance[lvlElementalResistance] * (float)lvlElementalResistance, 
			PRIMORDIAL_STATS_BONUS.Secondary_Res => upgradePerLevelSecondaryResistance[lvlSecondaryResistance] * (float)lvlSecondaryResistance, 
			_ => 0f, 
		};
	}

	public float GetNextBonusForTextDisplay(PRIMORDIAL_STATS_BONUS p_statsBonus)
	{
		return p_statsBonus switch
		{
			PRIMORDIAL_STATS_BONUS.Str => upgradePerLevelStrength[1] * (float)(lvlStr + 1), 
			PRIMORDIAL_STATS_BONUS.Int => upgradePerLevelIntelligence[1] * (float)(lvlInt + 1), 
			PRIMORDIAL_STATS_BONUS.Piercing => upgradePerLevelPiercing[1] * (float)(lvlPiercing + 1), 
			PRIMORDIAL_STATS_BONUS.Mental_Res => upgradePerLevelMentalResistance[1] * (float)(lvlMentalResistance + 1), 
			PRIMORDIAL_STATS_BONUS.Physical_Res => upgradePerLevelPhysicalResistance[1] * (float)(lvlPhysicalResistance + 1), 
			PRIMORDIAL_STATS_BONUS.Elemental_Res => upgradePerLevelElementalResistance[1] * (float)(lvlElementalResistance + 1), 
			PRIMORDIAL_STATS_BONUS.Secondary_Res => upgradePerLevelSecondaryResistance[1] * (float)(lvlSecondaryResistance + 1), 
			_ => 0f, 
		};
	}

	public float GetAllBonus(PRIMORDIAL_STATS_BONUS p_statsBonus)
	{
		switch (p_statsBonus)
		{
		case PRIMORDIAL_STATS_BONUS.Str:
		{
			float num = 0f;
			for (int j = 0; j <= lvlStr; j++)
			{
				num += upgradePerLevelStrength[j];
			}
			return num;
		}
		case PRIMORDIAL_STATS_BONUS.Int:
		{
			float num = 0f;
			for (int n = 0; n <= lvlInt; n++)
			{
				num += upgradePerLevelIntelligence[n];
			}
			return num;
		}
		case PRIMORDIAL_STATS_BONUS.Piercing:
		{
			float num = 0f;
			for (int k = 0; k <= lvlPiercing; k++)
			{
				num += upgradePerLevelPiercing[k];
			}
			return num;
		}
		case PRIMORDIAL_STATS_BONUS.Mental_Res:
		{
			float num = 0f;
			for (int m = 0; m <= lvlMentalResistance; m++)
			{
				num += upgradePerLevelMentalResistance[m];
			}
			return num;
		}
		case PRIMORDIAL_STATS_BONUS.Physical_Res:
		{
			float num = 0f;
			for (int num2 = 0; num2 <= lvlPhysicalResistance; num2++)
			{
				num += upgradePerLevelPhysicalResistance[num2];
			}
			return num;
		}
		case PRIMORDIAL_STATS_BONUS.Elemental_Res:
		{
			float num = 0f;
			for (int l = 0; l <= lvlElementalResistance; l++)
			{
				num += upgradePerLevelElementalResistance[l];
			}
			return num;
		}
		case PRIMORDIAL_STATS_BONUS.Secondary_Res:
		{
			float num = 0f;
			for (int i = 0; i <= lvlSecondaryResistance; i++)
			{
				num += upgradePerLevelSecondaryResistance[i];
			}
			return num;
		}
		default:
			return 0f;
		}
	}

	public string GetCurrentCostDisplay(PRIMORDIAL_STATS_BONUS p_statsBonus)
	{
		switch (p_statsBonus)
		{
		case PRIMORDIAL_STATS_BONUS.Str:
			if (IsMaxLevel(costPerLevelStrength, lvlStr))
			{
				return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX");
			}
			return GetCurrentUpgradeCost(PRIMORDIAL_STATS_BONUS.Str).ToString();
		case PRIMORDIAL_STATS_BONUS.Int:
			if (IsMaxLevel(costPerLevelIntelligence, lvlInt))
			{
				return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX");
			}
			return GetCurrentUpgradeCost(PRIMORDIAL_STATS_BONUS.Int).ToString();
		case PRIMORDIAL_STATS_BONUS.Piercing:
			if (IsMaxLevel(costPerLevelPiercing, lvlPiercing))
			{
				return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX");
			}
			return GetCurrentUpgradeCost(PRIMORDIAL_STATS_BONUS.Piercing).ToString();
		case PRIMORDIAL_STATS_BONUS.Mental_Res:
			if (IsMaxLevel(costPerLevelMentalResistance, lvlMentalResistance))
			{
				return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX");
			}
			return GetCurrentUpgradeCost(PRIMORDIAL_STATS_BONUS.Mental_Res).ToString();
		case PRIMORDIAL_STATS_BONUS.Physical_Res:
			if (IsMaxLevel(costPerLevelPhysicalResistance, lvlPhysicalResistance))
			{
				return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX");
			}
			return GetCurrentUpgradeCost(PRIMORDIAL_STATS_BONUS.Physical_Res).ToString();
		case PRIMORDIAL_STATS_BONUS.Elemental_Res:
			if (IsMaxLevel(costPerLevelElementalResistance, lvlElementalResistance))
			{
				return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX");
			}
			return GetCurrentUpgradeCost(PRIMORDIAL_STATS_BONUS.Elemental_Res).ToString();
		case PRIMORDIAL_STATS_BONUS.Secondary_Res:
			if (IsMaxLevel(costPerLevelSecondaryResistance, lvlSecondaryResistance))
			{
				return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX");
			}
			return GetCurrentUpgradeCost(PRIMORDIAL_STATS_BONUS.Secondary_Res).ToString();
		default:
			return "0";
		}
	}

	private bool IsMaxLevel(float[] p_costsArray, int p_currentLevel)
	{
		if (p_costsArray.IsIndexInArray(p_currentLevel) && p_costsArray[p_currentLevel] == -1f)
		{
			return true;
		}
		return false;
	}
}
