namespace Crime_System;

public class DivineWorship : CrimeType
{
	public DivineWorship()
		: base(CRIME_TYPE.Divine_Worship)
	{
	}

	public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target)
	{
		if (witness.religionComponent.religion == RELIGION.Divine_Worship)
		{
			return CRIME_SEVERITY.None;
		}
		if (CharacterManager.Instance.IsCultistOfSameReligion(witness, actor))
		{
			return CRIME_SEVERITY.None;
		}
		if (witness.traitContainer.HasTrait("Cleric"))
		{
			return CRIME_SEVERITY.None;
		}
		if (witness == target && actor.currentActionNode != null && (actor.currentActionNode.action.goapType == INTERACTION_TYPE.EVANGELIZE || actor.currentActionNode.action.goapType == INTERACTION_TYPE.LIBERATE) && actor.currentActionNode.target == target)
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
