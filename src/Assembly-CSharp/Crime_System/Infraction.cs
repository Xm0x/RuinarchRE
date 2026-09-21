namespace Crime_System;

public class Infraction : CrimeSeverity
{
	public Infraction()
		: base(CRIME_SEVERITY.Infraction)
	{
	}

	public override void Effect(Character witness, Character actor, IPointOfInterest target, CrimeType crimeType, ICrimeable crime, REACTION_STATUS reactionStatus)
	{
		witness.relationshipContainer.AdjustOpinion(witness, actor, base.localizedName + ": " + crimeType.localizedName, -5, crimeType.GetLastStrawReason(witness, actor, target, crime));
	}
}
