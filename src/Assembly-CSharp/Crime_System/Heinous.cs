namespace Crime_System;

public class Heinous : CrimeSeverity
{
	public Heinous()
		: base(CRIME_SEVERITY.Heinous)
	{
	}

	public override void Effect(Character witness, Character actor, IPointOfInterest target, CrimeType crimeType, ICrimeable crime, REACTION_STATUS reactionStatus)
	{
		witness.relationshipContainer.AdjustOpinion(witness, actor, base.localizedName + ": " + crimeType.localizedName, -40, crimeType.GetLastStrawReason(witness, actor, target, crime));
	}
}
