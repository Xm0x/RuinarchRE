using System;
using UnityEngine.UI.Extensions;
using UtilityScripts;

[Serializable]
public struct Cost
{
	[ReadOnly]
	public string name;

	public CURRENCY currency;

	public int amount;

	public int processedAmount
	{
		get
		{
			if (currency == CURRENCY.Chaotic_Energy && CharacterManager.Instance != null)
			{
				return SpellUtilities.GetModifiedSpellCost(amount, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease());
			}
			return SpellUtilities.GetModifiedSpellCost(amount, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
		}
	}

	public Cost(CURRENCY p_currency, int p_amount)
	{
		currency = p_currency;
		amount = p_amount;
		name = ((currency != CURRENCY.Chaotic_Energy || !(CharacterManager.Instance != null)) ? SpellUtilities.GetModifiedSpellCost(amount, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification()) : SpellUtilities.GetModifiedSpellCost(amount, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease())) + " " + currency.ToStringEnum();
	}

	public override string ToString()
	{
		if (WorldSettings.Instance != null)
		{
			return processedAmount + " " + currency.ToStringEnum();
		}
		return amount + " " + currency.ToStringEnum();
	}

	public string GetCostStringWithIcon()
	{
		string empty = string.Empty;
		return string.Format(arg1: currency switch
		{
			CURRENCY.Mana => Utilities.ManaIcon(), 
			CURRENCY.Chaotic_Energy => Utilities.ChaoticEnergyIcon(), 
			CURRENCY.Spirit_Energy => Utilities.SpiritEnergyIcon(), 
			_ => string.Empty, 
		}, format: "{0}{1}", arg0: processedAmount);
	}
}
