using JetBrains.Annotations;

public class StartPatrol : GoapAction
{
	public StartPatrol()
		: base(INTERACTION_TYPE.START_PATROL)
	{
		base.actionIconString = GoapActionStateDB.Patrol_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Start Patrol Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	[UsedImplicitly]
	public void AfterStartPatrolSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Patrolling");
	}
}
