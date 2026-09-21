using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UtilityScripts;

public class PlayerSkillManager : MonoBehaviour
{
	public static PlayerSkillManager Instance;

	public PlayerSkillTree[] allSkillTrees;

	public PlayerArchetypeLoadoutDictionary allSkillLoadouts;

	[SerializeField]
	private bool _unlockAllSkills;

	[SerializeField]
	private bool _unlimitedCast;

	[SerializeField]
	private Color _withChaosOrbsTextColor;

	[SerializeField]
	private Color _withoutChaosOrbsTextColor;

	[SerializeField]
	private PlayerSkillDataDictionary _playerSkillDataDictionary;

	public Dictionary<string, PLAYER_SKILL_TYPE> afflictionsNameSkillTypeDictionary = new Dictionary<string, PLAYER_SKILL_TYPE>
	{
		{
			"Agoraphobic",
			PLAYER_SKILL_TYPE.AGORAPHOBIA
		},
		{
			"Alcoholic",
			PLAYER_SKILL_TYPE.ALCOHOLIC
		},
		{
			"Cannibal",
			PLAYER_SKILL_TYPE.CANNIBALISM
		},
		{
			"Coward",
			PLAYER_SKILL_TYPE.COWARDICE
		},
		{
			"Glutton",
			PLAYER_SKILL_TYPE.GLUTTONY
		},
		{
			"Hothead",
			PLAYER_SKILL_TYPE.HOTHEADED
		},
		{
			"Kleptomaniac",
			PLAYER_SKILL_TYPE.KLEPTOMANIA
		},
		{
			"Lazy",
			PLAYER_SKILL_TYPE.GLUTTONY
		},
		{
			"Lycanthrope",
			PLAYER_SKILL_TYPE.LYCANTHROPY
		},
		{
			"Music Hater",
			PLAYER_SKILL_TYPE.MUSIC_HATER
		},
		{
			"Narcoleptic",
			PLAYER_SKILL_TYPE.NARCOLEPSY
		},
		{
			"Paralyzed",
			PLAYER_SKILL_TYPE.PARALYSIS
		},
		{
			"Plagued",
			PLAYER_SKILL_TYPE.PLAGUE
		},
		{
			"Psychopath",
			PLAYER_SKILL_TYPE.PSYCHOPATHY
		},
		{
			"Pyrophobic",
			PLAYER_SKILL_TYPE.PYROPHOBIA
		},
		{
			"Unfaithful",
			PLAYER_SKILL_TYPE.UNFAITHFULNESS
		},
		{
			"Vampire",
			PLAYER_SKILL_TYPE.VAMPIRISM
		}
	};

	[NonSerialized]
	public List<PLAYER_SKILL_TYPE> constantSkills = new List<PLAYER_SKILL_TYPE>
	{
		PLAYER_SKILL_TYPE.AFFLICT,
		PLAYER_SKILL_TYPE.BUILD_DEMONIC_STRUCTURE,
		PLAYER_SKILL_TYPE.UNSUMMON,
		PLAYER_SKILL_TYPE.TORTURE,
		PLAYER_SKILL_TYPE.BRAINWASH,
		PLAYER_SKILL_TYPE.EVANGELIZE,
		PLAYER_SKILL_TYPE.CULTIST_POISON,
		PLAYER_SKILL_TYPE.SACRIFICE,
		PLAYER_SKILL_TYPE.REPAIR,
		PLAYER_SKILL_TYPE.FOUND_CULT,
		PLAYER_SKILL_TYPE.SPREAD_RUMOR,
		PLAYER_SKILL_TYPE.CULTIST_BOOBY_TRAP,
		PLAYER_SKILL_TYPE.UPGRADE,
		PLAYER_SKILL_TYPE.INSTIGATE_WAR,
		PLAYER_SKILL_TYPE.RESIGN,
		PLAYER_SKILL_TYPE.LEAVE_FACTION,
		PLAYER_SKILL_TYPE.LEAVE_HOME,
		PLAYER_SKILL_TYPE.LEAVE_VILLAGE,
		PLAYER_SKILL_TYPE.BREAK_UP,
		PLAYER_SKILL_TYPE.JOIN_FACTION,
		PLAYER_SKILL_TYPE.REBELLION,
		PLAYER_SKILL_TYPE.SCHEME,
		PLAYER_SKILL_TYPE.OVERTHROW_LEADER,
		PLAYER_SKILL_TYPE.STIFLE_MIGRATION,
		PLAYER_SKILL_TYPE.INDUCE_MIGRATION,
		PLAYER_SKILL_TYPE.CULTIST_JOIN_FACTION,
		PLAYER_SKILL_TYPE.SPAWN_EYE_WARD,
		PLAYER_SKILL_TYPE.DESTROY_EYE_WARD,
		PLAYER_SKILL_TYPE.DRAIN_SPIRIT,
		PLAYER_SKILL_TYPE.LET_GO,
		PLAYER_SKILL_TYPE.FULL_HEAL,
		PLAYER_SKILL_TYPE.CREATE_BLACKMAIL,
		PLAYER_SKILL_TYPE.RELEASE_ABILITIES,
		PLAYER_SKILL_TYPE.SNATCH_VILLAGER,
		PLAYER_SKILL_TYPE.SNATCH_MONSTER,
		PLAYER_SKILL_TYPE.RAID,
		PLAYER_SKILL_TYPE.UPGRADE_ABILITIES,
		PLAYER_SKILL_TYPE.DEFEND,
		PLAYER_SKILL_TYPE.UPGRADE_PORTAL,
		PLAYER_SKILL_TYPE.DESTROY_STRUCTURE,
		PLAYER_SKILL_TYPE.SPAWN_PARTY,
		PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_EYE_LEVEL,
		PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_RADIUS_LEVEL,
		PLAYER_SKILL_TYPE.UPGRADE_MONSTERS_STATS,
		PLAYER_SKILL_TYPE.MONSTER_SPAWNER,
		PLAYER_SKILL_TYPE.ABSORB_CULTIST,
		PLAYER_SKILL_TYPE.FABRICATE_CRIME,
		PLAYER_SKILL_TYPE.SNATCH_OBJECT,
		PLAYER_SKILL_TYPE.ATTACK_VILLAGE,
		PLAYER_SKILL_TYPE.TRIGGER_GRUDGE,
		PLAYER_SKILL_TYPE.UNDEPLOY_PARTY,
		PLAYER_SKILL_TYPE.LURE,
		PLAYER_SKILL_TYPE.INFUSE,
		PLAYER_SKILL_TYPE.CORRUPT_TILE,
		PLAYER_SKILL_TYPE.DEMONIC_WALL,
		PLAYER_SKILL_TYPE.DECORATIONS,
		PLAYER_SKILL_TYPE.DEMOLISH,
		PLAYER_SKILL_TYPE.EVENTS,
		PLAYER_SKILL_TYPE.ASSASSINATE,
		PLAYER_SKILL_TYPE.CREATE_CAPTIVE_INTEL,
		PLAYER_SKILL_TYPE.CRITICAL_BREAK,
		PLAYER_SKILL_TYPE.KILL_VILLAGER,
		PLAYER_SKILL_TYPE.DESTROY_SUPPLIES,
		PLAYER_SKILL_TYPE.DESTROY_STRUCTURES,
		PLAYER_SKILL_TYPE.HARASS_VILLAGERS,
		PLAYER_SKILL_TYPE.DESTROY_DEFENSES,
		PLAYER_SKILL_TYPE.CLEAR_VILLAGE,
		PLAYER_SKILL_TYPE.FOUND_FACTION
	};

