using System;
using Locations.Settlements;

public class DemonRaidPartyQuest : PartyQuest
{
	public BaseSettlement targetSettlement { get; private set; }

	public bool isRaiding { get; private set; }

	public GameDate expiryDate { get; private set; }

	public DEMON_RAID_TYPE raidType { get; private set; }

	public override IPartyQuestTarget target => targetSettlement;

	public override Type serializedData => typeof(SaveDataDemonRaidPartyQuest);

	public DemonRaidPartyQuest()
		: base(PARTY_QUEST_TYPE.Demon_Raid)
	{
		base.minimumPartySize = 3;
		base.priority = 5;
		base.relatedBehaviour = typeof(DemonRaidBehaviour);
	}

	public DemonRaidPartyQuest(SaveDataDemonRaidPartyQuest data)
		: base(data)
	{
		isRaiding = data.isRaiding;
		expiryDate = data.expiryDate;
		raidType = data.raidType;
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		return targetSettlement;
	}

	public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState)
	{
		base.OnAssignedPartySwitchedState(fromState, toState);
		if (toState == PARTY_STATE.Working)
		{
			StartRaidTimer();
		}
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName + " " + targetSettlement.name;
	}

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		return targetSettlement != null;
	}

	private void ProcessDisbandment()
	{
		if (base.assignedParty != null && base.assignedParty.isActive && base.assignedParty.currentQuest == this)
		{
			EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
		}
	}

	public void SetTargetSettlement(BaseSettlement settlement)
	{
		if (targetSettlement != settlement)
		{
			targetSettlement = settlement;
		}
	}

	private bool HasAliveResidentInsideSettlementThatIsHostileWith(Faction faction, BaseSettlement settlement)
	{
		for (int i = 0; i < settlement.residents.Count; i++)
		{
			Character character = settlement.residents[i];
			if (!character.isDead && !character.isBeingSeized && character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(settlement) && (character.faction == null || faction == null || faction.IsHostileWith(character.faction)))
			{
				return true;
			}
		}
		return false;
	}

	public void SetRaidType(DEMON_RAID_TYPE p_raidType)
	{
		raidType = p_raidType;
	}

	private void StartRaidTimer()
	{
		if (!isRaiding)
		{
			isRaiding = true;
			expiryDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(5));
			SchedulingManager.Instance.AddEntry(expiryDate, DoneRaidTimer, this);
		}
	}

	private void DoneRaidTimer()
	{
		if (isRaiding)
		{
			isRaiding = false;
			ProcessDisbandment();
		}
	}

	public override void LoadReferences(SaveDataPartyQuest data)
	{
		base.LoadReferences(data);
		if (data is SaveDataDemonRaidPartyQuest saveDataDemonRaidPartyQuest && !string.IsNullOrEmpty(saveDataDemonRaidPartyQuest.targetSettlement))
		{
			targetSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataDemonRaidPartyQuest.targetSettlement);
		}
	}

	public override void LoadReferencesInMainThread(SaveDataPartyQuest data)
	{
		base.LoadReferencesInMainThread(data);
		if (isRaiding)
		{
			SchedulingManager.Instance.AddEntry(expiryDate, DoneRaidTimer, this);
		}
	}
}
