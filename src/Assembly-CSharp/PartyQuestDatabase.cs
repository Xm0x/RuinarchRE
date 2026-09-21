using System;
using System.Collections.Generic;

public class PartyQuestDatabase
{
	public Dictionary<string, PartyQuest> allPartyQuests { get; }

	public PartyQuestDatabase()
	{
		allPartyQuests = new Dictionary<string, PartyQuest>();
	}

	public void AddPartyQuest(PartyQuest party)
	{
		if (!allPartyQuests.ContainsKey(party.persistentID))
		{
			allPartyQuests.Add(party.persistentID, party);
		}
	}

	public void RemovePartyQuest(PartyQuest party)
	{
		if (allPartyQuests.ContainsKey(party.persistentID))
		{
			allPartyQuests.Remove(party.persistentID);
		}
	}

	public PartyQuest GetPartyQuestByPersistentID(string id)
	{
		if (allPartyQuests.ContainsKey(id))
		{
			return allPartyQuests[id];
		}
		throw new NullReferenceException("Trying to get a party quest from the database with id " + id + " but the party is not loaded");
	}
}
