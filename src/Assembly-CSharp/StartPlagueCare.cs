using Traits;

public class StartPlagueCare : GoapAction
{
	public StartPlagueCare()
		: base(INTERACTION_TYPE.START_PLAGUE_CARE)
	{
		base.actionIconString = GoapActionStateDB.Cure_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Care Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterCareSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Plague Caring", goapNode.actor);
		goapNode.actor.traitContainer.GetTraitOrStatus<Trait>("Plague Caring")?.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
	}
}
