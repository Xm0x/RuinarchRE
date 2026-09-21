using System.Collections.Generic;

namespace Character_Talents;

public class CombatMagicData : CharacterTalentData
{
	public CombatMagicData()
		: base(CHARACTER_TALENT.Combat_Magic)
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
		p_character.classComponent.AddAbleClass("Druid");
	}

	private void Level2(Character p_character)
	{
		p_character.classComponent.AddAbleClass("Shaman");
	}

	private void Level3(Character p_character)
	{
		p_character.classComponent.AddAbleClass("Mage");
	}

	private void Level4(Character p_character)
	{
		p_character.combatComponent.AdjustIntelligencePercentModifier(15f);
		p_character.combatComponent.AdjustCritRate(5);
		if (p_character.classComponent.characterClass.className == "Mage")
		{
			p_character.combatComponent.specialSkillParent.SetSpecialSkillForTalentSkills(COMBAT_SPECIAL_SKILL.Polymorph);
		}
	}

	private void Level5(Character p_character)
	{
		p_character.combatComponent.AdjustIntelligencePercentModifier(15f);
		p_character.combatComponent.AdjustCritRate(5);
	}

	public void PopulateHighestClasses(List<string> classes, int level)
	{
		switch (level)
		{
		case 3:
			classes.Add("Mage");
			break;
		case 2:
			classes.Add("Shaman");
			break;
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
	}

	private void ReevaluateLevel3(Character p_character)
	{
	}

	private void ReevaluateLevel4(Character p_character)
	{
		if (p_character.classComponent.characterClass.className == "Mage")
		{
			p_character.combatComponent.specialSkillParent.SetSpecialSkillForTalentSkills(COMBAT_SPECIAL_SKILL.Polymorph);
		}
	}

	private void ReevaluateLevel5(Character p_character)
	{
	}
}
