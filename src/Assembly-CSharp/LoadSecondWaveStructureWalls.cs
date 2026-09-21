using System;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class LoadSecondWaveStructureWalls : MapGenerationComponent
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
				if (!(saveDataLocationStructure is SaveDataManMadeStructure saveDataManMadeStructure))
				{
					continue;
				}
				ManMadeStructure manMadeStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataLocationStructure.persistentID) as ManMadeStructure;
				if (manMadeStructure.structureWalls != null)
				{
					for (int j = 0; j < manMadeStructure.structureWalls.Count; j++)
					{
						ThinWall thinWall = manMadeStructure.structureWalls[j];
						SaveDataTileObject saveDataTileObject = saveDataManMadeStructure.structureWallObjects[j];
						thinWall.traitContainer.Load(thinWall, saveDataTileObject.saveDataTraitContainer);
					}
				}
			}
			loadThreadQueueItem.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}
