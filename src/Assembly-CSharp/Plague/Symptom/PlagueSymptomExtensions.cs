using System;
using UtilityScripts;

namespace Plague.Symptom;

public static class PlagueSymptomExtensions
{
	public static int GetSymptomCost(this PLAGUE_SYMPTOM p_symptom)
	{
		return p_symptom switch
		{
			PLAGUE_SYMPTOM.Paralysis => SpellUtilities.GetModifiedSpellCost(30, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_SYMPTOM.Vomiting => SpellUtilities.GetModifiedSpellCost(60, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_SYMPTOM.Lethargy => SpellUtilities.GetModifiedSpellCost(20, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_SYMPTOM.Seizure => SpellUtilities.GetModifiedSpellCost(60, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_SYMPTOM.Insomnia => SpellUtilities.GetModifiedSpellCost(20, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_SYMPTOM.Poison_Cloud => SpellUtilities.GetModifiedSpellCost(40, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_SYMPTOM.Monster_Scent => SpellUtilities.GetModifiedSpellCost(20, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_SYMPTOM.Sneezing => SpellUtilities.GetModifiedSpellCost(60, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_SYMPTOM.Depression => SpellUtilities.GetModifiedSpellCost(30, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_SYMPTOM.Hunger_Pangs => SpellUtilities.GetModifiedSpellCost(20, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			_ => throw new ArgumentOutOfRangeException("p_symptom", p_symptom, null), 
		};
	}

	public static string GetSymptomTooltip(this PLAGUE_SYMPTOM p_symptom)
	{
		return LocalizationManager.Instance.GetLocalizedValue("Plague_Table", p_symptom.ToStringEnum() + "_Tooltip");
	}
}
