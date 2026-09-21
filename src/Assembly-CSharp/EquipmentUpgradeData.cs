using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

[Serializable]
public class EquipmentUpgradeData
{
	[HideInInspector]
	public List<EQUIPMENT_BONUS> bonuses = new List<EQUIPMENT_BONUS>();

	[HideInInspector]
	public float AdditionalPiercing;

	[HideInInspector]
	public float AdditionalMaxHPPercentage;

	[HideInInspector]
	public int AdditionalMaxHPActual;

	[HideInInspector]
	public float AdditionalAttackPercentage;

	[HideInInspector]
	public int AdditionalAttackActual;

	[HideInInspector]
	public float AdditionalIntPercentage;

	[HideInInspector]
	public int AdditionalIntActual;

	[HideInInspector]
	public int AdditionalCritRate;

	[HideInInspector]
	public float additionalResistanceBonus;

	[HideInInspector]
	public ELEMENTAL_TYPE elementAttackBonus;

	[HideInInspector]
	public EQUIPMENT_SLAYER_BONUS slayerBonus;

	[HideInInspector]
	public EQUIPMENT_WARD_BONUS wardBonus;

	public float GetProcessedAdditionalPiercing(EQUIPMENT_QUALITY p_quality)
	{
		float num = AdditionalPiercing;
		switch (p_quality)
		{
		case EQUIPMENT_QUALITY.High:
			num += num * 0.25f;
			break;
		case EQUIPMENT_QUALITY.Premium:
			num += num * 0.5f;
			break;
		}
		return num;
	}

	public float GetProcessedAdditionalResistanceBonus(EQUIPMENT_QUALITY p_quality)
	{
		float num = additionalResistanceBonus;
		switch (p_quality)
		{
		case EQUIPMENT_QUALITY.High:
			num += num * 0.25f;
			break;
		case EQUIPMENT_QUALITY.Premium:
			num += num * 0.5f;
			break;
		}
		return num;
	}

	public int GetProcessedAdditionalAttack(EQUIPMENT_QUALITY p_quality)
	{
		int num = AdditionalAttackActual;
		switch (p_quality)
		{
		case EQUIPMENT_QUALITY.High:
			num += (int)((float)num * 0.25f);
			break;
		case EQUIPMENT_QUALITY.Premium:
			num += (int)((float)num * 0.5f);
			break;
		}
		return num;
	}

	public float GetProcessedAdditionalAttackPercentage(EQUIPMENT_QUALITY p_quality)
	{
		float num = AdditionalAttackPercentage;
		switch (p_quality)
		{
		case EQUIPMENT_QUALITY.High:
			num += (float)(int)(num * 0.25f);
			break;
		case EQUIPMENT_QUALITY.Premium:
			num += (float)(int)(num * 0.5f);
			break;
		}
		return num;
	}

	public int GetProcessedAdditionalInt(EQUIPMENT_QUALITY p_quality)
	{
		int num = AdditionalIntActual;
		switch (p_quality)
		{
		case EQUIPMENT_QUALITY.High:
			num += (int)((float)num * 0.25f);
			break;
		case EQUIPMENT_QUALITY.Premium:
			num += (int)((float)num * 0.5f);
			break;
		}
		return num;
	}

	public float GetProcessedAdditionalIntPercentage(EQUIPMENT_QUALITY p_quality)
	{
		float num = AdditionalIntPercentage;
		switch (p_quality)
		{
		case EQUIPMENT_QUALITY.High:
			num += (float)(int)(num * 0.25f);
			break;
		case EQUIPMENT_QUALITY.Premium:
			num += (float)(int)(num * 0.5f);
			break;
		}
		return num;
	}

	public int GetProcessedAdditionalCritRate(EQUIPMENT_QUALITY p_quality)
	{
		int num = AdditionalCritRate;
		switch (p_quality)
		{
		case EQUIPMENT_QUALITY.High:
			num += (int)((float)num * 0.25f);
			break;
		case EQUIPMENT_QUALITY.Premium:
			num += (int)((float)num * 0.5f);
			break;
		}
		return num;
	}

	public int GetProcessedAdditionalmaxHP(EQUIPMENT_QUALITY p_quality)
	{
		int num = AdditionalMaxHPActual;
		switch (p_quality)
		{
		case EQUIPMENT_QUALITY.High:
			num += (int)((float)num * 0.25f);
			break;
		case EQUIPMENT_QUALITY.Premium:
			num += (int)((float)num * 0.5f);
			break;
		}
		return num;
	}

	public float GetProcessedAdditionalmaxHPPercentage(EQUIPMENT_QUALITY p_quality)
	{
		float num = AdditionalMaxHPPercentage;
		switch (p_quality)
		{
		case EQUIPMENT_QUALITY.High:
			num += (float)(int)(num * 0.25f);
			break;
		case EQUIPMENT_QUALITY.Premium:
			num += (float)(int)(num * 0.5f);
			break;
		}
		return num;
	}

