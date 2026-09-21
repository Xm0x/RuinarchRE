using System.Collections.Generic;

public class JobQueue
{
	public Character owner { get; private set; }

	public List<JobQueueItem> jobsInQueue { get; private set; }

	public List<JobQueueItem> pendingTopPriorityJobs { get; private set; }

	public JobQueue(Character owner)
	{
		this.owner = owner;
		jobsInQueue = new List<JobQueueItem>();
		pendingTopPriorityJobs = new List<JobQueueItem>();
	}

	public void LoadReferences(SaveDataCharacter saveDataCharacter)
	{
		for (int i = 0; i < saveDataCharacter.jobs.Count; i++)
		{
			string id = saveDataCharacter.jobs[i];
			JobQueueItem jobWithPersistentIDSafe = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentIDSafe(id);
			if (jobWithPersistentIDSafe != null)
			{
				jobsInQueue.Add(jobWithPersistentIDSafe);
			}
		}
		if (saveDataCharacter.pendingTopPriorityJobs == null)
		{
			return;
		}
		for (int j = 0; j < saveDataCharacter.pendingTopPriorityJobs.Count; j++)
		{
			string id2 = saveDataCharacter.pendingTopPriorityJobs[j];
			JobQueueItem jobWithPersistentIDSafe2 = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentIDSafe(id2);
			if (jobWithPersistentIDSafe2 != null)
			{
				pendingTopPriorityJobs.Add(jobWithPersistentIDSafe2);
			}
		}
	}

	public bool AddJobInQueue(JobQueueItem job, bool shouldPersonalJobBeReturnedToObjectPoolOnAddFail = true)
	{
		if (owner == null || owner.isDead)
		{
			return false;
		}
		bool flag = owner.minion != null || IsJobTopPriorityWhenAdded(job);
		if (flag && job is GoapPlanJob { assignedPlan: null } goapPlanJob)
		{
			return AddJobToPendingJobQueue(goapPlanJob);
		}
		if (AddJobInQueueBase(job, flag))
		{
			return true;
		}
		if (shouldPersonalJobBeReturnedToObjectPoolOnAddFail && job.originalOwner == owner)
		{
			JobManager.Instance.ReleaseJob(job);
		}
		return false;
	}

	private bool AddJobInQueueBase(JobQueueItem job, bool isNewJobTopPriority)
	{
		if (!IsJobValidForCharacterGivenCurrentState(job))
		{
			return false;
		}
		if (!CanJobBeAddedToQueue(job))
		{
			return false;
		}
		job.SetAssignedCharacter(owner);
		InsertJobToMainJobQueue(job, isNewJobTopPriority);
		job.OnAddJobToQueue();
		job.originalOwner?.OnJobAddedToCharacterJobQueue(job, owner);
		Messenger.Broadcast(JobSignals.JOB_ADDED_TO_QUEUE, job, owner);
		if (job.jobType == JOB_TYPE.TRIGGER_FLAW)
		{
			job.isTriggeredFlaw = true;
		}
		return true;
	}

	public bool RemoveJobInQueue(JobQueueItem job, string reason = "", bool shouldBlacklist = false)
	{
		bool flag = pendingTopPriorityJobs.Remove(job);
		bool flag2 = jobsInQueue.Remove(job);
		if (flag || flag2)
		{
			Messenger.Broadcast(JobSignals.JOB_REMOVED_FROM_QUEUE, job, owner);
			owner.OnJobRemovedFromQueue(job);
			job.UnassignJob(reason);
			_ = owner.name;
			bool result = job.OnRemoveJobFromQueue();
			IJobOwner originalOwner = job.originalOwner;
			if (originalOwner != null)
			{
				originalOwner.OnJobRemovedFromCharacterJobQueue(job, owner, shouldBlacklist);
				return result;
			}
			return result;
		}
		return false;
	}

	public bool ProcessFirstJobInQueue()
	{
		if (jobsInQueue.Count > 0 && owner.HasSameOrHigherPriorityJobThanBehaviour())
		{
			jobsInQueue[0].ProcessJob();
			return true;
		}
		return false;
	}