	[NonSerialized]
	public PLAYER_SKILL_TYPE[] allSpells = new PLAYER_SKILL_TYPE[40]
	{
		PLAYER_SKILL_TYPE.METEOR,
		PLAYER_SKILL_TYPE.TORNADO,
		PLAYER_SKILL_TYPE.RAVENOUS_SPIRIT,
		PLAYER_SKILL_TYPE.FEEBLE_SPIRIT,
		PLAYER_SKILL_TYPE.FORLORN_SPIRIT,
		PLAYER_SKILL_TYPE.LIGHTNING,
		PLAYER_SKILL_TYPE.POISON_CLOUD,
		PLAYER_SKILL_TYPE.EARTHQUAKE,
		PLAYER_SKILL_TYPE.MANIFEST_FOOD,
		PLAYER_SKILL_TYPE.BRIMSTONES,
		PLAYER_SKILL_TYPE.SPLASH_POISON,
		PLAYER_SKILL_TYPE.LOCUST_SWARM,
		PLAYER_SKILL_TYPE.BLIZZARD,
		PLAYER_SKILL_TYPE.RAIN,
		PLAYER_SKILL_TYPE.BALL_LIGHTNING,
		PLAYER_SKILL_TYPE.ELECTRIC_STORM,
		PLAYER_SKILL_TYPE.FROSTY_FOG,
		PLAYER_SKILL_TYPE.VAPOR,
		PLAYER_SKILL_TYPE.FIRE_BALL,
		PLAYER_SKILL_TYPE.POISON_BLOOM,
		PLAYER_SKILL_TYPE.LANDMINE,
		PLAYER_SKILL_TYPE.TERRIFYING_HOWL,
		PLAYER_SKILL_TYPE.FREEZING_TRAP,
		PLAYER_SKILL_TYPE.SNARE_TRAP,
		PLAYER_SKILL_TYPE.WIND_BLAST,
		PLAYER_SKILL_TYPE.ICE_BLAST,
		PLAYER_SKILL_TYPE.EARTH_SPIKE,
		PLAYER_SKILL_TYPE.WATER_SPIKE,
		PLAYER_SKILL_TYPE.ICETEROIDS,
		PLAYER_SKILL_TYPE.HEAT_WAVE,
		PLAYER_SKILL_TYPE.SPLASH_WATER,
		PLAYER_SKILL_TYPE.WALL,
		PLAYER_SKILL_TYPE.PROTECTION,
		PLAYER_SKILL_TYPE.PLAGUED_RAT,
		PLAYER_SKILL_TYPE.SPAWN_NECRONOMICON,
		PLAYER_SKILL_TYPE.SPAWN_RATMAN,
		PLAYER_SKILL_TYPE.MONSTER_SPAWNER,
		PLAYER_SKILL_TYPE.SUMMON_SKELETONS,
		PLAYER_SKILL_TYPE.LICH_GRAVEYARD,
		PLAYER_SKILL_TYPE.STAMPEDE
	};

	[NonSerialized]
	public PLAYER_SKILL_TYPE[] allPlayerActions = new PLAYER_SKILL_TYPE[70]
	{
		PLAYER_SKILL_TYPE.ZAP,
		PLAYER_SKILL_TYPE.RAISE_DEAD,
		PLAYER_SKILL_TYPE.DESTROY,
		PLAYER_SKILL_TYPE.IGNITE,
		PLAYER_SKILL_TYPE.POISON,
		PLAYER_SKILL_TYPE.TORTURE,
		PLAYER_SKILL_TYPE.SEIZE_OBJECT,
		PLAYER_SKILL_TYPE.SEIZE_CHARACTER,
		PLAYER_SKILL_TYPE.SEIZE_MONSTER,
		PLAYER_SKILL_TYPE.BUILD_DEMONIC_STRUCTURE,
		PLAYER_SKILL_TYPE.AFFLICT,
		PLAYER_SKILL_TYPE.AGITATE,
		PLAYER_SKILL_TYPE.HEAL,
		PLAYER_SKILL_TYPE.BRAINWASH,
		PLAYER_SKILL_TYPE.UNSUMMON,
		PLAYER_SKILL_TYPE.TRIGGER_FLAW,
		PLAYER_SKILL_TYPE.CULTIST_POISON,
		PLAYER_SKILL_TYPE.CULTIST_BOOBY_TRAP,
		PLAYER_SKILL_TYPE.SACRIFICE,
		PLAYER_SKILL_TYPE.REPAIR,
		PLAYER_SKILL_TYPE.SPREAD_RUMOR,
		PLAYER_SKILL_TYPE.EVANGELIZE,
		PLAYER_SKILL_TYPE.FOUND_CULT,
		PLAYER_SKILL_TYPE.UPGRADE,
		PLAYER_SKILL_TYPE.SCHEME,
		PLAYER_SKILL_TYPE.RELEASE,
		PLAYER_SKILL_TYPE.EXPEL,
		PLAYER_SKILL_TYPE.REMOVE_BUFF,
		PLAYER_SKILL_TYPE.REMOVE_FLAW,
		PLAYER_SKILL_TYPE.CULTIST_JOIN_FACTION,
		PLAYER_SKILL_TYPE.SPAWN_EYE_WARD,
		PLAYER_SKILL_TYPE.DESTROY_EYE_WARD,
		PLAYER_SKILL_TYPE.DRAIN_SPIRIT,
		PLAYER_SKILL_TYPE.LET_GO,
		PLAYER_SKILL_TYPE.FULL_HEAL,
		PLAYER_SKILL_TYPE.CREATE_BLACKMAIL,
		PLAYER_SKILL_TYPE.RELEASE_ABILITIES,
		PLAYER_SKILL_TYPE.SNATCH_VILLAGER,
		PLAYER_SKILL_TYPE.SNATCH_MONSTER,
		PLAYER_SKILL_TYPE.RAID,
		PLAYER_SKILL_TYPE.UPGRADE_ABILITIES,
		PLAYER_SKILL_TYPE.DEFEND,
		PLAYER_SKILL_TYPE.UPGRADE_PORTAL,
		PLAYER_SKILL_TYPE.DESTROY_STRUCTURE,
		PLAYER_SKILL_TYPE.EMPOWER,
		PLAYER_SKILL_TYPE.SPAWN_PARTY,
		PLAYER_SKILL_TYPE.INDUCE_MIGRATION,
		PLAYER_SKILL_TYPE.STIFLE_MIGRATION,
		PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_EYE_LEVEL,
		PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_RADIUS_LEVEL,
		PLAYER_SKILL_TYPE.UPGRADE_MONSTERS_STATS,
		PLAYER_SKILL_TYPE.ABSORB_CULTIST,
		PLAYER_SKILL_TYPE.FABRICATE_CRIME,
		PLAYER_SKILL_TYPE.SNATCH_OBJECT,
		PLAYER_SKILL_TYPE.ATTACK_VILLAGE,
		PLAYER_SKILL_TYPE.FINGER_OF_DEATH,
		PLAYER_SKILL_TYPE.HELLSPAWN,
		PLAYER_SKILL_TYPE.TRIGGER_AROUSAL,
		PLAYER_SKILL_TYPE.TRIGGER_GRUDGE,
		PLAYER_SKILL_TYPE.UNDEPLOY_PARTY,
		PLAYER_SKILL_TYPE.LURE,
		PLAYER_SKILL_TYPE.INFUSE,
		PLAYER_SKILL_TYPE.EVENTS,
		PLAYER_SKILL_TYPE.CREATE_CAPTIVE_INTEL,
		PLAYER_SKILL_TYPE.CRITICAL_BREAK,
		PLAYER_SKILL_TYPE.KILL_VILLAGER,
		PLAYER_SKILL_TYPE.CLEAR_VILLAGE,
		PLAYER_SKILL_TYPE.REFRESH,
		PLAYER_SKILL_TYPE.FOUND_FACTION,
		PLAYER_SKILL_TYPE.GLOOM
	};

