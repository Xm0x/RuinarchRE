using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Villager_Wants;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Settings;
using Traits;
using UnityEngine;
using UtilityScripts;

public class CharacterManager : BaseMonoBehaviour
{
	public static CharacterManager Instance;

	[Header("Sub Managers")]
	[SerializeField]
	private CharacterClassManager classManager;

	[SerializeField]
	private CharacterBehaviourManager behaviourManager;

	public CharacterTalentManager talentManager;

	public static readonly string[] sevenDeadlySinsClassNames = new string[7] { "Lust", "Gluttony", "Greed", "Sloth", "Wrath", "Envy", "Pride" };

	public const string Make_Love = "Make Love";

	public const string Steal = "Steal";

	public const string Poison_Food = "Poison Food";

	public const string Place_Trap = "Place Trap";

	public const string Flirt = "Flirt";

	public const string Transform_To_Wolf = "Transform To Wolf";

	public const string Drink_Blood = "Drink Blood";

	public const string Destroy_Action = "Destroy";

	public static readonly string[] resistRuinarchPowerText = new string[7] { "How dare you!", "Nope!", "Fuck off bitch!", "Huh?!", "Who's there?!", "What's this feeling?", "I feel violated!" };

	private SUMMON_TYPE[] wisps = new SUMMON_TYPE[3]
	{
		SUMMON_TYPE.Electric_Wisp,
		SUMMON_TYPE.Earthen_Wisp,
		SUMMON_TYPE.Fire_Wisp
	};

	public const int VISION_RANGE = 8;

	public const int AVOID_COMBAT_VISION_RANGE = 12;

	public bool lessenCharacterLogs;

	[Header("Character Portrait Assets")]
	[SerializeField]
	private CharacterPortraitData _portraitCollection;

	[SerializeField]
	private RolePortraitFramesDictionary portraitFrames;

	[SerializeField]
	private HairColorDictionary _hairColors;

	[Header("Character Marker Assets")]
	[SerializeField]
	private List<Sprite> _markerAnimationReferences;

	[Header("Summon Settings")]
	[SerializeField]
	private SummonSettingDictionary summonSettings;

	[Header("Minion Settings")]
	[SerializeField]
	private MinionSettingDictionary minionSettings;

	[Header("Character Marker Effects")]
	public Sprite webbedEffect;

	public Sprite stakeEffect;

	[Header("Character Name Colors")]
	public Color summonNameColor;

	public Color demonNameColor;

	public Color undeadNameColor;

	public Color normalNameColor;

	private string _summonNameColorHex;

	private string _demonNameColorHex;

	private string _undeadNameColorHex;

	private string _normalNameColorHex;

	public bool useRigidBody;

	public bool useRigidBody2;

	private Dictionary<Collider2D, BaseVisionTrigger> _poiVisionCache;

	private Dictionary<Collider2D, Projectile> _projectileCache;

	private Dictionary<Sprite, string> _markerAnimationSpriteNames;

	private List<Character> _charactersToCleanUp;

	private Dictionary<Type, VillagerWant> _allWants;

	private Dictionary<string, DeadlySin> deadlySins { get; set; }

	private Dictionary<EMOTION, Emotion> emotionData { get; set; }

	private List<Emotion> allEmotions { get; set; }

	public int defaultSleepTicks { get; private set; }

	public SUMMON_TYPE[] summonsPool { get; private set; }

	public COMBAT_MODE[] combatModes { get; private set; }

	public List<string> rumorWorthyActions { get; private set; }

	public Character necromancerInTheWorld { get; private set; }

	public bool hasSpawnedNecromancerOnce { get; private set; }

	public bool toggleCharacterMarkerName { get; private set; }

	public int CHARACTER_MISSING_THRESHOLD { get; private set; }

	public int CHARACTER_PRESUMED_DEAD_THRESHOLD { get; private set; }

	public int PLAYER_MONSTER_EXPIRATION_TICKS { get; private set; }

	public int CHARACTER_CLEAR_UNKILLABLE_LIST_TICKS { get; private set; }

	public Dictionary<RELIGION, List<Character>> activeReligiousCultists { get; private set; }

	public Dictionary<RELIGION, int> activeReligiousCultLeaders { get; private set; }

	public List<Character> powerLockers { get; private set; }

	private Dictionary<string, List<string>> _villagerHairCollection => ExternalFileManager.Instance.GetVillagerHairCollection();

	private Dictionary<string, CharacterSpritesPerAnimation> _specialCharacterSpriteCollection => ExternalFileManager.Instance.GetSpecialCharacterSpriteCollection();

	public List<Character> allCharacters => DatabaseManager.Instance.characterDatabase.allCharactersList;

	public List<Character> limboCharacters => DatabaseManager.Instance.characterDatabase.limboCharactersList;

	public string demonNameColorHex => _demonNameColorHex;

	public string summonNameColorHex => _summonNameColorHex;

	public string normalNameColorHex => _normalNameColorHex;

	public CharacterPortraitRaceCollection portraitCollection => ExternalFileManager.Instance.GetVillagerPortraitCollection();

	public List<DailySchedule> allDailySchedules { get; private set; }

	public Dictionary<Type, VillagerWant> allWants => _allWants;

	private void Awake()
	{
		Instance = this;
	}

	public void Initialize()
	{
		toggleCharacterMarkerName = true;
		_poiVisionCache = new Dictionary<Collider2D, BaseVisionTrigger>(200);
		_projectileCache = new Dictionary<Collider2D, Projectile>(100);
		_charactersToCleanUp = new List<Character>(50);
		_summonNameColorHex = ColorUtility.ToHtmlStringRGB(summonNameColor);
		_demonNameColorHex = ColorUtility.ToHtmlStringRGB(demonNameColor);
		_undeadNameColorHex = ColorUtility.ToHtmlStringRGB(undeadNameColor);
		_normalNameColorHex = ColorUtility.ToHtmlStringRGB(normalNameColor);
		PLAYER_MONSTER_EXPIRATION_TICKS = GameManager.Instance.GetTicksBasedOnHour(3);
		CHARACTER_CLEAR_UNKILLABLE_LIST_TICKS = GameManager.Instance.GetTicksBasedOnHour(2);
		classManager.Initialize();
		talentManager.Initialize();
		behaviourManager.Initialize();
		CreateDeadlySinsData();
		defaultSleepTicks = GameManager.Instance.GetTicksBasedOnHour(6);
		CHARACTER_MISSING_THRESHOLD = GameManager.Instance.GetTicksBasedOnHour(24);
		CHARACTER_PRESUMED_DEAD_THRESHOLD = GameManager.Instance.GetTicksBasedOnHour(24);
		summonsPool = new SUMMON_TYPE[4]
		{
			SUMMON_TYPE.Wolf,
			SUMMON_TYPE.Golem,
			SUMMON_TYPE.Incubus,
			SUMMON_TYPE.Succubus
		};
		combatModes = new COMBAT_MODE[3]
		{
			COMBAT_MODE.Aggressive,
			COMBAT_MODE.Passive,
			COMBAT_MODE.Defend
		};
		rumorWorthyActions = new List<string> { "Make Love", "Steal", "Poison Food", "Place Trap", "Flirt", "Transform To Wolf", "Drink Blood" };
		ConstructEmotionData();
		ConstructDailySchedules();
		CreateVillagerWantInstances();
		ConstructMarkerAnimationSpriteNames();
		activeReligiousCultists = new Dictionary<RELIGION, List<Character>>();
		activeReligiousCultLeaders = new Dictionary<RELIGION, int>();
		powerLockers = new List<Character>();
		Messenger.AddListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
		Messenger.AddListener<string, string>(CharacterSignals.RENAME_CHARACTER, OnRenameCharacter);
		Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
		Messenger.AddListener(Signals.AFTER_TICK_ENDED, BeforeTickStarted);
	}

	private void OnDayStarted()
	{
		SpawnBasicMonstersAtTheStartOfDay();
	}

