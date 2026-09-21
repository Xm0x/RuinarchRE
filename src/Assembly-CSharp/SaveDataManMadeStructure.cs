using System.Collections.Generic;
using BayatGames.SaveGameFree.Types;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class SaveDataManMadeStructure : SaveDataLocationStructure
{
	public string structureTemplateName;

	public SaveDataTileObject[] structureWallObjects;

	public WALL_RESOURCE wallsMadeOf;

	public Vector3Save structureObjectWorldPosition;

	public SaveDataStructureConnector[] structureConnectors;

	public string[] assignedWorkerIDs;

	public Dictionary<string, GameDate> workerCheckins;

	public string[] dirtyObjects;

	public GameDate scheduledDirtProduction;

	public bool hasStartedExpiration;

	public GameDate expirationDate;

	public override void Save(LocationStructure locationStructure)
	{
		base.Save(locationStructure);
		ManMadeStructure manMadeStructure = locationStructure as ManMadeStructure;
		hasStartedExpiration = manMadeStructure.hasStartedExpiration;
		expirationDate = manMadeStructure.expirationDate;
		assignedWorkerIDs = manMadeStructure.assignedWorkerIDs.ToArray();
		workerCheckins = new Dictionary<string, GameDate>(manMadeStructure.workerCheckins);
		if (manMadeStructure.hasBeenDestroyed)
		{
			structureTemplateName = string.Empty;
			structureObjectWorldPosition = Vector3.zero;
		}
		else if (manMadeStructure.structureObj != null)
		{
			string templateName = manMadeStructure.templateName;
			templateName = templateName.Replace("(Clone)", "");
			structureTemplateName = templateName;
			structureObjectWorldPosition = manMadeStructure.structureObjectWorldPos;
			structureConnectors = new SaveDataStructureConnector[manMadeStructure.structureObj.connectors.Length];
			for (int i = 0; i < structureConnectors.Length; i++)
			{
				StructureConnector data = manMadeStructure.structureObj.connectors[i];
				SaveDataStructureConnector saveDataStructureConnector = new SaveDataStructureConnector();
				saveDataStructureConnector.Save(data);
				structureConnectors[i] = saveDataStructureConnector;
			}
		}
		if (manMadeStructure.structureWalls != null)
		{
			structureWallObjects = new SaveDataTileObject[manMadeStructure.structureWalls.Count];
			for (int j = 0; j < manMadeStructure.structureWalls.Count; j++)
			{
				ThinWall data2 = manMadeStructure.structureWalls[j];
				SaveDataTileObject saveDataTileObject = new SaveDataTileObject();
				saveDataTileObject.Save(data2);
				structureWallObjects[j] = saveDataTileObject;
			}
			wallsMadeOf = manMadeStructure.wallsAreMadeOf;
		}
		if (manMadeStructure.dirtyObjects.Count > 0)
		{
			dirtyObjects = new string[manMadeStructure.dirtyObjects.Count];
			for (int k = 0; k < manMadeStructure.dirtyObjects.Count; k++)
			{
				TileObject tileObject = manMadeStructure.dirtyObjects[k];
				dirtyObjects[k] = tileObject.persistentID;
			}
		}
		if (manMadeStructure.scheduledDirtProduction.hasValue)
		{
			scheduledDirtProduction = manMadeStructure.scheduledDirtProduction;
		}
	}
}
