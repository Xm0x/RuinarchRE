using Inner_Maps;
using Inner_Maps.Location_Structures;

public class BuildLair : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public BuildLair()
		: base(INTERACTION_TYPE.BUILD_LAIR)
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

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			if (otherData != null && otherData.Length == 1 && otherData[0].obj is LocationGridTile locationGridTile)
			{
				return !locationGridTile.hasBlueprint;
			}
			return true;
		}
		return false;
	}

	public void AfterBuildSuccess(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		Character actor = goapNode.actor;
		FactionManager.Instance.RerollFactionRelationships(FactionManager.Instance.undeadFaction, actor, isRerollForNewFaction: false, logRelationshipChangeFromLeaderRelationship: false);
		Area area = (otherData[0].obj as LocationGridTile).area;
		NPCSettlement nPCSettlement = LandmarkManager.Instance.CreateNewSettlement(area.region, LOCATION_TYPE.DUNGEON, area);
		LandmarkManager.Instance.PlaceBuiltStructureForSettlement(FACTION_TYPE.None, nPCSettlement, area.region.innerMap, area, STRUCTURE_TYPE.NECROMANCER_LAIR, RESOURCE.NONE);
		LocationStructure firstStructureOfType = nPCSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.NECROMANCER_LAIR);
		goapNode.actor.necromancerTrait.SetLairStructure(firstStructureOfType);
		goapNode.actor.MigrateHomeStructureTo(firstStructureOfType);
	}
}
