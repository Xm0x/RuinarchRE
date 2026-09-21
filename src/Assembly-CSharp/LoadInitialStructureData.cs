using System;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class LoadInitialStructureData : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		try
		{
			LoadThreadQueueItem loadThreadQueueItem = state as LoadThreadQueueItem;
			_ = loadThreadQueueItem.mapData;
			SaveDataCurrentProgress saveData = loadThreadQueueItem.saveData;
			Region mainRegion = DatabaseManager.Instance.regionDatabase.mainRegion;
			for (int i = 0; i < saveData.worldMapSave.structureSaves.Count; i++)
			{
				SaveDataLocationStructure saveDataLocationStructure = saveData.worldMapSave.structureSaves[i];
				LocationStructure locationStructure = saveDataLocationStructure.InitialLoad(mainRegion);
				if (locationStructure != null && !locationStructure.hasBeenDestroyed)
				{
					if (locationStructure is Wilderness p_wilderness)
					{
						mainRegion.LoadWilderness(p_wilderness);
					}
					mainRegion.AddStructure(locationStructure);
					if (locationStructure.shouldBeLoadedOnMainThread)
					{
						DatabaseManager.Instance.structureDatabase.AddStructureToBeLoadedOnMainThread(locationStructure);
					}
				}
				saveDataLocationStructure.Load();
			}
			loadThreadQueueItem.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}
