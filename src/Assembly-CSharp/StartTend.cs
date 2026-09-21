using Traits;

public class StartTend : GoapAction
{
	public StartTend()
		: base(INTERACTION_TYPE.START_TEND)
	{
		base.actionIconString = GoapActionStateDB.Harvest_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Start Tend Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterStartTendSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Tending", goapNode.actor);
		goapNode.actor.traitContainer.GetTraitOrStatus<Trait>("Tending")?.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
	}
}
