using Character_Talents;

public class SaveDataCharacterTalent : SaveData<CharacterTalent>
{
	public CHARACTER_TALENT talentType;

	public int experience;

	public int level;

	public override void Save(CharacterTalent data)
	{
		talentType = data.talentType;
		experience = data.experience;
		level = data.level;
	}

	public override CharacterTalent Load()
	{
		CharacterTalent characterTalent = new CharacterTalent();
		characterTalent.SetTalentType(talentType);
		characterTalent.SetExperience(experience);
		characterTalent.SetLevel(level);
		return characterTalent;
	}
}
