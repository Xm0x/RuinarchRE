public class CastMesmerized : GoapAction
{
	public int m_amountProducedPerTick = 1;

	private const float _coinGainMultiplier = 0.206f;

	public CastMesmerized()
		: base(INTERACTION_TYPE.CAST_MESMERIZED)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Cast Mesmerized Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		return base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
	}

	public void AfterCastMesmerizedSuccess(ActualGoapNode p_node)
	{
		if (p_node.target is Character character)
		{
			character.traitContainer.AddTrait(character, "Mesmerized");
		}
	}
}
