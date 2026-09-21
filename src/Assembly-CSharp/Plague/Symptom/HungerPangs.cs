using UtilityScripts;

namespace Plague.Symptom;

public class HungerPangs : PlagueSymptom
{
	public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Hunger_Pangs;

	protected override void ActivateSymptom(Character p_character)
	{
		if (p_character.needsComponent.HasNeeds())
		{
			p_character.needsComponent.AdjustFullness(-10f);
		}
	}

	public override void PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		if (GameUtilities.RollChance(2.5f))
		{
			ActivateSymptomOn(p_character);
		}
	}
}
