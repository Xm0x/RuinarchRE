using System;
using System.Collections.Generic;
using Goap.Job_Checkers;
using Inner_Maps.Location_Structures;
using Object_Pools;
using UtilityScripts;

public abstract class JobQueueItem : ISavable
{
	protected int _priority;

	public string persistentID { get; private set; }

	public abstract OBJECT_TYPE objectType { get; }

	public abstract Type serializedData { get; }

	public int id { get; protected set; }

	public IJobOwner originalOwner { get; protected set; }

	public Character assignedCharacter { get; protected set; }

	public string name { get; private set; }

	public JOB_TYPE jobType { get; protected set; }

	public bool finishedSuccessfully { get; protected set; }

	public List<Character> blacklistedCharacters { get; private set; }

	public CanTakeJobChecker canTakeJobChecker { get; private set; }

	public JobApplicabilityChecker stillApplicable { get; protected set; }

	public bool doNotRecalculate { get; protected set; }

	public int invalidCounter { get; protected set; }

	public bool isThisAPartyJob { get; protected set; }

	public bool isThisAGatheringJob { get; protected set; }

	public bool cannotBePushedBack { get; protected set; }

	public bool shouldBeRemovedFromSettlementWhenUnassigned { get; protected set; }

	public bool forceCancelOnInvalid { get; protected set; }

	public bool isInMultithread { get; protected set; }

	public bool shouldForceCancelUponReceiving { get; protected set; }

	public bool isTriggeredFlaw { get; set; }

	public int numOfTimesPushedBack { get; protected set; }

	public FACTION_IDEOLOGY connectedFactionIdeology { get; protected set; }

	public bool hasBeenReset { get; protected set; }

	public virtual IPointOfInterest poiTarget => null;

	public int priority => GetPriority();

	public JobQueueItem()
	{
		id = -1;
		blacklistedCharacters = new List<Character>();
	}

