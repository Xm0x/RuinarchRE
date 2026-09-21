using System;
using UnityEngine;

public class LoadSecondWaveActions : MapGenerationComponent
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
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			SaveDataCurrentProgress saveData = obj.saveData;
			saveData.LoadActionReferences();
			saveData.LoadAdditionalActionReferences();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}
