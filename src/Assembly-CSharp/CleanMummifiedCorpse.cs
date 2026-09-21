public class CleanMummifiedCorpse : GoapAction
{
	public CleanMummifiedCorpse()
		: base(INTERACTION_TYPE.CLEAN_MUMMIFIED_CORPSE)
	{
		base.actionIconString = GoapActionStateDB.Clean_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Clean Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && poiTarget is Character character)
		{
			if (actor != poiTarget && character.hasMarker)
			{
				return character.gridTileLocation != null;
			}
			return false;
		}
		return false;
	}

	public void AfterCleanSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(10f);
	}
}
