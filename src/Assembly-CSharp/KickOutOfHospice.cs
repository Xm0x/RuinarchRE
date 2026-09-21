using Inner_Maps.Location_Structures;

public class KickOutOfHospice : GoapAction
{
	public KickOutOfHospice()
		: base(INTERACTION_TYPE.KICK_OUT_OF_HOSPICE)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Anger_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Kick Out Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterKickOutSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is Character character)
		{
			if (character.currentStructure is Hospice hospice)
			{
				hospice.BanCharacter(character);
			}
			character.StopCurrentActionNode("Disliked_By_Hospice_Worker");
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is Character)
			{
				return poiTarget.traitContainer.HasTrait("Recuperating");
			}
			return false;
		}
		return false;
	}
}
