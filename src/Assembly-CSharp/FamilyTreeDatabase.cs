using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

[Serializable]
public class FamilyTreeDatabase
{
	public Dictionary<RACE, List<FamilyTree>> allFamilyTreesDictionary;

	public FamilyTreeDatabase()
	{
		allFamilyTreesDictionary = new Dictionary<RACE, List<FamilyTree>>();
	}

	public void AddFamilyTree(FamilyTree familyTree)
	{
		if (!allFamilyTreesDictionary.ContainsKey(familyTree.race))
		{
			allFamilyTreesDictionary.Add(familyTree.race, new List<FamilyTree>());
		}
		allFamilyTreesDictionary[familyTree.race].Add(familyTree);
	}

	public PreCharacterData GetCharacterWithID(int id)
	{
		foreach (KeyValuePair<RACE, List<FamilyTree>> item in allFamilyTreesDictionary)
		{
			for (int i = 0; i < item.Value.Count; i++)
			{
				FamilyTree familyTree = item.Value[i];
				for (int j = 0; j < familyTree.allFamilyMembers.Count; j++)
				{
					PreCharacterData preCharacterData = familyTree.allFamilyMembers[j];
					if (preCharacterData.id == id)
					{
						return preCharacterData;
					}
				}
			}
		}
		return null;
	}

	public void ForcePopulateAllUnspawnedCharactersThatFitFaction(List<PreCharacterData> availableCharacters, RACE race, Faction faction)
	{
		List<FamilyTree> list = RuinarchListPool<FamilyTree>.Claim();
		if (faction.factionType.type == FACTION_TYPE.Elven_Kingdom || faction.factionType.type == FACTION_TYPE.Wiccans)
		{
			list.AddRange(allFamilyTreesDictionary[RACE.ELVES]);
		}
		else if (faction.factionType.type == FACTION_TYPE.Human_Empire || faction.factionType.type == FACTION_TYPE.Divine_Church)
		{
			list.AddRange(allFamilyTreesDictionary[RACE.HUMANS]);
		}
		else
		{
			list = RuinarchListPool<FamilyTree>.Claim();
			foreach (KeyValuePair<RACE, List<FamilyTree>> item in allFamilyTreesDictionary)
			{
				list.AddRange(item.Value);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			FamilyTree familyTree = list[i];
			for (int j = 0; j < familyTree.allFamilyMembers.Count; j++)
			{
				PreCharacterData preCharacterData = familyTree.allFamilyMembers[j];
				if (!preCharacterData.hasBeenSpawned && faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(preCharacterData))
				{
					availableCharacters.Add(preCharacterData);
				}
			}
		}
		RuinarchListPool<FamilyTree>.Release(list);
		if (availableCharacters.Count > 0)
		{
			return;
		}
		FamilyTree familyTree2 = FamilyTreeGenerator.GenerateFamilyTree(race);
		AddFamilyTree(familyTree2);
		for (int k = 0; k < familyTree2.allFamilyMembers.Count; k++)
		{
			PreCharacterData preCharacterData2 = familyTree2.allFamilyMembers[k];
			if (faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(preCharacterData2))
			{
				availableCharacters.Add(preCharacterData2);
			}
		}
	}

	public void ForcePopulateAllUnspawnedCharactersThatFitRace(List<PreCharacterData> availableCharacters, RACE race)
	{
		List<FamilyTree> list = allFamilyTreesDictionary[race];
		for (int i = 0; i < list.Count; i++)
		{
			FamilyTree familyTree = list[i];
			for (int j = 0; j < familyTree.allFamilyMembers.Count; j++)
			{
				PreCharacterData preCharacterData = familyTree.allFamilyMembers[j];
				if (!preCharacterData.hasBeenSpawned)
				{
					availableCharacters.Add(preCharacterData);
				}
			}
		}
		if (availableCharacters.Count <= 0)
		{
			FamilyTree familyTree2 = FamilyTreeGenerator.GenerateFamilyTree(race);
			AddFamilyTree(familyTree2);
			for (int k = 0; k < familyTree2.allFamilyMembers.Count; k++)
			{
				PreCharacterData item = familyTree2.allFamilyMembers[k];
				availableCharacters.Add(item);
			}
		}
	}

	public void Load(SaveDataCurrentProgress saveDataCurrentProgress)
	{
		allFamilyTreesDictionary = saveDataCurrentProgress.familyTreeDatabase.allFamilyTreesDictionary;
		foreach (KeyValuePair<RACE, List<FamilyTree>> item in allFamilyTreesDictionary)
		{
			for (int i = 0; i < item.Value.Count; i++)
			{
				item.Value[i].Load();
			}
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		foreach (KeyValuePair<RACE, List<FamilyTree>> item in allFamilyTreesDictionary)
		{
			for (int i = 0; i < item.Value.Count; i++)
			{
				item.Value[i].CheckIfStructureIsStillReferenced(p_structure);
			}
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		foreach (KeyValuePair<RACE, List<FamilyTree>> item in allFamilyTreesDictionary)
		{
			for (int i = 0; i < item.Value.Count; i++)
			{
				item.Value[i].CheckIfCharacterIsStillReferenced(p_character);
			}
		}
	}
}
