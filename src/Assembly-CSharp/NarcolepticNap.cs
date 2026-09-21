public class NarcolepticNap : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public NarcolepticNap()
		: base(INTERACTION_TYPE.NARCOLEPTIC_NAP)
	{
		base.actionIconString = GoapActionStateDB.Sleep_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Nap Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 1;
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		Character actor = node.actor;
		actor.traitContainer.RemoveTrait(actor, "Resting");
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor == poiTarget;
		}
		return false;
	}

	public void PreNapSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
	}

	public void PerTickNapSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.needsComponent.HasNeeds())
		{
			goapNode.actor.needsComponent.AdjustTiredness(1.1f);
		}
	}

	public void AfterNapSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
	}
}
