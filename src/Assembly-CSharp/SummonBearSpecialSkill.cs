public class SummonBearSpecialSkill : CombatSpecialSkill
{
	public SummonBearSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Summon_Bear, COMBAT_SPECIAL_SKILL_TARGET.Single, COMBAT_SPECIAL_SKILL_CATEGORY.Magical, 100, 2)
	{
	}

	public override bool TryActivateSkill(Character p_character)
	{
		if (!p_character.petComponent.HasPetOfType(SUMMON_TYPE.Bear) && !p_character.petComponent.ownedPetsData.IsAtMaxCapacity())
		{
			Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Bear, p_character.faction, p_character.homeSettlement, p_character.homeRegion, p_character.homeStructure, "", bypassIdeologyChecking: true);
			CharacterManager.Instance.PlaceSummonInitially(summon, p_character.gridTileLocation);
			p_character.petComponent.AddPet(summon, p_setRelationship: false);
			summon.traitContainer.AddTrait(summon, "Temporal");
			return true;
		}
		return false;
	}
}
