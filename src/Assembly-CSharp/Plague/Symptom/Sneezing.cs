using UtilityScripts;

namespace Plague.Symptom;

public class Sneezing : PlagueSymptom
{
	public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Sneezing;

	protected override void ActivateSymptom(Character p_character)
	{
		p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Sneeze, p_character);
		if (PlayerManager.Instance.player.currenciesComponent.CanGainPlaguePoints())
		{
			PlayerManager.Instance.player.currenciesComponent.GainPlaguePointFromCharacter(1, p_character);
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
