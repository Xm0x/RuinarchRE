using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;

[Serializable]
public class PartyStructureData
{
	public List<Character> deployedSummons = new List<Character>();

	public Character leader { get; private set; }

	public IStoredTarget target { get; private set; }

	public LocationStructure targetStructure { get; private set; }

	public int deployedSummonCount => deployedSummons.Count;

	public void ClearAllData()
	{
		deployedSummons.Clear();
		leader = null;
		target = null;
		targetStructure = null;
	}

	public void SetTargetStructure(LocationStructure p_structure)
	{
		targetStructure = p_structure;
	}

	public void SetTarget(IStoredTarget p_target)
	{
		target = p_target;
	}

	public void SetMinionLeader(Character p_character)
	{
		leader = p_character;
	}

	public Character GetPartyLeader()
	{
		if (leader != null)
		{
			return leader;
		}
		return deployedSummons.First();
	}

	public string GetTestingData()
	{
		return string.Concat(string.Concat(string.Concat(string.Empty + "Deployed Summons: " + deployedSummons?.ComafyList(), "\nMinion Leader: ", leader?.name), "\nTarget: ", target?.name), "\nTarget Structure: ", targetStructure?.name);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = target;
		_ = targetStructure;
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		deployedSummons.Contains(p_character);
		_ = leader;
		_ = target;
	}
}
