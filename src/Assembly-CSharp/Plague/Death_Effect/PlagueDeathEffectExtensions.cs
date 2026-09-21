using System;
using UtilityScripts;

namespace Plague.Death_Effect;

public static class PlagueDeathEffectExtensions
{
	public static int GetUnlockCost(this PLAGUE_DEATH_EFFECT p_deathEffect)
	{
		return p_deathEffect switch
		{
			PLAGUE_DEATH_EFFECT.Explosion => SpellUtilities.GetModifiedSpellCost(25, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_DEATH_EFFECT.Zombie => SpellUtilities.GetModifiedSpellCost(25, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_DEATH_EFFECT.Chaos_Generator => SpellUtilities.GetModifiedSpellCost(25, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			PLAGUE_DEATH_EFFECT.Haunted_Spirits => SpellUtilities.GetModifiedSpellCost(25, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease()), 
			_ => throw new ArgumentOutOfRangeException("p_deathEffect", p_deathEffect, null), 
		};
	}

	public static string GetEffectTooltip(this PLAGUE_DEATH_EFFECT p_deathEffect, int p_level)
	{
		return LocalizationManager.Instance.GetLocalizedValue("Plague_Table", $"{p_deathEffect.ToStringEnum()}_Tooltip_{p_level}");
	}
}
