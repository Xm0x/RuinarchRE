using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Traits;

namespace Databases;

public class TraitDatabase
{
	public Dictionary<string, Trait> traitsByGUID { get; }

	public List<Trait> traitsToBeLoadedOnMainThread { get; private set; }

	public TraitDatabase()
	{
		traitsByGUID = new Dictionary<string, Trait>();
		traitsToBeLoadedOnMainThread = new List<Trait>();
	}

	public void RegisterTrait(Trait trait)
	{
		traitsByGUID.Add(trait.persistentID, trait);
	}

	public void UnRegisterTrait(Trait trait)
	{
		if (traitsByGUID.Remove(trait.persistentID))
		{
			trait.CleanUp();
		}
	}

	public Trait GetTraitByPersistentID(string id)
	{
		if (traitsByGUID.ContainsKey(id))
		{
			return traitsByGUID[id];
		}
		throw new Exception("There is no trait with persistent ID " + id);
	}

	public void AddTraitsToBeLoadedOnMainThread(Trait trait)
	{
		traitsToBeLoadedOnMainThread.Add(trait);
	}

	public void ClearTraitsToBeLoadedOnMainThread()
	{
		traitsToBeLoadedOnMainThread.Clear();
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		foreach (KeyValuePair<string, Trait> item in traitsByGUID)
		{
			item.Value.CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		foreach (KeyValuePair<string, Trait> item in traitsByGUID)
		{
			item.Value.CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
