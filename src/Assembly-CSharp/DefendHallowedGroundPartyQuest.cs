using System;
using Inner_Maps.Location_Structures;

public class DefendHallowedGroundPartyQuest : PartyQuest
{
	public LocationStructure targetStructure { get; private set; }

	public GameDate expiryDate { get; private set; }

	public bool isDefending { get; private set; }

	public override IPartyQuestTarget target => targetStructure;

	public override Type serializedData => typeof(SaveDataDefendHallowedGroundPartyQuest);

	public DefendHallowedGroundPartyQuest()
		: base(PARTY_QUEST_TYPE.Defend_Hallowed_Ground)
	{
		base.minimumPartySize = 3;
		base.priority = 5;
		base.isFactionWideQuest = true;
		base.relatedBehaviour = typeof(DefendHallowedGroundBehaviour);
	}

	public DefendHallowedGroundPartyQuest(SaveDataDefendHallowedGroundPartyQuest data)
		: base(data)
	{
		isDefending = data.isDefending;
		expiryDate = data.expiryDate;
	}

	public void SetTargetStructure(LocationStructure p_structure)
	{
		if (targetStructure != p_structure)
		{
			targetStructure = p_structure;
		}
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		return targetStructure;
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName + " " + targetStructure.name;
	}

	public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState)
	{
		base.OnAssignedPartySwitchedState(fromState, toState);
		if (toState == PARTY_STATE.Working)
		{
			StartTimer();
		}
	}

	protected override bool IsConnectedToStructure(LocationStructure p_structure)
	{
		if (targetStructure == p_structure)
		{
			return true;
		}
		return base.IsConnectedToStructure(p_structure);
	}

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		return targetStructure != null;
	}

	private void StartTimer()
	{
		if (!isDefending)
		{
			isDefending = true;
			expiryDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(8));
			SchedulingManager.Instance.AddEntry(expiryDate, DoneTimer, this);
		}
	}

	private void DoneTimer()
	{
		if (isDefending)
		{
			isDefending = false;
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
		if (data is SaveDataDefendHallowedGroundPartyQuest saveDataDefendHallowedGroundPartyQuest && !string.IsNullOrEmpty(saveDataDefendHallowedGroundPartyQuest.targetStructure))
		{
			targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataDefendHallowedGroundPartyQuest.targetStructure);
		}
	}

	public override void LoadReferencesInMainThread(SaveDataPartyQuest data)
	{
		base.LoadReferencesInMainThread(data);
		if (isDefending)
		{
			SchedulingManager.Instance.AddEntry(expiryDate, DoneTimer, this);
		}
	}
}
