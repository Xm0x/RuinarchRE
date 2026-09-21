using JetBrains.Annotations;

public class StartCleanse : GoapAction
{
	public StartCleanse()
		: base(INTERACTION_TYPE.START_CLEANSE)
	{
		base.actionIconString = GoapActionStateDB.Divine_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Start Cleanse Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	[UsedImplicitly]
	public void PreStartCleanseSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.associatedJob.originalOwner is NPCSettlement nPCSettlement)
		{
			goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Cleansing");
			nPCSettlement.settlementJobTriggerComponent.OnTakeCleanseTileJob(goapNode.actor);
		}
	}
}
