using System;
using Inner_Maps.Location_Structures;

public class ExterminationPartyQuest : PartyQuest
{
	public LocationStructure targetStructure { get; private set; }

	public override IPartyQuestTarget target => targetStructure;

	public override Type serializedData => typeof(SaveDataExterminationPartyQuest);

	public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;

	public ExterminationPartyQuest()
		: base(PARTY_QUEST_TYPE.Extermination)
	{
		base.minimumPartySize = 1;
		base.priority = 3;
		base.relatedBehaviour = typeof(ExterminateBehaviour);
	}

	public ExterminationPartyQuest(SaveDataExterminationPartyQuest data)
		: base(data)
	{
	}

	public override void OnWaitTimeOver()
	{
		base.OnWaitTimeOver();
		if (targetStructure == null || targetStructure.hasBeenDestroyed || targetStructure.tiles.Count <= 0)
		{
			EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Destroyed"));
		}
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		return targetStructure;
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName + " " + targetStructure?.name;
	}

	protected override void AfterEndQuest()
	{
		base.AfterEndQuest();
		SetTargetStructure(null);
	}

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		if (targetStructure != null)
		{
			return targetStructure.HasAliveResident(null);
		}
		return false;
	}

	private void ProcessExterminationOrDisbandment()
	{
		if (base.assignedParty != null && base.assignedParty.isActive && base.assignedParty.currentQuest == this)
		{
			Faction owner = base.assignedParty.partySettlement.owner;
			if (targetStructure == null || targetStructure.hasBeenDestroyed || targetStructure.tiles.Count <= 0 || !targetStructure.settlementLocation.HasResidentForExterminationPartyQuest(targetStructure.settlementLocation, owner, base.assignedParty))
			{
				base.assignedParty.GoBackHomeAndEndQuest();
			}
			else
			{
				StartExterminationTimer();
			}
		}
	}

	public void SetTargetStructure(LocationStructure structure)
	{
		if (targetStructure != structure)
		{
			targetStructure = structure;
		}
	}

	private void StartExterminationTimer()
	{
	}

	private void DoneExterminationTimer()
	{
	}

	public override void LoadReferences(SaveDataPartyQuest data)
	{
		base.LoadReferences(data);
		if (data is SaveDataExterminationPartyQuest saveDataExterminationPartyQuest && !string.IsNullOrEmpty(saveDataExterminationPartyQuest.targetStructure))
		{
			targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataExterminationPartyQuest.targetStructure);
		}
	}
}
