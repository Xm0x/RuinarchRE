using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

namespace Databases;

public class JobDatabase
{
	public Dictionary<string, JobQueueItem> jobsByGUID { get; }

	public List<JobQueueItem> allJobs { get; }

	public JobDatabase()
	{
		jobsByGUID = new Dictionary<string, JobQueueItem>();
		allJobs = new List<JobQueueItem>();
	}

	public void Register(JobQueueItem job)
	{
		jobsByGUID.Add(job.persistentID, job);
		allJobs.Add(job);
	}

	public bool UnRegister(JobQueueItem job)
	{
		allJobs.Remove(job);
		if (jobsByGUID.Remove(job.persistentID))
		{
			return true;
		}
		return false;
	}

	public JobQueueItem GetJobWithPersistentID(string id)
	{
		if (jobsByGUID.ContainsKey(id))
		{
			return jobsByGUID[id];
		}
		throw new Exception("Could not find job with id " + id);
	}

	public JobQueueItem GetJobWithPersistentIDSafe(string id)
	{
		if (jobsByGUID.ContainsKey(id))
		{
			return jobsByGUID[id];
		}
		return null;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		for (int i = 0; i < allJobs.Count; i++)
		{
			allJobs[i].CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		for (int i = 0; i < allJobs.Count; i++)
		{
			allJobs[i].CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
