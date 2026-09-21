public class ZombieDeath : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public ZombieDeath()
		: base(INTERACTION_TYPE.ZOMBIE_DEATH)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.actionIconString = GoapActionStateDB.Death_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Zombie Death Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			return actor == target;
		}
		return false;
	}

	public void AfterZombieDeathSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.Death("Zombie Virus", goapNode, null, goapNode.descriptionLog);
	}
}
