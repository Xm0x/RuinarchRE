using System;
using UnityEngine;

public class LoadSecondWaveJobs : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		base.LoadSavedData(state);
		Load(state);
	}

	private void Load(object state)
	{
		try
		{
			LoadThreadQueueItem loadThreadQueueItem = state as LoadThreadQueueItem;
			_ = loadThreadQueueItem.mapData;
			SaveDataCurrentProgress saveData = loadThreadQueueItem.saveData;
			for (int i = 0; i < DatabaseManager.Instance.jobDatabase.allJobs.Count; i++)
			{
				JobQueueItem jobQueueItem = DatabaseManager.Instance.jobDatabase.allJobs[i];
				SaveDataJobQueueItem fromSaveHub = saveData.GetFromSaveHub<SaveDataJobQueueItem>(OBJECT_TYPE.Job, jobQueueItem.persistentID);
				if (fromSaveHub != null)
				{
					jobQueueItem.LoadSecondWave(fromSaveHub);
				}
			}
			loadThreadQueueItem.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}