	private void BeforeTickStarted()
	{
		ProcessCharactersMarkedForCleanUp();
	}

	public Character CreateNewCharacter(string className, RACE race, GENDER gender, Faction faction = null, BaseSettlement homeLocation = null, Region homeRegion = null, LocationStructure homeStructure = null, bool randomizeTraits = true)
	{
		Character character = new Character(className, race, gender, FamilyTreeGenerator.GetRandomHairColorByRace(race));
		character.SetRandomName();
		character.Initialize();
		if (faction != null)
		{
			if (!faction.JoinFaction(character, broadcastSignal: true, bypassIdeologyChecking: false, isInitial: true))
			{
				FactionManager.Instance.vagrantFaction.JoinFaction(character, broadcastSignal: true, bypassIdeologyChecking: false, isInitial: true);
			}
		}
		else
		{
			FactionManager.Instance.wildMonsterFaction.JoinFaction(character, broadcastSignal: true, bypassIdeologyChecking: false, isInitial: true);
		}
		if (homeStructure != null)
		{
			character.MigrateHomeStructureTo(homeStructure, broadcast: false);
			homeStructure.region.AddCharacterToLocation(character);
		}
		else if (homeLocation != null)
		{
			character.MigrateHomeTo(homeLocation, null, broadcast: false);
			if (homeLocation is NPCSettlement nPCSettlement)
			{
				nPCSettlement.region.AddCharacterToLocation(character);
			}
			else if (homeRegion != null)
			{
				homeRegion.AddResident(character);
				homeRegion.AddCharacterToLocation(character);
			}
		}
		else if (homeRegion != null)
		{
			homeRegion.AddResident(character);
			homeRegion.AddCharacterToLocation(character);
		}
		if (randomizeTraits)
		{
			character.CreateRandomInitialTraits();
		}
		character.CreateDefaultTraits();
		AddNewCharacter(character);
		return character;
	}

	public Character CreateNewCharacter(SaveDataCharacter data)
	{
		Character character = new Character(data);
		if (character.isInLimbo)
		{
			AddNewLimboCharacter(character);
		}
		else
		{
			AddNewCharacter(character);
		}
		return character;
	}

	public Character CreateNewCharacter(PreCharacterData data, string className, Faction faction = null, NPCSettlement homeLocation = null, LocationStructure homeStructure = null, Action<Character> afterInitializationAction = null)
	{
		Character character = new Character(className, data.race, data.gender, data.sexuality, data.hairColorType, data.id);
		character.SetFirstName(data.firstName);
		character.Initialize();
		afterInitializationAction?.Invoke(character);
		if (faction != null)
		{
			if (!faction.JoinFaction(character, broadcastSignal: true, bypassIdeologyChecking: false, isInitial: true))
			{
				FactionManager.Instance.vagrantFaction.JoinFaction(character, broadcastSignal: true, bypassIdeologyChecking: false, isInitial: true);
			}
		}
		else
		{
			FactionManager.Instance.wildMonsterFaction.JoinFaction(character, broadcastSignal: true, bypassIdeologyChecking: false, isInitial: true);
		}
		if (homeStructure != null)
		{
			character.MigrateHomeStructureTo(homeStructure, broadcast: false);
			homeStructure.region.AddCharacterToLocation(character);
		}
		else if (homeLocation != null)
		{
			character.MigrateHomeTo(homeLocation, null, broadcast: false);
			homeLocation.region.AddCharacterToLocation(character);
		}
		character.CreateDefaultTraits();
		AddNewCharacter(character);
		data.SetHasBeenSpawned();
		return character;
	}

	public SaveDataCharacter CreateNewSaveDataCharacter(Character character)
	{
		SaveDataCharacter obj = Activator.CreateInstance(character.serializedData) as SaveDataCharacter;
		obj.Save(character);
		return obj;
	}

	public void AddNewCharacter(Character character, bool broadcastSignal = true, bool addToAliveVillagersList = true)
	{
		DatabaseManager.Instance.characterDatabase.AddCharacter(character, addToAliveVillagersList);
		if (broadcastSignal)
		{
			Messenger.Broadcast(CharacterSignals.CHARACTER_CREATED, character);
		}
	}

	public void RemoveCharacter(Character character, bool broadcastSignal = true, bool removeFromAliveVillagersList = true)
	{
		if (DatabaseManager.Instance.characterDatabase.RemoveCharacter(character, removeFromAliveVillagersList) && broadcastSignal)
		{
			Messenger.Broadcast(CharacterSignals.CHARACTER_REMOVED, character);
		}
	}

	public void AddNewLimboCharacter(Character character)
	{
		DatabaseManager.Instance.characterDatabase.AddLimboCharacter(character);
		character.SetIsInLimbo(state: true);
	}

	public void RemoveLimboCharacter(Character character)
	{
		if (DatabaseManager.Instance.characterDatabase.RemoveLimboCharacter(character))
		{
			character.SetIsInLimbo(state: false);
		}
	}

	public void PlaceInitialCharacters(List<Character> characters, NPCSettlement npcSettlement)
	{
		for (int i = 0; i < characters.Count; i++)
		{
			Character character = characters[i];
			if (!character.marker)
			{
				character.CreateMarker();
			}
			if (character.homeStructure != null && character.homeStructure.settlementLocation == npcSettlement)
			{
				LocationGridTile randomUnoccupiedTileThatHasNoCharacters = character.homeStructure.GetRandomUnoccupiedTileThatHasNoCharacters();
				character.InitialCharacterPlacement(randomUnoccupiedTileThatHasNoCharacters);
			}
			else
			{
				LocationGridTile randomUnoccupiedTileThatHasNoCharacters2 = npcSettlement.region.wilderness.GetRandomUnoccupiedTileThatHasNoCharacters();
				character.InitialCharacterPlacement(randomUnoccupiedTileThatHasNoCharacters2);
			}
		}
	}

	public int GetFoodAmountTakenFromPOI(IPointOfInterest poi)
	{
		if (poi is Character character)
		{
			if (character.race == RACE.WOLF)
			{
				return 150;
			}
			if (character.race == RACE.HUMANS)
			{
				return 200;
			}
			if (character.race == RACE.ELVES)
			{
				return 200;
			}
			return 100;
		}
		return 50;
	}

	public FoodPile CreateFoodPileForPOI(IPointOfInterest poi, LocationGridTile tileOverride = null, bool createLog = true)
	{
		LocationGridTile locationGridTile = tileOverride;
		Character character = null;
		if (poi is Character character2)
		{
			character = character2;
		}
		else if (poi is Tombstone tombstone)
		{
			character = tombstone.character;
		}
		if (locationGridTile == null)
		{
			locationGridTile = poi.gridTileLocation;
		}
		if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
		{
			locationGridTile = locationGridTile.GetFirstNearestTileFromThisWithNoObject();
		}
		if (locationGridTile != null)
		{
			int foodAmountTakenFromPOI = GetFoodAmountTakenFromPOI(poi);
			TILE_OBJECT_TYPE tileObjectType;
			if (character == null)
			{
				tileObjectType = ((!(poi is Crops crops)) ? TILE_OBJECT_TYPE.ANIMAL_MEAT : crops.producedObjectOnHarvest);
			}
			else
			{
				switch (character.race)
				{
				case RACE.HUMANS:
					tileObjectType = TILE_OBJECT_TYPE.HUMAN_MEAT;
					break;
				case RACE.ELVES:
					tileObjectType = TILE_OBJECT_TYPE.ELF_MEAT;
					break;
				case RACE.RAT:
				case RACE.RATMAN:
					tileObjectType = TILE_OBJECT_TYPE.RAT_MEAT;
					break;
				default:
					tileObjectType = TILE_OBJECT_TYPE.ANIMAL_MEAT;
					break;
				}
			}
			if (poi != null)
			{
				FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(tileObjectType);
				if (poi.traitContainer.HasTrait("Abomination Germ"))
				{
					foodPile.traitContainer.AddTrait(foodPile, "Abomination Germ");
					poi.traitContainer.RemoveStatusAndStacks(poi, "Abomination Germ");
				}
				if (poi.traitContainer.HasTrait("Plagued"))
				{
					PlagueDisease.Instance.AddPlaguedStatusOnPOIWithLifespanDuration(foodPile);
				}
				if (character != null && createLog)
				{
					Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "became_food_pile", LOG_TAG.Life_Changes);
					log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					log.AddToFillers(foodPile, foodPile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
					log.AddLogToDatabase(releaseLogAfter: true);
				}
				foodPile.SetResourceInPile(foodAmountTakenFromPOI);
				locationGridTile.structure.AddPOI(foodPile, locationGridTile);
				return foodPile;
			}
		}
		return null;
	}

