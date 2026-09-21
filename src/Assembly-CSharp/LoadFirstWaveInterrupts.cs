using System;
using UnityEngine;

public class LoadFirstWaveInterrupts : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		LoadInterrupts(state);
	}

	private void LoadInterrupts(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadInterrupts();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}
