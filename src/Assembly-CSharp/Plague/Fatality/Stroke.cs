using Traits;
using UtilityScripts;

namespace Plague.Fatality;

public class Stroke : Fatality
{
	public override PLAGUE_FATALITY fatalityType => PLAGUE_FATALITY.Stroke;

	protected override void ActivateFatality(Character p_character)
	{
		p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Stroke, p_character);
		PlagueDisease.Instance.UpdateDeathsOnCharacterDied(p_character);
	}

	public override void CharacterGainedTrait(Character p_character, Trait p_gainedTrait)
	{
		if (p_gainedTrait.name == "Tired")
		{
			if (GameUtilities.RollChance(15))
			{
				ActivateFatalityOn(p_character);
			}
		}
		else if (p_gainedTrait.name == "Exhausted" && GameUtilities.RollChance(30))
		{
			ActivateFatalityOn(p_character);
		}
	}
}
