using Inner_Maps;
using UtilityScripts;

public class Scrap : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public Scrap()
		: base(INTERACTION_TYPE.SCRAP)
	{
		validTimeOfDays = new TIME_IN_WORDS[4]
		{
			TIME_IN_WORDS.MORNING,
			TIME_IN_WORDS.LUNCH_TIME,
			TIME_IN_WORDS.AFTERNOON,
			TIME_IN_WORDS.EARLY_NIGHT
		};
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Scrap Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return Utilities.Rng.Next(15, 31);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && poiTarget is TileObject)
		{
			TileObject tileObject = poiTarget as TileObject;
			if (tileObject.tileObjectType != TILE_OBJECT_TYPE.HEALING_POTION && tileObject.tileObjectType != TILE_OBJECT_TYPE.TOOL)
			{
				return false;
			}
			if (tileObject.characterOwner != null && !tileObject.IsOwnedBy(actor))
			{
				return false;
			}
			if (tileObject.gridTileLocation == null)
			{
				return false;
			}
			if (tileObject.gridTileLocation.structure.region.IsRequiredByLocation(tileObject))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void AfterScrapSuccess(ActualGoapNode goapNode)
	{
		TileObject tileObject = goapNode.poiTarget as TileObject;
		LocationGridTile gridTileLocation = tileObject.gridTileLocation;
		goapNode.actor.DestroyItem(tileObject);
		StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
		stonePile.SetResourceInPile(10);
		gridTileLocation.structure.AddPOI(stonePile, gridTileLocation);
	}
}
