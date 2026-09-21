namespace Crime_System;

public class Cannibalism : CrimeType
{
	public Cannibalism()
		: base(CRIME_TYPE.Cannibalism)
	{
	}

	public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target)
	{
		if (CharacterManager.Instance.IsCultistOfSameReligion(witness, actor))
		{
			return CRIME_SEVERITY.None;
		}
		if (witness.traitContainer.HasTrait("Cannibal"))
		{
			return CRIME_SEVERITY.None;
		}
		return base.GetCrimeSeverity(witness, actor, target);
	}
}
