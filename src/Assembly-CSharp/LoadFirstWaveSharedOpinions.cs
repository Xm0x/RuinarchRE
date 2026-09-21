using System;
using UnityEngine;

public class LoadFirstWaveSharedOpinions : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		LoadSharedOpinions(state);
	}

	private void LoadSharedOpinions(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadSharedOpinionModifiers();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}