	[NonSerialized]
	public PLAYER_SKILL_TYPE[] allAfflictions = new PLAYER_SKILL_TYPE[17]
	{
		PLAYER_SKILL_TYPE.CANNIBALISM,
		PLAYER_SKILL_TYPE.LYCANTHROPY,
		PLAYER_SKILL_TYPE.VAMPIRISM,
		PLAYER_SKILL_TYPE.KLEPTOMANIA,
		PLAYER_SKILL_TYPE.UNFAITHFULNESS,
		PLAYER_SKILL_TYPE.ALCOHOLIC,
		PLAYER_SKILL_TYPE.AGORAPHOBIA,
		PLAYER_SKILL_TYPE.PARALYSIS,
		PLAYER_SKILL_TYPE.PLAGUE,
		PLAYER_SKILL_TYPE.PSYCHOPATHY,
		PLAYER_SKILL_TYPE.COWARDICE,
		PLAYER_SKILL_TYPE.PYROPHOBIA,
		PLAYER_SKILL_TYPE.NARCOLEPSY,
		PLAYER_SKILL_TYPE.HOTHEADED,
		PLAYER_SKILL_TYPE.LAZINESS,
		PLAYER_SKILL_TYPE.MUSIC_HATER,
		PLAYER_SKILL_TYPE.GLUTTONY
	};

	[NonSerialized]
	public PLAYER_SKILL_TYPE[] allSchemes = new PLAYER_SKILL_TYPE[10]
	{
		PLAYER_SKILL_TYPE.INSTIGATE_WAR,
		PLAYER_SKILL_TYPE.RESIGN,
		PLAYER_SKILL_TYPE.LEAVE_FACTION,
		PLAYER_SKILL_TYPE.LEAVE_HOME,
		PLAYER_SKILL_TYPE.LEAVE_VILLAGE,
		PLAYER_SKILL_TYPE.BREAK_UP,
		PLAYER_SKILL_TYPE.JOIN_FACTION,
		PLAYER_SKILL_TYPE.REBELLION,
		PLAYER_SKILL_TYPE.OVERTHROW_LEADER,
		PLAYER_SKILL_TYPE.ASSASSINATE
	};

	[NonSerialized]
	public PLAYER_SKILL_TYPE[] allRaidActions = new PLAYER_SKILL_TYPE[4]
	{
		PLAYER_SKILL_TYPE.DESTROY_SUPPLIES,
		PLAYER_SKILL_TYPE.DESTROY_STRUCTURES,
		PLAYER_SKILL_TYPE.HARASS_VILLAGERS,
		PLAYER_SKILL_TYPE.DESTROY_DEFENSES
	};

	[NonSerialized]
	public PLAYER_SKILL_TYPE[] allDemonicStructureSkills = new PLAYER_SKILL_TYPE[13]
	{
		PLAYER_SKILL_TYPE.MEDDLER,
		PLAYER_SKILL_TYPE.WATCHER,
		PLAYER_SKILL_TYPE.CRYPT,
		PLAYER_SKILL_TYPE.KENNEL,
		PLAYER_SKILL_TYPE.TORTURE_CHAMBERS,
		PLAYER_SKILL_TYPE.DEFILER,
		PLAYER_SKILL_TYPE.BIOLAB,
		PLAYER_SKILL_TYPE.SPIRE,
		PLAYER_SKILL_TYPE.MANA_PIT,
		PLAYER_SKILL_TYPE.MARAUD,
		PLAYER_SKILL_TYPE.IMP_HUT,
		PLAYER_SKILL_TYPE.PRIMORDIAL_POOL,
		PLAYER_SKILL_TYPE.PRISM
	};

	[NonSerialized]
	public PLAYER_SKILL_TYPE[] allBuildSkills = new PLAYER_SKILL_TYPE[4]
	{
		PLAYER_SKILL_TYPE.CORRUPT_TILE,
		PLAYER_SKILL_TYPE.DEMONIC_WALL,
		PLAYER_SKILL_TYPE.DECORATIONS,
		PLAYER_SKILL_TYPE.DEMOLISH
	};

