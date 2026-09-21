using System.Collections.Generic;
using Inner_Maps.Location_Structures;

namespace Databases;

public class BurningSourceDatabase
{
	public Dictionary<string, BurningSource> burningSourcesByID { get; }

	public List<BurningSource> burningSources { get; }

	public BurningSourceDatabase()
	{
		burningSourcesByID = new Dictionary<string, BurningSource>();
		burningSources = new List<BurningSource>();
	}

	public void Register(BurningSource burningSource)
	{
		burningSourcesByID.Add(burningSource.persistentID, burningSource);
		burningSources.Add(burningSource);
	}

	public void UnRegister(BurningSource burningSource)
	{
		burningSourcesByID.Remove(burningSource.persistentID);
		burningSources.Remove(burningSource);
		burningSource.CleanUp();
	}

	public BurningSource GetOrCreateBurningSourceWithID(string id)
	{
		if (burningSourcesByID.ContainsKey(id))
		{
			return burningSourcesByID[id];
		}
		return new BurningSource(id);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		for (int i = 0; i < burningSources.Count; i++)
		{
			burningSources[i].CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		for (int i = 0; i < burningSources.Count; i++)
		{
			burningSources[i].CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
