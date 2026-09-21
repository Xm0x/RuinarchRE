using Inner_Maps;

public class PackFood : GoapAction
{
	public const int NeededFood = 10;

	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public PackFood()
		: base(INTERACTION_TYPE.PACK_FOOD)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Pack Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is ResourcePile { gridTileLocation: not null, resourceInPile: >=10 } resourcePile)
			{
				if (resourcePile.characterOwner != actor)
				{
					return resourcePile.gridTileLocation.structure == actor.homeStructure;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public void AfterPackSuccess(ActualGoapNode goapNode)
	{
		TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.LUNCH_PACK);
		goapNode.actor.PickUpItem(item, changeCharacterOwnership: true);
		if (goapNode.poiTarget is ResourcePile resourcePile)
		{
			resourcePile.AdjustResourceInPile(-10);
		}
	}
}
