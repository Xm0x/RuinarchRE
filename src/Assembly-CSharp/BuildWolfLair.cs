using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class BuildWolfLair : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public BuildWolfLair()
		: base(INTERACTION_TYPE.BUILD_WOLF_LAIR)
	{
		base.actionIconString = GoapActionStateDB.Build_Icon;
		base.logTags = new LOG_TAG[1];
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

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		OtherData[] otherData = node.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0].obj is LocationGridTile)
		{
			return (otherData[0].obj as LocationGridTile).structure;
		}
		return base.GetTargetStructure(node);
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0].obj is LocationGridTile)
		{
			return otherData[0].obj as LocationGridTile;
		}
		return null;
	}

	public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode)
	{
		return null;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid)
		{
			OtherData[] otherData = node.otherData;
			if (otherData != null && otherData.Length == 1 && otherData[0].obj is LocationGridTile locationGridTile && node.actor.gridTileLocation != locationGridTile && !node.actor.gridTileLocation.IsNeighbour(locationGridTile))
			{
				goapActionInvalidity.isInvalid = true;
			}
		}
		return goapActionInvalidity;
	}

	public void AfterBuildSuccess(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		_ = goapNode.actor;
		Area area = (otherData[0].obj as LocationGridTile).area;
		NPCSettlement nPCSettlement = LandmarkManager.Instance.CreateNewSettlement(area.region, LOCATION_TYPE.DUNGEON, area);
		LocationStructure locationStructure = LandmarkManager.Instance.CreateNewStructureAt(area.region, STRUCTURE_TYPE.MONSTER_LAIR);
		nPCSettlement.GenerateStructures(locationStructure);
		List<LocationGridTile> gridTiles = area.gridTileComponent.gridTiles;
		LocationStructure wilderness = area.region.wilderness;
		InnerMapManager.Instance.MonsterLairCellAutomata(gridTiles, locationStructure, area.region, wilderness);
		locationStructure.SetOccupiedArea(area);
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		locationStructure.PopulateTileObjectsOfType(list, TILE_OBJECT_TYPE.BLOCK_WALL);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].baseMapObjectVisual.ApplyGraphUpdate();
		}
		RuinarchListPool<TileObject>.Release(list);
		area.areaItem.UpdatePathfindingGraph();
		goapNode.actor.MigrateHomeStructureTo(locationStructure);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (otherData != null && otherData.Length == 1 && otherData[0].obj is LocationGridTile { hasBlueprint: not false })
			{
				return false;
			}
			return poiTarget == actor;
		}
		return false;
	}
}
