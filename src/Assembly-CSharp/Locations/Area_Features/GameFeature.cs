using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

namespace Locations.Area_Features;

public class GameFeature : AreaFeature
{
	private const int MaxAnimals = 6;

	private Area owner;

	public static readonly SUMMON_TYPE[] spawnChoices = new SUMMON_TYPE[3]
	{
		SUMMON_TYPE.Pig,
		SUMMON_TYPE.Sheep,
		SUMMON_TYPE.Chicken
	};

	public bool isGeneratingPerHour { get; private set; }

	public SUMMON_TYPE animalTypeBeingSpawned { get; private set; }

	public List<Animal> ownedAnimals { get; private set; }

	public override Type serializedData => typeof(SaveDataGameFeature);

	public GameFeature()
	{
		base.name = "Game";
		base.description = "Hunters can obtain food here.";
		ownedAnimals = new List<Animal>();
		SetSpawnType(CollectionUtilities.GetRandomElement(spawnChoices));
	}

	public override void GameStartActions(Area p_area)
	{
		owner = p_area;
		List<Character> list = RuinarchListPool<Character>.Claim();
		p_area.locationCharacterTracker.PopulateAnimalsListInsideHex(list, includeBeingSeized: true);
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Animal animal = list[i] as Animal;
				AddOwnedAnimal(animal);
			}
		}
		RuinarchListPool<Character>.Release(list);
		if (ownedAnimals.Count < 6)
		{
			int num = 6 - ownedAnimals.Count;
			for (int j = 0; j < num; j++)
			{
				SpawnNewAnimal();
			}
		}
	}

	public override void LoadedGameStartActions(Area p_area)
	{
		owner = p_area;
	}

	public override void OnRemoveFeature(Area p_area)
	{
		base.OnRemoveFeature(p_area);
		Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		if (ownedAnimals != null)
		{
			ownedAnimals.Clear();
			ownedAnimals = null;
		}
	}

	private void OnCharacterDied(Character character)
	{
		if (character is Animal animal)
		{
			RemoveOwnedAnimal(animal);
		}
	}

	private void DisconnectFromCharacter(Character p_character)
	{
		if (p_character is Animal item)
		{
			ownedAnimals.Remove(item);
		}
	}

	public void SetSpawnType(SUMMON_TYPE summon)
	{
		animalTypeBeingSpawned = summon;
	}

	private void RemoveOwnedAnimal(Animal animal)
	{
		if (ownedAnimals.Remove(animal))
		{
			if (ownedAnimals.Count == 0)
			{
				Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
				Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
			}
			if (ownedAnimals.Count < 6 && !isGeneratingPerHour)
			{
				isGeneratingPerHour = true;
				Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
			}
		}
	}

	private void AddOwnedAnimal(Animal animal)
	{
		ownedAnimals.Add(animal);
		if (ownedAnimals.Count == 1)
		{
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
			Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		}
		if (ownedAnimals.Count >= 6)
		{
			isGeneratingPerHour = false;
			Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
		}
	}

	private void OnHourStarted()
	{
		if (GameManager.Instance.currentTick == 120)
		{
			int num = 6 - ownedAnimals.Count;
			if (num < 0)
			{
				num = 0;
			}
			int num2 = Mathf.Min(2, num);
			for (int i = 0; i < num2; i++)
			{
				SpawnNewAnimal();
			}
		}
	}

	private void SpawnNewAnimal()
	{
		LocationGridTile randomTileThatIsPassableAndOpenSpace = owner.gridTileComponent.GetRandomTileThatIsPassableAndOpenSpace();
		if (randomTileThatIsPassableAndOpenSpace != null)
		{
			Animal animal = CharacterManager.Instance.CreateNewSummon(animalTypeBeingSpawned, FactionManager.Instance.wildMonsterFaction, null, owner.region) as Animal;
			CharacterManager.Instance.PlaceSummonInitially(animal, randomTileThatIsPassableAndOpenSpace);
			animal.SetTerritory(owner, GameManager.Instance.gameHasStarted);
			AddOwnedAnimal(animal);
		}
	}

	public void LoadAnimals(List<string> p_ids)
	{
		for (int i = 0; i < p_ids.Count; i++)
		{
			string id = p_ids[i];
			if (DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(id) is Animal animal)
			{
				AddOwnedAnimal(animal);
			}
		}
	}

	public void LoadGeneration(bool p_isGeneratingPerHour)
	{
		isGeneratingPerHour = p_isGeneratingPerHour;
		if (isGeneratingPerHour)
		{
			Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
		}
	}

	public override string GetTestingData()
	{
		return "Game feature spawned animals: " + ownedAnimals.ComafyList();
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		ownedAnimals.Contains(p_character);
	}
}
