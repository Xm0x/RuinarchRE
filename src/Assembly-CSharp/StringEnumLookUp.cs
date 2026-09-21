using System;
using System.Collections.Generic;
using Tutorial;
using UtilityScripts;

public static class StringEnumLookUp
{
	private static bool _hasInitialized;

	private static Dictionary<AGITATE_MESSAGE_TYPE, string> _agitateMessageTypeStrings;

	private static Dictionary<TILE_OBJECT_TYPE, string> _tileObjectTypeStrings;

	private static Dictionary<TILE_OBJECT_TYPE, string> _tileObjectTypeStringsNoSpace;

	private static Dictionary<TILE_OBJECT_TYPE, string> _tileObjectTypeStringsWithSpace;

	private static Dictionary<STRUCTURE_TYPE, string> _structureTypeStrings;

	private static Dictionary<STRUCTURE_TYPE, string> _structureTypeStringsWithSpace;

	private static Dictionary<CRIME_TYPE, string> _crimeTypeStringsWithSpace;

	private static Dictionary<LOG_IDENTIFIER, string> _logIdentifierTypeStrings;

	private static Dictionary<INTERACTION_TYPE, string> _interactionTypeStrings;

	private static Dictionary<SUMMON_TYPE, string> _monsterTypeStringsWithSpace;

	private static Dictionary<SUMMON_TYPE, string> _monsterTypeStringsNoSpace;

	private static Dictionary<JOB_TYPE, string> _jobTypeStrings;

	private static Dictionary<JOB_TYPE, string> _jobTypeStringsNormalizedWithSpace;

	private static Dictionary<INTERRUPT, string> _interruptTypeStrings;

	private static Dictionary<RACE, string> _raceTypeStrings;

	private static Dictionary<RACE, string> _raceTypeStringsWithSpaceNormalized;

	private static Dictionary<PLAYER_SKILL_TYPE, string> _playerSkillTypeStringsNoSpace;

	private static Dictionary<PLAYER_SKILL_TYPE, string> _playerSkillTypeStrings;

	private static Dictionary<POWER_ADDED_EFFECT, string> _afflictionBehaviourTypeStrings;

	public static void Initialize()
	{
		if (!_hasInitialized)
		{
			_hasInitialized = true;
			InitializeAgitateMessageType();
			InitializeTileObjectType();
			InitializeStructureType();
			InitializeCrimeType();
			InitializeLogIdentifierType();
			InitializeInteractionType();
			InitializeMonsterType();
			InitializeJobType();
			InitializeInterruptType();
			InitializeRaceType();
			InitializePlayerSkillType();
			InitializeAfflictionBehaviourType();
		}
	}

	private static void InitializeAgitateMessageType()
	{
		AGITATE_MESSAGE_TYPE[] enumValues = CollectionUtilities.GetEnumValues<AGITATE_MESSAGE_TYPE>();
		_agitateMessageTypeStrings = new Dictionary<AGITATE_MESSAGE_TYPE, string>(enumValues.Length);
		for (int i = 0; i < enumValues.Length; i++)
		{
			_agitateMessageTypeStrings.Add(enumValues[i], enumValues[i].ToString());
		}
	}

	private static void InitializeTileObjectType()
	{
		TILE_OBJECT_TYPE[] enumValues = CollectionUtilities.GetEnumValues<TILE_OBJECT_TYPE>();
		_tileObjectTypeStrings = new Dictionary<TILE_OBJECT_TYPE, string>(enumValues.Length);
		_tileObjectTypeStringsNoSpace = new Dictionary<TILE_OBJECT_TYPE, string>(enumValues.Length);
		_tileObjectTypeStringsWithSpace = new Dictionary<TILE_OBJECT_TYPE, string>(enumValues.Length);
		for (int i = 0; i < enumValues.Length; i++)
		{
			string text = enumValues[i].ToString();
			_tileObjectTypeStrings.Add(enumValues[i], text);
			_tileObjectTypeStringsNoSpace.Add(enumValues[i], Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(text));
			_tileObjectTypeStringsWithSpace.Add(enumValues[i], Utilities.NormalizeStringUpperCaseFirstLetters(text));
		}
	}

	private static void InitializeStructureType()
	{
		STRUCTURE_TYPE[] enumValues = CollectionUtilities.GetEnumValues<STRUCTURE_TYPE>();
		_structureTypeStrings = new Dictionary<STRUCTURE_TYPE, string>(enumValues.Length);
		_structureTypeStringsWithSpace = new Dictionary<STRUCTURE_TYPE, string>(enumValues.Length);
		for (int i = 0; i < enumValues.Length; i++)
		{
			string text = enumValues[i].ToString();
			_structureTypeStrings.Add(enumValues[i], text);
			_structureTypeStringsWithSpace.Add(enumValues[i], Utilities.NormalizeStringUpperCaseFirstLetters(text));
		}
	}

	private static void InitializeCrimeType()
	{
		CRIME_TYPE[] enumValues = CollectionUtilities.GetEnumValues<CRIME_TYPE>();
		_crimeTypeStringsWithSpace = new Dictionary<CRIME_TYPE, string>(enumValues.Length);
		for (int i = 0; i < enumValues.Length; i++)
		{
			_crimeTypeStringsWithSpace.Add(enumValues[i], Utilities.NotNormalizedConversionEnumToString(enumValues[i].ToString()));
		}
	}

	private static void InitializeLogIdentifierType()
	{
		LOG_IDENTIFIER[] enumValues = CollectionUtilities.GetEnumValues<LOG_IDENTIFIER>();
		_logIdentifierTypeStrings = new Dictionary<LOG_IDENTIFIER, string>(enumValues.Length);
		for (int i = 0; i < enumValues.Length; i++)
		{
			_logIdentifierTypeStrings.Add(enumValues[i], enumValues[i].ToString());
		}
	}

	private static void InitializeInteractionType()
	{
		INTERACTION_TYPE[] enumValues = CollectionUtilities.GetEnumValues<INTERACTION_TYPE>();
		_interactionTypeStrings = new Dictionary<INTERACTION_TYPE, string>(enumValues.Length);
		for (int i = 0; i < enumValues.Length; i++)
		{
			_interactionTypeStrings.Add(enumValues[i], enumValues[i].ToString());
		}
	}

	private static void InitializeMonsterType()
	{
		SUMMON_TYPE[] enumValues = CollectionUtilities.GetEnumValues<SUMMON_TYPE>();
		_monsterTypeStringsWithSpace = new Dictionary<SUMMON_TYPE, string>(enumValues.Length);
		_monsterTypeStringsNoSpace = new Dictionary<SUMMON_TYPE, string>(enumValues.Length);
		for (int i = 0; i < enumValues.Length; i++)
		{
			string s = enumValues[i].ToString();
			_monsterTypeStringsWithSpace.Add(enumValues[i], Utilities.NotNormalizedConversionEnumToString(s));
			_monsterTypeStringsNoSpace.Add(enumValues[i], Utilities.NotNormalizedConversionEnumToStringNoSpaces(s));
		}
	}

	private static void InitializeJobType()
	{
		JOB_TYPE[] enumValues = CollectionUtilities.GetEnumValues<JOB_TYPE>();
		_jobTypeStrings = new Dictionary<JOB_TYPE, string>(enumValues.Length);
		_jobTypeStringsNormalizedWithSpace = new Dictionary<JOB_TYPE, string>(enumValues.Length);
		for (int i = 0; i < enumValues.Length; i++)
		{
			string text = enumValues[i].ToString();
			_jobTypeStrings.Add(enumValues[i], text);
			_jobTypeStringsNormalizedWithSpace.Add(enumValues[i], Utilities.NormalizeStringUpperCaseFirstLetters(text));
		}
	}

