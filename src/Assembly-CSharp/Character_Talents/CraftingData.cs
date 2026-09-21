namespace Character_Talents;

public class CraftingData : CharacterTalentData
{
	public CraftingData()
		: base(CHARACTER_TALENT.Crafting)
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
		p_character.classComponent.AddAbleClass("Crafter");
	}

	private void Level2(Character p_character)
	{
	}

	private void Level3(Character p_character)
	{
	}

	private void Level4(Character p_character)
	{
	}

	private void Level5(Character p_character)
	{
	}

	public override string GetAdditionalBonusDescription(Character p_character, int p_level)
	{
		return string.Empty;
	}

	public override void OnReevaluateTalentPerLevel(Character p_character, int level)
	{
	}

	public override void OnReevaluateTalentAsAWhole(Character p_character)
	{
	}
}
