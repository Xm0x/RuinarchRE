namespace Crime_System;

public class Arson : CrimeType
{
	public Arson()
		: base(CRIME_TYPE.Arson)
	{
	}

	public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target)
	{
		if (witness.traitContainer.HasTrait("Pyromaniac"))
		{
			return CRIME_SEVERITY.None;
		}
		return base.GetCrimeSeverity(witness, actor, target);
	}
}
