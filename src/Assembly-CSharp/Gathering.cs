using System;
using System.Collections.Generic;
using UtilityScripts;

public class Gathering : ISavable
{
	public string persistentID { get; private set; }

	public Character host { get; protected set; }

	public GATHERING_TYPE gatheringType { get; protected set; }

	public string gatheringName { get; protected set; }

	public int waitTimeInTicks { get; protected set; }

	public int minimumGatheringSize { get; protected set; }

	public Type relatedBehaviour { get; protected set; }

	public JOB_OWNER jobQueueOwnerType { get; protected set; }

	public IJobOwner jobOwner { get; protected set; }

	public List<Character> attendees { get; protected set; }

	public bool isWaitTimeOver { get; protected set; }

	public bool isDisbanded { get; protected set; }

	public bool isAlreadyWaiting { get; private set; }

	public virtual IGatheringTarget target => null;

	public virtual Area waitingHexArea => null;

	public virtual Type serializedData => typeof(SaveDataGathering);

	public OBJECT_TYPE objectType => OBJECT_TYPE.Gathering;

	public Gathering(GATHERING_TYPE gatheringType)
	{
		persistentID = Utilities.GetNewUniqueID();
		this.gatheringType = gatheringType;
		switch (gatheringType)
		{
		case GATHERING_TYPE.Social:
			gatheringName = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Social");
			break;
		case GATHERING_TYPE.Monster_Invade:
			gatheringName = LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "MONSTER_INVADE");
			break;
		default:
			throw new ArgumentOutOfRangeException("gatheringType", gatheringType, null);
		}
		attendees = new List<Character>();
	}

	public Gathering(SaveDataGathering data)
	{
		attendees = new List<Character>();
		persistentID = data.persistentID;
		gatheringType = data.gatheringType;
		gatheringName = data.gatheringName;
		waitTimeInTicks = data.waitTimeInTicks;
		minimumGatheringSize = data.minimumGatheringSize;
		relatedBehaviour = Type.GetType(data.relatedBehaviour);
		jobQueueOwnerType = data.jobQueueOwnerType;
		isWaitTimeOver = data.isWaitTimeOver;
		isDisbanded = data.isDisbanded;
		isAlreadyWaiting = data.isAlreadyWaiting;
	}

	public virtual bool IsAllowedToJoin(Character character)
	{
		return true;
	}

	protected virtual void OnAddAttendee(Character member)
	{
		member.gatheringComponent.SetCurrentGathering(this);
		member.behaviourComponent.AddBehaviourComponent(relatedBehaviour);
		if (member == host)
		{
			host.AddAdvertisedAction(INTERACTION_TYPE.JOIN_GATHERING);
		}
		ProcessAdditionOfJoinGatheringJobs();
	}

	protected virtual void OnRemoveAttendee(Character member)
	{
		member.gatheringComponent.SetCurrentGathering(null);
		member.behaviourComponent.RemoveBehaviourComponent(relatedBehaviour);
		if (member == host)
		{
			host.RemoveAdvertisedAction(INTERACTION_TYPE.JOIN_GATHERING);
		}
	}

	protected virtual void OnRemoveAttendeeOnDisband(Character member)
	{
		member.gatheringComponent.SetCurrentGathering(null);
		member.behaviourComponent.RemoveBehaviourComponent(relatedBehaviour);
		if (member == host)
		{
			host.RemoveAdvertisedAction(INTERACTION_TYPE.JOIN_GATHERING);
		}
		member.jobQueue.CancelAllGatheringJobs();
	}

	protected virtual void OnDisbandGathering()
	{
		isDisbanded = true;
		CancelAllJoinPartyJobs();
	}

	protected virtual void OnBeforeDisbandGathering()
	{
	}

	protected virtual void OnWaitTimeOver()
	{
	}

	protected virtual void OnWaitTimeOverButGatheringIsDisbanded()
	{
	}

	protected virtual void OnSetHost()
	{
		if (host != null)
		{
			if (jobQueueOwnerType == JOB_OWNER.SETTLEMENT)
			{
				jobOwner = host.homeSettlement;
			}
			else if (jobQueueOwnerType == JOB_OWNER.FACTION)
			{
				jobOwner = host.faction;
			}
			StartWaitTime();
			AddAttendee(host);
		}
	}

	public void SetHost(Character newHost)
	{
		if (host != newHost)
		{
			if (host != null)
			{
				RemoveAttendee(host);
			}
			host = newHost;
			OnSetHost();
		}
	}

	public bool AddAttendee(Character character)
	{
		if (!attendees.Contains(character))
		{
			attendees.Add(character);
			OnAddAttendee(character);
			return true;
		}
		return false;
	}

	public bool RemoveAttendee(Character character)
	{
		if (attendees.Remove(character))
		{
			OnRemoveAttendee(character);
			if (attendees.Count <= 0)
			{
				DisbandGathering();
			}
			return true;
		}
		return false;
	}

	public void DisbandGathering()
	{
		if (!isDisbanded)
		{
			OnBeforeDisbandGathering();
			for (int i = 0; i < attendees.Count; i++)
			{
				OnRemoveAttendeeOnDisband(attendees[i]);
			}
			attendees.Clear();
			OnDisbandGathering();
		}
	}

	public bool IsHost(Character character)
	{
		return host == character;
	}

	public bool IsAttendee(Character character)
	{
		return attendees.Contains(character);
	}

	public void StartWaitTime()
	{
		if (!isWaitTimeOver && !isAlreadyWaiting)
		{
			isAlreadyWaiting = true;
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(waitTimeInTicks);
			SchedulingManager.Instance.AddEntry(gameDate, ProcessWaiting, this);
		}
	}

	private void ProcessWaiting()
	{
		if (!isWaitTimeOver)
		{
			if (attendees.Count < minimumGatheringSize)
			{
				DisbandGathering();
				OnWaitTimeOverButGatheringIsDisbanded();
			}
			else
			{
				OnWaitTimeOver();
			}
			isWaitTimeOver = true;
		}
	}

	private void ProcessAdditionOfJoinGatheringJobs()
	{
		if (attendees.Count < minimumGatheringSize + 2 && !isWaitTimeOver)
		{
			CreateJoinGatheringJob();
		}
	}

	private void CreateJoinGatheringJob()
	{
		if (jobQueueOwnerType == JOB_OWNER.SETTLEMENT)
		{
			(jobOwner as NPCSettlement).settlementJobTriggerComponent.TriggerJoinGatheringJob(this);
		}
		else if (jobQueueOwnerType == JOB_OWNER.FACTION)
		{
			(jobOwner as Faction).factionJobTriggerComponent.TriggerJoinGatheringJob(this);
		}
	}

	private void CancelAllJoinPartyJobs()
	{
		if (jobQueueOwnerType == JOB_OWNER.SETTLEMENT)
		{
			jobOwner.ForceCancelJobTypesTargetingPOI(JOB_TYPE.JOIN_GATHERING, host);
		}
		else if (jobQueueOwnerType == JOB_OWNER.FACTION)
		{
			jobOwner.ForceCancelJobTypesTargetingPOI(JOB_TYPE.JOIN_GATHERING, host);
		}
	}

	public virtual void LoadReferences(SaveDataGathering data)
	{
		host = CharacterManager.Instance.GetCharacterByPersistentID(data.host);
		if (jobQueueOwnerType == JOB_OWNER.CHARACTER)
		{
			jobOwner = CharacterManager.Instance.GetCharacterByPersistentID(data.jobOwner);
		}
		else if (jobQueueOwnerType == JOB_OWNER.FACTION)
		{
			jobOwner = FactionManager.Instance.GetFactionByPersistentID(data.jobOwner);
		}
		else if (jobQueueOwnerType == JOB_OWNER.SETTLEMENT)
		{
			jobOwner = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.jobOwner) as NPCSettlement;
		}
		else if (jobQueueOwnerType == JOB_OWNER.PARTY)
		{
			jobOwner = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(data.jobOwner);
		}
		for (int i = 0; i < data.attendees.Count; i++)
		{
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(data.attendees[i]);
			if (characterByPersistentID != null)
			{
				attendees.Add(characterByPersistentID);
			}
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = host;
		_ = jobOwner;
		attendees.Contains(p_character);
	}
}
