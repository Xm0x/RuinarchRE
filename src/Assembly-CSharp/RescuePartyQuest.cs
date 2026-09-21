using System;

public class RescuePartyQuest : PartyQuest, IRescuePartyQuest
{
	public Character targetCharacter { get; private set; }

	public bool isReleasing { get; private set; }

	public override IPartyQuestTarget target => targetCharacter;

	public override Type serializedData => typeof(SaveDataRescuePartyQuest);

	public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;

	public override bool waitingToWorkingStateImmediately => true;

	public RescuePartyQuest()
		: base(PARTY_QUEST_TYPE.Rescue)
	{
		base.minimumPartySize = 1;
		base.priority = 5;
		base.relatedBehaviour = typeof(RescueBehaviour);
	}

	public RescuePartyQuest(SaveDataRescuePartyQuest data)
		: base(data)
	{
		isReleasing = data.isReleasing;
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
			return targetCharacter.traitContainer.HasTrait("Restrained");
		}
		return false;
	}

	public void SetTargetCharacter(Character character)
	{
		targetCharacter = character;
	}

	public void SetIsReleasing(bool state)
	{
		isReleasing = state;
	}

	public override bool IsInterestedInJoiningQuest(Character p_character)
	{
		if (p_character == targetCharacter)
		{
			return false;
		}
		if (p_character.relationshipContainer.HasGrudgeAgainst(targetCharacter))
		{
			return false;
		}
		if (targetCharacter != null)
		{
			return !p_character.relationshipContainer.IsEnemiesWith(targetCharacter);
		}
		return base.IsInterestedInJoiningQuest(p_character);
	}

	public override void LoadReferences(SaveDataPartyQuest data)
	{
		base.LoadReferences(data);
		if (data is SaveDataRescuePartyQuest saveDataRescuePartyQuest && !string.IsNullOrEmpty(saveDataRescuePartyQuest.targetCharacter))
		{
			targetCharacter = CharacterManager.Instance.GetCharacterByPersistentID(saveDataRescuePartyQuest.targetCharacter);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = targetCharacter;
	}
}