	[NonSerialized]
	public PLAYER_SKILL_TYPE[] allMinionPlayerSkills = new PLAYER_SKILL_TYPE[7]
	{
		PLAYER_SKILL_TYPE.DEMON_WRATH,
		PLAYER_SKILL_TYPE.DEMON_PRIDE,
		PLAYER_SKILL_TYPE.DEMON_LUST,
		PLAYER_SKILL_TYPE.DEMON_GLUTTONY,
		PLAYER_SKILL_TYPE.DEMON_SLOTH,
		PLAYER_SKILL_TYPE.DEMON_ENVY,
		PLAYER_SKILL_TYPE.DEMON_GREED
	};

	[NonSerialized]
	public PLAYER_SKILL_TYPE[] allSummonPlayerSkills = new PLAYER_SKILL_TYPE[64]
	{
		PLAYER_SKILL_TYPE.SKELETON,
		PLAYER_SKILL_TYPE.WOLF,
		PLAYER_SKILL_TYPE.GOLEM,
		PLAYER_SKILL_TYPE.INCUBUS,
		PLAYER_SKILL_TYPE.SUCCUBUS,
		PLAYER_SKILL_TYPE.FIRE_ELEMENTAL,
		PLAYER_SKILL_TYPE.KOBOLD,
		PLAYER_SKILL_TYPE.GHOST,
		PLAYER_SKILL_TYPE.ABOMINATION,
		PLAYER_SKILL_TYPE.MIMIC,
		PLAYER_SKILL_TYPE.PIG,
		PLAYER_SKILL_TYPE.CHICKEN,
		PLAYER_SKILL_TYPE.SHEEP,
		PLAYER_SKILL_TYPE.SLUDGE,
		PLAYER_SKILL_TYPE.WATER_NYMPH,
		PLAYER_SKILL_TYPE.WIND_NYMPH,
		PLAYER_SKILL_TYPE.ICE_NYMPH,
		PLAYER_SKILL_TYPE.ELECTRIC_WISP,
		PLAYER_SKILL_TYPE.EARTHEN_WISP,
		PLAYER_SKILL_TYPE.FIRE_WISP,
		PLAYER_SKILL_TYPE.GRASS_ENT,
		PLAYER_SKILL_TYPE.SNOW_ENT,
		PLAYER_SKILL_TYPE.CORRUPT_ENT,
		PLAYER_SKILL_TYPE.DESERT_ENT,
		PLAYER_SKILL_TYPE.FOREST_ENT,
		PLAYER_SKILL_TYPE.GIANT_SPIDER,
		PLAYER_SKILL_TYPE.SMALL_SPIDER,
		PLAYER_SKILL_TYPE.VENGEFUL_GHOST,
		PLAYER_SKILL_TYPE.WURM,
		PLAYER_SKILL_TYPE.TROLL,
		PLAYER_SKILL_TYPE.REVENANT,
		PLAYER_SKILL_TYPE.BONE_GOLEM,
		PLAYER_SKILL_TYPE.SCORPION,
		PLAYER_SKILL_TYPE.HARPY,
		PLAYER_SKILL_TYPE.DRAGON,
		PLAYER_SKILL_TYPE.MINK,
		PLAYER_SKILL_TYPE.MOONWALKER,
		PLAYER_SKILL_TYPE.RABBIT,
		PLAYER_SKILL_TYPE.RAT,
		PLAYER_SKILL_TYPE.BEAR,
		PLAYER_SKILL_TYPE.BOAR,
		PLAYER_SKILL_TYPE.MAGICAL_ANGEL,
		PLAYER_SKILL_TYPE.WARRIOR_ANGEL,
		PLAYER_SKILL_TYPE.IMP,
		PLAYER_SKILL_TYPE.BROODMOTHER,
		PLAYER_SKILL_TYPE.CENTAUR,
		PLAYER_SKILL_TYPE.GHOUL,
		PLAYER_SKILL_TYPE.MOTHMAN,
		PLAYER_SKILL_TYPE.ORC,
		PLAYER_SKILL_TYPE.TARANTULA,
		PLAYER_SKILL_TYPE.WHISPERER,
		PLAYER_SKILL_TYPE.FALLEN_ANGEL,
		PLAYER_SKILL_TYPE.NATURE_SPIRIT,
		PLAYER_SKILL_TYPE.UNICORN,
		PLAYER_SKILL_TYPE.WYVERN,
		PLAYER_SKILL_TYPE.WYVERNLING,
		PLAYER_SKILL_TYPE.GORGON,
		PLAYER_SKILL_TYPE.IFRIT,
		PLAYER_SKILL_TYPE.SPLATTER,
		PLAYER_SKILL_TYPE.GOBLIN,
		PLAYER_SKILL_TYPE.DWARF_KING,
		PLAYER_SKILL_TYPE.DWARF_PALADIN,
		PLAYER_SKILL_TYPE.TRITON,
		PLAYER_SKILL_TYPE.DIRE_WOLF
	};

	[NonSerialized]
	public PASSIVE_SKILL[] allPassiveSkillTypes = new PASSIVE_SKILL[14]
	{
		PASSIVE_SKILL.Prayer_Chaos_Orb,
		PASSIVE_SKILL.Auto_Absorb_Chaos_Orb,
		PASSIVE_SKILL.Spell_Damage_Chaos_Orb,
		PASSIVE_SKILL.Plague_Chaos_Orb,
		PASSIVE_SKILL.Player_Success_Raid_Chaos_Orb,
		PASSIVE_SKILL.Dark_Ritual_Chaos_Orb,
		PASSIVE_SKILL.Raid_Chaos_Orb,
		PASSIVE_SKILL.Night_Creature_Chaos_Orb,
		PASSIVE_SKILL.Meddler_Chaos_Orb,
		PASSIVE_SKILL.Trigger_Flaw_Chaos_Orb,
		PASSIVE_SKILL.Lycanthrope_Chaos_Orb,
		PASSIVE_SKILL.Trap_Chaos_Orb,
		PASSIVE_SKILL.Skill_Base_Chaos_Orb,
		PASSIVE_SKILL.Damage_Chaos_Orb_Threshold_Reduction
	};

	public PLAYER_ARCHETYPE selectedArchetype { get; private set; }

	public Color withChaosOrbsTextColor => _withChaosOrbsTextColor;

	public Color withoutChaosOrbsTextColor => _withoutChaosOrbsTextColor;

	public bool unlimitedCast => false;

	public bool unlockAllSkills => WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled;

	public Dictionary<PLAYER_SKILL_TYPE, SkillData> allSpellsData { get; private set; }

	public Dictionary<PLAYER_SKILL_TYPE, PlayerAction> allPlayerActionsData { get; private set; }

	public Dictionary<PLAYER_SKILL_TYPE, AfflictData> allAfflictionsData { get; private set; }

	public Dictionary<PLAYER_SKILL_TYPE, SchemeData> allSchemesData { get; private set; }

