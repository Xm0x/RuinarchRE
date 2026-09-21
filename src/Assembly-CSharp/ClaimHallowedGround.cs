using Inner_Maps.Location_Structures;

public class ClaimHallowedGround : GoapAction
{
	public ClaimHallowedGround()
		: base(INTERACTION_TYPE.CLAIM_HALLOWED_GROUND)
	{
		base.actionIconString = GoapActionStateDB.Found_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Major };
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
		log.AddToFillers(null, node.actor.religionComponent.religion.LocalizedName(), LOG_IDENTIFIER.STRING_1);
		log.AddInvolvedObjectManual(node.poiTarget.persistentID);
	}

	public void AfterClaimSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		if (goapNode.poiTarget is HallowedGround && goapNode.poiTarget.gridTileLocation?.structure is Inner_Maps.Location_Structures.HallowedGround hallowedGround)
		{
			hallowedGround.ClaimHallowedGround(goapNode.actor.religionComponent.religion);
			if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.currentParty.currentQuest is ClaimHallowedGroundPartyQuest claimHallowedGroundPartyQuest && claimHallowedGroundPartyQuest.targetStructure == hallowedGround)
			{
				actor.partyComponent.currentParty.currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
			}
		}
	}
}
