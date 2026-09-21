namespace Character_Talents;

public class CharacterTalent
{
	public CHARACTER_TALENT talentType { get; private set; }

	public int experience { get; private set; }

	public int level { get; private set; }

	public void SetTalentType(CHARACTER_TALENT p_talentType)
	{
		talentType = p_talentType;
	}

	public void SetExperience(int p_amount)
	{
		experience = p_amount;
	}

	public void AdjustExperience(int p_amount, Character p_character)
	{
		if (!IsMaxLevel())
		{
			if (p_character.equipmentComponent.HasMentorEquipment())
			{
				p_amount *= 2;
			}
			experience += p_amount;
			if (experience < 0)
			{
				experience = 0;
			}
			else if (experience > 100)
			{
				experience = 100;
			}
			if (experience == 100)
			{
				LevelUp(p_character);
				SetExperience(0);
			}
		}
	}

	public void SetLevel(int p_amount)
	{
		level = p_amount;
		if (level < 1)
		{
			level = 1;
		}
		else if (IsMaxLevel())
		{
			level = 5;
		}
	}

	public bool IsMaxLevel()
	{
		return level >= 5;
	}

	public void SetLevel(int p_amount, Character p_character)
	{
		SetLevel(p_amount);
		ApplyEffectsBasedOnLevel(p_character);
	}

	public void LevelUp(Character p_character)
	{
		SetLevel(level + 1);
		CharacterTalentData orCreateCharacterTalentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(talentType);
		orCreateCharacterTalentData.OnLevelUp(p_character, level);
		orCreateCharacterTalentData.OnLevelUpAsAWhole(p_character);
		p_character.OnThisCharactersTalentLeveledUp(this);
		if (p_character.race == RACE.HUMANS)
		{
			p_character.ApplyClassBonusOnLevelUp(p_character.classComponent.characterClass.className);
		}
	}

	public void LevelUpForInitialVillagersInWorldGen(Character p_character)
	{
		SetLevel(level + 1);
		CharacterTalentData orCreateCharacterTalentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(talentType);
		orCreateCharacterTalentData.OnLevelUp(p_character, level);
		orCreateCharacterTalentData.OnLevelUpAsAWhole(p_character);
		p_character.OnThisCharactersTalentLeveledUp(this);
	}

	public void ApplyEffectsBasedOnLevel(Character p_character)
	{
		CharacterTalentData orCreateCharacterTalentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(talentType);
		for (int i = 1; i <= level; i++)
		{
			orCreateCharacterTalentData.OnLevelUp(p_character, i);
		}
	}

	public void ReevaluateTalent(Character p_character)
	{
		CharacterTalentData orCreateCharacterTalentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(talentType);
		if (orCreateCharacterTalentData.hasReevaluation)
		{
			for (int i = 1; i <= level; i++)
			{
				orCreateCharacterTalentData.OnReevaluateTalentPerLevel(p_character, i);
			}
			orCreateCharacterTalentData.OnReevaluateTalentAsAWhole(p_character);
		}
	}

	public void Reset()
	{
		talentType = CHARACTER_TALENT.None;
		experience = 0;
		level = 1;
	}
}
