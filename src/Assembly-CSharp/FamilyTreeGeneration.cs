using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;

public class FamilyTreeGeneration : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating_Families");
		for (int i = 0; i < 15; i++)
		{
			FamilyTree familyTree = FamilyTreeGenerator.GenerateFamilyTree(RACE.HUMANS);
			DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(familyTree);
		}
		for (int j = 0; j < 15; j++)
		{
			FamilyTree familyTree2 = FamilyTreeGenerator.GenerateFamilyTree(RACE.ELVES);
			DatabaseManager.Instance.familyTreeDatabase.AddFamilyTree(familyTree2);
		}
		GenerateAdditionalCouples(RACE.HUMANS, data);
		GenerateAdditionalCouples(RACE.ELVES, data);
		yield return null;
	}

	private void GenerateAdditionalCouples(RACE race, MapGenerationData data)
	{
		List<FamilyTree> list = DatabaseManager.Instance.familyTreeDatabase.allFamilyTreesDictionary[race];
		int num = list.Count / 2;
		for (int i = 0; i < num; i++)
		{
			int num2 = i * 2;
			FamilyTree familyTree = list[num2];
			FamilyTree familyTree2 = list.ElementAt(num2 + 1);
			if (familyTree.children.Count != 0 && familyTree2.children.Count != 0)
			{
				PreCharacterData randomElement = CollectionUtilities.GetRandomElement(familyTree.children);
				PreCharacterData compatibleChildFromFamily = GetCompatibleChildFromFamily(randomElement, familyTree2, DatabaseManager.Instance.familyTreeDatabase);
				if (compatibleChildFromFamily != null)
				{
					randomElement.AddRelationship(RELATIONSHIP_TYPE.LOVER, compatibleChildFromFamily);
					compatibleChildFromFamily.AddRelationship(RELATIONSHIP_TYPE.LOVER, randomElement);
					randomElement.SetCompatibility(5, compatibleChildFromFamily);
					compatibleChildFromFamily.SetCompatibility(5, randomElement);
					randomElement.RandomizeOpinion(30, 100, compatibleChildFromFamily);
					compatibleChildFromFamily.RandomizeOpinion(30, 100, randomElement);
				}
			}
		}
	}

	private PreCharacterData GetCompatibleChildFromFamily(PreCharacterData target, FamilyTree familyTree, FamilyTreeDatabase database)
	{
		for (int i = 0; i < familyTree.children.Count; i++)
		{
			PreCharacterData preCharacterData = familyTree.children[i];
			if (RelationshipManager.IsSexuallyCompatible(target.sexuality, preCharacterData.sexuality, target.gender, preCharacterData.gender) && preCharacterData.GetCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER, database) == null)
			{
				return preCharacterData;
			}
		}
		return null;
	}

	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		DatabaseManager.Instance.familyTreeDatabase.Load(saveData);
		yield return null;
	}

	public override void LoadSavedData(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			SaveDataCurrentProgress saveData = obj.saveData;
			DatabaseManager.Instance.familyTreeDatabase.Load(saveData);
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}
