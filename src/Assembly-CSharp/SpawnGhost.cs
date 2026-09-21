public class SpawnGhost : GoapAction
{
	public SpawnGhost()
		: base(INTERACTION_TYPE.SPAWN_GHOST)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Spawn Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor.gridTileLocation != null)
			{
				return actor == poiTarget;
			}
			return false;
		}
		return false;
	}

	public void AfterSpawnSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		if (actor.gridTileLocation != null && actor is Revenant revenant)
		{
			int amount = 1;
			if (goapNode.otherData != null && goapNode.otherData.Length != 0 && goapNode.otherData[0] is IntOtherData intOtherData)
			{
				amount = intOtherData.integer;
			}
			revenant.SpawnGhosts(amount);
		}
	}
}
