using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using Locations.Area_Features;
using Locations.Region_Features;
using Locations.Settlements;
using Locations.Settlements.Settlement_Types;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Serialization;
using UtilityScripts;

public class LandmarkManager : BaseMonoBehaviour
{
	[FormerlySerializedAs("areaMapsParent")]
	[SerializeField]
	private Transform innerMapsParent;

	[SerializeField]
	private GameObject regionInnerStructurePrefab;

	public static LandmarkManager Instance;

	[SerializeField]
	private StructureDataDictionary structureData;

	private List<LocationStructure> _structuresToCleanUp;

	public readonly Dictionary<BIOMES, WildernessMonsterSpawnerData[]> wildernessMonsterSpawnerData = new Dictionary<BIOMES, WildernessMonsterSpawnerData[]>
	{
		{
			BIOMES.FOREST,
			new WildernessMonsterSpawnerData[6]
			{
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Centaur,
					structureType = STRUCTURE_TYPE.JUNGLE_RAMPART,
					biomeType = BIOMES.FOREST
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Gorgon,
					structureType = STRUCTURE_TYPE.JUNGLE_RAMPART,
					biomeType = BIOMES.FOREST
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Water_Nymph,
					structureType = STRUCTURE_TYPE.TEMPLE,
					biomeType = BIOMES.FOREST
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Wyvern,
					structureType = STRUCTURE_TYPE.DRAGON_LAIR,
					biomeType = BIOMES.FOREST
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Scorpion,
					structureType = STRUCTURE_TYPE.NOXIOUS_CAVE,
					biomeType = BIOMES.FOREST
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Incubus,
					structureType = STRUCTURE_TYPE.ANCIENT_RUIN,
					biomeType = BIOMES.FOREST
				}
			}
		},
		{
			BIOMES.GRASSLAND,
			new WildernessMonsterSpawnerData[5]
			{
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Electric_Wisp,
					structureType = STRUCTURE_TYPE.MAGE_TOWER,
					biomeType = BIOMES.GRASSLAND
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Skeleton,
					structureType = STRUCTURE_TYPE.DEAD_GROUNDS,
					biomeType = BIOMES.GRASSLAND
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Tarantula,
					structureType = STRUCTURE_TYPE.ABANDONED_MINE,
					biomeType = BIOMES.GRASSLAND
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Giant_Spider,
					structureType = STRUCTURE_TYPE.ABANDONED_MINE,
					biomeType = BIOMES.GRASSLAND
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Orc,
					structureType = STRUCTURE_TYPE.MONSTER_CAMP,
					biomeType = BIOMES.GRASSLAND
				}
			}
		},
		{
			BIOMES.SNOW,
			new WildernessMonsterSpawnerData[6]
			{
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Ghoul,
					structureType = STRUCTURE_TYPE.ANCIENT_GRAVEYARD,
					biomeType = BIOMES.SNOW
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Kobold,
					structureType = STRUCTURE_TYPE.TEMPLE,
					biomeType = BIOMES.SNOW
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Succubus,
					structureType = STRUCTURE_TYPE.ANCIENT_RUIN,
					biomeType = BIOMES.SNOW
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Troll,
					structureType = STRUCTURE_TYPE.BEAST_LAIR,
					biomeType = BIOMES.SNOW
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Ice_Nymph,
					structureType = STRUCTURE_TYPE.FROZEN_SHRINE,
					biomeType = BIOMES.SNOW
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Broodmother,
					structureType = STRUCTURE_TYPE.DEAD_GROUNDS,
					biomeType = BIOMES.SNOW
				}
			}
		},
		{
			BIOMES.DESERT,
			new WildernessMonsterSpawnerData[7]
			{
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Golem,
					structureType = STRUCTURE_TYPE.MAGE_TOWER,
					biomeType = BIOMES.DESERT
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Harpy,
					structureType = STRUCTURE_TYPE.BEAST_LAIR,
					biomeType = BIOMES.DESERT
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Sludge,
					structureType = STRUCTURE_TYPE.NOXIOUS_CAVE,
					biomeType = BIOMES.DESERT
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Wind_Nymph,
					structureType = STRUCTURE_TYPE.MAGE_TOWER,
					biomeType = BIOMES.DESERT
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Fire_Elemental,
					structureType = STRUCTURE_TYPE.FIERY_CAVE,
					biomeType = BIOMES.DESERT
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Dwarf_Paladin,
					structureType = STRUCTURE_TYPE.ANCIENT_GRAVEYARD,
					biomeType = BIOMES.DESERT
				},
				new WildernessMonsterSpawnerData
				{
					monsterType = SUMMON_TYPE.Splatter,
					structureType = STRUCTURE_TYPE.BEAST_LAIR,
					biomeType = BIOMES.DESERT
				}
			}
		}
	};

	public List<BaseSettlement> allSettlements => DatabaseManager.Instance.settlementDatabase.allSettlements;

	public List<NPCSettlement> allNonPlayerSettlements => DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements;

	public IEnumerator GenerateRegionMap(Region region, MapGenerationComponent mapGenerationComponent, MapGenerationData data)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(regionInnerStructurePrefab, innerMapsParent);
		RegionInnerTileMap innerTileMap = gameObject.GetComponent<RegionInnerTileMap>();
		float xSeed = UnityEngine.Random.Range(0f, 99999f);
		float ySeed = UnityEngine.Random.Range(0f, 99999f);
		int biomeSeed = UnityEngine.Random.Range(0, 99999);
		int elevationSeed = UnityEngine.Random.Range(0, 99999);
		innerTileMap.Initialize(region, xSeed, ySeed, biomeSeed, elevationSeed);
		region.GenerateStructures();
		yield return StartCoroutine(innerTileMap.GenerateMap(mapGenerationComponent, data));
		InnerMapManager.Instance.OnCreateInnerMap(innerTileMap);
	}

	public IEnumerator GenerateScenarioMap(Region region, MapGenerationComponent mapGenerationComponent, MapGenerationData data, ScenarioMapData scenarioMapData)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(regionInnerStructurePrefab, innerMapsParent);
		RegionInnerTileMap innerTileMap = gameObject.GetComponent<RegionInnerTileMap>();
		innerTileMap.Initialize(region, scenarioMapData.worldMapSave.xSeed, scenarioMapData.worldMapSave.ySeed, scenarioMapData.worldMapSave.elevationPerlinNoiseSettings, scenarioMapData.worldMapSave.warpWeight, scenarioMapData.worldMapSave.temperatureSeed);
		region.GenerateStructures();
		yield return StartCoroutine(innerTileMap.GenerateMap(mapGenerationComponent, data));
		InnerMapManager.Instance.OnCreateInnerMap(innerTileMap);
	}

	public IEnumerator LoadRegionMap(Region region, MapGenerationComponent mapGenerationComponent, SaveDataInnerMap saveDataInnerMap, SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Map");
		GameObject gameObject = UnityEngine.Object.Instantiate(regionInnerStructurePrefab, innerMapsParent);
		RegionInnerTileMap innerTileMap = gameObject.GetComponent<RegionInnerTileMap>();
		float xSeed = saveDataInnerMap.xSeed;
		float ySeed = saveDataInnerMap.ySeed;
		innerTileMap.Initialize(region, xSeed, ySeed, saveDataInnerMap.elevationPerlinNoiseSettings, saveDataInnerMap.warpWeight, saveDataInnerMap.temperatureSeed);
		yield return StartCoroutine(innerTileMap.LoadMap(mapGenerationComponent, saveDataInnerMap, saveData));
		InnerMapManager.Instance.OnCreateInnerMap(innerTileMap);
	}

	public void Initialize()
	{
		_structuresToCleanUp = new List<LocationStructure>();
		Messenger.AddListener(Signals.AFTER_TICK_ENDED, AfterTickEnded);
	}

	private void AfterTickEnded()
	{
		ProcessStructuresMarkedForCleanUp();
	}

	private void Awake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}

	public List<LocationStructure> GetStructuresOfType(STRUCTURE_TYPE p_structureType)
	{
		Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures = GridMap.Instance.mainRegion.structures;
		if (structures.ContainsKey(p_structureType))
		{
			return structures[p_structureType];
		}
		return null;
	}

	public int GetStructuresOfTypeCount(STRUCTURE_TYPE p_structureType)
	{
		int result = 0;
		List<LocationStructure> structuresOfType = GetStructuresOfType(p_structureType);
		if (structuresOfType != null)
		{
			result = structuresOfType.Count;
		}
		return result;
	}

	public void PopulateAllSpecialStructures(List<LocationStructure> specialStructures)
	{
		foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> structure in GridMap.Instance.mainRegion.structures)
		{
			if (structure.Key.IsSpecialStructure())
			{
				specialStructures.AddRange(structure.Value);
			}
		}
	}

	public NPCSettlement CreateNewSettlement(Region region, LOCATION_TYPE locationType, Area p_occupiedArea)
	{
		NPCSettlement nPCSettlement = new NPCSettlement(region, locationType);
		if (p_occupiedArea != null)
		{
			nPCSettlement.AddAreaToSettlement(p_occupiedArea);
		}
		Messenger.Broadcast(SettlementSignals.SETTLEMENT_CREATED, (BaseSettlement)nPCSettlement);
		DatabaseManager.Instance.settlementDatabase.RegisterSettlement(nPCSettlement);
		nPCSettlement.Initialize(shouldSubscribeToSignals: true);
		return nPCSettlement;
	}

	public NPCSettlement CreateNewSettlementMultipleAreas(Region region, LOCATION_TYPE locationType, List<Area> p_occupiedAreas)
	{
		NPCSettlement nPCSettlement = new NPCSettlement(region, locationType);
		if (p_occupiedAreas != null)
		{
			nPCSettlement.AddAreaToSettlement(p_occupiedAreas);
		}
		Messenger.Broadcast(SettlementSignals.SETTLEMENT_CREATED, (BaseSettlement)nPCSettlement);
		DatabaseManager.Instance.settlementDatabase.RegisterSettlement(nPCSettlement);
		nPCSettlement.Initialize(shouldSubscribeToSignals: true);
		return nPCSettlement;
	}

	public NPCSettlement LoadNPCSettlement(SaveDataNPCSettlement saveDataNpcSettlement)
	{
		NPCSettlement nPCSettlement = new NPCSettlement(saveDataNpcSettlement);
		DatabaseManager.Instance.settlementDatabase.RegisterSettlement(nPCSettlement);
		return nPCSettlement;
	}

	public PlayerSettlement CreateNewPlayerSettlement(Area p_area)
	{
		PlayerSettlement playerSettlement = new PlayerSettlement();
		playerSettlement.AddAreaToSettlement(p_area);
		Messenger.Broadcast(SettlementSignals.SETTLEMENT_CREATED, (BaseSettlement)playerSettlement);
		DatabaseManager.Instance.settlementDatabase.RegisterSettlement(playerSettlement);
		playerSettlement.SubscribeToListeners();
		return playerSettlement;
	}

	public PlayerSettlement LoadPlayerSettlement(SaveDataPlayerSettlement saveDataPlayerSettlement)
	{
		PlayerSettlement playerSettlement = new PlayerSettlement(saveDataPlayerSettlement);
		DatabaseManager.Instance.settlementDatabase.RegisterSettlement(playerSettlement);
		return playerSettlement;
	}

	public NPCSettlement GetRandomActiveSapientSettlement()
	{
		List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim();
		for (int i = 0; i < allNonPlayerSettlements.Count; i++)
		{
			NPCSettlement nPCSettlement = allNonPlayerSettlements[i];
			if (nPCSettlement.locationType == LOCATION_TYPE.VILLAGE && nPCSettlement.owner != null && nPCSettlement.residents.Count > 0 && nPCSettlement.owner.race.IsSapient())
			{
				list.Add(nPCSettlement);
			}
		}
		NPCSettlement result = null;
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)] as NPCSettlement;
		}
		RuinarchListPool<BaseSettlement>.Release(list);
		return result;
	}

	public NPCSettlement GetRandomVillageSettlementForNecromancerAttack(Region region, Faction faction, Character p_necromancer)
	{
		NPCSettlement result = null;
		List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim();
		bool flag = p_necromancer.traitContainer.HasTrait("Demon Cultist");
		for (int i = 0; i < allNonPlayerSettlements.Count; i++)
		{
			NPCSettlement nPCSettlement = allNonPlayerSettlements[i];
			if (nPCSettlement.region == region && nPCSettlement.locationType == LOCATION_TYPE.VILLAGE && nPCSettlement.HasAliveResident() && nPCSettlement.owner != faction && (nPCSettlement.owner == null || faction == null || faction.IsHostileWith(nPCSettlement.owner)) && (!flag || nPCSettlement.owner == null || (nPCSettlement.owner.factionType.type != FACTION_TYPE.Demon_Cult && nPCSettlement.owner.factionType.type != FACTION_TYPE.Demons)))
			{
				list.Add(nPCSettlement);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)] as NPCSettlement;
		}
		RuinarchListPool<BaseSettlement>.Release(list);
		return result;
	}

	public BaseSettlement GetSettlementByPersistentID(string id)
	{
		return DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(id);
	}

	public void OwnSettlement(Faction newOwner, BaseSettlement settlement)
	{
		if (settlement.owner != null)
		{
			UnownSettlement(settlement);
		}
		newOwner.AddToOwnedSettlements(settlement);
		settlement.SetOwner(newOwner);
	}

	public void UnownSettlement(BaseSettlement settlement)
	{
		settlement.owner?.RemoveFromOwnedSettlements(settlement);
		settlement.SetOwner(null);
	}

	public SETTLEMENT_TYPE GetSettlementTypeForCharacter(Character character)
	{
		if (character.faction != null && character.faction.factionType.type == FACTION_TYPE.Demon_Cult)
		{
			return SETTLEMENT_TYPE.Cult_Town;
		}
		return GetSettlementTypeForRace(character.race);
	}

	public SETTLEMENT_TYPE GetSettlementTypeForRace(RACE race)
	{
		return race switch
		{
			RACE.HUMANS => SETTLEMENT_TYPE.Human_Village, 
			RACE.ELVES => SETTLEMENT_TYPE.Elven_Hamlet, 
			_ => SETTLEMENT_TYPE.Human_Village, 
		};
	}

	public SETTLEMENT_TYPE GetSettlementTypeForFaction(Faction faction)
	{
		switch (faction.factionType.type)
		{
		case FACTION_TYPE.Elven_Kingdom:
			return SETTLEMENT_TYPE.Elven_Hamlet;
		case FACTION_TYPE.Human_Empire:
			return SETTLEMENT_TYPE.Human_Village;
		case FACTION_TYPE.Vampire_Clan:
		case FACTION_TYPE.Lycan_Clan:
			return GetSettlementTypeForRace(faction.race);
		case FACTION_TYPE.Demon_Cult:
			return SETTLEMENT_TYPE.Cult_Town;
		default:
			return GetSettlementTypeForRace(faction.race);
		}
	}

	public LocationStructure CreateNewStructureAt(Region location, STRUCTURE_TYPE structureType, BaseSettlement settlement = null)
	{
		string text = Utilities.RemoveAllWhiteSpace(structureType.ToStringEnumWithSpace());
		Type type = Type.GetType("Inner_Maps.Location_Structures." + text + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
		if (type != null)
		{
			LocationStructure locationStructure = Activator.CreateInstance(type, location) as LocationStructure;
			location.AddStructure(locationStructure);
			settlement?.AddStructure(locationStructure);
			locationStructure.Initialize();
			DatabaseManager.Instance.structureDatabase.RegisterStructure(locationStructure);
			return locationStructure;
		}
		throw new Exception("No structure class for type " + structureType.ToStringEnumWithSpace() + ", " + text);
	}

	public LocationStructure LoadNewStructureAt(Region location, STRUCTURE_TYPE structureType, SaveDataLocationStructure saveDataLocationStructure)
	{
		string text = Utilities.RemoveAllWhiteSpace(structureType.ToStringEnumWithSpace());
		Type type = Type.GetType("Inner_Maps.Location_Structures." + text + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
		if (type != null)
		{
			LocationStructure locationStructure = Activator.CreateInstance(type, location, saveDataLocationStructure) as LocationStructure;
			if (!locationStructure.hasBeenDestroyed)
			{
				locationStructure.InitializeFromSave();
			}
			DatabaseManager.Instance.structureDatabase.RegisterStructure(locationStructure);
			return locationStructure;
		}
		throw new Exception("No structure class for type " + structureType.ToStringEnumWithSpace() + ", " + text);
	}

	public IEnumerator PlaceBuiltSpecialStructure(BaseSettlement settlement, InnerTileMap innerTileMap, RESOURCE structureResource, [NotNull] params STRUCTURE_TYPE[] structureTypes)
	{
		foreach (STRUCTURE_TYPE structureType in structureTypes)
		{
			Area tileLocation = settlement.areas.ElementAtOrDefault(0);
			PlaceBuiltStructureForSettlement(FACTION_TYPE.None, settlement, innerTileMap, tileLocation, structureType, structureResource);
			yield return null;
		}
	}

	public void PlaceBuiltStructureForSettlement(FACTION_TYPE p_factionType, BaseSettlement settlement, InnerTileMap innerTileMap, Area tileLocation, STRUCTURE_TYPE structureType, RESOURCE structureResource)
	{
		GameObject randomElement = CollectionUtilities.GetRandomElement(InnerMapManager.Instance.GetStructurePrefabsForStructure(p_factionType, structureType, structureResource));
		innerTileMap.PlaceBuiltStructureTemplateAt(randomElement, tileLocation, settlement);
	}

	public IEnumerator PlaceFirstStructureForSettlement(FACTION_TYPE p_factionType, BaseSettlement settlement, InnerTileMap innerTileMap, StructureSetting structureSetting)
	{
		Area tileLocation = settlement.areas[0];
		PlaceIndividualBuiltStructureForSettlement(p_factionType, settlement, innerTileMap, tileLocation, structureSetting);
		yield return null;
	}

	private List<LocationStructure> PlaceIndividualBuiltStructureForSettlement(FACTION_TYPE p_factionType, BaseSettlement settlement, InnerTileMap innerTileMap, Area tileLocation, StructureSetting structureSetting)
	{
		GameObject randomElement = CollectionUtilities.GetRandomElement(InnerMapManager.Instance.GetStructurePrefabsForStructure(p_factionType, structureSetting));
		return innerTileMap.PlaceBuiltStructureTemplateAt(randomElement, tileLocation, settlement);
	}

	public LocationStructure PlaceIndividualBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, LocationGridTile tileLocation, string prefabName)
	{
		GameObject originalObjectFromPool = ObjectPoolManager.Instance.GetOriginalObjectFromPool(prefabName);
		return innerTileMap.PlaceBuiltStructureTemplateAt(originalObjectFromPool, tileLocation, settlement);
	}

	public LocationStructure PlaceIndividualBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, GameObject chosenPrefab, LocationGridTile centerTile)
	{
		return innerTileMap.PlaceBuiltStructureTemplateAt(chosenPrefab, centerTile, settlement);
	}

	public bool CanPlaceStructureBlueprint(FACTION_TYPE p_factionType, NPCSettlement npcSettlement, StructureSetting structureToPlace, out LocationGridTile targetTile, out string structurePrefabName, out int connectorToUse, out LocationGridTile connectorTile)
	{
		bool canPlace = false;
		List<StructureConnector> list = RuinarchListPool<StructureConnector>.Claim();
		npcSettlement.PopulateStructureConnectorsForStructureType(list, structureToPlace.structureType);
		string functionLog;
		if (structureToPlace.structureType == STRUCTURE_TYPE.MINE)
		{
			list = list.OrderBy((StructureConnector c) => Vector2.Distance(c.transform.position, npcSettlement.cityCenter.tiles.ElementAt(0).centeredWorldLocation)).ToList();
			CanPlaceStructureBlueprintMine(p_factionType, npcSettlement, structureToPlace, list, out targetTile, out structurePrefabName, out connectorToUse, out connectorTile, out canPlace, out functionLog);
		}
		else
		{
			CollectionUtilities.Shuffle(list);
			CanPlaceStructureBlueprintDefault(p_factionType, npcSettlement, structureToPlace, list, out targetTile, out structurePrefabName, out connectorToUse, out connectorTile, out canPlace, out functionLog);
		}
		RuinarchListPool<StructureConnector>.Release(list);
		return canPlace;
	}

	public StructureConnector CanPlaceStructureBlueprintDefault(FACTION_TYPE p_factionType, NPCSettlement npcSettlement, StructureSetting structureToPlace, List<StructureConnector> availableStructureConnectors, out LocationGridTile targetTile, out string structurePrefabName, out int connectorToUse, out LocationGridTile connectorTile, out bool canPlace, out string functionLog)
	{
		List<GameObject> structurePrefabsForStructure = InnerMapManager.Instance.GetStructurePrefabsForStructure(p_factionType, structureToPlace);
		CollectionUtilities.Shuffle(structurePrefabsForStructure);
		canPlace = false;
		targetTile = null;
		structurePrefabName = string.Empty;
		connectorToUse = -1;
		connectorTile = null;
		functionLog = string.Empty;
		for (int i = 0; i < structurePrefabsForStructure.Count; i++)
		{
			GameObject gameObject = structurePrefabsForStructure[i];
			int usedConnectorIndex;
			LocationGridTile tileToPlaceStructure;
			string functionLog2;
			StructureConnector firstValidConnector = gameObject.GetComponent<LocationStructureObject>().GetFirstValidConnector(availableStructureConnectors, npcSettlement.region.innerMap, npcSettlement, out usedConnectorIndex, out tileToPlaceStructure, out connectorTile, structureToPlace, out functionLog2);
			if (firstValidConnector != null)
			{
				targetTile = tileToPlaceStructure;
				structurePrefabName = gameObject.name;
				connectorToUse = usedConnectorIndex;
				canPlace = true;
				return firstValidConnector;
			}
		}
		return null;
	}

	public StructureConnector CanPlaceStructureBlueprintMine(FACTION_TYPE p_factionType, NPCSettlement npcSettlement, StructureSetting structureToPlace, List<StructureConnector> availableStructureConnectors, out LocationGridTile targetTile, out string structurePrefabName, out int connectorToUse, out LocationGridTile connectorTile, out bool canPlace, out string functionLog)
	{
		List<GameObject> structurePrefabsForStructure = InnerMapManager.Instance.GetStructurePrefabsForStructure(p_factionType, structureToPlace);
		CollectionUtilities.Shuffle(structurePrefabsForStructure);
		canPlace = false;
		targetTile = null;
		structurePrefabName = string.Empty;
		connectorToUse = -1;
		connectorTile = null;
		functionLog = string.Empty;
		for (int i = 0; i < availableStructureConnectors.Count; i++)
		{
			StructureConnector structureConnector = availableStructureConnectors[i];
			for (int j = 0; j < structurePrefabsForStructure.Count; j++)
			{
				GameObject gameObject = structurePrefabsForStructure[j];
				if (gameObject.GetComponent<LocationStructureObject>().IsConnectorValid(structureConnector, npcSettlement.region.innerMap, npcSettlement, out var usedConnectorIndex, out var tileToPlaceStructure, out connectorTile, structureToPlace, out var _))
				{
					targetTile = tileToPlaceStructure;
					structurePrefabName = gameObject.name;
					connectorToUse = usedConnectorIndex;
					canPlace = true;
					return structureConnector;
				}
			}
		}
		return null;
	}

	public bool HasEnoughSpaceForStructure(string structurePrefabName, LocationGridTile tileLocation)
	{
		return ObjectPoolManager.Instance.GetOriginalObjectFromPool(structurePrefabName).GetComponent<LocationStructureObject>().HasEnoughSpaceIfPlacedOn(tileLocation);
	}

	public bool HasAffectedCorruptedTilesForStructure(string structurePrefabName, LocationGridTile tileLocation)
	{
		return ObjectPoolManager.Instance.GetOriginalObjectFromPool(structurePrefabName).GetComponent<LocationStructureObject>().HasAffectedCorruptedTilesIfPlacedOn(tileLocation);
	}

	public T CreateAreaFeature<T>([NotNull] string featureName) where T : AreaFeature
	{
		return Activator.CreateInstance(Type.GetType("Locations.Area_Features." + featureName + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")) as T;
	}

	public T CreateRegionFeature<T>([NotNull] string featureName) where T : RegionFeature
	{
		return Activator.CreateInstance(Type.GetType("Locations.Region_Features." + featureName + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")) as T;
	}

	public SettlementType CreateSettlementType(SETTLEMENT_TYPE settlementType)
	{
		string text = "Locations.Settlements.Settlement_Types." + settlementType.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		Type type = Type.GetType(text);
		if (type != null)
		{
			return Activator.CreateInstance(type) as SettlementType;
		}
		throw new Exception(text + " has no data!");
	}

	public SettlementType CreateSettlementType(SaveDataSettlementType saveData)
	{
		string text = "Locations.Settlements.Settlement_Types." + saveData.settlementType.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		Type type = Type.GetType(text);
		if (type != null)
		{
			return Activator.CreateInstance(type, saveData) as SettlementType;
		}
		throw new Exception(text + " has no data!");
	}

	public StructureData GetStructureData(STRUCTURE_TYPE p_structureType)
	{
		if (structureData.ContainsKey(p_structureType))
		{
			return structureData[p_structureType];
		}
		return null;
	}

	public WildernessMonsterSpawnerData GetRandomWildernessMonsterSpawnerData(BIOMES p_biomeType)
	{
		WildernessMonsterSpawnerData result = null;
		if (wildernessMonsterSpawnerData.ContainsKey(p_biomeType))
		{
			WildernessMonsterSpawnerData[] array = wildernessMonsterSpawnerData[p_biomeType];
			result = array[GameUtilities.RandomBetweenTwoNumbers(0, array.Length - 1)];
		}
		return result;
	}

	public WildernessMonsterSpawnerData GetWildernessMonsterSpawnerDataByMonsterType(SUMMON_TYPE p_monsterType)
	{
		WildernessMonsterSpawnerData result = null;
		if (this.wildernessMonsterSpawnerData != null)
		{
			foreach (WildernessMonsterSpawnerData[] value in this.wildernessMonsterSpawnerData.Values)
			{
				foreach (WildernessMonsterSpawnerData wildernessMonsterSpawnerData in value)
				{
					if (wildernessMonsterSpawnerData.monsterType == p_monsterType)
					{
						result = wildernessMonsterSpawnerData;
						break;
					}
				}
			}
		}
		return result;
	}

	public void AddStructureToBeCleanedUp(LocationStructure p_structure)
	{
		if (!_structuresToCleanUp.Contains(p_structure))
		{
			_structuresToCleanUp.Add(p_structure);
		}
	}

	public void ProcessStructuresMarkedForCleanUp()
	{
		for (int i = 0; i < _structuresToCleanUp.Count; i++)
		{
			_structuresToCleanUp[i].CleanUp();
		}
		_structuresToCleanUp.Clear();
	}
}
