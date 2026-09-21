using System;
using UnityEngine.UI.Extensions;
using UtilityScripts;

[Serializable]
public struct Reward
{
	[ReadOnly]
	public string name;

	public CURRENCY currency;

	public int amount;

	public Reward(CURRENCY p_currency, int p_amount)
	{
		currency = p_currency;
		amount = p_amount;
		name = amount + " " + currency.ToStringEnum();
	}

	public override string ToString()
	{
		return amount + " " + currency.ToStringEnum();
	}

	public string GetCostStringWithIcon()
	{
		string empty = string.Empty;
		return string.Concat(str1: currency switch
		{
			CURRENCY.Mana => Utilities.ManaIcon(), 
			CURRENCY.Chaotic_Energy => Utilities.ChaoticEnergyIcon(), 
			CURRENCY.Spirit_Energy => Utilities.SpiritEnergyIcon(), 
			_ => string.Empty, 
		}, str0: amount.ToString());
	}
}
