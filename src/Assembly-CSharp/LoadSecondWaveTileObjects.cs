using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class LoadSecondWaveTileObjects : MapGenerationComponent
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
			HashSet<TileObject> allTileObjectsList = DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList;
			for (int i = 0; i < allTileObjectsList.Count; i++)
			{
				TileObject tileObject = allTileObjectsList.ElementAt(i);
				string persistentID = tileObject.persistentID;
				SaveDataTileObject fromSaveHub = saveData.GetFromSaveHub<SaveDataTileObject>(OBJECT_TYPE.Tile_Object, persistentID);
				if (fromSaveHub == null || tileObject is ThinWall)
				{
					continue;
				}
				if (tileObject is GenericTileObject || !fromSaveHub.tileLocationID.hasValue)
				{
					tileObject.LoadSecondWave(fromSaveHub);
				}
				else if (tileObject is Tombstone)
				{
					tileObject.LoadSecondWave(fromSaveHub);
				}
				else
				{
					if (!fromSaveHub.tileLocationID.hasValue)
					{
						continue;
					}
					LocationGridTile tileBySavedData = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(fromSaveHub.tileLocationID);
					if (tileObject is MovingTileObject)
					{
						SaveDataMovingTileObject saveDataMovingTileObject = fromSaveHub as SaveDataMovingTileObject;
						if (!saveDataMovingTileObject.hasExpired)
						{
							tileObject.SetGridTileLocation(tileBySavedData);
							tileObject.OnPlacePOI();
							tileObject.mapObjectVisual.SetWorldPosition(saveDataMovingTileObject.mapVisualWorldPosition);
							tileObject.LoadSecondWave(fromSaveHub);
						}
						continue;
					}
					tileBySavedData.structure.LoadPOI(tileObject, tileBySavedData);
					if (tileObject.mapObjectVisual != null)
					{
						TileObjectScriptableObject tileObjectScriptableObject = InnerMapManager.Instance.GetTileObjectScriptableObject<TileObjectScriptableObject>(tileObject.tileObjectType);
						tileObject.mapObjectVisual.SetVisual(tileObjectScriptableObject.GetSpriteByIndex(fromSaveHub.spriteIndex), fromSaveHub.spriteIndex);
						if (tileObject is Table)
						{
							tileObject.RevalidateTileObjectSlots();
						}
						tileObject.mapObjectVisual.SetRotation(fromSaveHub.rotation);
					}
					tileObject.LoadSecondWave(fromSaveHub);
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
