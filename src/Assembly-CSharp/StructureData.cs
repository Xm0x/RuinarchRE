using System;
using System.Collections.Generic;
using AK.Wwise;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Structure Data", menuName = "Scriptable Objects/Structure Data")]
public class StructureData : ScriptableObject
{
	[FormerlySerializedAs("structureSprite")]
	[SerializeField]
	private Sprite _structureSprite;

	[FormerlySerializedAs("itemGenerationSetting")]
	[SerializeField]
	private ItemGenerationSetting _itemGenerationSetting;

	[SerializeField]
	private FactionTypeStructuresDictionary structurePrefabs;

	[SerializeField]
	private MonsterMigrationBiomeAtomizedData[] monsterSpawningChoices;

	[FormerlySerializedAs("playerActions")]
	[SerializeField]
	private PLAYER_SKILL_TYPE[] _playerActions;

	[SerializeField]
	private STRUCTURE_PARTY_TYPE _structurePartyType;

	[SerializeField]
	private SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR[] _structurePartyBehaviours;

	private WeightedDictionary<MonsterMigrationBiomeAtomizedData> _spawningWeightedDictionary;

	public string appropriateWorkerClassName;

	[Header("SFX")]
	public AK.Wwise.Event uiSFX;

	public ItemGenerationSetting itemGenerationSetting => _itemGenerationSetting;

	public Sprite structureSprite => _structureSprite;

	public PLAYER_SKILL_TYPE[] playerActions => _playerActions;

	public SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR[] structurePartyBehaviours => _structurePartyBehaviours;

	public STRUCTURE_PARTY_TYPE structurePartyType => _structurePartyType;

	public StructureData()
	{
		structurePrefabs = new FactionTypeStructuresDictionary();
	}

	public List<GameObject> GetStructurePrefabs(FACTION_TYPE p_type, StructureSetting p_structureSetting)
	{
		if (structurePrefabs.ContainsKey(p_type))
		{
			FactionTypeStructures factionTypeStructures = structurePrefabs[p_type];
			if (factionTypeStructures.structureChoices.ContainsKey(p_structureSetting))
			{
				return factionTypeStructures.structureChoices[p_structureSetting];
			}
		}
		if (structurePrefabs.ContainsKey(FACTION_TYPE.None))
		{
			FactionTypeStructures factionTypeStructures2 = structurePrefabs[FACTION_TYPE.None];
			if (factionTypeStructures2.structureChoices.ContainsKey(p_structureSetting))
			{
				return factionTypeStructures2.structureChoices[p_structureSetting];
			}
		}
		throw new Exception($"No structure prefabs for {p_structureSetting}");
	}

	public MonsterMigrationBiomeAtomizedData GetRandomMonsterToSpawn()
	{
		if (_spawningWeightedDictionary == null)
		{
			ConstructWeightedDictionary();
		}
		return _spawningWeightedDictionary?.PickRandomElementGivenWeights();
	}

	public MonsterMigrationBiomeAtomizedData GetMonsterToSpawn(SUMMON_TYPE p_monsterType)
	{
		if (monsterSpawningChoices != null && monsterSpawningChoices.Length != 0)
		{
			for (int i = 0; i < monsterSpawningChoices.Length; i++)
			{
				MonsterMigrationBiomeAtomizedData monsterMigrationBiomeAtomizedData = monsterSpawningChoices[i];
				if (monsterMigrationBiomeAtomizedData.monsterType == p_monsterType)
				{
					return monsterMigrationBiomeAtomizedData;
				}
			}
		}
		return null;
	}

	private void ConstructWeightedDictionary()
	{
		if (monsterSpawningChoices != null && monsterSpawningChoices.Length != 0)
		{
			_spawningWeightedDictionary = new WeightedDictionary<MonsterMigrationBiomeAtomizedData>();
			for (int i = 0; i < monsterSpawningChoices.Length; i++)
			{
				MonsterMigrationBiomeAtomizedData monsterMigrationBiomeAtomizedData = monsterSpawningChoices[i];
				_spawningWeightedDictionary.AddElement(monsterMigrationBiomeAtomizedData, monsterMigrationBiomeAtomizedData.weight);
			}
		}
	}

	public List<GameObject> GetAllStructurePrefabs()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (KeyValuePair<FACTION_TYPE, FactionTypeStructures> structurePrefab in structurePrefabs)
		{
			foreach (KeyValuePair<StructureSetting, List<GameObject>> structureChoice in structurePrefab.Value.structureChoices)
			{
				list.AddRange(structureChoice.Value);
			}
		}
		return list;
	}
}
