using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class SharedOpinionDatabase
{
	public Dictionary<string, SharedOpinionModifier> allSharedOpinionModifiers;

	public SharedOpinionDatabase()
	{
		allSharedOpinionModifiers = new Dictionary<string, SharedOpinionModifier>();
	}

	public void AddSharedOpinion(SharedOpinionModifier p_modifier)
	{
		if (!allSharedOpinionModifiers.ContainsKey(p_modifier.persistentID))
		{
			allSharedOpinionModifiers.Add(p_modifier.persistentID, p_modifier);
		}
	}

	public void RemoveSharedOpinion(SharedOpinionModifier p_modifier)
	{
		if (allSharedOpinionModifiers.ContainsKey(p_modifier.persistentID))
		{
			allSharedOpinionModifiers.Remove(p_modifier.persistentID);
			p_modifier.OnModifierRemovedFromDatabase();
			Messenger.Broadcast(CharacterSignals.SHARED_OPINION_REMOVED_FROM_DATABASE, p_modifier);
		}
	}

	public SharedOpinionModifier GetOpinionModifierByPersistentID(string id)
	{
		if (allSharedOpinionModifiers.ContainsKey(id))
		{
			return allSharedOpinionModifiers[id];
		}
		throw new NullReferenceException("Trying to get a shared opinion modifier from the database with id " + id + " but the modifier is not loaded");
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		foreach (KeyValuePair<string, SharedOpinionModifier> allSharedOpinionModifier in allSharedOpinionModifiers)
		{
			allSharedOpinionModifier.Value.CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		foreach (KeyValuePair<string, SharedOpinionModifier> allSharedOpinionModifier in allSharedOpinionModifiers)
		{
			allSharedOpinionModifier.Value.CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
