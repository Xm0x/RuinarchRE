using System.Collections.Generic;
using Inner_Maps;

public class BuildTrollCauldron : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public BuildTrollCauldron()
		: base(INTERACTION_TYPE.BUILD_TROLL_CAULDRON)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
		base.actionIconString = GoapActionStateDB.Build_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Build Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void PopulateNearbyLocation(List<LocationGridTile> gridTiles, ActualGoapNode goapNode)
	{
		goapNode.actor.gridTileLocation.PopulateTilesInRadius(gridTiles, 3, 0, includeCenterTile: false, includeTilesInDifferentStructure: false, includeImpassable: false, includeTilesWithObject: false);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor == poiTarget;
		}
		return false;
	}

	public void AfterBuildSuccess(ActualGoapNode goapNode)
	{
		TrollCauldron poi = InnerMapManager.Instance.CreateNewTileObject<TrollCauldron>(TILE_OBJECT_TYPE.TROLL_CAULDRON);
		goapNode.actor.gridTileLocation.structure.AddPOI(poi, goapNode.actor.gridTileLocation);
	}
}
