using UtilityScripts;

public class Tease : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public Tease()
		: base(INTERACTION_TYPE.TEASE)
	{
		base.actionIconString = GoapActionStateDB.Mock_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Tease Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest poiTarget, JobQueueItem job, OtherData[] otherData)
	{
		Character character = poiTarget as Character;
		if (actor.relationshipContainer.IsFriendsWith(character))
		{
			return Utilities.Rng.Next(40, 61);
		}
		return Utilities.Rng.Next(50, 71);
	}

	public void PerTickTeaseSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(5f);
	}
}
