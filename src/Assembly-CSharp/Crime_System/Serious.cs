namespace Crime_System;

public class Serious : CrimeSeverity
{
	public Serious()
		: base(CRIME_SEVERITY.Serious)
	{
	}

	public override void Effect(Character witness, Character actor, IPointOfInterest target, CrimeType crimeType, ICrimeable crime, REACTION_STATUS reactionStatus)
	{
		witness.relationshipContainer.AdjustOpinion(witness, actor, base.localizedName + ": " + crimeType.localizedName, -20, crimeType.GetLastStrawReason(witness, actor, target, crime));
	}
}
