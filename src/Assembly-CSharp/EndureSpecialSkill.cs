public class EndureSpecialSkill : CombatSpecialSkill
{
	public EndureSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Endure, COMBAT_SPECIAL_SKILL_TARGET.Single, COMBAT_SPECIAL_SKILL_CATEGORY.Physical, 50, 1)
	{
	}

	public override bool TryActivateSkill(Character p_character)
	{
		if (!p_character.traitContainer.HasTrait("Endure Buff"))
		{
			p_character.traitContainer.AddTrait(p_character, "Endure Buff");
			return true;
		}
		return false;
	}
}
