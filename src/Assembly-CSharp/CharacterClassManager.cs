using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class CharacterClassManager : BaseMonoBehaviour
{
	private WeightedDictionary<TILE_OBJECT_TYPE> _monsterDropWeights;

	public SUMMON_TYPE[] weakAnimalTypes = new SUMMON_TYPE[7]
	{
		SUMMON_TYPE.Chicken,
		SUMMON_TYPE.Mink,
		SUMMON_TYPE.Moonwalker,
		SUMMON_TYPE.Pig,
		SUMMON_TYPE.Rabbit,
		SUMMON_TYPE.Sheep,
		SUMMON_TYPE.Rat
	};

	private Dictionary<string, CharacterClass> _classesDictionary => ExternalFileManager.Instance.GetCharacterClassCollection();

	public Dictionary<string, CharacterClassBehaviour> classBehaviourDictionary { get; private set; }

	public List<CharacterClass> lowTierNormalCombatantClasses { get; private set; }

	public void Initialize()
	{
		ConstructAllClasses();
		ConstructAllClassBehaviours();
		_monsterDropWeights = new WeightedDictionary<TILE_OBJECT_TYPE>();
	}

	private void ConstructAllClasses()
	{
		lowTierNormalCombatantClasses = new List<CharacterClass>();
		foreach (CharacterClass value in _classesDictionary.Values)
		{
			if (value.IsCombatant() && value.identifier == "Normal" && value.className != "Hero" && (value.className == "Archer" || value.className == "Marauder" || value.className == "Druid"))
			{
				lowTierNormalCombatantClasses.Add(value);
			}
		}
	}

	public CharacterClass GetCharacterClass(string p_className)
	{
		if (HasCharacterClass(p_className))
		{
			return _classesDictionary[p_className];
		}
		return null;
	}

	public bool HasCharacterClass(string p_className)
	{
		return _classesDictionary.ContainsKey(p_className);
	}

	public CharacterClass GetRandomLowTierCombatant()
	{
		return lowTierNormalCombatantClasses[Random.Range(0, lowTierNormalCombatantClasses.Count)];
	}

	public TILE_OBJECT_TYPE GetRandomWeightedItemDropByMonsterType(SUMMON_TYPE p_monsterType)
	{
		string summonClassNameBySummonType = CharacterManager.Instance.GetSummonClassNameBySummonType(p_monsterType);
		return GetRandomWeightedItemDropByCharacterClass(summonClassNameBySummonType);
	}

	private TILE_OBJECT_TYPE GetRandomWeightedItemDropByCharacterClass(string p_className)
	{
		TILE_OBJECT_TYPE result = TILE_OBJECT_TYPE.NONE;
		CharacterClass characterClass = GetCharacterClass(p_className);
		_monsterDropWeights.Clear();
		if (characterClass != null)
		{
			for (int i = 0; i < characterClass.craftableWeapons.Count; i++)
			{
				TILE_OBJECT_TYPE tILE_OBJECT_TYPE = characterClass.craftableWeapons[i];
				EquipmentData equipmentDataBaseOnName = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(tILE_OBJECT_TYPE.ToStringEnumWithSpace());
				if (!(equipmentDataBaseOnName != null) || !equipmentDataBaseOnName.isLegendary)
				{
					_monsterDropWeights.AddElement(tILE_OBJECT_TYPE, 10);
				}
			}
			_monsterDropWeights.AddElement(TILE_OBJECT_TYPE.HEALING_POTION, 20);
			_monsterDropWeights.AddElement(TILE_OBJECT_TYPE.ANTIDOTE, 5);
			_monsterDropWeights.AddElement(TILE_OBJECT_TYPE.POWER_CRYSTAL, 30);
			_monsterDropWeights.AddElement(TILE_OBJECT_TYPE.NONE, ChanceData.GetChance(CHANCE_TYPE.Monster_To_Drop_None));
		}
		if (_monsterDropWeights.Count > 0)
		{
			result = _monsterDropWeights.PickRandomElementGivenWeights();
		}
		return result;
	}

	public TILE_OBJECT_TYPE GetRandomWeightedItemDropByCharacter(Character p_dropper, Character p_attacker)
	{
		TILE_OBJECT_TYPE result = TILE_OBJECT_TYPE.NONE;
		CharacterClass characterClass = GetCharacterClass(p_attacker.characterClass.className);
		_monsterDropWeights.Clear();
		if (characterClass != null)
		{
			for (int i = 0; i < characterClass.craftableWeapons.Count; i++)
			{
				TILE_OBJECT_TYPE tILE_OBJECT_TYPE = characterClass.craftableWeapons[i];
				EquipmentData equipmentDataBaseOnName = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(tILE_OBJECT_TYPE.ToStringEnumWithSpace());
				if (!(equipmentDataBaseOnName != null) || !equipmentDataBaseOnName.isLegendary)
				{
					_monsterDropWeights.AddElement(tILE_OBJECT_TYPE, 10);
				}
			}
			int num = 30;
			int num2 = ChanceData.GetChance(CHANCE_TYPE.Monster_To_Drop_None);
			if (p_attacker.race == RACE.ELVES)
			{
				num += 60;
				num2 -= 60;
				num2 = Mathf.Max(num2, 0);
			}
			_monsterDropWeights.AddElement(TILE_OBJECT_TYPE.HEALING_POTION, 20);
			_monsterDropWeights.AddElement(TILE_OBJECT_TYPE.ANTIDOTE, 5);
			_monsterDropWeights.AddElement(TILE_OBJECT_TYPE.POWER_CRYSTAL, num);
			_monsterDropWeights.AddElement(TILE_OBJECT_TYPE.NONE, num2);
			if (p_dropper.faction != null && p_dropper.faction.factionType.type == FACTION_TYPE.Demons)
			{
				_monsterDropWeights.AddElement(TILE_OBJECT_TYPE.TREASURE_CHEST, 10);
			}
		}
		if (_monsterDropWeights.Count > 0)
		{
			result = _monsterDropWeights.PickRandomElementGivenWeights();
		}
		return result;
	}

	private void ConstructAllClassBehaviours()
	{
		classBehaviourDictionary = new Dictionary<string, CharacterClassBehaviour>
		{
			{
				"Mage",
				new MageClassBehaviour()
			},
			{
				"Shaman",
				new ShamanClassBehaviour()
			},
			{
				"Druid",
				new DruidClassBehaviour()
			},
			{
				"Archer",
				new ArcherClassBehaviour()
			},
			{
				"Stalker",
				new StalkerClassBehaviour()
			},
			{
				"Hunter",
				new HunterClassBehaviour()
			},
			{
				"Marauder",
				new MarauderClassBehaviour()
			},
			{
				"Barbarian",
				new BarbarianClassBehaviour()
			},
			{
				"Knight",
				new KnightClassBehaviour()
			},
			{
				"Noble",
				new NobleClassBehaviour()
			},
			{
				"Hero",
				new HeroClassBehaviour()
			}
		};
	}

	public CharacterClassBehaviour GetClassBehaviour(string p_className)
	{
		if (classBehaviourDictionary.ContainsKey(p_className))
		{
			return classBehaviourDictionary[p_className];
		}
		return null;
	}

	public SUMMON_TYPE GetRandomWeakAnimalType()
	{
		return weakAnimalTypes[GameUtilities.RandomBetweenTwoNumbers(0, weakAnimalTypes.Length - 1)];
	}

	public int GetFoodProducerClassTier(string p_className)
	{
		return p_className switch
		{
			"Farmer" => 1, 
			"Fisher" => 2, 
			"Butcher" => 3, 
			_ => 0, 
		};
	}

	public int GetResourceProducerClassTier(string p_className)
	{
		switch (p_className)
		{
		case "Logger":
		case "Miner":
			return 1;
		case "Skinner":
			return 2;
		default:
			return 0;
		}
	}
}
