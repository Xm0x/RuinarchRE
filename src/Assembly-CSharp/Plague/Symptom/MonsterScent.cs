using UtilityScripts;

namespace Plague.Symptom;

public class MonsterScent : PlagueSymptom
{
	public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Monster_Scent;

	protected override void ActivateSymptom(Character p_character)
	{
		if (p_character.currentRegion != null && p_character.gridTileLocation != null && !p_character.isInLimbo)
		{
			p_character.currentRegion.GetRandomCharacterForMonsterScent(p_character)?.combatComponent.Fight(p_character, "Monster_Scent");
		}
	}

	protected override bool CanActivateSymptomOn(Character p_character)
	{
		if (base.CanActivateSymptomOn(p_character))
		{
			return p_character.isNotSummonAndDemon;
		}
		return false;
	}

	public override void HourStarted(Character p_character, int p_numOfHoursPassed)
	{
		if (GameUtilities.RollChance(5))
		{
			ActivateSymptomOn(p_character);
		}
	}
}
