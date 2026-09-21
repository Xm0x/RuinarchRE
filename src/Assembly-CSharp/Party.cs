using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Logs;
using Object_Pools;
using Traits;
using UnityEngine;
using UtilityScripts;

public class Party : ILogFiller, ISavable, IJobOwner, IBookmarkable
{
	public interface PartyEventsIListener
	{
		void OnQuestSucceed();

		void OnQuestFailed();
	}

	public Action onQuestSucceed;

	public Action onQuestFailed;

	private PartyJobTriggerComponent _jobComponent;

	private List<Character> _activeMembers;

	private string _uiString;

	public string persistentID { get; private set; }

	public string partyName { get; private set; }

	public PARTY_STATE partyState { get; private set; }

	public bool startedTrueRestingState { get; private set; }

	public bool isDisbanded { get; private set; }

	public bool hasChangedTargetDestination { get; private set; }

	public Character partyLeader { get; private set; }

	public LocationStructure meetingPlace { get; private set; }

	public LocationStructure targetRestingTavern { get; private set; }

	public Area targetCamp { get; private set; }

	public IPartyTargetDestination targetDestination { get; private set; }

	public PartyQuest currentQuest { get; private set; }

	public PARTY_QUEST_TYPE prevQuestType { get; private set; }

	public float partyWalkSpeed { get; private set; }

	public bool doNotDisband { get; private set; }

	public PARTY_QUEST_TYPE plannedPartyQuestType { get; private set; }

	public GameDate waitingEndDate { get; private set; }

	public List<Character> members { get; private set; }

	public List<Character> membersThatJoinedQuest { get; private set; }

	public List<Character> deadMembers { get; private set; }

	public GameDate nextQuestCheckDate { get; private set; }

	public GameDate nextWaitingCheckDate { get; private set; }

	public GameDate endQuestDate { get; private set; }

	public bool hasSetNextSwitchToWaitingStateTrigger { get; private set; }

	public bool hasSetEndQuestDate { get; private set; }

	public int chanceToRetreatUponKnockoutOrDeath { get; private set; }

	public BaseSettlement partySettlement { get; private set; }

	public Faction partyFaction { get; private set; }

	public JobBoard jobBoard { get; private set; }

