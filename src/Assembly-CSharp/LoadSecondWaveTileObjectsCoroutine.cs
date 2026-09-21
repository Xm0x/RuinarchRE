using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class LoadSecondWaveTileObjectsCoroutine : MapGenerationComponent
{
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Tile_Objects");
		yield return MapGenerator.Instance.StartCoroutine(Load(data, saveData));
	}

	private IEnumerator Load(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		yield return MapGenerator.Instance.StartCoroutine(LoadTileObjects(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadTileObjectAsDeadReference(saveData));
	}

	private IEnumerator LoadTileObjects(SaveDataCurrentProgress saveData)
	{
		int batchCount = 0;
		HashSet<TileObject> allTileObjects = DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList;
		DatabaseManager.Instance.tileObjectDatabase.tempHolderOfDeadTileObjects.Clear();
		for (int i = 0; i < allTileObjects.Count; i++)
		{
			TileObject tileObject = allTileObjects.ElementAt(i);
			string persistentID = tileObject.persistentID;
			SaveDataTileObject fromSaveHub = saveData.GetFromSaveHub<SaveDataTileObject>(OBJECT_TYPE.Tile_Object, persistentID);
			if (tileObject.isDeadReference)
			{
				DatabaseManager.Instance.tileObjectDatabase.tempHolderOfDeadTileObjects.Add(tileObject);
			}
			if (fromSaveHub == null)
			{
				continue;
			}
			SaveDataTraitContainer saveDataTraitContainer = fromSaveHub.saveDataTraitContainer;
			tileObject.traitContainer.Load(tileObject, saveDataTraitContainer);
			if (tileObject is ThinWall || (!string.IsNullOrEmpty(persistentID) && persistentID == saveData.playerSave.seizedPOIID))
			{
				continue;
			}
			if (tileObject is GenericTileObject || !fromSaveHub.tileLocationID.hasValue)
			{
				tileObject.LoadSecondWave(fromSaveHub);
				continue;
			}
			if (tileObject is Tombstone tombstone)
			{
				tileObject.LoadSecondWave(fromSaveHub);
				if (!fromSaveHub.tileLocationID.hasValue)
				{
					continue;
				}
				if (tombstone.character == null || tombstone.character.marker == null)
				{
					Debug.LogWarning($"{tombstone} with persistent id {tombstone.persistentID} does not have a character inside it, but has a tile location. Not placing it to prevent errors, but this case should not happen!");
					continue;
				}
				LocationGridTile tileBySavedData = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(fromSaveHub.tileLocationID);
				tileBySavedData.structure.LoadPOI(tileObject, tileBySavedData);
				if (tileObject.mapObjectVisual != null)
				{
					TileObjectScriptableObject tileObjectScriptableObject = InnerMapManager.Instance.GetTileObjectScriptableObject<TileObjectScriptableObject>(tileObject.tileObjectType);
					tileObject.mapObjectVisual.SetVisual(tileObjectScriptableObject.GetSpriteByIndex(fromSaveHub.spriteIndex), fromSaveHub.spriteIndex);
					tileObject.mapObjectVisual.SetRotation(fromSaveHub.rotation);
				}
				continue;
			}
			if (fromSaveHub.tileLocationID.hasValue)
			{
				LocationGridTile tileBySavedData2 = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(fromSaveHub.tileLocationID);
				if (tileObject is MovingTileObject)
				{
					SaveDataMovingTileObject saveDataMovingTileObject = fromSaveHub as SaveDataMovingTileObject;
					if (!saveDataMovingTileObject.hasExpired)
					{
						tileObject.SetGridTileLocation(tileBySavedData2);
						tileObject.OnPlacePOI();
						tileObject.mapObjectVisual.SetWorldPosition(saveDataMovingTileObject.mapVisualWorldPosition);
						tileObject.LoadSecondWave(fromSaveHub);
					}
				}
				else
				{
					tileBySavedData2.structure.LoadPOI(tileObject, tileBySavedData2);
					if (tileObject.mapObjectVisual != null)
					{
						TileObjectScriptableObject tileObjectScriptableObject2 = InnerMapManager.Instance.GetTileObjectScriptableObject<TileObjectScriptableObject>(tileObject.tileObjectType);
						Sprite spriteByIndex = tileObjectScriptableObject2.GetSpriteByIndex(fromSaveHub.spriteIndex);
						if (spriteByIndex == null)
						{
							tileObject.mapObjectVisual.SetVisual(tileObjectScriptableObject2.defaultSprite);
							if (tileObjectScriptableObject2.defaultSprite != null && tileObject is Table)
							{
								tileObject.RevalidateTileObjectSlots();
							}
						}
						else
						{
							tileObject.mapObjectVisual.SetVisual(spriteByIndex);
							if (tileObject is Table)
							{
								tileObject.RevalidateTileObjectSlots();
							}
						}
						tileObject.mapObjectVisual.SetRotation(fromSaveHub.rotation);
					}
					tileObject.LoadSecondWave(fromSaveHub);
					tileObject.LoadAdditionalInfo(fromSaveHub);
				}
			}
			batchCount++;
			if (batchCount == MapGenerationData.TileObjectLoadingBatches)
			{
				batchCount = 0;
				yield return null;
			}
		}
	}

	private IEnumerator LoadTileObjectAsDeadReference(SaveDataCurrentProgress saveData)
	{
		int batchCount = 0;
		List<TileObject> allTileObjects = DatabaseManager.Instance.tileObjectDatabase.tempHolderOfDeadTileObjects;
		for (int i = 0; i < allTileObjects.Count; i++)
		{
			TileObject tileObject = allTileObjects[i];
			DatabaseManager.Instance.tileObjectDatabase.UnRegisterTileObject(tileObject, processCleanUp: false);
			batchCount++;
			if (batchCount == MapGenerationData.TileObjectLoadingBatches)
			{
				batchCount = 0;
				yield return null;
			}
		}
		allTileObjects.Clear();
	}
}
