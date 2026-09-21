using Inner_Maps;
using Inner_Maps.Location_Structures;

public class BuildBanditCamp : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public BuildBanditCamp()
		: base(INTERACTION_TYPE.BUILD_BANDIT_CAMP)
	{
		base.actionIconString = GoapActionStateDB.Build_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Major };
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
			if (actor.faction == null || actor.faction.factionType.type != FACTION_TYPE.Bandits)
			{
				return false;
			}
			if (otherData != null && otherData.Length == 1 && otherData[0].obj is LocationGridTile locationGridTile)
			{
				return !locationGridTile.hasBlueprint;
			}
			return true;
		}
		return false;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid)
		{
			LocationGridTile locationGridTile = node.otherData[0].obj as LocationGridTile;
			if (node.actor.faction.HasMemberWithJob(JOB_TYPE.BUILD_BANDIT_CAMP, node.actor))
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "already_building_bandit_camp";
			}
			else if (!locationGridTile.IsAreaSuitableForBanditCamp(node.actor))
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "no_space_structure";
			}
		}
		return goapActionInvalidity;
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		LocationGridTile locationGridTile = node.otherData[0].obj as LocationGridTile;
		if (locationGridTile.tileObjectComponent.genericTileObject.blueprintOnTile != null)
		{
			locationGridTile.tileObjectComponent.genericTileObject.ForceExpireBlueprint();
		}
	}

	public override void OnStoppedInterrupt(ActualGoapNode node)
	{
		base.OnStoppedInterrupt(node);
		LocationGridTile locationGridTile = node.otherData[0].obj as LocationGridTile;
		if (locationGridTile.tileObjectComponent.genericTileObject.blueprintOnTile != null)
		{
			locationGridTile.tileObjectComponent.genericTileObject.ForceExpireBlueprint();
		}
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		LocationGridTile locationGridTile = node.otherData[0].obj as LocationGridTile;
		if (locationGridTile.tileObjectComponent.genericTileObject.blueprintOnTile != null)
		{
			locationGridTile.tileObjectComponent.genericTileObject.ForceExpireBlueprint();
		}
	}

	public void PreBuildSuccess(ActualGoapNode goapNode)
	{
		(goapNode.otherData[0].obj as LocationGridTile).tileObjectComponent.genericTileObject.PlaceExpiringBlueprintOnTile("Bandit Camp 1");
	}

	public void AfterBuildSuccess(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		Character actor = goapNode.actor;
		LocationGridTile locationGridTile = otherData[0].obj as LocationGridTile;
		if (locationGridTile.tileObjectComponent.genericTileObject.blueprintOnTile != null)
		{
			Area area = locationGridTile.area;
			NPCSettlement nPCSettlement = LandmarkManager.Instance.CreateNewSettlement(area.region, LOCATION_TYPE.PSEUDO_VILLAGE, area);
			LandmarkManager.Instance.OwnSettlement(FactionManager.Instance.banditFaction, nPCSettlement);
			locationGridTile.tileObjectComponent.genericTileObject.BuildBlueprintOnTile(nPCSettlement, locationGridTile);
			if (actor.traitContainer.HasTrait("Demon Cultist"))
			{
				FactionManager.Instance.undeadFaction.SetRelationshipFor(PlayerManager.Instance.player.playerFaction, FACTION_RELATIONSHIP_STATUS.Friendly);
			}
			LocationStructure firstStructureOfType = nPCSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.BANDIT_CAMP);
			goapNode.actor.MigrateHomeStructureTo(firstStructureOfType);
		}
	}
}