	public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; private set; }

	public BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

	public PartyBeaconComponent beaconComponent { get; private set; }

	public PartyDamageAccumulator damageAccumulator { get; private set; }

	public PartyBanningComponent banningComponent { get; private set; }

	public BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Text;

	public string name => partyName;

	public string bookmarkName => partyName;

	public OBJECT_TYPE objectType => OBJECT_TYPE.Party;

	public Type serializedData => typeof(SaveDataParty);

	public bool isActive => currentQuest != null;

	public bool isPlayerParty
	{
		get
		{
			Faction faction = partyFaction;
			if ((faction == null || !faction.isPlayerFaction) && plannedPartyQuestType != PARTY_QUEST_TYPE.Demon_Snatch && plannedPartyQuestType != PARTY_QUEST_TYPE.Demon_Defend && plannedPartyQuestType != PARTY_QUEST_TYPE.Demon_Raid)
			{
				return plannedPartyQuestType == PARTY_QUEST_TYPE.Demon_Steal;
			}
			return true;
		}
	}

	public List<Character> activeMembers => GetActiveMembers();

	public JOB_OWNER ownerType => JOB_OWNER.PARTY;

	public JobTriggerComponent jobTriggerComponent => _jobComponent;

	public PartyJobTriggerComponent jobComponent => _jobComponent;

	public string uiString => GetUIString();

	public Party()
	{
		members = new List<Character>();
		membersThatJoinedQuest = new List<Character>();
		_activeMembers = new List<Character>();
		deadMembers = new List<Character>();
		forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
		_jobComponent = new PartyJobTriggerComponent(this);
		jobBoard = new JobBoard();
		beaconComponent = new PartyBeaconComponent();
		beaconComponent.SetOwner(this);
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		damageAccumulator = new PartyDamageAccumulator();
		banningComponent = new PartyBanningComponent();
		banningComponent.SetOwner(this);
	}

	public bool IsPartyTheSameAsThisParty(Party p_party)
	{
		if (p_party == null)
		{
			return false;
		}
		return persistentID == p_party.persistentID;
	}

	private void InitializePartyName(Character partyCreator)
	{
		if (plannedPartyQuestType == PARTY_QUEST_TYPE.Demon_Snatch || plannedPartyQuestType == PARTY_QUEST_TYPE.Demon_Defend || plannedPartyQuestType == PARTY_QUEST_TYPE.Demon_Raid || plannedPartyQuestType == PARTY_QUEST_TYPE.Demon_Rescue)
		{
			partyName = PartyManager.Instance.GetNewPartyNameForPlayerParty(partyCreator, plannedPartyQuestType);
		}
		else
		{
			partyName = PartyManager.Instance.GetNewPartyName(partyCreator);
		}
	}

	public void Initialize(Character partyCreator)
	{
		InitializePartyName(partyCreator);
		persistentID = Utilities.GetNewUniqueID();
		if (partyCreator.faction != null && partyCreator.faction.isPlayerFaction)
		{
			partySettlement = PlayerManager.Instance.player.playerSettlement;
		}
		else
		{
			partySettlement = partyCreator.homeSettlement;
		}
		partyFaction = partyCreator.faction;
		isDisbanded = false;
		forcedCancelJobsOnTickEnded.Clear();
		jobBoard.Initialize();
		beaconComponent.Initialize();
		SetPartyState(PARTY_STATE.None);
		AddMember(partyCreator);
		partySettlement.AddParty(this);
		Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
		Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
		Messenger.AddListener<JobQueueItem, JobBoard>(JobSignals.JOB_REMOVED_FROM_JOB_BOARD, OnJobRemovedFromJobBoard);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		DatabaseManager.Instance.partyDatabase.AddParty(this);
		if (!isPlayerParty)
		{
			InitialScheduleToCheckQuest();
		}
	}

	public void Initialize(SaveDataParty data)
	{
		persistentID = data.persistentID;
		partyName = data.partyName;
		partyState = data.partyState;
		startedTrueRestingState = data.startedTrueRestingState;
		isDisbanded = data.isDisbanded;
		doNotDisband = data.doNotDisband;
		hasChangedTargetDestination = data.hasChangedTargetDestination;
		waitingEndDate = data.waitingEndDate;
		nextQuestCheckDate = data.nextQuestCheckDate;
		hasSetNextSwitchToWaitingStateTrigger = data.hasSetNextSwitchToWaitingStateTrigger;
		nextWaitingCheckDate = data.nextWaitingCheckDate;
		hasSetEndQuestDate = data.hasSetEndQuestDate;
		endQuestDate = data.endQuestDate;
		prevQuestType = data.prevQuestType;
		plannedPartyQuestType = data.plannedPartyQuestType;
		chanceToRetreatUponKnockoutOrDeath = data.chanceToRetreatUponKnockoutOrDeath;
		jobBoard.InitializeFromSaveData(data.jobBoard);
		beaconComponent.Initialize(data.beaconComponent);
		damageAccumulator.Initialize(data.damageAccumulator);
		if (partyName != string.Empty)
		{
			Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
			Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
			Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
			Messenger.AddListener<JobQueueItem, JobBoard>(JobSignals.JOB_REMOVED_FROM_JOB_BOARD, OnJobRemovedFromJobBoard);
			Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
			DatabaseManager.Instance.partyDatabase.AddParty(this);
		}
		if (!isDisbanded)
		{
			SchedulingManager.Instance.AddEntry(nextQuestCheckDate, TryAcceptQuest, null);
		}
		if (partyState == PARTY_STATE.Waiting)
		{
			SchedulingManager.Instance.AddEntry(waitingEndDate, WaitingEndedDecisionMaking, this);
		}
		if (hasSetNextSwitchToWaitingStateTrigger)
		{
			SchedulingManager.Instance.AddEntry(nextWaitingCheckDate, TryStartToWaitQuest, null);
		}
		if (hasSetEndQuestDate)
		{
			SchedulingManager.Instance.AddEntry(endQuestDate, TryScheduledEndQuest, null);
		}
	}

	private void DisconnectFromStructure(LocationStructure p_structure)
	{
		OnMeetingPlaceDestroyed(p_structure);
		if (targetRestingTavern == p_structure && partyState == PARTY_STATE.Resting)
		{
			FindNearbyTavernOrCamp();
			if (targetRestingTavern == null && targetCamp == null)
			{
				SetPartyState(PARTY_STATE.Moving);
			}
		}
	}

	private void DisconnectFromCharacter(Character p_character)
	{
		deadMembers.Remove(p_character);
		_activeMembers.Remove(p_character);
		membersThatJoinedQuest.Remove(p_character);
		banningComponent?.DisconnectFromCharacter(p_character);
		beaconComponent?.DisconnectFromCharacter(p_character);
	}

	private void OnTickEnded()
	{
		ProcessForcedCancelJobsOnTickEnded();
	}

	private void OnHourStarted()
	{
		if (isActive)
		{
			PerHourInWaitingState();
		}
	}

	private void OnJobRemovedFromJobBoard(JobQueueItem p_job, JobBoard p_jobBoard)
	{
		if (jobBoard == p_jobBoard)
		{
			JobRemovedFromJobBoard(p_job);
		}
	}

	public void SetMeetingPlace()
	{
		if (partySettlement == null)
		{
			return;
		}
		if (partySettlement.locationType == LOCATION_TYPE.DUNGEON)
		{
			meetingPlace = partySettlement.GetRandomStructure();
			return;
		}
		meetingPlace = partySettlement.GetRandomStructureWithTypeWhereAPartyHasPathTo(STRUCTURE_TYPE.TAVERN, this);
		if (meetingPlace != null)
		{
			return;
		}
		meetingPlace = partySettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
		if (meetingPlace == null)
		{
			if (isPlayerParty)
			{
				meetingPlace = partySettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL);
			}
			else
			{
				meetingPlace = partySettlement.GetRandomStructure();
			}
		}
	}

	private void OnMeetingPlaceDestroyed(LocationStructure structure)
	{
		if (meetingPlace == structure)
		{
			meetingPlace = null;
			SetMeetingPlace();
		}
	}

	public void TryAcceptQuest()
	{
		if (isDisbanded)
		{
			return;
		}
		if (!isActive && members.Count > 0)
		{
			Character character = (CanAcceptQuests(partyLeader) ? partyLeader : null);
			if (character == null)
			{
				character = GetRandomMemberThatCanAcceptQuests();
			}
			if (character != null)
			{
				partyFaction.partyQuestBoard.RemoveAllNoLongerFitQuests();
				PartyQuest firstPriorityUnassignedPartyQuestFor = partyFaction.partyQuestBoard.GetFirstPriorityUnassignedPartyQuestFor(this, character);
				if (firstPriorityUnassignedPartyQuestFor != null)
				{
					AcceptQuest(firstPriorityUnassignedPartyQuestFor, character);
				}
			}
		}
		if (!isPlayerParty)
		{
			ScheduleNextDateToCheckQuest();
		}
	}

	private bool CanAcceptQuests(Character p_character)
	{
		return !p_character.traitContainer.HasTrait("Restrained", "Unconscious");
	}

	public void TryAcceptQuest(PartyQuest p_quest, Character p_questAcceptor = null)
	{
		if (isDisbanded)
		{
			return;
		}
		if (!isActive)
		{
			Character character = p_questAcceptor;
			if (character == null)
			{
				character = partyLeader;
			}
			if (character == null)
			{
				character = GetRandomMemberThatCanAcceptQuests();
			}
			partyFaction.partyQuestBoard.RemoveAllNoLongerFitQuests();
			AcceptQuest(p_quest, character);
		}
		if (!isPlayerParty)
		{
			ScheduleNextDateToCheckQuest();
		}
	}

	private void TryStartToWaitQuest()
	{
		if (!hasSetNextSwitchToWaitingStateTrigger)
		{
			return;
		}
		hasSetNextSwitchToWaitingStateTrigger = false;
		if (partyState == PARTY_STATE.None && isActive)
		{
			if (currentQuest.workingStateImmediately)
			{
				SetPartyState(PARTY_STATE.Working);
			}
			else
			{
				SetPartyState(PARTY_STATE.Waiting);
			}
		}
	}

	private void TryScheduledEndQuest()
	{
		if (hasSetEndQuestDate)
		{
			hasSetEndQuestDate = false;
			if (isActive)
			{
				currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Work_Hours_Over"));
			}
		}
	}

	private void InitialScheduleToCheckQuest()
	{
		int ticksBasedOnHour = GameManager.Instance.GetTicksBasedOnHour(5);
		int ticksBasedOnHour2 = GameManager.Instance.GetTicksBasedOnHour(7);
		int ticks = GameUtilities.RandomBetweenTwoNumbers(ticksBasedOnHour, ticksBasedOnHour2);
		GameDate gameDate = GameManager.Instance.Today().AddDays(1);
		gameDate.SetTicks(ticks);
		nextQuestCheckDate = gameDate;
		SchedulingManager.Instance.AddEntry(nextQuestCheckDate, TryAcceptQuest, null);
	}

	private void ScheduleNextDateToCheckQuest()
	{
		nextQuestCheckDate = GameManager.Instance.Today().AddDays(1);
		SchedulingManager.Instance.AddEntry(nextQuestCheckDate, TryAcceptQuest, null);
	}

	private void ScheduleToStartWaitingQuest(Character partyMember)
	{
		if (!hasSetNextSwitchToWaitingStateTrigger)
		{
			hasSetNextSwitchToWaitingStateTrigger = true;
			int startTickOfScheduleType = partyMember.dailyScheduleComponent.schedule.GetStartTickOfScheduleType(DAILY_SCHEDULE.Work);
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.SetTicks(startTickOfScheduleType);
			nextWaitingCheckDate = gameDate;
			SchedulingManager.Instance.AddEntry(gameDate, TryStartToWaitQuest, null);
		}
	}

	private void ScheduleToEndQuest(Character partyMember)
	{
		if (!hasSetEndQuestDate)
		{
			hasSetEndQuestDate = true;
			int startTickOfScheduleType = partyMember.dailyScheduleComponent.schedule.GetStartTickOfScheduleType(DAILY_SCHEDULE.Work);
			int endTickOfScheduleType = partyMember.dailyScheduleComponent.schedule.GetEndTickOfScheduleType(DAILY_SCHEDULE.Work);
			GameDate gameDate = ((endTickOfScheduleType >= startTickOfScheduleType) ? GameManager.Instance.Today() : GameManager.Instance.Today().AddDays(1));
			gameDate.SetTicks(endTickOfScheduleType);
			endQuestDate = gameDate;
			SchedulingManager.Instance.AddEntry(gameDate, TryScheduledEndQuest, null);
		}
	}

	private LocationStructure GetStructureToCheckFromSettlement(BaseSettlement settlement)
	{
		LocationStructure locationStructure = settlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
		if (locationStructure == null)
		{
			locationStructure = settlement.GetRandomStructure();
		}
		return locationStructure;
	}

	public void GoBackHomeAndEndQuest()
	{
		currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
	}

	public void SetTargetDestination(IPartyTargetDestination target)
	{
		if (targetDestination != target)
		{
			targetDestination = target;
			SetHasChangedTargetDestination(state: true);
		}
	}

	public void SetHasChangedTargetDestination(bool state)
	{
		hasChangedTargetDestination = state;
	}

	public void SetDoNotDisband(bool state)
	{
		doNotDisband = state;
	}

	private Faction GetPartyFaction()
	{
		if (partyLeader != null && partyLeader.faction != null)
		{
			return partyLeader.faction;
		}
		if (members.Count > 0)
		{
			for (int i = 0; i < members.Count; i++)
			{
				Character character = members[i];
				if (character.faction != null)
				{
					return character.faction;
				}
			}
		}
		return null;
	}

	private BaseSettlement GetPartySettlement()
	{
		Faction faction = partyFaction;
		if (faction != null && faction.isPlayerFaction)
		{
			return PlayerManager.Instance.player.playerSettlement;
		}
		if (partyLeader != null && partyLeader.homeSettlement != null)
		{
			return partyLeader.homeSettlement;
		}
		if (members.Count > 0)
		{
			for (int i = 0; i < members.Count; i++)
			{
				Character character = members[i];
				if (character.homeSettlement != null)
				{
					return character.homeSettlement;
				}
			}
		}
		return null;
	}

	public void UpdatePartySettlement()
	{
		BaseSettlement baseSettlement = GetPartySettlement();
		if (partySettlement != baseSettlement)
		{
			partySettlement?.RemoveParty(this);
			baseSettlement?.AddParty(this);
		}
		partySettlement = baseSettlement;
	}

	public void UpdatePartyFaction()
	{
		Faction faction = GetPartyFaction();
		if (partyFaction != faction)
		{
			string localizedEndQuestReason = PartyQuest.GetLocalizedEndQuestReason("Party_Changed_Faction");
			if (faction == null)
			{
				localizedEndQuestReason = PartyQuest.GetLocalizedEndQuestReason("Party_Disbanded_No_Faction");
			}
			if (partyFaction != null && isActive && partyFaction == currentQuest.postedFaction)
			{
				currentQuest.EndQuest(localizedEndQuestReason);
			}
		}
		partyFaction = faction;
	}

	public void SetPartyState(PARTY_STATE state, bool goHome = false)
	{
		if (partyState != state)
		{
			PARTY_STATE pARTY_STATE = partyState;
			partyState = state;
			OnSwitchFromState(pARTY_STATE);
			OnSwitchToState(state, pARTY_STATE, goHome);
			if (isActive)
			{
				currentQuest.OnAssignedPartySwitchedState(pARTY_STATE, partyState);
			}
		}
	}

	private void OnSwitchFromState(PARTY_STATE prevState)
	{
		switch (prevState)
		{
		case PARTY_STATE.None:
			OnSwitchFromNoneState(prevState);
			break;
		case PARTY_STATE.Waiting:
			OnSwitchFromWaitingState(prevState);
			break;
		case PARTY_STATE.Moving:
			OnSwitchFromMovingState(prevState);
			break;
		case PARTY_STATE.Resting:
			OnSwitchFromRestingState(prevState);
			break;
		case PARTY_STATE.Working:
			OnSwitchFromWorkingState(prevState);
			break;
		}
	}

	private void OnSwitchToState(PARTY_STATE state, PARTY_STATE prevState, bool goHome)
	{
		CancellAllPartyGoToJobsOfMembers();
		ForceCancelAllJobs();
		switch (state)
		{
		case PARTY_STATE.None:
			OnSwitchToNoneState(prevState);
			break;
		case PARTY_STATE.Waiting:
			OnSwitchToWaitingState(prevState);
			break;
		case PARTY_STATE.Moving:
			OnSwitchToMovingState(prevState, goHome);
			break;
		case PARTY_STATE.Resting:
			OnSwitchToRestingState(prevState);
			break;
		case PARTY_STATE.Working:
			OnSwitchToWorkingState(prevState);
			break;
		}
		beaconComponent.UpdateBeaconCharacter();
	}

	private void OnSwitchToNoneState(PARTY_STATE prevState)
	{
	}

	private void OnSwitchFromNoneState(PARTY_STATE prevState)
	{
	}

	private void OnSwitchToWaitingState(PARTY_STATE prevState)
	{
		if (isPlayerParty || currentQuest == null || currentQuest.isDemonicQuest)
		{
			StartWaitTimer();
			return;
		}
		CancelAllTirednessRecoveryJobsOfMembers();
		SetMeetingPlace();
		StartWaitTimer();
	}

	private void OnSwitchFromWaitingState(PARTY_STATE prevState)
	{
	}

	private void StartWaitTimer()
	{
		int amount = 1;
		if (!isPlayerParty && (currentQuest == null || !currentQuest.isDemonicQuest))
		{
			amount = GameManager.Instance.GetTicksBasedOnHour(2);
		}
		waitingEndDate = GameManager.Instance.Today().AddTicks(amount);
		SchedulingManager.Instance.AddEntry(waitingEndDate, WaitingEndedDecisionMaking, this);
	}

	private void PerHourInWaitingState()
	{
		if (partyState == PARTY_STATE.Waiting && isActive && membersThatJoinedQuest.Count >= currentQuest.minimumPartySize)
		{
			WaitingEndedDecisionMaking();
		}
	}

	private void WaitingEndedDecisionMaking()
	{
		if (partyState != PARTY_STATE.Waiting || isDisbanded || !isActive)
		{
			return;
		}
		if (membersThatJoinedQuest.Count >= currentQuest.minimumPartySize || isPlayerParty || currentQuest.isDemonicQuest)
		{
			for (int i = 0; i < membersThatJoinedQuest.Count; i++)
			{
				Character character = membersThatJoinedQuest[i];
				if (!character.traitContainer.HasTrait("Travelling"))
				{
					character.movementComponent.SetEnableDigging(state: true);
					character.traitContainer.AddTrait(character, "Travelling");
				}
			}
			if (currentQuest.waitingToWorkingStateImmediately)
			{
				SetPartyState(PARTY_STATE.Working);
			}
			else
			{
				SetPartyState(PARTY_STATE.Moving);
			}
		}
		else
		{
			currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Not_Enough_Members"));
		}
	}

	public bool CanAMemberGoTo(LocationStructure structure)
	{
		LocationGridTile randomPassableTile = structure.GetRandomPassableTile();
		if (randomPassableTile != null)
		{
			for (int i = 0; i < members.Count; i++)
			{
				if (members[i].movementComponent.HasPathToEvenIfDiffRegion(randomPassableTile))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void OnSwitchToMovingState(PARTY_STATE prevState, bool goHome)
	{
		if (targetDestination == partySettlement)
		{
			goHome = true;
		}
		if (goHome)
		{
			SetTargetDestination(partySettlement);
		}
		else
		{
			SetTargetDestination(currentQuest.GetTargetDestination());
		}
		if (prevState == PARTY_STATE.Waiting)
		{
			CancelAllJobsOfMembersThatJoinedQuest();
		}
	}

	private void OnSwitchFromMovingState(PARTY_STATE prevState)
	{
	}

	private void PerHourInMovingState()
	{
		if (partyState == PARTY_STATE.Moving && !isPlayerParty && HasActiveMemberThatMustDoCriticalNeedsRecovery())
		{
			SetPartyState(PARTY_STATE.Resting);
		}
	}

	private void OnSwitchToRestingState(PARTY_STATE prevState)
	{
		SetStartedTrueRestingState(p_state: false);
		targetRestingTavern = null;
		targetCamp = null;
		FindNearbyTavernOrCamp();
		if (targetRestingTavern == null && targetCamp == null)
		{
			SetPartyState(PARTY_STATE.Moving);
		}
		else if (targetCamp != null)
		{
			_jobComponent.CreateBuildCampfireJob(JOB_TYPE.BUILD_CAMP);
		}
		else if (targetRestingTavern != null)
		{
			SetStartedTrueRestingState(p_state: true);
		}
	}

	private void OnSwitchFromRestingState(PARTY_STATE prevState)
	{
	}

	private void PerHourInRestingState()
	{
		if (partyState == PARTY_STATE.Resting && startedTrueRestingState && !HasActiveMemberThatMustDoNeedsRecovery())
		{
			SetPartyState(PARTY_STATE.Moving);
		}
	}

	private void FindNearbyTavernOrCamp()
	{
		Character character = null;
		for (int i = 0; i < membersThatJoinedQuest.Count; i++)
		{
			Character character2 = membersThatJoinedQuest[i];
			if (character2.gridTileLocation != null && IsMemberActive(character2))
			{
				character = character2;
				break;
			}
		}
		if (character == null)
		{
			return;
		}
		Area area = character.gridTileLocation.area;
		if (area != null && area.HasSettlementLocationType(LOCATION_TYPE.VILLAGE))
		{
			area = null;
		}
		List<Area> list = RuinarchListPool<Area>.Claim(10);
		character.gridTileLocation.area.PopulateAreasInRange(list, 3);
		if (list != null && list.Count > 0)
		{
			for (int j = 0; j < list.Count; j++)
			{
				Area area2 = list[j];
				for (int k = 0; k < area2.settlementsOnArea.Count; k++)
				{
					BaseSettlement baseSettlement = area2.settlementsOnArea[k];
					if (baseSettlement.locationType != LOCATION_TYPE.VILLAGE)
					{
						continue;
					}
					if (baseSettlement == partySettlement && targetDestination == partySettlement)
					{
						return;
					}
					if (baseSettlement.owner == null || baseSettlement.owner == partySettlement.owner || !baseSettlement.owner.IsHostileWith(partySettlement.owner))
					{
						LocationStructure randomStructureOfType = baseSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN);
						if (randomStructureOfType != null)
						{
							targetRestingTavern = randomStructureOfType;
							break;
						}
					}
				}
				if (targetRestingTavern != null)
				{
					break;
				}
				if (area == null && area2.elevationType != ELEVATION.WATER && (!area2.HasSettlementOnArea() || !area2.HasSettlementLocationType(LOCATION_TYPE.VILLAGE)))
				{
					area = area2;
				}
			}
		}
		RuinarchListPool<Area>.Release(list);
		if (targetRestingTavern == null)
		{
			targetCamp = area;
		}
	}

	private void SetStartedTrueRestingState(bool p_state)
	{
		startedTrueRestingState = p_state;
	}

	private void OnSwitchToWorkingState(PARTY_STATE prevState)
	{
		if (prevState == PARTY_STATE.Waiting)
		{
			SetTargetDestination(currentQuest.GetTargetDestination());
			CancelAllJobsOfMembersThatJoinedQuest();
		}
		SetHasChangedTargetDestination(state: false);
	}

	private void OnSwitchFromWorkingState(PARTY_STATE prevState)
	{
	}

	private void AcceptQuest(PartyQuest quest, Character memberThatAcceptedQuest)
	{
		if (isActive || quest == null)
		{
			return;
		}
		SetCurrentQuest(quest);
		currentQuest.SetAssignedParty(this);
		if (isPlayerParty || currentQuest.isDemonicQuest)
		{
			if (currentQuest.workingStateImmediately)
			{
				SetPartyState(PARTY_STATE.Working);
			}
			else
			{
				SetPartyState(PARTY_STATE.Waiting);
			}
		}
		else
		{
			SetPartyState(PARTY_STATE.None);
		}
		if (!partyFaction.isPlayerFaction && memberThatAcceptedQuest != null)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Party", "Party_Table", "accept_quest", LOG_TAG.Party, LOG_TAG.Major);
			log.AddToFillers(memberThatAcceptedQuest, memberThatAcceptedQuest.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(this, partyName, LOG_IDENTIFIER.PARTY_1);
			log.AddToFillers(null, currentQuest.GetPartyQuestName(), LOG_IDENTIFIER.STRING_2);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFrom(memberThatAcceptedQuest, log);
			LogPool.Release(log);
		}
		OnAcceptQuest(quest);
		quest.OnAcceptQuest(this);
		if (!isPlayerParty)
		{
			ScheduleToStartWaitingQuest(members[0]);
			ScheduleToEndQuest(members[0]);
			SetChanceToRetreatUponKnockoutOrDeath(ChanceData.GetChance(CHANCE_TYPE.Party_Quest_First_Knockout));
		}
		Messenger.Broadcast(FactionSignals.PARTY_QUEST_ACCEPTED, this, quest);
	}

	public void DropQuest(Faction partyFaction, string reason)
	{
		if (isActive)
		{
			PartyQuest partyQuest = currentQuest;
			if (partyFaction != null && !partyFaction.isPlayerFaction)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Party", "Party_Table", "drop_quest", LOG_TAG.Party);
				log.AddToFillers(this, partyName, LOG_IDENTIFIER.PARTY_1);
				log.AddToFillers(null, currentQuest.GetPartyQuestName(), LOG_IDENTIFIER.STRING_1);
				log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_2);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			}
			OnDropQuest(currentQuest);
			if (partyQuest.isSuccessful)
			{
				MembersThatJoinedQuestGainsGold(partyQuest);
			}
			ClearMembersThatJoinedQuest(shouldDropQuest: false);
			partyFaction?.partyQuestBoard.RemovePartyQuest(currentQuest);
			SetPartyState(PARTY_STATE.None);
			currentQuest.SetAssignedParty(null);
			SetCurrentQuest(null);
			targetRestingTavern = null;
			meetingPlace = null;
			targetCamp = null;
			targetDestination = null;
			SetHasChangedTargetDestination(state: false);
			hasSetEndQuestDate = false;
			if (partyQuest.isSuccessful)
			{
				Messenger.Broadcast(PartySignals.PARTY_QUEST_FINISHED_SUCCESSFULLY, this);
				onQuestSucceed?.Invoke();
			}
			else
			{
				Messenger.Broadcast(PartySignals.PARTY_QUEST_FAILED, this);
				onQuestFailed?.Invoke();
			}
			OnAfterDropQuest(partyQuest);
			Messenger.Broadcast(FactionSignals.PARTY_QUEST_DROPPED, this, partyQuest);
		}
	}

	private void SetCurrentQuest(PartyQuest p_quest)
	{
		if (currentQuest != p_quest)
		{
			if (currentQuest != null)
			{
				prevQuestType = currentQuest.partyQuestType;
			}
			else
			{
				prevQuestType = PARTY_QUEST_TYPE.None;
			}
			currentQuest = p_quest;
		}
	}

	private void OnAcceptQuest(PartyQuest quest)
	{
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			character.dailyScheduleComponent.OnPartyAcceptedQuest(character, quest);
		}
	}

	private void OnDropQuest(PartyQuest quest)
	{
		ForceCancelAllJobs();
		CancellAllPartyGoToJobsOfMembers();
	}

	private void OnAfterDropQuest(PartyQuest quest)
	{
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			character.dailyScheduleComponent.OnPartyEndQuest(character, quest);
		}
	}

	public bool AddMember(Character character)
	{
		if (!members.Contains(character))
		{
			members.Add(character);
			OnAddMember(character);
			return true;
		}
		return false;
	}

	public bool RemoveMember(Character character)
	{
		if (members.Remove(character))
		{
			OnRemoveMember(character);
			if (members.Count <= 0)
			{
				DisbandParty();
			}
			return true;
		}
		return false;
	}

	public void AddMemberThatJoinedQuest(Character character)
	{
		if (isActive && !membersThatJoinedQuest.Contains(character))
		{
			membersThatJoinedQuest.Add(character);
			OnAddMemberThatJoinedQuest(character);
		}
	}

	public void ClearMembersThatJoinedQuest(bool shouldDropQuest = true)
	{
		while (membersThatJoinedQuest.Count > 0)
		{
			RemoveMemberThatJoinedQuest(membersThatJoinedQuest[0], broadcastSignal: false, shouldDropQuest);
		}
		membersThatJoinedQuest.Clear();
		Messenger.Broadcast(PartySignals.CLEAR_MEMBERS_THAT_JOINED_QUEST, this);
	}

	public void MembersThatJoinedQuestGainsGold(PartyQuest p_quest)
	{
		if (!isPlayerParty)
		{
			int coinRewardPerMember = p_quest.GetCoinRewardPerMember(partyFaction);
			for (int i = 0; i < membersThatJoinedQuest.Count; i++)
			{
				membersThatJoinedQuest[i].moneyComponent.AdjustCoins(coinRewardPerMember);
			}
		}
	}

	public bool RemoveMemberThatJoinedQuest(Character character, bool broadcastSignal = true, bool shouldDropQuest = true, bool shouldGainRewards = false)
	{
		if (membersThatJoinedQuest.Remove(character))
		{
			OnRemoveMemberThatJoinedQuest(character, broadcastSignal);
			if (shouldGainRewards && currentQuest != null && currentQuest.isSuccessful)
			{
				int coinRewardPerMember = currentQuest.GetCoinRewardPerMember(partyFaction);
				character.moneyComponent.AdjustCoins(coinRewardPerMember);
			}
			if ((membersThatJoinedQuest.Count <= 0 || (!HasActiveMemberThatJoinedQuest() && !isPlayerParty)) && shouldDropQuest && isActive)
			{
				currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
			}
			return true;
		}
		return false;
	}

	private void OnAddMemberThatJoinedQuest(Character character)
	{
		UpdatePartyWalkSpeed();
		character.combatComponent.combatBehaviourParent.currentCombatBehaviour?.OnCharacterJoinedPartyQuest(character, currentQuest.partyQuestType);
		character.behaviourComponent.AddBehaviourComponent(currentQuest.relatedBehaviour);
		character.behaviourComponent.SetCanFlirtOnActivePartyQuest(p_state: true);
		Messenger.Broadcast(PartySignals.CHARACTER_JOINED_PARTY_QUEST, this, character);
	}

	private void OnRemoveMemberThatJoinedQuest(Character character, bool broadcastSignal)
	{
		character.movementComponent.UpdateSpeed();
		if (isActive)
		{
			character.combatComponent.combatBehaviourParent.currentCombatBehaviour?.OnCharacterLeftPartyQuest(character, currentQuest.partyQuestType);
		}
		else
		{
			character.combatComponent.combatBehaviourParent.currentCombatBehaviour?.OnCharacterLeftPartyQuest(character, prevQuestType);
		}
		if (character.traitContainer.HasTrait("Travelling"))
		{
			character.movementComponent.SetEnableDigging(state: false);
			character.traitContainer.RemoveTrait(character, "Travelling");
		}
		character.behaviourComponent.RemoveBehaviourComponent(currentQuest.relatedBehaviour);
		beaconComponent.OnRemoveMemberThatJoinedQuest(character);
		if (isActive)
		{
			currentQuest.OnRemoveMemberThatJoinedQuest(character);
		}
		if (broadcastSignal)
		{
			Messenger.Broadcast(PartySignals.CHARACTER_LEFT_PARTY_QUEST, this, character);
		}
	}

	private void OnAddMember(Character character)
	{
		character.partyComponent.SetCurrentParty(this);
		character.behaviourComponent.AddBehaviourComponent(typeof(PartyBehaviour));
		character.dailyScheduleComponent.OnCharacterJoinedParty(character);
		if (partyLeader == null)
		{
			SetPartyLeader(character, processPartyLeader: true);
		}
		UpdatePartyFaction();
		UpdatePartySettlement();
		ProcessKnightBonusesOnJoinParty(character);
		Messenger.Broadcast(PartySignals.CHARACTER_JOINED_PARTY, this, character);
	}

	private void OnRemoveMember(Character character)
	{
		character.partyComponent.SetCurrentParty(null);
		character.behaviourComponent.RemoveBehaviourComponent(typeof(PartyBehaviour));
		character.dailyScheduleComponent.OnCharacterLeftParty(character);
		character.jobQueue.CancelAllPartyJobs();
		if (character.isDead)
		{
			CharacterDies(character);
		}
		RemoveMemberThatJoinedQuest(character);
		if (character == partyLeader)
		{
			SetPartyLeader(null, processPartyLeader: true);
		}
		UpdatePartyFaction();
		UpdatePartySettlement();
		ProcessKnightBonusesOnLeaveParty(character);
		if (PlayerManager.Instance.player.underlingsComponent.persistentDefendParty == this)
		{
			PlayerManager.Instance.player.underlingsComponent.OnCharacterRemovedFromPersistentDefenseParty(character);
		}
		Messenger.Broadcast(PartySignals.CHARACTER_LEFT_PARTY, this, character);
	}

	private void OnRemoveMemberOnDisband(Character character)
	{
		character.partyComponent.SetCurrentParty(null);
		character.behaviourComponent.RemoveBehaviourComponent(typeof(PartyBehaviour));
		character.dailyScheduleComponent.OnCharacterLeftParty(character);
		character.jobQueue.CancelAllPartyJobs();
		RemoveMemberThatJoinedQuest(character);
		Messenger.Broadcast(PartySignals.CHARACTER_LEFT_PARTY_DISBAND, this, character);
	}

	private List<Character> GetActiveMembers()
	{
		_activeMembers.Clear();
		if (isActive)
		{
			for (int i = 0; i < membersThatJoinedQuest.Count; i++)
			{
				Character character = membersThatJoinedQuest[i];
				if (IsMemberActive(character))
				{
					_activeMembers.Add(character);
				}
			}
		}
		return _activeMembers;
	}

	private int GetNumberOfMembersThatJoinedInMeetingPlace()
	{
		int num = 0;
		for (int i = 0; i < membersThatJoinedQuest.Count; i++)
		{
			if (membersThatJoinedQuest[i].currentStructure == meetingPlace)
			{
				num++;
			}
		}
		return num;
	}

	public bool IsMemberActive(Character character)
	{
		if (character.limiterComponent.canMove && character.carryComponent.IsNotBeingCarried() && !character.isBeingSeized)
		{
			bool result = false;
			if (partyState == PARTY_STATE.Waiting)
			{
				if (meetingPlace != null && !meetingPlace.hasBeenDestroyed && meetingPlace.passableTiles.Count > 0)
				{
					if (character.currentStructure == meetingPlace)
					{
						result = true;
					}
					else
					{
						LocationGridTile toTile = meetingPlace.passableTiles[0];
						if (character.movementComponent.HasPathToEvenIfDiffRegion(toTile))
						{
							result = true;
						}
					}
				}
			}
			else if (partyState == PARTY_STATE.Moving || partyState == PARTY_STATE.Working)
			{
				if (targetDestination != null && !targetDestination.hasBeenDestroyed)
				{
					if (targetDestination.IsAtTargetDestination(character))
					{
						result = true;
					}
					else
					{
						LocationGridTile randomPassableTile = targetDestination.GetRandomPassableTile();
						if (character.movementComponent.HasPathToEvenIfDiffRegion(randomPassableTile))
						{
							result = true;
						}
					}
				}
			}
			else if (partyState == PARTY_STATE.Resting)
			{
				if (targetRestingTavern != null && !targetRestingTavern.hasBeenDestroyed && targetRestingTavern.passableTiles.Count > 0)
				{
					if (character.currentStructure == targetRestingTavern)
					{
						result = true;
					}
					else
					{
						LocationGridTile toTile2 = targetRestingTavern.passableTiles[0];
						if (character.movementComponent.HasPathToEvenIfDiffRegion(toTile2))
						{
							result = true;
						}
					}
				}
				else if (targetCamp != null)
				{
					if (character.gridTileLocation != null && character.gridTileLocation.area == targetCamp)
					{
						result = true;
					}
					else
					{
						LocationGridTile centerGridTile = targetCamp.gridTileComponent.centerGridTile;
						if (character.movementComponent.HasPathToEvenIfDiffRegion(centerGridTile))
						{
							result = true;
						}
					}
				}
				else
				{
					LocationGridTile randomPassableTile2 = GetStructureToCheckFromSettlement(partySettlement).GetRandomPassableTile();
					if (character.movementComponent.HasPathToEvenIfDiffRegion(randomPassableTile2))
					{
						result = true;
					}
				}
			}
			return result;
		}
		return false;
	}

	public bool DidMemberJoinQuest(Character member)
	{
		return membersThatJoinedQuest.Contains(member);
	}

	private void CancelAllTirednessRecoveryJobsOfMembers()
	{
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character.currentActionNode != null && character.currentJob != null && character.currentActionNode.action.goapType.IsRestingAction())
			{
				character.currentJob.CancelJob();
			}
			character.jobQueue.CancelAllJobs(JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT);
		}
	}

	private void CancelAllJobsOfMembersThatJoinedQuest()
	{
		for (int i = 0; i < membersThatJoinedQuest.Count; i++)
		{
			membersThatJoinedQuest[i].jobQueue.CancelAllJobs();
		}
	}

	private void CancelAllJobsOfMembersThatJoinedQuestThatAreStillActive()
	{
		for (int i = 0; i < membersThatJoinedQuest.Count; i++)
		{
			Character character = membersThatJoinedQuest[i];
			if (IsMemberActive(character))
			{
				character.jobQueue.CancelAllJobs();
			}
		}
	}

	private void CancellAllPartyGoToJobsOfMembers()
	{
		for (int i = 0; i < membersThatJoinedQuest.Count; i++)
		{
			Character character = membersThatJoinedQuest[i];
			character.jobQueue.CancelAllJobs(JOB_TYPE.PARTY_GO_TO, JOB_TYPE.GO_TO_WAITING);
			character.trapStructure.ResetAllTrapStructures();
			character.trapStructure.ResetTrapArea();
		}
	}

	private bool HasActiveMemberThatMustDoNeedsRecovery()
	{
		for (int i = 0; i < membersThatJoinedQuest.Count; i++)
		{
			Character character = membersThatJoinedQuest[i];
			if (IsMemberActive(character) && (((character.needsComponent.isStarving || character.needsComponent.isHungry) && character.limiterComponent.canDoFullnessRecovery) || ((character.needsComponent.isExhausted || character.needsComponent.isTired) && character.limiterComponent.canDoTirednessRecovery) || ((character.needsComponent.isSulking || character.needsComponent.isBored) && character.limiterComponent.canDoHappinessRecovery)))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasActiveMemberThatMustDoCriticalNeedsRecovery()
	{
		for (int i = 0; i < membersThatJoinedQuest.Count; i++)
		{
			Character character = membersThatJoinedQuest[i];
			if (IsMemberActive(character) && ((character.needsComponent.isStarving && character.limiterComponent.canDoFullnessRecovery) || (character.needsComponent.isExhausted && character.limiterComponent.canDoTirednessRecovery) || (character.needsComponent.isSulking && character.limiterComponent.canDoHappinessRecovery)))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasActiveMemberThatJoinedQuest()
	{
		for (int i = 0; i < membersThatJoinedQuest.Count; i++)
		{
			Character character = membersThatJoinedQuest[i];
			if (IsMemberActive(character))
			{
				return true;
			}
		}
		return false;
	}

	public Character GetMemberInCombatExcept(Character character)
	{
		for (int i = 0; i < membersThatJoinedQuest.Count; i++)
		{
			Character character2 = membersThatJoinedQuest[i];
			if (character2 != character && character2.combatComponent.isInCombat)
			{
				return character2;
			}
		}
		return null;
	}

	public bool IsMember(Character character)
	{
		return members.Contains(character);
	}

	private void CharacterDies(Character character)
	{
		if (membersThatJoinedQuest.Contains(character) && !deadMembers.Contains(character))
		{
			deadMembers.Add(character);
		}
		if (currentQuest != null)
		{
			currentQuest.OnCharacterDeath(character);
		}
	}

	public bool HasMemberThatJoinedQuestThatIsInRangeOfCharacterThatConsidersCrimeTypeACrime(Character character, CRIME_TYPE crimeType)
	{
		for (int i = 0; i < membersThatJoinedQuest.Count; i++)
		{
			Character character2 = membersThatJoinedQuest[i];
			if (character != character2 && character2.limiterComponent.canWitness && (bool)character.marker && character.marker.IsPOIInVision(character2) && CrimeManager.Instance.GetCrimeSeverity(character2, character, character, crimeType).IsConsideredACrime())
			{
				return true;
			}
		}
		return false;
	}

	private Character GetRandomMemberThatCanAcceptQuests()
	{
		if (members.Count > 0)
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			for (int i = 0; i < members.Count; i++)
			{
				Character character = members[i];
				if (CanAcceptQuests(character))
				{
					list.Add(character);
				}
			}
			Character randomElement = CollectionUtilities.GetRandomElement(list);
			RuinarchListPool<Character>.Release(list);
			return randomElement;
		}
		return null;
	}

	private void DisbandParty()
	{
		if (isDisbanded || doNotDisband)
		{
			return;
		}
		if (members.Count > 0)
		{
			for (int i = 0; i < members.Count; i++)
			{
				OnRemoveMemberOnDisband(members[i]);
			}
			members.Clear();
		}
		OnDisbandParty();
	}

	private void OnDisbandParty()
	{
		isDisbanded = true;
		if (isActive)
		{
			currentQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Party_Disbanded"));
		}
		Messenger.Broadcast(PartySignals.DISBAND_PARTY, this);
		DestroyParty();
	}

	public void AllMembersThatJoinedQuestGainsRandomCoinAmount(int p_minAmount, int p_maxAmount)
	{
		if (!isPlayerParty)
		{
			for (int i = 0; i < membersThatJoinedQuest.Count; i++)
			{
				membersThatJoinedQuest[i].moneyComponent.AdjustCoins(GameUtilities.RandomBetweenTwoNumbers(p_minAmount, p_maxAmount));
			}
		}
	}

	private void UpdatePartyWalkSpeed()
	{
		if (!isPlayerParty || membersThatJoinedQuest.Count <= 0)
		{
			return;
		}
		partyWalkSpeed = float.MaxValue;
		bool flag = false;
		for (int i = 0; i < membersThatJoinedQuest.Count; i++)
		{
			Character character = membersThatJoinedQuest[i];
			if (character.movementComponent.walkSpeed > 0f && character.movementComponent.walkSpeed < partyWalkSpeed)
			{
				flag = true;
				partyWalkSpeed = character.movementComponent.walkSpeed;
			}
		}
		if (!flag)
		{
			partyWalkSpeed = 0f;
		}
		for (int j = 0; j < membersThatJoinedQuest.Count; j++)
		{
			membersThatJoinedQuest[j].movementComponent.UpdateSpeed();
		}
	}

	public void LoadReferences(SaveDataParty data)
	{
		if (!string.IsNullOrEmpty(data.partyLeader))
		{
			partyLeader = CharacterManager.Instance.GetCharacterByPersistentID(data.partyLeader);
		}
		jobBoard.LoadReferences(data.jobBoard);
		if (data.forcedCancelJobsOnTickEnded != null)
		{
			for (int i = 0; i < data.forcedCancelJobsOnTickEnded.Count; i++)
			{
				forcedCancelJobsOnTickEnded.Add(DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(data.forcedCancelJobsOnTickEnded[i]));
			}
		}
		if (!string.IsNullOrEmpty(data.meetingPlace))
		{
			meetingPlace = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.meetingPlace);
		}
		if (!string.IsNullOrEmpty(data.targetRestingTavern))
		{
			targetRestingTavern = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.targetRestingTavern);
		}
		if (!string.IsNullOrEmpty(data.targetCamp))
		{
			targetCamp = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.targetCamp);
		}
		if (!string.IsNullOrEmpty(data.targetDestination))
		{
			if (data.targetDestinationType == PARTY_TARGET_DESTINATION_TYPE.Area)
			{
				targetDestination = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.targetDestination);
			}
			else if (data.targetDestinationType == PARTY_TARGET_DESTINATION_TYPE.Structure)
			{
				targetDestination = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.targetDestination);
			}
			else if (data.targetDestinationType == PARTY_TARGET_DESTINATION_TYPE.Settlement)
			{
				targetDestination = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(data.targetDestination);
			}
		}
		if (data.members != null)
		{
			members = SaveUtilities.ConvertIDListToCharacters(data.members);
		}
		if (data.membersThatJoinedQuest != null)
		{
			membersThatJoinedQuest = SaveUtilities.ConvertIDListToCharacters(data.membersThatJoinedQuest);
		}
		if (data.deadmembers != null)
		{
			deadMembers = SaveUtilities.ConvertIDListToCharacters(data.deadmembers);
		}
		if (!string.IsNullOrEmpty(data.partySettlement))
		{
			partySettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.partySettlement);
		}
		if (!string.IsNullOrEmpty(data.partyFaction))
		{
			partyFaction = FactionManager.Instance.GetFactionByPersistentID(data.partyFaction);
		}
		beaconComponent.LoadReferences(data.beaconComponent);
		banningComponent.LoadReferences(data.banningComponent);
		UpdatePartyWalkSpeed();
		if (!string.IsNullOrEmpty(data.currentQuest))
		{
			currentQuest = DatabaseManager.Instance.partyQuestDatabase.GetPartyQuestByPersistentID(data.currentQuest);
		}
	}

	private void JobRemovedFromJobBoard(JobQueueItem job)
	{
		if (job.jobType == JOB_TYPE.BUILD_CAMP)
		{
			SetStartedTrueRestingState(p_state: true);
		}
	}

	public void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character)
	{
	}

	public void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character, bool shouldBlacklist = false)
	{
		if (!job.IsJobStillApplicable() || job.shouldBeRemovedFromSettlementWhenUnassigned)
		{
			jobBoard.RemoveFromAvailableJobs(job);
		}
		else if (shouldBlacklist && character != null)
		{
			job.AddBlacklistedCharacter(character);
		}
	}

	public bool ForceCancelJob(JobQueueItem job)
	{
		return jobBoard.RemoveFromAvailableJobs(job);
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

	private void ForceCancelAllJobsTargetingCharacter(IPointOfInterest target, string reason)
	{
		for (int i = 0; i < jobBoard.availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = jobBoard.availableJobs[i];
			if (jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (goapPlanJob.targetPOI == target)
				{
					AddForcedCancelJobsOnTickEnded(goapPlanJob);
				}
			}
		}
	}

	private void ForceCancelJobTypesTargetingPOI(IPointOfInterest target, string reason, JOB_TYPE jobType)
	{
		for (int i = 0; i < jobBoard.availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = jobBoard.availableJobs[i];
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

	public void ForceCancelJobTypesTargetingPOI(JOB_TYPE jobType, IPointOfInterest target)
	{
		for (int i = 0; i < jobBoard.availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = jobBoard.availableJobs[i];
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

	public void ForceCancelAllJobs()
	{
		for (int i = 0; i < jobBoard.availableJobs.Count; i++)
		{
			AddForcedCancelJobsOnTickEnded(jobBoard.availableJobs[i]);
		}
	}

	public void ForceCancelAllJobsImmediately()
	{
		for (int i = 0; i < jobBoard.availableJobs.Count; i++)
		{
			if (jobBoard.availableJobs[i].ForceCancelJob())
			{
				i--;
			}
		}
		for (int j = 0; j < forcedCancelJobsOnTickEnded.Count; j++)
		{
			forcedCancelJobsOnTickEnded[j].ForceCancelJob();
		}
		forcedCancelJobsOnTickEnded.Clear();
	}

	public void OnSelectBookmark()
	{
		CenterOnParty();
		UIManager.Instance.ShowPartyInfo(this);
	}

	public void RemoveBookmark()
	{
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this);
	}

	public void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		string info = string.Empty;
		if (currentQuest is DemonSnatchPartyQuest demonSnatchPartyQuest)
		{
			info = partyName + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Assigned_Snatch") + " " + demonSnatchPartyQuest.targetCharacter?.bookmarkName;
		}
		else if (currentQuest is DemonRaidPartyQuest demonRaidPartyQuest)
		{
			info = partyName + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Assigned_Harass") + " " + demonRaidPartyQuest.targetSettlement?.bookmarkName;
		}
		else if (currentQuest is KillVillagerPartyQuest killVillagerPartyQuest)
		{
			info = partyName + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Assigned_Kill") + " " + killVillagerPartyQuest.targetCharacter?.bookmarkName;
		}
		else if (currentQuest is DemonDefendPartyQuest)
		{
			info = partyName + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Assigned_Defend");
		}
		else if (currentQuest is DemonStealPartyQuest)
		{
			info = partyName + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Assigned_Steal");
		}
		UIManager.Instance.ShowSmallInfo(info, p_pos, "", autoReplaceText: false);
	}

	public void OnHoverOutBookmarkItem()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void CenterOnParty()
	{
		if (beaconComponent.currentBeaconCharacter != null)
		{
			beaconComponent.currentBeaconCharacter.CenterOnCharacter();
		}
		else if (activeMembers.Count > 0)
		{
			activeMembers[0].CenterOnCharacter();
		}
		else if (members.Count > 0)
		{
			members[0].CenterOnCharacter();
		}
	}

	public void SetChanceToRetreatUponKnockoutOrDeath(int p_chance)
	{
		chanceToRetreatUponKnockoutOrDeath = p_chance;
	}

	public void SetPlannedPartyQuestType(PARTY_QUEST_TYPE p_plannedPartyQuestType)
	{
		plannedPartyQuestType = p_plannedPartyQuestType;
	}

	public void SetPartyLeader(Character p_character, bool processPartyLeader = false)
	{
		if (partyLeader != p_character)
		{
			partyLeader = p_character;
			if (processPartyLeader && partyLeader == null && members.Count > 0)
			{
				partyLeader = members[0];
			}
		}
	}

	public int GetTotalOpinionOfCharacterTowardsParty(Character p_character)
	{
		int num = 0;
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character != p_character)
			{
				int num2 = p_character.relationshipContainer.GetTotalOpinion(character);
				if (character == partyLeader)
				{
					num2 *= 3;
				}
				num += num2;
			}
		}
		return num;
	}

	public int GetTotalOpinionOfPartyMembersTowardsCharacterIgnoreNewcomer(Character p_character)
	{
		int num = 0;
		Newcomer traitOrStatus = p_character.traitContainer.GetTraitOrStatus<Newcomer>("Newcomer");
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character != p_character)
			{
				int num2 = character.relationshipContainer.GetTotalOpinion(p_character);
				if (traitOrStatus != null && !character.traitContainer.HasTrait("Newcomer"))
				{
					num2 += Mathf.Abs(traitOrStatus.opinionModifier.modifierValue);
				}
				if (character == partyLeader)
				{
					num2 *= 3;
				}
				num += num2;
			}
		}
		return num;
	}

	private void ProcessKnightBonusesOnJoinParty(Character p_characterThatJoined)
	{
		if (p_characterThatJoined.characterClass.className == "Knight")
		{
			UpdateKnightBonusesToParty();
		}
		else
		{
			UpdateKnightBonusToAPartyMember(p_characterThatJoined);
		}
	}

	private void ProcessKnightBonusesOnLeaveParty(Character p_characterThatLeft)
	{
		if (p_characterThatLeft.characterClass.className == "Knight")
		{
			UpdateKnightBonusesToParty();
			if (!p_characterThatLeft.isDead)
			{
				p_characterThatLeft.traitComponent.ApplyKnightBonuses();
			}
		}
		else
		{
			p_characterThatLeft.traitComponent.RemoveKnightBonuses();
		}
	}

	public void UpdateKnightBonusesToParty()
	{
		int highestLevelMartialArtsOfAKnight = GetHighestLevelMartialArtsOfAKnight();
		if (highestLevelMartialArtsOfAKnight != 0)
		{
			for (int i = 0; i < members.Count; i++)
			{
				Character p_partyMember = members[i];
				UpdateKnightBonusToAPartyMember(p_partyMember, highestLevelMartialArtsOfAKnight);
			}
		}
		else
		{
			for (int j = 0; j < members.Count; j++)
			{
				members[j].traitComponent.RemoveKnightBonuses();
			}
		}
	}

	private void UpdateKnightBonusToAPartyMember(Character p_partyMember)
	{
		p_partyMember.traitComponent.RemoveKnightBonuses();
		int highestLevelMartialArtsOfAKnight = GetHighestLevelMartialArtsOfAKnight();
		if (highestLevelMartialArtsOfAKnight != 0)
		{
			UpdateKnightBonusToAPartyMember(p_partyMember, highestLevelMartialArtsOfAKnight);
		}
	}

	private void UpdateKnightBonusToAPartyMember(Character p_partyMember, int p_level)
	{
		p_partyMember.traitComponent.ApplyKnightBonuses(p_level);
	}

	private int GetHighestLevelMartialArtsOfAKnight()
	{
		int num = 0;
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character.characterClass.className == "Knight")
			{
				int num2 = character.TryGetTalentLevel(CHARACTER_TALENT.Martial_Arts);
				if (num == 0 || num2 > num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	private void AddKnightBonusToPartyExceptKnight(Character p_knight, int p_talentLevel)
	{
		switch (p_talentLevel)
		{
		case 1:
			AddLevel1KnightBonusToParty(p_knight);
			break;
		case 2:
			AddLevel2KnightBonusToParty(p_knight);
			break;
		case 3:
			AddLevel3KnightBonusToParty(p_knight);
			break;
		case 4:
			AddLevel4KnightBonusToParty(p_knight);
			break;
		case 5:
			AddLevel5KnightBonusToParty(p_knight);
			break;
		}
	}

	private void RemoveKnightBonusToPartyExceptKnight(Character p_knight, int p_talentLevel)
	{
		switch (p_talentLevel)
		{
		case 1:
			RemoveLevel1KnightBonusToParty(p_knight);
			break;
		case 2:
			RemoveLevel2KnightBonusToParty(p_knight);
			break;
		case 3:
			RemoveLevel3KnightBonusToParty(p_knight);
			break;
		case 4:
			RemoveLevel4KnightBonusToParty(p_knight);
			break;
		case 5:
			RemoveLevel5KnightBonusToParty(p_knight);
			break;
		}
	}

	private void AddLevel1KnightBonusToParty(Character p_knight)
	{
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character != p_knight)
			{
				character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, 20f);
			}
		}
	}

	private void RemoveLevel1KnightBonusToParty(Character p_knight)
	{
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character != p_knight)
			{
				character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, -20f);
			}
		}
	}

	private void AddLevel2KnightBonusToParty(Character p_knight)
	{
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character != p_knight)
			{
				character.combatComponent.AdjustHPRecoveryPerTickOutsideCombat(3);
			}
		}
	}

	private void RemoveLevel2KnightBonusToParty(Character p_knight)
	{
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character != p_knight)
			{
				character.combatComponent.AdjustHPRecoveryPerTickOutsideCombat(-3);
			}
		}
	}

	private void AddLevel3KnightBonusToParty(Character p_knight)
	{
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character != p_knight)
			{
				character.traitContainer.AddTrait(character, "Knight Mood Bonus");
			}
		}
	}

	private void RemoveLevel3KnightBonusToParty(Character p_knight)
	{
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character != p_knight)
			{
				character.traitContainer.RemoveTrait(character, "Knight Mood Bonus");
			}
		}
	}

	private void AddLevel4KnightBonusToParty(Character p_knight)
	{
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character != p_knight)
			{
				character.traitContainer.AddTrait(character, "Hermit");
			}
		}
	}

	private void RemoveLevel4KnightBonusToParty(Character p_knight)
	{
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character != p_knight)
			{
				character.traitContainer.RemoveTrait(character, "Hermit");
			}
		}
	}

	private void AddLevel5KnightBonusToParty(Character p_knight)
	{
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character != p_knight)
			{
				character.needsComponent.AdjustFullnessDecreaseRate(-0.05f);
			}
		}
	}

	private void RemoveLevel5KnightBonusToParty(Character p_knight)
	{
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character != p_knight)
			{
				character.needsComponent.AdjustFullnessDecreaseRate(0.05f);
			}
		}
	}

	private string GetUIString()
	{
		if (string.IsNullOrEmpty(_uiString))
		{
			string text = GetType().ToString() + "|" + persistentID;
			_uiString = "<link=" + text + ">" + name + "</link>";
		}
		return _uiString;
	}

	private void DestroyParty()
	{
		beaconComponent.OnDestroyParty();
		ObjectPoolManager.Instance.ReturnPartyToPool(this);
	}

	public void Reset()
	{
		_uiString = string.Empty;
		partySettlement?.RemoveParty(this);
		partyName = string.Empty;
		partyState = PARTY_STATE.None;
		partySettlement = null;
		partyFaction = null;
		targetRestingTavern = null;
		targetCamp = null;
		targetDestination = null;
		currentQuest = null;
		prevQuestType = PARTY_QUEST_TYPE.None;
		meetingPlace = null;
		partyLeader = null;
		hasChangedTargetDestination = false;
		bookmarkEventDispatcher.ClearAll();
		damageAccumulator?.Reset();
		members.Clear();
		deadMembers.Clear();
		onQuestFailed = null;
		onQuestSucceed = null;
		hasSetNextSwitchToWaitingStateTrigger = false;
		hasSetEndQuestDate = false;
		doNotDisband = false;
		chanceToRetreatUponKnockoutOrDeath = ChanceData.GetChance(CHANCE_TYPE.Party_Quest_First_Knockout);
		ClearMembersThatJoinedQuest(shouldDropQuest: false);
		_activeMembers.Clear();
		ForceCancelAllJobsImmediately();
		jobBoard?.Reset();
		banningComponent?.Reset();
		Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
		Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
		Messenger.RemoveListener<JobQueueItem, JobBoard>(JobSignals.JOB_REMOVED_FROM_JOB_BOARD, OnJobRemovedFromJobBoard);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		DatabaseManager.Instance.partyDatabase.RemoveParty(this);
		Messenger.Broadcast(PartySignals.PARTY_DESTROYED, this);
	}

	public void Subscribe(PartyEventsIListener p_iListener)
	{
		onQuestFailed = (Action)Delegate.Combine(onQuestFailed, new Action(p_iListener.OnQuestFailed));
		onQuestSucceed = (Action)Delegate.Combine(onQuestSucceed, new Action(p_iListener.OnQuestSucceed));
	}

	public void Unsubscribe(PartyEventsIListener p_iListener)
	{
		onQuestFailed = (Action)Delegate.Remove(onQuestFailed, new Action(p_iListener.OnQuestFailed));
		onQuestSucceed = (Action)Delegate.Remove(onQuestSucceed, new Action(p_iListener.OnQuestSucceed));
	}

	public string GetTestingInfo()
	{
		return string.Concat(name + " info:", "\nNext Quest Check Date: ", nextQuestCheckDate.ConvertToContinuousDaysWithTime());
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = meetingPlace;
		_ = targetRestingTavern;
		_ = targetDestination;
		beaconComponent?.CheckIfStructureIsStillReferenced(p_structure);
		damageAccumulator?.CheckIfStructureIsStillReferenced(p_structure);
		banningComponent?.CheckIfStructureIsStillReferenced(p_structure);
		_jobComponent?.CheckIfStructureIsStillReferenced(p_structure);
		currentQuest?.CheckIfStructureIsStillReferenced(p_structure);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = partyLeader;
		members.Contains(p_character);
		membersThatJoinedQuest.Contains(p_character);
		deadMembers.Contains(p_character);
		_activeMembers.Contains(p_character);
		currentQuest?.CheckIfCharacterIsStillReferenced(p_character);
		beaconComponent?.CheckIfCharacterIsStillReferenced(p_character);
		damageAccumulator?.CheckIfCharacterIsStillReferenced(p_character);
		banningComponent?.CheckIfCharacterIsStillReferenced(p_character);
		_jobComponent?.CheckIfCharacterIsStillReferenced(p_character);
	}
}
