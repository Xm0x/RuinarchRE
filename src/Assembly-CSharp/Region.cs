using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Grid_Tile_Features;
using Inner_Maps.Location_Structures;
using Locations.Region_Components;
using Locations.Region_Features;
using Locations.Settlements;
using Logs;
using UnityEngine;
using UtilityScripts;

public class Region : ISavable, ILogFiller
{
	private RegionInnerTileMap _regionInnerTileMap;

	private string _activeEventAfterEffectScheduleId;

	private string _uiString;

	public string persistentID { get; }

	public int id { get; }

	public string name { get; private set; }

	public List<Area> areas { get; private set; }

	public Area coreTile { get; private set; }

	public Color regionColor { get; }

	public List<Faction> factionsHere { get; private set; }

	public List<Character> residents { get; private set; }

	public List<Character> charactersAtLocation { get; private set; }

	public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; private set; }

	public List<LocationStructure> allStructures { get; private set; }

	public List<LocationStructure> allSpecialStructures { get; private set; }

	public List<BaseSettlement> settlementsInRegion { get; private set; }

	public BiomeDivisionComponent biomeDivisionComponent { get; }

	public GridTileFeatureComponent gridTileFeatureComponent { get; }

	public RegionSpellsComponent regionSpellsComponent { get; }

	public RegionTileObjectsComponent tileObjectsComponent { get; }

	public LocationStructure wilderness { get; private set; }

	public List<VillageSpot> villageSpots { get; private set; }

	public InnerTileMap innerMap => _regionInnerTileMap;

	public OBJECT_TYPE objectType => OBJECT_TYPE.Region;

	public Type serializedData => typeof(SaveDataRegion);

	public string uiString => GetUIString();

	public Area[,] areaMap => GridMap.Instance.map;

	private Region()
	{
		charactersAtLocation = new List<Character>();
		factionsHere = new List<Faction>();
		residents = new List<Character>();
		settlementsInRegion = new List<BaseSettlement>();
		villageSpots = new List<VillageSpot>();
		regionSpellsComponent = new RegionSpellsComponent();
		tileObjectsComponent = new RegionTileObjectsComponent();
	}

	public Region(Area coreTile, string p_name = "")
		: this()
	{
		persistentID = Guid.NewGuid().ToString();
		id = Utilities.SetID(this);
		name = (string.IsNullOrEmpty(p_name) ? RandomNameGenerator.GetRegionName() : p_name);
		this.coreTile = coreTile;
		areas = new List<Area>();
		AddTile(coreTile);
		regionColor = GenerateRandomRegionColor();
		biomeDivisionComponent = new BiomeDivisionComponent();
		gridTileFeatureComponent = new GridTileFeatureComponent();
		gridTileFeatureComponent.Initialize();
		tileObjectsComponent = new RegionTileObjectsComponent();
	}

	public Region(SaveDataRegion data)
		: this()
	{
		persistentID = data.persistentID;
		id = Utilities.SetID(this, data.id);
		name = data.name;
		areas = new List<Area>();
		regionColor = data.regionColor;
		biomeDivisionComponent = data.regionDivisionComponent.Load();
		gridTileFeatureComponent = data.gridTileFeatureComponent.Load();
		tileObjectsComponent = data.regionTileObjectsComponent.Load();
	}

	public void LoadWilderness(Wilderness p_wilderness)
	{
		wilderness = p_wilderness;
	}

	public void LoadReferences(SaveDataRegion saveDataRegion)
	{
		for (int i = 0; i < saveDataRegion.residentIDs.Length; i++)
		{
			string text = saveDataRegion.residentIDs[i];
			Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(text);
			if (characterByPersistentID != null)
			{
				residents.Add(characterByPersistentID);
			}
		}
		for (int j = 0; j < saveDataRegion.charactersAtLocationIDs.Length; j++)
		{
			string text2 = saveDataRegion.charactersAtLocationIDs[j];
			Character characterByPersistentID2 = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(text2);
			if (characterByPersistentID2 != null)
			{
				charactersAtLocation.Add(characterByPersistentID2);
			}
		}
		for (int k = 0; k < saveDataRegion.factionsHereIDs.Length; k++)
		{
			string text3 = saveDataRegion.factionsHereIDs[k];
			Faction factionBasedOnPersistentID = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(text3);
			factionsHere.Add(factionBasedOnPersistentID);
		}
		for (int l = 0; l < saveDataRegion.villageSpots.Length; l++)
		{
			VillageSpot item = saveDataRegion.villageSpots[l].Load();
			villageSpots.Add(item);
		}
	}

	public void LoadReferencesMainThread(SaveDataRegion saveDataRegion)
	{
		coreTile = GridMap.Instance.allAreas[saveDataRegion.coreTileID];
		gridTileFeatureComponent.LoadReferences(saveDataRegion.gridTileFeatureComponent);
	}

	public void AddTile(Area tile)
	{
		if (!areas.Contains(tile))
		{
			areas.Add(tile);
			tile.SetRegion(this);
		}
	}

	private Color GenerateRandomRegionColor()
	{
		if (id == 1)
		{
			return Color.cyan;
		}
		if (id == 2)
		{
			return Color.yellow;
		}
		if (id == 3)
		{
			return Color.green;
		}
		if (id == 4)
		{
			return Color.red;
		}
		if (id == 5)
		{
			return Color.magenta;
		}
		return UnityEngine.Random.ColorHSV();
	}

	public Area GetAreaThatIsNearbyWithFeatureThatIsNearestTo(string featureName, Character p_character)
	{
		List<Area> list = RuinarchListPool<Area>.Claim();
		p_character.areaLocation?.PopulateAreasInRange(list, 6, includeCenterTile: true);
		float num = 0f;
		Area area = null;
		for (int i = 0; i < list.Count; i++)
		{
			Area area2 = list[i];
			if (area2.featureComponent.HasFeature(featureName))
			{
				float num2 = Vector2.Distance(area2.gridTileComponent.centerGridTile.centeredWorldLocation, p_character.worldPosition);
				if (area == null || num2 < num)
				{
					area = area2;
					num = num2;
				}
			}
		}
		RuinarchListPool<Area>.Release(list);
		return area;
	}

	public void PopulateAreasOccupiedByVillagers(List<Area> areas)
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (character.isNormalCharacter && character.HasTerritory() && !areas.Contains(character.territory))
			{
				areas.Add(character.territory);
			}
		}
	}

	public void AddCharacterToLocation(Character character, LocationGridTile tileOverride = null, bool isInitial = false)
	{
		character.SetRegionLocation(this);
		if (!charactersAtLocation.Contains(character))
		{
			charactersAtLocation.Add(character);
			Messenger.Broadcast(RegionSignals.CHARACTER_ENTERED_REGION, character, this);
		}
	}

	public void RemoveCharacterFromLocation(Character character)
	{
		if (charactersAtLocation.Remove(character))
		{
			character.currentStructure?.RemoveCharacterAtLocation(character);
			character.SetRegionLocation(null);
			Messenger.Broadcast(RegionSignals.CHARACTER_EXITED_REGION, character, this);
		}
	}

	public bool IsResident(Character character)
	{
		return residents.Contains(character);
	}

	public bool AddResident(Character character)
	{
		if (!residents.Contains(character))
		{
			residents.Add(character);
			character.SetHomeRegion(this);
		}
		return false;
	}

	public void RemoveResident(Character character)
	{
		if (residents.Remove(character))
		{
			character.SetHomeRegion(null);
		}
	}

	public bool HasAliveCharacterWithSameTerritoryAndMonsterTypeIs(Character character, SUMMON_TYPE summonType)
	{
		if (character.HasTerritory())
		{
			for (int i = 0; i < residents.Count; i++)
			{
				Character character2 = residents[i];
				if (character2 != character && !character2.isDead && character2.HasTerritory() && character2 is Summon summon && summon.summonType == summonType && character2.IsTerritory(character.territory))
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetCountOfAliveCharacterWithSameTerritory(Character character)
	{
		int num = 0;
		if (character.HasTerritory())
		{
			for (int i = 0; i < residents.Count; i++)
			{
				Character character2 = residents[i];
				if (character2 != character && !character2.isDead && character2.HasTerritory() && character2.IsTerritory(character.territory))
				{
					num++;
				}
			}
		}
		return num;
	}

	public int GetCountOfAliveCharacterWithSameTerritoryAndRace(Character character)
	{
		int num = 0;
		if (character.HasTerritory())
		{
			for (int i = 0; i < residents.Count; i++)
			{
				Character character2 = residents[i];
				if (character2 != character && !character2.isDead && character2.HasTerritory() && character.race == character2.race && character2.IsTerritory(character.territory))
				{
					num++;
				}
			}
		}
		return num;
	}

	public int GetCountOfAliveCharacterForSpiderEggHatching(Area p_area)
	{
		int num = 0;
		if (HasResidentThatIsAliveAndMonsterTypeIs(SUMMON_TYPE.Broodmother))
		{
			for (int i = 0; i < residents.Count; i++)
			{
				Character character = residents[i];
				if (character.IsTerritory(p_area) && !character.isDead && (character is SmallSpider || character is GiantSpider))
				{
					num++;
				}
			}
		}
		else
		{
			for (int j = 0; j < residents.Count; j++)
			{
				Character character2 = residents[j];
				if (!character2.isDead && character2.IsTerritory(p_area))
				{
					num++;
				}
			}
		}
		return num;
	}

	public Character GetRandomCharacterWithPathAndFaction(Character source)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character result = null;
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (source != character && source.movementComponent.HasPathTo(character.gridTileLocation) && !character.isDead && character.faction == source.faction)
			{
				list.Add(character);
			}
		}
		if (list != null)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterForSuccubusMakeLove(Character p_succubus)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character result = null;
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gender == GENDER.MALE && !character.isDead && ((p_succubus.tileObjectComponent.primaryBed != null && p_succubus.tileObjectComponent.primaryBed.gridTileLocation != null) || (character.tileObjectComponent.primaryBed != null && character.tileObjectComponent.primaryBed.gridTileLocation != null)) && character.homeSettlement != null && !character.partyComponent.isActiveMember && character.limiterComponent.canPerform && !character.combatComponent.isInCombat && !character.hasBeenRaisedFromDead && !character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld && character.currentRegion == p_succubus.currentRegion && character.homeSettlement.GetFirstBuiltBedThatIsAvailableAndNoActiveUsers() != null)
			{
				list.Add(character);
			}
		}
		if (list != null)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterForIncubusMakeLove(Character p_succubus)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character result = null;
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gender == GENDER.FEMALE && !character.isDead && ((p_succubus.tileObjectComponent.primaryBed != null && p_succubus.tileObjectComponent.primaryBed.gridTileLocation != null) || (character.tileObjectComponent.primaryBed != null && character.tileObjectComponent.primaryBed.gridTileLocation != null)) && character.homeSettlement != null && !character.partyComponent.isActiveMember && character.limiterComponent.canPerform && !character.combatComponent.isInCombat && !character.hasBeenRaisedFromDead && !character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld && character.currentRegion == p_succubus.currentRegion && character.homeSettlement.GetFirstBuiltBedThatIsAvailableAndNoActiveUsers() != null)
			{
				list.Add(character);
			}
		}
		if (list != null)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterForSuccubusMakeLoveTamed(Character p_succubus)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character result = null;
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gender == GENDER.MALE && !character.isDead && ((p_succubus.tileObjectComponent.primaryBed != null && p_succubus.tileObjectComponent.primaryBed.gridTileLocation != null) || (character.tileObjectComponent.primaryBed != null && character.tileObjectComponent.primaryBed.gridTileLocation != null)) && character.homeSettlement != null && !character.partyComponent.isActiveMember && character.faction != p_succubus.faction && character.homeRegion == p_succubus.currentRegion && character.limiterComponent.canPerform && !character.combatComponent.isInCombat && !character.hasBeenRaisedFromDead && !character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld && character.currentRegion == p_succubus.currentRegion && character.homeSettlement.GetFirstBuiltBedThatIsAvailableAndNoActiveUsers() != null)
			{
				list.Add(character);
			}
		}
		if (list != null)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterForIncubusMakeLoveTamed(Character p_succubus)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character result = null;
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gender == GENDER.FEMALE && !character.isDead && ((p_succubus.tileObjectComponent.primaryBed != null && p_succubus.tileObjectComponent.primaryBed.gridTileLocation != null) || (character.tileObjectComponent.primaryBed != null && character.tileObjectComponent.primaryBed.gridTileLocation != null)) && character.homeSettlement != null && !character.partyComponent.isActiveMember && character.faction != p_succubus.faction && character.homeRegion == p_succubus.currentRegion && character.limiterComponent.canPerform && !character.combatComponent.isInCombat && !character.hasBeenRaisedFromDead && !character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld && character.currentRegion == p_succubus.currentRegion && character.homeSettlement.GetFirstBuiltBedThatIsAvailableAndNoActiveUsers() != null)
			{
				list.Add(character);
			}
		}
		if (list != null)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterThatIsThisGenderVillagerAndNotDead(GENDER p_gender)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character result = null;
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (!character.isDead && character.isNormalCharacter && character.gender == p_gender && character.race != RACE.RATMAN)
			{
				list.Add(character);
			}
		}
		if (list != null)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterThatIsMaleVillagerAndNotDeadAndFactionIsNotTheSameAs(Character p_character)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character result = null;
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character.gender == GENDER.MALE && character.isNormalCharacter && !character.isDead && character.homeRegion == p_character.currentRegion && character.faction != p_character.faction)
			{
				list.Add(character);
			}
		}
		if (list != null)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterThatIsNotDeadAndInDemonFactionAndHasPathTo(Character p_character)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character result = null;
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (character != p_character && !character.isDead && character.movementComponent.HasPathTo(p_character.gridTileLocation))
			{
				Faction faction = character.faction;
				if (faction != null && faction.factionType.type == FACTION_TYPE.Demons)
				{
					list.Add(character);
				}
			}
		}
		if (list != null)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterForMonsterScent(Character p_character)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character result = null;
		for (int i = 0; i < charactersAtLocation.Count; i++)
		{
			Character character = charactersAtLocation[i];
			if (!character.isDead && character.limiterComponent.canPerform && character.limiterComponent.canMove && character.movementComponent.HasPathTo(p_character.gridTileLocation) && !character.movementComponent.isStationary && character is Summon && !(character is Animal) && !character.isInLimbo && !CharacterManager.Instance.IsCharacterTheSameLycan(p_character, character) && !character.partyComponent.hasParty && !PlayerManager.Instance.player.retaliationComponent.HasRetaliator(character))
			{
				list.Add(character);
			}
		}
		if (list != null)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public bool HasResidentThatIsAliveAndMonsterTypeIs(SUMMON_TYPE p_summonType)
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (!character.isDead && character is Summon summon && summon.summonType == p_summonType)
			{
				return true;
			}
		}
		return false;
	}

	public void AddFactionHere(Faction faction)
	{
		if (!IsFactionHere(faction) && faction.isMajorFaction)
		{
			factionsHere.Add(faction);
		}
	}

	public void RemoveFactionHere(Faction faction)
	{
		factionsHere.Remove(faction);
	}

	public bool IsFactionHere(Faction faction)
	{
		return factionsHere.Contains(faction);
	}

	public void CreateStructureList()
	{
		structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
		allStructures = new List<LocationStructure>();
		allSpecialStructures = new List<LocationStructure>();
	}

	public void GenerateStructures()
	{
		CreateStructureList();
		wilderness = LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WILDERNESS);
	}

	public void AddStructure(LocationStructure structure)
	{
		if (!structures.ContainsKey(structure.structureType))
		{
			structures.Add(structure.structureType, new List<LocationStructure>());
		}
		if (!structures[structure.structureType].Contains(structure))
		{
			structures[structure.structureType].Add(structure);
			allStructures.Add(structure);
			if (structure.structureType.IsSpecialStructure())
			{
				allSpecialStructures.Add(structure);
			}
		}
	}

	public void RemoveStructure(LocationStructure structure)
	{
		if (structures.ContainsKey(structure.structureType) && structures[structure.structureType].Remove(structure))
		{
			allStructures.Remove(structure);
			if (structures[structure.structureType].Count == 0)
			{
				structures.Remove(structure.structureType);
			}
			if (structure.structureType.IsSpecialStructure())
			{
				allSpecialStructures.Remove(structure);
			}
		}
	}

	public LocationStructure GetRandomStructureOfType(STRUCTURE_TYPE type)
	{
		if (structures.ContainsKey(type))
		{
			return structures[type][Utilities.Rng.Next(0, structures[type].Count)];
		}
		return null;
	}

	public LocationStructure GetStructureOfTypeWithLowestResidentCountOwnedBy(STRUCTURE_TYPE type, Faction p_owner)
	{
		if (structures.ContainsKey(type))
		{
			List<LocationStructure> list = structures[type];
			if (list != null)
			{
				LocationStructure locationStructure = null;
				int num = 0;
				for (int i = 0; i < list.Count; i++)
				{
					LocationStructure locationStructure2 = list[i];
					if (!locationStructure2.hasBeenDestroyed && locationStructure2.settlementLocation != null && locationStructure2.settlementLocation.owner == p_owner && (locationStructure == null || locationStructure2.residents.Count < num))
					{
						locationStructure = locationStructure2;
						num = locationStructure2.residents.Count;
					}
				}
				return locationStructure;
			}
		}
		return null;
	}

	public LocationStructure GetStructureOfTypeWithNoResidentAndIsUnowned(STRUCTURE_TYPE type)
	{
		if (structures.ContainsKey(type))
		{
			List<LocationStructure> list = structures[type];
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					LocationStructure locationStructure = list[i];
					if (!locationStructure.hasBeenDestroyed && (locationStructure.settlementLocation == null || locationStructure.settlementLocation.owner == null) && locationStructure.residents.Count <= 0)
					{
						return locationStructure;
					}
				}
			}
		}
		return null;
	}

	public LocationStructure GetRandomStructure()
	{
		LocationStructure locationStructure = null;
		while (locationStructure == null)
		{
			KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair = structures.ElementAt(UnityEngine.Random.Range(0, structures.Count));
			if (keyValuePair.Key != STRUCTURE_TYPE.CAVE && keyValuePair.Key != STRUCTURE_TYPE.OCEAN && keyValuePair.Value.Count > 0)
			{
				locationStructure = keyValuePair.Value[UnityEngine.Random.Range(0, keyValuePair.Value.Count)];
			}
		}
		return locationStructure;
	}

	public LocationStructure GetRandomStructureThatIsInADungeonAndHasPassableTiles()
	{
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		LocationStructure result = null;
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			if (locationStructure.settlementLocation != null && locationStructure.settlementLocation.locationType == LOCATION_TYPE.DUNGEON && locationStructure.passableTiles.Count > 0)
			{
				list.Add(locationStructure);
			}
		}
		if (list != null && list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	public LocationStructure GetRandomStructureThatIsInAnUnoccupiedDungeonAndHasPassableTiles()
	{
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		LocationStructure result = null;
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			if (!locationStructure.IsOccupied() && locationStructure.settlementLocation != null && locationStructure.settlementLocation.locationType == LOCATION_TYPE.DUNGEON && locationStructure.passableTiles.Count > 0 && !(locationStructure is AnimalDen))
			{
				list.Add(locationStructure);
			}
		}
		if (list != null && list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	public LocationStructure GetRandomStructureThatIsHabitableAndUnoccupiedButNot(LocationStructure p_exceptionStructure)
	{
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		LocationStructure result = null;
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			if (!locationStructure.IsOccupied() && locationStructure.HasStructureTag(STRUCTURE_TAG.Shelter) && p_exceptionStructure != locationStructure)
			{
				list.Add(locationStructure);
			}
		}
		if (list != null && list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	public LocationStructure GetRandomStructureOfTypeThatHasTombstone(STRUCTURE_TYPE type)
	{
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		LocationStructure result = null;
		if (structures.ContainsKey(type))
		{
			List<LocationStructure> list2 = structures[type];
			for (int i = 0; i < list2.Count; i++)
			{
				LocationStructure locationStructure = list2[i];
				if (locationStructure.HasTileObjectOfType(TILE_OBJECT_TYPE.TOMBSTONE))
				{
					list.Add(locationStructure);
				}
			}
		}
		if (list != null && list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	public LocationStructure GetRandomSpecialStructure()
	{
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		LocationStructure result = null;
		for (int i = 0; i < allSpecialStructures.Count; i++)
		{
			LocationStructure locationStructure = allSpecialStructures[i];
			if (locationStructure.passableTiles.Count > 0)
			{
				list.Add(locationStructure);
			}
		}
		if (list != null && list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	public LocationStructure GetFirstStructureOfTypeNotExceedingOrEqualToAliveResidentLimit(STRUCTURE_TYPE type, int aliveResidentLimitCount)
	{
		if (structures.ContainsKey(type))
		{
			List<LocationStructure> list = structures[type];
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					LocationStructure locationStructure = list[i];
					if (locationStructure.GetNumberOfResidentsThatIsAlive() < aliveResidentLimitCount)
					{
						return locationStructure;
					}
				}
			}
		}
		return null;
	}

	public LocationStructure GetFirstUnoccupiedOrOccupiedByFactionStructureOfType(STRUCTURE_TYPE type, Faction faction)
	{
		if (structures.ContainsKey(type))
		{
			List<LocationStructure> list = structures[type];
			for (int i = 0; i < list.Count; i++)
			{
				LocationStructure locationStructure = list[i];
				if (!locationStructure.IsOccupied())
				{
					return locationStructure;
				}
				if (faction != null && locationStructure.residents[0].faction == faction)
				{
					return locationStructure;
				}
			}
		}
		return null;
	}

	public LocationStructure GetStructureOfTypeWithoutSettlement(STRUCTURE_TYPE type)
	{
		if (structures.ContainsKey(type))
		{
			List<LocationStructure> list = structures[type];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].settlementLocation == null)
				{
					return list[i];
				}
			}
		}
		return null;
	}

	public LocationStructure GetStructureByID(STRUCTURE_TYPE type, int id)
	{
		if (structures.ContainsKey(type))
		{
			List<LocationStructure> list = structures[type];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].id == id)
				{
					return list[i];
				}
			}
		}
		return null;
	}

	public List<LocationStructure> GetStructuresAtLocation(STRUCTURE_TYPE type)
	{
		List<LocationStructure> result = null;
		if (structures.ContainsKey(type))
		{
			result = structures[type];
		}
		return result;
	}

	public bool HasStructure(STRUCTURE_TYPE type)
	{
		return structures.ContainsKey(type);
	}

	public void PopulateForageStructuresForVagrantEating(List<LocationStructure> structures, Character p_vagrant)
	{
		Area areaLocation = p_vagrant.areaLocation;
		if (areaLocation == null)
		{
			return;
		}
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			if (locationStructure.structureType.IsForageStructure() && locationStructure.occupiedArea != null && locationStructure.occupiedArea.IsNearbyTo(areaLocation))
			{
				structures.Add(locationStructure);
			}
		}
	}

	public bool HasStructureBlueprint(STRUCTURE_TYPE p_type)
	{
		for (int i = 0; i < settlementsInRegion.Count; i++)
		{
			if (!(settlementsInRegion[i] is NPCSettlement nPCSettlement))
			{
				continue;
			}
			for (int j = 0; j < nPCSettlement.availableJobs.Count; j++)
			{
				if (nPCSettlement.availableJobs[j] is GoapPlanJob { jobType: JOB_TYPE.BUILD_BLUEPRINT, poiTarget: GenericTileObject poiTarget } && poiTarget.blueprintOnTile != null && poiTarget.blueprintOnTile.structureType == p_type)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void PopulateNearbyStructuresFrom(List<LocationStructure> structures, Character p_character, STRUCTURE_TYPE p_structureType1, STRUCTURE_TYPE p_structureType2)
	{
		Area areaLocation = p_character.areaLocation;
		if (areaLocation == null)
		{
			return;
		}
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			if ((locationStructure.structureType == p_structureType1 || locationStructure.structureType == p_structureType2) && !locationStructure.hasBeenDestroyed && locationStructure.occupiedArea != null && locationStructure.occupiedArea.IsNearbyTo(areaLocation))
			{
				structures.Add(locationStructure);
			}
		}
	}

	public LocationStructure GetFirstNearbyStructureFrom(Character p_character, STRUCTURE_TYPE p_structureType)
	{
		Area areaLocation = p_character.areaLocation;
		if (areaLocation != null)
		{
			for (int i = 0; i < allStructures.Count; i++)
			{
				LocationStructure locationStructure = allStructures[i];
				if (locationStructure.structureType == p_structureType && !locationStructure.hasBeenDestroyed && locationStructure.occupiedArea != null && locationStructure.occupiedArea.IsNearbyTo(areaLocation))
				{
					return locationStructure;
				}
			}
		}
		return null;
	}

	public LocationStructure GetFirstNearbyMageTowerFromCharacterWithNoResidentThatIsNotTargetOfDevastationRitual(Character p_character)
	{
		Area areaLocation = p_character.areaLocation;
		if (areaLocation != null)
		{
			for (int i = 0; i < allStructures.Count; i++)
			{
				LocationStructure locationStructure = allStructures[i];
				if (locationStructure.structureType != STRUCTURE_TYPE.MAGE_TOWER || locationStructure.hasBeenDestroyed || locationStructure.IsOccupied() || locationStructure.occupiedArea == null || !locationStructure.occupiedArea.IsNearbyTo(areaLocation))
				{
					continue;
				}
				bool flag = false;
				List<TileObject> tileObjectsOfType = locationStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.MAGIC_CIRCLE);
				if (tileObjectsOfType != null && tileObjectsOfType.Count > 0)
				{
					for (int j = 0; j < tileObjectsOfType.Count; j++)
					{
						if (tileObjectsOfType[j].HasJobTargetingThis(JOB_TYPE.DEVASTATION_RITUAL))
						{
							flag = true;
						}
					}
				}
				if (!flag)
				{
					return locationStructure;
				}
			}
		}
		return null;
	}

	public LocationStructure GetFirstNearbyMageTowerFromCharacterThatIsNotTargetOfDevastationRitual(Character p_character)
	{
		Area areaLocation = p_character.areaLocation;
		if (areaLocation != null)
		{
			for (int i = 0; i < allStructures.Count; i++)
			{
				LocationStructure locationStructure = allStructures[i];
				if (locationStructure.structureType != STRUCTURE_TYPE.MAGE_TOWER || locationStructure.hasBeenDestroyed || locationStructure.occupiedArea == null || !locationStructure.occupiedArea.IsNearbyTo(areaLocation))
				{
					continue;
				}
				bool flag = false;
				List<TileObject> tileObjectsOfType = locationStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.MAGIC_CIRCLE);
				if (tileObjectsOfType != null && tileObjectsOfType.Count > 0)
				{
					for (int j = 0; j < tileObjectsOfType.Count; j++)
					{
						if (tileObjectsOfType[j].HasJobTargetingThis(JOB_TYPE.DEVASTATION_RITUAL))
						{
							flag = true;
						}
					}
				}
				if (!flag)
				{
					return locationStructure;
				}
			}
		}
		return null;
	}

	public void SetRegionInnerMap(RegionInnerTileMap regionInnerTileMap)
	{
		_regionInnerTileMap = regionInnerTileMap;
	}

	public bool IsRequiredByLocation(TileObject item)
	{
		return false;
	}

	public void PopulateTileObjectsOfType(List<TileObject> p_tileObjects, TILE_OBJECT_TYPE type)
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			List<TileObject> tileObjectsOfType = allStructures[i].GetTileObjectsOfType(type);
			if (tileObjectsOfType != null && tileObjectsOfType.Count > 0)
			{
				p_tileObjects.AddRange(tileObjectsOfType);
			}
		}
	}

	public void PopulateBuiltTileObjectsOfTypeWithAreaDistanceFrom(List<TileObject> p_tileObjects, TILE_OBJECT_TYPE type, Area p_sourceArea, int p_distanceLimit)
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			List<TileObject> tileObjectsOfType = allStructures[i].GetTileObjectsOfType(type);
			if (tileObjectsOfType == null || tileObjectsOfType.Count <= 0)
			{
				continue;
			}
			for (int j = 0; j < tileObjectsOfType.Count; j++)
			{
				TileObject tileObject = tileObjectsOfType[j];
				LocationGridTile gridTileLocation = tileObject.gridTileLocation;
				if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT && gridTileLocation != null && gridTileLocation.area.GetAreaDistanceTo(p_sourceArea) <= p_distanceLimit)
				{
					p_tileObjects.Add(tileObject);
				}
			}
		}
	}

	public bool HasTileObjectOfType(TILE_OBJECT_TYPE type)
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			if (allStructures[i].HasTileObjectOfType(type))
			{
				return true;
			}
		}
		return false;
	}

	public void PopulateTileObjectsOfType<T>(List<TileObject> objs) where T : TileObject
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			allStructures[i].PopulateTileObjectsOfType<T>(objs);
		}
	}

	public void LinkAllUnlinkedSpecialStructures()
	{
		for (int i = 0; i < allSpecialStructures.Count; i++)
		{
			LocationStructure locationStructure = allSpecialStructures[i];
			if (locationStructure.linkedSettlement == null)
			{
				locationStructure.LinkThisStructureToAVillage();
			}
		}
	}

	public Area GetRandomNearbyAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage(Character relativeTo)
	{
		Area areaLocation = relativeTo.areaLocation;
		Area result = null;
		if (areaLocation != null)
		{
			List<Area> list = RuinarchListPool<Area>.Claim();
			for (int i = 0; i < areas.Count; i++)
			{
				Area area = areas[i];
				if (area.elevationType != ELEVATION.WATER && area.elevationType != ELEVATION.MOUNTAIN && !area.structureComponent.HasStructureInArea() && !area.IsNextToOrPartOfVillage() && !area.gridTileComponent.HasCorruption() && areaLocation.IsNearbyTo(area))
				{
					list.Add(area);
				}
			}
			if (list != null && list.Count > 0)
			{
				result = CollectionUtilities.GetRandomElement(list);
			}
			RuinarchListPool<Area>.Release(list);
		}
		return result;
	}

	public Area GetRandomNearbyAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption(Character relativeTo)
	{
		Area areaLocation = relativeTo.areaLocation;
		Area result = null;
		if (areaLocation != null)
		{
			List<Area> list = RuinarchListPool<Area>.Claim();
			for (int i = 0; i < areas.Count; i++)
			{
				Area area = areas[i];
				if (area.elevationType != ELEVATION.WATER && area.elevationType != ELEVATION.MOUNTAIN && !area.structureComponent.HasStructureInArea() && !area.gridTileComponent.HasCorruption() && areaLocation.IsNearbyTo(area))
				{
					list.Add(area);
				}
			}
			if (list != null && list.Count > 0)
			{
				result = CollectionUtilities.GetRandomElement(list);
			}
			RuinarchListPool<Area>.Release(list);
		}
		return result;
	}

	public Area GetRandomAreaThatIsNotMountainWaterAndNoCorruption()
	{
		List<Area> list = RuinarchListPool<Area>.Claim();
		Area result = null;
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			if (area.elevationType != ELEVATION.WATER && area.elevationType != ELEVATION.MOUNTAIN && !area.gridTileComponent.HasCorruption())
			{
				list.Add(area);
			}
		}
		if (list != null && list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Area>.Release(list);
		return result;
	}

	public Area GetRandomAreaThatIsUncorruptedFullyPlainNoStructureAndNotNextToOrPartOfVillage()
	{
		List<Area> list = RuinarchListPool<Area>.Claim();
		Area result = null;
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			if (area.elevationComponent.IsFully(ELEVATION.PLAIN) && !area.structureComponent.HasStructureInArea() && !area.IsNextToOrPartOfVillage() && !area.gridTileComponent.HasCorruption())
			{
				list.Add(area);
			}
		}
		if (list != null && list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Area>.Release(list);
		return result;
	}

	public Area GetRandomAreaThatIsNotMountainAndWaterAndNoSettlement()
	{
		List<Area> list = RuinarchListPool<Area>.Claim();
		Area result = null;
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			if (!area.HasSettlementOnArea() && area.elevationType != ELEVATION.WATER && area.elevationType != ELEVATION.MOUNTAIN)
			{
				list.Add(area);
			}
		}
		if (list != null && list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Area>.Release(list);
		return result;
	}

	public Area GetRandomAreaThatIsNotWater()
	{
		List<Area> list = RuinarchListPool<Area>.Claim();
		Area result = null;
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			if (area.elevationType != ELEVATION.WATER)
			{
				list.Add(area);
			}
		}
		if (list != null && list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Area>.Release(list);
		return result;
	}

	public void PopulateAreasThatAreNextToAVillageButNotMountainAndWaterAndNoSettlementAndWithPathTo(List<Area> p_areas, Character p_character)
	{
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			if (area.elevationType != ELEVATION.MOUNTAIN && area.elevationType != ELEVATION.WATER && area.neighbourComponent.IsNextToVillage() && !area.HasSettlementOnArea() && p_character.movementComponent.HasPathTo(area))
			{
				p_areas.Add(area);
			}
		}
	}

	public Area GetRandomAreaThatIsInWilderness()
	{
		List<Area> list = RuinarchListPool<Area>.Claim();
		Area result = null;
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			if (area.primaryStructureInArea.structureType == STRUCTURE_TYPE.WILDERNESS)
			{
				list.Add(area);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Area>.Release(list);
		return result;
	}

	public void UpdateSettlementsInRegion()
	{
		settlementsInRegion.Clear();
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			for (int j = 0; j < area.settlementsOnArea.Count; j++)
			{
				BaseSettlement item = area.settlementsOnArea[j];
				if (!settlementsInRegion.Contains(item))
				{
					settlementsInRegion.Add(item);
				}
			}
		}
	}

	public bool AddSettlementInRegion(BaseSettlement p_settlement)
	{
		if (!settlementsInRegion.Contains(p_settlement))
		{
			settlementsInRegion.Add(p_settlement);
			return true;
		}
		return false;
	}

	public bool RemoveSettlementFromRegion(BaseSettlement p_settlement)
	{
		return settlementsInRegion.Remove(p_settlement);
	}

	public void PopulateSettlementsInRegionForInvadeBehaviour(List<BaseSettlement> settlements)
	{
		for (int i = 0; i < settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement = settlementsInRegion[i];
			if (baseSettlement.HasResidentForInvadeBehaviour())
			{
				settlements.Add(baseSettlement);
			}
		}
	}

	public void PopulateSettlementsInRegionForPestBehaviour(List<BaseSettlement> settlements, Character p_character)
	{
		for (int i = 0; i < settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement = settlementsInRegion[i];
			if (baseSettlement.HasResidentThatIsNotDead(p_character))
			{
				settlements.Add(baseSettlement);
			}
		}
	}

	public void PopulateSettlementsInRegionForGettingGeneralVillageTargets(List<BaseSettlement> settlements)
	{
		for (int i = 0; i < settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement = settlementsInRegion[i];
			if (baseSettlement.HasResidentForGettingGeneralVillageTargets())
			{
				settlements.Add(baseSettlement);
			}
		}
	}

	public void PopulateSettlementsInRegionThatHasAliveResidentExcept(List<BaseSettlement> settlements, Character exception, BaseSettlement exceptionSettlement)
	{
		for (int i = 0; i < settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement = settlementsInRegion[i];
			if (baseSettlement != exceptionSettlement && baseSettlement.HasResidentThatIsNotDead(exception))
			{
				settlements.Add(baseSettlement);
			}
		}
	}

	public void PopulateValidVillagesToVisit(Character p_character, List<NPCSettlement> settlements, Faction p_faction)
	{
		for (int i = 0; i < settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement = settlementsInRegion[i];
			if (baseSettlement.owner == null || baseSettlement == p_character.homeSettlement || baseSettlement.locationType != LOCATION_TYPE.VILLAGE || !(baseSettlement is NPCSettlement item) || (baseSettlement.owner != p_faction && baseSettlement.owner.IsHostileWith(p_faction) && !(p_character.characterClass.className == "Noble")) || p_character.crimeComponent.IsWantedBy(baseSettlement.owner) || (baseSettlement.owner.factionType.type == FACTION_TYPE.Demon_Cult && p_character.faction != null && p_character.faction.factionType.GetCrimeSeverity(CRIME_TYPE.Demon_Worship) == CRIME_SEVERITY.Heinous))
			{
				continue;
			}
			Area areaLocation = p_character.areaLocation;
			if (areaLocation != null)
			{
				LocationStructure firstStructureOfType = baseSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
				if (areaLocation.GetAreaDistanceTo(firstStructureOfType.occupiedArea) <= 10)
				{
					settlements.Add(item);
				}
			}
		}
	}

	public void PopulateValidBanditCampsToVisit(Character p_character, List<LocationStructure> p_banditCamps)
	{
		List<LocationStructure> list = structures[STRUCTURE_TYPE.BANDIT_CAMP];
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			LocationStructure locationStructure = list[i];
			if (p_character.homeStructure != locationStructure && locationStructure.settlementLocation != null && locationStructure.settlementLocation.owner != null && locationStructure.settlementLocation.owner.factionType.type == FACTION_TYPE.Bandits)
			{
				Area areaLocation = p_character.areaLocation;
				if (areaLocation != null && areaLocation.GetAreaDistanceTo(locationStructure.occupiedArea) <= 10)
				{
					p_banditCamps.Add(locationStructure);
				}
			}
		}
	}

	public BaseSettlement GetFirstSettlementInRegionThatIsAUnoccupiedOrFactionlessResidentVillageThatIsNotHomeOf(Character p_character)
	{
		for (int i = 0; i < settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement = settlementsInRegion[i];
			if (baseSettlement.locationType == LOCATION_TYPE.VILLAGE && p_character.previousCharacterDataComponent.previousHomeSettlement != baseSettlement && p_character.homeSettlement != baseSettlement && (!baseSettlement.HasResidents() || baseSettlement.AreAllResidentsVagrantOrFactionless()))
			{
				return baseSettlement;
			}
		}
		return null;
	}

	public BaseSettlement GetFirstSettlementInRegionThatIsAUnoccupiedVillageThatIsNotPreviousHomeOf(Character p_character)
	{
		for (int i = 0; i < settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement = settlementsInRegion[i];
			if (baseSettlement.locationType == LOCATION_TYPE.VILLAGE && !baseSettlement.HasResidents() && baseSettlement != p_character.previousCharacterDataComponent.previousHomeSettlement)
			{
				return baseSettlement;
			}
		}
		return null;
	}

	public bool IsRegionVillageCapacityReached()
	{
		int num = 0;
		for (int i = 0; i < settlementsInRegion.Count; i++)
		{
			if (settlementsInRegion[i].locationType == LOCATION_TYPE.VILLAGE)
			{
				num++;
			}
		}
		return num >= villageSpots.Count;
	}

	public void SetVillageSpots(List<VillageSpot> p_villageSpots)
	{
		villageSpots.Clear();
		villageSpots.AddRange(p_villageSpots);
	}

	public VillageSpot GetFirstUnoccupiedVillageSpot()
	{
		for (int i = 0; i < villageSpots.Count; i++)
		{
			VillageSpot villageSpot = villageSpots[i];
			if (!villageSpot.isDisabled)
			{
				return villageSpot;
			}
		}
		return null;
	}

	public VillageSpot GetFirstUnoccupiedVillageSpotThatCanAccomodateFaction(FACTION_TYPE p_factionType)
	{
		for (int i = 0; i < villageSpots.Count; i++)
		{
			VillageSpot villageSpot = villageSpots[i];
			if (!villageSpot.isDisabled && villageSpot.CanAccommodateFaction(p_factionType))
			{
				return villageSpot;
			}
		}
		return null;
	}

	public VillageSpot GetCoreVillageSpotOnArea(Area p_area)
	{
		for (int i = 0; i < villageSpots.Count; i++)
		{
			VillageSpot villageSpot = villageSpots[i];
			if (villageSpot.coreSpot == p_area)
			{
				return villageSpot;
			}
		}
		return null;
	}

	public LocationGridTile GetFirstUnoccupiedGridTileInWilderness()
	{
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			for (int j = 0; j < area.gridTileComponent.gridTiles.Count; j++)
			{
				LocationGridTile locationGridTile = area.gridTileComponent.gridTiles[j];
				if (locationGridTile.tileObjectComponent.objHere == null && !locationGridTile.isOccupied && locationGridTile.structure.structureType == STRUCTURE_TYPE.WILDERNESS)
				{
					return locationGridTile;
				}
			}
		}
		return null;
	}

	private string GetUIString()
	{
		if (string.IsNullOrEmpty(_uiString))
		{
			string text = GetType().ToString() + "|" + persistentID;
			_uiString = "<link=" + text + ">" + name + "</link>";
		}
		return _uiString;
	}

	public void CleanUp()
	{
		areas?.Clear();
		areas = null;
		coreTile = null;
		factionsHere?.Clear();
		factionsHere = null;
		residents?.Clear();
		residents = null;
		charactersAtLocation?.Clear();
		charactersAtLocation = null;
		structures?.Clear();
		structures = null;
		settlementsInRegion?.Clear();
		settlementsInRegion = null;
		villageSpots?.Clear();
		villageSpots = null;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> structure in structures)
		{
			structure.Value.Contains(p_structure);
		}
		allStructures.Contains(p_structure);
		allSpecialStructures.Contains(p_structure);
		_ = wilderness;
		biomeDivisionComponent.CheckIfStructureIsStillReferenced(p_structure);
		gridTileFeatureComponent.CheckIfStructureIsStillReferenced(p_structure);
		regionSpellsComponent.CheckIfStructureIsStillReferenced(p_structure);
		tileObjectsComponent.CheckIfStructureIsStillReferenced(p_structure);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		residents.Contains(p_character);
		charactersAtLocation.Contains(p_character);
		biomeDivisionComponent.CheckIfCharacterIsStillReferenced(p_character);
		gridTileFeatureComponent.CheckIfCharacterIsStillReferenced(p_character);
		regionSpellsComponent.CheckIfCharacterIsStillReferenced(p_character);
		tileObjectsComponent.CheckIfCharacterIsStillReferenced(p_character);
	}
}
