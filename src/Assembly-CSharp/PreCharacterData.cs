using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

[Serializable]
public class PreCharacterData
{
	public int id { get; set; }

	public RACE race { get; set; }

	public GENDER gender { get; set; }

	public string firstName { get; set; }

	public SEXUALITY sexuality { get; set; }

	public HAIR_COLOR hairColorType { get; set; }

	public Dictionary<int, PreCharacterRelationship> relationships { get; set; }

	public bool hasBeenSpawned { get; set; }

	public string fullName => firstName ?? "";

	public PreCharacterData()
	{
		relationships = new Dictionary<int, PreCharacterRelationship>();
	}

	public PreCharacterData(RACE _race, GENDER _gender, WeightedDictionary<SEXUALITY> _sexualityWeights)
		: this()
	{
		id = Utilities.SetID(this);
		race = _race;
		gender = _gender;
		SetName(RandomNameGenerator.GenerateRandomName(_race, _gender));
		sexuality = _sexualityWeights.PickRandomElementGivenWeights();
	}

	private void SetName(string name)
	{
		firstName = name;
		RandomNameGenerator.RemoveNameAsAvailable(gender, race, firstName);
	}

	public void SetCompatibility(int compatibility, PreCharacterData characterData)
	{
		GetOrInitializeRelationshipWith(characterData).SetCompatibility(compatibility);
	}

	public void RandomizeOpinion(int lowerBound, int upperBound, PreCharacterData characterData)
	{
		GetOrInitializeRelationshipWith(characterData).SetOpinion(UnityEngine.Random.Range(lowerBound, upperBound + 1));
	}

	public void AddRelationship(RELATIONSHIP_TYPE relationshipType, PreCharacterData characterData)
	{
		GetOrInitializeRelationshipWith(characterData).AddRelationship(relationshipType);
	}

	public PreCharacterRelationship GetOrInitializeRelationshipWith(PreCharacterData characterData)
	{
		if (!relationships.ContainsKey(characterData.id))
		{
			relationships.Add(characterData.id, new PreCharacterRelationship());
		}
		return relationships[characterData.id];
	}

	public PreCharacterData GetCharacterWithRelationship(RELATIONSHIP_TYPE relationshipType, FamilyTreeDatabase database)
	{
		foreach (KeyValuePair<int, PreCharacterRelationship> relationship in relationships)
		{
			if (relationship.Value.relationships.Contains(relationshipType))
			{
				int key = relationship.Key;
				return database.GetCharacterWithID(key);
			}
		}
		return null;
	}

	public int GetCompatibilityWith(PreCharacterData character)
	{
		return relationships[character.id].compatibility;
	}

	public void SetHasBeenSpawned()
	{
		hasBeenSpawned = true;
	}

	public void SetHairColorType(HAIR_COLOR p_hairColorType)
	{
		hairColorType = p_hairColorType;
	}
}