	public Dictionary<PLAYER_SKILL_TYPE, RaidData> allRaidData { get; private set; }

	public Dictionary<PLAYER_SKILL_TYPE, DemonicStructurePlayerSkill> allDemonicStructureSkillsData { get; private set; }

	public Dictionary<PLAYER_SKILL_TYPE, BuildPlayerSkill> allBuildSkillsData { get; private set; }

	public Dictionary<PLAYER_SKILL_TYPE, MinionPlayerSkill> allMinionPlayerSkillsData { get; private set; }

	public Dictionary<PLAYER_SKILL_TYPE, SummonPlayerSkill> allSummonPlayerSkillsData { get; private set; }

	public Dictionary<SUMMON_TYPE, SummonPlayerSkill> allSummonPlayerSkillsDataBySummonType { get; private set; }

	public Dictionary<PLAYER_SKILL_TYPE, SkillData> allPlayerSkillsData { get; private set; }

	public Dictionary<PASSIVE_SKILL, PassiveSkill> passiveSkillsData { get; private set; }

	public PlayerSkillDataDictionary playerSkillDataDictionary => _playerSkillDataDictionary;

	public int monsterSpawnerMaxCapacity => GetMonsterSpawnerMaxCapacity();

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		Initialize();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
		}
	}

	public void Initialize()
	{
		allPlayerSkillsData = new Dictionary<PLAYER_SKILL_TYPE, SkillData>();
		ConstructAllSpellsData();
		ConstructAllPlayerActionsData();
		ConstructAllAfflictionsData();
		ConstructAllSchemesData();
		ConstructAllRaidData();
		ConstructAllDemonicStructureSkillsData();
		ConstructAllBuildSkillsData();
		ConstructAllMinionPlayerSkillsData();
		ConstructAllSummonPlayerSkillsData();
		ConstructPassiveSkills();
	}

	private void ConstructAllSpellsData()
	{
		allSpellsData = new Dictionary<PLAYER_SKILL_TYPE, SkillData>(41);
		for (int i = 0; i < allSpells.Length; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = allSpells[i];
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.NONE)
			{
				string text = pLAYER_SKILL_TYPE.ToStringEnumNoSpace() + "Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
				SkillData value = Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("Problem with creating spell data for " + text)) as SkillData;
				allSpellsData.Add(pLAYER_SKILL_TYPE, value);
				allPlayerSkillsData.Add(pLAYER_SKILL_TYPE, value);
			}
		}
	}

	private void ConstructAllPlayerActionsData()
	{
		allPlayerActionsData = new Dictionary<PLAYER_SKILL_TYPE, PlayerAction>(62);
		for (int i = 0; i < allPlayerActions.Length; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = allPlayerActions[i];
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.NONE)
			{
				string text = pLAYER_SKILL_TYPE.ToStringEnumNoSpace() + "Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
				PlayerAction value = Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("Problem with creating spell data for " + text)) as PlayerAction;
				allPlayerActionsData.Add(pLAYER_SKILL_TYPE, value);
				allPlayerSkillsData.Add(pLAYER_SKILL_TYPE, value);
			}
		}
	}

	private void ConstructAllDemonicStructureSkillsData()
	{
		allDemonicStructureSkillsData = new Dictionary<PLAYER_SKILL_TYPE, DemonicStructurePlayerSkill>(13);
		for (int i = 0; i < allDemonicStructureSkills.Length; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = allDemonicStructureSkills[i];
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.NONE)
			{
				string text = pLAYER_SKILL_TYPE.ToStringEnumNoSpace() + "Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
				DemonicStructurePlayerSkill value = Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("Problem with creating spell data for " + text)) as DemonicStructurePlayerSkill;
				allDemonicStructureSkillsData.Add(pLAYER_SKILL_TYPE, value);
				allPlayerSkillsData.Add(pLAYER_SKILL_TYPE, value);
			}
		}
	}

	private void ConstructAllBuildSkillsData()
	{
		allBuildSkillsData = new Dictionary<PLAYER_SKILL_TYPE, BuildPlayerSkill>();
		for (int i = 0; i < allBuildSkills.Length; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = allBuildSkills[i];
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.NONE)
			{
				string text = pLAYER_SKILL_TYPE.ToStringEnumNoSpace() + "Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
				BuildPlayerSkill value = Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("Problem with creating spell data for " + text)) as BuildPlayerSkill;
				allBuildSkillsData.Add(pLAYER_SKILL_TYPE, value);
				allPlayerSkillsData.Add(pLAYER_SKILL_TYPE, value);
			}
		}
	}

	private void ConstructAllAfflictionsData()
	{
		allAfflictionsData = new Dictionary<PLAYER_SKILL_TYPE, AfflictData>(17);
		for (int i = 0; i < allAfflictions.Length; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = allAfflictions[i];
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.NONE)
			{
				string text = pLAYER_SKILL_TYPE.ToStringEnumNoSpace() + "Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
				AfflictData value = Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("Problem with creating spell data for " + text)) as AfflictData;
				allAfflictionsData.Add(pLAYER_SKILL_TYPE, value);
				allPlayerSkillsData.Add(pLAYER_SKILL_TYPE, value);
			}
		}
	}

	private void ConstructAllSchemesData()
	{
		allSchemesData = new Dictionary<PLAYER_SKILL_TYPE, SchemeData>(9);
		for (int i = 0; i < allSchemes.Length; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = allSchemes[i];
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.NONE)
			{
				string text = pLAYER_SKILL_TYPE.ToStringEnumNoSpace() + "Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
				SchemeData value = Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("Problem with creating spell data for " + text)) as SchemeData;
				allSchemesData.Add(pLAYER_SKILL_TYPE, value);
				allPlayerSkillsData.Add(pLAYER_SKILL_TYPE, value);
			}
		}
	}

	private void ConstructAllRaidData()
	{
		allRaidData = new Dictionary<PLAYER_SKILL_TYPE, RaidData>(9);
		for (int i = 0; i < allRaidActions.Length; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = allRaidActions[i];
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.NONE)
			{
				string text = pLAYER_SKILL_TYPE.ToStringEnumNoSpace() + "Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
				RaidData value = Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("Problem with creating spell data for " + text)) as RaidData;
				allRaidData.Add(pLAYER_SKILL_TYPE, value);
				allPlayerSkillsData.Add(pLAYER_SKILL_TYPE, value);
			}
		}
	}

	private void ConstructAllMinionPlayerSkillsData()
	{
		allMinionPlayerSkillsData = new Dictionary<PLAYER_SKILL_TYPE, MinionPlayerSkill>(7);
		for (int i = 0; i < allMinionPlayerSkills.Length; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = allMinionPlayerSkills[i];
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.NONE)
			{
				string text = pLAYER_SKILL_TYPE.ToStringEnumNoSpace() + "Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
				MinionPlayerSkill value = Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("Problem with creating spell data for " + text)) as MinionPlayerSkill;
				allMinionPlayerSkillsData.Add(pLAYER_SKILL_TYPE, value);
				allPlayerSkillsData.Add(pLAYER_SKILL_TYPE, value);
			}
		}
	}

	private void ConstructAllSummonPlayerSkillsData()
	{
		allSummonPlayerSkillsData = new Dictionary<PLAYER_SKILL_TYPE, SummonPlayerSkill>(57);
		allSummonPlayerSkillsDataBySummonType = new Dictionary<SUMMON_TYPE, SummonPlayerSkill>();
		for (int i = 0; i < allSummonPlayerSkills.Length; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = allSummonPlayerSkills[i];
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.NONE)
			{
				string text = pLAYER_SKILL_TYPE.ToStringEnumNoSpace() + "Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
				SummonPlayerSkill summonPlayerSkill = Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("Problem with creating spell data for " + text)) as SummonPlayerSkill;
				allSummonPlayerSkillsData.Add(pLAYER_SKILL_TYPE, summonPlayerSkill);
				allSummonPlayerSkillsDataBySummonType.Add(summonPlayerSkill.summonType, summonPlayerSkill);
				allPlayerSkillsData.Add(pLAYER_SKILL_TYPE, summonPlayerSkill);
			}
		}
	}

	public bool IsSpell(PLAYER_SKILL_TYPE type)
	{
		return allSpells.Contains(type);
	}

	public bool IsAffliction(PLAYER_SKILL_TYPE type)
	{
		return allAfflictions.Contains(type);
	}

	public bool IsScheme(PLAYER_SKILL_TYPE type)
	{
		return allSchemes.Contains(type);
	}

	public bool IsMinion(PLAYER_SKILL_TYPE type)
	{
		return allMinionPlayerSkills.Contains(type);
	}

	public bool IsPlayerAction(PLAYER_SKILL_TYPE type)
	{
		return allPlayerActions.Contains(type);
	}

	public bool IsDemonicStructure(PLAYER_SKILL_TYPE type)
	{
		return allDemonicStructureSkills.Contains(type);
	}

	public SkillData GetSkillData(PLAYER_SKILL_TYPE type)
	{
		if (allPlayerSkillsData.ContainsKey(type))
		{
			return allPlayerSkillsData[type];
		}
		return null;
	}

	public SkillData GetSpellData(PLAYER_SKILL_TYPE type)
	{
		if (allSpellsData.ContainsKey(type))
		{
			return allSpellsData[type];
		}
		return null;
	}

	public AfflictData GetAfflictionData(PLAYER_SKILL_TYPE type)
	{
		if (allAfflictionsData.ContainsKey(type))
		{
			return allAfflictionsData[type];
		}
		return null;
	}

	public SchemeData GetSchemeData(PLAYER_SKILL_TYPE type)
	{
		if (allSchemesData.ContainsKey(type))
		{
			return allSchemesData[type];
		}
		return null;
	}

	public RaidData GetRaidData(PLAYER_SKILL_TYPE type)
	{
		if (allRaidData.ContainsKey(type))
		{
			return allRaidData[type];
		}
		return null;
	}

	public PlayerAction GetPlayerActionData(PLAYER_SKILL_TYPE type)
	{
		if (allPlayerActionsData.ContainsKey(type))
		{
			return allPlayerActionsData[type];
		}
		return null;
	}

	public DemonicStructurePlayerSkill GetDemonicStructureSkillData(PLAYER_SKILL_TYPE type)
	{
		if (allDemonicStructureSkillsData.ContainsKey(type))
		{
			return allDemonicStructureSkillsData[type];
		}
		return null;
	}

	public BuildPlayerSkill GetBuildSkillData(PLAYER_SKILL_TYPE type)
	{
		if (allBuildSkillsData.ContainsKey(type))
		{
			return allBuildSkillsData[type];
		}
		return null;
	}

	public DemonicStructurePlayerSkill GetDemonicStructureSkillData(STRUCTURE_TYPE type)
	{
		PLAYER_SKILL_TYPE type2 = (PLAYER_SKILL_TYPE)Enum.Parse(typeof(PLAYER_SKILL_TYPE), type.ToStringEnum());
		return GetDemonicStructureSkillData(type2);
	}

	public MinionPlayerSkill GetMinionPlayerSkillData(PLAYER_SKILL_TYPE type)
	{
		if (allMinionPlayerSkillsData.ContainsKey(type))
		{
			return allMinionPlayerSkillsData[type];
		}
		return null;
	}

	public MinionPlayerSkill GetMinionPlayerSkillDataByMinionType(MINION_TYPE type)
	{
		for (int i = 0; i < allMinionPlayerSkills.Length; i++)
		{
			MinionPlayerSkill minionPlayerSkillData = GetMinionPlayerSkillData(allMinionPlayerSkills[i]);
			if (minionPlayerSkillData.minionType == type)
			{
				return minionPlayerSkillData;
			}
		}
		return null;
	}

	public SummonPlayerSkill GetSummonPlayerSkillData(PLAYER_SKILL_TYPE type)
	{
		if (allSummonPlayerSkillsData.ContainsKey(type))
		{
			return allSummonPlayerSkillsData[type];
		}
		return null;
	}

	public SummonPlayerSkill GetSummonPlayerSkillData(RACE race, string className)
	{
		foreach (SummonPlayerSkill value in allSummonPlayerSkillsData.Values)
		{
			if (value.race == race && value.className == className)
			{
				return value;
			}
		}
		return null;
	}

	public SummonPlayerSkill GetSummonPlayerSkillData(SUMMON_TYPE type)
	{
		if (allSummonPlayerSkillsDataBySummonType.ContainsKey(type))
		{
			return allSummonPlayerSkillsDataBySummonType[type];
		}
		return null;
	}

	public PLAYER_SKILL_TYPE GetSummonPlayerSkillDataType(SUMMON_TYPE type)
	{
		return GetSummonPlayerSkillData(type)?.type ?? PLAYER_SKILL_TYPE.NONE;
	}

	public PlayerSkillTreeNode GetPlayerSkillTreeNode(PLAYER_SKILL_TYPE skillType)
	{
		for (int i = 0; i < allSkillTrees.Length; i++)
		{
			PlayerSkillTree playerSkillTree = allSkillTrees[i];
			if (playerSkillTree.nodes.ContainsKey(skillType))
			{
				return playerSkillTree.nodes[skillType];
			}
		}
		return null;
	}

	public void SetSelectedArchetype(PLAYER_ARCHETYPE archetype)
	{
		selectedArchetype = archetype;
	}

	public PlayerSkillLoadout GetSelectedLoadout()
	{
		if (selectedArchetype == PLAYER_ARCHETYPE.Normal)
		{
			selectedArchetype = PLAYER_ARCHETYPE.Progression_Ravager;
		}
		return allSkillLoadouts[selectedArchetype];
	}

	public void GetDifferentArchetypeLoadouts(List<PlayerSkillLoadout> p_loadouts)
	{
		if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Progression)
		{
			if (selectedArchetype != PLAYER_ARCHETYPE.Progression_Puppet_Master)
			{
				p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Progression_Puppet_Master));
			}
			if (selectedArchetype != PLAYER_ARCHETYPE.Progression_Ravager)
			{
				p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Progression_Ravager));
			}
			if (selectedArchetype != PLAYER_ARCHETYPE.Progression_Lich)
			{
				p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Progression_Lich));
			}
		}
		else if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Attainment)
		{
			if (selectedArchetype != PLAYER_ARCHETYPE.Attainment_Puppet_Master)
			{
				p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Attainment_Puppet_Master));
			}
			if (selectedArchetype != PLAYER_ARCHETYPE.Attainment_Ravager)
			{
				p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Attainment_Ravager));
			}
			if (selectedArchetype != PLAYER_ARCHETYPE.Attainment_Lich)
			{
				p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Attainment_Lich));
			}
		}
		else if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Eradication)
		{
			if (selectedArchetype != PLAYER_ARCHETYPE.Eradication_Puppet_Master)
			{
				p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Eradication_Puppet_Master));
			}
			if (selectedArchetype != PLAYER_ARCHETYPE.Eradication_Ravager)
			{
				p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Eradication_Ravager));
			}
			if (selectedArchetype != PLAYER_ARCHETYPE.Eradication_Lich)
			{
				p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Eradication_Lich));
			}
		}
	}

	public void GetAllMainArchetypeLoadouts(List<PlayerSkillLoadout> p_loadouts)
	{
		if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Progression)
		{
			p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Progression_Puppet_Master));
			p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Progression_Ravager));
			p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Progression_Lich));
		}
		else if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Attainment)
		{
			p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Attainment_Puppet_Master));
			p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Attainment_Ravager));
			p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Attainment_Lich));
		}
		else if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Eradication)
		{
			p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Eradication_Puppet_Master));
			p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Eradication_Ravager));
			p_loadouts.Add(GetArchetypeLoadout(PLAYER_ARCHETYPE.Eradication_Lich));
		}
	}

	public PlayerSkillLoadout GetArchetypeLoadout(PLAYER_ARCHETYPE p_archetype)
	{
		return allSkillLoadouts[p_archetype];
	}

	public void ResetSpellsInUse()
	{
		for (int i = 0; i < allPlayerSkillsData.Values.Count; i++)
		{
			allPlayerSkillsData.Values.ElementAt(i).ResetData();
		}
	}

	public void ResetSummonPlayerSkills()
	{
		for (int i = 0; i < allSummonPlayerSkills.Length; i++)
		{
			allSummonPlayerSkillsData[allSummonPlayerSkills[i]].ResetData();
		}
	}

	public void ResetCachedTexts()
	{
		foreach (KeyValuePair<PLAYER_SKILL_TYPE, PlayerSkillData> item in _playerSkillDataDictionary)
		{
			item.Value.ResetAllBonusUIText();
		}
		foreach (KeyValuePair<PLAYER_SKILL_TYPE, SkillData> allPlayerSkillsDatum in allPlayerSkillsData)
		{
			allPlayerSkillsDatum.Value.ResetAllChargesUIText();
		}
	}

	public T GetScriptableObjPlayerSkillData<T>(PLAYER_SKILL_TYPE spellType) where T : PlayerSkillData
	{
		if (_playerSkillDataDictionary.ContainsKey(spellType))
		{
			return _playerSkillDataDictionary[spellType] as T;
		}
		return null;
	}

	private void ConstructPassiveSkills()
	{
		passiveSkillsData = new Dictionary<PASSIVE_SKILL, PassiveSkill>();
		for (int i = 0; i < allPassiveSkillTypes.Length; i++)
		{
			PASSIVE_SKILL key = allPassiveSkillTypes[i];
			string text = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(key.ToString()) + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			PassiveSkill value = Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("Problem with creating spell data for " + text)) as PassiveSkill;
			passiveSkillsData.Add(key, value);
		}
	}

	public PassiveSkill GetPassiveSkill(PASSIVE_SKILL passiveSkill)
	{
		if (passiveSkillsData.ContainsKey(passiveSkill))
		{
			return passiveSkillsData[passiveSkill];
		}
		throw new Exception("Could not find class for passive skill " + passiveSkill);
	}

	public int GetDamageBaseOnLevel(PLAYER_SKILL_TYPE p_skillType, int p_forcedLevel = -1)
	{
		SkillData skillData = GetSkillData(p_skillType);
		return GetDamageBaseOnLevel(skillData, p_forcedLevel);
	}

	public int GetDamageBaseOnLevel(SkillData p_skill, int p_forcedLevel = -1)
	{
		PlayerSkillData scriptableObjPlayerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type);
		if (p_forcedLevel == -1)
		{
			return scriptableObjPlayerSkillData.skillUpgradeData.GetAdditionalDamageBaseOnLevel(p_skill.currentLevel);
		}
		return scriptableObjPlayerSkillData.skillUpgradeData.GetAdditionalDamageBaseOnLevel(p_forcedLevel);
	}

	public int GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		SkillData skillData = GetSkillData(p_skillType);
		return GetTileRangeBonusPerLevel(skillData);
	}

	public int GetTileRangeBonusPerLevel(SkillData p_skill)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type).skillUpgradeData.GetTileRangeBonusPerLevel(p_skill.currentLevel);
	}

	public float GetPierceBasedOnCurrentLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		SkillData skillData = GetSkillData(p_skillType);
		return GetPierceBasedOnCurrentLevel(skillData);
	}

	public float GetPierceBasedOnCurrentLevel(SkillData p_skill)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type).skillUpgradeData.GetAdditionalPiercePerLevelBaseOnLevel(p_skill.currentLevel);
	}

	public float GetChanceBonusPerLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		SkillData skillData = GetSkillData(p_skillType);
		return GetChanceBonusPerLevel(skillData);
	}

	public float GetChanceBonusPerLevel(SkillData p_skill)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type).skillUpgradeData.GetChanceBonusPerLevel(p_skill.currentLevel);
	}

	public float GetAdditionalHpPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType).skillUpgradeData.GetAdditionalHpPercentagePerLevelBaseOnLevel(GetSkillData(p_skillType).currentLevel);
	}

	public float GetAdditionalMaxHpPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType).skillUpgradeData.GetAdditionalMaxHpPercentagePerLevelBaseOnLevel(GetSkillData(p_skillType).currentLevel);
	}

	public float GetAdditionalAttackPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType).skillUpgradeData.GetAdditionalAttackPercentagePerLevelBaseOnLevel(GetSkillData(p_skillType).currentLevel);
	}

	public int GetAdditionalAttackActualPerLevelBaseOnLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType).skillUpgradeData.GetAdditionalAttackActualPerLevelBaseOnLevel(GetSkillData(p_skillType).currentLevel);
	}

	public float GetIncreaseStatsPercentagePerLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		return GetIncreaseStatsPercentagePerLevel(GetSkillData(p_skillType));
	}

	public float GetIncreaseStatsPercentagePerLevel(SkillData p_skill)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type).skillUpgradeData.GetIncreaseStatsPercentagePerLevel(p_skill.currentLevel);
	}

	public float GetIncreaseStatsPercentagePerLevel(SkillData p_skill, int p_level)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type).skillUpgradeData.GetIncreaseStatsPercentagePerLevel(p_level);
	}

	public int GetDurationBonusPerLevel(PLAYER_SKILL_TYPE p_skillType, int p_forcedLevel = -1)
	{
		return GetDurationBonusPerLevel(GetSkillData(p_skillType), p_forcedLevel);
	}

	public int GetDurationBonusPerLevel(SkillData p_skill, int p_forcedLevel = -1)
	{
		PlayerSkillData scriptableObjPlayerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type);
		if (p_forcedLevel == -1)
		{
			return scriptableObjPlayerSkillData.skillUpgradeData.GetDurationBonusPerLevel(p_skill.currentLevel);
		}
		return scriptableObjPlayerSkillData.skillUpgradeData.GetDurationBonusPerLevel(p_forcedLevel);
	}

	public int GetSkillMovementSpeedPerLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType).skillUpgradeData.GetSkillMovementSpeedPerLevel(GetSkillData(p_skillType).currentLevel);
	}

	public float GetAfflictionRateChancePerLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType).afflictionUpgradeData.GetRateChancePerLevel(GetSkillData(p_skillType).currentLevel);
	}

	public float GetAfflictionNapsDurationPerLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType).afflictionUpgradeData.GetNapsDurationPerLevel(GetSkillData(p_skillType).currentLevel);
	}

	public float GetAfflictionHungerRatePerLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType).afflictionUpgradeData.GetHungerRatePerLevel(GetSkillData(p_skillType).currentLevel);
	}

	public float GetAfflictionHungerRatePerLevel(PLAYER_SKILL_TYPE p_skillType, int p_level)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType).afflictionUpgradeData.GetHungerRatePerLevel(p_level);
	}

	public int GetAfflictionCrowdNumberPerLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType).afflictionUpgradeData.GetCrowdNumberPerLevel(GetSkillData(p_skillType).currentLevel);
	}

	public List<OPINIONS> GetAfflictionOpinionTriggers(PLAYER_SKILL_TYPE p_skillType)
	{
		return GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType).afflictionUpgradeData.GetAllOpinionsTrigger();
	}

	public float GetTriggerRateForCurrentLevel(PLAYER_SKILL_TYPE p_skillType)
	{
		PlayerSkillData scriptableObjPlayerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
		SkillData skillData = GetSkillData(p_skillType);
		return scriptableObjPlayerSkillData.afflictionUpgradeData.GetRateChancePerLevel(skillData.currentLevel);
	}

	public bool HasOpinionTriggerAtCurrentLevel(PLAYER_SKILL_TYPE p_skillType, OPINIONS p_opinion)
	{
		PlayerSkillData scriptableObjPlayerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
		SkillData skillData = GetSkillData(p_skillType);
		return scriptableObjPlayerSkillData.afflictionUpgradeData.HasOpinionTriggerForLevel(p_opinion, skillData.currentLevel);
	}

	public void PopulateOpinionTriggersAtCurrentLevel(PLAYER_SKILL_TYPE p_skillType, List<OPINIONS> p_opinions)
	{
		PlayerSkillData scriptableObjPlayerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
		SkillData skillData = GetSkillData(p_skillType);
		for (int i = 0; i < scriptableObjPlayerSkillData.afflictionUpgradeData.opinionTrigger.Count && i <= skillData.currentLevel; i++)
		{
			OPINIONS item = scriptableObjPlayerSkillData.afflictionUpgradeData.opinionTrigger[i];
			p_opinions.Add(item);
		}
	}

	public PLAYER_SKILL_TYPE GetAfflictionTypeByTraitName(string p_traitName)
	{
		if (afflictionsNameSkillTypeDictionary.ContainsKey(p_traitName))
		{
			return afflictionsNameSkillTypeDictionary[p_traitName];
		}
		return PLAYER_SKILL_TYPE.NONE;
	}

	private int GetMonsterSpawnerMaxCapacity()
	{
		MAP_SIZE mapSize = WorldSettings.Instance.worldSettingsData.mapSettings.mapSize;
		SkillData spellData = GetSpellData(PLAYER_SKILL_TYPE.MONSTER_SPAWNER);
		return GetMonsterSpawnerMaxCapacity(mapSize, spellData.currentLevel);
	}

	public int GetMonsterSpawnerMaxCapacity(MAP_SIZE p_mapSize, int p_level)
	{
		switch (p_level)
		{
		case 0:
			if (p_mapSize == MAP_SIZE.Small || p_mapSize == MAP_SIZE.Medium)
			{
				return 2;
			}
			return 3;
		case 1:
			if (p_mapSize == MAP_SIZE.Small || p_mapSize == MAP_SIZE.Medium)
			{
				return 3;
			}
			return 4;
		case 2:
			if (p_mapSize == MAP_SIZE.Small || p_mapSize == MAP_SIZE.Medium)
			{
				return 4;
			}
			return 5;
		case 3:
			if (p_mapSize == MAP_SIZE.Small || p_mapSize == MAP_SIZE.Medium)
			{
				return 5;
			}
			return 6;
		default:
			return 0;
		}
	}

	private void OnLocaleChanged(Locale obj)
	{
		if (allPlayerActionsData == null)
		{
			return;
		}
		foreach (KeyValuePair<PLAYER_SKILL_TYPE, PlayerAction> allPlayerActionsDatum in allPlayerActionsData)
		{
			allPlayerActionsDatum.Value.ResetCachedTexts();
		}
	}
}
