using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Inner_Maps.Location_Structures.Components;
using UtilityScripts;

[Serializable]
public class SaveDataLocationStructure : SaveData<LocationStructure>
{
	public string persistentID;

	public int id;

	public string name;

	public STRUCTURE_TYPE structureType;

	public STRUCTURE_TAG[] structureTags;

	public Point[] tileCoordinates;

	public int maxHP;

	public int currentHP;

	public List<string> residentIDs;

	public List<string> charactersHereIDs;

	public string occupiedAreaID;

	public string settlementLocationID;

	public bool isInterior;

	public SaveDataStructureRoom[] structureRoomSaveData;

	public bool hasBeenDestroyed;

	public bool isStoredAsTarget;

	public List<string> tileObjectDamageContributors;

	public int differentFoodPileKindsInDwelling;

	public SaveDataPartyStructureComponent saveDataPartyStructureComponent;

	public override void Save(LocationStructure structure)
	{
		persistentID = structure.persistentID;
		id = structure.id;
		name = structure.name;
		structureType = structure.structureType;
		settlementLocationID = structure.settlementLocation?.persistentID ?? string.Empty;
		isStoredAsTarget = structure.isStoredAsTarget;
		structureTags = new STRUCTURE_TAG[structure.structureTags.Count];
		for (int i = 0; i < structure.structureTags.Count; i++)
		{
			STRUCTURE_TAG sTRUCTURE_TAG = structure.structureTags[i];
			structureTags[i] = sTRUCTURE_TAG;
		}
		tileCoordinates = new Point[structure.tiles.Count];
		List<LocationGridTile> list = new List<LocationGridTile>(structure.tiles);
		for (int j = 0; j < list.Count; j++)
		{
			LocationGridTile locationGridTile = list[j];
			Point point = new Point(locationGridTile.localPlace.x, locationGridTile.localPlace.y);
			tileCoordinates[j] = point;
		}
		maxHP = structure.maxHP;
		currentHP = structure.currentHP;
		residentIDs = SaveUtilities.ConvertSavableListToIDs(structure.residents);
		charactersHereIDs = SaveUtilities.ConvertSavableListToIDs(structure.charactersHere);
		if (structure.occupiedArea != null)
		{
			occupiedAreaID = structure.occupiedArea.persistentID;
		}
		else
		{
			occupiedAreaID = string.Empty;
		}
		isInterior = structure.isInterior;
		if (structure.rooms != null)
		{
			structureRoomSaveData = new SaveDataStructureRoom[structure.rooms.Length];
			for (int k = 0; k < structure.rooms.Length; k++)
			{
				StructureRoom structureRoom = structure.rooms[k];
				SaveDataStructureRoom saveDataStructureRoom = SaveUtilities.CreateSaveDataForRoom(structureRoom);
				saveDataStructureRoom.Save(structureRoom);
				structureRoomSaveData[k] = saveDataStructureRoom;
			}
		}
		hasBeenDestroyed = structure.hasBeenDestroyed;
		if (!structure.hasBeenDestroyed)
		{
			tileObjectDamageContributors = new List<string>();
			for (int l = 0; l < structure.objectsThatContributeToDamage.Count; l++)
			{
				if (structure.objectsThatContributeToDamage.ElementAt(l) is TileObject tileObject)
				{
					tileObjectDamageContributors.Add(tileObject.persistentID);
				}
			}
		}
		differentFoodPileKindsInDwelling = structure.differentFoodPileKindsInDwelling;
		if (structure.partyStructureComponent != null)
		{
			saveDataPartyStructureComponent = Activator.CreateInstance(structure.partyStructureComponent.serializedData) as SaveDataPartyStructureComponent;
			saveDataPartyStructureComponent.Save(structure.partyStructureComponent);
		}
	}

	public LocationStructure InitialLoad(Region region)
	{
		return LandmarkManager.Instance.LoadNewStructureAt(region, structureType, this);
	}

	public override void CleanUp()
	{
		structureTags = null;
		tileCoordinates = null;
		if (residentIDs != null)
		{
			RuinarchListPool<string>.Release(residentIDs);
			residentIDs = null;
		}
		if (charactersHereIDs != null)
		{
			RuinarchListPool<string>.Release(charactersHereIDs);
			charactersHereIDs = null;
		}
		if (structureRoomSaveData != null)
		{
			for (int i = 0; i < structureRoomSaveData.Length; i++)
			{
				structureRoomSaveData[i].CleanUp();
			}
			structureRoomSaveData = null;
		}
		if (tileObjectDamageContributors != null)
		{
			RuinarchListPool<string>.Release(tileObjectDamageContributors);
			tileObjectDamageContributors = null;
		}
		saveDataPartyStructureComponent?.CleanUp();
		saveDataPartyStructureComponent = null;
	}
}
