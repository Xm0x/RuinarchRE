using System;
using UnityEngine;

public class LoadSecondWaveStructures : MapGenerationComponent
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
			for (int i = 0; i < saveData.worldMapSave.structureSaves.Count; i++)
			{
				SaveDataLocationStructure saveDataLocationStructure = saveData.worldMapSave.structureSaves[i];
				DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataLocationStructure.persistentID).LoadReferences(saveDataLocationStructure);
			}
			loadThreadQueueItem.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}
