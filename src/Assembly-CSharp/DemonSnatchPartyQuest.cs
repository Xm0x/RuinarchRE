using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

public class DemonSnatchPartyQuest : PartyQuest
{
	public Character targetCharacter { get; private set; }

	public LocationStructure dropStructure { get; private set; }

	public override IPartyQuestTarget target => targetCharacter;

	public override Type serializedData => typeof(SaveDataDemonSnatchPartyQuest);

	public override bool waitingToWorkingStateImmediately => true;

	public DemonSnatchPartyQuest()
		: base(PARTY_QUEST_TYPE.Demon_Snatch)
	{
		base.minimumPartySize = 1;
		base.priority = 5;
		base.relatedBehaviour = typeof(DemonSnatchBehaviour);
	}

	public DemonSnatchPartyQuest(SaveDataDemonSnatchPartyQuest data)
		: base(data)
	{
	}

	public override void OnAcceptQuest(Party partyThatAcceptedQuest)
	{
		base.OnAcceptQuest(partyThatAcceptedQuest);
		Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfSnatchJobIsFinished);
		Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		Messenger.AddListener<Prisoner>(TraitSignals.HAS_BECOME_PRISONER, OnHasBecomePrisoner);
		Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnSnatchJobRemoved);
		Messenger.AddListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchedFromLimbo);
		if (dropStructure is DemonicStructure demonicStructure)
		{
			demonicStructure.SetPreOccupiedBy(targetCharacter);
		}
		targetCharacter.limiterComponent.IncreaseTargetedByDemonicSnatch();
	}

	protected override void OnEndQuest()
	{
		base.OnEndQuest();
		Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		Messenger.RemoveListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfSnatchJobIsFinished);
		Messenger.RemoveListener<Prisoner>(TraitSignals.HAS_BECOME_PRISONER, OnHasBecomePrisoner);
		Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnSnatchJobRemoved);
		Messenger.RemoveListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchedFromLimbo);
		if (dropStructure is DemonicStructure demonicStructure)
		{
			demonicStructure.SetPreOccupiedBy(null);
		}
		targetCharacter.limiterComponent.DecreaseTargetedByDemonicSnatch();
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

	public void SetDropStructure(LocationStructure p_structure)
	{
		dropStructure = p_structure;
	}

	private void CheckIfSnatchJobIsFinished(Character p_character, GoapPlanJob p_job)
	{
		if (p_job.jobType == JOB_TYPE.SNATCH && p_job.poiTarget == targetCharacter)
		{
			SetIsSuccessful(state: true);
		}
	}

	private void OnUnseizePOI(IPointOfInterest poi)
	{
		if (poi != targetCharacter || base.assignedParty == null)
		{
			return;
		}
		Prisoner traitOrStatus = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
		if (traitOrStatus != null && traitOrStatus.IsFactionPrisonerOf(PlayerManager.Instance.player.playerFaction))
		{
			CreateSnatchJobFor(targetCharacter, base.assignedParty, dropStructure);
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

	private void OnHasBecomePrisoner(Prisoner p_prisoner)
	{
		if (p_prisoner.owner != targetCharacter || base.assignedParty == null)
		{
			return;
		}
		LocationStructure currentStructure = p_prisoner.owner.currentStructure;
		if (p_prisoner.IsFactionPrisonerOf(PlayerManager.Instance.player.playerFaction) && currentStructure != null && currentStructure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS)
		{
			EndQuest(PartyQuest.GetLocalizedEndQuestReason("Already_Prisoner_Demon"));
			return;
		}
		for (int i = 0; i < base.assignedParty.membersThatJoinedQuest.Count; i++)
		{
			Character character = base.assignedParty.membersThatJoinedQuest[i];
			if (character.currentJob != null && character.currentJob.isThisAPartyJob && (character.currentJob.jobType == JOB_TYPE.SNATCH_RESTRAIN || character.currentJob.jobType == JOB_TYPE.GO_TO || character.currentJob.jobType == JOB_TYPE.PARTY_GO_TO))
			{
				character.currentJob.CancelJob();
			}
		}
	}

	private void OnSnatchJobRemoved(JobQueueItem job, Character character)
	{
		if (job.jobType != JOB_TYPE.SNATCH || !(job is GoapPlanJob goapPlanJob) || goapPlanJob.poiTarget != targetCharacter || base.assignedParty == null)
		{
			return;
		}
		for (int i = 0; i < base.assignedParty.membersThatJoinedQuest.Count; i++)
		{
			Character character2 = base.assignedParty.membersThatJoinedQuest[i];
			if (character2.currentJob != null && character2.currentJob.isThisAPartyJob && (character2.currentJob.jobType == JOB_TYPE.GO_TO || character2.currentJob.jobType == JOB_TYPE.PARTY_GO_TO))
			{
				character2.currentJob.CancelJob();
			}
		}
	}

	public void CreateSnatchJobFor(Character p_target, Party p_party, LocationStructure p_dropStructure)
	{
		LocationGridTile locationGridTile = null;
		locationGridTile = ((!(p_dropStructure is Kennel kennel)) ? ((!(p_dropStructure is TortureChambers tortureChambers)) ? (p_dropStructure.GetRandomPassableTile() ?? p_dropStructure.GetRandomTile()) : tortureChambers.GetRandomBorderTile()) : kennel.GetRandomBorderTile());
		if (locationGridTile != null)
		{
			p_party.jobComponent.CreateSnatchJob(p_target, locationGridTile, p_dropStructure);
		}
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
		if (data is SaveDataDemonSnatchPartyQuest saveDataDemonSnatchPartyQuest)
		{
			if (!string.IsNullOrEmpty(saveDataDemonSnatchPartyQuest.targetCharacter))
			{
				targetCharacter = CharacterManager.Instance.GetCharacterByPersistentID(saveDataDemonSnatchPartyQuest.targetCharacter);
			}
			if (!string.IsNullOrEmpty(saveDataDemonSnatchPartyQuest.dropStructure))
			{
				dropStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(saveDataDemonSnatchPartyQuest.dropStructure) as DemonicStructure;
			}
		}
	}

	public override void LoadReferencesInMainThread(SaveDataPartyQuest data)
	{
		base.LoadReferencesInMainThread(data);
		Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfSnatchJobIsFinished);
		Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		Messenger.AddListener<Prisoner>(TraitSignals.HAS_BECOME_PRISONER, OnHasBecomePrisoner);
		Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnSnatchJobRemoved);
		Messenger.AddListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchedFromLimbo);
	}

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		base.CheckIfStructureIsStillReferenced(p_structure);
		_ = dropStructure;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = targetCharacter;
	}
}