	public string GetBonusDescription(EQUIPMENT_QUALITY p_quality, ELEMENTAL_TYPE p_element = ELEMENTAL_TYPE.Normal)
	{
		string text = string.Empty;
		if (bonuses.Contains(EQUIPMENT_BONUS.Increased_Piercing) && AdditionalPiercing > 0f)
		{
			text += $"+{Mathf.Round(GetProcessedAdditionalPiercing(p_quality))}{Utilities.PiercingIcon()}\n";
		}
		if (bonuses.Contains(EQUIPMENT_BONUS.Str_Actual) && AdditionalAttackActual > 0)
		{
			text += string.Format("+{0} {1}\n", GetProcessedAdditionalAttack(p_quality), LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Damage_Title"));
		}
		if (bonuses.Contains(EQUIPMENT_BONUS.Str_Percentage) && AdditionalAttackPercentage > 0f)
		{
			text += string.Format("+{0}% {1}\n", Mathf.Round(GetProcessedAdditionalAttackPercentage(p_quality)), LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Damage_Title"));
		}
		if (bonuses.Contains(EQUIPMENT_BONUS.Max_HP_Actual) && AdditionalMaxHPActual > 0)
		{
			text += string.Format("+{0} {1}\n", GetProcessedAdditionalmaxHP(p_quality), LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Hitpoints"));
		}
		if (bonuses.Contains(EQUIPMENT_BONUS.Max_HP_Percentage) && AdditionalMaxHPPercentage > 0f)
		{
			text += string.Format("+{0}% {1}\n", Mathf.Round(GetProcessedAdditionalmaxHPPercentage(p_quality)), LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Hitpoints"));
		}
		if (bonuses.Contains(EQUIPMENT_BONUS.Int_Actual) && AdditionalIntActual > 0 && AdditionalAttackActual <= 0)
		{
			text += string.Format("+{0} {1}\n", GetProcessedAdditionalInt(p_quality), LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Damage_Title"));
		}
		if (bonuses.Contains(EQUIPMENT_BONUS.Int_Percentage) && AdditionalIntPercentage > 0f && AdditionalAttackPercentage <= 0f)
		{
			text += string.Format("+{0}% {1}\n", Mathf.Round(GetProcessedAdditionalIntPercentage(p_quality)), LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Damage_Title"));
		}
		if (bonuses.Contains(EQUIPMENT_BONUS.Crit_Rate_Actual))
		{
			text += string.Format("+{0}% {1}\n", Mathf.Round(GetProcessedAdditionalCritRate(p_quality)), LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Crit Rate"));
		}
		return text;
	}

	public string GetDescriptionForRandomResistance(List<RESISTANCE> resistanceBonuses, EQUIPMENT_QUALITY p_quality)
	{
		string description = string.Empty;
		if (bonuses.Contains(EQUIPMENT_BONUS.Increased_3_Random_Resistance) || bonuses.Contains(EQUIPMENT_BONUS.Increased_4_Random_Resistance) || bonuses.Contains(EQUIPMENT_BONUS.Increased_5_Random_Resistance))
		{
			resistanceBonuses.ForEach(delegate(RESISTANCE eachBonus)
			{
				if (eachBonus != RESISTANCE.None && additionalResistanceBonus > 0f)
				{
					string text4 = eachBonus.LocalizedName();
					description = $"{description}+{GetProcessedAdditionalResistanceBonus(p_quality)} {Utilities.GetRichTextIconForResistance(eachBonus)} {text4}\n";
				}
			});
		}
		else if (bonuses.Contains(EQUIPMENT_BONUS.Mental_Resistance))
		{
			string text = RESISTANCE.Mental.LocalizedName();
			description = $"{description}+{GetProcessedAdditionalResistanceBonus(p_quality)} {Utilities.GetRichTextIconForResistance(RESISTANCE.Mental)} {text}\n";
		}
		else if (bonuses.Contains(EQUIPMENT_BONUS.Normal_Resistance))
		{
			string text2 = RESISTANCE.Physical.LocalizedName();
			description = $"{description}+{GetProcessedAdditionalResistanceBonus(p_quality)} {Utilities.GetRichTextIconForResistance(RESISTANCE.Physical)} {text2}\n";
		}
		else if (bonuses.Contains(EQUIPMENT_BONUS.Secondary_Resistances))
		{
			RESISTANCE[] enumValues = CollectionUtilities.GetEnumValues<RESISTANCE>();
			foreach (RESISTANCE rESISTANCE in enumValues)
			{
				if (rESISTANCE != RESISTANCE.None && rESISTANCE.IsSecondary())
				{
					string text3 = rESISTANCE.LocalizedName();
					description = $"{description}+{GetProcessedAdditionalResistanceBonus(p_quality)} {Utilities.GetRichTextIconForResistance(rESISTANCE)} {text3}\n";
				}
			}
		}
		return description;
	}
}
