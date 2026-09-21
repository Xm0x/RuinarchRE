using Inner_Maps;
using UtilityScripts;

public class SpawnPoisonCloud : GoapAction
{
	public SpawnPoisonCloud()
		: base(INTERACTION_TYPE.SPAWN_POISON_CLOUD)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Player };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Spawn Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor == poiTarget;
		}
		return false;
	}

	public void AfterSpawnSuccess(ActualGoapNode goapNode)
	{
		int p_min = 3;
		int p_max = 8;
		if (goapNode.otherData != null && goapNode.otherData.Length > 1 && goapNode.otherData[0] is IntOtherData intOtherData && goapNode.otherData[1] is IntOtherData intOtherData2)
		{
			p_min = intOtherData.integer;
			p_max = intOtherData2.integer;
		}
		InnerMapManager.Instance.SpawnPoisonCloud(goapNode.actor.gridTileLocation, GameUtilities.RandomBetweenTwoNumbers(p_min, p_max), GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(GameUtilities.RandomBetweenTwoNumbers(2, 5))));
	}
}
