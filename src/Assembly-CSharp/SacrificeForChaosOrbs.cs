using UtilityScripts;

public class SacrificeForChaosOrbs : GoapAction
{
	public SacrificeForChaosOrbs()
		: base(INTERACTION_TYPE.SACRIFICE_FOR_CHAOS_ORBS)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Cult_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Player };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Sacrifice Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterSacrificeSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.poiTarget as Character;
		int p_amount = GameUtilities.RandomBetweenTwoNumbers(2, 5);
		if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.BRAINWASH).TryDecreaseRemainingChaosOrbs(ref p_amount))
		{
			Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.gridTileLocation.centeredWorldLocation, p_amount, character.gridTileLocation.parentMap);
		}
		character.SetDestroyMarkerOnDeath(state: true);
		character.Death("normal", goapNode, null, goapNode.descriptionLog);
	}
}
