using System;

[AttributeUsage(AttributeTargets.Field)]
public class PlayerSkillSubCategoryOf : Attribute
{
	public PLAYER_SKILL_CATEGORY Category { get; private set; }

	public PlayerSkillSubCategoryOf(PLAYER_SKILL_CATEGORY cat)
	{
		Category = cat;
	}
}
