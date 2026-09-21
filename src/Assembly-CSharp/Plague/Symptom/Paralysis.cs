using UtilityScripts;

namespace Plague.Symptom;

public class Paralysis : PlagueSymptom
{
	public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Paralysis;

	protected override void ActivateSymptom(Character p_character)
	{
		if (!p_character.traitContainer.HasTrait("Paralyzed"))
		{
			p_character.traitContainer.AddTrait(p_character, "Paralyzed");
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "plague_paralysis", LOG_TAG.Life_Changes);
			log.AddToFillers(p_character, p_character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
	}

	public override void HourStarted(Character p_character, int p_numOfHoursPassed)
	{
		if (p_numOfHoursPassed == 49 && GameUtilities.RollChance(25))
		{
			ActivateSymptomOn(p_character);
		}
	}
}
