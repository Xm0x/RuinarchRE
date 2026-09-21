using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public static class FamilyTreeGenerator
{
	private static WeightedDictionary<SEXUALITY> parentsSexuality = new WeightedDictionary<SEXUALITY>(new Dictionary<SEXUALITY, int>
	{
		{
			SEXUALITY.STRAIGHT,
			90
		},
		{
			SEXUALITY.BISEXUAL,
			10
		}
	});

	private static WeightedDictionary<int> childCountWeights = new WeightedDictionary<int>(new Dictionary<int, int>
	{
		{ 0, 10 },
		{ 1, 20 },
		{ 2, 50 },
		{ 3, 20 }
	});

	private static WeightedDictionary<SEXUALITY> childSexuality = new WeightedDictionary<SEXUALITY>(new Dictionary<SEXUALITY, int>
	{
		{
			SEXUALITY.STRAIGHT,
			80
		},
		{
			SEXUALITY.BISEXUAL,
			10
		},
		{
			SEXUALITY.GAY,
			10
		}
	});

	public static FamilyTree GenerateFamilyTree(RACE race)
	{
		HAIR_COLOR randomHairColorByRace = GetRandomHairColorByRace(race);
		PreCharacterData preCharacterData = new PreCharacterData(race, GENDER.MALE, parentsSexuality);
		PreCharacterData preCharacterData2 = new PreCharacterData(race, GENDER.FEMALE, parentsSexuality);
		preCharacterData.SetHairColorType(randomHairColorByRace);
		preCharacterData2.SetHairColorType(randomHairColorByRace);
		int compatibility = Random.Range(3, 6);
		preCharacterData.SetCompatibility(compatibility, preCharacterData2);
		preCharacterData2.SetCompatibility(compatibility, preCharacterData);
		preCharacterData.AddRelationship(RELATIONSHIP_TYPE.LOVER, preCharacterData2);
		preCharacterData2.AddRelationship(RELATIONSHIP_TYPE.LOVER, preCharacterData);
		int num = childCountWeights.PickRandomElementGivenWeights();
		List<PreCharacterData> list = new List<PreCharacterData>();
		for (int i = 0; i < num; i++)
		{
			PreCharacterData preCharacterData3 = new PreCharacterData(race, Utilities.GetRandomGender(), childSexuality);
			preCharacterData3.SetHairColorType(randomHairColorByRace);
			list.Add(preCharacterData3);
		}
		for (int j = 0; j < list.Count; j++)
		{
			PreCharacterData preCharacterData4 = list[j];
			int compatibility2 = Random.Range(2, 5);
			int compatibility3 = Random.Range(2, 5);
			preCharacterData4.SetCompatibility(compatibility2, preCharacterData);
			preCharacterData4.SetCompatibility(compatibility3, preCharacterData2);
			preCharacterData.SetCompatibility(compatibility2, preCharacterData4);
			preCharacterData2.SetCompatibility(compatibility3, preCharacterData4);
			preCharacterData4.AddRelationship(RELATIONSHIP_TYPE.PARENT, preCharacterData);
			preCharacterData4.AddRelationship(RELATIONSHIP_TYPE.PARENT, preCharacterData2);
			preCharacterData.AddRelationship(RELATIONSHIP_TYPE.CHILD, preCharacterData4);
			preCharacterData2.AddRelationship(RELATIONSHIP_TYPE.CHILD, preCharacterData4);
			for (int k = 0; k < list.Count; k++)
			{
				PreCharacterData preCharacterData5 = list[k];
				if (preCharacterData4 != preCharacterData5)
				{
					preCharacterData4.GetOrInitializeRelationshipWith(preCharacterData5);
					preCharacterData5.GetOrInitializeRelationshipWith(preCharacterData4);
					int num2 = preCharacterData5.GetCompatibilityWith(preCharacterData4);
					if (num2 == -1)
					{
						num2 = Random.Range(2, 5);
					}
					preCharacterData4.SetCompatibility(num2, preCharacterData5);
					preCharacterData4.AddRelationship(RELATIONSHIP_TYPE.SIBLING, preCharacterData5);
				}
			}
		}
		FamilyTree familyTree = new FamilyTree(preCharacterData, preCharacterData2, list);
		for (int l = 0; l < familyTree.allFamilyMembers.Count; l++)
		{
			PreCharacterData preCharacterData6 = familyTree.allFamilyMembers[l];
			for (int m = 0; m < familyTree.allFamilyMembers.Count; m++)
			{
				PreCharacterData preCharacterData7 = familyTree.allFamilyMembers[m];
				if (preCharacterData6 == preCharacterData7)
				{
					continue;
				}
				if ((preCharacterData6 == preCharacterData && preCharacterData7 == preCharacterData2) || (preCharacterData6 == preCharacterData2 && preCharacterData7 == preCharacterData))
				{
					preCharacterData6.RandomizeOpinion(30, 100, preCharacterData7);
					continue;
				}
				int compatibilityWith = preCharacterData6.GetCompatibilityWith(preCharacterData7);
				if (compatibilityWith == 2)
				{
					preCharacterData6.RandomizeOpinion(-40, 20, preCharacterData7);
				}
				else if (compatibilityWith == 3)
				{
					preCharacterData6.RandomizeOpinion(10, 50, preCharacterData7);
				}
				else if (compatibilityWith == 4)
				{
					preCharacterData6.RandomizeOpinion(30, 70, preCharacterData7);
				}
				else if (compatibilityWith >= 5)
				{
					preCharacterData6.RandomizeOpinion(50, 100, preCharacterData7);
				}
			}
		}
		return familyTree;
	}

	public static HAIR_COLOR GetRandomHairColorByRace(RACE p_raceType)
	{
		return p_raceType switch
		{
			RACE.HUMANS => GetRandomHumanHairColor(), 
			RACE.ELVES => GetRandomElfHairColor(), 
			_ => HAIR_COLOR.Brunette, 
		};
	}

	private static HAIR_COLOR GetRandomHumanHairColor()
	{
		return Random.Range(0, 3) switch
		{
			0 => HAIR_COLOR.Brunette, 
			1 => HAIR_COLOR.Blonde, 
			_ => HAIR_COLOR.Redhead, 
		};
	}

	private static HAIR_COLOR GetRandomElfHairColor()
	{
		if (Random.Range(0, 2) == 0)
		{
			return HAIR_COLOR.Brunette;
		}
		return HAIR_COLOR.White;
	}
}
