public class Disguise : GoapAction
{
	public Disguise()
		: base(INTERACTION_TYPE.DISGUISE)
	{
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Disguise Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is Character character)
			{
				if (!character.isDead)
				{
					return actor != poiTarget;
				}
				return false;
			}
			return actor != poiTarget;
		}
		return false;
	}

	public void AfterDisguiseSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Stealthy");
		goapNode.actor.reactionComponent.SetDisguisedCharacter(goapNode.poiTarget as Character);
	}
}
