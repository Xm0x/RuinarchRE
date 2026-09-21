using System;

public class NightPatrolPartyQuest : PartyQuest
{
	public override IPartyQuestTarget target => base.madeInLocation;

	public override Type serializedData => typeof(SaveDataNightPatrolPartyQuest);

	public override bool workingStateImmediately => true;

	public override bool canStillJoinQuestAnytime => true;

	public NightPatrolPartyQuest()
		: base(PARTY_QUEST_TYPE.Night_Patrol)
	{
		base.minimumPartySize = 1;
		base.priority = 1;
		base.relatedBehaviour = typeof(NightPatrolBehaviour);
	}

	public NightPatrolPartyQuest(SaveDataNightPatrolPartyQuest data)
		: base(data)
	{
	}

	public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState)
	{
		base.OnAssignedPartySwitchedState(fromState, toState);
		if (toState == PARTY_STATE.Working)
		{
			SetIsSuccessful(state: true);
		}
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		return base.madeInLocation;
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName;
	}

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		if (base.madeInLocation != null)
		{
			return DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(base.madeInLocation.persistentID) != null;
		}
		return false;
	}
}
