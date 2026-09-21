using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class AreaDatabase
{
	public Dictionary<string, Area> areaByGUID { get; }

	public List<Area> allAreas { get; }

	public AreaDatabase()
	{
		areaByGUID = new Dictionary<string, Area>();
		allAreas = new List<Area>();
	}

	public void RegisterArea(Area p_area)
	{
		areaByGUID.Add(p_area.areaData.persistentID, p_area);
		allAreas.Add(p_area);
	}

	public Area GetAreaByPersistentID(string id)
	{
		if (areaByGUID.ContainsKey(id))
		{
			return areaByGUID[id];
		}
		throw new Exception("There is no area with persistent ID " + id);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		for (int i = 0; i < allAreas.Count; i++)
		{
			allAreas[i].CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		for (int i = 0; i < allAreas.Count; i++)
		{
			allAreas[i].CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
