using System;
using System.Collections.Generic;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class LichGraveyard : ManMadeStructure
{
	public List<string> spawnedMonstersID { get; private set; }

	public int spawnRate { get; private set; }

	public GameDate spawnDueDate { get; private set; }

	public int maxCapacity { get; private set; }

	public string factionID { get; private set; }

	public int limit { get; private set; }

	public override Type serializedData => typeof(SaveDataLichGraveyard);

	public LichGraveyard(Region location)
		: base(STRUCTURE_TYPE.LICH_GRAVEYARD, location)
	{
		spawnedMonstersID = new List<string>();
		factionID = string.Empty;
		limit = GameUtilities.RandomBetweenTwoNumbers(10, 15);
	}

	public LichGraveyard(Region location, SaveDataLichGraveyard data)
		: base(location, data)
	{
		spawnRate = data.spawnRate;
		spawnDueDate = data.spawnDueDate;
		maxCapacity = data.maxCapacity;
		factionID = data.factionID;
		limit = data.limit;
		spawnedMonstersID = new List<string>(data.spawnedMonstersID);
	}

	public override void LoadStructureSecondWaveInMainThread(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadStructureSecondWaveInMainThread(saveDataLocationStructure);
		SchedulingManager.Instance.AddEntry(spawnDueDate, SpawnMonster, this);
	}

	public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource)
	{
		base.OnTileDamaged(tile, amount, isPlayerSource);
		AdjustHP(amount, null, isPlayerSource);
		OnStructureDamaged();
	}

	public override bool DoesTileContributeToDamage(LocationGridTile tile)
	{
		return true;
	}

	public void SetSpawnRate(int p_rateInTicks)
	{
		spawnRate = p_rateInTicks;
	}

	public void SetMaxCapacity(int p_capacity)
	{
		maxCapacity = p_capacity;
	}

	public void SetFaction(Faction p_faction)
	{
		factionID = p_faction.persistentID;
	}

	public void StartSpawning()
	{
		ScheduleNextSpawn();
	}

	private void ScheduleNextSpawn()
	{
		spawnDueDate = GameManager.Instance.Today().AddTicks(spawnRate);
		SchedulingManager.Instance.AddEntry(spawnDueDate, SpawnMonster, this);
	}

	private void SpawnMonster()
	{
		if (base.hasBeenDestroyed)
		{
			return;
		}
		EvaluateSpawnedMonsters();
		if (spawnedMonstersID.Count < maxCapacity)
		{
			MonsterMigrationBiomeAtomizedData randomMonsterToSpawn = LandmarkManager.Instance.GetStructureData(base.structureType).GetRandomMonsterToSpawn();
			if (randomMonsterToSpawn.monsterType != SUMMON_TYPE.None)
			{
				Faction factionByPersistentID = FactionManager.Instance.GetFactionByPersistentID(factionID);
				LocationGridTile randomPassableTile = GetRandomPassableTile();
				if (randomPassableTile != null)
				{
					Summon summon = CharacterManager.Instance.CreateNewSummon(randomMonsterToSpawn.monsterType, factionByPersistentID, null, randomPassableTile.parentMap.region, this, "", bypassIdeologyChecking: true);
					summon.behaviourComponent.AddBehaviourComponent(typeof(LichGraveyardMonsterBehaviour));
					CharacterManager.Instance.PlaceSummonInitially(summon, randomPassableTile);
					GameManager.Instance.CreateParticleEffectAt(summon, PARTICLE_EFFECT.Spawn_Effect);
					spawnedMonstersID.Add(summon.persistentID);
					limit--;
					EvaluateLimit();
					if (base.hasBeenDestroyed)
					{
						LichGraveyardMonsterBehaviour obj = CharacterManager.Instance.GetCharacterBehaviourComponent(typeof(LichGraveyardMonsterBehaviour)) as LichGraveyardMonsterBehaviour;
						string log = string.Empty;
						obj.TryAttackVillage(summon, ref log);
					}
				}
			}
		}
		if (!base.hasBeenDestroyed)
		{
			ScheduleNextSpawn();
		}
	}

	private void EvaluateSpawnedMonsters()
	{
		for (int i = 0; i < spawnedMonstersID.Count; i++)
		{
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(spawnedMonstersID[i]);
			if (characterByPersistentID == null || characterByPersistentID.isDead || characterByPersistentID.faction.persistentID != factionID)
			{
				spawnedMonstersID.RemoveAt(i);
				i--;
			}
		}
	}

	private void EvaluateLimit()
	{
		if (limit <= 0)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Locations", "LocationAlerts_Table", "lich_graveyard_decay", LOG_TAG.Major);
			log.AddToFillers(null, base.name, LOG_IDENTIFIER.LANDMARK_1);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			AdjustHP(-base.maxHP);
		}
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		spawnedMonstersID.Remove(p_character.persistentID);
	}

	public override void CleanUp()
	{
		if (DatabaseManager.Instance.structureDatabase.HasStructure(base.persistentID))
		{
			spawnedMonstersID.Clear();
			base.CleanUp();
		}
	}
}
