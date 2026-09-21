using System.Collections.Generic;

namespace Character_Talents;

public class MartialArtsData : CharacterTalentData
{
	public override bool hasReevaluation => true;

	public MartialArtsData()
		: base(CHARACTER_TALENT.Martial_Arts)
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
		ReevaluateKnightBonuses(p_character);
	}

	private void Level1(Character p_character)
	{
		p_character.classComponent.AddAbleClass("Marauder");
		p_character.classComponent.AddAbleClass("Archer");
		p_character.classComponent.SetStalkerCannotBeTurned(p_state: true);
	}

	private void Level2(Character p_character)
	{
		p_character.classComponent.AddAbleClass("Knight");
		p_character.classComponent.AddAbleClass("Stalker");
		p_character.classComponent.SetStalkerDealDoubleDamage(p_state: true);
	}

	private void Level3(Character p_character)
	{
		p_character.classComponent.AddAbleClass("Barbarian");
		p_character.classComponent.AddAbleClass("Hunter");
		p_character.classComponent.SetStalkerCanIdentifyVampiresAndLycans(p_state: true);
	}

	private void Level4(Character p_character)
	{
		p_character.combatComponent.AdjustStrengthPercentModifier(15f);
		p_character.combatComponent.AdjustMaxHPPercentModifier(10f);
		p_character.classComponent.SetStalkerCanIdentifyCultists(p_state: true);
	}

	private void Level5(Character p_character)
	{
		p_character.combatComponent.AdjustStrengthPercentModifier(15f);
		p_character.combatComponent.AdjustMaxHPPercentModifier(10f);
	}

	public void PopulateHighestClasses(List<string> classes, int level)
	{
		switch (level)
		{
		case 3:
			classes.Add("Barbarian");
			classes.Add("Hunter");
			break;
		case 2:
			classes.Add("Knight");
			classes.Add("Stalker");
			break;
		}
	}

	public override string GetAdditionalBonusDescription(Character p_character, int p_level)
	{
		string empty = string.Empty;
		if (p_character.characterClass.className == "Knight")
		{
			switch (p_level)
			{
			case 1:
				return "+20% Bonus Physical Resistance.";
			case 2:
				return "+20% Bonus Physical Resistance.\n+3 HP Regeneration per tick.";
			case 3:
				return "+20% Bonus Physical Resistance.\n+3 HP Regeneration per tick.\n+10 Mood.";
			case 4:
				return "+20% Bonus Physical Resistance.\n+3 HP Regeneration per tick.\n+10 Mood.\nReceives 30% reduced crit chance.";
			case 5:
				return "+20% Bonus Physical Resistance.\n+3 HP Regeneration per tick.\n+10 Mood.\nReceives 30% reduced crit chance.\nSlower needs reduction.";
			}
		}
		return empty;
	}

	public override void OnReevaluateTalentPerLevel(Character p_character, int level)
	{
	}

	public override void OnReevaluateTalentAsAWhole(Character p_character)
	{
		ReevaluateKnightBonuses(p_character);
	}

	private void ReevaluateKnightBonuses(Character p_character)
	{
		if (p_character.characterClass.className == "Knight")
		{
			if (p_character.partyComponent.hasParty)
			{
				p_character.partyComponent.currentParty.UpdateKnightBonusesToParty();
			}
			else
			{
				p_character.traitComponent.ApplyKnightBonuses();
			}
		}
		else if (p_character.partyComponent.hasParty)
		{
			p_character.partyComponent.currentParty.UpdateKnightBonusesToParty();
		}
		else
		{
			p_character.traitComponent.RemoveKnightBonuses();
		}
	}
}
