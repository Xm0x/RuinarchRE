public class JoinGathering : GoapAction
{
	public JoinGathering()
		: base(INTERACTION_TYPE.JOIN_GATHERING)
	{
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Party };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Join Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		if (node.poiTarget is Character character)
		{
			if (node.otherData != null && node.otherData.Length == 1)
			{
				log.AddToFillers(null, (string)node.otherData[0].obj, LOG_IDENTIFIER.STRING_1);
			}
			else
			{
				log.AddToFillers(null, character.gatheringComponent.currentGathering.gatheringName, LOG_IDENTIFIER.STRING_1);
			}
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is Character character)
			{
				if (!character.isDead)
				{
					return character.gatheringComponent.hasGathering;
				}
				return false;
			}
			return actor != poiTarget;
		}
		return false;
	}

	public void AfterJoinSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character character)
		{
			character.gatheringComponent.currentGathering.AddAttendee(goapNode.actor);
		}
	}
}
