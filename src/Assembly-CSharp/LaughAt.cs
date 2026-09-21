using UtilityScripts;

public class LaughAt : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public LaughAt()
		: base(INTERACTION_TYPE.LAUGH_AT)
	{
		base.actionIconString = GoapActionStateDB.Mock_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.doesNotStopTargetCharacter = true;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Laugh Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return Utilities.Rng.Next(40, 61);
	}

	public void AfterLaughSuccess(ActualGoapNode goapNode)
	{
		if (!goapNode.poiTarget.traitContainer.HasTrait("Unconscious"))
		{
			goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Ashamed");
		}
	}
}
