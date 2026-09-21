using System;
using System.Collections.Generic;
using System.Linq;
using Factions.Faction_Components;
using Factions.Faction_Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Logs;
using Traits;
using UnityEngine;
using UtilityScripts;

public class Faction : IJobOwner, ISavable, ILogFiller
{
	private string _uiString;

	public string persistentID { get; }

	public int id { get; }

	public string name { get; private set; }

	public string description { get; private set; }

	public bool isMajorFaction { get; private set; }

	public ILeader leader { get; private set; }

	public Sprite emblem { get; private set; }

	public string emblemName { get; private set; }

	public Color factionColor { get; private set; }

	public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; }

	public List<Character> characters { get; }

	public List<BaseSettlement> ownedSettlements { get; }

	public List<Character> bannedCharacters { get; }

	public Dictionary<Faction, FactionRelationship> relationships { get; }

	public FactionType factionType { get; protected set; }

	public bool isActive { get; private set; }

	public List<JobQueueItem> availableJobs { get; }

	public int newLeaderDesignationChance { get; private set; }

	public uint pathfindingTag { get; private set; }

	public uint pathfindingDoorTag { get; private set; }

	public bool isDisbanded { get; private set; }

	public bool isAwareOfPlayer { get; private set; }

	public bool isInfoUnlocked { get; private set; }

	public RACE race { get; private set; }

	public FactionJobTriggerComponent factionJobTriggerComponent { get; private set; }

	public PartyQuestBoard partyQuestBoard { get; private set; }

	public FactionEventDispatcher factionEventDispatcher { get; private set; }

	public FactionIdeologyComponent ideologyComponent { get; private set; }

	public FactionSuccessionComponent successionComponent { get; private set; }

	public FactionCrimeComponent crimeComponent { get; private set; }

	public FactionOpinionComponent opinionComponent { get; private set; }

	public FactionCharactersComponent charactersComponent { get; private set; }

	public bool isMajorOrVagrant
	{
		get
		{
			if (!isMajorFaction)
			{
				return factionType.type == FACTION_TYPE.Vagrants;
			}
			return true;
		}
	}

	public bool isMajorNonPlayerOrVagrant
	{
		get
		{
			if (!isMajorNonPlayer)
			{
				return factionType.type == FACTION_TYPE.Vagrants;
			}
			return true;
		}
	}

	public bool isMajorNonPlayer
	{
		get
		{
			if (isMajorFaction)
			{
				return !isPlayerFaction;
			}
			return false;
		}
	}

	public bool isMajorNonPlayerOrBandits
	{
		get
		{
			if (!isMajorNonPlayer)
			{
				return factionType.type == FACTION_TYPE.Bandits;
			}
			return true;
		}
	}

	public bool isMajorOrBandits
	{
		get
		{
			if (!isMajorFaction)
			{
				return factionType.type == FACTION_TYPE.Bandits;
			}
			return true;
		}
	}

	public bool isNonMajorOrPlayer
	{
		get
		{
			if (isMajorFaction)
			{
				return isPlayerFaction;
			}
			return true;
		}
	}

	public JobTriggerComponent jobTriggerComponent => factionJobTriggerComponent;

	public bool isPlayerFaction => factionType.type == FACTION_TYPE.Demons;

	public string nameWithColor => GetNameWithColor();

	public JOB_OWNER ownerType => JOB_OWNER.FACTION;

	public OBJECT_TYPE objectType => OBJECT_TYPE.Faction;

	public Type serializedData => typeof(SaveDataFaction);

	public string uiString => GetUIString();

	public Faction(FACTION_TYPE p_factionType, RACE p_race = RACE.NONE)
	{
		persistentID = Utilities.GetNewUniqueID();
		id = Utilities.SetID(this);
		SetName(RandomNameGenerator.GenerateFactionName());
		SetFactionColor(Utilities.GetColorForFaction());
		SetFactionType(p_factionType);
		SetFactionActiveState(state: true);
		SetIsInfoUnlocked(p_state: true);
		race = ((p_race == RACE.NONE) ? p_factionType.GetRaceForFactionType(randomizeDefault: true) : p_race);
		characters = new List<Character>();
		relationships = new Dictionary<Faction, FactionRelationship>();
		ownedSettlements = new List<BaseSettlement>();
		bannedCharacters = new List<Character>();
		availableJobs = new List<JobQueueItem>();
		forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
		partyQuestBoard = new PartyQuestBoard(this);
		factionEventDispatcher = new FactionEventDispatcher();
		factionJobTriggerComponent = new FactionJobTriggerComponent(this);
		ideologyComponent = new FactionIdeologyComponent();
		ideologyComponent.SetOwner(this);
		successionComponent = new FactionSuccessionComponent();
		successionComponent.SetOwner(this);
		crimeComponent = new FactionCrimeComponent();
		crimeComponent.SetOwner(this);
		opinionComponent = new FactionOpinionComponent();
		opinionComponent.SetOwner(this);
		charactersComponent = new FactionCharactersComponent();
		charactersComponent.SetOwner(this);
		ResetNewLeaderDesignationChance();
		AddListeners(shouldLock: false);
	}

	public Faction(SaveDataFaction data)
	{
		persistentID = data.persistentID;
		id = Utilities.SetID(this, data.id);
		factionJobTriggerComponent = new FactionJobTriggerComponent(this);
		factionType = data.factionType.Load();
		partyQuestBoard = data.partyQuestBoard.Load();
		ideologyComponent = data.ideologyComponent.Load();
		ideologyComponent.SetOwner(this);
		successionComponent = data.successionComponent.Load();
		successionComponent.SetOwner(this);
		crimeComponent = data.crimeComponent.Load();
		crimeComponent.SetOwner(this);
		opinionComponent = data.opinionComponent.Load();
		opinionComponent.SetOwner(this);
		if (data.charactersComponent == null)
		{
			charactersComponent = new FactionCharactersComponent();
			charactersComponent.SetOwner(this);
		}
		else
		{
			charactersComponent = data.charactersComponent.Load();
			charactersComponent.SetOwner(this);
		}
		name = data.name;
		description = data.description;
		factionColor = data.factionColor;
		race = data.race;
		isActive = data.isActive;
		isMajorFaction = data.isMajorFaction;
		newLeaderDesignationChance = data.newLeaderDesignationChance;
		pathfindingTag = data.pathfindingTag;
		pathfindingDoorTag = data.pathfindingDoorTag;
		isAwareOfPlayer = data.isAwareOfPlayer;
		characters = new List<Character>();
		relationships = new Dictionary<Faction, FactionRelationship>();
		ownedSettlements = new List<BaseSettlement>();
		bannedCharacters = new List<Character>();
		availableJobs = new List<JobQueueItem>();
		forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
		factionEventDispatcher = new FactionEventDispatcher();
		isInfoUnlocked = data.isInfoUnlocked;
		isDisbanded = data.isDisbanded;
	}

	public bool CanCharacterJoinFactionBasedOnNonReligionIdeologiesAndBanning(Character p_character)
	{
		if (IsCharacterBannedFromJoining(p_character))
		{
			return false;
		}
		if (!ideologyComponent.DoesCharacterFitCurrentIdeologiesIgnoreReligion(p_character))
		{
			return false;
		}
		return true;
	}

	public bool JoinFaction(Character character, bool broadcastSignal = true, bool bypassIdeologyChecking = false, bool isInitial = false)
	{
		if (bypassIdeologyChecking || ideologyComponent.DoesCharacterFitCurrentIdeologies(character))
		{
			if (AddCharacter(character))
			{
				if (character.homeSettlement != null && character.homeSettlement.owner != null && character.homeSettlement.owner != this)
				{
					character.MigrateHomeStructureTo(null);
				}
				if (!isInitial)
				{
					character.traitContainer.AddTrait(character, "Transitioning");
					character.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT, JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT, JOB_TYPE.HAPPINESS_RECOVERY);
				}
				character.behaviourComponent.UpdateDefaultBehaviourSet();
				if (factionType.type == FACTION_TYPE.Undead && character.necromancerTrait != null && !HasAliveNecromancerLeaderExcept(character))
				{
					FactionManager.Instance.undeadFaction.OnlySetLeader(character);
					Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Become Faction Leader became_leader", LOG_TAG.Major);
					log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
					log.AddLogToDatabase();
					PlayerManager.Instance.player.ShowNotificationFrom(character, log, releaseLogAfter: true);
				}
				character.jobQueue.CancelAllJobs(JOB_TYPE.REPORT_CRIME);
				if (!isActive && factionType is CultFaction)
				{
					SetFactionActiveState(state: true);
				}
				if (broadcastSignal)
				{
					Messenger.Broadcast(FactionSignals.CHARACTER_ADDED_TO_FACTION, character, this);
				}
			}
			return true;
		}
		return false;
	}

	public bool LeaveFaction(Character character)
	{
		if (characters.Remove(character))
		{
			if (leader == character)
			{
				SetLeader(null);
			}
			character.SetFaction(null);
			Messenger.Broadcast(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, character, this);
			if (characters.All((Character c) => c.isDead) && isMajorNonPlayer)
			{
				if (factionType.type == FACTION_TYPE.Demon_Cult || factionType.type == FACTION_TYPE.Wiccans || factionType.type == FACTION_TYPE.Divine_Church)
				{
					SetFactionActiveState(state: false);
				}
				else
				{
					Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "disband", LOG_TAG.Major);
					log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
					log.AddLogToDatabase();
					if (PlayerManager.Instance.player != null)
					{
						PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
					}
					DisbandFaction();
				}
			}
			return true;
		}
		return false;
	}

	public bool LeaveFactionForDisband(Character character)
	{
		if (characters.Remove(character))
		{
			if (leader == character)
			{
				SetLeader(null);
			}
			character.SetFaction(null);
			Messenger.Broadcast(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, character, this);
			return true;
		}
		return false;
	}

	private bool AddCharacter(Character character)
	{
		if (!characters.Contains(character))
		{
			characters.Add(character);
			character.SetFaction(this);
			factionType.ProcessNewMember(character);
			opinionComponent.OnFactionMemberAdded(character);
			return true;
		}
		return false;
	}

	public void OnlySetLeader(ILeader newLeader)
	{
		if (this.leader == newLeader)
		{
			return;
		}
		ILeader leader = this.leader;
		this.leader = newLeader;
		if (newLeader == null && leader is Character character)
		{
			FactionManager.Instance.RevalidateFactionCrimesOnLeaderRemoved(this, character);
		}
		if (leader != null && leader is Character character2 && isMajorNonPlayer)
		{
			character2.behaviourComponent.RemoveBehaviourComponent(typeof(FactionLeaderBehaviour));
			if (!character2.isSettlementRuler)
			{
				character2.jobComponent.RemoveAbleJob(JOB_TYPE.JUDGE_PRISONER);
				character2.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.REPORT_CRIME);
			}
		}
		Character character3 = this.leader as Character;
		if (this.leader != null && character3 != null && isMajorNonPlayer)
		{
			character3.behaviourComponent.AddBehaviourComponent(typeof(FactionLeaderBehaviour));
			character3.jobComponent.AddAbleJob(JOB_TYPE.JUDGE_PRISONER);
		}
		if (character3 != null)
		{
			Messenger.Broadcast(CharacterSignals.ON_SET_AS_FACTION_LEADER, character3, leader);
		}
		else if (newLeader == null)
		{
			Messenger.Broadcast(CharacterSignals.ON_FACTION_LEADER_REMOVED, this, leader);
		}
		factionEventDispatcher.ExecuteFactionLeaderChangedEvent(newLeader);
		factionType.ProcessOnFactionLeaderChanged(leader, newLeader);
		ProcessFactionLeaderAsSettlementRuler();
	}

	public void ProcessFactionLeaderAsSettlementRuler()
	{
		if (leader == null || !(leader is Character { homeSettlement: { } homeSettlement } character) || (homeSettlement.locationType != LOCATION_TYPE.VILLAGE && homeSettlement.locationType != LOCATION_TYPE.PSEUDO_VILLAGE) || homeSettlement.owner != this)
		{
			return;
		}
		Character ruler = homeSettlement.ruler;
		if (ruler == character)
		{
			return;
		}
		if (GameManager.Instance.gameHasStarted)
		{
			if (ruler == null)
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Settlement_Ruler, character);
				return;
			}
			homeSettlement.SetRuler(character);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "replace_ruler", LOG_TAG.Life_Changes);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(ruler, ruler.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddToFillers(homeSettlement, homeSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
		else
		{
			homeSettlement.SetRuler(character);
		}
	}

	private void OnCharacterRaceChange(Character character)
	{
		CheckIfCharacterStillFitsIdeology(character, willLog: true, rollForGrudge: false);
	}

	private void OnCharacterRemoved(Character character)
	{
		LeaveFaction(character);
	}

	private void OnCharacterGainedTrait(Character character, Trait trait)
	{
		if (character == leader && (trait is DemonCultist || trait is Witch || trait is Cleric))
		{
			ideologyComponent.OnLeaderBecameCultist(character);
		}
	}

	public void CheckIfCharacterStillFitsIdeology(Character character, bool willLog = true, bool rollForGrudge = true)
	{
		if (character.faction != this || character.isDead || ideologyComponent.DoesCharacterFitCurrentIdeologies(character) || !character.race.IsSapient())
		{
			return;
		}
		if (willLog)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "left_faction_not_fit");
		}
		else
		{
			character.ChangeToDefaultFaction();
		}
		if (rollForGrudge && leader is Character character2)
		{
			int chance = 0;
			if (character.moodComponent.moodState == MOOD_STATE.Normal)
			{
				chance = ChanceData.GetChance(CHANCE_TYPE.Grudge_Exile_Normal_Mood);
			}
			else if (character.moodComponent.moodState == MOOD_STATE.Bad)
			{
				chance = ChanceData.GetChance(CHANCE_TYPE.Grudge_Exile_Bad_Mood);
			}
			else if (character.moodComponent.moodState == MOOD_STATE.Critical)
			{
				chance = ChanceData.GetChance(CHANCE_TYPE.Grudge_Exile_Critical_Mood);
			}
			if (GameUtilities.RollChance(chance) && character.relationshipContainer.SetHasGrudgeAgainst(character, character2, p_state: true))
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Grudge", "Relationships_Table", "Grudge_Expelled", LOG_TAG.Social);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(character2, character2.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
			}
		}
	}

	public bool IsCharacterBannedFromJoining(Character character)
	{
		return HasCharacterBeenBanned(character);
	}

	private bool HasCharacterBeenBanned(Character character)
	{
		for (int i = 0; i < bannedCharacters.Count; i++)
		{
			if (character == bannedCharacters[i])
			{
				return true;
			}
		}
		return false;
	}

	public void AddBannedCharacter(Character character)
	{
		if (!HasCharacterBeenBanned(character))
		{
			bannedCharacters.Add(character);
		}
	}

	public void KickOutCharacterAndRollForGrudge(Character character, out bool p_wasGrudgeAdded)
	{
		p_wasGrudgeAdded = false;
		if (character.faction != this)
		{
			return;
		}
		AddBannedCharacter(character);
		character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "kick_out_faction_character");
		if (leader is Character character2)
		{
			int chance = 0;
			if (character.moodComponent.moodState == MOOD_STATE.Normal)
			{
				chance = ChanceData.GetChance(CHANCE_TYPE.Grudge_Exile_Normal_Mood);
			}
			else if (character.moodComponent.moodState == MOOD_STATE.Bad)
			{
				chance = ChanceData.GetChance(CHANCE_TYPE.Grudge_Exile_Bad_Mood);
			}
			else if (character.moodComponent.moodState == MOOD_STATE.Critical)
			{
				chance = ChanceData.GetChance(CHANCE_TYPE.Grudge_Exile_Critical_Mood);
			}
			if (GameUtilities.RollChance(chance) && character.relationshipContainer.SetHasGrudgeAgainst(character, character2, p_state: true))
			{
				p_wasGrudgeAdded = true;
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Grudge", "Relationships_Table", "Grudge_Expelled", LOG_TAG.Social);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(character2, character2.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
			}
		}
	}

	public void KickOutCharacter(Character character)
	{
		if (character.faction == this)
		{
			AddBannedCharacter(character);
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "kick_out_faction_character");
		}
	}

	public void KickOutCharacterByPlayer(Character character)
	{
		if (character.faction == this)
		{
			AddBannedCharacter(character);
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "kick_out_faction_character");
		}
	}

	private void OnCharacterDied(Character deadCharacter)
	{
		if (leader != null && deadCharacter == leader)
		{
			SetLeader(null);
		}
		successionComponent.OnCharacterDied(deadCharacter);
		UpdateFactionCount();
		if (!isDisbanded && isMajorNonPlayer && characters.All((Character c) => c.isDead))
		{
			if (factionType.type == FACTION_TYPE.Demon_Cult || factionType.type == FACTION_TYPE.Wiccans || factionType.type == FACTION_TYPE.Divine_Church)
			{
				SetFactionActiveState(state: false);
			}
			else
			{
				DisbandFaction();
			}
		}
	}

	private void DisbandFaction()
	{
		isDisbanded = true;
		SetFactionActiveState(state: false);
		ClearOutReservedFactionData();
		List<Character> list = RuinarchListPool<Character>.Claim();
		list.AddRange(characters);
		for (int i = 0; i < list.Count; i++)
		{
			Character character = list[i];
			LeaveFactionForDisband(character);
		}
		RuinarchListPool<Character>.Release(list);
		RemoveListeners();
		factionJobTriggerComponent.OnDisbandFaction();
		partyQuestBoard.OnDisbandFaction();
		factionEventDispatcher.OnDisbandFaction();
		ideologyComponent.OnDisbandFaction();
		successionComponent.OnDisbandFaction();
		crimeComponent.OnDisbandFaction();
		opinionComponent.OnDisbandFaction();
		charactersComponent.OnDisbandFaction();
		FactionManager.Instance.RemoveRelationshipsWith(this);
		Messenger.Broadcast(FactionSignals.FACTION_DISBANDED, this);
	}

	private void ClearOutReservedFactionData()
	{
		FactionEmblemRandomizer.SetEmblemAsUnUsed(emblem);
		InnerMapManager.Instance.ReturnPathfindingPair(this);
	}

	private void OnCharacterReturnToLife(Character deadCharacter)
	{
		UpdateFactionCount();
	}

	private void OnFactionMemberChanges(Character newMember, Faction faction)
	{
		UpdateFactionCount();
	}

	private void OnNewVillagerMigrated(Character character)
	{
		UpdateFactionCount();
	}

	private void OnUpdateFactionCount(Faction faction)
	{
		if (faction == this)
		{
			UpdateFactionCount();
		}
	}

	private void UpdateFactionCount()
	{
		FactionInfoHubUI.Instance.UpdateFactionItem(this);
	}

	public void SetLeader(ILeader newLeader)
	{
		if (!isMajorOrBandits)
		{
			return;
		}
		OnlySetLeader(newLeader);
		if (newLeader != null)
		{
			if (newLeader is Character character)
			{
				character.currentRegion?.AddFactionHere(this);
				Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForNewLeaderDesignation);
			}
		}
		else
		{
			Messenger.AddListener(Signals.HOUR_STARTED, CheckForNewLeaderDesignation);
		}
	}

	private void CheckForNewLeaderDesignation()
	{
		if (UnityEngine.Random.Range(0, 100) < newLeaderDesignationChance)
		{
			DesignateNewLeader();
		}
		else
		{
			newLeaderDesignationChance += 2;
		}
	}

	public void DesignateNewLeader(bool willLog = true)
	{
		Character character = successionComponent.PickSuccessor();
		if (character != null)
		{
			if (willLog)
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Faction_Leader, character, "succession");
			}
			else
			{
				SetLeader(character);
			}
		}
		ResetNewLeaderDesignationChance();
	}

	private void ResetNewLeaderDesignationChance()
	{
		newLeaderDesignationChance = 5;
	}

	public void GenerateInitialOpinionBetweenMembers()
	{
		for (int i = 0; i < characters.Count; i++)
		{
			Character character = characters[i];
			for (int j = 0; j < characters.Count; j++)
			{
				Character character2 = characters[j];
				if (character != character2)
				{
					character.relationshipContainer.AdjustOpinion(character, character2, "Base", 0);
				}
			}
		}
	}

	public bool HasMemberThatIsNotDeadHasHomeSettlementUnoccupiedDwelling()
	{
		for (int i = 0; i < characters.Count; i++)
		{
			Character character = characters[i];
			if (!character.isDead && character.homeSettlement != null && character.homeSettlement.GetFirstStructureThatIsUnoccupiedDwelling() != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemberThatIsNotDeadAndIsFamilyOrLoverAndNotEnemyRivalWith(Character p_character)
	{
		for (int i = 0; i < characters.Count; i++)
		{
			Character character = characters[i];
			if (!character.isDead && (p_character.relationshipContainer.IsFamilyMember(character) || p_character.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.LOVER)) && !p_character.relationshipContainer.IsEnemiesWith(character))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemberThatIsNotDeadAndIsCloseFriendWith(Character p_character)
	{
		for (int i = 0; i < characters.Count; i++)
		{
			Character character = characters[i];
			if (!character.isDead && p_character.relationshipContainer.GetOpinionLabel(character) == "Close Friend")
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemberThatIsNotDeadAndIsRivalWith(Character p_character)
	{
		for (int i = 0; i < characters.Count; i++)
		{
			Character character = characters[i];
			if (!character.isDead && p_character.relationshipContainer.GetOpinionLabel(character) == "Rival")
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemberThatIsNotDeadAndIsFriendWith(Character p_character)
	{
		for (int i = 0; i < characters.Count; i++)
		{
			Character character = characters[i];
			if (!character.isDead && p_character.relationshipContainer.GetOpinionLabel(character) == "Friend")
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemberThatIsSapientAndIsAtHomeOrHasJoinedQuest()
	{
		for (int i = 0; i < characters.Count; i++)
		{
			Character character = characters[i];
			if (character.race.IsSapient() && (character.IsAtHome() || character.partyComponent.isMemberThatJoinedQuest))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemberThatIsNotDead()
	{
		for (int i = 0; i < characters.Count; i++)
		{
			if (!characters[i].isDead)
			{
				return true;
			}
		}
		return false;
	}

	public int GetAliveMembersCount()
	{
		int num = 0;
		for (int i = 0; i < characters.Count; i++)
		{
			if (!characters[i].isDead)
			{
				num++;
			}
		}
		return num;
	}

	public int GetAliveNonEphemeralMembersCount()
	{
		int num = 0;
		for (int i = 0; i < characters.Count; i++)
		{
			Character character = characters[i];
			if (!character.isDead && !character.traitContainer.HasTrait("Ephemeral"))
			{
				num++;
			}
		}
		return num;
	}

	public bool HasAliveMember()
	{
		for (int i = 0; i < characters.Count; i++)
		{
			if (!characters[i].isDead)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAliveMemberExcept(Character p_except)
	{
		for (int i = 0; i < characters.Count; i++)
		{
			Character character = characters[i];
			if (!character.isDead && character != p_except)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAliveNecromancerLeaderExcept(Character p_character)
	{
		for (int i = 0; i < characters.Count; i++)
		{
			Character character = characters[i];
			if (character != p_character && !character.isDead && character.necromancerTrait != null && leader == character)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemberWithJob(JOB_TYPE p_jobType, Character p_exception = null)
	{
		for (int i = 0; i < characters.Count; i++)
		{
			Character character = characters[i];
			if ((p_exception == null || p_exception != character) && character.jobQueue.HasJob(p_jobType))
			{
				return true;
			}
		}
		return false;
	}

	private void AddListeners(bool shouldLock)
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_REMOVED, OnCharacterRemoved, shouldLock);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_RACE, OnCharacterRaceChange, shouldLock);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied, shouldLock);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_RETURNED_TO_LIFE, OnCharacterReturnToLife, shouldLock);
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnFactionMemberChanges, shouldLock);
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnFactionMemberChanges, shouldLock);
		Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerMigrated, shouldLock);
		Messenger.AddListener<Faction>(FactionSignals.UPDATE_FACTION_COUNT, OnUpdateFactionCount, shouldLock);
		Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted, shouldLock);
		Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded, shouldLock);
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait, shouldLock);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter, shouldLock);
		successionComponent.AddListeners(shouldLock);
		charactersComponent.AddListeners(shouldLock);
		opinionComponent.SubscribeListeners(shouldLock);
	}

	private void RemoveListeners()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_REMOVED, OnCharacterRemoved);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_RACE, OnCharacterRaceChange);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_RETURNED_TO_LIFE, OnCharacterReturnToLife);
		Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnFactionMemberChanges);
		Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnFactionMemberChanges);
		Messenger.RemoveListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerMigrated);
		Messenger.RemoveListener<Faction>(FactionSignals.UPDATE_FACTION_COUNT, OnUpdateFactionCount);
		Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
		Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
		Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForNewLeaderDesignation);
		successionComponent.RemoveListeners();
		charactersComponent.RemoveListeners();
		opinionComponent.UnsubscribeListeners();
	}

	private void DisconnectFromCharacter(Character p_character)
	{
		charactersComponent?.DisconnectFromCharacter(p_character);
		characters.Remove(p_character);
		bannedCharacters.Remove(p_character);
		crimeComponent?.DisconnectFromCharacter(p_character);
	}

	private void SetFactionColor(Color color)
	{
		factionColor = color;
	}

	public void SetName(string name)
	{
		this.name = name;
	}

	private void SetDescription(string description)
	{
		this.description = description;
	}

	public void SetIsMajorFaction(bool state)
	{
		isMajorFaction = state;
	}

	public void SetIsInfoUnlocked(bool p_state)
	{
		isInfoUnlocked = p_state;
	}

	public void SetIsAwareOfPlayer(bool p_state)
	{
		if (p_state != isAwareOfPlayer)
		{
			isAwareOfPlayer = p_state;
			if (isAwareOfPlayer)
			{
				Messenger.Broadcast(FactionSignals.FACTION_BECAME_AWARE_OF_PLAYER, this);
			}
		}
	}

	public bool IsHostileWith(Faction faction)
	{
		if (faction == this)
		{
			return false;
		}
		if (faction == null)
		{
			return false;
		}
		FactionRelationship relationshipWith = GetRelationshipWith(faction);
		if (relationshipWith == null)
		{
			return false;
		}
		return relationshipWith.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile;
	}

	public bool IsFriendlyWith(Faction faction)
	{
		if (faction == null)
		{
			return false;
		}
		if (faction == this)
		{
			return true;
		}
		FactionRelationship relationshipWith = GetRelationshipWith(faction);
		if (relationshipWith == null)
		{
			return false;
		}
		return relationshipWith.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Friendly;
	}

	public override string ToString()
	{
		return name;
	}

	public void SetFactionActiveState(bool state)
	{
		if (isActive != state)
		{
			isActive = state;
			Messenger.Broadcast(FactionSignals.FACTION_ACTIVE_CHANGED, this);
		}
	}

	private void OnTickEnded()
	{
		ProcessForcedCancelJobsOnTickEnded();
	}

	private void OnDayStarted()
	{
		ClearAllBlacklistToAllExistingJobs();
		PartyQuest partyQuest = partyQuestBoard.GetPartyQuest(PARTY_QUEST_TYPE.Counterattack);
		if (partyQuest != null && partyQuest.assignedParty == null)
		{
			partyQuestBoard.RemovePartyQuest(partyQuest);
		}
		successionComponent.OnDayStarted();
	}

	private string GetNameWithColor()
	{
		if (FactionManager.Instance != null)
		{
			string factionNameColorHex = FactionManager.Instance.GetFactionNameColorHex();
			return "<color=#" + factionNameColorHex + ">" + name + "</color>";
		}
		return name;
	}

	public void AddNewRelationship(Faction relWith, FactionRelationship relationship)
	{
		if (!relationships.ContainsKey(relWith))
		{
			relationships.Add(relWith, relationship);
		}
	}

	public void RemoveRelationshipWith(Faction relWith)
	{
		if (relationships.ContainsKey(relWith))
		{
			relationships.Remove(relWith);
		}
	}

	public FactionRelationship GetRelationshipWith(Faction faction)
	{
		if (relationships.ContainsKey(faction))
		{
			return relationships[faction];
		}
		return null;
	}

	public bool HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS stat, Faction faction)
	{
		if (relationships.ContainsKey(faction))
		{
			return relationships[faction].relationshipStatus == stat;
		}
		return false;
	}

	public bool SetRelationshipFor(Faction otherFaction, FACTION_RELATIONSHIP_STATUS status)
	{
		if (relationships.ContainsKey(otherFaction))
		{
			return relationships[otherFaction].SetRelationshipStatus(status);
		}
		Debug.LogWarning("There is no key for " + otherFaction.name + " in " + name + "'s relationship dictionary");
		return false;
	}

	public bool IsAtWar()
	{
		foreach (KeyValuePair<Faction, FactionRelationship> relationship in relationships)
		{
			if (relationship.Key.isActive && relationship.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile)
			{
				return true;
			}
		}
		return false;
	}

	public Faction GetRandomAtWarMajorNonPlayerFaction()
	{
		Faction result = null;
		List<Faction> list = RuinarchListPool<Faction>.Claim();
		foreach (KeyValuePair<Faction, FactionRelationship> relationship in relationships)
		{
			if (relationship.Key.isActive && relationship.Key.isMajorNonPlayer && relationship.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile)
			{
				list.Add(relationship.Key);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<Faction>.Release(list);
		return result;
	}

	public Faction GetRandomNotFriendlyMajorNonPlayerFaction()
	{
		Faction result = null;
		List<Faction> list = RuinarchListPool<Faction>.Claim();
		foreach (KeyValuePair<Faction, FactionRelationship> relationship in relationships)
		{
			if (relationship.Key.isActive && relationship.Key.isMajorNonPlayer && relationship.Value.relationshipStatus != FACTION_RELATIONSHIP_STATUS.Friendly)
			{
				list.Add(relationship.Key);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<Faction>.Release(list);
		return result;
	}

	public void AddToOwnedSettlements(BaseSettlement settlement)
	{
		if (!ownedSettlements.Contains(settlement))
		{
			ownedSettlements.Add(settlement);
			Messenger.Broadcast(FactionSignals.FACTION_OWNED_SETTLEMENT_ADDED, this, settlement);
		}
	}

	public void RemoveFromOwnedSettlements(BaseSettlement settlement)
	{
		if (ownedSettlements.Remove(settlement))
		{
			partyQuestBoard.RemoveAllQuestsThatAreMadeInLocation(settlement);
			Messenger.Broadcast(FactionSignals.FACTION_OWNED_SETTLEMENT_REMOVED, this, settlement);
		}
	}

	public bool HasOwnedSettlementInRegion(Region region)
	{
		for (int i = 0; i < ownedSettlements.Count; i++)
		{
			if (ownedSettlements[i] is NPCSettlement nPCSettlement && nPCSettlement.region == region)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasOwnedVillages()
	{
		for (int i = 0; i < ownedSettlements.Count; i++)
		{
			if (ownedSettlements[i].locationType == LOCATION_TYPE.VILLAGE)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasOwnedSettlementExcept(NPCSettlement settlementException)
	{
		for (int i = 0; i < ownedSettlements.Count; i++)
		{
			if (ownedSettlements[i] is NPCSettlement nPCSettlement && nPCSettlement != settlementException)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasOwnedSettlementThatHasAliveResidentAndIsNotHomeOf(Character p_character)
	{
		for (int i = 0; i < ownedSettlements.Count; i++)
		{
			BaseSettlement baseSettlement = ownedSettlements[i];
			if (baseSettlement != p_character.homeSettlement && baseSettlement.HasResidentThatIsNotDead())
			{
				return true;
			}
		}
		return false;
	}

	public BaseSettlement GetRandomOwnedSettlement()
	{
		if (ownedSettlements.Count > 0)
		{
			return ownedSettlements[UnityEngine.Random.Range(0, ownedSettlements.Count)];
		}
		return null;
	}

	public BaseSettlement GetRandomOwnedVillage()
	{
		BaseSettlement result = null;
		List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim();
		for (int i = 0; i < ownedSettlements.Count; i++)
		{
			BaseSettlement baseSettlement = ownedSettlements[i];
			if (baseSettlement.locationType == LOCATION_TYPE.VILLAGE)
			{
				list.Add(baseSettlement);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<BaseSettlement>.Release(list);
		return result;
	}

	public void SetEmblem(Sprite sprite)
	{
		emblem = sprite;
		if (emblem != null)
		{
			emblemName = emblem.name;
		}
		else
		{
			emblemName = string.Empty;
		}
	}

	public void AddToAvailableJobs(JobQueueItem job, int position = -1)
	{
		if (position == -1)
		{
			availableJobs.Add(job);
		}
		else
		{
			availableJobs.Insert(position, job);
		}
	}

	public bool RemoveFromAvailableJobs(JobQueueItem job)
	{
		if (availableJobs.Remove(job))
		{
			_ = job is GoapPlanJob;
			OnJobRemovedFromAvailableJobs(job);
			return true;
		}
		return false;
	}

	public JobQueueItem GetFirstUnassignedJobToCharacterJob(Character character)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && character.jobQueue.CanJobBeAddedToQueue(jobQueueItem))
			{
				return jobQueueItem;
			}
		}
		return null;
	}

	public JobQueueItem GetFirstUnassignedJobToCharacterJobGivenAllowedTypes(Character character, List<JOB_TYPE> p_jobTypes)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && p_jobTypes.Contains(jobQueueItem.jobType) && character.jobQueue.CanJobBeAddedToQueue(jobQueueItem))
			{
				return jobQueueItem;
			}
		}
		return null;
	}

	private void OnJobRemovedFromAvailableJobs(JobQueueItem job)
	{
		JobManager.Instance.ReleaseJob(job);
	}

	private void ClearAllBlacklistToAllExistingJobs()
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			availableJobs[i].ClearBlacklist();
		}
	}

	public void ForceCancelJobTypesTargetingPOI(JOB_TYPE jobType, IPointOfInterest target)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.jobType == jobType && jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (goapPlanJob.targetPOI == target)
				{
					AddForcedCancelJobsOnTickEnded(goapPlanJob);
				}
			}
		}
	}

	public void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character)
	{
	}

	public void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character, bool shouldBlacklist = false)
	{
		if (!job.IsJobStillApplicable())
		{
			RemoveFromAvailableJobs(job);
		}
		else if (shouldBlacklist && character != null)
		{
			job.AddBlacklistedCharacter(character);
		}
	}

	public bool ForceCancelJob(JobQueueItem job)
	{
		return RemoveFromAvailableJobs(job);
	}

	public void AddForcedCancelJobsOnTickEnded(JobQueueItem job)
	{
		if (!forcedCancelJobsOnTickEnded.Contains(job))
		{
			forcedCancelJobsOnTickEnded.Add(job);
		}
	}

	public void ProcessForcedCancelJobsOnTickEnded()
	{
		if (forcedCancelJobsOnTickEnded.Count > 0)
		{
			for (int i = 0; i < forcedCancelJobsOnTickEnded.Count; i++)
			{
				forcedCancelJobsOnTickEnded[i].ForceCancelJob();
			}
			forcedCancelJobsOnTickEnded.Clear();
		}
	}

	public void CheckForWar(Faction targetFaction, CRIME_SEVERITY crimeSeverity, Character crimeCommitter, Character crimeTarget, ActualGoapNode crime)
	{
		if (targetFaction == this || targetFaction == null || factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful))
		{
			return;
		}
		bool num = crimeTarget != null && crimeTarget.faction == this;
		float num2 = 0f;
		if (num)
		{
			switch (crimeSeverity)
			{
			case CRIME_SEVERITY.Misdemeanor:
				num2 = ((!crimeTarget.isFactionLeader) ? ((!crimeTarget.isSettlementRuler) ? 5f : 15f) : 30f);
				break;
			case CRIME_SEVERITY.Serious:
				num2 = ((!crimeTarget.isFactionLeader) ? ((!crimeTarget.isSettlementRuler) ? 20f : 40f) : 60f);
				break;
			case CRIME_SEVERITY.Heinous:
				num2 = ((!crimeTarget.isFactionLeader) ? ((!crimeTarget.isSettlementRuler) ? 35f : 65f) : 90f);
				break;
			}
			if (factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger))
			{
				num2 *= 1.5f;
			}
		}
		else
		{
			if (crimeSeverity == CRIME_SEVERITY.Heinous && (crimeCommitter.isFactionLeader || crimeCommitter.isSettlementRuler))
			{
				num2 = 50f;
			}
			if (factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger))
			{
				num2 *= 1.5f;
			}
		}
		if (UnityEngine.Random.Range(0f, 100f) < num2 && SetRelationshipFor(targetFaction, FACTION_RELATIONSHIP_STATUS.Hostile))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "declare_war", LOG_TAG.Life_Changes);
			log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
			log.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
			log.AddToFillers(crime.descriptionLog.fillers);
			log.AddToFillers(null, crime.descriptionLog.unreplacedText, LOG_IDENTIFIER.APPEND);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		}
	}

	public CRIME_SEVERITY GetCrimeSeverity(Character actor, IPointOfInterest target, CRIME_TYPE crimeType)
	{
		return factionType.GetCrimeSeverity(actor, target, crimeType);
	}

	public void TrySetDemonWorshipAsHeinousCrimeBecauseOfPlayer()
	{
		if (factionType.GetCrimeSeverity(CRIME_TYPE.Demon_Worship) != CRIME_SEVERITY.Heinous)
		{
			factionType.AddCrime(CRIME_TYPE.Demon_Worship, CRIME_SEVERITY.Heinous);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "demon_worship_became_crime_because_of_player", LOG_TAG.Player, LOG_TAG.Crimes);
			log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
			log.AddLogToDatabase();
			PlayerManager.Instance?.player?.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, this);
		}
	}

	public void SetPathfindingTag(uint tag)
	{
		pathfindingTag = tag;
	}

	public void SetPathfindingDoorTag(uint tag)
	{
		pathfindingDoorTag = tag;
	}

	private bool SetFactionType(FactionType factionType)
	{
		if (this.factionType == null || this.factionType.type != factionType.type)
		{
			this.factionType = factionType;
			return true;
		}
		return false;
	}

	private bool SetFactionType(FACTION_TYPE type)
	{
		if (this.factionType == null || this.factionType.type != type)
		{
			FactionType factionType = FactionManager.Instance.CreateFactionType(type);
			return SetFactionType(factionType);
		}
		return false;
	}

	public void FactionProcessingAbductionOrMurder(Character actor, Character target, ActualGoapNode actionDone)
	{
		string log = string.Empty;
		int chance = ChanceData.GetChance(CHANCE_TYPE.War_Declaration_Chance);
		if (factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger))
		{
			chance = 100;
		}
		else if (factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful))
		{
			chance = 30;
		}
		if (!isMajorNonPlayer || !GameUtilities.RollChance(chance, ref log) || !(leader is Character character))
		{
			return;
		}
		FactionRelationship relationshipWith = GetRelationshipWith(actor.faction);
		if (relationshipWith == null || relationshipWith.relationshipStatus != FACTION_RELATIONSHIP_STATUS.Hostile)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Declare_War, actor);
		}
		if (ShouldRescueCharacter(actionDone))
		{
			if (!partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, target) && !partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, target))
			{
				partyQuestBoard.CreateRescuePartyQuest(character, null, target);
			}
		}
		else
		{
			if (actor.homeSettlement == null)
			{
				return;
			}
			Faction faction = actor.faction;
			if (faction != null && faction.factionType.type == FACTION_TYPE.Bandits && actor.homeStructure != null)
			{
				if (!partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Extermination, actor.homeStructure))
				{
					partyQuestBoard.CreateExterminatePartyQuest(character, null, actor.homeStructure);
				}
			}
			else if (!partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Raid, actor.homeSettlement))
			{
				partyQuestBoard.CreateRaidPartyQuest(character, null, actor.homeSettlement);
			}
		}
	}

	public bool ShouldRescueCharacter(ActualGoapNode actionDone)
	{
		if (actionDone.action.goapType == INTERACTION_TYPE.DROP || actionDone.action.goapType == INTERACTION_TYPE.DROP_RESTRAINED || actionDone.action.goapType == INTERACTION_TYPE.ABDUCT || actionDone.associatedJobType.IsJobAbduction())
		{
			return true;
		}
		return false;
	}

	private string GetUIString()
	{
		if (string.IsNullOrEmpty(_uiString))
		{
			string text = GetType().ToString() + "|" + persistentID;
			_uiString = "<link=" + text + ">" + Utilities.ColorizeName(name, FactionManager.Instance.GetFactionNameColorHex()) + "</link>";
		}
		return _uiString;
	}

	public bool HasStructure(STRUCTURE_TYPE p_structure)
	{
		if (ownedSettlements != null)
		{
			for (int i = 0; i < ownedSettlements.Count; i++)
			{
				BaseSettlement baseSettlement = ownedSettlements[i];
				if (!baseSettlement.hasBeenDestroyed && baseSettlement.HasStructure(p_structure))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasStructureBlueprint(STRUCTURE_TYPE p_structure)
	{
		if (ownedSettlements != null)
		{
			for (int i = 0; i < ownedSettlements.Count; i++)
			{
				if (ownedSettlements[i] is NPCSettlement { hasBeenDestroyed: false } nPCSettlement && nPCSettlement.HasBlueprintOnTileForStructure(p_structure))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void LoadReferences(SaveDataFaction data)
	{
		if (!data.isLeaderPlayer && !string.IsNullOrEmpty(data.leaderID))
		{
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(data.leaderID);
			leader = characterByPersistentID;
		}
		for (int i = 0; i < data.characterIDs.Count; i++)
		{
			Character characterByPersistentID2 = CharacterManager.Instance.GetCharacterByPersistentID(data.characterIDs[i]);
			if (characterByPersistentID2 != null)
			{
				characters.Add(characterByPersistentID2);
			}
		}
		for (int j = 0; j < data.bannedCharacterIDs.Count; j++)
		{
			Character characterByPersistentID3 = CharacterManager.Instance.GetCharacterByPersistentID(data.bannedCharacterIDs[j]);
			if (characterByPersistentID3 != null)
			{
				bannedCharacters.Add(characterByPersistentID3);
			}
		}
		if (!isDisbanded)
		{
			foreach (KeyValuePair<string, SaveDataFactionRelationship> relationship in data.relationships)
			{
				Faction factionByPersistentID = FactionManager.Instance.GetFactionByPersistentID(relationship.Key);
				FactionRelationship factionRelationship = GetRelationshipWith(factionByPersistentID);
				if (factionRelationship == null)
				{
					factionRelationship = factionByPersistentID.GetRelationshipWith(this);
					if (factionRelationship == null)
					{
						factionRelationship = relationship.Value.Load();
					}
				}
				AddNewRelationship(factionByPersistentID, factionRelationship);
				factionByPersistentID.AddNewRelationship(this, factionRelationship);
			}
			if (leader == null)
			{
				SetLeader(null);
			}
		}
		for (int k = 0; k < data.ownedSettlementIDs.Count; k++)
		{
			BaseSettlement settlementByPersistentIDSafe = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(data.ownedSettlementIDs[k]);
			if (settlementByPersistentIDSafe != null)
			{
				ownedSettlements.Add(settlementByPersistentIDSafe);
			}
		}
		partyQuestBoard.LoadReferences(data.partyQuestBoard);
		successionComponent.LoadReferences(data.successionComponent);
		crimeComponent.LoadReferences(data.crimeComponent);
		opinionComponent.LoadReferences(data.opinionComponent);
	}

	public void LoadReferencesMainThread(SaveDataFaction data)
	{
		emblem = FactionManager.Instance.GetFactionEmblem(data);
		emblemName = data.emblemName;
		factionType.LoadReferencesInMainThread(data.factionType);
		if (isMajorNonPlayer && isActive && !isDisbanded)
		{
			PathfindingTagPair pathfindingTagPairAsClaimed = new PathfindingTagPair(data.pathfindingTag, data.pathfindingDoorTag);
			InnerMapManager.Instance.SetPathfindingTagPairAsClaimed(pathfindingTagPairAsClaimed);
		}
		if (isActive && !isDisbanded)
		{
			FactionEmblemRandomizer.SetEmblemAsUsed(emblem);
		}
		if (!isDisbanded)
		{
			AddListeners(shouldLock: false);
		}
		if (data.charactersComponent != null)
		{
			charactersComponent.LoadReferences(data.charactersComponent);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		factionJobTriggerComponent.CheckIfStructureIsStillReferenced(p_structure);
		partyQuestBoard.CheckIfStructureIsStillReferenced(p_structure);
		factionEventDispatcher.CheckIfStructureIsStillReferenced(p_structure);
		ideologyComponent.CheckIfStructureIsStillReferenced(p_structure);
		successionComponent.CheckIfStructureIsStillReferenced(p_structure);
		crimeComponent.CheckIfStructureIsStillReferenced(p_structure);
		opinionComponent.CheckIfStructureIsStillReferenced(p_structure);
		charactersComponent.CheckIfStructureIsStillReferenced(p_structure);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = leader;
		characters.Contains(p_character);
		bannedCharacters.Contains(p_character);
		factionJobTriggerComponent?.CheckIfCharacterIsStillReferenced(p_character);
		partyQuestBoard?.CheckIfCharacterIsStillReferenced(p_character);
		factionEventDispatcher?.CheckIfCharacterIsStillReferenced(p_character);
		ideologyComponent?.CheckIfCharacterIsStillReferenced(p_character);
		successionComponent?.CheckIfCharacterIsStillReferenced(p_character);
		crimeComponent?.CheckIfCharacterIsStillReferenced(p_character);
		opinionComponent?.CheckIfCharacterIsStillReferenced(p_character);
		charactersComponent?.CheckIfCharacterIsStillReferenced(p_character);
	}
}
