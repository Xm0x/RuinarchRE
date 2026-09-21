using System;
using Inner_Maps.Location_Structures;

public class ClaimHallowedGroundPartyQuest : PartyQuest
{
	public LocationStructure targetStructure { get; private set; }

	public override IPartyQuestTarget target => targetStructure;

	public override Type serializedData => typeof(SaveDataClaimHallowedGroundPartyQuest);

	public ClaimHallowedGroundPartyQuest()
		: base(PARTY_QUEST_TYPE.Claim_Hallowed_Ground)
	{
		base.minimumPartySize = 3;
		base.priority = 5;
		base.isFactionWideQuest = true;
		base.relatedBehaviour = typeof(ClaimHallowedGroundBehaviour);
	}

	public ClaimHallowedGroundPartyQuest(SaveDataClaimHallowedGroundPartyQuest data)
		: base(data)
	{
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		return targetStructure;
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName + " " + targetStructure.name;
	}

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		return targetStructure != null;
	}

	public void SetTargetStructure(LocationStructure p_structure)
	{
		targetStructure = p_structure;
	}

	public override void LoadReferences(SaveDataPartyQuest data)
	{
		base.LoadReferences(data);
		if (data is SaveDataClaimHallowedGroundPartyQuest saveDataClaimHallowedGroundPartyQuest && !string.IsNullOrEmpty(saveDataClaimHallowedGroundPartyQuest.dropStructure))
		{
			targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataClaimHallowedGroundPartyQuest.dropStructure);
		}
	}
}
