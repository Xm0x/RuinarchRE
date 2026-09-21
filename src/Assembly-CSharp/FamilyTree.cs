using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

[Serializable]
public class FamilyTree
{
	public PreCharacterData father { get; set; }

	public PreCharacterData mother { get; set; }

	public List<PreCharacterData> children { get; set; }

	public List<PreCharacterData> allFamilyMembers { get; set; }

	public RACE race => father.race;

	public FamilyTree()
	{
		allFamilyMembers = new List<PreCharacterData>();
	}

	public FamilyTree(PreCharacterData _father, PreCharacterData _mother, List<PreCharacterData> _children)
		: this()
	{
		father = _father;
		mother = _mother;
		children = _children;
		allFamilyMembers.Add(father);
		allFamilyMembers.Add(mother);
		allFamilyMembers.AddRange(children);
	}

	public void Load()
	{
		for (int i = 0; i < allFamilyMembers.Count; i++)
		{
			PreCharacterData preCharacterData = allFamilyMembers[i];
			Utilities.SetID(preCharacterData, preCharacterData.id);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