	public Summon RaiseFromDeadReplaceCharacterWithMonsterType(SUMMON_TYPE monsterType, Character target, Faction faction)
	{
		target.SetHasBeenRaisedFromDead(state: true);
		LocationGridTile gridTileLocation = target.gridTileLocation;
		Summon summon = null;
		if (target.grave != null)
		{
			gridTileLocation = target.grave.gridTileLocation;
		}
		if (gridTileLocation != null)
		{
			target.traitContainer.RemoveAllTraitsAndStatuses(target);
			if (target.grave != null)
			{
				gridTileLocation.structure.RemovePOI(target.grave);
				target.SetGrave(null);
			}
			summon = CreateNewSummon(monsterType, faction, null, target.homeRegion, null, "", bypassIdeologyChecking: true);
			summon.SetFirstName(target.name);
			summon.SetHasBeenRaisedFromDead(state: true);
			PlaceSummonInitially(summon, gridTileLocation);
			target.TransferAllInventoryItemsAndEquippableEquipmentsTo(summon, transferOwnership: true);
			RemoveCharacter(target);
			target.DestroyMarker();
		}
		return summon;
	}

	public void RaiseFromDeadRetainCharacterInstance(Character target, Faction faction, RACE race, string className, Action<Character> onRaisedFromDeadAction = null)
	{
		RACE rACE = race;
		string text = className;
		if (rACE == RACE.NONE)
		{
			rACE = target.race;
		}
		if (string.IsNullOrEmpty(className))
		{
			text = target.characterClass.className;
		}
		if (text.Contains("Zombie"))
		{
			target.SetHasBeenRaisedFromDead(state: true);
			LocationGridTile tile = ((target.grave != null) ? target.grave.gridTileLocation : target.gridTileLocation);
			GameManager.Instance.CreateParticleEffectAt(tile, PARTICLE_EFFECT.Zombie_Transformation);
			target.ReturnToLife(faction, rACE, text);
			target.traitContainer.RemoveTrait(target, "Transitioning");
			target.MigrateHomeStructureTo(null);
			if (!target.behaviourComponent.HasBehaviour(typeof(ZombieBehaviour)))
			{
				target.behaviourComponent.AddBehaviourComponent(typeof(ZombieBehaviour));
			}
			target.combatComponent.UpdateMaxHPAndReset();
		}
		else
		{
			target.ReturnToLife();
		}
		onRaisedFromDeadAction?.Invoke(target);
	}

	public Summon SpawnNewMonsterInstanceFrom(SUMMON_TYPE p_monsterType, Character p_target, Faction p_faction = null, bool p_shouldCopyTargetName = true)
	{
		BaseSettlement homeSettlement = p_target.homeSettlement;
		LocationStructure homeStructure = p_target.homeStructure;
		LocationGridTile gridTileLocation = p_target.gridTileLocation;
		return SpawnNewMonsterInstanceFrom(p_monsterType, p_target, homeSettlement, homeStructure, gridTileLocation, p_faction, p_shouldCopyTargetName);
	}

	public Summon SpawnNewMonsterInstanceFrom(SUMMON_TYPE p_monsterType, Character p_target, BaseSettlement p_homeSettlement, LocationStructure p_homeStructure, LocationGridTile p_gridTile, Faction p_faction = null, bool p_shouldCopyTargetName = true)
	{
		if (p_gridTile == null)
		{
			return null;
		}
		Region mainRegion = GridMap.Instance.mainRegion;
		if (p_faction == null)
		{
			p_faction = FactionManager.Instance.GetDefaultFactionForMonster(p_monsterType);
		}
		Summon summon = CreateNewSummon(p_monsterType, p_faction, p_homeSettlement, mainRegion, p_homeStructure, "", bypassIdeologyChecking: true);
		if (p_shouldCopyTargetName)
		{
			summon.SetFirstName(p_target.name);
		}
		PlaceSummonInitially(summon, p_gridTile);
		return summon;
	}

	public Color GetCharacterNameColor(Character character)
	{
		if (character != null)
		{
			if (character.minion != null)
			{
				return demonNameColor;
			}
			Faction faction = character.faction;
			if (faction != null && faction.factionType.type == FACTION_TYPE.Undead)
			{
				return undeadNameColor;
			}
			Faction faction2 = character.faction;
			if (faction2 != null && faction2.factionType.type == FACTION_TYPE.Wild_Monsters)
			{
				return summonNameColor;
			}
		}
		return normalNameColor;
	}

	public string GetCharacterNameColorHex(Character character)
	{
		if (character != null)
		{
			if (character.minion != null)
			{
				return demonNameColorHex;
			}
			Faction faction = character.faction;
			if (faction != null && faction.factionType.type == FACTION_TYPE.Undead)
			{
				return _undeadNameColorHex;
			}
			Faction faction2 = character.faction;
			if (faction2 != null && faction2.factionType.type == FACTION_TYPE.Wild_Monsters)
			{
				return summonNameColorHex;
			}
		}
		return normalNameColorHex;
	}

	public void ToggleCharacterMarkerNameplate()
	{
		SetToggleCharacterMarkerNameplate(!toggleCharacterMarkerName);
	}

	private void SetToggleCharacterMarkerNameplate(bool p_state)
	{
		if (toggleCharacterMarkerName != p_state)
		{
			toggleCharacterMarkerName = p_state;
			Messenger.Broadcast(CharacterSignals.TOGGLE_CHARACTER_MARKER_NAMEPLATE, toggleCharacterMarkerName);
		}
	}

	public bool HasCharacterClass(string className)
	{
		return classManager.HasCharacterClass(className);
	}

	public CharacterClass GetCharacterClass(string p_className)
	{
		return classManager.GetCharacterClass(p_className);
	}

	public CharacterClassBehaviour GetClassBehaviour(string p_className)
	{
		return classManager.GetClassBehaviour(p_className);
	}

	public Type[] GetDefaultBehaviourSet(string setName)
	{
		return behaviourManager.GetDefaultBehaviourSet(setName);
	}

	public bool HasDefaultBehaviourSet(string setName)
	{
		return behaviourManager.HasDefaultBehaviourSet(setName);
	}

	public CharacterBehaviour GetCharacterBehaviourComponent(Type type)
	{
		return behaviourManager.GetCharacterBehaviourComponent(type);
	}

	public T GetCharacterBehaviourComponent<T>(Type type) where T : CharacterBehaviour
	{
		return behaviourManager.GetCharacterBehaviourComponent<T>(type);
	}

	public string GetTraitBehaviourSetOf(Character p_character)
	{
		return behaviourManager.GetTraitBehaviourSetOf(p_character);
	}

	public int GetFoodProducerClassTier(string p_className)
	{
		return classManager.GetFoodProducerClassTier(p_className);
	}

	public int GetResourceProducerClassTier(string p_className)
	{
		return classManager.GetResourceProducerClassTier(p_className);
	}

	public SUMMON_TYPE GetRandomWeakAnimalType()
	{
		return classManager.GetRandomWeakAnimalType();
	}

	public CharacterClass GetRandomLowTierCombatant()
	{
		return classManager.GetRandomLowTierCombatant();
	}

