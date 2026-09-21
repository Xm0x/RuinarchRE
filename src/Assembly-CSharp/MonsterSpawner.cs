using System;
using System.Collections.Generic;
using Inner_Maps;
using Object_Pools;
using UtilityScripts;

public class MonsterSpawner : TileObject
{
	public GameDate spawnDate { get; private set; }

	public GameDate chaosOrbsDate { get; private set; }

	public GameDate wildernessMonsterSpawnDate { get; private set; }

	public SUMMON_TYPE monsterType { get; private set; }

	public string monsterClassName { get; private set; }

	public STRUCTURE_TYPE currentStructureType { get; private set; }

	public int minLimit { get; private set; }

	public int maxLimit { get; private set; }

	public string pluralizedMonsterTypeString { get; private set; }

	public List<string> spawnedCharacterIDs { get; private set; }

	public bool stopProcessingOnPlacement { get; private set; }

	public bool shouldProcessWildernessPlacement { get; private set; }

	public bool willSpawnWildernessMonsterAgain { get; private set; }

	public override Type serializedData => typeof(SaveDataMonsterSpawner);

	public override string description => GetFlavorText();

	public override bool canBeDirectlyUnseizedToCharacter => false;

	public int maxSpawnLimit
	{
		get
		{
			float spawnMultiplier = (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.MONSTER_SPAWNER) as MonsterSpawnerData).GetSpawnMultiplier();
			return (int)((float)maxLimit * spawnMultiplier);
		}
	}

	public MonsterSpawner()
	{
		Initialize(TILE_OBJECT_TYPE.MONSTER_SPAWNER);
		RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		RemoveAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		base.traitContainer.RemoveTrait(this, "Flammable");
		base.maxHP = InnerMapManager.Instance.GetMonsterSpawnerMaxHP();
		base.currentHP = base.maxHP;
		pluralizedMonsterTypeString = string.Empty;
		spawnedCharacterIDs = new List<string>();
		willSpawnWildernessMonsterAgain = false;
	}

	public MonsterSpawner(SaveDataMonsterSpawner data)
		: base(data)
	{
		spawnDate = data.spawnDate;
		chaosOrbsDate = data.chaosOrbsDate;
		wildernessMonsterSpawnDate = data.wildernessMonsterSpawnDate;
		monsterType = data.monsterType;
		monsterClassName = data.monsterClassName;
		currentStructureType = data.currentStructureType;
		minLimit = data.minLimit;
		maxLimit = data.maxLimit;
		pluralizedMonsterTypeString = data.pluralizedMonsterTypeString;
		spawnedCharacterIDs = ((data.spawnedCharacterIDs != null) ? new List<string>(data.spawnedCharacterIDs) : new List<string>());
		stopProcessingOnPlacement = data.stopProcessingOnPlacement;
		shouldProcessWildernessPlacement = data.shouldProcessWildernessPlacement;
		willSpawnWildernessMonsterAgain = data.willSpawnWildernessMonsterAgain;
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		base.ConstructDefaultPlayerActions(broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.RAID, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.HARASS_VILLAGERS, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.DESTROY_SUPPLIES, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.DESTROY_DEFENSES, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.SNATCH_VILLAGER, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.KILL_VILLAGER, broadcastSignal);
	}

	protected override void SubscribeListeners(bool shouldLock)
	{
		if (!base.hasSubscribedToSignals)
		{
			base.hasSubscribedToSignals = true;
			base.SubscribeListeners(shouldLock);
			Messenger.AddListener(Signals.HOUR_STARTED, PerHour, shouldLock);
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDeath, shouldLock);
			Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterJoinedFaction, shouldLock);
		}
	}

	protected override void UnsubscribeListeners()
	{
		if (base.hasSubscribedToSignals)
		{
			base.hasSubscribedToSignals = false;
			base.UnsubscribeListeners();
			Messenger.RemoveListener(Signals.HOUR_STARTED, PerHour);
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDeath);
			Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterJoinedFaction);
		}
	}

	protected override void SubscribeListenersDuringSeize()
	{
		base.SubscribeListenersDuringSeize();
		Messenger.AddListener(Signals.HOUR_STARTED, PerHour);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDeath);
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterJoinedFaction);
	}

	protected override void UnsubscribeListenersDuringSeize()
	{
		base.UnsubscribeListenersDuringSeize();
		Messenger.RemoveListener(Signals.HOUR_STARTED, PerHour);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDeath);
		Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterJoinedFaction);
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		LocationGridTile locationGridTile = gridTileLocation;
		if (stopProcessingOnPlacement)
		{
			if (locationGridTile != null)
			{
				currentStructureType = locationGridTile.structure.structureType;
			}
			return;
		}
		bool flag = false;
		if (locationGridTile != null && (currentStructureType != locationGridTile.structure.structureType || (monsterType == SUMMON_TYPE.None && string.IsNullOrEmpty(monsterClassName))))
		{
			if (locationGridTile.structure.structureType.IsSpecialStructure())
			{
				MonsterMigrationBiomeAtomizedData monsterMigrationBiomeAtomizedData = LandmarkManager.Instance.GetStructureData(locationGridTile.structure.structureType)?.GetRandomMonsterToSpawn();
				if (monsterMigrationBiomeAtomizedData != null)
				{
					monsterType = monsterMigrationBiomeAtomizedData.monsterType;
					monsterClassName = monsterMigrationBiomeAtomizedData.monsterClassName;
					minLimit = monsterMigrationBiomeAtomizedData.minRange;
					maxLimit = monsterMigrationBiomeAtomizedData.maxRange;
					flag = true;
				}
			}
			else if (locationGridTile.structure.structureType == STRUCTURE_TYPE.WILDERNESS && shouldProcessWildernessPlacement)
			{
				WildernessMonsterSpawnerData randomWildernessMonsterSpawnerData = LandmarkManager.Instance.GetRandomWildernessMonsterSpawnerData(locationGridTile.mainBiomeType);
				STRUCTURE_TYPE structureType = randomWildernessMonsterSpawnerData.structureType;
				MonsterMigrationBiomeAtomizedData monsterMigrationBiomeAtomizedData2 = LandmarkManager.Instance.GetStructureData(structureType)?.GetMonsterToSpawn(randomWildernessMonsterSpawnerData.monsterType);
				if (monsterMigrationBiomeAtomizedData2 != null)
				{
					monsterType = randomWildernessMonsterSpawnerData.monsterType;
					monsterClassName = string.Empty;
					pluralizedMonsterTypeString = string.Empty;
					minLimit = monsterMigrationBiomeAtomizedData2.minRange;
					maxLimit = monsterMigrationBiomeAtomizedData2.maxRange;
					ScheduleInitialWildernessSpawning();
					flag = true;
				}
			}
			if (monsterType != SUMMON_TYPE.None)
			{
				string s = monsterType.LocalizedName();
				pluralizedMonsterTypeString = Utilities.PluralizeString(s);
			}
			else if (!string.IsNullOrEmpty(monsterClassName))
			{
				string displayName = CharacterManager.Instance.GetCharacterClass(monsterClassName).displayName;
				pluralizedMonsterTypeString = Utilities.PluralizeString(displayName);
			}
			currentStructureType = locationGridTile.structure.structureType;
		}
		if (InnerMapManager.Instance.AddMonsterSpawner(this))
		{
			if (flag)
			{
				InitialSpawnMonsters();
			}
			ScheduleChaosOrbsSpawn();
		}
		else
		{
			GameDate otherDate = GameManager.Instance.Today();
			if (spawnDate.IsBefore(otherDate))
			{
				InitialSpawnMonsters();
			}
			if (chaosOrbsDate.IsBefore(otherDate))
			{
				ScheduleChaosOrbsSpawn();
			}
		}
		Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
	}

	public override void OnLoadPlacePOI()
	{
		DefaultProcessOnPlacePOI();
		InnerMapManager.Instance.AddMonsterSpawner(this);
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		InnerMapManager.Instance.RemoveMonsterSpawner(this);
		if (p_destroyer != null)
		{
			DropMonsterItem();
		}
		Messenger.Broadcast(TileObjectSignals.MONSTER_SPAWNER_DESTROYED, this);
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
		Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
	}

	public override void VillagerReactionToTileObject(Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObject(actor, ref debugLog);
		if (actor.traitContainer.HasTrait("Combatant") && !actor.isAlliedWithPlayer && (actor.faction == null || actor.faction != FactionManager.Instance.ratmenFaction) && (actor.race != RACE.RATMAN || (actor.faction != null && actor.faction.isMajorNonPlayer)))
		{
			actor.jobComponent.TriggerDestroy(this, "Destroy_Dangerous");
		}
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		SchedulingManager.Instance.AddEntry(spawnDate, SpawnMonsters, this);
		SchedulingManager.Instance.AddEntry(chaosOrbsDate, ProduceChaosOrbs, this);
		if (willSpawnWildernessMonsterAgain)
		{
			SchedulingManager.Instance.AddEntry(wildernessMonsterSpawnDate, ProcessWildernessSpawning, this);
		}
	}

	private void PerHour()
	{
		AdjustHP(-50, ELEMENTAL_TYPE.Normal, triggerDeath: false, null, null, showHPBar: true, 0f, isPlayerSource: false, isTrueDamage: true);
	}

	private void OnCharacterDeath(Character p_character)
	{
		if (RemoveSpawnedCharacter(p_character) && p_character.behaviourComponent.HasBehaviour(typeof(WildernessMonsterSpawnerBuildBehaviour)) && !p_character.behaviourComponent.hasBuiltWildernessMonsterSpawnerStructure)
		{
			ScheduleProcessWildernessSpawning();
		}
	}

	private void OnCharacterJoinedFaction(Character p_character, Faction p_faction)
	{
		if (p_character is Summon summon && spawnedCharacterIDs.Contains(p_character.persistentID) && FactionManager.Instance.GetDefaultFactionForMonster(summon.summonType) != p_faction && RemoveSpawnedCharacter(p_character) && p_character.behaviourComponent.HasBehaviour(typeof(WildernessMonsterSpawnerBuildBehaviour)))
		{
			if (!p_character.behaviourComponent.hasBuiltWildernessMonsterSpawnerStructure)
			{
				ScheduleProcessWildernessSpawning();
			}
			p_character.behaviourComponent.RemoveBehaviourComponent(typeof(WildernessMonsterSpawnerBuildBehaviour));
		}
	}

	private void ScheduleInitialWildernessSpawning()
	{
		wildernessMonsterSpawnDate = GameManager.Instance.Today().AddTicks(1);
		SchedulingManager.Instance.AddEntry(wildernessMonsterSpawnDate, InitialWildernessSpawn, this);
	}

	private void ScheduleProcessWildernessSpawning()
	{
		if (!willSpawnWildernessMonsterAgain)
		{
			wildernessMonsterSpawnDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(5));
			SchedulingManager.Instance.AddEntry(wildernessMonsterSpawnDate, ProcessWildernessSpawning, this);
			willSpawnWildernessMonsterAgain = true;
		}
	}

	private void ProcessWildernessSpawning()
	{
		if (!willSpawnWildernessMonsterAgain)
		{
			return;
		}
		willSpawnWildernessMonsterAgain = false;
		if (gridTileLocation != null && gridTileLocation.structure.structureType == STRUCTURE_TYPE.WILDERNESS && spawnedCharacterIDs.Count <= 0 && monsterType != SUMMON_TYPE.None)
		{
			WildernessMonsterSpawnerData wildernessMonsterSpawnerDataByMonsterType = LandmarkManager.Instance.GetWildernessMonsterSpawnerDataByMonsterType(monsterType);
			STRUCTURE_TYPE structureType = wildernessMonsterSpawnerDataByMonsterType.structureType;
			if (wildernessMonsterSpawnerDataByMonsterType != null)
			{
				WildernessSpawn(wildernessMonsterSpawnerDataByMonsterType, structureType);
			}
		}
	}

	private void InitialWildernessSpawn()
	{
		if (base.isBeingSeized)
		{
			ScheduleInitialWildernessSpawning();
		}
		else
		{
			if (gridTileLocation == null)
			{
				return;
			}
			SetShouldProcessWildernessPlacement(p_state: false);
			if (gridTileLocation.structure.structureType == STRUCTURE_TYPE.WILDERNESS && monsterType != SUMMON_TYPE.None)
			{
				WildernessMonsterSpawnerData wildernessMonsterSpawnerDataByMonsterType = LandmarkManager.Instance.GetWildernessMonsterSpawnerDataByMonsterType(monsterType);
				STRUCTURE_TYPE structureType = wildernessMonsterSpawnerDataByMonsterType.structureType;
				if (wildernessMonsterSpawnerDataByMonsterType != null)
				{
					WildernessSpawn(wildernessMonsterSpawnerDataByMonsterType, structureType);
				}
			}
		}
	}

	private void WildernessSpawn(WildernessMonsterSpawnerData p_data, STRUCTURE_TYPE p_structureType)
	{
		LocationGridTile locationTile = gridTileLocation;
		Summon summon = CharacterManager.Instance.CreateNewSummon(p_data.monsterType, FactionManager.Instance.GetDefaultFactionForMonster(p_data.monsterType), null, GridMap.Instance.mainRegion, null, "", bypassIdeologyChecking: true);
		CharacterManager.Instance.PlaceSummonInitially(summon, locationTile);
		summon.traitContainer.RemoveTrait(summon, "Hibernating");
		summon.traitContainer.RemoveTrait(summon, "Indestructible");
		summon.AddAdvertisedAction(INTERACTION_TYPE.BUILD_MONSTER_SPAWNER_STRUCTURE);
		summon.behaviourComponent.SetWildernessMonsterSpawnerStructureType(p_structureType);
		summon.behaviourComponent.SetHasBuiltWildernessMonsterSpawnerStructure(p_state: false);
		summon.behaviourComponent.SetMonsterSpawnerID(base.persistentID);
		summon.behaviourComponent.AddBehaviourComponent(typeof(WildernessMonsterSpawnerBuildBehaviour));
		GameManager.Instance.CreateParticleEffectAt(summon, PARTICLE_EFFECT.Spawn_Effect);
		AddSpawnedCharacter(summon);
	}

	private void ScheduleNextSpawn()
	{
		spawnDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(InnerMapManager.Instance.GetMonsterSpawnerSpawnHour()));
		SchedulingManager.Instance.AddEntry(spawnDate, SpawnMonsters, this);
	}

	private void InitialSpawnMonsters()
	{
		spawnDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
		SchedulingManager.Instance.AddEntry(spawnDate, SpawnMonsters, this);
	}

	private void SpawnMonsters()
	{
		LocationGridTile locationGridTile = gridTileLocation;
		if (locationGridTile == null && !base.isBeingSeized)
		{
			return;
		}
		int residentCount = 0;
		int num = 0;
		if (CanSpawnNewMonsters(ref residentCount))
		{
			if (base.mapObjectVisual != null)
			{
				AkSoundEngine.PostEvent("Play_Monster_Spawner_Spawn", base.mapObjectVisual.gameObject);
			}
			int num2 = maxSpawnLimit;
			num2 -= residentCount;
			if (minLimit > num2)
			{
				minLimit = num2;
			}
			int num3 = GameUtilities.RandomBetweenTwoNumbers(minLimit, num2);
			for (int i = 0; i < num3; i++)
			{
				LocationGridTile locationGridTile2 = locationGridTile.structure.GetRandomPassableTile();
				if (locationGridTile2 == null)
				{
					locationGridTile2 = locationGridTile.structure.GetRandomTile();
				}
				if (monsterType != SUMMON_TYPE.None)
				{
					Summon summon = CharacterManager.Instance.CreateNewSummon(monsterType, FactionManager.Instance.GetDefaultFactionForMonster(monsterType), locationGridTile.structure.settlementLocation, locationGridTile.structure.region, locationGridTile.structure, "", bypassIdeologyChecking: true);
					CharacterManager.Instance.PlaceSummonInitially(summon, locationGridTile2);
					summon.traitContainer.RemoveTrait(summon, "Hibernating");
					summon.traitContainer.RemoveTrait(summon, "Indestructible");
					GameManager.Instance.CreateParticleEffectAt(summon, PARTICLE_EFFECT.Spawn_Effect);
					AddSpawnedCharacter(summon);
					num += CharacterManager.Instance.GetCharacterClass(summon.characterClass.className).monsterSpawnerDamageOnSpawn;
				}
				else if (monsterClassName == "Ratman")
				{
					Character character = CharacterManager.Instance.GenerateRatman(locationGridTile2, locationGridTile2.structure);
					AddSpawnedCharacter(character);
					num += CharacterManager.Instance.GetCharacterClass(character.characterClass.className).monsterSpawnerDamageOnSpawn;
				}
			}
		}
		if (num > 0)
		{
			AdjustHP(-num, ELEMENTAL_TYPE.Normal, triggerDeath: false, null, null, showHPBar: true, 0f, isPlayerSource: false, isTrueDamage: true);
		}
		if (locationGridTile != null)
		{
			ScheduleNextSpawn();
		}
	}

	public string GetMonsterSpawnerNextSpawnTime()
	{
		string text = string.Empty;
		if (CanSpawnNewMonsters())
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Will_Spawn_Monster");
			log.AddToFillers(null, Utilities.ColorizeAndBoldName(pluralizedMonsterTypeString), LOG_IDENTIFIER.STRING_1);
			log.AddToFillers(null, Utilities.ColorizeName(spawnDate.ConvertToContinuousDaysWithTime(nextLineTime: false, capitalizedDay: true)), LOG_IDENTIFIER.STRING_2);
			log.AddToFillers(null, $"{minLimit}-{maxLimit}", LOG_IDENTIFIER.OTHER);
			text = text + log.logText + ".\n\n" + GetMonsterCapacityString();
			text += GetMonsterChaosOrbsGenerationString();
			LogPool.Release(log);
		}
		else if (monsterType != SUMMON_TYPE.None || !string.IsNullOrEmpty(monsterClassName))
		{
			if (willSpawnWildernessMonsterAgain && wildernessMonsterSpawnDate.hasValue)
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Will_Spawn_Monster");
				log2.AddToFillers(null, monsterType.LocalizedName(), LOG_IDENTIFIER.STRING_1);
				log2.AddToFillers(null, Utilities.ColorizeName(wildernessMonsterSpawnDate.ConvertToContinuousDaysWithTime(nextLineTime: false, capitalizedDay: true)), LOG_IDENTIFIER.STRING_2);
				log2.AddToFillers(null, minLimit + "-" + maxLimit, LOG_IDENTIFIER.OTHER);
				text += log2.logText;
				LogPool.Release(log2);
			}
			else
			{
				text = text + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cannot_Spawn_Monster") + " " + Utilities.ColorizeAndBoldName(pluralizedMonsterTypeString) + ".\n\n" + GetMonsterCapacityString();
			}
			text += GetMonsterChaosOrbsGenerationString();
		}
		return text;
	}

	private string GetMonsterChaosOrbsGenerationString()
	{
		string p_reason = string.Empty;
		if (CanSpawnChaosOrbs(ref p_reason))
		{
			return "\n\n" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Generate_2_Chaos_Orbs") + " " + Utilities.ColorizeName(chaosOrbsDate.ConvertToContinuousDaysWithTime(nextLineTime: false, capitalizedDay: true)) + ".";
		}
		if (!string.IsNullOrEmpty(p_reason))
		{
			return "\n\n" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", p_reason ?? "") + ".";
		}
		return "\n\n" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cant_Generate_Chaos_Orbs") + ".";
	}

	private string GetMonsterCapacityString()
	{
		return string.Format("{0}: {1}/{2}", LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Capital_Capacity"), GetCountOfValidSpawnedMonsters(), maxSpawnLimit);
	}

	private bool CanSpawnNewMonsters(ref int residentCount)
	{
		LocationGridTile locationGridTile = gridTileLocation;
		if (!base.isBeingSeized && locationGridTile != null && locationGridTile.structure.structureType.IsSpecialStructure() && (monsterType != SUMMON_TYPE.None || !string.IsNullOrEmpty(monsterClassName)))
		{
			residentCount = GetCountOfValidSpawnedMonsters();
			if (residentCount < maxSpawnLimit)
			{
				return true;
			}
		}
		return false;
	}

	private bool CanSpawnNewMonsters()
	{
		int residentCount = 0;
		return CanSpawnNewMonsters(ref residentCount);
	}

	public bool CanSpawnChaosOrbs(ref string p_reason)
	{
		if (base.isBeingSeized)
		{
			return false;
		}
		if (gridTileLocation == null)
		{
			return false;
		}
		if (!gridTileLocation.structure.structureType.IsSpecialStructure())
		{
			p_reason = "Cant_Generate_Orbs_Structure";
			return false;
		}
		if (!IsThereAliveSpawnedMonsterInsideStructureOfSpawner())
		{
			p_reason = "Cant_Generate_Orbs_No_Monsters";
			return false;
		}
		if (!PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.MONSTER_SPAWNER).hasRemainingOrUnliChaosOrbs)
		{
			p_reason = "Cant_Generate_Orbs_Exhausted";
			return false;
		}
		return true;
	}

	private bool IsThereAliveSpawnedMonsterInsideStructureOfSpawner()
	{
		LocationGridTile locationGridTile = gridTileLocation;
		if (locationGridTile != null)
		{
			for (int i = 0; i < spawnedCharacterIDs.Count; i++)
			{
				string text = spawnedCharacterIDs[i];
				Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(text);
				if (characterByPersistentID is Summon p_summon && IsValidSpawnedMonster(p_summon) && characterByPersistentID.gridTileLocation?.structure == locationGridTile.structure)
				{
					return true;
				}
			}
		}
		return false;
	}

	private int GetCountOfValidSpawnedMonsters()
	{
		int num = 0;
		for (int i = 0; i < spawnedCharacterIDs.Count; i++)
		{
			string text = spawnedCharacterIDs[i];
			if (CharacterManager.Instance.GetCharacterByPersistentID(text) is Summon p_summon && IsValidSpawnedMonster(p_summon))
			{
				num++;
			}
		}
		return num;
	}

	public bool HasValidSpawnedMonster()
	{
		for (int i = 0; i < spawnedCharacterIDs.Count; i++)
		{
			string text = spawnedCharacterIDs[i];
			if (CharacterManager.Instance.GetCharacterByPersistentID(text) is Summon p_summon && IsValidSpawnedMonster(p_summon))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasValidSpawnedMonsterThatIsNotAttackingVillage()
	{
		for (int i = 0; i < spawnedCharacterIDs.Count; i++)
		{
			string text = spawnedCharacterIDs[i];
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(text);
			if (characterByPersistentID is Summon p_summon && IsValidSpawnedMonster(p_summon) && !characterByPersistentID.behaviourComponent.HasBehaviour(typeof(AttackVillageBehaviour)))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsValidSpawnedMonster(Summon p_summon)
	{
		if (!p_summon.isDead && p_summon.faction == FactionManager.Instance.GetDefaultFactionForMonster(p_summon.summonType))
		{
			LocationGridTile locationGridTile = p_summon.gridTileLocation;
			if (locationGridTile == null)
			{
				return true;
			}
			return locationGridTile.structure.structureType != STRUCTURE_TYPE.KENNEL;
		}
		return false;
	}

	private void AddSpawnedCharacter(Character p_character)
	{
		if (!spawnedCharacterIDs.Contains(p_character.persistentID))
		{
			spawnedCharacterIDs.Add(p_character.persistentID);
		}
	}

	private bool RemoveSpawnedCharacter(Character p_character)
	{
		return spawnedCharacterIDs.Remove(p_character.persistentID);
	}

	public void SetStopProcessingOnPlacement(bool p_state)
	{
		stopProcessingOnPlacement = p_state;
	}

	public void SetShouldProcessWildernessPlacement(bool p_state)
	{
		shouldProcessWildernessPlacement = p_state;
	}

	private void ScheduleChaosOrbsSpawn()
	{
		chaosOrbsDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(12));
		SchedulingManager.Instance.AddEntry(chaosOrbsDate, ProduceChaosOrbs, this);
	}

	private void ProduceChaosOrbs()
	{
		LocationGridTile locationGridTile = gridTileLocation;
		if (locationGridTile != null || base.isBeingSeized)
		{
			string p_reason = string.Empty;
			if (CanSpawnChaosOrbs(ref p_reason))
			{
				PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.MONSTER_SPAWNER).DecreaseRemainingChaosOrbs(2);
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, locationGridTile.centeredWorldLocation, 2, locationGridTile.parentMap);
			}
			ScheduleChaosOrbsSpawn();
		}
	}

	private void DropMonsterItem()
	{
		TILE_OBJECT_TYPE randomWeightedItemDropByMonsterType = CharacterManager.Instance.GetRandomWeightedItemDropByMonsterType(monsterType);
		if (randomWeightedItemDropByMonsterType == TILE_OBJECT_TYPE.NONE)
		{
			return;
		}
		TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(randomWeightedItemDropByMonsterType);
		LocationGridTile firstNeighborThatIsPassableAndNoObject = gridTileLocation;
		if (firstNeighborThatIsPassableAndNoObject != null && firstNeighborThatIsPassableAndNoObject.tileObjectComponent.objHere != null)
		{
			firstNeighborThatIsPassableAndNoObject = gridTileLocation.GetFirstNeighborThatIsPassableAndNoObject();
		}
		if (firstNeighborThatIsPassableAndNoObject != null)
		{
			firstNeighborThatIsPassableAndNoObject.structure.AddPOI(tileObject, firstNeighborThatIsPassableAndNoObject);
			if (tileObject is EquipmentItem equipmentItem)
			{
				equipmentItem.TryAddRandomPrefix();
			}
		}
	}

	public override string GetAdditionalTestingData()
	{
		string additionalTestingData = base.GetAdditionalTestingData();
		additionalTestingData += "\n\tSpawned Characters:";
		for (int i = 0; i < spawnedCharacterIDs.Count; i++)
		{
			string text = spawnedCharacterIDs[i];
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(text);
			additionalTestingData = additionalTestingData + "|" + characterByPersistentID.name + "|";
		}
		return additionalTestingData;
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		spawnedCharacterIDs.Remove(p_character.persistentID);
	}

	public override void DestroyPermanently()
	{
		base.DestroyPermanently();
		spawnedCharacterIDs?.Clear();
		spawnedCharacterIDs = null;
	}
}
