using Traits;

namespace Crime_System;

public class Vampire : CrimeType
{
	public Vampire()
		: base(CRIME_TYPE.Vampire)
	{
	}

	public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target)
	{
		if (CharacterManager.Instance.IsCultistOfSameReligion(witness, actor))
		{
			return CRIME_SEVERITY.None;
		}
		if (witness.traitContainer.HasTrait("Vampire"))
		{
			if (witness.traitContainer.GetTraitOrStatus<Traits.Vampire>("Vampire").dislikedBeingVampire)
			{
				return CRIME_SEVERITY.Unapplicable;
			}
			return CRIME_SEVERITY.None;
		}
		if (witness.traitContainer.HasTrait("Hemophiliac"))
		{
			return CRIME_SEVERITY.None;
		}
		return base.GetCrimeSeverity(witness, actor, target);
	}

	public override void ProcessReactionOnAccuse(Character p_witness, Character p_actor, IPointOfInterest p_target)
	{
		base.ProcessReactionOnAccuse(p_witness, p_actor, p_target);
		StalkerReactionsToVampireLycanCultist(p_witness, p_actor, p_target);
	}
}
