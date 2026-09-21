public class SelfHealSpecialSkill : CombatSpecialSkill
{
	public SelfHealSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Self_Heal, COMBAT_SPECIAL_SKILL_TARGET.Single, COMBAT_SPECIAL_SKILL_CATEGORY.Healing, 20, 1)
	{
	}

	public override bool TryActivateSkill(Character p_character)
	{
		Character validTargetFor = GetValidTargetFor(p_character);
		if (validTargetFor != null)
		{
			validTargetFor.AdjustHP(100, ELEMENTAL_TYPE.Normal);
			GameManager.Instance.CreateParticleEffectAt(validTargetFor, PARTICLE_EFFECT.Heal, allowRotation: false);
			p_character.talentComponent?.GetTalent(CHARACTER_TALENT.Healing_Magic).AdjustExperience(3, p_character);
			return true;
		}
		return base.TryActivateSkill(p_character);
	}

	protected override Character GetValidTargetFor(Character p_character)
	{
		return p_character;
	}
}