	public Summon CreateNewLimboSummon(SUMMON_TYPE summonType, Faction faction = null, NPCSettlement homeLocation = null, LocationStructure homeStructure = null, string className = "")
	{
		Summon summon = CreateNewSummonClassFromType(summonType, className);
		if (SettingsManager.Instance.settings.randomizeMonsterNames)
		{
			summon.SetRandomName();
		}
		else
		{
			summon.SetFirstName(summon.raceClassName);
		}
		summon.Initialize();
		if (faction == null || !faction.JoinFaction(summon, broadcastSignal: true, bypassIdeologyChecking: false, isInitial: true))
		{
			FactionManager.Instance.wildMonsterFaction.JoinFaction(summon, broadcastSignal: true, bypassIdeologyChecking: false, isInitial: true);
		}
		if (homeStructure != null)
		{
			summon.MigrateHomeStructureTo(homeStructure, broadcast: false);
			homeStructure.region.AddCharacterToLocation(summon);
		}
		else if (homeLocation != null)
		{
			summon.MigrateHomeTo(homeLocation, null, broadcast: false);
			homeLocation.region.AddCharacterToLocation(summon);
		}
		summon.CreateRandomInitialTraits();
		summon.CreateDefaultTraits();
		AddNewLimboCharacter(summon);
		return summon;
	}

	public Summon CreateNewSummon(SUMMON_TYPE summonType, Faction faction = null, BaseSettlement homeLocation = null, Region homeRegion = null, LocationStructure homeStructure = null, string className = "", bool bypassIdeologyChecking = false)
	{
		Summon summon = CreateNewSummonClassFromType(summonType, className);
		if (SettingsManager.Instance.settings.randomizeMonsterNames)
		{
			summon.SetRandomName();
		}
		else
		{
			summon.SetFirstName(summon.raceClassName);
		}
		summon.Initialize();
		if (faction == null || !faction.JoinFaction(summon, broadcastSignal: true, bypassIdeologyChecking, isInitial: true))
		{
			FactionManager.Instance.wildMonsterFaction.JoinFaction(summon, broadcastSignal: true, bypassIdeologyChecking, isInitial: true);
		}
		if (homeStructure != null)
		{
			summon.MigrateHomeStructureTo(homeStructure, broadcast: false);
			homeStructure.region.AddCharacterToLocation(summon);
		}
		else if (homeLocation != null)
		{
			summon.MigrateHomeTo(homeLocation, null, broadcast: false);
			if (homeLocation is NPCSettlement nPCSettlement)
			{
				nPCSettlement.region.AddCharacterToLocation(summon);
			}
			else if (homeRegion != null)
			{
				homeRegion.AddResident(summon);
				homeRegion.AddCharacterToLocation(summon);
			}
		}
		else if (homeRegion != null)
		{
			homeRegion.AddResident(summon);
			homeRegion.AddCharacterToLocation(summon);
		}
		summon.CreateRandomInitialTraits();
		summon.CreateDefaultTraits();
		if (!GameManager.Instance.gameHasStarted)
		{
			summon.OnMonsterCreatedForInitialWorldGeneration();
		}
		AddNewCharacter(summon);
		return summon;
	}

	public Summon CreateNewSummon(SaveDataSummon data)
	{
		Summon summon = CreateNewSummonClassFromType(data);
		if (summon.isInLimbo)
		{
			AddNewLimboCharacter(summon);
		}
		else
		{
			AddNewCharacter(summon);
		}
		return summon;
	}

