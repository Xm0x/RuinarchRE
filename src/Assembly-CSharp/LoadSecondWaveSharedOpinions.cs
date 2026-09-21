using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadSecondWaveSharedOpinions : MapGenerationComponent
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
			foreach (KeyValuePair<string, SharedOpinionModifier> allSharedOpinionModifier in DatabaseManager.Instance.sharedOpinionDatabase.allSharedOpinionModifiers)
			{
				SharedOpinionModifier value = allSharedOpinionModifier.Value;
				SaveDataSharedOpinionModifier fromSaveHub = saveData.GetFromSaveHub<SaveDataSharedOpinionModifier>(OBJECT_TYPE.Shared_Opinion_Modifier, value.persistentID);
				value.LoadSecondWave(fromSaveHub);
			}
			loadThreadQueueItem.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}
