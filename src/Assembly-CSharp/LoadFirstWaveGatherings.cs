using System;
using UnityEngine;

public class LoadFirstWaveGatherings : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		LoadGatherings(state);
	}

	private void LoadGatherings(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadGatherings();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}