	protected void Initialize(JOB_TYPE jobType, IJobOwner owner)
	{
		persistentID = Utilities.GetNewUniqueID();
		id = Utilities.SetID(this);
		hasBeenReset = false;
		this.jobType = jobType;
		originalOwner = owner;
		if (originalOwner == null)
		{
			throw new Exception("Original owner of job " + ToString() + " is null");
		}
		name = LocalizationManager.Instance.GetLocalizedValue("Jobs_Table", jobType.ToStringEnum());
		SetInitialPriority();
		Messenger.AddListener<JOB_TYPE, IPointOfInterest>(JobSignals.CHECK_JOB_APPLICABILITY, CheckJobApplicability);
		Messenger.AddListener<IPointOfInterest>(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, CheckJobApplicability);
		Messenger.AddListener<JOB_TYPE>(JobSignals.CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE, CheckJobApplicability);
		Messenger.AddListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, DisconnectFromSettlement);
		Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		Messenger.AddListener<CrimeData>(CharacterSignals.CRIME_REMOVED_FROM_DATABASE, OnCrimeRemovedFromDatabase);
		DatabaseManager.Instance.jobDatabase.Register(this);
	}

	protected void Initialize(SaveDataJobQueueItem data)
	{
		persistentID = data.persistentID;
		id = Utilities.SetID(this, data.id);
		hasBeenReset = false;
		name = data.name;
		jobType = data.jobType;
		SetDoNotRecalculate(data.doNotRecalculate);
		invalidCounter = data.invalidCounter;
		SetIsThisAPartyJob(data.isThisAPartyJob);
		SetIsThisAGatheringJob(data.isThisAGatheringJob);
		SetCannotBePushedBack(data.cannotBePushedBack);
		SetShouldBeRemovedFromSettlementWhenUnassigned(data.shouldBeRemovedFromSettlementWhenUnassigned);
		SetForceCancelOnInvalid(data.forceCancelOnInvalid);
		SetConnectedFactionIdeology(data.connectedFactionIdeology);
		if (!string.IsNullOrEmpty(data.canTakeJobKey))
		{
			SetCanTakeThisJobChecker(data.canTakeJobKey);
		}
		if (!string.IsNullOrEmpty(data.applicabilityCheckerKey))
		{
			SetStillApplicableChecker(data.applicabilityCheckerKey);
		}
		SetInitialPriority();
		DatabaseManager.Instance.jobDatabase.Register(this);
	}

	public virtual bool LoadSecondWave(SaveDataJobQueueItem data)
	{
		bool result = true;
		if (!string.IsNullOrEmpty(data.originalOwnerID))
		{
			if (data.originalOwnerType == OBJECT_TYPE.Settlement)
			{
				originalOwner = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.originalOwnerID) as NPCSettlement;
			}
			else if (data.originalOwnerType == OBJECT_TYPE.Character)
			{
				originalOwner = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.originalOwnerID);
			}
			else if (data.originalOwnerType == OBJECT_TYPE.Faction)
			{
				originalOwner = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(data.originalOwnerID);
			}
			else if (data.originalOwnerType == OBJECT_TYPE.Party)
			{
				originalOwner = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(data.originalOwnerID);
			}
			if (originalOwner == null)
			{
				result = false;
			}
		}
		if (!string.IsNullOrEmpty(data.assignedCharacterID))
		{
			assignedCharacter = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.assignedCharacterID);
			if (assignedCharacter == null)
			{
				result = false;
			}
		}
		for (int i = 0; i < data.blacklistedCharacterIDs.Count; i++)
		{
			Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.blacklistedCharacterIDs[i]);
			if (characterByPersistentID != null)
			{
				blacklistedCharacters.Add(characterByPersistentID);
			}
		}
		Messenger.AddListener<JOB_TYPE, IPointOfInterest>(JobSignals.CHECK_JOB_APPLICABILITY, CheckJobApplicability, shouldLock: true);
		Messenger.AddListener<IPointOfInterest>(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, CheckJobApplicability, shouldLock: true);
		Messenger.AddListener<JOB_TYPE>(JobSignals.CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE, CheckJobApplicability, shouldLock: true);
		Messenger.AddListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, DisconnectFromSettlement, shouldLock: true);
		Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure, shouldLock: true);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter, shouldLock: true);
		return result;
	}

	protected virtual bool CanTakeJob(Character character)
	{
		bool flag = false;
		if (character.traitContainer.HasTrait("Criminal") && character.faction != null && character.crimeComponent.activeCrimes.Count > 0)
		{
			for (int i = 0; i < character.crimeComponent.activeCrimes.Count; i++)
			{
				if (character.crimeComponent.activeCrimes[i].IsWantedBy(character.faction))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return character.limiterComponent.canPerform;
		}
		return false;
	}

	public virtual void UnassignJob(string reason)
	{
	}

	public virtual void OnAddJobToQueue()
	{
	}

	public virtual bool OnRemoveJobFromQueue()
	{
		return true;
	}

	public virtual void AddOtherData(INTERACTION_TYPE actionType, object[] data)
	{
	}

	public virtual void AddOtherData(INTERACTION_TYPE actionType, OtherData[] data)
	{
	}

	public virtual bool CanCharacterTakeThisJob(Character character)
	{
		if (originalOwner.ownerType == JOB_OWNER.CHARACTER)
		{
			return CanTakeJob(character);
		}
		if ((originalOwner.ownerType == JOB_OWNER.SETTLEMENT || originalOwner.ownerType == JOB_OWNER.FACTION) && !character.jobComponent.CanDoJob(jobType))
		{
			return false;
		}
		if (canTakeJobChecker != null)
		{
			if (canTakeJobChecker.CanTakeJob(character, this))
			{
				return CanTakeJob(character);
			}
			return false;
		}
		return CanTakeJob(character);
	}

	public virtual void OnCharacterAssignedToJob(Character character)
	{
	}

	public virtual void OnCharacterUnassignedToJob(Character character)
	{
	}

	public virtual bool ProcessJob()
	{
		return false;
	}

	public virtual bool CancelJob(string reason = "", bool shouldBlacklist = false)
	{
		if (assignedCharacter == null)
		{
			return false;
		}
		return assignedCharacter.jobQueue.RemoveJobInQueue(this, reason, shouldBlacklist);
	}

	public virtual bool ForceCancelJob(string reason = "", bool shouldBlacklist = false)
	{
		if (assignedCharacter != null)
		{
			JOB_OWNER ownerType = originalOwner.ownerType;
			bool result = assignedCharacter.jobQueue.RemoveJobInQueue(this, reason, shouldBlacklist);
			if (ownerType == JOB_OWNER.CHARACTER)
			{
				return result;
			}
		}
		return originalOwner.ForceCancelJob(this);
	}

	public virtual void PushedBack(JobQueueItem jobThatPushedBack, bool shouldIncreasePushBackCount = true)
	{
		if (!cannotBePushedBack || jobThatPushedBack.jobType == JOB_TYPE.DIG_THROUGH)
		{
			string reason = string.Empty;
			if (jobThatPushedBack.jobType != JOB_TYPE.DIG_THROUGH)
			{
				bool flag = shouldIncreasePushBackCount;
				reason = "Important_To_Do";
				if (jobThatPushedBack is CharacterStateJob { targetState: CHARACTER_STATE.COMBAT })
				{
					Character character = assignedCharacter;
					if (character != null)
					{
						if (flag && poiTarget != null && character.combatComponent.GetCombatData(poiTarget)?.connectedAction?.associatedJob == this)
						{
							flag = false;
						}
						if (character.combatComponent.avoidInRange.Count > 0)
						{
							reason = "Scared_Of_Something";
						}
					}
				}
				if (flag)
				{
					numOfTimesPushedBack++;
					if (numOfTimesPushedBack >= 3)
					{
						SetCannotBePushedBack(state: true);
					}
				}
			}
			if (assignedCharacter != null)
			{
				Character character2 = assignedCharacter;
				character2.StopCurrentActionNode(reason);
				if (character2.carryComponent.carriedPOI is Character poi)
				{
					character2.UncarryPOI(poi);
				}
			}
		}
		else
		{
			CancelJob("", numOfTimesPushedBack >= 3);
		}
	}

	public virtual void StopJobNotDrop()
	{
		if (cannotBePushedBack)
		{
			CancelJob();
		}
		else
		{
			assignedCharacter?.StopCurrentActionNode();
		}
	}

	public virtual bool CanBeInterruptedBy(JOB_TYPE jobType)
	{
		return true;
	}

	protected virtual void CheckJobApplicability(JOB_TYPE p_jobType, IPointOfInterest p_targetPOI)
	{
	}

	protected virtual void CheckJobApplicability(JOB_TYPE p_jobType)
	{
	}

	protected virtual void CheckJobApplicability(IPointOfInterest p_targetPOI)
	{
	}

	protected virtual void DisconnectFromSettlement(NPCSettlement p_settlement)
	{
		if (originalOwner == p_settlement)
		{
			if (jobType == JOB_TYPE.BUILD_BLUEPRINT && poiTarget is GenericTileObject genericTileObject && genericTileObject.blueprintOnTile != null)
			{
				genericTileObject.ForceExpireBlueprint();
			}
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Jobs", "CancelReasons_Table", "Location_Destroyed", LOG_TAG.Life_Changes);
			log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
			string logText = log.logText;
			LogPool.Release(log);
			ForceCancelJob(logText);
		}
	}

	protected virtual void DisconnectFromStructure(LocationStructure p_structure)
	{
	}

	protected virtual void DisconnectFromCharacter(Character p_character)
	{
		blacklistedCharacters.Remove(p_character);
	}

	protected virtual void OnCrimeRemovedFromDatabase(CrimeData p_crime)
	{
	}

	public void SetAssignedCharacter(Character character)
	{
		Character character2 = null;
		if (assignedCharacter != null)
		{
			character2 = assignedCharacter;
		}
		assignedCharacter = character;
		if (assignedCharacter != null)
		{
			OnCharacterAssignedToJob(assignedCharacter);
		}
		else if (assignedCharacter == null && character2 != null)
		{
			OnCharacterUnassignedToJob(character2);
		}
	}

	private void SetCanTakeThisJobChecker(CanTakeJobChecker canTakeJobChecker)
	{
		this.canTakeJobChecker = canTakeJobChecker;
	}

	public void SetCanTakeThisJobChecker(string canTakeJobCheckerKey)
	{
		SetCanTakeThisJobChecker(JobManager.Instance.GetJobChecker(canTakeJobCheckerKey));
	}

	public void SetStillApplicableChecker(string applicabilityKey)
	{
		SetStillApplicableChecker(JobManager.Instance.GetApplicabilityChecker(applicabilityKey));
	}

	public void SetStillApplicableChecker(JobApplicabilityChecker jobApplicabilityChecker)
	{
		stillApplicable = jobApplicabilityChecker;
	}

	public void SetCannotBePushedBack(bool state)
	{
		cannotBePushedBack = state;
	}

	public void SetShouldBeRemovedFromSettlementWhenUnassigned(bool state)
	{
		shouldBeRemovedFromSettlementWhenUnassigned = state;
	}

	public void AddBlacklistedCharacter(Character character)
	{
		if (!blacklistedCharacters.Contains(character))
		{
			blacklistedCharacters.Add(character);
		}
	}

	public void RemoveBlacklistedCharacter(Character character)
	{
		blacklistedCharacters.Remove(character);
	}

	public void ClearBlacklist()
	{
		blacklistedCharacters.Clear();
	}

	public void SetFinishedSuccessfully(bool state)
	{
		finishedSuccessfully = state;
	}

	public void SetForceCancelOnInvalid(bool state)
	{
		forceCancelOnInvalid = state;
	}

	public int GetPriority()
	{
		return _priority;
	}

	public void SetPriority(int amount)
	{
		_priority = amount;
	}

	private void SetInitialPriority()
	{
		int jobTypePriority = jobType.GetJobTypePriority();
		SetPriority(jobTypePriority);
	}

	public bool CanCharacterDoJob(Character character)
	{
		if (CanCharacterTakeThisJob(character))
		{
			return !blacklistedCharacters.Contains(character);
		}
		return false;
	}

	public override string ToString()
	{
		return $"{jobType} assigned to {assignedCharacter?.name}" ?? "None";
	}

	public bool IsJobStillApplicable()
	{
		if (stillApplicable != null)
		{
			return stillApplicable.IsJobStillApplicable(this);
		}
		return true;
	}

	public void SetDoNotRecalculate(bool state)
	{
		doNotRecalculate = state;
	}

	public void IncreaseInvalidCounter()
	{
		invalidCounter++;
	}

	public void ResetInvalidCounter()
	{
		invalidCounter = 0;
	}

	public void SetIsThisAPartyJob(bool state)
	{
		isThisAPartyJob = state;
	}

	public void SetIsThisAGatheringJob(bool state)
	{
		isThisAGatheringJob = state;
	}

	public void SetIsInMultithread(bool state)
	{
		isInMultithread = state;
	}

	public void SetShouldForceCancelJobUponReceiving(bool state)
	{
		shouldForceCancelUponReceiving = state;
	}

	public void LogUnableToDoReportCrime(Character p_actor)
	{
		if (!(this is GoapPlanJob goapPlanJob))
		{
			return;
		}
		ICrimeable crimeable = null;
		foreach (KeyValuePair<INTERACTION_TYPE, OtherData[]> otherDatum in goapPlanJob.otherData)
		{
			for (int i = 0; i < otherDatum.Value.Length; i++)
			{
				if (otherDatum.Value[i] is CrimeableOtherData crimeableOtherData)
				{
					crimeable = crimeableOtherData.crimeable;
					break;
				}
			}
			if (crimeable != null)
			{
				break;
			}
		}
		if (crimeable != null)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "unable_report_crime", LOG_TAG.Life_Changes, LOG_TAG.Crimes);
			log.AddToFillers(p_actor, p_actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(crimeable.actor, crimeable.actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFrom(p_actor, log, releaseLogAfter: true);
		}
	}

	public void SetConnectedFactionIdeology(FACTION_IDEOLOGY p_ideology)
	{
		connectedFactionIdeology = p_ideology;
	}

	public virtual void Reset()
	{
		DatabaseManager.Instance.jobDatabase.UnRegister(this);
		persistentID = string.Empty;
		hasBeenReset = true;
		shouldBeRemovedFromSettlementWhenUnassigned = false;
		id = -1;
		originalOwner = null;
		name = string.Empty;
		jobType = JOB_TYPE.NONE;
		blacklistedCharacters.Clear();
		canTakeJobChecker = null;
		assignedCharacter = null;
		stillApplicable = null;
		isTriggeredFlaw = false;
		numOfTimesPushedBack = 0;
		SetConnectedFactionIdeology(FACTION_IDEOLOGY.Inclusive);
		SetPriority(-1);
		SetCannotBePushedBack(state: false);
		SetFinishedSuccessfully(state: false);
		SetDoNotRecalculate(state: false);
		SetIsThisAPartyJob(state: false);
		SetIsThisAGatheringJob(state: false);
		SetForceCancelOnInvalid(state: false);
		SetIsInMultithread(state: false);
		SetShouldForceCancelJobUponReceiving(state: false);
		ResetInvalidCounter();
		Messenger.RemoveListener<JOB_TYPE, IPointOfInterest>(JobSignals.CHECK_JOB_APPLICABILITY, CheckJobApplicability);
		Messenger.RemoveListener<IPointOfInterest>(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, CheckJobApplicability);
		Messenger.RemoveListener<JOB_TYPE>(JobSignals.CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE, CheckJobApplicability);
		Messenger.RemoveListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, DisconnectFromSettlement);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
	}

	public virtual void CleanUpAfterActionIsDone()
	{
	}

	public virtual void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public virtual void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = originalOwner;
		_ = assignedCharacter;
		blacklistedCharacters.Contains(p_character);
	}
}