	private void InsertJobToMainJobQueue(JobQueueItem job, bool isNewJobTopPriority)
	{
		if (isNewJobTopPriority)
		{
			if (jobsInQueue.Count > 0)
			{
				if (owner.minion != null)
				{
					if (job.jobType == JOB_TYPE.COMBAT)
					{
						jobsInQueue[0].PushedBack(job, shouldIncreasePushBackCount: false);
					}
					else
					{
						CancelAllJobs();
					}
				}
				else
				{
					jobsInQueue[0].PushedBack(job);
				}
			}
			jobsInQueue.Insert(0, job);
			job.ProcessJob();
			return;
		}
		bool flag = false;
		if (jobsInQueue.Count > 1)
		{
			for (int i = 1; i < jobsInQueue.Count; i++)
			{
				if (job.priority > jobsInQueue[i].priority)
				{
					jobsInQueue.Insert(i, job);
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			jobsInQueue.Add(job);
		}
	}

	public JobQueueItem GetJob(params JOB_TYPE[] jobTypes)
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			for (int j = 0; j < jobTypes.Length; j++)
			{
				JobQueueItem jobQueueItem = jobsInQueue[i];
				if (jobQueueItem.jobType == jobTypes[j])
				{
					return jobQueueItem;
				}
			}
		}
		for (int k = 0; k < pendingTopPriorityJobs.Count; k++)
		{
			for (int l = 0; l < jobTypes.Length; l++)
			{
				JobQueueItem jobQueueItem2 = pendingTopPriorityJobs[k];
				if (jobQueueItem2.jobType == jobTypes[l])
				{
					return jobQueueItem2;
				}
			}
		}
		return null;
	}

