using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EZObjectPools;
using FullSerializer;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Maccima_Games.Util;
using Object_Pools;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UtilityScripts;

public class Utilities : MonoBehaviour
{
	public static readonly System.Random Rng = new System.Random();

	private static int _lastFactionColorIndex;

	private static int _lastLogID;

	private static int _lastFactionID;

	private static int _lastCharacterID;

	private static int _lastAreaID;

	private static int _lastTileObjectID;

	private static int _lastStructureID;

	private static int _lastRegionID;

	private static int _lastJobID;

	private static int _lastBurningSourceID;

	private static int _lastGoapPlanID;

	public static LANGUAGES defaultLanguage = LANGUAGES.ENGLISH;

	public static char[] delimiters = new char[7] { ' ', '.', ',', '\'', '!', '"', ':' };

	private static readonly string _coreStreamingPath = Application.streamingAssetsPath + "/Core";

	private static readonly string _coreStreamingDataPath = coreStreamingPath + "/Data";

	private static readonly string _gameSavePath = Application.persistentDataPath + "/Ruinarch Game Saves/";

	private static readonly string _tempPath = gameSavePath + "/Temp/";

	private static readonly string _tempZipPath = gameSavePath + "/Temp/Temp/";

	private static readonly string _autosavePath = gameSavePath + "/Autosaves/";

	private static readonly Dictionary<string, string> pluralExceptions = new Dictionary<string, string>
	{
		{ "man", "men" },
		{ "woman", "women" },
		{ "child", "children" },
		{ "tooth", "teeth" },
		{ "foot", "feet" },
		{ "mouse", "mice" },
		{ "belief", "beliefs" }
	};

	private static Point[] _possibleNeighbours;

	public static Dictionary<GridNeighbourDirection, Point> gridPossibleNeighbours = new Dictionary<GridNeighbourDirection, Point>
	{
		{
			GridNeighbourDirection.North,
			new Point(0, 1)
		},
		{
			GridNeighbourDirection.South,
			new Point(0, -1)
		},
		{
			GridNeighbourDirection.West,
			new Point(-1, 0)
		},
		{
			GridNeighbourDirection.East,
			new Point(1, 0)
		},
		{
			GridNeighbourDirection.North_West,
			new Point(-1, 1)
		},
		{
			GridNeighbourDirection.North_East,
			new Point(1, 1)
		},
		{
			GridNeighbourDirection.South_West,
			new Point(-1, -1)
		},
		{
			GridNeighbourDirection.South_East,
			new Point(1, -1)
		}
	};

	public static Color darkGreen = new Color(0f, 20f / 51f, 0f);

	public static Color lightGreen = new Color(0.4862745f, 84f / 85f, 0f);

	public static Color darkRed = new Color(0.54509807f, 0f, 0f);

	public static Color lightRed = new Color(1f, 0f, 0f);

