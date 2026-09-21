using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class CharacterDatabase
{
	public Dictionary<string, Character> allCharacters { get; }

	public Dictionary<string, Character> limboCharacters { get; }

	public List<Character> allCharactersList { get; }

	public List<Character> limboCharactersList { get; }

	public List<Character> aliveVillagersList { get; }

	public CharacterDatabase()
	{
		allCharacters = new Dictionary<string, Character>();
		limboCharacters = new Dictionary<string, Character>();
		allCharactersList = new List<Character>();
		limboCharactersList = new List<Character>();
		aliveVillagersList = new List<Character>();
		Messenger.AddListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterChangedClass);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOMES_MINION_OR_SUMMON, OnCharacterBecameSummonOrMinion);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOMES_NON_MINION_OR_SUMMON, OnCharacterNoLongerSummonOrMinion);
	}

	private void OnCharacterNoLongerSummonOrMinion(Character p_character)
	{
		if (IsConsideredVillager(p_character) && !p_character.isDead)
		{
			AddToAliveVillagersList(p_character);
		}
	}

	private void OnCharacterDied(Character p_character)
	{
		RemoveFromAliveVillagersList(p_character);
	}

	private void OnCharacterBecameSummonOrMinion(Character p_character)
	{
		RemoveFromAliveVillagersList(p_character);
	}

	private void OnCharacterChangedClass(Character p_character, CharacterClass p_previousClass, CharacterClass p_newClass)
	{
		if (IsConsideredVillager(p_character) && !p_character.isDead)
		{
			AddToAliveVillagersList(p_character);
		}
		else
		{
			RemoveFromAliveVillagersList(p_character);
		}
	}

	public void RemoveFromAliveVillagersList(Character p_character)
	{
		if (aliveVillagersList.Remove(p_character))
		{
			Messenger.Broadcast(CharacterSignals.CHARACTER_REMOVED_FROM_ALIVE_VILLAGERS, p_character);
		}
	}

	private void AddToAliveVillagersList(Character p_character)
	{
		if (!aliveVillagersList.Contains(p_character))
		{
			aliveVillagersList.Add(p_character);
			Messenger.Broadcast(CharacterSignals.CHARACTER_ADDED_TO_ALIVE_VILLAGERS, p_character);
		}
	}

	private bool IsConsideredVillager(Character p_character)
	{
		if (p_character.isNormalCharacter)
		{
			return p_character.race != RACE.RATMAN;
		}
		return false;
	}

	internal void AddCharacter(Character character, bool addToAliveVillagersList = true)
	{
		allCharacters.Add(character.persistentID, character);
		allCharactersList.Add(character);
		if (addToAliveVillagersList && IsConsideredVillager(character) && !character.isDead)
		{
			AddToAliveVillagersList(character);
		}
	}

	internal bool RemoveCharacter(Character character, bool removeFromAliveVillagersList = true)
	{
		allCharacters.Remove(character.persistentID);
		if (removeFromAliveVillagersList)
		{
			RemoveFromAliveVillagersList(character);
		}
		return allCharactersList.Remove(character);
	}

	internal void AddLimboCharacter(Character character)
	{
		limboCharacters.Add(character.persistentID, character);
		limboCharactersList.Add(character);
	}

	internal bool RemoveLimboCharacter(Character character)
	{
		limboCharacters.Remove(character.persistentID);
		return limboCharactersList.Remove(character);
	}

	internal Character GetCharacterByPersistentID(string id)
	{
		if (id != null)
		{
			if (DatabaseManager.Instance.characterDatabase.allCharacters.TryGetValue(id, out var value))
			{
				return value;
			}
			if (DatabaseManager.Instance.characterDatabase.limboCharacters.TryGetValue(id, out value))
			{
				return value;
			}
		}
		return null;
	}

	internal Character GetCharacterByID(int id)
	{
		for (int i = 0; i < allCharactersList.Count; i++)
		{
			Character character = allCharactersList[i];
			if (character.id == id)
			{
				return character;
			}
		}
		for (int j = 0; j < limboCharactersList.Count; j++)
		{
			Character character2 = limboCharactersList[j];
			if (character2.id == id)
			{
				return character2;
			}
		}
		return null;
	}

	public void CleanUpCharacter(Character p_character)
	{
		allCharacters.Remove(p_character.persistentID);
		limboCharacters.Remove(p_character.persistentID);
		allCharactersList.Remove(p_character);
		limboCharactersList.Remove(p_character);
		aliveVillagersList.Remove(p_character);
		p_character.CleanUp();
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		for (int i = 0; i < allCharactersList.Count; i++)
		{
			allCharactersList[i].CheckIfStructureIsStillReferenced(p_structure);
		}
		for (int j = 0; j < limboCharactersList.Count; j++)
		{
			limboCharactersList[j].CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		for (int i = 0; i < allCharactersList.Count; i++)
		{
			Character character = allCharactersList[i];
			if (character != p_character)
			{
				character.CheckIfCharacterIsStillReferenced(p_character);
			}
		}
		for (int j = 0; j < limboCharactersList.Count; j++)
		{
			Character character2 = limboCharactersList[j];
			if (character2 != p_character)
			{
				character2.CheckIfCharacterIsStillReferenced(p_character);
			}
		}
	}
}