	private static void InitializeInterruptType()
	{
		INTERRUPT[] enumValues = CollectionUtilities.GetEnumValues<INTERRUPT>();
		_interruptTypeStrings = new Dictionary<INTERRUPT, string>(enumValues.Length);
		for (int i = 0; i < enumValues.Length; i++)
		{
			_interruptTypeStrings.Add(enumValues[i], enumValues[i].ToString());
		}
	}

	private static void InitializeRaceType()
	{
		RACE[] enumValues = CollectionUtilities.GetEnumValues<RACE>();
		_raceTypeStrings = new Dictionary<RACE, string>(enumValues.Length);
		_raceTypeStringsWithSpaceNormalized = new Dictionary<RACE, string>(enumValues.Length);
		for (int i = 0; i < enumValues.Length; i++)
		{
			string text = enumValues[i].ToString();
			_raceTypeStrings.Add(enumValues[i], text);
			_raceTypeStringsWithSpaceNormalized.Add(enumValues[i], Utilities.NormalizeStringUpperCaseFirstLetters(text));
		}
	}

	private static void InitializePlayerSkillType()
	{
		PLAYER_SKILL_TYPE[] enumValues = CollectionUtilities.GetEnumValues<PLAYER_SKILL_TYPE>();
		_playerSkillTypeStrings = new Dictionary<PLAYER_SKILL_TYPE, string>(enumValues.Length);
		_playerSkillTypeStringsNoSpace = new Dictionary<PLAYER_SKILL_TYPE, string>(enumValues.Length);
		for (int i = 0; i < enumValues.Length; i++)
		{
			string text = enumValues[i].ToString();
			_playerSkillTypeStrings.Add(enumValues[i], text);
			_playerSkillTypeStringsNoSpace.Add(enumValues[i], Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(text));
		}
	}

	private static void InitializeAfflictionBehaviourType()
	{
		POWER_ADDED_EFFECT[] enumValues = CollectionUtilities.GetEnumValues<POWER_ADDED_EFFECT>();
		_afflictionBehaviourTypeStrings = new Dictionary<POWER_ADDED_EFFECT, string>(enumValues.Length);
		for (int i = 0; i < enumValues.Length; i++)
		{
			string value = enumValues[i].ToString();
			_afflictionBehaviourTypeStrings.Add(enumValues[i], value);
		}
	}

	public static string ToStringEnum(this AGITATE_MESSAGE_TYPE p_type)
	{
		return _agitateMessageTypeStrings[p_type];
	}

	public static string ToStringEnum(this TILE_OBJECT_TYPE p_type)
	{
		return _tileObjectTypeStrings[p_type];
	}

	public static string ToStringEnumNoSpace(this TILE_OBJECT_TYPE p_type)
	{
		return _tileObjectTypeStringsNoSpace[p_type];
	}

	public static string ToStringEnumWithSpace(this TILE_OBJECT_TYPE p_type)
	{
		return _tileObjectTypeStringsWithSpace[p_type];
	}

	public static string ToStringEnum(this STRUCTURE_TYPE p_type)
	{
		return _structureTypeStrings[p_type];
	}

	public static string ToStringEnumWithSpace(this STRUCTURE_TYPE p_type)
	{
		return _structureTypeStringsWithSpace[p_type];
	}

	public static string ToStringEnumWithSpace(this CRIME_TYPE p_type)
	{
		return _crimeTypeStringsWithSpace[p_type];
	}

	public static string ToStringEnum(this LOG_IDENTIFIER p_type)
	{
		return _logIdentifierTypeStrings[p_type];
	}

	public static string ToStringEnum(this INTERACTION_TYPE p_type)
	{
		return _interactionTypeStrings[p_type];
	}

	public static string ToStringEnumWithSpace(this SUMMON_TYPE p_type)
	{
		return _monsterTypeStringsWithSpace[p_type];
	}

	public static string ToStringEnumNoSpace(this SUMMON_TYPE p_type)
	{
		return _monsterTypeStringsNoSpace[p_type];
	}

	public static string ToStringEnum(this JOB_TYPE p_type)
	{
		return _jobTypeStrings[p_type];
	}

	public static string ToStringEnumWithSpaceNormalized(this JOB_TYPE p_type)
	{
		return _jobTypeStringsNormalizedWithSpace[p_type];
	}

	public static string ToStringEnum(this INTERRUPT p_type)
	{
		return _interruptTypeStrings[p_type];
	}

	public static string ToStringEnum(this RACE p_type)
	{
		return _raceTypeStrings[p_type];
	}

	public static string ToStringEnumWithSpaceNormalized(this RACE p_type)
	{
		return _raceTypeStringsWithSpaceNormalized[p_type];
	}

	public static string ToStringEnum(this PLAYER_SKILL_TYPE p_type)
	{
		Initialize();
		return _playerSkillTypeStrings[p_type];
	}

	public static string ToStringEnumNoSpace(this PLAYER_SKILL_TYPE p_type)
	{
		return _playerSkillTypeStringsNoSpace[p_type];
	}

	public static string ToStringEnum(this POWER_ADDED_EFFECT p_type)
	{
		return _afflictionBehaviourTypeStrings[p_type];
	}

