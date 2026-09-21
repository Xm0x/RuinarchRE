using System.Collections.Generic;
using Inner_Maps;

public class BuildCampfire : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public BuildCampfire()
		: base(INTERACTION_TYPE.BUILD_CAMPFIRE)
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
		Area area = goapNode.actor.gridTileLocation.area;
		if (area != null)
		{
			area.gridTileComponent.PopulateUnoccupiedTiles(gridTiles);
			if (gridTiles.Count <= 0)
			{
				gridTiles = area.gridTileComponent.gridTiles;
			}
		}
		else
		{
			goapNode.actor.gridTileLocation.PopulateTilesInRadius(gridTiles, 3, 0, includeCenterTile: false, includeTilesInDifferentStructure: false, includeImpassable: false, includeTilesWithObject: false);
		}
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
		Character actor = goapNode.actor;
		LocationGridTile locationGridTile = actor.gridTileLocation;
		if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
		{
			locationGridTile = locationGridTile.GetFirstNeighborThatIsPassableAndNoObjectAndSameAreaAs(locationGridTile.area);
		}
		if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
		{
			locationGridTile = locationGridTile.GetFirstNeighborThatIsPassableAndNoObject();
		}
		if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
		{
			locationGridTile.structure.RemovePOI(locationGridTile.tileObjectComponent.objHere);
		}
		Campfire campfire = InnerMapManager.Instance.CreateNewTileObject<Campfire>(TILE_OBJECT_TYPE.CAMPFIRE);
		locationGridTile.structure.AddPOI(campfire, locationGridTile);
		goapNode.descriptionLog.AddInvolvedObjectManual(campfire.persistentID);
		if (locationGridTile == null)
		{
			return;
		}
		LocationGridTile locationGridTile2 = locationGridTile.GetFirstNeighborThatIsPassableAndNoObjectAndSameAreaAs(locationGridTile.area);
		if (locationGridTile2 == null)
		{
			locationGridTile2 = locationGridTile.GetFirstNeighborThatIsPassableAndNoObject();
		}
		if (locationGridTile2 == null)
		{
			locationGridTile2 = locationGridTile.GetFirstNeighborThatIsPassable();
		}
		if (locationGridTile2 != null)
		{
			if (locationGridTile2.tileObjectComponent.objHere != null)
			{
				locationGridTile2.structure.RemovePOI(locationGridTile2.tileObjectComponent.objHere);
			}
			int resourceInPile = 12;
			if (actor.partyComponent.isMemberThatJoinedQuest)
			{
				resourceInPile = actor.partyComponent.currentParty.membersThatJoinedQuest.Count * 12;
			}
			FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.ANIMAL_MEAT);
			foodPile.SetResourceInPile(resourceInPile);
			locationGridTile2.structure.AddPOI(foodPile, locationGridTile2);
		}
	}
}
