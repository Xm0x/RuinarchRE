using System;
using Inner_Maps.Location_Structures;

public class KillVillagerPartyQuest : PartyQuest
{
	public Character targetCharacter { get; private set; }

	public override IPartyQuestTarget target => targetCharacter;

	public override Type serializedData => typeof(SaveDataKillVillagerPartyQuest);

	public override bool waitingToWorkingStateImmediately => true;

	public KillVillagerPartyQuest()
		: base(PARTY_QUEST_TYPE.Kill_Villager)
	{
		base.minimumPartySize = 1;
		base.priority = 5;
		base.relatedBehaviour = typeof(KillVillagerPartyBehaviour);
	}

	public KillVillagerPartyQuest(SaveDataKillVillagerPartyQuest data)
		: base(data)
	{
	}

	public override void OnAcceptQuest(Party partyThatAcceptedQuest)
	{
		base.OnAcceptQuest(partyThatAcceptedQuest);
		Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		Messenger.AddListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchedFromLimbo);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
	}

	protected override void OnEndQuest()
	{
		base.OnEndQuest();
		Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		Messenger.RemoveListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchedFromLimbo);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
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
		return targetCharacter != null;
	}

	public void SetTargetCharacter(Character character)
	{
		targetCharacter = character;
	}

	private void OnUnseizePOI(IPointOfInterest poi)
	{
		if (poi != targetCharacter || base.assignedParty == null)
		{
			return;
		}
		if (!targetCharacter.isDead)
		{
			CreateKillJobFor(targetCharacter, base.assignedParty);
		}
		for (int i = 0; i < base.assignedParty.membersThatJoinedQuest.Count; i++)
		{
			Character character = base.assignedParty.membersThatJoinedQuest[i];
			if (character.currentJob != null && character.currentJob.isThisAPartyJob && (character.currentJob.jobType == JOB_TYPE.PARTY_GO_TO || character.currentJob.jobType == JOB_TYPE.GO_TO))
			{
				character.currentJob.CancelJob();
			}
		}
	}

	private void OnCharacterDied(Character p_character)
	{
		if (p_character == target)
		{
			SetIsSuccessful(state: true);
			EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Dead"));
		}
	}

	public void CreateKillJobFor(Character p_target, Party p_party)
	{
		p_party.jobComponent.CreateKillJob(p_target);
	}

	private void OnCharacterSwitchedFromLimbo(Character p_inLimbo, Character p_activeCharacter)
	{
		if (p_inLimbo == target && base.assignedParty != null)
		{
			SetIsSuccessful(state: false);
			EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Disappeared"));
		}
	}

	protected override bool IsConnectedToStructure(LocationStructure p_structure)
	{
		return false;
	}

	public override void LoadReferences(SaveDataPartyQuest data)
	{
		base.LoadReferences(data);
		if (data is SaveDataKillVillagerPartyQuest saveDataKillVillagerPartyQuest && !string.IsNullOrEmpty(saveDataKillVillagerPartyQuest.targetCharacter))
		{
			targetCharacter = CharacterManager.Instance.GetCharacterByPersistentID(saveDataKillVillagerPartyQuest.targetCharacter);
		}
	}

	public override void LoadReferencesInMainThread(SaveDataPartyQuest data)
	{
		base.LoadReferencesInMainThread(data);
		Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		Messenger.AddListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchedFromLimbo);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = targetCharacter;
	}
}
