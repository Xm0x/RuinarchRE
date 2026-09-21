using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

namespace Databases;

public class SettlementDatabase
{
	public Dictionary<string, BaseSettlement> settlementsByGUID { get; }

	public List<BaseSettlement> allSettlements { get; }

	public List<NPCSettlement> allNonPlayerSettlements { get; }

	public SettlementDatabase()
	{
		settlementsByGUID = new Dictionary<string, BaseSettlement>();
		allSettlements = new List<BaseSettlement>();
		allNonPlayerSettlements = new List<NPCSettlement>();
	}

	public void RegisterSettlement(BaseSettlement baseSettlement)
	{
		settlementsByGUID.Add(baseSettlement.persistentID, baseSettlement);
		allSettlements.Add(baseSettlement);
		if (baseSettlement is NPCSettlement item)
		{
			allNonPlayerSettlements.Add(item);
		}
	}

	public void UnRegisterSettlement(BaseSettlement baseSettlement)
	{
		settlementsByGUID.Remove(baseSettlement.persistentID);
		allSettlements.Remove(baseSettlement);
		if (baseSettlement is NPCSettlement item)
		{
			allNonPlayerSettlements.Remove(item);
		}
	}

	public BaseSettlement GetSettlementByID(int id)
	{
		for (int i = 0; i < allSettlements.Count; i++)
		{
			BaseSettlement baseSettlement = allSettlements[i];
			if (baseSettlement.id == id)
			{
				return baseSettlement;
			}
		}
		return null;
	}

	public BaseSettlement GetSettlementByPersistentID(string id)
	{
		if (settlementsByGUID.ContainsKey(id))
		{
			return settlementsByGUID[id];
		}
		throw new Exception("There is no settlement with persistent ID " + id);
	}

	public BaseSettlement GetSettlementByPersistentIDSafe(string id)
	{
		if (settlementsByGUID.ContainsKey(id))
		{
			return settlementsByGUID[id];
		}
		return null;
	}

	public BaseSettlement GetSettlementByName(string name)
	{
		for (int i = 0; i < allSettlements.Count; i++)
		{
			BaseSettlement baseSettlement = allSettlements[i];
			if (baseSettlement.name.Equals(name))
			{
				return baseSettlement;
			}
		}
		return null;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		for (int i = 0; i < allSettlements.Count; i++)
		{
			allSettlements[i].CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		for (int i = 0; i < allSettlements.Count; i++)
		{
			allSettlements[i].CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
