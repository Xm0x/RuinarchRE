namespace Crime_System;

public class DemonWorship : CrimeType
{
	public DemonWorship()
		: base(CRIME_TYPE.Demon_Worship)
	{
	}

	public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target)
	{
		if (witness.religionComponent.religion == RELIGION.Demon_Worship)
		{
			return CRIME_SEVERITY.None;
		}
		if (CharacterManager.Instance.IsCultistOfSameReligion(witness, actor))
		{
			return CRIME_SEVERITY.None;
		}
		if (witness.traitContainer.HasTrait("Demon Cultist"))
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
