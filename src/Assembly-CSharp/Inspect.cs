public class Inspect : GoapAction
{
	public Inspect()
		: base(INTERACTION_TYPE.INSPECT)
	{
		base.actionIconString = GoapActionStateDB.Inspect_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Inspect Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.movementComponent.ShouldAvoidStructureLocationOfTarget(target) && !actor.partyComponent.hasParty)
		{
			return 2000;
		}
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.IsAvailable())
			{
				return poiTarget.gridTileLocation != null;
			}
			return false;
		}
		return false;
	}

	public void PreInspectSuccess(ActualGoapNode goapNode)
	{
		goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
	}

	public void AfterInspectSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is TileObject tileObject)
		{
			goapNode.actor.defaultCharacterTrait.AddAlreadyInspectedObject(tileObject);
			tileObject.OnInspect(goapNode.actor);
		}
	}
}
