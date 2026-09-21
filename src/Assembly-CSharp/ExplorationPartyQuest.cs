using System;
using Inner_Maps.Location_Structures;

public class ExplorationPartyQuest : PartyQuest
{
	public LocationStructure targetStructure { get; private set; }

	public bool isExploring { get; private set; }

	public GameDate expiryDate { get; private set; }

	public override IPartyQuestTarget target => targetStructure;

	public override Type serializedData => typeof(SaveDataExplorationPartyQuest);

	public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;

	public ExplorationPartyQuest()
		: base(PARTY_QUEST_TYPE.Exploration)
	{
		base.minimumPartySize = 3;
		base.priority = 2;
		base.relatedBehaviour = typeof(ExploreBehaviour);
	}

	public ExplorationPartyQuest(SaveDataExplorationPartyQuest data)
		: base(data)
	{
		isExploring = data.isExploring;
		expiryDate = data.expiryDate;
	}

	public override void OnWaitTimeOver()
	{
		base.OnWaitTimeOver();
		Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		return targetStructure;
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName;
	}

	protected override void OnEndQuest()
	{
		base.OnEndQuest();
		Messenger.RemoveListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
	}

	protected override void AfterEndQuest()
	{
		base.AfterEndQuest();
		SetTargetStructure(null);
	}

	public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState)
	{
		base.OnAssignedPartySwitchedState(fromState, toState);
		if (toState == PARTY_STATE.Working)
		{
			StartExplorationTimer();
		}
	}

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		return targetStructure != null;
	}

	private void ProcessExplorationOrDisbandment()
	{
		if (base.assignedParty != null && base.assignedParty.isActive && base.assignedParty.currentQuest == this)
		{
			SetIsSuccessful(state: true);
			EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
		}
	}

	public void SetTargetStructure(LocationStructure structure)
	{
		if (targetStructure != structure)
		{
			targetStructure = structure;
		}
	}

	private void StartExplorationTimer()
	{
		if (!isExploring)
		{
			isExploring = true;
			expiryDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
			SchedulingManager.Instance.AddEntry(expiryDate, DoneExplorationTimer, this);
		}
	}

	private void DoneExplorationTimer()
	{
		if (isExploring)
		{
			isExploring = false;
			ProcessExplorationOrDisbandment();
		}
	}

	private void OnStructureDestroyed(LocationStructure structure)
	{
		if (targetStructure == structure)
		{
			EndQuest(PartyQuest.GetLocalizedEndQuestReason("Structure_Destroyed"));
		}
	}

	public override void LoadReferences(SaveDataPartyQuest data)
	{
		base.LoadReferences(data);
		if (data is SaveDataExplorationPartyQuest saveDataExplorationPartyQuest && !string.IsNullOrEmpty(saveDataExplorationPartyQuest.targetStructure))
		{
			targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(saveDataExplorationPartyQuest.targetStructure);
		}
	}

	public override void LoadReferencesInMainThread(SaveDataPartyQuest data)
	{
		base.LoadReferencesInMainThread(data);
		if (base.isWaitTimeOver && base.assignedParty != null)
		{
			Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
		}
		if (isExploring)
		{
			SchedulingManager.Instance.AddEntry(expiryDate, DoneExplorationTimer, this);
		}
	}
}
