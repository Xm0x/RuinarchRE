using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RegionInnerMapGeneration : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating_Map");
		Region mainRegion = DatabaseManager.Instance.regionDatabase.mainRegion;
		yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.GenerateRegionMap(mainRegion, this, data));
	}

	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Inner_Maps");
		Region location = DatabaseManager.Instance.regionDatabase.mainRegion;
		SaveDataRegion saveDataRegion = saveData.worldMapSave.regionSave;
		yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.LoadRegionMap(location, this, saveDataRegion.innerMapSave, saveData));
		for (int i = 0; i < saveData.worldMapSave.structureSaves.Count; i++)
		{
			SaveDataLocationStructure saveDataLocationStructure = saveData.worldMapSave.structureSaves[i];
			if (!saveDataLocationStructure.hasBeenDestroyed)
			{
				yield return MapGenerator.Instance.StartCoroutine(AssignTilesToStructures(saveDataLocationStructure, location));
				yield return MapGenerator.Instance.StartCoroutine(LoadStructureObject(saveDataLocationStructure, location));
			}
		}
		Dictionary<string, TileBase> tileAssetDB = InnerMapManager.Instance.assetManager.GetFloorAndWallTileAssetDB();
		yield return MapGenerator.Instance.StartCoroutine(location.innerMap.LoadTileVisuals(this, saveDataRegion.innerMapSave, tileAssetDB));
		tileAssetDB.Clear();
		for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++)
		{
			InnerMapManager.Instance.innerMaps[i].groundTilemap.RefreshAllTiles();
			yield return null;
		}
	}

	private IEnumerator AssignTilesToStructures(SaveDataLocationStructure saveDataLocationStructure, Region region)
	{
		if (saveDataLocationStructure.structureType != STRUCTURE_TYPE.WILDERNESS)
		{
			LocationStructure structureByPersistentID = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataLocationStructure.persistentID);
			for (int i = 0; i < saveDataLocationStructure.tileCoordinates.Length; i++)
			{
				Point point = saveDataLocationStructure.tileCoordinates[i];
				region.innerMap.map[point.X, point.Y].SetStructure(structureByPersistentID);
			}
		}
		yield return null;
	}

	private IEnumerator LoadStructureObject(SaveDataLocationStructure saveDataLocationStructure, Region region)
	{
		LocationStructure structureByID = region.GetStructureByID(saveDataLocationStructure.structureType, saveDataLocationStructure.id);
		if (structureByID is ManMadeStructure manMadeStructure && saveDataLocationStructure is SaveDataManMadeStructure saveDataManMadeStructure)
		{
			LocationStructureObject component = ObjectPoolManager.Instance.InstantiateObjectFromPool(saveDataManMadeStructure.structureTemplateName, saveDataManMadeStructure.structureObjectWorldPosition, Quaternion.identity, region.innerMap.structureParent, isWorldPosition: true).GetComponent<LocationStructureObject>();
			component.RefreshAllTilemaps();
			List<LocationGridTile> tilesOccupiedByStructure = component.GetTilesOccupiedByStructure(region.innerMap);
			component.SetTilesInStructure(tilesOccupiedByStructure.ToArray());
			manMadeStructure.SetStructureObject(component);
			if (!string.IsNullOrEmpty(saveDataLocationStructure.occupiedAreaID))
			{
				Area areaByPersistentID = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(saveDataLocationStructure.occupiedAreaID);
				structureByID.SetOccupiedArea(areaByPersistentID);
			}
			component.OnLoadStructureObjectPlaced(region.innerMap, structureByID, saveDataLocationStructure);
			structureByID.CreateRoomsBasedOnStructureObject(component);
			structureByID.OnDoneLoadStructure();
			if (manMadeStructure.structureWalls != null)
			{
				for (int i = 0; i < manMadeStructure.structureWalls.Count; i++)
				{
					ThinWall thinWall = manMadeStructure.structureWalls[i];
					SaveDataTileObject saveDataStructureWallObject = saveDataManMadeStructure.structureWallObjects[i];
					thinWall.LoadDataFromSave(saveDataStructureWallObject);
				}
			}
			yield return null;
		}
		else if (structureByID is DemonicStructure demonicStructure && saveDataLocationStructure is SaveDataDemonicStructure saveDataDemonicStructure)
		{
			LocationStructureObject component2 = ObjectPoolManager.Instance.InstantiateObjectFromPool(saveDataDemonicStructure.structureTemplateName, saveDataDemonicStructure.structureObjectWorldPosition, Quaternion.identity, region.innerMap.structureParent, isWorldPosition: true).GetComponent<LocationStructureObject>();
			component2.RefreshAllTilemaps();
			List<LocationGridTile> tilesOccupiedByStructure2 = component2.GetTilesOccupiedByStructure(region.innerMap);
			component2.SetTilesInStructure(tilesOccupiedByStructure2.ToArray());
			demonicStructure.SetStructureObject(component2);
			if (!string.IsNullOrEmpty(saveDataLocationStructure.occupiedAreaID))
			{
				Area areaByPersistentID2 = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(saveDataLocationStructure.occupiedAreaID);
				structureByID.SetOccupiedArea(areaByPersistentID2);
			}
			component2.OnLoadStructureObjectPlaced(region.innerMap, structureByID, saveDataLocationStructure);
			structureByID.CreateRoomsBasedOnStructureObject(component2);
			structureByID.OnDoneLoadStructure();
			yield return null;
		}
		else if (structureByID is NaturalStructure naturalStructure && saveDataLocationStructure is SaveDataNaturalStructure saveDataNaturalStructure)
		{
			if (!string.IsNullOrEmpty(saveDataLocationStructure.occupiedAreaID))
			{
				Area areaByPersistentID3 = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(saveDataLocationStructure.occupiedAreaID);
				structureByID.SetOccupiedArea(areaByPersistentID3);
			}
			if (naturalStructure is NaturalStructureWithStructureObject naturalStructureWithStructureObject && saveDataNaturalStructure is SaveDataNaturalStructureWithStructureObject saveDataNaturalStructureWithStructureObject)
			{
				LocationStructureObject component3 = ObjectPoolManager.Instance.InstantiateObjectFromPool(saveDataNaturalStructureWithStructureObject.structureTemplateName, saveDataNaturalStructureWithStructureObject.structureObjectWorldPosition, Quaternion.identity, region.innerMap.structureParent, isWorldPosition: true).GetComponent<LocationStructureObject>();
				component3.RefreshAllTilemaps();
				List<LocationGridTile> tilesOccupiedByStructure3 = component3.GetTilesOccupiedByStructure(region.innerMap);
				component3.SetTilesInStructure(tilesOccupiedByStructure3.ToArray());
				naturalStructureWithStructureObject.SetStructureObject(component3);
				component3.OnLoadStructureObjectPlaced(region.innerMap, structureByID, saveDataLocationStructure);
				structureByID.CreateRoomsBasedOnStructureObject(component3);
				structureByID.OnDoneLoadStructure();
			}
		}
	}
}
