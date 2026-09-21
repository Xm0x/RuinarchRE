using System;
using Traits;

public class BountyHuntPartyQuest : PartyQuest
{
	public Character targetCharacter { get; private set; }

	public override IPartyQuestTarget target => targetCharacter;

	public override Type serializedData => typeof(SaveDataBountyHuntPartyQuest);

	public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;

	public override bool waitingToWorkingStateImmediately => true;

	public BountyHuntPartyQuest()
		: base(PARTY_QUEST_TYPE.Bounty_Hunt)
	{
		base.minimumPartySize = 2;
		base.priority = 4;
		base.relatedBehaviour = typeof(BountyHuntBehaviour);
	}

	public BountyHuntPartyQuest(SaveDataBountyHuntPartyQuest data)
		: base(data)
	{
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		if (targetCharacter.currentStructure != null && targetCharacter.currentStructure.structureType != STRUCTURE_TYPE.WILDERNESS)
		{
			return targetCharacter.currentStructure;
		}
		if (targetCharacter.gridTileLocation != null)
		{
			return targetCharacter.areaLocation;
		}
		return base.GetTargetDestination();
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName + " " + targetCharacter.name;
	}

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		if (targetCharacter != null)
		{
			Prisoner traitOrStatus = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
			if (traitOrStatus != null)
			{
				if (traitOrStatus.IsFactionPrisonerOf(p_faction))
				{
					return false;
				}
				if (traitOrStatus.IsFactionPrisonerOf(targetCharacter.faction) && targetCharacter.faction != null)
				{
					return false;
				}
			}
			return targetCharacter.crimeComponent.IsWantedBy(p_faction);
		}
		return false;
	}

	protected override void OnEndQuest()
	{
		base.OnEndQuest();
		if (base.assignedParty == null)
		{
			return;
		}
		for (int i = 0; i < base.assignedParty.membersThatJoinedQuest.Count; i++)
		{
			Character character = base.assignedParty.membersThatJoinedQuest[i];
			JobQueueItem currentJob = character.currentJob;
			if (currentJob != null && currentJob.jobType == JOB_TYPE.GO_TO && character.currentJob.poiTarget == targetCharacter)
			{
				character.currentJob.ForceCancelJob();
			}
			character.ForceCancelJobTypesTargetingPOI(JOB_TYPE.APPREHEND, targetCharacter);
			character.ForceCancelJobTypesTargetingPOI(JOB_TYPE.APPREHEND_RESTRAINED, targetCharacter);
		}
	}

	public void SetTargetCharacter(Character character)
	{
		targetCharacter = character;
	}

	public override void LoadReferences(SaveDataPartyQuest data)
	{
		base.LoadReferences(data);
		if (data is SaveDataBountyHuntPartyQuest saveDataBountyHuntPartyQuest && !string.IsNullOrEmpty(saveDataBountyHuntPartyQuest.targetCharacter))
		{
			targetCharacter = CharacterManager.Instance.GetCharacterByPersistentID(saveDataBountyHuntPartyQuest.targetCharacter);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = targetCharacter;
	}
}
