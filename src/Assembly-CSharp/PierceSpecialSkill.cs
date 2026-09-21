public class PierceSpecialSkill : CombatSpecialSkill
{
	public PierceSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Pierce, COMBAT_SPECIAL_SKILL_TARGET.Single, COMBAT_SPECIAL_SKILL_CATEGORY.Physical, 50, 2)
	{
	}

	public override bool TryActivateSkill(Character p_character)
	{
		if (!p_character.traitContainer.HasTrait("Pierce Buff"))
		{
			p_character.traitContainer.AddTrait(p_character, "Pierce Buff");
			return true;
		}
		return false;
	}
}