	private Summon CreateNewSummonClassFromType(SaveDataSummon data)
	{
		try
		{
			string text = data.summonType.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			return Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("provided summon type was invalid! " + text), data) as Summon;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.InnerException);
		}
		return null;
	}

	private Summon CreateNewSummonClassFromType(SUMMON_TYPE summonType, string className)
	{
		string text = summonType.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		if (className != "")
		{
			return Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("provided summon type was invalid! " + text), className) as Summon;
		}
		return Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("provided summon type was invalid! " + text)) as Summon;
	}

	public string GetSummonClassNameBySummonType(SUMMON_TYPE type)
	{
		if (summonSettings.ContainsKey(type))
		{
			return summonSettings[type].className;
		}
		return type.ToStringEnumWithSpace();
	}

	public MinionSettings GetMinionSettings(MINION_TYPE type)
	{
		return minionSettings[type];
	}

	public void PlaceSummonInitially(Summon summon, LocationGridTile locationTile)
	{
		summon.currentRegion?.RemoveCharacterFromLocation(summon);
		summon.CreateMarker();
		summon.InitialCharacterPlacement(locationTile);
		summon.OnPlaceSummon(locationTile);
		if (PlayerManager.Instance.player != null)
		{
			summon.ApplyAllPrimordialBonus();
		}
	}

	public void Teleport(Character character, LocationGridTile tile)
	{
		int num;
		if ((bool)character.marker)
		{
			num = ((InnerMapCameraMove.Instance.target == character.marker.gameObject.transform) ? 1 : 0);
			if (num != 0)
			{
				InnerMapCameraMove.Instance.CenterCameraOn(null);
			}
		}
		else
		{
			num = 0;
		}
		if (character.currentRegion != tile.structure.region)
		{
			character.currentRegion?.RemoveCharacterFromLocation(character);
		}
		if (!character.hasMarker)
		{
			character.CreateMarker();
			character.marker.InitialPlaceMarkerAt(tile);
		}
		else
		{
			character.marker.PlaceMarkerAt(tile);
		}
		character.marker.pathfindingAI.ClearAllCurrentPathData();
		character.marker.pathfindingAI.UpdateMe();
		if (num != 0)
		{
			character.CenterOnCharacter();
		}
	}

	public TILE_OBJECT_TYPE GetEggType(SUMMON_TYPE summonType)
	{
		switch (summonType)
		{
		case SUMMON_TYPE.Giant_Spider:
		case SUMMON_TYPE.Broodmother:
			return TILE_OBJECT_TYPE.SPIDER_EGG;
		case SUMMON_TYPE.Harpy:
			return TILE_OBJECT_TYPE.HARPY_EGG;
		default:
			return TILE_OBJECT_TYPE.NONE;
		}
	}

	private void SpawnBasicMonstersAtTheStartOfDay()
	{
		string log = string.Empty;
		if (!GameUtilities.RollChance(100, ref log))
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count; i++)
		{
			BaseSettlement baseSettlement = DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[i];
			if (baseSettlement.HasResidents() && baseSettlement.locationType == LOCATION_TYPE.VILLAGE && baseSettlement is NPCSettlement nPCSettlement && !nPCSettlement.structureComponent.HasLinkedStructureWithAliveResident())
			{
				num++;
			}
		}
		if (num <= 0)
		{
			return;
		}
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		Region mainRegion = GridMap.Instance.mainRegion;
		for (int j = 0; j < mainRegion.allSpecialStructures.Count; j++)
		{
			LocationStructure locationStructure = mainRegion.allSpecialStructures[j];
			if (!locationStructure.HasAliveResident(null) && !InnerMapManager.Instance.HasMonsterSpawnerInStructure(locationStructure))
			{
				list.Add(locationStructure);
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		for (int k = 0; k < num; k++)
		{
			if (list.Count <= 0)
			{
				break;
			}
			int index = GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1);
			LocationStructure p_structure = list[index];
			SpawnBasicMonstersProcessing(p_structure, ref log);
			list.RemoveAt(index);
		}
	}

	private void SpawnBasicMonstersProcessing(LocationStructure p_structure, ref string debugLog)
	{
		MonsterMigrationBiomeAtomizedData randomMonsterToSpawn = LandmarkManager.Instance.GetStructureData(p_structure.structureType).GetRandomMonsterToSpawn();
		if (randomMonsterToSpawn != null && p_structure.passableTiles.Count > 0)
		{
			List<LocationGridTile> p_locationChoices = RuinarchListPool<LocationGridTile>.Claim();
			p_locationChoices.AddRange(p_structure.passableTiles);
			randomMonsterToSpawn.SpawnMonsters(ref p_locationChoices, p_structure, ref debugLog);
			RuinarchListPool<LocationGridTile>.Release(p_locationChoices);
		}
	}

	public Character GetCharacterByID(int id)
	{
		return DatabaseManager.Instance.characterDatabase.GetCharacterByID(id);
	}

	public Character GetCharacterByPersistentID(string id)
	{
		return DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(id);
	}

	public Character GetCharacterByName(string name)
	{
		for (int i = 0; i < DatabaseManager.Instance.characterDatabase.allCharactersList.Count; i++)
		{
			Character character = DatabaseManager.Instance.characterDatabase.allCharactersList[i];
			if (character.name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
			{
				return character;
			}
		}
		return null;
	}

	public void GetCharactersByName(List<Character> p_list, string p_name)
	{
		for (int i = 0; i < DatabaseManager.Instance.characterDatabase.allCharactersList.Count; i++)
		{
			Character character = DatabaseManager.Instance.characterDatabase.allCharactersList[i];
			if (character.name.Equals(p_name, StringComparison.CurrentCultureIgnoreCase))
			{
				p_list.Add(character);
			}
		}
		for (int j = 0; j < DatabaseManager.Instance.characterDatabase.limboCharactersList.Count; j++)
		{
			Character character2 = DatabaseManager.Instance.characterDatabase.limboCharactersList[j];
			if (character2.name.Equals(p_name, StringComparison.CurrentCultureIgnoreCase))
			{
				p_list.Add(character2);
			}
		}
	}

	public Character GetLimboCharacterByName(string name)
	{
		for (int i = 0; i < DatabaseManager.Instance.characterDatabase.limboCharactersList.Count; i++)
		{
			Character character = DatabaseManager.Instance.characterDatabase.limboCharactersList[i];
			if (character.name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
			{
				return character;
			}
		}
		return null;
	}

	public bool CanAddCharacterLogOrShowNotif(INTERACTION_TYPE actionType)
	{
		if (!lessenCharacterLogs)
		{
			return true;
		}
		if (actionType != INTERACTION_TYPE.SIT && actionType != INTERACTION_TYPE.STAND && actionType != INTERACTION_TYPE.RETURN_HOME && actionType != INTERACTION_TYPE.SLEEP && actionType != INTERACTION_TYPE.SLEEP_OUTSIDE && actionType != INTERACTION_TYPE.NAP && actionType != INTERACTION_TYPE.GO_TO)
		{
			return true;
		}
		return false;
	}

	public bool HasCharacterNotConversedInMinutes(Character character, int minutes)
	{
		return character.nonActionEventsComponent.lastConversationDate.AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(minutes)).IsBefore(GameManager.Instance.Today());
	}

	public bool IsCharacterTheSameLycan(Character character1, Character character2)
	{
		LycanthropeData lycanthropeData = null;
		if (character1.isLycanthrope)
		{
			lycanthropeData = character1.lycanData;
		}
		else if (character2.isLycanthrope)
		{
			lycanthropeData = character2.lycanData;
		}
		if (lycanthropeData != null)
		{
			if (lycanthropeData.originalForm == character1 || lycanthropeData.lycanthropeForm == character1)
			{
				if (lycanthropeData.originalForm != character2)
				{
					return lycanthropeData.lycanthropeForm == character2;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public bool IsCharacterConsideredTargetOfBoneGolem(Character p_considerer, Character p_targetCharacter)
	{
		if (p_considerer != p_targetCharacter && p_targetCharacter.gridTileLocation != null && !p_targetCharacter.isDead && !p_targetCharacter.isAlliedWithPlayer && (bool)p_targetCharacter.marker && p_targetCharacter.marker.isMainVisualActive && p_considerer.movementComponent.HasPathTo(p_targetCharacter.gridTileLocation) && !p_targetCharacter.isInLimbo && !p_targetCharacter.isBeingSeized && p_targetCharacter.carryComponent.IsNotBeingCarried() && p_targetCharacter.combatComponent.combatMode != COMBAT_MODE.Passive && !p_targetCharacter.traitContainer.HasTrait("Hibernating", "Indestructible") && p_considerer.IsHostileWith(p_targetCharacter) && !IsCharacterConsideredPrisonerOf(p_considerer, p_targetCharacter))
		{
			return true;
		}
		return false;
	}

	public bool IsCharacterConsideredPrisonerOf(Character p_considerer, Character p_targetCharacter)
	{
		return p_targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner")?.IsConsideredPrisonerOf(p_considerer) ?? false;
	}

	public string GetRandomResistRuinarchPowerText()
	{
		return resistRuinarchPowerText[GameUtilities.RandomBetweenTwoNumbers(0, resistRuinarchPowerText.Length - 1)];
	}

	public int GetNumberOfAliveMonsterByType<T>() where T : Character
	{
		int num = 0;
		for (int i = 0; i < allCharacters.Count; i++)
		{
			if (allCharacters[i] is T val && !val.isDead)
			{
				num++;
			}
		}
		return num;
	}

	public bool ShouldMonsterRelocateHomeToStructureOnPlace(LocationStructure p_structure, Character p_character)
	{
		if (p_character.isDead)
		{
			return false;
		}
		if (p_structure is Wilderness || p_structure is DemonicStructure)
		{
			return false;
		}
		if (p_structure.settlementLocation?.owner != null && p_structure.settlementLocation.owner != p_character.faction)
		{
			return false;
		}
		if (p_structure.HasReachedMaxResidentCapacity())
		{
			return false;
		}
		if (p_character.race == RACE.RATMAN)
		{
			if (p_structure.HasVillagerResidentOfDifferentRace(RACE.RATMAN))
			{
				return false;
			}
		}
		else if (p_structure.HasVillagerResident())
		{
			return false;
		}
		return true;
	}

	public Character GetRandomCharacterKilledByPlayer()
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		list.AddRange(allCharacters);
		list.Shuffle();
		for (int i = 0; i < list.Count; i++)
		{
			Character character = list[i];
			if (character.isDead && character.wasKilledByPlayerSource)
			{
				RuinarchListPool<Character>.Release(list);
				return character;
			}
		}
		RuinarchListPool<Character>.Release(list);
		return null;
	}

	public PortraitSettings UpdatePortraitSettings(RACE p_race, GENDER p_gender, HAIR_COLOR p_hairColorType, string p_className, int p_portraitIndex)
	{
		return new PortraitSettings(p_race, p_gender, p_hairColorType, p_className, p_portraitIndex);
	}

	public PortraitSettings GeneratePortraitSettings(RACE p_race, GENDER p_gender, HAIR_COLOR p_hairColorType, string p_className)
	{
		return new PortraitSettings(p_race, p_gender, p_hairColorType, p_className);
	}

	public PortraitSettings GeneratePortraitSettings(RACE p_race, string p_className)
	{
		return new PortraitSettings(p_race, p_className);
	}

	public PortraitSettings GeneratePortraitSettings(Character character)
	{
		return GeneratePortraitSettings(character.race, character.gender, character.hairColorType, character.visuals.classToUseForVisuals);
	}

	public PortraitFrame GetPortraitFrame(CHARACTER_ROLE role)
	{
		if (portraitFrames.ContainsKey(role))
		{
			return portraitFrames[role];
		}
		throw new Exception("There is no frame for role " + role);
	}

	public Color GetHairColor(HAIR_COLOR p_colorType)
	{
		return _hairColors[p_colorType];
	}

	public void PutCharacterIntoLimbo(Character p_character)
	{
		if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == p_character)
		{
			UIManager.Instance.characterInfoUI.CloseMenu();
		}
		if ((bool)p_character.marker && p_character.marker.isMoving)
		{
			p_character.marker.StopMovement();
		}
		if (p_character.trapStructure.IsTrapped())
		{
			p_character.trapStructure.ResetAllTrapStructures();
		}
		if (p_character.trapStructure.IsTrappedInArea())
		{
			p_character.trapStructure.ResetTrapArea();
		}
		if (p_character.partyComponent.hasParty)
		{
			p_character.partyComponent.currentParty.RemoveMemberThatJoinedQuest(p_character);
		}
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)p_character, "");
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, (IPointOfInterest)p_character, "");
		if (!p_character.carryComponent.IsNotBeingCarried())
		{
			p_character.carryComponent.masterCharacter.UncarryPOI(p_character);
		}
		p_character.jobQueue.CancelAllJobs();
		p_character.UnsubscribeSignals();
		p_character.SetIsConversing(state: false);
		p_character.SetPOIState(POI_STATE.INACTIVE);
		SchedulingManager.Instance.ClearAllSchedulesBy(this);
		if ((bool)p_character.marker)
		{
			for (int i = 0; i < p_character.marker.inVisionCharacters.Count; i++)
			{
				Character character = p_character.marker.inVisionCharacters[i];
				if ((bool)character.marker)
				{
					character.marker.RemovePOIFromInVisionRange(p_character);
				}
			}
			for (int j = 0; j < p_character.marker.inVisionPOIsButDiffStructure.Count; j++)
			{
				IPointOfInterest pointOfInterest = p_character.marker.inVisionPOIsButDiffStructure[j];
				if (pointOfInterest is Character)
				{
					(pointOfInterest as Character).marker.RemovePOIAsInRangeButDifferentStructure(p_character);
				}
			}
			p_character.DestroyMarker(null, removeFromGame: false);
		}
		AddNewLimboCharacter(p_character);
		RemoveCharacter(p_character, broadcastSignal: false, removeFromAliveVillagersList: false);
	}

	public void ReleaseCharacterFromLimbo(Character p_character, LocationGridTile tileLocation)
	{
		p_character.SubscribeToSignals();
		p_character.SetPOIState(POI_STATE.ACTIVE);
		if (!p_character.hasMarker)
		{
			p_character.CreateMarker();
		}
		p_character.marker.InitialPlaceMarkerAt(tileLocation);
		AddNewCharacter(p_character, broadcastSignal: false, addToAliveVillagersList: false);
		RemoveLimboCharacter(p_character);
	}

	public CharacterSpritesPerAnimation GetCharacterAnimationSprites(RACE race, string characterClassName)
	{
		return GetCharacterClass((race == RACE.SKELETON) ? "Skeleton" : characterClassName).GetAssets(race);
	}

	public CharacterSpritesPerAnimation GetSpecialCharacterAnimationSprites(string p_characterID)
	{
		if (_specialCharacterSpriteCollection.ContainsKey(p_characterID))
		{
			return _specialCharacterSpriteCollection[p_characterID];
		}
		return null;
	}

	public TILE_OBJECT_TYPE GetRandomWeightedItemDropByCharacter(Character p_dropper, Character p_attacker)
	{
		return classManager.GetRandomWeightedItemDropByCharacter(p_dropper, p_attacker);
	}

	public TILE_OBJECT_TYPE GetRandomWeightedItemDropByMonsterType(SUMMON_TYPE p_monsterType)
	{
		return classManager.GetRandomWeightedItemDropByMonsterType(p_monsterType);
	}

	private string GetRandomMarkerHairSpriteID(GENDER gender)
	{
		switch (gender)
		{
		case GENDER.MALE:
		{
			List<string> list2 = _villagerHairCollection["Male"];
			return list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
		}
		case GENDER.FEMALE:
		{
			List<string> list = _villagerHairCollection["Female"];
			return list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		default:
			return null;
		}
	}

	private string GetRandomKnockedoutMarkerHairSpriteID(GENDER gender)
	{
		switch (gender)
		{
		case GENDER.MALE:
		{
			List<string> list2 = _villagerHairCollection["MaleKnockedout"];
			return list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
		}
		case GENDER.FEMALE:
		{
			List<string> list = _villagerHairCollection["FemaleKnockedout"];
			return list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		default:
			return null;
		}
	}

	public Sprite GetMarkerHairSprite(GENDER gender)
	{
		string randomMarkerHairSpriteID = GetRandomMarkerHairSpriteID(gender);
		return TextureManager.Instance.characterSpritesAtlas.GetSpriteByKey(randomMarkerHairSpriteID);
	}

	public Sprite GetMarkerKnockedOutHairSprite(GENDER gender)
	{
		string randomKnockedoutMarkerHairSpriteID = GetRandomKnockedoutMarkerHairSpriteID(gender);
		return TextureManager.Instance.characterSpritesAtlas.GetSpriteByKey(randomKnockedoutMarkerHairSpriteID);
	}

	private void ConstructMarkerAnimationSpriteNames()
	{
		_markerAnimationSpriteNames = new Dictionary<Sprite, string>();
		for (int i = 0; i < _markerAnimationReferences.Count; i++)
		{
			Sprite sprite = _markerAnimationReferences[i];
			_markerAnimationSpriteNames.Add(sprite, sprite.name);
		}
	}

	public string GetMarkerAnimationSpriteName(Sprite p_sprite)
	{
		if (_markerAnimationSpriteNames.ContainsKey(p_sprite))
		{
			return _markerAnimationSpriteNames[p_sprite];
		}
		return string.Empty;
	}

	private void OnCharacterFinishedAction(Character p_actor, IPointOfInterest p_target, INTERACTION_TYPE p_type, ACTION_STATUS p_status)
	{
		if ((bool)p_actor.marker)
		{
			p_actor.marker.UpdateAnimation();
		}
	}

	private void OnRenameCharacter(string characterPersistentID, string newName)
	{
		if (!string.IsNullOrEmpty(newName))
		{
			GetCharacterByPersistentID(characterPersistentID)?.RenameCharacter(newName);
		}
	}

	private void CreateDeadlySinsData()
	{
		deadlySins = new Dictionary<string, DeadlySin>();
		for (int i = 0; i < sevenDeadlySinsClassNames.Length; i++)
		{
			deadlySins.Add(sevenDeadlySinsClassNames[i], CreateNewDeadlySin(sevenDeadlySinsClassNames[i]));
		}
	}

	private DeadlySin CreateNewDeadlySin(string deadlySin)
	{
		Type type = Type.GetType(deadlySin + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
		if (type != null)
		{
			return Activator.CreateInstance(type) as DeadlySin;
		}
		return null;
	}

	public DeadlySin GetDeadlySin(string sinName)
	{
		if (deadlySins.ContainsKey(sinName))
		{
			return deadlySins[sinName];
		}
		return null;
	}

	public bool CanDoDeadlySinAction(string deadlySinName, DEADLY_SIN_ACTION action)
	{
		return deadlySins[deadlySinName].CanDoDeadlySinAction(action);
	}

	public bool POIValueTypeMatching(POIValueType poi1, POIValueType poi2)
	{
		if (poi1.id == poi2.id && poi1.poiType == poi2.poiType)
		{
			return poi1.tileObjectType == poi2.tileObjectType;
		}
		return false;
	}

	private void ConstructEmotionData()
	{
		emotionData = new Dictionary<EMOTION, Emotion>();
		allEmotions = new List<Emotion>();
		EMOTION[] enumValues = CollectionUtilities.GetEnumValues<EMOTION>();
		for (int i = 0; i < enumValues.Length; i++)
		{
			EMOTION key = enumValues[i];
			Type type = Type.GetType(Utilities.NotNormalizedConversionEnumToStringNoSpaces(key.ToString()) + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			if (type != null)
			{
				Emotion emotion = Activator.CreateInstance(type) as Emotion;
				emotionData.Add(key, emotion);
				allEmotions.Add(emotion);
			}
		}
	}

	public string TriggerEmotion(EMOTION emotionType, Character emoter, IPointOfInterest target, REACTION_STATUS status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode action = null, string reason = "", bool p_triggerOpinionChangesEffect = true)
	{
		if (emoter.isNormalCharacter)
		{
			if (emoter.CanFeelEmotion(emotionType))
			{
				return GetEmotion(emotionType).ProcessEmotion(emoter, target, status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, action, reason, p_triggerOpinionChangesEffect) + "|";
			}
			return string.Empty;
		}
		return string.Empty;
	}

	public string TriggerEmotion(EMOTION emotionType, Character emoter, IPointOfInterest target, REACTION_STATUS status, ActualGoapNode action = null, string reason = "", bool p_triggerOpinionChangesEffect = true)
	{
		if (emoter.isNormalCharacter)
		{
			if (emoter.CanFeelEmotion(emotionType))
			{
				int p_totalOpinionReduction = 0;
				string p_lastStrawReasonKey = string.Empty;
				return GetEmotion(emotionType).ProcessEmotion(emoter, target, status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, action, reason, p_triggerOpinionChangesEffect) + "|";
			}
			return string.Empty;
		}
		return string.Empty;
	}

	public void TriggerEmotion(EMOTION emotionType, Character emoter, IPointOfInterest target, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, bool p_triggerOpinionChangesEffect = true)
	{
		if (emoter.isNormalCharacter && emoter.CanFeelEmotion(emotionType))
		{
			GetEmotion(emotionType).ProcessEmotion(emoter, target, REACTION_STATUS.WITNESSED, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, null, null, p_triggerOpinionChangesEffect);
		}
	}

	public void TriggerEmotion(EMOTION emotionType, Character emoter, IPointOfInterest target, bool p_triggerOpinionChangesEffect = true)
	{
		if (emoter.isNormalCharacter && emoter.CanFeelEmotion(emotionType))
		{
			int p_totalOpinionReduction = 0;
			string p_lastStrawReasonKey = string.Empty;
			GetEmotion(emotionType).ProcessEmotion(emoter, target, REACTION_STATUS.WITNESSED, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, null, null, p_triggerOpinionChangesEffect);
		}
	}

	public string GetLocalizedEmotionText(EMOTION emotionType)
	{
		return GetEmotion(emotionType).localizedResponse ?? "";
	}

	public Emotion GetEmotion(string name)
	{
		for (int i = 0; i < allEmotions.Count; i++)
		{
			if (allEmotions[i].name == name)
			{
				return allEmotions[i];
			}
		}
		return null;
	}

	public Emotion GetEmotion(EMOTION emotionType)
	{
		return emotionData[emotionType];
	}

	public bool EmotionsChecker(string emotion)
	{
		string[] array = emotion.Split('|');
		for (int i = 0; i < array.Length; i++)
		{
			Emotion emotion2 = GetEmotion(array[i]);
			if (emotion2 == null)
			{
				continue;
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (i != j && (array[i] == array[j] || !emotion2.IsEmotionCompatibleWithThis(array[j])))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SetNecromancerInTheWorld(Character character)
	{
		if (necromancerInTheWorld != character)
		{
			necromancerInTheWorld = character;
			if (necromancerInTheWorld != null)
			{
				hasSpawnedNecromancerOnce = true;
				Messenger.Broadcast(CharacterSignals.NECROMANCER_SPAWNED, necromancerInTheWorld);
			}
		}
	}

	public Minion CreateNewMinion(Character character, bool initialize = true, bool keepData = false)
	{
		Minion minion = new Minion(character, keepData);
		if (initialize)
		{
			InitializeMinion(minion);
		}
		return minion;
	}

	public Minion CreateNewMinion(string className, RACE race, bool initialize = true)
	{
		Player player = PlayerManager.Instance.player;
		Minion minion = new Minion(CreateNewCharacter(className, race, GENDER.MALE, player.playerFaction, player.playerSettlement, player.portalArea.region), keepData: false);
		minion.character.behaviourComponent.UpdateDefaultBehaviourSet();
		if (initialize)
		{
			InitializeMinion(minion);
		}
		return minion;
	}

	public Minion CreateNewMinion(Character character, SaveDataMinion data)
	{
		return new Minion(character, data);
	}

	private void InitializeMinion(Minion minion)
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}

	private Gathering CreateNewGathering(GATHERING_TYPE type)
	{
		string text = Utilities.NotNormalizedConversionEnumToStringNoSpaces(type.ToString()) + "Gathering, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		return (Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("provided gathering type was invalid! " + text)) as Gathering) ?? throw new Exception("provided type not a gathering! " + text);
	}

	private SaveDataGathering CreateNewSaveDataGathering(Gathering gathering)
	{
		SaveDataGathering obj = Activator.CreateInstance(gathering.serializedData) as SaveDataGathering;
		obj.Save(gathering);
		return obj;
	}

	public Gathering CreateNewGathering(SaveDataGathering data)
	{
		return Activator.CreateInstance(Type.GetType(Utilities.NotNormalizedConversionEnumToStringNoSpaces(data.gatheringType.ToString()) + "Gathering, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"), data) as Gathering;
	}

	public Gathering CreateNewGathering(GATHERING_TYPE type, Character host)
	{
		Gathering gathering = CreateNewGathering(type);
		gathering.SetHost(host);
		return gathering;
	}

	public bool GenerateRatmen(LocationStructure structure, int amount, int chance = 10)
	{
		if (GameUtilities.RollChance(chance))
		{
			if (FactionManager.Instance.ratmenFaction == null)
			{
				FactionManager.Instance.CreateRatmenFaction();
			}
			for (int i = 0; i < amount; i++)
			{
				Character character = CreateNewCharacter("Ratman", RACE.RATMAN, GENDER.MALE, FactionManager.Instance.ratmenFaction ?? FactionManager.Instance.wildMonsterFaction, structure.settlementLocation, structure.settlementLocation.region, structure);
				LocationGridTile tile = CollectionUtilities.GetRandomElement(structure.passableTiles) ?? CollectionUtilities.GetRandomElement(structure.tiles);
				character.CreateMarker();
				character.InitialCharacterPlacement(tile);
			}
			return true;
		}
		return false;
	}

	public Character GenerateRatman(LocationGridTile p_gridTile, LocationStructure p_homeStructure = null, string p_firstName = "")
	{
		if (FactionManager.Instance.ratmenFaction == null)
		{
			FactionManager.Instance.CreateRatmenFaction();
		}
		Character character = CreateNewCharacter("Ratman", RACE.RATMAN, GENDER.MALE, FactionManager.Instance.ratmenFaction, p_homeStructure?.settlementLocation, p_homeStructure?.settlementLocation?.region, p_homeStructure);
		if (!string.IsNullOrEmpty(p_firstName))
		{
			character.SetFirstName(p_firstName);
		}
		character.CreateMarker();
		character.InitialCharacterPlacement(p_gridTile);
		return character;
	}

	private void ConstructDailySchedules()
	{
		allDailySchedules = ReflectiveEnumerator.GetEnumerableOfType<DailySchedule>(Array.Empty<object>()).ToList();
	}

	public DailySchedule GetDailySchedule<T>()
	{
		for (int i = 0; i < allDailySchedules.Count; i++)
		{
			DailySchedule dailySchedule = allDailySchedules[i];
			if (dailySchedule is T)
			{
				return dailySchedule;
			}
		}
		return null;
	}

	public DailySchedule GetDailySchedule(Type p_type)
	{
		for (int i = 0; i < allDailySchedules.Count; i++)
		{
			DailySchedule dailySchedule = allDailySchedules[i];
			if (dailySchedule.GetType() == p_type)
			{
				return dailySchedule;
			}
		}
		return null;
	}

	private void CreateVillagerWantInstances()
	{
		List<VillagerWant> list = ReflectiveEnumerator.GetEnumerableOfType<VillagerWant>(Array.Empty<object>()).ToList();
		_allWants = new Dictionary<Type, VillagerWant>();
		for (int i = 0; i < list.Count; i++)
		{
			VillagerWant villagerWant = list[i];
			allWants.Add(villagerWant.GetType(), villagerWant);
		}
	}

	public T GetVillagerWantInstance<T>(Type p_type) where T : VillagerWant
	{
		if (allWants.ContainsKey(p_type) && allWants[p_type] is T result)
		{
			return result;
		}
		return null;
	}

	public T GetVillagerWantInstance<T>() where T : VillagerWant
	{
		Type typeFromHandle = typeof(T);
		if (allWants.ContainsKey(typeFromHandle) && allWants[typeFromHandle] is T result)
		{
			return result;
		}
		return null;
	}

	public RESISTANCE GetRandomResistance(bool shouldIncludeNone = true)
	{
		int p_min = 0;
		if (!shouldIncludeNone)
		{
			p_min = 1;
		}
		RESISTANCE[] enumValues = CollectionUtilities.GetEnumValues<RESISTANCE>();
		int num = GameUtilities.RandomBetweenTwoNumbers(p_min, enumValues.Length - 1);
		return enumValues[num];
	}

	public bool IsThereALivingWhispererInTheWorld()
	{
		for (int i = 0; i < allCharacters.Count; i++)
		{
			if (allCharacters[i] is Whisperer { isDead: false })
			{
				return true;
			}
		}
		return false;
	}

	public SUMMON_TYPE GetRandomWispType()
	{
		return wisps[GameUtilities.RandomBetweenTwoNumbers(0, wisps.Length - 1)];
	}

	public void IncreaseActiveReligiousCultist(RELIGION p_religion, Character p_character)
	{
		if (!activeReligiousCultists.ContainsKey(p_religion))
		{
			activeReligiousCultists.Add(p_religion, new List<Character>());
		}
		activeReligiousCultists[p_religion].Add(p_character);
		Messenger.Broadcast(PlayerSkillSignals.UPDATE_SKILL_UNLOCK_COSTS);
		Messenger.Broadcast(CharacterSignals.ACTIVE_RELIGIOUS_CULTISTS_UPDATED, p_religion);
	}

	public void DecreaseActiveReligiousCultist(RELIGION p_religion, Character p_character)
	{
		if (activeReligiousCultists.ContainsKey(p_religion))
		{
			activeReligiousCultists[p_religion].Remove(p_character);
			Messenger.Broadcast(PlayerSkillSignals.UPDATE_SKILL_UNLOCK_COSTS);
			Messenger.Broadcast(CharacterSignals.ACTIVE_RELIGIOUS_CULTISTS_UPDATED, p_religion);
		}
	}

	public bool HasActiveReligiousCultistOfType(RELIGION p_religion)
	{
		if (activeReligiousCultists.ContainsKey(p_religion))
		{
			return activeReligiousCultists[p_religion].Count > 0;
		}
		return false;
	}

	public int GetChaoticEnergyCostIncrease()
	{
		if (activeReligiousCultists.ContainsKey(RELIGION.Nature_Worship))
		{
			return activeReligiousCultists[RELIGION.Nature_Worship].Count * 5;
		}
		return 0;
	}

	public void IncreaseActiveReligiousCultLeader(RELIGION p_religion)
	{
		if (!activeReligiousCultLeaders.ContainsKey(p_religion))
		{
			activeReligiousCultLeaders.Add(p_religion, 0);
		}
		activeReligiousCultLeaders[p_religion]++;
	}

	public void DecreaseActiveReligiousCultLeaders(RELIGION p_religion)
	{
		if (activeReligiousCultLeaders.ContainsKey(p_religion))
		{
			activeReligiousCultLeaders[p_religion]--;
		}
	}

	public bool HasActiveReligiousCultLeaderOfType(RELIGION p_religion)
	{
		if (activeReligiousCultLeaders.ContainsKey(p_religion))
		{
			return activeReligiousCultLeaders[p_religion] > 0;
		}
		return false;
	}

	public bool IsCultistOfSameReligion(Character p_character1, Character p_character2)
	{
		if (p_character1.traitContainer.IsReligiousCultist(out var p_religion) && p_character2.traitContainer.IsReligiousCultist(out var p_religion2))
		{
			return p_religion == p_religion2;
		}
		return false;
	}

	public void AddPowerLocker(Character p_character)
	{
		powerLockers.Add(p_character);
	}

	public void RemovePowerLocker(Character p_character)
	{
		powerLockers.Remove(p_character);
	}

	public int GetNumberOfSkillsThatShouldBeLocked()
	{
		return powerLockers.Count;
	}

	public bool CanCharacterWorkAt(Character p_character, STRUCTURE_TYPE p_structureType)
	{
		if (p_character.gridTileLocation != null && p_character.gridTileLocation.structure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS)
		{
			return false;
		}
		if (p_character.traitContainer.HasTrait("Paralyzed") && !p_character.traitContainer.HasScheduleTicket("Paralyzed"))
		{
			return false;
		}
		if (p_character.traitContainer.HasTrait("Restrained"))
		{
			return false;
		}
		if (p_character.characterClass.workStructureType == p_structureType && !p_character.classComponent.shouldChangeClass)
		{
			return true;
		}
		switch (p_structureType)
		{
		case STRUCTURE_TYPE.BUTCHERS_SHOP:
			if (p_character.classComponent.HasAbleClass("Butcher") && p_character.classComponent.shouldChangeClass)
			{
				return true;
			}
			break;
		case STRUCTURE_TYPE.FISHERY:
			if (p_character.classComponent.HasAbleClass("Fisher") && p_character.classComponent.shouldChangeClass)
			{
				return true;
			}
			break;
		case STRUCTURE_TYPE.HUNTER_LODGE:
			if (p_character.classComponent.HasAbleClass("Skinner") && p_character.classComponent.shouldChangeClass)
			{
				return true;
			}
			break;
		case STRUCTURE_TYPE.FARM:
			if (p_character.classComponent.HasAbleClass("Farmer") && p_character.classComponent.shouldChangeClass)
			{
				return true;
			}
			break;
		}
		return false;
	}

	private BaseVisionTrigger GetVisionTriggerFromCache(Collider2D p_collider)
	{
		if (_poiVisionCache.ContainsKey(p_collider))
		{
			return _poiVisionCache[p_collider];
		}
		return null;
	}

	private void AddPOIVisionCache(Collider2D p_collider, BaseVisionTrigger p_visionTrigger)
	{
		_poiVisionCache.Add(p_collider, p_visionTrigger);
	}

	public BaseVisionTrigger GetAndAddPOIVisionTriggerFromCache(Collider2D p_collider)
	{
		BaseVisionTrigger baseVisionTrigger = GetVisionTriggerFromCache(p_collider);
		if (baseVisionTrigger == null)
		{
			baseVisionTrigger = p_collider.gameObject.GetComponent<BaseVisionTrigger>();
			if (baseVisionTrigger != null)
			{
				AddPOIVisionCache(p_collider, baseVisionTrigger);
			}
		}
		return baseVisionTrigger;
	}

	private Projectile GetProjectileFromCache(Collider2D p_collider)
	{
		if (_projectileCache.ContainsKey(p_collider))
		{
			return _projectileCache[p_collider];
		}
		return null;
	}

	private void AddProjectileCache(Collider2D p_collider, Projectile p_projectile)
	{
		_projectileCache.Add(p_collider, p_projectile);
	}

	public Projectile GetAndAddProjectileFromCache(Collider2D p_collider)
	{
		Projectile projectile = GetProjectileFromCache(p_collider);
		if (projectile == null)
		{
			projectile = p_collider.gameObject.GetComponent<Projectile>();
			if (projectile != null)
			{
				AddProjectileCache(p_collider, projectile);
			}
		}
		return projectile;
	}

	public void AddCharacterToBeCleanedUp(Character p_character)
	{
		if (!_charactersToCleanUp.Contains(p_character))
		{
			_charactersToCleanUp.Add(p_character);
		}
	}

	public void ProcessCharactersMarkedForCleanUp()
	{
		for (int i = 0; i < _charactersToCleanUp.Count; i++)
		{
			Character character = _charactersToCleanUp[i];
			RemoveCharacter(character);
			DatabaseManager.Instance.characterDatabase.CleanUpCharacter(character);
			if (ConsoleBase.checkCharacterReferences)
			{
				DatabaseManager.Instance.CheckIfCharacterIsStillReferenced(character);
			}
		}
		_charactersToCleanUp.Clear();
	}
}
