using System;
using UnityEngine;

public class LoadFirstWaveCrimes : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		LoadCrimes(state);
	}

	private void LoadCrimes(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadCrimes();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}
