namespace Crime_System;

public class Murder : CrimeType
{
	public Murder()
		: base(CRIME_TYPE.Murder)
	{
	}

	public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target)
	{
		if (witness.traitContainer.HasTrait("Psychopath"))
		{
			return CRIME_SEVERITY.None;
		}
		return base.GetCrimeSeverity(witness, actor, target);
	}
}
