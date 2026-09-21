using System;
using Locations.Settlements;

public class RecruitVampiresPartyQuest : PartyQuest
{
	public BaseSettlement targetSettlement { get; private set; }

	public GameDate expiryDate { get; private set; }

	public bool isRecruiting { get; private set; }

	public override IPartyQuestTarget target => targetSettlement;

	public override Type serializedData => typeof(SaveDataRecruitVampiresPartyQuest);

	public RecruitVampiresPartyQuest()
		: base(PARTY_QUEST_TYPE.Recruit_Vampires)
	{
		base.minimumPartySize = 2;
		base.priority = 3;
		base.relatedBehaviour = typeof(RecruitVampiresBehaviour);
	}

	public RecruitVampiresPartyQuest(SaveDataRecruitVampiresPartyQuest data)
		: base(data)
	{
		isRecruiting = data.isHunting;
		expiryDate = data.expiryDate;
	}

	public void SetTargetSettlement(BaseSettlement p_settlement)
	{
		if (targetSettlement != p_settlement)
		{
			targetSettlement = p_settlement;
		}
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		return targetSettlement;
	}

	public override bool IsInterestedInJoiningQuest(Character p_character)
	{
		if (p_character.traitContainer.HasTrait("Vampire"))
		{
			return !p_character.crimeComponent.IsWantedBy(targetSettlement.owner);
		}
		return false;
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName + " " + targetSettlement.name;
	}

	public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState)
	{
		base.OnAssignedPartySwitchedState(fromState, toState);
		if (toState == PARTY_STATE.Working)
		{
			StartTimer();
		}
	}

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		return targetSettlement != null;
	}

	private void StartTimer()
	{
		if (!isRecruiting)
		{
			isRecruiting = true;
			expiryDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(4));
			SchedulingManager.Instance.AddEntry(expiryDate, DoneTimer, this);
		}
	}

	private void DoneTimer()
	{
		if (isRecruiting)
		{
			isRecruiting = false;
			if (base.assignedParty != null && base.assignedParty.isActive && base.assignedParty.currentQuest == this)
			{
				SetIsSuccessful(state: true);
				EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
			}
		}
	}

	public override void LoadReferences(SaveDataPartyQuest data)
	{
		base.LoadReferences(data);
		if (data is SaveDataRecruitVampiresPartyQuest saveDataRecruitVampiresPartyQuest && !string.IsNullOrEmpty(saveDataRecruitVampiresPartyQuest.targetSettlement))
		{
			targetSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataRecruitVampiresPartyQuest.targetSettlement);
		}
	}

	public override void LoadReferencesInMainThread(SaveDataPartyQuest data)
	{
		base.LoadReferencesInMainThread(data);
		if (isRecruiting)
		{
			SchedulingManager.Instance.AddEntry(expiryDate, DoneTimer, this);
		}
	}
}