	public void CancelAllJobs(JOB_TYPE jobType)
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			JobQueueItem jobQueueItem = jobsInQueue[i];
			if (jobQueueItem.jobType == jobType && jobQueueItem.CancelJob())
			{
				i--;
			}
		}
		for (int j = 0; j < pendingTopPriorityJobs.Count; j++)
		{
			JobQueueItem jobQueueItem2 = pendingTopPriorityJobs[j];
			if (jobQueueItem2.jobType == jobType && jobQueueItem2.CancelJob())
			{
				j--;
			}
		}
	}

	public void CancelAllPartyJobs()
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			JobQueueItem jobQueueItem = jobsInQueue[i];
			if (jobQueueItem.isThisAPartyJob && jobQueueItem.CancelJob())
			{
				i--;
			}
		}
		for (int j = 0; j < pendingTopPriorityJobs.Count; j++)
		{
			JobQueueItem jobQueueItem2 = pendingTopPriorityJobs[j];
			if (jobQueueItem2.isThisAPartyJob && jobQueueItem2.CancelJob())
			{
				j--;
			}
		}
	}

	public void CancelAllGatheringJobs()
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			JobQueueItem jobQueueItem = jobsInQueue[i];
			if (jobQueueItem.isThisAGatheringJob && jobQueueItem.CancelJob())
			{
				i--;
			}
		}
		for (int j = 0; j < pendingTopPriorityJobs.Count; j++)
		{
			JobQueueItem jobQueueItem2 = pendingTopPriorityJobs[j];
			if (jobQueueItem2.isThisAGatheringJob && jobQueueItem2.CancelJob())
			{
				j--;
			}
		}
	}

	public void CancelAllJobs(params JOB_TYPE[] jobTypes)
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			for (int j = 0; j < jobTypes.Length; j++)
			{
				JobQueueItem jobQueueItem = jobsInQueue[i];
				if (jobQueueItem.jobType == jobTypes[j])
				{
					if (jobQueueItem.CancelJob())
					{
						i--;
					}
					break;
				}
			}
		}
		for (int k = 0; k < pendingTopPriorityJobs.Count; k++)
		{
			for (int l = 0; l < jobTypes.Length; l++)
			{
				JobQueueItem jobQueueItem2 = pendingTopPriorityJobs[k];
				if (jobQueueItem2.jobType == jobTypes[l])
				{
					if (jobQueueItem2.CancelJob())
					{
						k--;
					}
					break;
				}
			}
		}
	}

	public void CancelAllJobs(string reason = "")
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			if (jobsInQueue[i].CancelJob(reason))
			{
				i--;
			}
		}
		for (int j = 0; j < pendingTopPriorityJobs.Count; j++)
		{
			if (pendingTopPriorityJobs[j].CancelJob(reason))
			{
				j--;
			}
		}
	}

	public void CancelFirstJob()
	{
		if (jobsInQueue.Count > 0)
		{
			jobsInQueue[0].CancelJob();
		}
	}

	public int GetJobQueueIndex(JobQueueItem job)
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			if (jobsInQueue[i] == job)
			{
				return i;
			}
		}
		return -1;
	}

	private bool AddJobToPendingJobQueue(JobQueueItem job)
	{
		if (!IsJobValidForCharacterGivenCurrentState(job))
		{
			return false;
		}
		if (!CanJobBeAddedToQueue(job))
		{
			return false;
		}
		job.SetAssignedCharacter(owner);
		if (!pendingTopPriorityJobs.Contains(job))
		{
			pendingTopPriorityJobs.Add(job);
			if (pendingTopPriorityJobs.Count == 1)
			{
				job.ProcessJob();
			}
			job.OnAddJobToQueue();
			job.originalOwner?.OnJobAddedToCharacterJobQueue(job, owner);
			Messenger.Broadcast(JobSignals.JOB_ADDED_TO_QUEUE, job, owner);
			if (job.jobType == JOB_TYPE.TRIGGER_FLAW)
			{
				job.isTriggeredFlaw = true;
			}
			return true;
		}
		return false;
	}

	public void TransferPendingJobToMainJobQueue(JobQueueItem p_job, GoapPlan p_createdPlan)
	{
		if (pendingTopPriorityJobs.Remove(p_job))
		{
			if (p_job is GoapPlanJob goapPlanJob)
			{
				goapPlanJob.SetAssignedPlan(p_createdPlan);
			}
			bool isNewJobTopPriority = owner.minion != null || IsJobTopPriorityWhenAdded(p_job);
			InsertJobToMainJobQueue(p_job, isNewJobTopPriority);
		}
	}

	private bool IsJobTopPriorityWhenAdded(JobQueueItem newJob)
	{
		if (owner.behaviourComponent.GetHighestBehaviourPriority() > newJob.priority)
		{
			return false;
		}
		if (jobsInQueue.Count > 0)
		{
			JobQueueItem jobQueueItem = jobsInQueue[0];
			if (newJob.priority > jobQueueItem.priority && jobQueueItem.CanBeInterruptedBy(newJob.jobType))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public bool CanJobBeAddedToQueue(JobQueueItem job)
	{
		if (job.originalOwner == null)
		{
			return false;
		}
		if (job.originalOwner.ownerType == JOB_OWNER.CHARACTER)
		{
			return job.originalOwner == owner;
		}
		if (jobsInQueue.Count > 0)
		{
			if (job.priority > jobsInQueue[0].priority)
			{
				return job.CanCharacterDoJob(owner);
			}
			return false;
		}
		return job.CanCharacterDoJob(owner);
	}

	public bool HasJobInAnyQueue()
	{
		if (jobsInQueue.Count <= 0)
		{
			return pendingTopPriorityJobs.Count > 0;
		}
		return true;
	}

	public bool HasJob<T>() where T : JobQueueItem
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			if (jobsInQueue[i] is T)
			{
				return true;
			}
		}
		for (int j = 0; j < pendingTopPriorityJobs.Count; j++)
		{
			if (pendingTopPriorityJobs[j] is T)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJob(GoapEffect effect, IPointOfInterest target)
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			if (jobsInQueue[i] is GoapPlanJob gpj && DoesJobHavTheSameGoal(gpj))
			{
				return true;
			}
		}
		for (int j = 0; j < pendingTopPriorityJobs.Count; j++)
		{
			if (pendingTopPriorityJobs[j] is GoapPlanJob gpj2 && DoesJobHavTheSameGoal(gpj2))
			{
				return true;
			}
		}
		return false;
		bool DoesJobHavTheSameGoal(GoapPlanJob goapPlanJob)
		{
			if (goapPlanJob.goal == null)
			{
				return false;
			}
			if (effect.conditionType == goapPlanJob.goal.conditionType && effect.conditionKey == goapPlanJob.goal.conditionKey && effect.target == goapPlanJob.goal.target && target == goapPlanJob.targetPOI)
			{
				return true;
			}
			return false;
		}
	}

	public bool HasJob(JOB_TYPE jobType)
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			if (jobsInQueue[i].jobType == jobType)
			{
				return true;
			}
		}
		for (int j = 0; j < pendingTopPriorityJobs.Count; j++)
		{
			if (pendingTopPriorityJobs[j].jobType == jobType)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJob(JOB_TYPE jobType1, JOB_TYPE jobType2)
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			JobQueueItem jobQueueItem = jobsInQueue[i];
			if (jobQueueItem.jobType == jobType1 || jobQueueItem.jobType == jobType2)
			{
				return true;
			}
		}
		for (int j = 0; j < pendingTopPriorityJobs.Count; j++)
		{
			JobQueueItem jobQueueItem2 = pendingTopPriorityJobs[j];
			if (jobQueueItem2.jobType == jobType1 || jobQueueItem2.jobType == jobType2)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJob(JOB_TYPE jobType1, JOB_TYPE jobType2, JOB_TYPE jobType3)
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			JobQueueItem jobQueueItem = jobsInQueue[i];
			if (jobQueueItem.jobType == jobType1 || jobQueueItem.jobType == jobType2 || jobQueueItem.jobType == jobType3)
			{
				return true;
			}
		}
		for (int j = 0; j < pendingTopPriorityJobs.Count; j++)
		{
			JobQueueItem jobQueueItem2 = pendingTopPriorityJobs[j];
			if (jobQueueItem2.jobType == jobType1 || jobQueueItem2.jobType == jobType2 || jobQueueItem2.jobType == jobType3)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJob(JOB_TYPE jobType, IPointOfInterest targetPOI)
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			JobQueueItem jobQueueItem = jobsInQueue[i];
			if (jobQueueItem.jobType == jobType && jobQueueItem is GoapPlanJob goapPlanJob && goapPlanJob.targetPOI == targetPOI)
			{
				return true;
			}
		}
		for (int j = 0; j < pendingTopPriorityJobs.Count; j++)
		{
			JobQueueItem jobQueueItem2 = pendingTopPriorityJobs[j];
			if (jobQueueItem2.jobType == jobType && jobQueueItem2 is GoapPlanJob goapPlanJob2 && goapPlanJob2.targetPOI == targetPOI)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJob(JOB_TYPE jobType1, JOB_TYPE jobType2, IPointOfInterest targetPOI)
	{
		for (int i = 0; i < jobsInQueue.Count; i++)
		{
			JobQueueItem jobQueueItem = jobsInQueue[i];
			if ((jobQueueItem.jobType == jobType1 || jobQueueItem.jobType == jobType2) && jobQueueItem is GoapPlanJob goapPlanJob && goapPlanJob.targetPOI == targetPOI)
			{
				return true;
			}
		}
		for (int j = 0; j < pendingTopPriorityJobs.Count; j++)
		{
			JobQueueItem jobQueueItem2 = pendingTopPriorityJobs[j];
			if ((jobQueueItem2.jobType == jobType1 || jobQueueItem2.jobType == jobType2) && jobQueueItem2 is GoapPlanJob goapPlanJob2 && goapPlanJob2.targetPOI == targetPOI)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsJobValidForCharacterGivenCurrentState(JobQueueItem job)
	{
		if (!owner.limiterComponent.canPerform && job is GoapPlanJob { assignedPlan: not null } && (owner.limiterComponent.canPerformValue != -1 || (!owner.traitContainer.HasTrait("Paralyzed") && !owner.traitContainer.HasTrait("Quarantined"))))
		{
			return false;
		}
		if (job.jobType.IsFullnessRecoveryTypeJob() && !owner.limiterComponent.canDoFullnessRecovery && (owner.limiterComponent.canDoFullnessRecoveryValue != -1 || !owner.traitContainer.HasTrait("Abstain Fullness")))
		{
			return false;
		}
		return true;
	}
}
