public class StrongHealSpecialSkill : CombatSpecialSkill
{
	public StrongHealSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Strong_Heal, COMBAT_SPECIAL_SKILL_TARGET.Single, COMBAT_SPECIAL_SKILL_CATEGORY.Healing, 20, 3)
	{
	}

	public override bool TryActivateSkill(Character p_character)
	{
		Character validTargetFor = GetValidTargetFor(p_character);
		if (validTargetFor != null)
		{
			validTargetFor.AdjustHP(150, ELEMENTAL_TYPE.Normal);
			GameManager.Instance.CreateParticleEffectAt(validTargetFor, PARTICLE_EFFECT.Heal, allowRotation: false);
			p_character.talentComponent?.GetTalent(CHARACTER_TALENT.Healing_Magic).AdjustExperience(3, p_character);
			return true;
		}
		return base.TryActivateSkill(p_character);
	}

	protected override Character GetValidTargetFor(Character p_character)
	{
		if (p_character.hasMarker)
		{
			Character character = null;
			int num = 0;
			if (!p_character.isDead && !p_character.IsHealthFull())
			{
				character = p_character;
				num = p_character.currentHP;
			}
			for (int i = 0; i < p_character.marker.inVisionCharacters.Count; i++)
			{
				Character character2 = p_character.marker.inVisionCharacters[i];
				if (!character2.isDead && !character2.IsHealthFull() && p_character.faction != null && character2.faction != null && p_character.faction.IsFriendlyWith(character2.faction) && !p_character.combatComponent.IsHostileInRange(character2) && !p_character.combatComponent.IsAvoidInRange(character2) && (character == null || character2.currentHP < num))
				{
					character = character2;
					num = character2.currentHP;
				}
			}
			return character;
		}
		return base.GetValidTargetFor(p_character);
	}
}
