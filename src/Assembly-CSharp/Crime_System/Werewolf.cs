namespace Crime_System;

public class Werewolf : CrimeType
{
	public Werewolf()
		: base(CRIME_TYPE.Werewolf)
	{
	}

	public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target)
	{
		if (CharacterManager.Instance.IsCultistOfSameReligion(witness, actor))
		{
			return CRIME_SEVERITY.None;
		}
		if (witness.isLycanthrope && witness.lycanData.activeForm == witness.lycanData.originalForm && witness.lycanData.dislikesBeingLycan)
		{
			return CRIME_SEVERITY.Unapplicable;
		}
		if (witness.traitContainer.HasTrait("Lycanphiliac"))
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
