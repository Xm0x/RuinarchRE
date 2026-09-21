using UtilityScripts;

namespace Plague.Symptom;

public class Seizure : PlagueSymptom
{
	public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Seizure;

	protected override void ActivateSymptom(Character p_character)
	{
		p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Seizure, p_character);
		if (PlayerManager.Instance.player.currenciesComponent.CanGainPlaguePoints())
		{
			PlayerManager.Instance.player.currenciesComponent.GainPlaguePointFromCharacter(2, p_character);
		}
	}

	public override void PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		if (GameUtilities.RollChance(1.5f))
		{
			ActivateSymptomOn(p_character);
		}
	}
}
