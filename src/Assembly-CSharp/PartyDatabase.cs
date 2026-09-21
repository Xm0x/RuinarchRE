using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class PartyDatabase
{
	public Dictionary<string, Party> allParties { get; }

	public PartyDatabase()
	{
		allParties = new Dictionary<string, Party>();
	}

	public void AddParty(Party party)
	{
		if (!allParties.ContainsKey(party.persistentID))
		{
			allParties.Add(party.persistentID, party);
		}
	}

	public void RemoveParty(Party party)
	{
		if (allParties.ContainsKey(party.persistentID))
		{
			allParties.Remove(party.persistentID);
		}
	}

	public Party GetPartyByPersistentID(string id)
	{
		if (allParties.ContainsKey(id))
		{
			return allParties[id];
		}
		throw new NullReferenceException("Trying to get a party from the database with id " + id + " but the party is not loaded");
	}

	public Party GetPartyByPersistentIDSafe(string id)
	{
		if (allParties.ContainsKey(id))
		{
			return allParties[id];
		}
		return null;
	}

	public Party GetPartyByName(string name)
	{
		foreach (Party value in allParties.Values)
		{
			if (value.partyName == name)
			{
				return value;
			}
		}
		return null;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		foreach (KeyValuePair<string, Party> allParty in allParties)
		{
			allParty.Value.CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		foreach (KeyValuePair<string, Party> allParty in allParties)
		{
			allParty.Value.CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
