namespace Character_Talents;

public class HealingMagicData : CharacterTalentData
{
	public override bool hasReevaluation => true;

	public HealingMagicData()
		: base(CHARACTER_TALENT.Healing_Magic)
	{
	}

	public override void OnLevelUp(Character p_character, int level)
	{
		switch (level)
		{
		case 1:
			Level1(p_character);
			break;
		case 2:
			Level2(p_character);
			break;
		case 3:
			Level3(p_character);
			break;
		case 4:
			Level4(p_character);
			break;
		case 5:
			Level5(p_character);
			break;
		}
	}

	public override void OnLevelUpAsAWhole(Character p_character)
	{
	}

	private void Level1(Character p_character)
	{
	}

	private void Level2(Character p_character)
	{
		if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL)
		{
			p_character.combatComponent.specialSkillParent.SetSpecialSkillForTalentSkills(COMBAT_SPECIAL_SKILL.Heal);
		}
	}

	private void Level3(Character p_character)
	{
		if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL)
		{
			p_character.combatComponent.specialSkillParent.SetSpecialSkillForTalentSkills(COMBAT_SPECIAL_SKILL.Strong_Heal);
		}
	}

	private void Level4(Character p_character)
	{
		if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL)
		{
			p_character.combatComponent.specialSkillParent.SetSpecialSkillForTalentSkills(COMBAT_SPECIAL_SKILL.Max_Heal);
		}
	}

	private void Level5(Character p_character)
	{
		if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL)
		{
			p_character.combatComponent.specialSkillParent.SetSpecialSkillForTalentSkills(COMBAT_SPECIAL_SKILL.Group_Heal);
		}
	}

	public override string GetAdditionalBonusDescription(Character p_character, int p_level)
	{
		return string.Empty;
	}

	public override void OnReevaluateTalentPerLevel(Character p_character, int level)
	{
		switch (level)
		{
		case 1:
			ReevaluateLevel1(p_character);
			break;
		case 2:
			ReevaluateLevel2(p_character);
			break;
		case 3:
			ReevaluateLevel3(p_character);
			break;
		case 4:
			ReevaluateLevel4(p_character);
			break;
		case 5:
			ReevaluateLevel5(p_character);
			break;
		}
	}

	public override void OnReevaluateTalentAsAWhole(Character p_character)
	{
	}

	private void ReevaluateLevel1(Character p_character)
	{
	}

	private void ReevaluateLevel2(Character p_character)
	{
		if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL)
		{
			p_character.combatComponent.specialSkillParent.SetSpecialSkillForTalentSkills(COMBAT_SPECIAL_SKILL.Heal);
		}
	}

	private void ReevaluateLevel3(Character p_character)
	{
		if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL)
		{
			p_character.combatComponent.specialSkillParent.SetSpecialSkillForTalentSkills(COMBAT_SPECIAL_SKILL.Strong_Heal);
		}
	}

	private void ReevaluateLevel4(Character p_character)
	{
		if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL)
		{
			p_character.combatComponent.specialSkillParent.SetSpecialSkillForTalentSkills(COMBAT_SPECIAL_SKILL.Max_Heal);
		}
	}

	private void ReevaluateLevel5(Character p_character)
	{
		if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL)
		{
			p_character.combatComponent.specialSkillParent.SetSpecialSkillForTalentSkills(COMBAT_SPECIAL_SKILL.Group_Heal);
		}
	}
}
