using System;
using Inner_Maps.Location_Structures;

public class HuntBeastPartyQuest : PartyQuest
{
	public LocationStructure targetStructure { get; private set; }

	public override IPartyQuestTarget target => targetStructure;

	public override Type serializedData => typeof(SaveDataHuntBeastPartyQuest);

	public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;

	public HuntBeastPartyQuest()
		: base(PARTY_QUEST_TYPE.Hunt_Beast)
	{
		base.minimumPartySize = 1;
		base.priority = 2;
		base.relatedBehaviour = typeof(HuntBeastBehaviour);
	}

	public HuntBeastPartyQuest(SaveDataHuntBeastPartyQuest data)
		: base(data)
	{
	}

	public override void OnWaitTimeOver()
	{
		base.OnWaitTimeOver();
		if (targetStructure == null || targetStructure.hasBeenDestroyed || targetStructure.tiles.Count <= 0)
		{
			EndQuest(PartyQuest.GetLocalizedEndQuestReason("Structure_Destroyed"));
		}
		else
		{
			Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
		}
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		return targetStructure?.occupiedArea;
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName + " " + targetStructure.name;
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

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		return targetStructure != null;
	}

	public void SetTargetStructure(LocationStructure structure)
	{
		if (targetStructure != structure)
		{
			targetStructure = structure;
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
		if (data is SaveDataHuntBeastPartyQuest saveDataHuntBeastPartyQuest)
		{
			if (!string.IsNullOrEmpty(saveDataHuntBeastPartyQuest.targetStructure))
			{
				targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataHuntBeastPartyQuest.targetStructure);
			}
			if (base.isWaitTimeOver && base.assignedParty != null)
			{
				Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
			}
		}
	}

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		base.CheckIfStructureIsStillReferenced(p_structure);
		_ = targetStructure;
	}
}
