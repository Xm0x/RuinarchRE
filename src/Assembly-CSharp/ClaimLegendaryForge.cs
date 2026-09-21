using Inner_Maps.Location_Structures;

public class ClaimLegendaryForge : GoapAction
{
	public ClaimLegendaryForge()
		: base(INTERACTION_TYPE.CLAIM_LEGENDARY_FORGE)
	{
		base.actionIconString = GoapActionStateDB.Found_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.shouldAddLogs = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Claim Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		Faction faction = node.actor.faction;
		if (faction != null)
		{
			log.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
		}
	}

	public void AfterClaimSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.faction != null && goapNode.poiTarget is LegendaryForgeTileObject && goapNode.poiTarget.gridTileLocation?.structure is LegendaryForge legendaryForge)
		{
			LandmarkManager.Instance.OwnSettlement(goapNode.actor.faction, legendaryForge.settlementLocation);
		}
	}
}