	public static Color[] factionColorCycle = new Color[16]
	{
		new Color32(219, 0, 0, 145),
		new Color32(0, 81, 243, 145),
		new Color32(byte.MaxValue, byte.MaxValue, 0, 145),
		new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 145),
		new Color32(120, byte.MaxValue, 43, 145),
		new Color32(249, 91, 205, 145),
		new Color32(29, 29, 29, 145),
		new Color32(15, 221, 240, 160),
		new Color32(byte.MaxValue, 142, 0, 160),
		new Color32(141, 18, 206, 145),
		new Color32(14, 119, 27, 145),
		new Color32(138, 7, 7, 148),
		new Color32(3, 33, 142, 160),
		new Color32(166, 86, 0, 185),
		new Color32(138, 128, 253, 148),
		new Color32(188, byte.MaxValue, 0, 166)
	};

	public static Dictionary<BIOMES, Color> biomeColor = new Dictionary<BIOMES, Color>
	{
		{
			BIOMES.GRASSLAND,
			new Color(2f / 15f, 0.54509807f, 2f / 15f)
		},
		{
			BIOMES.BARE,
			new Color(0.41568628f, 36f / 85f, 0.23137255f)
		},
		{
			BIOMES.DESERT,
			new Color(31f / 85f, 0.30980393f, 23f / 85f)
		},
		{
			BIOMES.FOREST,
			new Color(2f / 15f, 0.54509807f, 2f / 15f)
		},
		{
			BIOMES.SNOW,
			new Color(1f, 1f, 1f)
		},
		{
			BIOMES.TUNDRA,
			new Color(0.41568628f, 36f / 85f, 0.23137255f)
		}
	};

	public static Dictionary<string, LOG_IDENTIFIER> logIdentifiers = new Dictionary<string, LOG_IDENTIFIER>
	{
		{
			"00",
			LOG_IDENTIFIER.ACTIVE_CHARACTER
		},
		{
			"01",
			LOG_IDENTIFIER.FACTION_1
		},
		{
			"02",
			LOG_IDENTIFIER.FACTION_LEADER_1
		},
		{
			"04",
			LOG_IDENTIFIER.LANDMARK_1
		},
		{
			"05",
			LOG_IDENTIFIER.PARTY_1
		},
		{
			"06",
			LOG_IDENTIFIER.STRUCTURE_1
		},
		{
			"07",
			LOG_IDENTIFIER.STRUCTURE_2
		},
		{
			"08",
			LOG_IDENTIFIER.STRUCTURE_3
		},
		{
			"10",
			LOG_IDENTIFIER.TARGET_CHARACTER
		},
		{
			"11",
			LOG_IDENTIFIER.FACTION_2
		},
		{
			"12",
			LOG_IDENTIFIER.FACTION_LEADER_2
		},
		{
			"14",
			LOG_IDENTIFIER.LANDMARK_2
		},
		{
			"15",
			LOG_IDENTIFIER.PARTY_2
		},
		{
			"20",
			LOG_IDENTIFIER.CHARACTER_3
		},
		{
			"21",
			LOG_IDENTIFIER.FACTION_3
		},
		{
			"22",
			LOG_IDENTIFIER.FACTION_LEADER_3
		},
		{
			"24",
			LOG_IDENTIFIER.LANDMARK_3
		},
		{
			"25",
			LOG_IDENTIFIER.PARTY_3
		},
		{
			"51",
			LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_A
		},
		{
			"52",
			LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_A
		},
		{
			"53",
			LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_A
		},
		{
			"54",
			LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_A
		},
		{
			"55",
			LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_A
		},
		{
			"81",
			LOG_IDENTIFIER.ACTION_DESCRIPTION
		},
		{
			"82",
			LOG_IDENTIFIER.QUEST_NAME
		},
		{
			"83",
			LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_S
		},
		{
			"84",
			LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_O
		},
		{
			"85",
			LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_P
		},
		{
			"86",
			LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_R
		},
		{
			"87",
			LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_S
		},
		{
			"88",
			LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_O
		},
		{
			"89",
			LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_P
		},
		{
			"90",
			LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_R
		},
		{
			"91",
			LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_S
		},
		{
			"92",
			LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_O
		},
		{
			"93",
			LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_P
		},
		{
			"94",
			LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_R
		},
		{
			"95",
			LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_S
		},
		{
			"96",
			LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_O
		},
		{
			"97",
			LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_P
		},
		{
			"98",
			LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_R
		},
		{
			"100",
			LOG_IDENTIFIER.TASK
		},
		{
			"101",
			LOG_IDENTIFIER.DATE
		},
		{
			"102",
			LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_S
		},
		{
			"103",
			LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_O
		},
		{
			"104",
			LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_P
		},
		{
			"105",
			LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_R
		},
		{
			"106",
			LOG_IDENTIFIER.OTHER
		},
		{
			"107",
			LOG_IDENTIFIER.ITEM_1
		},
		{
			"108",
			LOG_IDENTIFIER.ITEM_2
		},
		{
			"109",
			LOG_IDENTIFIER.ITEM_3
		},
		{
			"110",
			LOG_IDENTIFIER.COMBAT_FILLER
		},
		{
			"111",
			LOG_IDENTIFIER.STRING_1
		},
		{
			"112",
			LOG_IDENTIFIER.STRING_2
		},
		{
			"113",
			LOG_IDENTIFIER.MINION_1
		},
		{
			"114",
			LOG_IDENTIFIER.MINION_1_PRONOUN_S
		},
		{
			"115",
			LOG_IDENTIFIER.MINION_1_PRONOUN_O
		},
		{
			"116",
			LOG_IDENTIFIER.MINION_1_PRONOUN_P
		},
		{
			"117",
			LOG_IDENTIFIER.MINION_1_PRONOUN_R
		},
		{
			"118",
			LOG_IDENTIFIER.MINION_2
		},
		{
			"119",
			LOG_IDENTIFIER.MINION_2_PRONOUN_S
		},
		{
			"120",
			LOG_IDENTIFIER.MINION_2_PRONOUN_O
		},
		{
			"121",
			LOG_IDENTIFIER.MINION_2_PRONOUN_P
		},
		{
			"122",
			LOG_IDENTIFIER.MINION_2_PRONOUN_R
		},
		{
			"123",
			LOG_IDENTIFIER.CHARACTER_LIST_1
		},
		{
			"124",
			LOG_IDENTIFIER.CHARACTER_LIST_2
		},
		{
			"125",
			LOG_IDENTIFIER.APPEND
		},
		{
			"126",
			LOG_IDENTIFIER.OTHER_2
		}
	};

	public static Dictionary<LOG_IDENTIFIER, string> logIdentifierStrings = new Dictionary<LOG_IDENTIFIER, string>
	{
		{
			LOG_IDENTIFIER.ACTIVE_CHARACTER,
			"00"
		},
		{
			LOG_IDENTIFIER.FACTION_1,
			"01"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_1,
			"02"
		},
		{
			LOG_IDENTIFIER.LANDMARK_1,
			"04"
		},
		{
			LOG_IDENTIFIER.PARTY_1,
			"05"
		},
		{
			LOG_IDENTIFIER.STRUCTURE_1,
			"06"
		},
		{
			LOG_IDENTIFIER.STRUCTURE_2,
			"07"
		},
		{
			LOG_IDENTIFIER.STRUCTURE_3,
			"08"
		},
		{
			LOG_IDENTIFIER.TARGET_CHARACTER,
			"10"
		},
		{
			LOG_IDENTIFIER.FACTION_2,
			"11"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_2,
			"12"
		},
		{
			LOG_IDENTIFIER.LANDMARK_2,
			"14"
		},
		{
			LOG_IDENTIFIER.PARTY_2,
			"15"
		},
		{
			LOG_IDENTIFIER.CHARACTER_3,
			"20"
		},
		{
			LOG_IDENTIFIER.FACTION_3,
			"21"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_3,
			"22"
		},
		{
			LOG_IDENTIFIER.LANDMARK_3,
			"24"
		},
		{
			LOG_IDENTIFIER.PARTY_3,
			"25"
		},
		{
			LOG_IDENTIFIER.ACTION_DESCRIPTION,
			"81"
		},
		{
			LOG_IDENTIFIER.QUEST_NAME,
			"82"
		},
		{
			LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_S,
			"83"
		},
		{
			LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_O,
			"84"
		},
		{
			LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_P,
			"85"
		},
		{
			LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_R,
			"86"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_S,
			"87"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_O,
			"88"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_P,
			"89"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_R,
			"90"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_S,
			"91"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_O,
			"92"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_P,
			"93"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_R,
			"94"
		},
		{
			LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_S,
			"95"
		},
		{
			LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_O,
			"96"
		},
		{
			LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_P,
			"97"
		},
		{
			LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_R,
			"98"
		},
		{
			LOG_IDENTIFIER.TASK,
			"100"
		},
		{
			LOG_IDENTIFIER.DATE,
			"101"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_S,
			"102"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_O,
			"103"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_P,
			"104"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_R,
			"105"
		},
		{
			LOG_IDENTIFIER.OTHER,
			"106"
		},
		{
			LOG_IDENTIFIER.ITEM_1,
			"107"
		},
		{
			LOG_IDENTIFIER.ITEM_2,
			"108"
		},
		{
			LOG_IDENTIFIER.ITEM_3,
			"109"
		},
		{
			LOG_IDENTIFIER.COMBAT_FILLER,
			"110"
		},
		{
			LOG_IDENTIFIER.STRING_1,
			"111"
		},
		{
			LOG_IDENTIFIER.STRING_2,
			"112"
		},
		{
			LOG_IDENTIFIER.MINION_1,
			"113"
		},
		{
			LOG_IDENTIFIER.MINION_1_PRONOUN_S,
			"114"
		},
		{
			LOG_IDENTIFIER.MINION_1_PRONOUN_O,
			"115"
		},
		{
			LOG_IDENTIFIER.MINION_1_PRONOUN_P,
			"116"
		},
		{
			LOG_IDENTIFIER.MINION_1_PRONOUN_R,
			"117"
		},
		{
			LOG_IDENTIFIER.MINION_2,
			"118"
		},
		{
			LOG_IDENTIFIER.MINION_2_PRONOUN_S,
			"119"
		},
		{
			LOG_IDENTIFIER.MINION_2_PRONOUN_O,
			"120"
		},
		{
			LOG_IDENTIFIER.MINION_2_PRONOUN_P,
			"121"
		},
		{
			LOG_IDENTIFIER.MINION_2_PRONOUN_R,
			"122"
		},
		{
			LOG_IDENTIFIER.CHARACTER_LIST_1,
			"123"
		},
		{
			LOG_IDENTIFIER.CHARACTER_LIST_2,
			"124"
		},
		{
			LOG_IDENTIFIER.APPEND,
			"125"
		},
		{
			LOG_IDENTIFIER.OTHER_2,
			"126"
		},
		{
			LOG_IDENTIFIER.ACTIVE_CHARACTER_PRONOUN_A,
			"51"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_1_PRONOUN_A,
			"52"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_2_PRONOUN_A,
			"53"
		},
		{
			LOG_IDENTIFIER.TARGET_CHARACTER_PRONOUN_A,
			"54"
		},
		{
			LOG_IDENTIFIER.FACTION_LEADER_3_PRONOUN_A,
			"55"
		}
	};

	public static Dictionary<LOG_IDENTIFIER, string> newLogIdentifierStrings = new Dictionary<LOG_IDENTIFIER, string>
	{
		{
			LOG_IDENTIFIER.ACTIVE_CHARACTER,
			"source"
		},
		{
			LOG_IDENTIFIER.FACTION_1,
			"faction1"
		},
		{
			LOG_IDENTIFIER.LANDMARK_1,
			"location1"
		},
		{
			LOG_IDENTIFIER.PARTY_1,
			"party1"
		},
		{
			LOG_IDENTIFIER.TARGET_CHARACTER,
			"target"
		},
		{
			LOG_IDENTIFIER.FACTION_2,
			"faction2"
		},
		{
			LOG_IDENTIFIER.LANDMARK_2,
			"location2"
		},
		{
			LOG_IDENTIFIER.PARTY_2,
			"party2"
		},
		{
			LOG_IDENTIFIER.CHARACTER_3,
			"otherCharacter"
		},
		{
			LOG_IDENTIFIER.OTHER,
			"other1"
		},
		{
			LOG_IDENTIFIER.OTHER_2,
			"other2"
		},
		{
			LOG_IDENTIFIER.ITEM_1,
			"item1"
		},
		{
			LOG_IDENTIFIER.STRING_1,
			"text1"
		},
		{
			LOG_IDENTIFIER.STRING_2,
			"text2"
		},
		{
			LOG_IDENTIFIER.APPEND,
			"append"
		}
	};

	public static List<BIOMES> biomeLayering = new List<BIOMES>
	{
		BIOMES.GRASSLAND,
		BIOMES.TUNDRA,
		BIOMES.FOREST,
		BIOMES.DESERT,
		BIOMES.SNOW
	};

	public static List<string> specialClasses = new List<string> { "Necromancer", "Archmage", "Witch", "Beastmaster", "Tempest" };

	public static INTERACTION_TYPE[] interactionPriorityList = new INTERACTION_TYPE[0];

	private static RACE[,] opposingRaces = new RACE[2, 2]
	{
		{
			RACE.HUMANS,
			RACE.ELVES
		},
		{
			RACE.FAERY,
			RACE.GOBLIN
		}
	};

	public static string coreStreamingDataPath => _coreStreamingDataPath;

	public static string coreStreamingPath => _coreStreamingPath;

	public static string gameSavePath => _gameSavePath;

	public static string tempPath => _tempPath;

	public static string tempZipPath => _tempZipPath;

	public static string autosavePath => _autosavePath;

	public static int lastCharacterID => _lastCharacterID;

	public static Point[] PossibleGridNeighbours
	{
		get
		{
			if (_possibleNeighbours == null)
			{
				_possibleNeighbours = new Point[8]
				{
					new Point(0, 1),
					new Point(1, 1),
					new Point(1, 0),
					new Point(1, -1),
					new Point(0, -1),
					new Point(-1, -1),
					new Point(-1, 0),
					new Point(-1, 1)
				};
			}
			return _possibleNeighbours;
		}
	}

	public static List<Point> OddNeighbours => new List<Point>
	{
		new Point(0, 1),
		new Point(1, 1),
		new Point(1, 0),
		new Point(1, -1),
		new Point(0, -1),
		new Point(-1, 0)
	};

	public static void ResetUsedIDs()
	{
		_lastFactionColorIndex = 0;
		_lastLogID = 0;
		_lastFactionID = 0;
		_lastCharacterID = 0;
		_lastAreaID = 0;
		_lastTileObjectID = 0;
		_lastStructureID = 0;
		_lastRegionID = 0;
		_lastJobID = 0;
		_lastBurningSourceID = 0;
		_lastGoapPlanID = 0;
	}

	public static int SetID<T>(T obj)
	{
		if (obj is Log)
		{
			_lastLogID++;
			return _lastLogID;
		}
		if (obj is Faction)
		{
			_lastFactionID++;
			return _lastFactionID;
		}
		if (obj is Character || obj is PreCharacterData)
		{
			_lastCharacterID++;
			return _lastCharacterID;
		}
		if (obj is BaseSettlement)
		{
			_lastAreaID++;
			return _lastAreaID;
		}
		if (obj is TileObject)
		{
			_lastTileObjectID++;
			return _lastTileObjectID;
		}
		if (obj is LocationStructure)
		{
			_lastStructureID++;
			return _lastStructureID;
		}
		if (obj is Region)
		{
			_lastRegionID++;
			return _lastRegionID;
		}
		if (obj is JobQueueItem)
		{
			_lastJobID++;
			return _lastJobID;
		}
		if (obj is BurningSource)
		{
			_lastBurningSourceID++;
			return _lastBurningSourceID;
		}
		if (obj is GoapPlan)
		{
			_lastGoapPlanID++;
			return _lastGoapPlanID;
		}
		return 0;
	}

	public static int SetID<T>(T obj, int idToUse)
	{
		if (obj is Log)
		{
			if (_lastLogID <= idToUse)
			{
				_lastLogID = idToUse;
			}
		}
		else if (obj is Faction)
		{
			if (_lastFactionID <= idToUse)
			{
				_lastFactionID = idToUse;
			}
		}
		else if (obj is Character || obj is PreCharacterData)
		{
			if (_lastCharacterID <= idToUse)
			{
				_lastCharacterID = idToUse;
			}
		}
		else if (obj is BaseSettlement)
		{
			if (_lastAreaID <= idToUse)
			{
				_lastAreaID = idToUse;
			}
		}
		else if (obj is LocationStructure)
		{
			if (_lastStructureID <= idToUse)
			{
				_lastStructureID = idToUse;
			}
		}
		else if (obj is TileObject)
		{
			if (_lastTileObjectID <= idToUse)
			{
				_lastTileObjectID = idToUse;
			}
		}
		else if (obj is Region)
		{
			if (_lastRegionID <= idToUse)
			{
				_lastRegionID = idToUse;
			}
		}
		else if (obj is JobQueueItem)
		{
			if (_lastJobID <= idToUse)
			{
				_lastJobID = idToUse;
			}
		}
		else if (obj is BurningSource && _lastBurningSourceID <= idToUse)
		{
			_lastBurningSourceID = idToUse;
		}
		return idToUse;
	}

	public static string GetNewUniqueID()
	{
		return Guid.NewGuid().ToString();
	}

	public static Color GetColorForFaction()
	{
		Color result = factionColorCycle[_lastFactionColorIndex];
		_lastFactionColorIndex++;
		if (_lastFactionColorIndex >= factionColorCycle.Length)
		{
			_lastFactionColorIndex = 0;
		}
		return result;
	}

	public static string RemoveRichText(string text)
	{
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		bool flag = false;
		foreach (char c in text)
		{
			if (flag)
			{
				if (c == '>')
				{
					flag = false;
				}
			}
			else if (c == '<')
			{
				flag = true;
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	private static void PopulateLogFillersAndIdentifyPronounsToBeUsed(string unReplacedLog, Dictionary<string, string> p_args, List<LogFiller> p_fillers)
	{
		for (int i = 0; i < p_fillers.Count; i++)
		{
			LogFiller p_filler = p_fillers[i];
			PopulateLogFillersAndIdentifyPronounsToBeUsed(unReplacedLog, p_args, p_filler);
		}
	}

	private static void PopulateLogFillersAndIdentifyPronounsToBeUsed(string unReplacedLog, Dictionary<string, string> p_args, LogFiller p_filler)
	{
		LOG_IDENTIFIER identifier = p_filler.identifier;
		string key = newLogIdentifierStrings[identifier];
		if (p_args.ContainsKey(key))
		{
			return;
		}
		p_args.Add(key, p_filler.uiString);
		switch (identifier)
		{
		case LOG_IDENTIFIER.ACTIVE_CHARACTER:
		{
			object objectForFiller2 = p_filler.GetObjectForFiller();
			GENDER gENDER2 = GENDER.MALE;
			if (objectForFiller2 is Character)
			{
				gENDER2 = (objectForFiller2 as Character).gender;
			}
			else if (objectForFiller2 is Minion)
			{
				gENDER2 = (objectForFiller2 as Minion).character.gender;
			}
			if (gENDER2 == GENDER.MALE)
			{
				LocalizationManager.sourcePronouns = LocalizationManager.sourceMalePronouns;
			}
			else
			{
				LocalizationManager.sourcePronouns = LocalizationManager.sourceFemalePronouns;
			}
			break;
		}
		case LOG_IDENTIFIER.TARGET_CHARACTER:
		{
			object objectForFiller = p_filler.GetObjectForFiller();
			GENDER gENDER = GENDER.MALE;
			if (objectForFiller is Character)
			{
				gENDER = (objectForFiller as Character).gender;
			}
			else if (objectForFiller is Minion)
			{
				gENDER = (objectForFiller as Minion).character.gender;
			}
			if (gENDER == GENDER.MALE)
			{
				LocalizationManager.targetPronouns = LocalizationManager.targetMalePronouns;
			}
			else
			{
				LocalizationManager.targetPronouns = LocalizationManager.targetFemalePronouns;
			}
			break;
		}
		}
	}

	private static void FormatAppendText(object[] p_params, Dictionary<string, string> p_args, List<LogFiller> p_fillers)
	{
		for (int i = 0; i < p_fillers.Count; i++)
		{
			LogFiller p_filler = p_fillers[i];
			FormatAppendText(p_params, p_args, p_filler);
		}
	}

	private static void FormatAppendText(object[] p_params, Dictionary<string, string> p_args, LogFiller p_filler)
	{
		LOG_IDENTIFIER identifier = p_filler.identifier;
		if (identifier == LOG_IDENTIFIER.APPEND)
		{
			string key = newLogIdentifierStrings[identifier];
			string uiString = p_filler.uiString;
			uiString = LocalizationSettings.StringDatabase.SmartFormatter.Format(uiString, p_args, LocalizationManager.sourcePronouns, LocalizationManager.targetPronouns);
			p_args[key] = uiString;
		}
	}

	public static string NewLogReplacer(string unReplacedLog, List<LogFiller> fillers, bool colorizeVerb = true)
	{
		if (string.IsNullOrEmpty(unReplacedLog))
		{
			return string.Empty;
		}
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		PopulateLogFillersAndIdentifyPronounsToBeUsed(unReplacedLog, dictionary, fillers);
		FormatAppendText(null, dictionary, fillers);
		string result = LocalizationSettings.StringDatabase.SmartFormatter.Format(unReplacedLog, dictionary, LocalizationManager.sourcePronouns, LocalizationManager.targetPronouns);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		return result;
	}

	public static string NewLogReplacer(string unReplacedLog, LogFiller fillers, bool colorizeVerb = true)
	{
		if (string.IsNullOrEmpty(unReplacedLog))
		{
			return string.Empty;
		}
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		PopulateLogFillersAndIdentifyPronounsToBeUsed(unReplacedLog, dictionary, fillers);
		FormatAppendText(null, dictionary, fillers);
		string result = LocalizationSettings.StringDatabase.SmartFormatter.Format(unReplacedLog, dictionary, LocalizationManager.sourcePronouns, LocalizationManager.targetPronouns);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		return result;
	}

	public static string LogReplacer(string unReplacedLog, List<LogFiller> fillers, bool colorizeVerb = true)
	{
		return NewLogReplacer(unReplacedLog, fillers, colorizeVerb);
	}

	public static string LogReplacer(string text, LogFiller p_filler)
	{
		return NewLogReplacer(text, p_filler);
	}

	private static string CustomStringReplacer(string wordToBeReplaced, LogFiller p_filler)
	{
		string result = string.Empty;
		string key = wordToBeReplaced.Substring(1, wordToBeReplaced.Length - 2);
		LOG_IDENTIFIER lOG_IDENTIFIER = logIdentifiers[key];
		if (lOG_IDENTIFIER == LOG_IDENTIFIER.APPEND)
		{
			if (p_filler.identifier == lOG_IDENTIFIER)
			{
				result = LogReplacer(p_filler.value, p_filler);
			}
		}
		else if (p_filler.identifier == lOG_IDENTIFIER)
		{
			result = p_filler.uiString;
		}
		return result;
	}

	private static string CustomStringReplacer(string wordToBeReplaced, List<LogFiller> fillers)
	{
		string result = string.Empty;
		string key = wordToBeReplaced.Substring(1, wordToBeReplaced.Length - 2);
		LOG_IDENTIFIER lOG_IDENTIFIER = logIdentifiers[key];
		if (lOG_IDENTIFIER == LOG_IDENTIFIER.APPEND)
		{
			for (int i = 0; i < fillers.Count; i++)
			{
				LogFiller logFiller = fillers[i];
				if (logFiller.identifier == lOG_IDENTIFIER)
				{
					result = LogReplacer(logFiller.value, fillers);
					break;
				}
			}
		}
		else
		{
			for (int j = 0; j < fillers.Count; j++)
			{
				LogFiller logFiller2 = fillers[j];
				if (logFiller2.identifier == lOG_IDENTIFIER)
				{
					result = logFiller2.uiString;
					break;
				}
			}
		}
		return result;
	}

	private static string CustomPronounReplacer(string wordToBeReplaced, LogFiller filler)
	{
		string text = logIdentifiers[wordToBeReplaced.Substring(1, wordToBeReplaced.Length - 2)].ToString();
		string text2 = string.Empty;
		char type = text.Last();
		LOG_IDENTIFIER result = LOG_IDENTIFIER.NONE;
		if (Enum.TryParse<LOG_IDENTIFIER>(text.Substring(0, text.Length - 10), out result))
		{
			string pronoun = GetPronoun(type, wordToBeReplaced.Last());
			if (filler.identifier == result)
			{
				text2 = PronounReplacer(pronoun, filler.GetObjectForFiller());
			}
		}
		if (text2 != string.Empty)
		{
			return text2;
		}
		return wordToBeReplaced;
	}

	private static string CustomPronounReplacer(string wordToBeReplaced, List<LogFiller> fillers)
	{
		string text = logIdentifiers[wordToBeReplaced.Substring(1, wordToBeReplaced.Length - 2)].ToString();
		string text2 = string.Empty;
		char type = text.Last();
		LOG_IDENTIFIER result = LOG_IDENTIFIER.NONE;
		if (Enum.TryParse<LOG_IDENTIFIER>(text.Substring(0, text.Length - 10), out result))
		{
			string pronoun = GetPronoun(type, wordToBeReplaced.Last());
			for (int i = 0; i < fillers.Count; i++)
			{
				if (fillers[i].identifier == result)
				{
					text2 = PronounReplacer(pronoun, fillers[i].GetObjectForFiller());
					break;
				}
			}
		}
		if (text2 != string.Empty)
		{
			return text2;
		}
		return wordToBeReplaced;
	}

	public static string GetStringForIdentifier(LOG_IDENTIFIER identifier)
	{
		foreach (KeyValuePair<string, LOG_IDENTIFIER> logIdentifier in logIdentifiers)
		{
			if (logIdentifier.Value == identifier)
			{
				string key = logIdentifier.Key;
				key = "%" + key;
				if (identifier.ToString().Contains("PRONOUN"))
				{
					return key + "b";
				}
				return ((uint)(identifier - 1) > 14u && identifier != LOG_IDENTIFIER.TASK && identifier != LOG_IDENTIFIER.COMBAT_FILLER) ? (key + "%") : (key + "@");
			}
		}
		return string.Empty;
	}

	public static string PronounReplacer(string word, object genderSubject)
	{
		string[] array = word.Split('/');
		GENDER gENDER = GENDER.MALE;
		if (genderSubject is Character)
		{
			gENDER = (genderSubject as Character).gender;
		}
		else if (genderSubject is Minion)
		{
			gENDER = (genderSubject as Minion).character.gender;
		}
		if (gENDER == GENDER.MALE)
		{
			if (array.Length != 0 && !string.IsNullOrEmpty(array[0]))
			{
				return array[0];
			}
		}
		else if (array.Length > 1 && !string.IsNullOrEmpty(array[1]))
		{
			return array[1];
		}
		return string.Empty;
	}

	private static string GetPronoun(char type, char caseIdentifier)
	{
		switch (type)
		{
		case 'S':
		{
			string he = LocalizationManager.He;
			string she = LocalizationManager.She;
			if (caseIdentifier == 'a')
			{
				return FirstLetterToUpperCase(he) + "/" + FirstLetterToUpperCase(she);
			}
			return he + "/" + she;
		}
		case 'O':
		{
			string him = LocalizationManager.Him;
			string her2 = LocalizationManager.Her;
			if (caseIdentifier == 'a')
			{
				return FirstLetterToUpperCase(him) + "/" + FirstLetterToUpperCase(her2);
			}
			return him + "/" + her2;
		}
		case 'P':
		{
			string his = LocalizationManager.His;
			string her = LocalizationManager.Her;
			if (caseIdentifier == 'a')
			{
				return FirstLetterToUpperCase(his) + "/" + FirstLetterToUpperCase(her);
			}
			return his + "/" + her;
		}
		case 'A':
		{
			string his2 = LocalizationManager.His;
			string hers = LocalizationManager.Hers;
			if (caseIdentifier == 'a')
			{
				return FirstLetterToUpperCase(his2) + "/" + FirstLetterToUpperCase(hers);
			}
			return his2 + "/" + hers;
		}
		case 'R':
		{
			string himself = LocalizationManager.Himself;
			string herself = LocalizationManager.Herself;
			if (caseIdentifier == 'a')
			{
				return FirstLetterToUpperCase(himself) + "/" + FirstLetterToUpperCase(herself);
			}
			return himself + "/" + herself;
		}
		default:
			return string.Empty;
		}
	}

	public static string GetPronounString(GENDER gender, PRONOUN_TYPE type, bool isUppercaseFirstLetter)
	{
		string text = string.Empty;
		if (gender == GENDER.MALE)
		{
			switch (type)
			{
			case PRONOUN_TYPE.SUBJECTIVE:
				text = LocalizationManager.He;
				break;
			case PRONOUN_TYPE.OBJECTIVE:
				text = LocalizationManager.Him;
				break;
			case PRONOUN_TYPE.POSSESSIVE:
				text = LocalizationManager.His;
				break;
			case PRONOUN_TYPE.REFLEXIVE:
				text = LocalizationManager.Himself;
				break;
			case PRONOUN_TYPE.POSSESSIVE_ALTERNATIVE:
				text = LocalizationManager.His;
				break;
			}
		}
		else
		{
			switch (type)
			{
			case PRONOUN_TYPE.SUBJECTIVE:
				text = LocalizationManager.She;
				break;
			case PRONOUN_TYPE.OBJECTIVE:
				text = LocalizationManager.Her;
				break;
			case PRONOUN_TYPE.POSSESSIVE:
				text = LocalizationManager.Her;
				break;
			case PRONOUN_TYPE.REFLEXIVE:
				text = LocalizationManager.Herself;
				break;
			case PRONOUN_TYPE.POSSESSIVE_ALTERNATIVE:
				text = LocalizationManager.Hers;
				break;
			}
		}
		if (isUppercaseFirstLetter)
		{
			text = FirstLetterToUpperCase(text);
		}
		return text;
	}

	public static string GetStringBetweenTwoChars(string word, char first, char last)
	{
		int num = word.IndexOf(first);
		int num2 = word.LastIndexOf(last);
		if (num == -1 || num2 == -1)
		{
			return string.Empty;
		}
		num++;
		if (num >= word.Length)
		{
			return string.Empty;
		}
		return word.Substring(num, num2 - num);
	}

	public static List<string> GetAllWordsInAString(string wordToFind, string text)
	{
		List<string> list = new List<string>();
		string empty = string.Empty;
		int num = 0;
		int num2 = 0;
		int num3 = num;
		while (num != -1)
		{
			num = text.IndexOf(wordToFind, num3);
			if (num != -1)
			{
				num3 = num + 1;
				if (num3 > text.Length - 1)
				{
					num3 = text.Length - 1;
				}
				num2 = 0;
				for (int i = num; i < text.Length && text[i] != ' '; i++)
				{
					num2++;
				}
				empty = text.Substring(num, num2);
				list.Add(empty);
			}
		}
		return list;
	}

	public static void PopulateSplittedStringAndKeepDelimiters(List<string> p_splittedStringParts, string s, char[] delimiters)
	{
		if (string.IsNullOrEmpty(s))
		{
			return;
		}
		int num = 0;
		do
		{
			int num2 = s.IndexOfAny(delimiters, num);
			if (num2 >= 0)
			{
				if (num2 > num)
				{
					p_splittedStringParts.Add(s.Substring(num, num2 - num));
				}
				p_splittedStringParts.Add(new string(s[num2], 1));
				num = num2 + 1;
				continue;
			}
			p_splittedStringParts.Add(s.Substring(num, s.Length - num));
			break;
		}
		while (num < s.Length);
	}

	public static void PopulateSplittedStringAndKeepDelimiters(List<string> p_splittedStringParts, string s, char delimiters)
	{
		if (string.IsNullOrEmpty(s))
		{
			return;
		}
		int num = 0;
		do
		{
			int num2 = s.IndexOf(delimiters, num);
			if (num2 >= 0)
			{
				if (num2 > num)
				{
					p_splittedStringParts.Add(s.Substring(num, num2 - num));
				}
				p_splittedStringParts.Add(new string(s[num2], 1));
				num = num2 + 1;
				continue;
			}
			p_splittedStringParts.Add(s.Substring(num, s.Length - num));
			break;
		}
		while (num < s.Length);
	}

	public static void SplitStringIntoNewLines(string s, char delimiter, StringBuilder p_strBuilder)
	{
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = s.Split(delimiter);
			foreach (string value in array)
			{
				p_strBuilder.AppendLine(value);
			}
		}
	}

	public static void SetSpriteSortingLayer(SpriteRenderer sprite, string layerName)
	{
		sprite.sortingLayerName = layerName;
	}

	public static void SetLayerRecursively(GameObject go, int layerNumber)
	{
		Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layerNumber;
		}
	}

	public static string MonsterIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Monster_Icon\">";
	}

	public static string VillagerIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Villager_Icon\">";
	}

	public static string ManaIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Mana_Icon\">";
	}

	public static string UpgradeArrowIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"upgrade_arrow_icon\">";
	}

	public static string YellowDotIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"exclamation\">";
	}

	public static string ChargesIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Charges_Icon\">";
	}

	public static string BonusChargesIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Bonus_Charges_Icon\">";
	}

	public static string ThreatIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Threat_Icon\">";
	}

	public static string CooldownIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Cooldown_Icon\">";
	}

	public static string CultistIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Cultist_Icon\">";
	}

	public static string LeaderIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Leader_Icon\"> ";
	}

	public static string DemonIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Demon_Icon\">";
	}

	public static string UndeadIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Undead_Icon\">";
	}

	public static string ChaoticEnergyIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Plague_Icon\">";
	}

	public static string SpiritEnergyIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"spirit_energy_icon\">";
	}

	public static string AttackIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"stats_attack_icon\"></link>";
	}

	public static string AttackIconWithLink()
	{
		return "<link=Normal_Element#><sprite=\"Text_Sprites\" name=\"physical_icon\"></link>";
	}

	public static string EarthIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"element_earth_icon\">";
	}

	public static string EarthIconWithLink()
	{
		return "<link=Earth#><sprite=\"Text_Sprites\" name=\"element_earth_icon\"></link>";
	}

	public static string NormalIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"element_normal_icon\">";
	}

	public static string NormalIconWithLink()
	{
		return "<link=Physical#><sprite=\"Text_Sprites\" name=\"element_normal_icon\"></link>";
	}

	public static string WaterIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"element_water_icon\">";
	}

	public static string WaterIconWithLink()
	{
		return "<link=Water#><sprite=\"Text_Sprites\" name=\"element_water_icon\"></link>";
	}

	public static string PoisonIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"element_poison_icon\">";
	}

	public static string PoisonIconWithLink()
	{
		return "<link=Poison#><sprite=\"Text_Sprites\" name=\"element_poison_icon\"></link>";
	}

	public static string IceIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"element_ice_icon\">";
	}

	public static string IceIconWithLink()
	{
		return "<link=Ice#><sprite=\"Text_Sprites\" name=\"element_ice_icon\"></link>";
	}

	public static string WindIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"element_wind_icon\">";
	}

	public static string WindIconWithLink()
	{
		return "<link=Wind#><sprite=\"Text_Sprites\" name=\"element_wind_icon\"></link>";
	}

	public static string FireIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"element_fire_icon\">";
	}

	public static string FireIconWithLink()
	{
		return "<link=Fire#><sprite=\"Text_Sprites\" name=\"element_fire_icon\"></link>";
	}

	public static string ElectricIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"element_electric_icon\">";
	}

	public static string ElectricIconWithLink()
	{
		return "<link=Electric#><sprite=\"Text_Sprites\" name=\"element_electric_icon\"></link>";
	}

	public static string MentalIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"mental_icon\">";
	}

	public static string MentalIconWithLink()
	{
		return "<link=Mental#><sprite=\"Text_Sprites\" name=\"mental_icon\"></link>";
	}

	public static string ResistanceIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"resistance_icon\">";
	}

	public static string PiercingIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"piercing_icon\">";
	}

	public static string HealthIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"stats_health_icon\">";
	}

	public static string SpeedIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"stats_speed_icon\">";
	}

	public static string RatmanIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Ratman_Icon\">";
	}

	public static string VillageIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Village_Icon\">";
	}

	public static string TileObjectIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Tile_Object_Icon\">";
	}

	public static string StructureIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"Structure_Icon\">";
	}

	public static string CoinIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"coin_icon\">";
	}

	public static string GrudgeIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"grudge_icon\">";
	}

	public static string CheckmarkIcon()
	{
		return "<sprite=\"Text_Sprites\" name=\"checkmark\">";
	}

	public static string ColorizeAction(string actionString)
	{
		return "<color=#f87f43>" + actionString + "</color>";
	}

	public static string ColorizeActionInLog(string actionString)
	{
		return "<color=#FFC700>" + actionString + "</color>";
	}

	public static string ColorizeInvalidText(string p_text)
	{
		return "<color=#FE3E83>" + p_text + "</color>";
	}

	public static string ColorizeUpgradeText(string p_text)
	{
		return "<color=#81FF00>" + p_text + "</color>";
	}

	public static string ColorizeName(string name)
	{
		return "<color=#f8ed43>" + name + "</color>";
	}

	public static string ColorizeName(string name, string color)
	{
		return "<color=#" + color + ">" + name + "</color>";
	}

	public static string ColorizeName(string name, Color color)
	{
		return "<color=#" + ColorToHex(color) + ">" + name + "</color>";
	}

	public static string ColorizeAndBoldName(string name)
	{
		return "<b>" + ColorizeName(name) + "</b>";
	}

	public static string ColorizeAndBoldName(string name, string color)
	{
		return "<b>" + ColorizeName(name, color) + "</b>";
	}

	public static string ColorizeSpellTitle(string name)
	{
		return "<color=#D7AE50>" + name + "</color>";
	}

	public static string ColorizeImportantTutorialText(string p_text)
	{
		return "<color=#FE3E83>" + p_text + "</color>";
	}

	private static string ColorToHex(Color color)
	{
		return ColorUtility.ToHtmlStringRGB(color);
	}

	public static string GetRichTextIconForElement(ELEMENTAL_TYPE p_type, TextMeshProUGUI p_textComponentForElementIconHover = null)
	{
		string text = string.Empty;
		switch (p_type)
		{
		case ELEMENTAL_TYPE.Normal:
			text = ColorizeSpellTitle(AttackIconWithLink() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Poison:
			text = ColorizeSpellTitle(PoisonIconWithLink() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Fire:
			text = ColorizeSpellTitle(FireIconWithLink() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Water:
			text = ColorizeSpellTitle(WaterIconWithLink() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Wind:
			text = ColorizeSpellTitle(WindIconWithLink() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Ice:
			text = ColorizeSpellTitle(IceIconWithLink() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Electric:
			text = ColorizeSpellTitle(ElectricIconWithLink() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Earth:
			text = ColorizeSpellTitle(EarthIconWithLink() ?? "") ?? "";
			break;
		}
		if (text != string.Empty && p_textComponentForElementIconHover != null && p_textComponentForElementIconHover.gameObject.GetComponent(typeof(ElementIconHoverEventLabel)) == null)
		{
			p_textComponentForElementIconHover.gameObject.AddComponent(typeof(ElementIconHoverEventLabel));
			p_textComponentForElementIconHover.raycastTarget = true;
		}
		return text;
	}

	public static string GetRichTextIconForElementWithoutLink(ELEMENTAL_TYPE p_type)
	{
		string result = string.Empty;
		switch (p_type)
		{
		case ELEMENTAL_TYPE.Normal:
			result = ColorizeSpellTitle(AttackIcon() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Poison:
			result = ColorizeSpellTitle(PoisonIcon() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Fire:
			result = ColorizeSpellTitle(FireIcon() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Water:
			result = ColorizeSpellTitle(WaterIcon() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Wind:
			result = ColorizeSpellTitle(WindIcon() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Ice:
			result = ColorizeSpellTitle(IceIcon() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Electric:
			result = ColorizeSpellTitle(ElectricIcon() ?? "") ?? "";
			break;
		case ELEMENTAL_TYPE.Earth:
			result = ColorizeSpellTitle(EarthIcon() ?? "") ?? "";
			break;
		}
		return result;
	}

	public static string GetRichTextIconForResistance(RESISTANCE p_type, TextMeshProUGUI p_textComponentForElementIconHover = null)
	{
		string text = string.Empty;
		switch (p_type)
		{
		case RESISTANCE.Fire:
			text = ColorizeSpellTitle(FireIconWithLink() ?? "") ?? "";
			break;
		case RESISTANCE.Poison:
			text = ColorizeSpellTitle(PoisonIconWithLink() ?? "") ?? "";
			break;
		case RESISTANCE.Water:
			text = ColorizeSpellTitle(WaterIconWithLink() ?? "") ?? "";
			break;
		case RESISTANCE.Ice:
			text = ColorizeSpellTitle(IceIconWithLink() ?? "") ?? "";
			break;
		case RESISTANCE.Electric:
			text = ColorizeSpellTitle(ElectricIconWithLink() ?? "") ?? "";
			break;
		case RESISTANCE.Earth:
			text = ColorizeSpellTitle(EarthIconWithLink() ?? "") ?? "";
			break;
		case RESISTANCE.Wind:
			text = ColorizeSpellTitle(WindIconWithLink() ?? "") ?? "";
			break;
		case RESISTANCE.Mental:
			text = ColorizeSpellTitle(MentalIconWithLink() ?? "") ?? "";
			break;
		case RESISTANCE.Physical:
			text = ColorizeSpellTitle(AttackIconWithLink() ?? "") ?? "";
			break;
		}
		if (text != string.Empty && p_textComponentForElementIconHover != null && p_textComponentForElementIconHover.gameObject.GetComponent(typeof(ElementIconHoverEventLabel)) == null)
		{
			p_textComponentForElementIconHover.gameObject.AddComponent(typeof(ElementIconHoverEventLabel));
			p_textComponentForElementIconHover.raycastTarget = true;
		}
		return text;
	}

	public static string GetFirstFewEmotionsAndComafy(string emotionsStr, int emotionCount)
	{
		string[] array = emotionsStr.Split('|');
		string text = string.Empty;
		int num = 0;
		List<string> list = RuinarchListPool<string>.Claim();
		foreach (string text2 in array)
		{
			if (!string.IsNullOrEmpty(text2) && !string.IsNullOrWhiteSpace(text2))
			{
				list.Add(text2);
				num++;
				if (num == emotionCount)
				{
					break;
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			string text3 = list[j];
			text = ((j + 1 != list.Count) ? ((j + 2 != list.Count) ? (text + text3 + ", ") : (text + text3 + " " + LocalizationManager.And + " ")) : (text + text3));
		}
		RuinarchListPool<string>.Release(list);
		return text;
	}

	public static string NormalizeNoSpaceString(string s)
	{
		string[] array = Regex.Split(s, "(?<!^)(?=[A-Z])");
		string text = FirstLetterToUpperCase(array.First());
		for (int i = 1; i < array.Length; i++)
		{
			text = text + " " + array[i];
		}
		return text;
	}

	public static string NormalizeStringLowerCase(string s)
	{
		return s.Replace('_', ' ').ToLowerInvariant();
	}

	public static string NormalizeStringUpperCaseFirstLetterOnly(string s)
	{
		return FirstLetterToUpperCase(s.Replace('_', ' ').ToLowerInvariant());
	}

	public static string NormalizeStringUpperCaseFirstLetters(string s)
	{
		return UpperCaseFirstLetters(s.Replace('_', ' ').ToLowerInvariant());
	}

	public static string Comafy(string originalText)
	{
		string text = string.Empty;
		string[] array = (from x in originalText.Split(' ')
			where !string.IsNullOrEmpty(x)
			select x).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			string text2 = array[num];
			text = ((num + 1 != array.Length) ? ((num + 2 != array.Length) ? (text + text2 + ", ") : (text + text2 + " " + LocalizationManager.And + " ")) : (text + text2));
		}
		return text.Replace('_', ' ');
	}

	public static string NormalizeStringUpperCaseFirstLettersNoSpace(string s)
	{
		s = s.ToLowerInvariant();
		string[] array = s.Split('_');
		string text = FirstLetterToUpperCase(array[0]);
		for (int i = 1; i < array.Length; i++)
		{
			text += FirstLetterToUpperCase(array[i]);
		}
		return text;
	}

	public static string NotNormalizedConversionEnumToString(string s)
	{
		return s.Replace('_', ' ');
	}

	public static string NotNormalizedConversionEnumToStringNoSpaces(string s)
	{
		s = s.Replace('_', ' ');
		return RemoveAllWhiteSpace(s);
	}

	public static string NotNormalizedConversionStringToEnum(string s)
	{
		return s.Replace(' ', '_');
	}

	public static string FirstLetterToUpperCase(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		char[] array = s.ToCharArray();
		array[0] = char.ToUpperInvariant(array[0]);
		return new string(array);
	}

	public static string UpperCaseFirstLetters(string s)
	{
		if (s.Length == 1)
		{
			return s.ToUpperInvariant();
		}
		if (s.Length > 1)
		{
			char[] array = s.ToCharArray();
			if (char.IsLower(array[0]))
			{
				array[0] = char.ToUpperInvariant(array[0]);
			}
			for (int i = 1; i < array.Length; i++)
			{
				if (array[i - 1] == ' ' && char.IsLower(array[i]))
				{
					array[i] = char.ToUpperInvariant(array[i]);
				}
			}
			return new string(array);
		}
		return s;
	}

	public static void GetEnumChoices<T>(List<string> p_choices, bool includeNone = false, params T[] exclude)
	{
		T[] array = (T[])Enum.GetValues(typeof(T));
		for (int i = 0; i < array.Length; i++)
		{
			T value = array[i];
			string text = value.ToString();
			if ((includeNone || !text.Equals("NONE", StringComparison.InvariantCultureIgnoreCase)) && (exclude == null || !exclude.Contains(value)))
			{
				p_choices.Add(text);
			}
		}
	}

	public static void PopulateEnumChoices<T>(List<string> p_choices)
	{
		T[] array = (T[])Enum.GetValues(typeof(T));
		for (int i = 0; i < array.Length; i++)
		{
			T val = array[i];
			string item = NormalizeStringUpperCaseFirstLetters(val.ToString());
			p_choices.Add(item);
		}
	}

	public static void PopulateEnumChoices<T>(List<string> p_choices, T p_param1, T p_param2, T p_param3)
	{
		T[] array = (T[])Enum.GetValues(typeof(T));
		for (int i = 0; i < array.Length; i++)
		{
			T val = array[i];
			string item = NormalizeStringUpperCaseFirstLetters(val.ToString());
			p_choices.Add(item);
		}
	}

	public static void PopulateEnumChoices<T>(List<string> p_choices, T[] p_params)
	{
		for (int i = 0; i < p_params.Length; i++)
		{
			T val = p_params[i];
			string item = NormalizeStringUpperCaseFirstLetters(val.ToString());
			p_choices.Add(item);
		}
	}

	public static void PopulateLocalizedEnumChoices<T>(List<string> p_choices)
	{
		T[] array = (T[])Enum.GetValues(typeof(T));
		for (int i = 0; i < array.Length; i++)
		{
			T val = array[i];
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", val.ToString());
			p_choices.Add(localizedValue);
		}
	}

	public static void PolishPopulateLocalizedEnumChoices<T>(List<string> p_options, bool p_useMaleVersion, params T[] choices)
	{
		if (choices.Length == 0)
		{
			choices = (T[])Enum.GetValues(typeof(T));
		}
		for (int i = 0; i < choices.Length; i++)
		{
			T val = choices[i];
			string text = val.ToString();
			if (text == "Normal" && !p_useMaleVersion)
			{
				text = "Norma";
			}
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", text);
			p_options.Add(localizedValue);
		}
	}

	public static void PopulateExtractionFromString(List<string> p_extractedStrings, string text, string startString, string endString)
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		while (!flag)
		{
			num = text.IndexOf(startString);
			num2 = text.IndexOf(endString);
			if (num != -1 && num2 != -1)
			{
				p_extractedStrings.Add(text.Substring(num + startString.Length, num2 - num - startString.Length));
				text = text.Substring(num2 + endString.Length);
			}
			else
			{
				flag = true;
			}
		}
	}

	public static List<string> ExtractFromString(string text, string startString, string endString)
	{
		List<string> list = new List<string>();
		int num = 0;
		int num2 = 0;
		bool flag = false;
		while (!flag)
		{
			num = text.IndexOf(startString);
			num2 = text.IndexOf(endString);
			if (num != -1 && num2 != -1)
			{
				list.Add(text.Substring(num + startString.Length, num2 - num - startString.Length));
				text = text.Substring(num2 + endString.Length);
			}
			else
			{
				flag = true;
			}
		}
		return list;
	}

	public static string GetArticleForWord(string word, bool capitalized = false)
	{
		if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)"))
		{
			return string.Empty;
		}
		char c = word.ToLowerInvariant().First();
		if (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u')
		{
			if (capitalized)
			{
				return LocalizationManager.Instance.GetLocalizedValue("Articles_Table", "Upper_An");
			}
			return LocalizationManager.Instance.GetLocalizedValue("Articles_Table", "Lower_An");
		}
		if (capitalized)
		{
			return LocalizationManager.Instance.GetLocalizedValue("Articles_Table", "Upper_A");
		}
		return LocalizationManager.Instance.GetLocalizedValue("Articles_Table", "Lower_A");
	}

	public static string RemoveAllWhiteSpace(string str)
	{
		int length = str.Length;
		char[] array = str.ToCharArray();
		int length2 = 0;
		for (int i = 0; i < length; i++)
		{
			char c = array[i];
			switch (c)
			{
			case '\t':
			case '\n':
			case '\v':
			case '\f':
			case '\r':
			case ' ':
			case '\u0085':
			case '\u00a0':
			case '\u1680':
			case '\u2000':
			case '\u2001':
			case '\u2002':
			case '\u2003':
			case '\u2004':
			case '\u2005':
			case '\u2006':
			case '\u2007':
			case '\u2008':
			case '\u2009':
			case '\u200a':
			case '\u2028':
			case '\u2029':
			case '\u202f':
			case '\u205f':
			case '\u3000':
				continue;
			}
			array[length2++] = c;
		}
		return new string(array, 0, length2);
	}

	public static string[] ConvertStringToArray(string str, char separator)
	{
		if (!string.IsNullOrEmpty(str))
		{
			return str.Split(separator);
		}
		return null;
	}

	public static string ConvertArrayToString(string[] str, char separator)
	{
		string text = string.Empty;
		if (str != null)
		{
			for (int i = 0; i < str.Length; i++)
			{
				if (i > 0)
				{
					text += separator;
				}
				text += str[i];
			}
		}
		return text;
	}

	public static string PossessionString(string s)
	{
		if (s.Length <= 1)
		{
			return s;
		}
		if (LocalizationSettings.SelectedLocale.LocaleName.Equals("English (en)"))
		{
			if (s.Last() != 's')
			{
				return s + "'s";
			}
			return s + "'";
		}
		return s;
	}

	public static string PluralizeString(string s)
	{
		if (s.Length <= 1 || !LocalizationSettings.SelectedLocale.LocaleName.Equals("English (en)"))
		{
			return s;
		}
		if (pluralExceptions.ContainsKey(s.ToLowerInvariant()))
		{
			return pluralExceptions[s.ToLowerInvariant()];
		}
		if (s.EndsWith("y", StringComparison.OrdinalIgnoreCase) && !s.EndsWith("ay", StringComparison.OrdinalIgnoreCase) && !s.EndsWith("ey", StringComparison.OrdinalIgnoreCase) && !s.EndsWith("iy", StringComparison.OrdinalIgnoreCase) && !s.EndsWith("oy", StringComparison.OrdinalIgnoreCase) && !s.EndsWith("uy", StringComparison.OrdinalIgnoreCase))
		{
			return s.Substring(0, s.Length - 1) + "ies";
		}
		if (s.EndsWith("us", StringComparison.InvariantCultureIgnoreCase))
		{
			return s + "es";
		}
		if (s.EndsWith("ss", StringComparison.InvariantCultureIgnoreCase))
		{
			return s + "es";
		}
		if (s.EndsWith("s", StringComparison.InvariantCultureIgnoreCase))
		{
			return s;
		}
		if (s.EndsWith("x", StringComparison.InvariantCultureIgnoreCase) || s.EndsWith("ch", StringComparison.InvariantCultureIgnoreCase) || s.EndsWith("sh", StringComparison.InvariantCultureIgnoreCase))
		{
			return s + "es";
		}
		if (s.EndsWith("f", StringComparison.InvariantCultureIgnoreCase) && s.Length > 1)
		{
			return s.Substring(0, s.Length - 1) + "ves";
		}
		if (s.EndsWith("fe", StringComparison.InvariantCultureIgnoreCase) && s.Length > 2)
		{
			return s.Substring(0, s.Length - 2) + "ves";
		}
		if (!s.Equals("Shaman", StringComparison.InvariantCultureIgnoreCase) && !s.Equals("Human", StringComparison.InvariantCultureIgnoreCase) && s.EndsWith("man", StringComparison.InvariantCultureIgnoreCase))
		{
			return s.Substring(0, s.Length - 3) + "men";
		}
		return s + "s";
	}

	public static string ForcePluralizeString(string s)
	{
		if (pluralExceptions.ContainsKey(s.ToLowerInvariant()))
		{
			return pluralExceptions[s.ToLowerInvariant()];
		}
		if (s.EndsWith("y", StringComparison.OrdinalIgnoreCase) && !s.EndsWith("ay", StringComparison.OrdinalIgnoreCase) && !s.EndsWith("ey", StringComparison.OrdinalIgnoreCase) && !s.EndsWith("iy", StringComparison.OrdinalIgnoreCase) && !s.EndsWith("oy", StringComparison.OrdinalIgnoreCase) && !s.EndsWith("uy", StringComparison.OrdinalIgnoreCase))
		{
			return s.Substring(0, s.Length - 1) + "ies";
		}
		if (s.EndsWith("us", StringComparison.InvariantCultureIgnoreCase))
		{
			return s + "es";
		}
		if (s.EndsWith("ss", StringComparison.InvariantCultureIgnoreCase))
		{
			return s + "es";
		}
		if (s.EndsWith("s", StringComparison.InvariantCultureIgnoreCase))
		{
			return s;
		}
		if (s.EndsWith("x", StringComparison.InvariantCultureIgnoreCase) || s.EndsWith("ch", StringComparison.InvariantCultureIgnoreCase) || s.EndsWith("sh", StringComparison.InvariantCultureIgnoreCase))
		{
			return s + "es";
		}
		if (s.EndsWith("f", StringComparison.InvariantCultureIgnoreCase) && s.Length > 1)
		{
			return s.Substring(0, s.Length - 1) + "ves";
		}
		if (s.EndsWith("fe", StringComparison.InvariantCultureIgnoreCase) && s.Length > 2)
		{
			return s.Substring(0, s.Length - 2) + "ves";
		}
		if (!s.Equals("Shaman", StringComparison.InvariantCultureIgnoreCase) && !s.Equals("Human", StringComparison.InvariantCultureIgnoreCase) && s.EndsWith("man", StringComparison.InvariantCultureIgnoreCase))
		{
			return s.Substring(0, s.Length - 3) + "men";
		}
		return s + "s";
	}

	public static string FormulateTextFromEmotions(string emotions, Character actor, IPointOfInterest target, Character reactor)
	{
		if (string.IsNullOrEmpty(emotions) || string.IsNullOrWhiteSpace(emotions))
		{
			if (actor != reactor)
			{
				return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Proper_Response");
			}
			return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Known_Action");
		}
		if (emotions == "aware")
		{
			return ColorizeAndBoldName(reactor.name) + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Already_Knows") + ".";
		}
		string[] source = emotions.Split('/');
		string text = source.ElementAtOrDefault(0);
		string text2 = source.ElementAtOrDefault(1);
		bool flag = !string.IsNullOrEmpty(text);
		bool flag2 = !string.IsNullOrEmpty(text2);
		if (!flag && !flag2)
		{
			return reactor.visuals.GetCharacterStringIcon() + ColorizeAndBoldName(reactor.name) + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Small_Seemed") + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Disinterested_Default_Emotion") + ".";
		}
		if (flag)
		{
			string firstFewEmotionsAndComafy = GetFirstFewEmotionsAndComafy(text, 2);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "After_Receive_Info");
			log.AddToFillers(reactor, reactor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, firstFewEmotionsAndComafy, LOG_IDENTIFIER.STRING_1);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			string logText = log.logText;
			LogPool.Release(log);
			return logText;
		}
		if (flag2)
		{
			string firstFewEmotionsAndComafy2 = GetFirstFewEmotionsAndComafy(text2, 2);
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "After_Receive_Info");
			log2.AddToFillers(reactor, reactor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log2.AddToFillers(null, firstFewEmotionsAndComafy2, LOG_IDENTIFIER.STRING_1);
			log2.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			string logText2 = log2.logText;
			LogPool.Release(log2);
			return logText2;
		}
		return string.Empty;
	}

	public static T Deserialize<T>(string text)
	{
		Encoding uTF = Encoding.UTF8;
		MemoryStream memoryStream = new MemoryStream(uTF.GetBytes(text));
		T result = Deserialize<T>(memoryStream, uTF);
		memoryStream.Dispose();
		return result;
	}

	public static T Deserialize<T>(Stream stream, Encoding encoding)
	{
		T instance = default(T);
		try
		{
			StreamReader streamReader = new StreamReader(stream, encoding);
			fsSerializer fsSerializer = new fsSerializer();
			fsData data = fsJsonParser.Parse(streamReader.ReadToEnd());
			fsSerializer.TryDeserialize(data, ref instance);
			if (instance == null)
			{
				instance = default(T);
			}
			streamReader.Dispose();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return instance;
	}

	public static Dictionary<T, int> MergeWeightedActionDictionaries<T>(Dictionary<T, int> dict1, Dictionary<T, int> dict2)
	{
		Dictionary<T, int> dictionary = new Dictionary<T, int>();
		foreach (KeyValuePair<T, int> item in dict1)
		{
			T key = item.Key;
			int num = item.Value;
			if (dict2.ContainsKey(key))
			{
				num += dict2[key];
			}
			dictionary.Add(key, num);
		}
		foreach (KeyValuePair<T, int> item2 in dict2)
		{
			T key2 = item2.Key;
			int num2 = item2.Value;
			if (dict1.ContainsKey(key2))
			{
				num2 += dict1[key2];
			}
			if (!dictionary.ContainsKey(key2))
			{
				dictionary.Add(key2, num2);
			}
		}
		return dictionary;
	}

	public static Dictionary<T, Dictionary<T, int>> MergeWeightedActionDictionaries<T>(Dictionary<T, Dictionary<T, int>> dict1, Dictionary<T, Dictionary<T, int>> dict2)
	{
		Dictionary<T, Dictionary<T, int>> dictionary = new Dictionary<T, Dictionary<T, int>>();
		foreach (KeyValuePair<T, Dictionary<T, int>> item in dict1)
		{
			T key = item.Key;
			Dictionary<T, int> dictionary2 = item.Value;
			if (dict2.ContainsKey(key))
			{
				dictionary2 = MergeWeightedActionDictionaries(dictionary2, dict2[key]);
			}
			dictionary.Add(key, dictionary2);
		}
		foreach (KeyValuePair<T, Dictionary<T, int>> item2 in dict2)
		{
			T key2 = item2.Key;
			Dictionary<T, int> dictionary3 = item2.Value;
			if (!dictionary.ContainsKey(key2))
			{
				if (dict1.ContainsKey(key2))
				{
					dictionary3 = MergeWeightedActionDictionaries(dictionary3, dict1[key2]);
				}
				dictionary.Add(key2, dictionary3);
			}
		}
		return dictionary;
	}

	public static T PickRandomElementWithWeights<T>(Dictionary<T, int> weights)
	{
		int totalOfWeights = GetTotalOfWeights(weights);
		if (totalOfWeights > 0)
		{
			int num = Rng.Next(0, totalOfWeights);
			int num2 = 0;
			int num3 = 0;
			foreach (KeyValuePair<T, int> weight in weights)
			{
				T key = weight.Key;
				int value = weight.Value;
				if (value > 0)
				{
					num2 += value;
					if (num >= num3 && num < num2)
					{
						return key;
					}
					num3 = num2;
				}
			}
		}
		return default(T);
	}

	public static T PickRandomElementWithWeights<T>(Dictionary<T, float> weights)
	{
		float totalOfWeights = GetTotalOfWeights(weights);
		int num = Rng.Next(0, (int)totalOfWeights);
		float num2 = 0f;
		float num3 = 0f;
		foreach (KeyValuePair<T, float> weight in weights)
		{
			T key = weight.Key;
			float value = weight.Value;
			if (!(value <= 0f))
			{
				num2 += value;
				if ((float)num >= num3 && (float)num < num2)
				{
					return key;
				}
				num3 = num2;
			}
		}
		throw new Exception("Could not pick element in weights");
	}

	public static T[] PickRandomElementWithWeights<T>(Dictionary<T, Dictionary<T, int>> weights)
	{
		int totalOfWeights = GetTotalOfWeights(weights);
		int num = UnityEngine.Random.Range(0, totalOfWeights);
		int num2 = 0;
		int num3 = 0;
		foreach (KeyValuePair<T, Dictionary<T, int>> weight in weights)
		{
			T key = weight.Key;
			foreach (KeyValuePair<T, int> item in weight.Value)
			{
				T key2 = item.Key;
				int value = item.Value;
				if (value >= 0)
				{
					num2 += value;
					if (num >= num3 && num < num2)
					{
						return new T[2] { key, key2 };
					}
					num3 = num2;
				}
			}
		}
		throw new Exception("Could not pick element in weights");
	}

	public static int GetTotalOfWeights<T>(Dictionary<T, Dictionary<T, int>> weights)
	{
		int num = 0;
		foreach (KeyValuePair<T, Dictionary<T, int>> weight in weights)
		{
			foreach (KeyValuePair<T, int> item in weight.Value)
			{
				if (item.Value > 0)
				{
					num += item.Value;
				}
			}
		}
		return num;
	}

	public static int GetTotalOfWeights<T>(Dictionary<T, int> weights)
	{
		return weights.Where((KeyValuePair<T, int> x) => x.Value > 0).Sum((KeyValuePair<T, int> x) => x.Value);
	}

	public static float GetTotalOfWeights<T>(Dictionary<T, float> weights)
	{
		float num = 0f;
		foreach (float value in weights.Values)
		{
			if (value > 0f)
			{
				num += value;
			}
		}
		return num;
	}

	public static string GetWeightsSummary<T>(Dictionary<T, int> weights, string title = "Weights Summary: ")
	{
		string text = title;
		foreach (KeyValuePair<T, int> weight in weights)
		{
			T key = weight.Key;
			_ = weight.Value;
			text = ((!(key is Character)) ? (text + $"\n{weight.Key} - {weight.Value}") : (text + $"\n{(key as Character).name} - {weight.Value}"));
		}
		return text;
	}

	public static string GetWeightsSummary<T>(Dictionary<T, float> weights, string title = "Weights Summary: ")
	{
		string text = title;
		foreach (KeyValuePair<T, float> weight in weights)
		{
			T key = weight.Key;
			_ = weight.Value;
			text = ((!(key is Character)) ? (text + $"\n{weight.Key} - {weight.Value}") : (text + $"\n{(key as Character).name} - {weight.Value}"));
		}
		return text;
	}

	public static string GetWeightsSummary<T>(Dictionary<T, Dictionary<T, int>> weights, string title = "Weights Summary: ")
	{
		string text = title;
		foreach (KeyValuePair<T, Dictionary<T, int>> weight in weights)
		{
			text += $"\n{weight.Key} : ";
			foreach (KeyValuePair<T, int> item in weight.Value)
			{
				text += $"\n     {item.Key} - {item.Value}";
			}
		}
		return text;
	}

	public static bool DoesFileExist(string path)
	{
		return File.Exists(path);
	}

	public static SEXUALITY GetCompatibleSexuality(SEXUALITY sexuality)
	{
		int num = Rng.Next(0, 100);
		switch (sexuality)
		{
		case SEXUALITY.STRAIGHT:
			if (num < 80)
			{
				return SEXUALITY.STRAIGHT;
			}
			return SEXUALITY.BISEXUAL;
		case SEXUALITY.BISEXUAL:
			if (num < 80)
			{
				return SEXUALITY.STRAIGHT;
			}
			if (num >= 80 && num < 90)
			{
				return SEXUALITY.GAY;
			}
			return SEXUALITY.BISEXUAL;
		case SEXUALITY.GAY:
			if (num < 50)
			{
				return SEXUALITY.GAY;
			}
			return SEXUALITY.BISEXUAL;
		default:
			return sexuality;
		}
	}

	public static GENDER GetRandomGender()
	{
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			return GENDER.MALE;
		}
		return GENDER.FEMALE;
	}

	public static GENDER GetOppositeGender(GENDER gender)
	{
		if (gender == GENDER.FEMALE)
		{
			return GENDER.MALE;
		}
		return GENDER.FEMALE;
	}

	public static bool IsEven(int num)
	{
		return num % 2 == 0;
	}

	public static bool IsPositive(float num)
	{
		if (num > 0f)
		{
			return true;
		}
		return false;
	}

	public static bool IsInRange(int value, int lowerBound, int upperBound)
	{
		if (value >= lowerBound && value < upperBound)
		{
			return true;
		}
		return false;
	}

	public static void DestroyChildren(Transform parent)
	{
		Transform[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<Transform>(parent.gameObject);
		foreach (Transform transform in componentsInDirectChildren)
		{
			PooledObject component = transform.gameObject.GetComponent<PooledObject>();
			if (ObjectPoolManager.Instance == null || (object)component == null)
			{
				if (Application.isEditor)
				{
					UnityEngine.Object.DestroyImmediate(transform.gameObject);
				}
				else
				{
					UnityEngine.Object.Destroy(transform.gameObject);
				}
			}
			else
			{
				ObjectPoolManager.Instance.DestroyObject(component);
			}
		}
	}

	public static void DestroyChildrenObjectPool(Transform parent)
	{
		PooledObject[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<PooledObject>(parent.gameObject);
		foreach (PooledObject pooledObject in componentsInDirectChildren)
		{
			ObjectPoolManager.Instance.DestroyObject(pooledObject);
		}
	}

	public static bool IsUIElementInsideScreen(RectTransform uiElement, Canvas canvas)
	{
		Vector3[] array = new Vector3[4];
		uiElement.GetWorldCorners(array);
		Rect pixelRect = Camera.main.pixelRect;
		foreach (Vector3 point in array)
		{
			if (!pixelRect.Contains(point))
			{
				return false;
			}
		}
		return true;
	}

	public static void ScrolRectSnapTo(ScrollRect scrollRect, RectTransform target)
	{
		Canvas.ForceUpdateCanvases();
		scrollRect.content.anchoredPosition = (Vector2)scrollRect.transform.InverseTransformPoint(scrollRect.content.position) - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
	}

	public static void GetAnchorMinMax(TextAnchor type, out Vector2 anchorMin, out Vector2 anchorMax)
	{
		switch (type)
		{
		case TextAnchor.UpperLeft:
			anchorMin = new Vector2(0f, 1f);
			anchorMax = new Vector2(0f, 1f);
			break;
		case TextAnchor.UpperCenter:
			anchorMin = new Vector2(0.5f, 1f);
			anchorMax = new Vector2(0.5f, 1f);
			break;
		case TextAnchor.UpperRight:
			anchorMin = new Vector2(1f, 1f);
			anchorMax = new Vector2(1f, 1f);
			break;
		case TextAnchor.MiddleLeft:
			anchorMin = new Vector2(0f, 0.5f);
			anchorMax = new Vector2(0f, 0.5f);
			break;
		case TextAnchor.MiddleCenter:
			anchorMin = new Vector2(0.5f, 0.5f);
			anchorMax = new Vector2(0.5f, 0.5f);
			break;
		case TextAnchor.MiddleRight:
			anchorMin = new Vector2(1f, 0.5f);
			anchorMax = new Vector2(1f, 0.5f);
			break;
		case TextAnchor.LowerLeft:
			anchorMin = new Vector2(0f, 0f);
			anchorMax = new Vector2(0f, 0f);
			break;
		case TextAnchor.LowerCenter:
			anchorMin = new Vector2(0.5f, 0f);
			anchorMax = new Vector2(0.5f, 0f);
			break;
		case TextAnchor.LowerRight:
			anchorMin = new Vector2(1f, 0f);
			anchorMax = new Vector2(1f, 0f);
			break;
		default:
			anchorMin = new Vector2(1f, 1f);
			anchorMax = new Vector2(1f, 1f);
			break;
		}
	}

	public static TValue GetRandomValueFromDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict)
	{
		return dict.Values.ToList()[Rng.Next(dict.Count)];
	}

	public static TKey GetRandomKeyFromDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict)
	{
		return dict.Keys.ToList()[Rng.Next(dict.Count)];
	}

	public static int GetInteractionPriorityIndex(INTERACTION_TYPE interactionType)
	{
		for (int i = 0; i < interactionPriorityList.Length; i++)
		{
			if (interactionType == interactionPriorityList[i])
			{
				return i;
			}
		}
		return -1;
	}

	public static List<List<T>> ItemCombinations<T>(List<T> inputList, int numOfResults = 0, int minimumItems = 1, int maximumItems = int.MaxValue)
	{
		int num = (int)Math.Pow(2.0, inputList.Count) - 1;
		List<List<T>> list = new List<List<T>>(num + 1);
		if (numOfResults <= 0)
		{
			if (minimumItems == 0)
			{
				list.Add(new List<T>());
			}
			if (minimumItems <= 1 && maximumItems >= inputList.Count)
			{
				for (int i = 1; i <= num; i++)
				{
					list.Add(GenerateCombination(inputList, i));
				}
			}
			else
			{
				for (int j = 1; j <= num; j++)
				{
					int num2 = CountBits(j);
					if (num2 >= minimumItems && num2 <= maximumItems)
					{
						list.Add(GenerateCombination(inputList, j));
					}
				}
			}
		}
		else
		{
			if (minimumItems == 0)
			{
				list.Add(new List<T>());
			}
			if (list.Count >= numOfResults)
			{
				return list;
			}
			if (minimumItems <= 1 && maximumItems >= inputList.Count)
			{
				for (int k = 1; k <= num; k++)
				{
					list.Add(GenerateCombination(inputList, k));
				}
				_ = list.Count;
				return list;
			}
			for (int l = 1; l <= num; l++)
			{
				int num3 = CountBits(l);
				if (num3 >= minimumItems && num3 <= maximumItems)
				{
					list.Add(GenerateCombination(inputList, l));
				}
				if (list.Count >= numOfResults)
				{
					return list;
				}
			}
		}
		return list;
	}

	private static List<T> GenerateCombination<T>(List<T> inputList, int bitPattern)
	{
		List<T> list = new List<T>(inputList.Count);
		for (int i = 0; i < inputList.Count; i++)
		{
			if (((bitPattern >> i) & 1) == 1)
			{
				list.Add(inputList[i]);
			}
		}
		return list;
	}

	private static int CountBits(int bitPattern)
	{
		int num = 0;
		while (bitPattern != 0)
		{
			num++;
			bitPattern &= bitPattern - 1;
		}
		return num;
	}

	public static bool AreTwoCharactersFromOpposingRaces(Character character1, Character character2)
	{
		if (character1.race != character2.race)
		{
			int length = opposingRaces.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				if ((character1.race == opposingRaces[i, 0] || character1.race == opposingRaces[i, 1]) && (character2.race == opposingRaces[i, 0] || character2.race == opposingRaces[i, 1]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static T GetRandomEnumValue<T>()
	{
		Array values = Enum.GetValues(typeof(T));
		int index = UnityEngine.Random.Range(0, values.Length);
		return (T)values.GetValue(index);
	}
}
