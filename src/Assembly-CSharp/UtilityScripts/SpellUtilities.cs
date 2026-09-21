using UnityEngine;

namespace UtilityScripts;

public static class SpellUtilities
{
	public static int GetModifiedSpellCost(int p_baseCost, float p_modification, int p_percentageIncrease = 0)
	{
		if (p_baseCost == -1)
		{
			return p_baseCost;
		}
		int num = Mathf.CeilToInt((float)p_baseCost * p_modification);
		return num + Mathf.CeilToInt((float)num * ((float)p_percentageIncrease / 100f));
	}

	public static float GetModifiedSpellCost(float p_baseCost, float p_modification, int p_percentageIncrease = 0)
	{
		if (p_baseCost == -1f)
		{
			return p_baseCost;
		}
		float num = Mathf.Ceil(p_baseCost * p_modification);
		return num + Mathf.Ceil(num * ((float)p_percentageIncrease / 100f));
	}

	public static string GetDisplayOfCurrentChargesWithBonusChargesNotCombined(int charges, int maxCharges, int bonusCharges, bool showCharges)
	{
		string text = string.Empty;
		if (showCharges)
		{
			text += $"{Utilities.ChargesIcon()}{charges}/{maxCharges}";
		}
		if (bonusCharges > 0)
		{
			if (text != string.Empty)
			{
				text += " + ";
			}
			text += $"{Utilities.BonusChargesIcon()}{bonusCharges}";
		}
		return text;
	}
}
