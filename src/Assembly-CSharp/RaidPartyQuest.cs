using System;
using Locations.Settlements;

public class RaidPartyQuest : PartyQuest
{
	public BaseSettlement targetSettlement { get; private set; }

	public bool isRaiding { get; private set; }

	public GameDate expiryDate { get; private set; }

	public override IPartyQuestTarget target => targetSettlement;

	public override Type serializedData => typeof(SaveDataRaidPartyQuest);

	public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;

	public RaidPartyQuest()
		: base(PARTY_QUEST_TYPE.Raid)
	{
		base.minimumPartySize = 3;
		base.priority = 4;
		base.relatedBehaviour = typeof(RaidBehaviour);
	}

	public RaidPartyQuest(SaveDataRaidPartyQuest data)
		: base(data)
	{
		isRaiding = data.isRaiding;
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		return targetSettlement;
	}

	public override void OnAcceptQuest(Party partyThatAcceptedQuest)
	{
		base.OnAcceptQuest(partyThatAcceptedQuest);
		Messenger.AddListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, DisconnectFromSettlement);
	}

	protected override void OnEndQuest()
	{
		base.OnEndQuest();
		Messenger.RemoveListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, DisconnectFromSettlement);
	}

	public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState)
	{
		base.OnAssignedPartySwitchedState(fromState, toState);
		if (toState == PARTY_STATE.Working)
		{
			StartRaidTimer();
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
		}
		else if (fromState == PARTY_STATE.Working)
		{
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
		}
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName + " " + targetSettlement.name;
	}

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		if (targetSettlement == null || targetSettlement.owner == null || targetSettlement.owner.IsFriendlyWith(p_faction))
		{
			return false;
		}
		return true;
	}

	private void OnCharacterCanNoLongerMove(Character character)
	{
		if (base.assignedParty != null && base.assignedParty.isActive && base.assignedParty.currentQuest == this)
		{
			if (character.homeSettlement != targetSettlement || !character.marker)
			{
				return;
			}
			for (int i = 0; i < character.marker.inVisionCharacters.Count; i++)
			{
				Character character2 = character.marker.inVisionCharacters[i];
				if ((bool)character2.marker && character2.partyComponent.isActiveMember && character2.partyComponent.currentParty.currentQuest == this)
				{
					character2.marker.AddPOIAsInVisionRange(character);
				}
			}
		}
		else
		{
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
		}
	}

	private void ProcessRaidOrDisbandment()
	{
		if (base.assignedParty != null && base.assignedParty.isActive && base.assignedParty.currentQuest == this)
		{
			base.assignedParty.GoBackHomeAndEndQuest();
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
			ProcessRaidOrDisbandment();
		}
	}

	public override void LoadReferences(SaveDataPartyQuest data)
	{
		base.LoadReferences(data);
		if (data is SaveDataRaidPartyQuest saveDataRaidPartyQuest && !string.IsNullOrEmpty(saveDataRaidPartyQuest.targetSettlement))
		{
			targetSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataRaidPartyQuest.targetSettlement);
		}
	}

	public override void LoadReferencesInMainThread(SaveDataPartyQuest data)
	{
		base.LoadReferencesInMainThread(data);
		if (base.assignedParty != null && base.assignedParty.isActive && base.assignedParty.partyState == PARTY_STATE.Working)
		{
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
		}
		if (isRaiding)
		{
			SchedulingManager.Instance.AddEntry(expiryDate, DoneRaidTimer, this);
		}
	}

	private void DisconnectFromSettlement(NPCSettlement p_settlement)
	{
		if (p_settlement == targetSettlement)
		{
			EndQuest(PartyQuest.GetLocalizedEndQuestReason("Specific_Settlement_Destroyed", p_settlement));
		}
	}
}
