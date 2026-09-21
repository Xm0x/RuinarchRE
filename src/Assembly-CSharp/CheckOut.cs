public class CheckOut : GoapAction
{
	public CheckOut()
		: base(INTERACTION_TYPE.CHECK_OUT)
	{
		base.actionIconString = GoapActionStateDB.Inspect_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Check Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return poiTarget.gridTileLocation != null;
		}
		return false;
	}

	public void AfterCheckSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget.traitContainer.HasTrait("Mummified") && ChanceData.RollChance(CHANCE_TYPE.Release_Mummy_Character) && goapNode.poiTarget is Character character)
		{
			character.traitComponent.MummifiedReleaseIncantationByCharacter(goapNode.actor);
		}
	}
}
