public class WarmUp : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public WarmUp()
		: base(INTERACTION_TYPE.WARM_UP)
	{
		base.actionIconString = GoapActionStateDB.Happy_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Warm Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		int num = 20;
		if (target is TileObject { characterOwner: not null } tileObject && actor.relationshipContainer.IsEnemiesWith(tileObject.characterOwner))
		{
			num += 2000;
		}
		return num;
	}

	public void AfterWarmSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.RemoveStatusAndStacks(goapNode.actor, "Freezing");
		goapNode.actor.traitContainer.RemoveStatusAndStacks(goapNode.actor, "Frozen");
		if (goapNode.poiTarget is TileObject { characterOwner: null } tileObject)
		{
			tileObject.SetCharacterOwner(goapNode.actor);
		}
	}
}
