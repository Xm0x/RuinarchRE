using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public abstract class AnimalBurrow : TileObject
{
	public SUMMON_TYPE monsterToSpawn { get; private set; }

	public List<Summon> spawnedMonsters { get; private set; }

	public List<Summon> deadSpawnedMonsters { get; private set; }

	public override Vector2 selectableSize => new Vector2(1.7f, 1.7f);

	public override Vector3 worldPosition
	{
		get
		{
			Vector2 vector = mapVisual.transform.position;
			vector.x += 0.5f;
			vector.y += 0.5f;
			return vector;
		}
	}

	public override Type serializedData => typeof(SaveDataAnimalBurrow);

	public AnimalBurrow(SUMMON_TYPE p_summonType)
	{
		monsterToSpawn = p_summonType;
		spawnedMonsters = new List<Summon>();
		deadSpawnedMonsters = new List<Summon>();
	}

	public AnimalBurrow(SaveDataTileObject data, SUMMON_TYPE p_summonType)
		: base(data)
	{
		monsterToSpawn = p_summonType;
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		SaveDataAnimalBurrow saveDataAnimalBurrow = data as SaveDataAnimalBurrow;
		spawnedMonsters = SaveUtilities.ConvertIDListToMonsters(saveDataAnimalBurrow.spawnedMonsters);
		deadSpawnedMonsters = SaveUtilities.ConvertIDListToMonsters(saveDataAnimalBurrow.deadSpawnedMonsters);
	}

	protected override void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true)
	{
		base.Initialize(tileObjectType, shouldAddCommonAdvertisements);
		base.traitContainer.AddTrait(this, "Indestructible");
		base.traitContainer.AddTrait(this, "Immovable");
		base.traitContainer.RemoveTrait(this, "Flammable");
		RemovePlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT);
		RemovePlayerAction(PLAYER_SKILL_TYPE.POISON);
		RemovePlayerAction(PLAYER_SKILL_TYPE.IGNITE);
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
	}

	public override void OnLoadPlacePOI()
	{
		DefaultProcessOnPlacePOI();
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
		Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		if (p_character is Summon item)
		{
			spawnedMonsters?.Remove(item);
			deadSpawnedMonsters?.Remove(item);
		}
	}

	protected override void SubscribeListeners(bool shouldLock)
	{
		if (!base.hasSubscribedToSignals)
		{
			base.hasSubscribedToSignals = true;
			base.SubscribeListeners(shouldLock);
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied, shouldLock);
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_MARKER_DESTROYED, OnCharacterMarkerDestroyed, shouldLock);
			Messenger.AddListener<Character>(CharacterSignals.ON_CHARACTER_TAMED, OnBeastTamed, shouldLock);
		}
	}

	protected override void UnsubscribeListeners()
	{
		if (base.hasSubscribedToSignals)
		{
			base.hasSubscribedToSignals = false;
			base.UnsubscribeListeners();
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_MARKER_DESTROYED, OnCharacterMarkerDestroyed);
			Messenger.RemoveListener<Character>(CharacterSignals.ON_CHARACTER_TAMED, OnBeastTamed);
		}
	}

	private void OnCharacterDied(Character p_character)
	{
		if (p_character is Summon summon && spawnedMonsters.Remove(summon) && summon.hasMarker)
		{
			deadSpawnedMonsters.Add(summon);
		}
	}

	private void OnCharacterMarkerDestroyed(Character p_character)
	{
		if (p_character is Summon { isDead: not false } summon)
		{
			deadSpawnedMonsters.Remove(summon);
		}
	}

	private void OnBeastTamed(Character p_character)
	{
		if (p_character is Summon item)
		{
			spawnedMonsters.Remove(item);
		}
	}

	protected virtual void OnGameLoaded()
	{
		Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
	}

	protected void CreateNewMonster(List<LocationGridTile> p_locationChoices = null)
	{
		Summon summon = CharacterManager.Instance.CreateNewSummon(monsterToSpawn, FactionManager.Instance.GetDefaultFactionForMonster(monsterToSpawn), null, gridTileLocation.parentMap.region);
		Area area = gridTileLocation.area;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		if (p_locationChoices != null)
		{
			list.AddRange(p_locationChoices);
		}
		else
		{
			for (int i = 0; i < area.gridTileComponent.passableTiles.Count; i++)
			{
				LocationGridTile locationGridTile = area.gridTileComponent.passableTiles[i];
				if (locationGridTile.structure is Wilderness)
				{
					list.Add(locationGridTile);
				}
			}
		}
		if (list.Count > 0)
		{
			LocationGridTile randomElement = CollectionUtilities.GetRandomElement(list);
			CharacterManager.Instance.PlaceSummonInitially(summon, randomElement);
			summon.SetTerritory(area, returnHome: false);
			spawnedMonsters.Add(summon);
			p_locationChoices?.Remove(randomElement);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	public Summon GetRandomAliveSpawnedMonsterFor(Character p_character)
	{
		Summon result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < spawnedMonsters.Count; i++)
		{
			Summon summon = spawnedMonsters[i];
			LocationGridTile locationGridTile = summon.gridTileLocation;
			if (!summon.isDead && locationGridTile != null && summon.hasMarker && !summon.isBeingSeized && (locationGridTile.area == gridTileLocation.area || locationGridTile.area.neighbourComponent.HasNeighbour(gridTileLocation.area)) && p_character.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile))
			{
				list.Add(summon);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)] as Summon;
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Summon GetRandomDeadSpawnedMonsterForAnimalHaulRelativeTo(Character p_actor)
	{
		LocationGridTile locationGridTile = p_actor.gridTileLocation;
		Summon result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < deadSpawnedMonsters.Count; i++)
		{
			Summon summon = deadSpawnedMonsters[i];
			LocationGridTile locationGridTile2 = summon.gridTileLocation;
			if (summon.isDead && locationGridTile2 != null && locationGridTile != null && summon.hasMarker && locationGridTile2.structure.structureType != STRUCTURE_TYPE.KENNEL && locationGridTile2.structure.structureType != STRUCTURE_TYPE.CITY_CENTER && locationGridTile.area.GetAreaDistanceTo(locationGridTile2.area) < 3 && !locationGridTile2.IsPartOfSettlement(p_actor.homeSettlement) && p_actor.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile2))
			{
				list.Add(summon);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)] as Summon;
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public bool HasAliveSpawnedMonster()
	{
		for (int i = 0; i < spawnedMonsters.Count; i++)
		{
			Summon summon = spawnedMonsters[i];
			LocationGridTile locationGridTile = summon.gridTileLocation;
			if (!summon.isDead && locationGridTile != null && summon.hasMarker)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasDeadOrAliveSpawnedMonster(Summon p_monster)
	{
		for (int i = 0; i < spawnedMonsters.Count; i++)
		{
			Summon summon = spawnedMonsters[i];
			if (p_monster == summon && !summon.isInLimbo)
			{
				return true;
			}
		}
		for (int j = 0; j < deadSpawnedMonsters.Count; j++)
		{
			Summon summon2 = deadSpawnedMonsters[j];
			if (p_monster == summon2 && !summon2.isInLimbo)
			{
				return true;
			}
		}
		return false;
	}

	public override string GetAdditionalTestingData()
	{
		return base.GetAdditionalTestingData() + "\n\tSpawned Monsters: " + spawnedMonsters.ComafyList();
	}

	public override void RightSelectAction()
	{
	}

	public override void MiddleSelectAction()
	{
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		base.actions = new List<PLAYER_SKILL_TYPE>();
	}

	public override void LeftSelectAction()
	{
		UIManager.Instance.ShowStructureInfo(gridTileLocation.structure);
	}

	public override bool CanBeSelected()
	{
		return true;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		deadSpawnedMonsters.Contains(p_character);
	}
}
