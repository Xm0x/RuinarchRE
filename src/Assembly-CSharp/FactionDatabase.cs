using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class FactionDatabase
{
	public Dictionary<string, Faction> factionsByGUID { get; private set; }

	public List<Faction> allFactionsList { get; private set; }

	public FactionDatabase()
	{
		factionsByGUID = new Dictionary<string, Faction>();
		allFactionsList = new List<Faction>();
	}

	public void RegisterFaction(Faction faction)
	{
		factionsByGUID.Add(faction.persistentID, faction);
		allFactionsList.Add(faction);
	}

	public void UnRegisterFaction(Faction faction)
	{
		factionsByGUID.Remove(faction.persistentID);
		allFactionsList.Remove(faction);
	}

	public Faction GetRandomMajorNonPlayerFaction()
	{
		List<Faction> list = RuinarchListPool<Faction>.Claim();
		for (int i = 0; i < allFactionsList.Count; i++)
		{
			Faction faction = allFactionsList[i];
			if (faction.isMajorNonPlayer && faction.characters.Any((Character c) => !c.isDead))
			{
				list.Add(faction);
			}
		}
		if (list.Count > 0)
		{
			Faction randomElement = CollectionUtilities.GetRandomElement(list);
			RuinarchListPool<Faction>.Release(list);
			return randomElement;
		}
		RuinarchListPool<Faction>.Release(list);
		return null;
	}

	public Faction GetFactionBasedOnID(int id)
	{
		for (int i = 0; i < allFactionsList.Count; i++)
		{
			if (allFactionsList[i].id == id)
			{
				return allFactionsList[i];
			}
		}
		return null;
	}

	public Faction GetFactionBasedOnPersistentID(string id)
	{
		if (factionsByGUID.ContainsKey(id))
		{
			return factionsByGUID[id];
		}
		throw new Exception("There was no faction with persistent id " + id);
	}

	public Faction GetFactionByPersistentID(string persistentID)
	{
		if (factionsByGUID.ContainsKey(persistentID))
		{
			return factionsByGUID[persistentID];
		}
		return null;
	}

	public Faction GetFactionBasedOnName(string name)
	{
		for (int i = 0; i < allFactionsList.Count; i++)
		{
			if (allFactionsList[i].name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
			{
				return allFactionsList[i];
			}
		}
		return null;
	}

	public List<Faction> GetMajorFactionWithRace(RACE race)
	{
		List<Faction> list = null;
		for (int i = 0; i < allFactionsList.Count; i++)
		{
			Faction faction = allFactionsList[i];
			if (faction.race == race && faction.isMajorFaction)
			{
				if (list == null)
				{
					list = new List<Faction>();
				}
				list.Add(faction);
			}
		}
		return list;
	}

	public List<Faction> GetFactionsWithFactionType(params FACTION_TYPE[] p_factionType)
	{
		List<Faction> list = null;
		for (int i = 0; i < allFactionsList.Count; i++)
		{
			Faction faction = allFactionsList[i];
			if (p_factionType.Contains(faction.factionType.type))
			{
				if (list == null)
				{
					list = new List<Faction>();
				}
				list.Add(faction);
			}
		}
		return list;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		for (int i = 0; i < allFactionsList.Count; i++)
		{
			allFactionsList[i].CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		for (int i = 0; i < allFactionsList.Count; i++)
		{
			allFactionsList[i].CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
