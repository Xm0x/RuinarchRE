using JetBrains.Annotations;

public class StartPurifyingGround : GoapAction
{
	public StartPurifyingGround()
		: base(INTERACTION_TYPE.START_PURIFYING_GROUND)
	{
		base.actionIconString = GoapActionStateDB.Divine_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Start Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	[UsedImplicitly]
	public void PreStartSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.associatedJob.originalOwner is NPCSettlement settlementTargetForPurifyingGround && !goapNode.actor.behaviourComponent.HasBehaviour(typeof(PurifyGroundBehaviour)))
		{
			goapNode.actor.behaviourComponent.AddBehaviourComponent(typeof(PurifyGroundBehaviour));
			goapNode.actor.behaviourComponent.SetSettlementTargetForPurifyingGround(settlementTargetForPurifyingGround);
		}
	}
}
