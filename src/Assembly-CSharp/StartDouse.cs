using JetBrains.Annotations;

public class StartDouse : GoapAction
{
	public StartDouse()
		: base(INTERACTION_TYPE.START_DOUSE)
	{
		base.actionIconString = GoapActionStateDB.Douse_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Start Douse Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	[UsedImplicitly]
	public void PreStartDouseSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.associatedJob.originalOwner is NPCSettlement nPCSettlement)
		{
			goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Dousing");
			nPCSettlement.settlementJobTriggerComponent.OnTakeDouseFireJob(goapNode.actor);
		}
	}
}
