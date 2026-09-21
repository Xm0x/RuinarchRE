public class SummonWolfSpecialSkill : CombatSpecialSkill
{
	public SummonWolfSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Summon_Wolf, COMBAT_SPECIAL_SKILL_TARGET.Single, COMBAT_SPECIAL_SKILL_CATEGORY.Magical, 100, 1)
	{
	}

	public override bool TryActivateSkill(Character p_character)
	{
		if (!p_character.petComponent.HasPetOfType(SUMMON_TYPE.Wolf) && !p_character.petComponent.ownedPetsData.IsAtMaxCapacity())
		{
			Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Wolf, p_character.faction, p_character.homeSettlement, p_character.homeRegion, p_character.homeStructure, "", bypassIdeologyChecking: true);
			CharacterManager.Instance.PlaceSummonInitially(summon, p_character.gridTileLocation);
			p_character.petComponent.AddPet(summon, p_setRelationship: false);
			summon.traitContainer.AddTrait(summon, "Temporal");
			return true;
		}
		return false;
	}
}
