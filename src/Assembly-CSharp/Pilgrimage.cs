using Inner_Maps.Location_Structures;

public class Pilgrimage : GoapAction
{
	public Pilgrimage()
		: base(INTERACTION_TYPE.PILGRIMAGE)
	{
		base.actionIconString = GoapActionStateDB.Pray_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Pilgrimage Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job) && target is HallowedGround hallowedGround)
		{
			if (hallowedGround.gridTileLocation?.structure is Inner_Maps.Location_Structures.HallowedGround hallowedGround2)
			{
				return hallowedGround2.claimedByReligion == actor.religionComponent.religion;
			}
			return false;
		}
		return false;
	}

	public void AfterPilgrimageSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.religionComponent.religion == RELIGION.Demon_Worship)
		{
			int p_amount = 3;
			if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.BRAINWASH).TryDecreaseRemainingChaosOrbs(ref p_amount))
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, goapNode.actor.worldPosition, p_amount, goapNode.actor.gridTileLocation.parentMap);
			}
		}
		else
		{
			goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Blessed", null, bypassElementalChance: false, 480);
		}
	}
}
