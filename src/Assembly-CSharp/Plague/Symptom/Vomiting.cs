using UtilityScripts;

namespace Plague.Symptom;

public class Vomiting : PlagueSymptom
{
	public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Vomiting;

	protected override void ActivateSymptom(Character p_character)
	{
		p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, p_character, "", null, "Plagued");
		if (PlayerManager.Instance.player.currenciesComponent.CanGainPlaguePoints())
		{
			PlayerManager.Instance.player.currenciesComponent.GainPlaguePointFromCharacter(1, p_character);
		}
	}

	public override void PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		if (GameUtilities.RollChance(2))
		{
			ActivateSymptomOn(p_character);
		}
	}
}
