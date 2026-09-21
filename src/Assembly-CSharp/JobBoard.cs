using System.Collections.Generic;
using System.Linq;
using UtilityScripts;

public class JobBoard
{
	public List<JobQueueItem> availableJobs { get; }

	public JobBoard()
	{
		availableJobs = new List<JobQueueItem>();
	}

	public void Initialize()
	{
		availableJobs.Clear();
		Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
	}

	public void InitializeFromSaveData(SaveDataJobBoard data)
	{
		Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
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
			OnJobRemovedFromAvailableJobs(job);
			return true;
		}
		return false;
	}

	private void OnJobRemovedFromAvailableJobs(JobQueueItem job)
	{
		Messenger.Broadcast(JobSignals.JOB_REMOVED_FROM_JOB_BOARD, job, this);
		JobManager.Instance.ReleaseJob(job);
	}

	public int GetNumberOfJobsWith(JOB_TYPE type)
	{
		int num = 0;
		for (int i = 0; i < availableJobs.Count; i++)
		{
			if (availableJobs[i].jobType == type)
			{
				num++;
			}
		}
		return num;
	}

	public bool HasJob(JOB_TYPE job, IPointOfInterest target)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (job == goapPlanJob.jobType && target == goapPlanJob.targetPOI)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasJob(params JOB_TYPE[] jobTypes)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			for (int j = 0; j < jobTypes.Length; j++)
			{
				if (availableJobs[i].jobType == jobTypes[j])
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasJob(GoapEffect effect, IPointOfInterest target)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (goapPlanJob.goal != null && effect.conditionType == goapPlanJob.goal.conditionType && effect.conditionKey == goapPlanJob.goal.conditionKey && effect.target == goapPlanJob.goal.target && target == goapPlanJob.targetPOI)
				{
					return true;
				}
			}
		}
		return false;
	}

	public JobQueueItem GetJob(params JOB_TYPE[] jobTypes)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			for (int j = 0; j < jobTypes.Length; j++)
			{
				JobQueueItem jobQueueItem = availableJobs[i];
				if (jobQueueItem.jobType == jobTypes[j])
				{
					return jobQueueItem;
				}
			}
		}
		return null;
	}

	public List<JobQueueItem> GetJobs(params JOB_TYPE[] jobTypes)
	{
		List<JobQueueItem> list = new List<JobQueueItem>();
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobTypes.Contains(jobQueueItem.jobType))
			{
				list.Add(jobQueueItem);
			}
		}
		return list;
	}

	public JobQueueItem GetJob(JOB_TYPE job, IPointOfInterest target)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (job == goapPlanJob.jobType && target == goapPlanJob.targetPOI)
				{
					return goapPlanJob;
				}
			}
		}
		return null;
	}

	public bool AddFirstUnassignedJobToCharacterJob(Character character)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && character.jobQueue.AddJobInQueue(jobQueueItem))
			{
				return true;
			}
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

	public bool AssignCharacterToJobBasedOnVision(Character character)
	{
		List<JobQueueItem> list = new List<JobQueueItem>();
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (goapPlanJob.targetPOI != null && character.marker.IsPOIInVision(goapPlanJob.targetPOI) && character.jobQueue.CanJobBeAddedToQueue(jobQueueItem))
				{
					list.Add(jobQueueItem);
				}
			}
		}
		if (list.Count > 0)
		{
			JobQueueItem randomElement = CollectionUtilities.GetRandomElement(list);
			return character.jobQueue.AddJobInQueue(randomElement);
		}
		return false;
	}

	public JobQueueItem GetFirstJobBasedOnVision(Character character)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (goapPlanJob.targetPOI != null && character.marker.IsPOIInVision(goapPlanJob.targetPOI) && character.jobQueue.CanJobBeAddedToQueue(jobQueueItem))
				{
					return jobQueueItem;
				}
			}
		}
		return null;
	}

	public JobQueueItem GetFirstJobBasedOnVisionExcept(Character character, params JOB_TYPE[] jobTypes)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && jobQueueItem is GoapPlanJob goapPlanJob && !jobTypes.Contains(goapPlanJob.jobType) && goapPlanJob.targetPOI != null && character.marker.IsPOIInVision(goapPlanJob.targetPOI) && character.jobQueue.CanJobBeAddedToQueue(goapPlanJob))
			{
				return goapPlanJob;
			}
		}
		return null;
	}

	private void ClearAllBlacklistToAllExistingJobs()
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			availableJobs[i].ClearBlacklist();
		}
	}

	private void OnDayStarted()
	{
		ClearAllBlacklistToAllExistingJobs();
	}

	public void LoadReferences(SaveDataJobBoard data)
	{
		if (data.availableJobs != null)
		{
			for (int i = 0; i < data.availableJobs.Count; i++)
			{
				availableJobs.Add(DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(data.availableJobs[i]));
			}
		}
	}

	public void Reset()
	{
		Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
	}
}
