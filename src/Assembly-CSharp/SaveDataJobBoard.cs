using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataJobBoard : SaveData<JobBoard>
{
	public List<string> availableJobs;

	public override void Save(JobBoard data)
	{
		availableJobs = new List<string>();
		for (int i = 0; i < data.availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = data.availableJobs[i];
			availableJobs.Add(jobQueueItem.persistentID);
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(jobQueueItem);
		}
	}
}
