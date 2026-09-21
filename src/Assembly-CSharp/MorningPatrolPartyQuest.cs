using System;

public class MorningPatrolPartyQuest : PartyQuest
{
	public override IPartyQuestTarget target => base.madeInLocation;

	public override Type serializedData => typeof(SaveDataMorningPatrolPartyQuest);

	public override bool workingStateImmediately => true;

	public override bool canStillJoinQuestAnytime => true;

	public MorningPatrolPartyQuest()
		: base(PARTY_QUEST_TYPE.Morning_Patrol)
	{
		base.minimumPartySize = 1;
		base.priority = 1;
		base.relatedBehaviour = typeof(MorningPatrolBehaviour);
	}

	public MorningPatrolPartyQuest(SaveDataMorningPatrolPartyQuest data)
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
