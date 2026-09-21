using System;
using UnityEngine;

public class LoadFirstWaveJobs : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		LoadJobs(state);
	}

	private void LoadJobs(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadJobs();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}
