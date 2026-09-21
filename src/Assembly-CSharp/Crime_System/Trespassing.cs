namespace Crime_System;

public class Trespassing : CrimeType
{
	public Trespassing()
		: base(CRIME_TYPE.Trespassing)
	{
	}

	public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target)
	{
		if (witness.faction != target.factionOwner)
		{
			return CRIME_SEVERITY.None;
		}
		return base.GetCrimeSeverity(witness, actor, target);
	}
}
