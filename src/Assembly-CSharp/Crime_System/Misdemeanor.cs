namespace Crime_System;

public class Misdemeanor : CrimeSeverity
{
	public Misdemeanor()
		: base(CRIME_SEVERITY.Misdemeanor)
	{
	}

	public override void Effect(Character witness, Character actor, IPointOfInterest target, CrimeType crimeType, ICrimeable crime, REACTION_STATUS reactionStatus)
	{
		witness.relationshipContainer.AdjustOpinion(witness, actor, base.localizedName + ": " + crimeType.localizedName, -10, crimeType.GetLastStrawReason(witness, actor, target, crime));
	}
}
