namespace Crime_System;

public class Assault : CrimeType
{
	public Assault()
		: base(CRIME_TYPE.Assault)
	{
	}

	public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target)
	{
		if (target is Character character)
		{
			bool num = character.crimeComponent.IsAnActiveCrimeWitnessedBy(witness);
			bool flag = false;
			CombatData combatData = actor.combatComponent.GetCombatData(character);
			if (combatData != null && combatData.attackBecauseOfCrime)
			{
				flag = true;
			}
			if (num && flag)
			{
				return CRIME_SEVERITY.None;
			}
		}
		return base.GetCrimeSeverity(witness, actor, target);
	}
}
