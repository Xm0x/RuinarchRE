using System;
using UnityEngine;

public class LoadFirstWaveCharacters : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		LoadCharacters(state);
	}

	private void LoadCharacters(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadCharacters();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}
