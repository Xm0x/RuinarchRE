public class BloatedChoice : InfuseChoice
{
	public BloatedChoice()
	{
		base.name = "Bloated";
		base.localizedName = LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", base.name);
	}

	public override void Infuse(FoodPile p_foodPile)
	{
		p_foodPile.SetInfusedType(FOOD_INFUSE_TYPE.Bloated);
	}

	public override bool IsValid()
	{
		ManifestFoodData manifestFoodData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.MANIFEST_FOOD) as ManifestFoodData;
		if (!manifestFoodData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Unlock_Bloated) && !manifestFoodData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Unlock_Bloated_FoodComa))
		{
			return manifestFoodData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Unlock_Bloated_FoodComa_Rabid);
		}
		return true;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(TileObject p_tileObject)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(p_tileObject);
		if (p_tileObject is FoodPile { infusedType: not FOOD_INFUSE_TYPE.None })
		{
			text = text + LocalizationManager.Instance.GetLocalizedValue("PlayerPowerReasons_Table", "Already_Infused") + "\n";
		}
		return text;
	}
}
