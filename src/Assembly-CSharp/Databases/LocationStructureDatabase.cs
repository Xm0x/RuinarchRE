using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

namespace Databases;

public class LocationStructureDatabase
{
	public Dictionary<string, LocationStructure> structuresByGUID { get; }

	public List<LocationStructure> allStructures { get; }

	public List<LocationStructure> structuresToBeLoadedOnMainThread { get; private set; }

	public LocationStructureDatabase()
	{
		structuresByGUID = new Dictionary<string, LocationStructure>();
		allStructures = new List<LocationStructure>();
		structuresToBeLoadedOnMainThread = new List<LocationStructure>();
	}

	public void RegisterStructure(LocationStructure locationStructure)
	{
		structuresByGUID.Add(locationStructure.persistentID, locationStructure);
		allStructures.Add(locationStructure);
	}

	public void UnregisterStructure(LocationStructure locationStructure)
	{
		structuresByGUID.Remove(locationStructure.persistentID);
		allStructures.Remove(locationStructure);
	}

	public LocationStructure GetStructureByPersistentID(string id)
	{
		if (structuresByGUID.ContainsKey(id))
		{
			return structuresByGUID[id];
		}
		throw new Exception("There is no structure with persistent ID " + id);
	}

	public LocationStructure GetStructureByPersistentIDSafe(string id)
	{
		if (structuresByGUID.ContainsKey(id))
		{
			return structuresByGUID[id];
		}
		return null;
	}

	public bool HasStructure(string persistendID)
	{
		return GetStructureByPersistentIDSafe(persistendID) != null;
	}

	public LocationStructure GetStructureByID(int id)
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			if (locationStructure.id == id)
			{
				return locationStructure;
			}
		}
		return null;
	}

	public void AddStructureToBeLoadedOnMainThread(LocationStructure p_structure)
	{
		if (!structuresToBeLoadedOnMainThread.Contains(p_structure))
		{
			structuresToBeLoadedOnMainThread.Add(p_structure);
		}
	}

	public void ClearStructuresToBeLoadedOnMainThread()
	{
		structuresToBeLoadedOnMainThread.Clear();
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			if (locationStructure != p_structure)
			{
				locationStructure.CheckIfStructureIsStillReferenced(p_structure);
			}
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			allStructures[i].CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