	public static string ToStringEnum(this RESOURCE p_type)
	{
		return p_type switch
		{
			RESOURCE.METAL => "METAL", 
			RESOURCE.STONE => "STONE", 
			RESOURCE.CLOTH => "CLOTH", 
			RESOURCE.FOOD => "FOOD", 
			RESOURCE.WOOD => "WOOD", 
			RESOURCE.LEATHER => "LEATHER", 
			RESOURCE.NONE => "NONE", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this RELIGION p_type)
	{
		return p_type switch
		{
			RELIGION.None => "None", 
			RELIGION.Demon_Worship => "Demon_Worship", 
			RELIGION.Divine_Worship => "Divine_Worship", 
			RELIGION.Nature_Worship => "Nature_Worship", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithSpace(this RELIGION p_type)
	{
		return p_type switch
		{
			RELIGION.None => "None", 
			RELIGION.Demon_Worship => "Demon Worship", 
			RELIGION.Divine_Worship => "Divine Worship", 
			RELIGION.Nature_Worship => "Nature Worship", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithPrefix(this BOOKMARK_CATEGORY p_type)
	{
		return p_type switch
		{
			BOOKMARK_CATEGORY.None => "Bookmark_Category_None", 
			BOOKMARK_CATEGORY.Win_Condition => "Bookmark_Category_Win_Condition", 
			BOOKMARK_CATEGORY.Major_Events => "Bookmark_Category_Major_Events", 
			BOOKMARK_CATEGORY.Portal => "Bookmark_Category_Portal", 
			BOOKMARK_CATEGORY.Player_Parties => "Bookmark_Category_Player_Parties", 
			BOOKMARK_CATEGORY.Targets => "Bookmark_Category_Targets", 
			BOOKMARK_CATEGORY.Alerts => "Bookmark_Category_Alerts", 
			BOOKMARK_CATEGORY.Sub_Goals => "Bookmark_Category_Sub_Goals", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this PLAGUE_FATALITY p_type)
	{
		return p_type switch
		{
			PLAGUE_FATALITY.Septic_Shock => "Septic_Shock", 
			PLAGUE_FATALITY.Heart_Attack => "Heart_Attack", 
			PLAGUE_FATALITY.Stroke => "Stroke", 
			PLAGUE_FATALITY.Total_Organ_Failure => "Total_Organ_Failure", 
			PLAGUE_FATALITY.Pneumonia => "Pneumonia", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumNoSpace(this PLAGUE_FATALITY p_type)
	{
		return p_type switch
		{
			PLAGUE_FATALITY.Septic_Shock => "SepticShock", 
			PLAGUE_FATALITY.Heart_Attack => "HeartAttack", 
			PLAGUE_FATALITY.Stroke => "Stroke", 
			PLAGUE_FATALITY.Total_Organ_Failure => "TotalOrganFailure", 
			PLAGUE_FATALITY.Pneumonia => "Pneumonia", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithSpace(this PLAGUE_FATALITY p_type)
	{
		return p_type switch
		{
			PLAGUE_FATALITY.Septic_Shock => "Septic Shock", 
			PLAGUE_FATALITY.Heart_Attack => "Heart Attack", 
			PLAGUE_FATALITY.Stroke => "Stroke", 
			PLAGUE_FATALITY.Total_Organ_Failure => "Total Organ Failure", 
			PLAGUE_FATALITY.Pneumonia => "Pneumonia", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this PLAGUE_SYMPTOM p_type)
	{
		return p_type switch
		{
			PLAGUE_SYMPTOM.Paralysis => "Paralysis", 
			PLAGUE_SYMPTOM.Vomiting => "Vomiting", 
			PLAGUE_SYMPTOM.Lethargy => "Lethargy", 
			PLAGUE_SYMPTOM.Seizure => "Seizure", 
			PLAGUE_SYMPTOM.Insomnia => "Insomnia", 
			PLAGUE_SYMPTOM.Poison_Cloud => "Poison_Cloud", 
			PLAGUE_SYMPTOM.Monster_Scent => "Monster_Scent", 
			PLAGUE_SYMPTOM.Sneezing => "Sneezing", 
			PLAGUE_SYMPTOM.Depression => "Depression", 
			PLAGUE_SYMPTOM.Hunger_Pangs => "Hunger_Pangs", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithNoSpace(this PLAGUE_SYMPTOM p_type)
	{
		return p_type switch
		{
			PLAGUE_SYMPTOM.Paralysis => "Paralysis", 
			PLAGUE_SYMPTOM.Vomiting => "Vomiting", 
			PLAGUE_SYMPTOM.Lethargy => "Lethargy", 
			PLAGUE_SYMPTOM.Seizure => "Seizure", 
			PLAGUE_SYMPTOM.Insomnia => "Insomnia", 
			PLAGUE_SYMPTOM.Poison_Cloud => "PoisonCloud", 
			PLAGUE_SYMPTOM.Monster_Scent => "MonsterScent", 
			PLAGUE_SYMPTOM.Sneezing => "Sneezing", 
			PLAGUE_SYMPTOM.Depression => "Depression", 
			PLAGUE_SYMPTOM.Hunger_Pangs => "HungerPangs", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithSpace(this PLAGUE_SYMPTOM p_type)
	{
		return p_type switch
		{
			PLAGUE_SYMPTOM.Paralysis => "Paralysis", 
			PLAGUE_SYMPTOM.Vomiting => "Vomiting", 
			PLAGUE_SYMPTOM.Lethargy => "Lethargy", 
			PLAGUE_SYMPTOM.Seizure => "Seizure", 
			PLAGUE_SYMPTOM.Insomnia => "Insomnia", 
			PLAGUE_SYMPTOM.Poison_Cloud => "Poison Cloud", 
			PLAGUE_SYMPTOM.Monster_Scent => "Monster Scent", 
			PLAGUE_SYMPTOM.Sneezing => "Sneezing", 
			PLAGUE_SYMPTOM.Depression => "Depression", 
			PLAGUE_SYMPTOM.Hunger_Pangs => "Hunger Pangs", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this PLAGUE_DEATH_EFFECT p_type)
	{
		return p_type switch
		{
			PLAGUE_DEATH_EFFECT.Explosion => "Explosion", 
			PLAGUE_DEATH_EFFECT.Zombie => "Zombie", 
			PLAGUE_DEATH_EFFECT.Chaos_Generator => "Chaos_Generator", 
			PLAGUE_DEATH_EFFECT.Haunted_Spirits => "Haunted_Spirits", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumNoSpace(this PLAGUE_DEATH_EFFECT p_type)
	{
		return p_type switch
		{
			PLAGUE_DEATH_EFFECT.Explosion => "Explosion", 
			PLAGUE_DEATH_EFFECT.Zombie => "Zombie", 
			PLAGUE_DEATH_EFFECT.Chaos_Generator => "ChaosGenerator", 
			PLAGUE_DEATH_EFFECT.Haunted_Spirits => "HauntedSpirits", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this PLAGUE_TRANSMISSION p_type)
	{
		return p_type switch
		{
			PLAGUE_TRANSMISSION.Airborne => "Airborne", 
			PLAGUE_TRANSMISSION.Consumption => "Consumption", 
			PLAGUE_TRANSMISSION.Physical_Contact => "Physical_Contact", 
			PLAGUE_TRANSMISSION.Combat => "Combat", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this FACTION_TYPE p_type)
	{
		return p_type switch
		{
			FACTION_TYPE.None => "None", 
			FACTION_TYPE.Elven_Kingdom => "Elven_Kingdom", 
			FACTION_TYPE.Human_Empire => "Human_Empire", 
			FACTION_TYPE.Demons => "Demons", 
			FACTION_TYPE.Vagrants => "Vagrants", 
			FACTION_TYPE.Wild_Monsters => "Wild_Monsters", 
			FACTION_TYPE.Bandits => "Bandits", 
			FACTION_TYPE.Undead => "Undead", 
			FACTION_TYPE.Disguised => "Disguised", 
			FACTION_TYPE.Vampire_Clan => "Vampire_Clan", 
			FACTION_TYPE.Lycan_Clan => "Lycan_Clan", 
			FACTION_TYPE.Demon_Cult => "Demon_Cult", 
			FACTION_TYPE.Ratmen => "Ratmen", 
			FACTION_TYPE.Retaliator => "Retaliator", 
			FACTION_TYPE.Divine_Church => "Divine_Church", 
			FACTION_TYPE.Wiccans => "Wiccans", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumNoSpace(this FACTION_TYPE p_type)
	{
		return p_type switch
		{
			FACTION_TYPE.None => "None", 
			FACTION_TYPE.Elven_Kingdom => "ElvenKingdom", 
			FACTION_TYPE.Human_Empire => "HumanEmpire", 
			FACTION_TYPE.Demons => "Demons", 
			FACTION_TYPE.Vagrants => "Vagrants", 
			FACTION_TYPE.Wild_Monsters => "WildMonsters", 
			FACTION_TYPE.Bandits => "Bandits", 
			FACTION_TYPE.Undead => "Undead", 
			FACTION_TYPE.Disguised => "Disguised", 
			FACTION_TYPE.Vampire_Clan => "VampireClan", 
			FACTION_TYPE.Lycan_Clan => "LycanClan", 
			FACTION_TYPE.Demon_Cult => "DemonCult", 
			FACTION_TYPE.Ratmen => "Ratmen", 
			FACTION_TYPE.Retaliator => "Retaliator", 
			FACTION_TYPE.Divine_Church => "DivineChurch", 
			FACTION_TYPE.Wiccans => "Wiccans", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this FACTION_SUCCESSION_TYPE p_type)
	{
		return p_type switch
		{
			FACTION_SUCCESSION_TYPE.None => "None", 
			FACTION_SUCCESSION_TYPE.Lineage => "Lineage", 
			FACTION_SUCCESSION_TYPE.Popularity => "Popularity", 
			FACTION_SUCCESSION_TYPE.Power => "Power", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumNoSpace(this FACTION_SUCCESSION_TYPE p_type)
	{
		return p_type.ToStringEnum();
	}

	public static string ToStringEnumWithSpace(this CRIME_SEVERITY p_type)
	{
		return p_type switch
		{
			CRIME_SEVERITY.Unapplicable => "Unapplicable", 
			CRIME_SEVERITY.None => "None", 
			CRIME_SEVERITY.Infraction => "Infraction", 
			CRIME_SEVERITY.Misdemeanor => "Misdemeanor", 
			CRIME_SEVERITY.Serious => "Serious", 
			CRIME_SEVERITY.Heinous => "Heinous", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this FACTION_RELATIONSHIP_STATUS p_type)
	{
		return p_type switch
		{
			FACTION_RELATIONSHIP_STATUS.Friendly => "Friendly", 
			FACTION_RELATIONSHIP_STATUS.Neutral => "Neutral", 
			FACTION_RELATIONSHIP_STATUS.Hostile => "Hostile", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithSpace(this ARTIFACT_TYPE p_type)
	{
		return p_type switch
		{
			ARTIFACT_TYPE.None => "None", 
			ARTIFACT_TYPE.Necronomicon => "Necronomicon", 
			ARTIFACT_TYPE.Ankh_Of_Anubis => "Ankh Of Anubis", 
			ARTIFACT_TYPE.Berserk_Orb => "Berserk Orb", 
			ARTIFACT_TYPE.Heart_Of_The_Wind => "Heart Of The Wind", 
			ARTIFACT_TYPE.Gorgon_Eye => "Gorgon Eye", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this TIME_IN_WORDS p_type)
	{
		return p_type switch
		{
			TIME_IN_WORDS.AFTER_MIDNIGHT => "AFTER_MIDNIGHT", 
			TIME_IN_WORDS.MORNING => "MORNING", 
			TIME_IN_WORDS.AFTERNOON => "AFTERNOON", 
			TIME_IN_WORDS.EARLY_NIGHT => "EARLY_NIGHT", 
			TIME_IN_WORDS.LATE_NIGHT => "LATE_NIGHT", 
			TIME_IN_WORDS.LUNCH_TIME => "LUNCH_TIME", 
			TIME_IN_WORDS.NONE => "NONE", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this DAILY_SCHEDULE p_type)
	{
		return p_type switch
		{
			DAILY_SCHEDULE.Free_Time => "Free_Time", 
			DAILY_SCHEDULE.Work => "Work", 
			DAILY_SCHEDULE.Sleep => "Sleep", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this GENDER p_type)
	{
		return p_type switch
		{
			GENDER.MALE => "Male", 
			GENDER.FEMALE => "Female", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this CHARACTER_CATEGORY p_type)
	{
		return p_type switch
		{
			CHARACTER_CATEGORY.Villager => "Villager", 
			CHARACTER_CATEGORY.Beast => "Beast", 
			CHARACTER_CATEGORY.Humanoid => "Humanoid", 
			CHARACTER_CATEGORY.Demonic => "Demonic", 
			CHARACTER_CATEGORY.Undead => "Undead", 
			CHARACTER_CATEGORY.None => "None", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this SETTLEMENT_JOB_TRIGGER p_type)
	{
		if (p_type == SETTLEMENT_JOB_TRIGGER.Plague_Care)
		{
			return "Plague_Care";
		}
		throw new ArgumentOutOfRangeException("p_type", p_type, null);
	}

	public static string ToStringEnum(this ELEVATION p_type)
	{
		return p_type switch
		{
			ELEVATION.PLAIN => "PLAIN", 
			ELEVATION.MOUNTAIN => "MOUNTAIN", 
			ELEVATION.WATER => "WATER", 
			ELEVATION.TREES => "TREES", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this CURRENCY p_type)
	{
		return p_type switch
		{
			CURRENCY.Mana => "Mana", 
			CURRENCY.Chaotic_Energy => "Chaotic_Energy", 
			CURRENCY.Spirit_Energy => "Spirit_Energy", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this EQUIPMENT_TYPE p_type)
	{
		return p_type switch
		{
			EQUIPMENT_TYPE.WEAPON => "WEAPON", 
			EQUIPMENT_TYPE.ARMOR => "ARMOR", 
			EQUIPMENT_TYPE.ACCESSORY => "ACCESSORY", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this MAP_OBJECT_STATE p_type)
	{
		return p_type switch
		{
			MAP_OBJECT_STATE.BUILT => "BUILT", 
			MAP_OBJECT_STATE.UNBUILT => "UNBUILT", 
			MAP_OBJECT_STATE.BUILDING => "BUILDING", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this Crops.Growth_State p_type)
	{
		return p_type switch
		{
			Crops.Growth_State.Ripe => "Ripe", 
			Crops.Growth_State.Growing => "Growing", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithPrefix(this FOOD_INFUSE_TYPE p_type)
	{
		return p_type switch
		{
			FOOD_INFUSE_TYPE.None => "Infused_Food_None", 
			FOOD_INFUSE_TYPE.Bloated => "Infused_Food_Bloated", 
			FOOD_INFUSE_TYPE.Food_Coma => "Infused_Food_Food_Coma", 
			FOOD_INFUSE_TYPE.Rabid => "Infused_Food_Rabid", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this FURNITURE_TYPE p_type)
	{
		return p_type switch
		{
			FURNITURE_TYPE.NONE => "NONE", 
			FURNITURE_TYPE.BED => "BED", 
			FURNITURE_TYPE.TABLE => "TABLE", 
			FURNITURE_TYPE.DESK => "DESK", 
			FURNITURE_TYPE.GUITAR => "GUITAR", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumNoSpace(this SETTLEMENT_TYPE p_type)
	{
		return p_type switch
		{
			SETTLEMENT_TYPE.Human_Village => "HumanVillage", 
			SETTLEMENT_TYPE.Elven_Hamlet => "ElvenHamlet", 
			SETTLEMENT_TYPE.Capital => "Capital", 
			SETTLEMENT_TYPE.Cult_Town => "CultTown", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithSpace(this SETTLEMENT_TYPE p_type)
	{
		return p_type switch
		{
			SETTLEMENT_TYPE.Human_Village => "Human Village", 
			SETTLEMENT_TYPE.Elven_Hamlet => "Elven Hamlet", 
			SETTLEMENT_TYPE.Capital => "Capital", 
			SETTLEMENT_TYPE.Cult_Town => "Cult Town", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumNoSpace(this SETTLEMENT_EVENT p_type)
	{
		return p_type switch
		{
			SETTLEMENT_EVENT.Vampire_Hunt => "VampireHunt", 
			SETTLEMENT_EVENT.Werewolf_Hunt => "WerewolfHunt", 
			SETTLEMENT_EVENT.Plagued_Event => "PlaguedEvent", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this PLAGUE_EVENT_RESPONSE p_type)
	{
		return p_type switch
		{
			PLAGUE_EVENT_RESPONSE.Undecided => "Undecided", 
			PLAGUE_EVENT_RESPONSE.Do_Nothing => "Do_Nothing", 
			PLAGUE_EVENT_RESPONSE.Quarantine => "Quarantine", 
			PLAGUE_EVENT_RESPONSE.Slay => "Slay", 
			PLAGUE_EVENT_RESPONSE.Exile => "Exile", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumNoSpace(this CHARACTER_TALENT p_type)
	{
		return p_type switch
		{
			CHARACTER_TALENT.None => "None", 
			CHARACTER_TALENT.Martial_Arts => "MartialArts", 
			CHARACTER_TALENT.Combat_Magic => "CombatMagic", 
			CHARACTER_TALENT.Healing_Magic => "HealingMagic", 
			CHARACTER_TALENT.Crafting => "Crafting", 
			CHARACTER_TALENT.Resources => "Resources", 
			CHARACTER_TALENT.Food => "Food", 
			CHARACTER_TALENT.Social => "Social", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this CHARACTER_COMBAT_BEHAVIOUR p_type)
	{
		return p_type switch
		{
			CHARACTER_COMBAT_BEHAVIOUR.None => "None", 
			CHARACTER_COMBAT_BEHAVIOUR.Tower => "Tower", 
			CHARACTER_COMBAT_BEHAVIOUR.Attacker => "Attacker", 
			CHARACTER_COMBAT_BEHAVIOUR.Snatcher => "Snatcher", 
			CHARACTER_COMBAT_BEHAVIOUR.Razer => "Razer", 
			CHARACTER_COMBAT_BEHAVIOUR.Healer => "Healer", 
			CHARACTER_COMBAT_BEHAVIOUR.Tank => "Tank", 
			CHARACTER_COMBAT_BEHAVIOUR.Escort => "Escort", 
			CHARACTER_COMBAT_BEHAVIOUR.Glass_Cannon => "Glass_Cannon", 
			CHARACTER_COMBAT_BEHAVIOUR.Defender => "Defender", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumNoSpace(this CHARACTER_COMBAT_BEHAVIOUR p_type)
	{
		if (p_type == CHARACTER_COMBAT_BEHAVIOUR.Glass_Cannon)
		{
			return "GlassCannon";
		}
		return p_type.ToStringEnum();
	}

	public static string ToStringEnumLowercase(this REACTION_STATUS p_type)
	{
		return p_type switch
		{
			REACTION_STATUS.WITNESSED => "witnessed", 
			REACTION_STATUS.INFORMED => "informed", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithSpace(this CRIME_STATUS p_type)
	{
		return p_type switch
		{
			CRIME_STATUS.Unpunished => "Unpunished", 
			CRIME_STATUS.Punished => "Punished", 
			CRIME_STATUS.Exiled => "Exiled", 
			CRIME_STATUS.Absolved => "Absolved", 
			CRIME_STATUS.Executed => "Executed", 
			CRIME_STATUS.Burned_At_Stake => "Burned At Stake", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumNoSpace(this FACTION_IDEOLOGY p_type)
	{
		return p_type switch
		{
			FACTION_IDEOLOGY.Inclusive => "Inclusive", 
			FACTION_IDEOLOGY.Exclusive => "Exclusive", 
			FACTION_IDEOLOGY.Warmonger => "Warmonger", 
			FACTION_IDEOLOGY.Peaceful => "Peaceful", 
			FACTION_IDEOLOGY.Divine_Worship => "DivineWorship", 
			FACTION_IDEOLOGY.Nature_Worship => "NatureWorship", 
			FACTION_IDEOLOGY.Demon_Worship => "DemonWorship", 
			FACTION_IDEOLOGY.Reveres_Vampires => "ReveresVampires", 
			FACTION_IDEOLOGY.Reveres_Werewolves => "ReveresWerewolves", 
			FACTION_IDEOLOGY.Hates_Vampires => "HatesVampires", 
			FACTION_IDEOLOGY.Hates_Werewolves => "HatesWerewolves", 
			FACTION_IDEOLOGY.Bone_Golem_Makers => "BoneGolemMakers", 
			FACTION_IDEOLOGY.Raiders => "Raiders", 
			FACTION_IDEOLOGY.Wyvern_Tamers => "WyvernTamers", 
			FACTION_IDEOLOGY.Tower_Defense => "TowerDefense", 
			FACTION_IDEOLOGY.Lightning_Tower_Defense => "LightningTowerDefense", 
			FACTION_IDEOLOGY.Breeders => "Breeders", 
			FACTION_IDEOLOGY.Entkin => "Entkin", 
			FACTION_IDEOLOGY.Golem_Makers => "GolemMakers", 
			FACTION_IDEOLOGY.Blood_Sacrifices => "BloodSacrifices", 
			FACTION_IDEOLOGY.Mage_Guild => "MageGuild", 
			FACTION_IDEOLOGY.Necromantic => "Necromantic", 
			FACTION_IDEOLOGY.Beastmasters => "Beastmasters", 
			FACTION_IDEOLOGY.Infested => "Infested", 
			FACTION_IDEOLOGY.Slavers => "Slavers", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumNoSpace(this PARTY_QUEST_TYPE p_type)
	{
		return p_type switch
		{
			PARTY_QUEST_TYPE.None => "None", 
			PARTY_QUEST_TYPE.Exploration => "Exploration", 
			PARTY_QUEST_TYPE.Rescue => "Rescue", 
			PARTY_QUEST_TYPE.Extermination => "Extermination", 
			PARTY_QUEST_TYPE.Counterattack => "Counterattack", 
			PARTY_QUEST_TYPE.Raid => "Raid", 
			PARTY_QUEST_TYPE.Heirloom_Hunt => "HeirloomHunt", 
			PARTY_QUEST_TYPE.Demon_Defend => "DemonDefend", 
			PARTY_QUEST_TYPE.Demon_Snatch => "DemonSnatch", 
			PARTY_QUEST_TYPE.Demon_Raid => "DemonRaid", 
			PARTY_QUEST_TYPE.Demon_Rescue => "DemonRescue", 
			PARTY_QUEST_TYPE.Morning_Patrol => "MorningPatrol", 
			PARTY_QUEST_TYPE.Night_Patrol => "NightPatrol", 
			PARTY_QUEST_TYPE.Hunt_Beast => "HuntBeast", 
			PARTY_QUEST_TYPE.Blood_Hunt => "BloodHunt", 
			PARTY_QUEST_TYPE.Recruit_Vampires => "RecruitVampires", 
			PARTY_QUEST_TYPE.Demon_Steal => "DemonSteal", 
			PARTY_QUEST_TYPE.Bounty_Hunt => "BountyHunt", 
			PARTY_QUEST_TYPE.Claim_Hallowed_Ground => "ClaimHallowedGround", 
			PARTY_QUEST_TYPE.Defend_Hallowed_Ground => "DefendHallowedGround", 
			PARTY_QUEST_TYPE.Kill_Villager => "KillVillager", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this PARTY_QUEST_TYPE p_type)
	{
		return p_type switch
		{
			PARTY_QUEST_TYPE.None => "None", 
			PARTY_QUEST_TYPE.Exploration => "Exploration", 
			PARTY_QUEST_TYPE.Rescue => "Rescue", 
			PARTY_QUEST_TYPE.Extermination => "Extermination", 
			PARTY_QUEST_TYPE.Counterattack => "Counterattack", 
			PARTY_QUEST_TYPE.Raid => "Raid", 
			PARTY_QUEST_TYPE.Heirloom_Hunt => "Heirloom_Hunt", 
			PARTY_QUEST_TYPE.Demon_Defend => "Demon_Defend", 
			PARTY_QUEST_TYPE.Demon_Snatch => "Demon_Snatch", 
			PARTY_QUEST_TYPE.Demon_Raid => "Demon_Raid", 
			PARTY_QUEST_TYPE.Demon_Rescue => "Demon_Rescue", 
			PARTY_QUEST_TYPE.Morning_Patrol => "Morning_Patrol", 
			PARTY_QUEST_TYPE.Night_Patrol => "Night_Patrol", 
			PARTY_QUEST_TYPE.Hunt_Beast => "Hunt_Beast", 
			PARTY_QUEST_TYPE.Blood_Hunt => "Blood_Hunt", 
			PARTY_QUEST_TYPE.Recruit_Vampires => "Recruit_Vampires", 
			PARTY_QUEST_TYPE.Demon_Steal => "Demon_Steal", 
			PARTY_QUEST_TYPE.Bounty_Hunt => "Bounty_Hunt", 
			PARTY_QUEST_TYPE.Claim_Hallowed_Ground => "Claim_Hallowed_Ground", 
			PARTY_QUEST_TYPE.Defend_Hallowed_Ground => "Defend_Hallowed_Ground", 
			PARTY_QUEST_TYPE.Kill_Villager => "Kill_Villager", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this PLAYER_ARCHETYPE p_type)
	{
		return p_type switch
		{
			PLAYER_ARCHETYPE.Normal => "Normal", 
			PLAYER_ARCHETYPE.Progression_Ravager => "Progression_Ravager", 
			PLAYER_ARCHETYPE.Progression_Lich => "Progression_Lich", 
			PLAYER_ARCHETYPE.Progression_Puppet_Master => "Progression_Puppet_Master", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumNoSpace(this PLAYER_GOAL p_type)
	{
		return p_type switch
		{
			PLAYER_GOAL.Undead_Supremacy => "UndeadSupremacy", 
			PLAYER_GOAL.Creatures_of_the_Night => "CreaturesOfTheNight", 
			PLAYER_GOAL.Idol_Worship => "IdolWorship", 
			PLAYER_GOAL.Terrorized_Villagers => "TerrorizedVillagers", 
			PLAYER_GOAL.Outbreak => "Outbreak", 
			PLAYER_GOAL.Death_And_Destruction => "DeathAndDestruction", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this PLAYER_GOAL p_type)
	{
		return p_type switch
		{
			PLAYER_GOAL.Undead_Supremacy => "Undead_Supremacy", 
			PLAYER_GOAL.Creatures_of_the_Night => "Creatures_of_the_Night", 
			PLAYER_GOAL.Idol_Worship => "Idol_Worship", 
			PLAYER_GOAL.Terrorized_Villagers => "Terrorized_Villagers", 
			PLAYER_GOAL.Outbreak => "Outbreak", 
			PLAYER_GOAL.Death_And_Destruction => "Death_And_Destruction", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this SUB_GOAL p_type)
	{
		return p_type switch
		{
			SUB_GOAL.GOAL_POISON_FOOD => "GOAL_POISON_FOOD", 
			SUB_GOAL.GOAL_IMPRISON_VILLAGER => "GOAL_IMPRISON_VILLAGER", 
			SUB_GOAL.GOAL_CAPTURE_TRITON => "GOAL_CAPTURE_TRITON", 
			SUB_GOAL.GOAL_FALLEN_ANGEL => "GOAL_FALLEN_ANGEL", 
			SUB_GOAL.GOAL_DEMON_CULTIST => "GOAL_DEMON_CULTIST", 
			SUB_GOAL.GOAL_DESTROY_RESOURCE => "GOAL_DESTROY_RESOURCE", 
			SUB_GOAL.GOAL_CREATE_PSYCHO => "GOAL_CREATE_PSYCHO", 
			SUB_GOAL.GOAL_REMOVE_BUFF => "GOAL_REMOVE_BUFF", 
			SUB_GOAL.GOAL_KILL_ELF => "GOAL_KILL_ELF", 
			SUB_GOAL.GOAL_KILL_HUMAN => "GOAL_KILL_HUMAN", 
			SUB_GOAL.GOAL_MAKE_VILLAGER_EVIL => "GOAL_MAKE_VILLAGER_EVIL", 
			SUB_GOAL.GOAL_SHARE_CRIME_INTEL => "GOAL_SHARE_CRIME_INTEL", 
			SUB_GOAL.GOAL_SNATCH_OBJECT => "GOAL_SNATCH_OBJECT", 
			SUB_GOAL.GOAL_TRIGGER_FLAW => "GOAL_TRIGGER_FLAW", 
			SUB_GOAL.GOAL_TRIGGER_AROUSAL => "GOAL_TRIGGER_AROUSAL", 
			SUB_GOAL.GOAL_POISON_CLOUD => "GOAL_POISON_CLOUD", 
			SUB_GOAL.GOAL_FROSTY_FOG => "GOAL_FROSTY_FOG", 
			SUB_GOAL.GOAL_BALL_LIGHTNING => "GOAL_BALL_LIGHTNING", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this EQUIPMENT_PREFIX p_type)
	{
		return p_type switch
		{
			EQUIPMENT_PREFIX.None => "None", 
			EQUIPMENT_PREFIX.Weightless => "Weightless", 
			EQUIPMENT_PREFIX.Burning => "Burning", 
			EQUIPMENT_PREFIX.Icy => "Icy", 
			EQUIPMENT_PREFIX.Venomous => "Venomous", 
			EQUIPMENT_PREFIX.Moist => "Moist", 
			EQUIPMENT_PREFIX.Static => "Static", 
			EQUIPMENT_PREFIX.Breezy => "Breezy", 
			EQUIPMENT_PREFIX.Sharp => "Sharp", 
			EQUIPMENT_PREFIX.Mentor => "Mentor", 
			EQUIPMENT_PREFIX.Dull => "Dull", 
			EQUIPMENT_PREFIX.Deadly => "Deadly", 
			EQUIPMENT_PREFIX.Festering => "Festering", 
			EQUIPMENT_PREFIX.Haunted => "Haunted", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this Game_Alert p_type)
	{
		return p_type switch
		{
			Game_Alert.Retaliation => "Retaliation", 
			Game_Alert.Upgrade_Portal => "Upgrade_Portal", 
			Game_Alert.Release_Powers => "Release_Powers", 
			Game_Alert.Pause_Reminder => "Pause_Reminder", 
			Game_Alert.Trigger_Flaw => "Trigger_Flaw", 
			Game_Alert.Chaos_Orbs => "Chaos_Orbs", 
			Game_Alert.Build_Spire => "Build_Spire", 
			Game_Alert.Build_Prison => "Build_Prison", 
			Game_Alert.Build_Watcher => "Build_Watcher", 
			Game_Alert.Build_Imp_Hut_Or_Crypt => "Build_Imp_Hut_Or_Crypt", 
			Game_Alert.Apply_Affliction => "Apply_Affliction", 
			Game_Alert.Cultists => "Cultists", 
			Game_Alert.Devastation_Ritual_Alert => "Devastation_Ritual_Alert", 
			Game_Alert.Faction_Aware_Alert => "Faction_Aware_Alert", 
			Game_Alert.Spawn_Defensive_Units => "Spawn_Defensive_Units", 
			Game_Alert.Mummified_Release_Alert => "Mummified_Release_Alert", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumNoSpace(this Game_Alert p_type)
	{
		return p_type switch
		{
			Game_Alert.Retaliation => "Retaliation", 
			Game_Alert.Upgrade_Portal => "UpgradePortal", 
			Game_Alert.Release_Powers => "ReleasePowers", 
			Game_Alert.Pause_Reminder => "PauseReminder", 
			Game_Alert.Trigger_Flaw => "TriggerFlaw", 
			Game_Alert.Chaos_Orbs => "ChaosOrbs", 
			Game_Alert.Build_Spire => "BuildSpire", 
			Game_Alert.Build_Prison => "BuildPrison", 
			Game_Alert.Build_Watcher => "BuildWatcher", 
			Game_Alert.Build_Imp_Hut_Or_Crypt => "BuildImpHutOrCrypt", 
			Game_Alert.Apply_Affliction => "ApplyAffliction", 
			Game_Alert.Cultists => "Cultists", 
			Game_Alert.Devastation_Ritual_Alert => "DevastationRitualAlert", 
			Game_Alert.Faction_Aware_Alert => "FactionAwareAlert", 
			Game_Alert.Spawn_Defensive_Units => "SpawnDefensiveUnits", 
			Game_Alert.Mummified_Release_Alert => "MummifiedReleaseAlert", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this TutorialManager.Tutorial_Type p_type)
	{
		return p_type switch
		{
			TutorialManager.Tutorial_Type.Unlocking_Bonus_Powers => "Unlocking_Bonus_Powers", 
			TutorialManager.Tutorial_Type.Upgrading_The_Portal => "Upgrading_The_Portal", 
			TutorialManager.Tutorial_Type.Mana => "Mana", 
			TutorialManager.Tutorial_Type.Chaotic_Energy => "Chaotic_Energy", 
			TutorialManager.Tutorial_Type.Storing_Targets => "Storing_Targets", 
			TutorialManager.Tutorial_Type.Maraud => "Maraud", 
			TutorialManager.Tutorial_Type.Intel => "Intel", 
			TutorialManager.Tutorial_Type.Spirit_Energy => "Spirit_Energy", 
			TutorialManager.Tutorial_Type.Migration_Controls => "Migration_Controls", 
			TutorialManager.Tutorial_Type.Base_Building => "Base_Building", 
			TutorialManager.Tutorial_Type.Abilities => "Abilities", 
			TutorialManager.Tutorial_Type.Resistances => "Resistances", 
			TutorialManager.Tutorial_Type.Time_Management => "Time_Management", 
			TutorialManager.Tutorial_Type.Target_Menu => "Target_Menu", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithSpaceNormalizedUppercaseFirstLetterOnly(this RELATIONSHIP_TYPE p_type)
	{
		return p_type switch
		{
			RELATIONSHIP_TYPE.NONE => "None", 
			RELATIONSHIP_TYPE.RELATIVE => "Relative", 
			RELATIONSHIP_TYPE.LOVER => "Lover", 
			RELATIONSHIP_TYPE.AFFAIR => "Affair", 
			RELATIONSHIP_TYPE.EX_LOVER => "Ex lover", 
			RELATIONSHIP_TYPE.SIBLING => "Sibling", 
			RELATIONSHIP_TYPE.PARENT => "Parent", 
			RELATIONSHIP_TYPE.CHILD => "Child", 
			RELATIONSHIP_TYPE.MASTER => "Master", 
			RELATIONSHIP_TYPE.PET => "Pet", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithSpaceNormalized(this RELATIONSHIP_TYPE p_type)
	{
		if (p_type == RELATIONSHIP_TYPE.EX_LOVER)
		{
			return "Ex Lover";
		}
		return p_type.ToStringEnumWithSpaceNormalizedUppercaseFirstLetterOnly();
	}

	public static string ToStringEnum(this LIST_OF_CRITERIA p_type)
	{
		return p_type switch
		{
			LIST_OF_CRITERIA.NoOne => "NoOne", 
			LIST_OF_CRITERIA.Trait => "Trait", 
			LIST_OF_CRITERIA.Class => "Class", 
			LIST_OF_CRITERIA.Gender => "Gender", 
			LIST_OF_CRITERIA.Race => "Race", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this OPINIONS p_type)
	{
		return p_type switch
		{
			OPINIONS.NoOne => "NoOne", 
			OPINIONS.Rival => "Rival", 
			OPINIONS.Enemy => "Enemy", 
			OPINIONS.Acquaintance => "Acquaintance", 
			OPINIONS.Everyone => "Everyone", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this Portal_Upgrade_Type p_type)
	{
		return p_type switch
		{
			Portal_Upgrade_Type.Essential => "Essential", 
			Portal_Upgrade_Type.Optional_Other => "Optional_Other", 
			Portal_Upgrade_Type.Optional_Self => "Optional_Self", 
			Portal_Upgrade_Type.Common_Structures => "Common_Structures", 
			Portal_Upgrade_Type.Lesser_Demon => "Lesser_Demon", 
			Portal_Upgrade_Type.Wildcard => "Wildcard", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this ELEMENTAL_TYPE p_type)
	{
		return p_type switch
		{
			ELEMENTAL_TYPE.Normal => "Normal", 
			ELEMENTAL_TYPE.Fire => "Fire", 
			ELEMENTAL_TYPE.Poison => "Poison", 
			ELEMENTAL_TYPE.Water => "Water", 
			ELEMENTAL_TYPE.Ice => "Ice", 
			ELEMENTAL_TYPE.Electric => "Electric", 
			ELEMENTAL_TYPE.Earth => "Earth", 
			ELEMENTAL_TYPE.Wind => "Wind", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithIcon(this ELEMENTAL_TYPE p_type)
	{
		return p_type switch
		{
			ELEMENTAL_TYPE.Normal => "Normal_Element", 
			ELEMENTAL_TYPE.Fire => "Fire", 
			ELEMENTAL_TYPE.Poison => "Poison", 
			ELEMENTAL_TYPE.Water => "Water", 
			ELEMENTAL_TYPE.Ice => "Ice", 
			ELEMENTAL_TYPE.Electric => "Electric", 
			ELEMENTAL_TYPE.Earth => "Earth", 
			ELEMENTAL_TYPE.Wind => "Wind", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithIcon(this RESISTANCE p_type)
	{
		return p_type switch
		{
			RESISTANCE.None => "None", 
			RESISTANCE.Fire => "Fire", 
			RESISTANCE.Poison => "Poison", 
			RESISTANCE.Water => "Water", 
			RESISTANCE.Ice => "Ice", 
			RESISTANCE.Electric => "Electric", 
			RESISTANCE.Earth => "Earth", 
			RESISTANCE.Wind => "Wind", 
			RESISTANCE.Mental => "Mental", 
			RESISTANCE.Physical => "Normal_Element", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this STORED_TARGET_TYPE p_type)
	{
		return p_type switch
		{
			STORED_TARGET_TYPE.Character => "Character", 
			STORED_TARGET_TYPE.Tile_Objects => "Tile_Objects", 
			STORED_TARGET_TYPE.Structures => "Structures", 
			STORED_TARGET_TYPE.Monster => "Monster", 
			STORED_TARGET_TYPE.Village => "Village", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this LOG_TAG p_type)
	{
		return p_type switch
		{
			LOG_TAG.Life_Changes => "Life_Changes", 
			LOG_TAG.Social => "Social", 
			LOG_TAG.Needs => "Needs", 
			LOG_TAG.Work => "Work", 
			LOG_TAG.Combat => "Combat", 
			LOG_TAG.Crimes => "Crimes", 
			LOG_TAG.Witnessed => "Witnessed", 
			LOG_TAG.Informed => "Informed", 
			LOG_TAG.Party => "Party", 
			LOG_TAG.Major => "Major", 
			LOG_TAG.Player => "Player", 
			LOG_TAG.Intel => "Intel", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithSpace(this LOG_TAG p_type)
	{
		if (p_type == LOG_TAG.Life_Changes)
		{
			return "Life Changes";
		}
		return p_type.ToStringEnum();
	}

	public static string ToStringEnum(this BLACKMAIL_TYPE p_type)
	{
		return p_type switch
		{
			BLACKMAIL_TYPE.None => "None", 
			BLACKMAIL_TYPE.Strong => "Strong", 
			BLACKMAIL_TYPE.Normal => "Normal", 
			BLACKMAIL_TYPE.Weak => "Weak", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithSpace(this TEMPTATION p_type)
	{
		return p_type switch
		{
			TEMPTATION.Dark_Blessing => "Dark Blessing", 
			TEMPTATION.Empower => "Empower", 
			TEMPTATION.Cleanse_Flaws => "Cleanse Flaws", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this TIPS p_type)
	{
		return p_type switch
		{
			TIPS.Time_Management => "Time_Management", 
			TIPS.Chaotic_Energy => "Chaotic_Energy", 
			TIPS.Target_Menu => "Target_Menu", 
			TIPS.Base_Building => "Base_Building", 
			TIPS.Unlocking_Bonus_Powers => "Unlocking_Bonus_Powers", 
			TIPS.Upgrading_The_Portal => "Upgrading_The_Portal", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this BOOKMARK_TYPE p_type)
	{
		return p_type switch
		{
			BOOKMARK_TYPE.Progress_Bar => "Progress_Bar", 
			BOOKMARK_TYPE.Text => "Text", 
			BOOKMARK_TYPE.Text_With_Cancel => "Text_With_Cancel", 
			BOOKMARK_TYPE.Special => "Special", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this CRITICAL_BREAK_ACTION p_type)
	{
		return p_type switch
		{
			CRITICAL_BREAK_ACTION.Break_Up => "Break_Up", 
			CRITICAL_BREAK_ACTION.Abandon_Faction => "Abandon_Faction", 
			CRITICAL_BREAK_ACTION.Kill_Target => "Kill_Target", 
			CRITICAL_BREAK_ACTION.Become_Cannibal => "Become_Cannibal", 
			CRITICAL_BREAK_ACTION.Lightning_Storm => "Lightning_Storm", 
			CRITICAL_BREAK_ACTION.Expel => "Expel", 
			CRITICAL_BREAK_ACTION.Commit_Suicide => "Commit_Suicide", 
			CRITICAL_BREAK_ACTION.Become_Bandit => "Become_Bandit", 
			CRITICAL_BREAK_ACTION.Become_Evil => "Become_Evil", 
			CRITICAL_BREAK_ACTION.Destroy_Structure => "Destroy_Structure", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this TRIGGER_GRUDGE_ACTION p_type)
	{
		return p_type switch
		{
			TRIGGER_GRUDGE_ACTION.Attack_Kill => "Attack_Kill", 
			TRIGGER_GRUDGE_ACTION.Destroy_Home => "Destroy_Home", 
			TRIGGER_GRUDGE_ACTION.Fabricate_Crime => "Fabricate_Crime", 
			TRIGGER_GRUDGE_ACTION.Booby_Trap => "Booby_Trap", 
			TRIGGER_GRUDGE_ACTION.Summon_Monsters => "Summon_Monsters", 
			TRIGGER_GRUDGE_ACTION.Expel_Party => "Expel_Party", 
			TRIGGER_GRUDGE_ACTION.Leave_Party => "Leave_Party", 
			TRIGGER_GRUDGE_ACTION.Expel_Faction => "Expel_Faction", 
			TRIGGER_GRUDGE_ACTION.Turn_Evil => "Turn_Evil", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_type)
	{
		return p_type switch
		{
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager => "Snatch_Villager", 
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster => "Snatch_Monster", 
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Kill_Villager => "Kill_Villager", 
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Supplies => "Destroy_Supplies", 
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Structures => "Destroy_Structures", 
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Harass_Villagers => "Harass_Villagers", 
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Defenses => "Destroy_Defenses", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithSpace(this SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_type)
	{
		return p_type switch
		{
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager => "Snatch Villager", 
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster => "Snatch Monster", 
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Kill_Villager => "Kill Villager", 
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Supplies => "Destroy Supplies", 
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Structures => "Destroy Structures", 
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Harass_Villagers => "Harass Villagers", 
			SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Defenses => "Destroy Defenses", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithSpaceNormalized(this LOCATION_TYPE p_type)
	{
		return p_type switch
		{
			LOCATION_TYPE.VILLAGE => "Village", 
			LOCATION_TYPE.DEMONIC_INTRUSION => "Demonic Intrusion", 
			LOCATION_TYPE.DUNGEON => "Dungeon", 
			LOCATION_TYPE.EMPTY => "Empty", 
			LOCATION_TYPE.PSEUDO_VILLAGE => "Pseudo Village", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnumWithSpaceNormalized(this PLAYER_SKILL_CATEGORY p_type)
	{
		return p_type switch
		{
			PLAYER_SKILL_CATEGORY.NONE => "None", 
			PLAYER_SKILL_CATEGORY.SPELL => "Spell", 
			PLAYER_SKILL_CATEGORY.AFFLICTION => "Affliction", 
			PLAYER_SKILL_CATEGORY.PLAYER_ACTION => "Player Action", 
			PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE => "Demonic Structure", 
			PLAYER_SKILL_CATEGORY.MINION => "Minion", 
			PLAYER_SKILL_CATEGORY.SUMMON => "Summon", 
			PLAYER_SKILL_CATEGORY.SCHEME => "Scheme", 
			PLAYER_SKILL_CATEGORY.BUILD => "Build", 
			PLAYER_SKILL_CATEGORY.RAID => "Raid", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}

	public static string ToStringEnum(this HAIR_COLOR p_type)
	{
		return p_type switch
		{
			HAIR_COLOR.Brunette => "Brunette", 
			HAIR_COLOR.Blonde => "Blonde", 
			HAIR_COLOR.Redhead => "Redhead", 
			HAIR_COLOR.Orange => "Orange", 
			HAIR_COLOR.Green => "Green", 
			HAIR_COLOR.Blue => "Blue", 
			HAIR_COLOR.White => "White", 
			_ => throw new ArgumentOutOfRangeException("p_type", p_type, null), 
		};
	}
}
