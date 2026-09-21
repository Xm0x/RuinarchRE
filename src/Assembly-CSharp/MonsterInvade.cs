using Inner_Maps.Location_Structures;

public class MonsterInvade : GoapAction
{
	public MonsterInvade()
		: base(INTERACTION_TYPE.MONSTER_INVADE)
	{
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Combat };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Invade Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return !actor.partyComponent.hasParty;
		}
		return false;
	}

	public void AfterInvadeSuccess(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData.Length == 1)
		{
			MonsterInvadeGathering monsterInvadeGathering = CharacterManager.Instance.CreateNewGathering(GATHERING_TYPE.Monster_Invade, goapNode.actor) as MonsterInvadeGathering;
			if (otherData[0].obj is LocationStructure targetStructure)
			{
				monsterInvadeGathering.SetTargetStructure(targetStructure);
			}
			else if (otherData[0].obj is Area targetArea)
			{
				monsterInvadeGathering.SetTargetArea(targetArea);
			}
		}
	}
}
