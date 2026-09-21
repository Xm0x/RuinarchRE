using System;
using UtilityScripts;

namespace Plague.Fatality;

public static class FatalityExtensions
{
	public static int GetFatalityCost(this PLAGUE_FATALITY fatality)
	{
		return fatality switch
		{
			PLAGUE_FATALITY.Septic_Shock => SpellUtilities.GetModifiedSpellCost(30, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_FATALITY.Heart_Attack => SpellUtilities.GetModifiedSpellCost(30, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_FATALITY.Stroke => SpellUtilities.GetModifiedSpellCost(20, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_FATALITY.Total_Organ_Failure => SpellUtilities.GetModifiedSpellCost(40, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_FATALITY.Pneumonia => SpellUtilities.GetModifiedSpellCost(40, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			_ => throw new ArgumentOutOfRangeException("fatality", fatality, null), 
		};
	}

	public static string GetFatalityTooltip(this PLAGUE_FATALITY fatality)
	{
		return LocalizationManager.Instance.GetLocalizedValue("Plague_Table", fatality.ToStringEnum() + "_Tooltip");
	}
}
