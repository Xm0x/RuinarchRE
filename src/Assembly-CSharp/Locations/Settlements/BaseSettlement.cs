using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Logs;
using Traits;
using UnityEngine;
using UtilityScripts;

namespace Locations.Settlements;

public abstract class BaseSettlement : IPartyQuestTarget, IPartyTargetDestination, IGatheringTarget, ILogFiller, IPlayerActionTarget, ILocation, ISavable, IStoredTarget, IBookmarkable
{
	private string _uiString;

	public string persistentID { get; private set; }

	public int id { get; }

	public LOCATION_TYPE locationType { get; private set; }

	public string name { get; private set; }

	public bool isStoredAsTarget { get; private set; }

	public Faction owner { get; private set; }

	public List<Area> areas { get; }

	public List<Character> residents { get; protected set; }

	public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; protected set; }

	public List<IPointOfInterest> firesToDouseInSettlement { get; }

	public List<LocationStructure> allStructures { get; protected set; }

	public List<Party> parties { get; protected set; }

	public List<PLAYER_SKILL_TYPE> actions { get; private set; }

	public virtual SettlementResources SettlementResources { get; protected set; }

	public string bookmarkName
	{
		get
		{
			if (HasStructure(STRUCTURE_TYPE.HALLOWED_GROUND))
			{
				return iconRichText + " Hallowed Grounds";
			}
			return iconRichText + " " + name;
		}
	}

	public BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Settlement;

	public STORED_TARGET_TYPE storedTargetType => STORED_TARGET_TYPE.Village;

	public bool isTargetted { get; set; }

	public string iconRichText => Utilities.VillageIcon();

	public virtual Type serializedData => typeof(SaveDataBaseSettlement);

	public virtual Region region => null;

	public string locationName => name;

	public LocationStructure currentStructure => null;

	public BaseSettlement currentSettlement => this;

	public bool hasBeenDestroyed => false;

	public PARTY_TARGET_DESTINATION_TYPE partyTargetDestinationType => PARTY_TARGET_DESTINATION_TYPE.Settlement;

	public BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Text_With_Cancel;

	public string uiString => GetUIString();

	protected BaseSettlement(LOCATION_TYPE locationType)
	{
		persistentID = Utilities.GetNewUniqueID();
		id = Utilities.SetID(this);
		SetName(RandomNameGenerator.GenerateSettlementName(RACE.HUMANS));
		areas = new List<Area>();
		residents = new List<Character>();
		structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
		firesToDouseInSettlement = new List<IPointOfInterest>();
		allStructures = new List<LocationStructure>();
		parties = new List<Party>();
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		SetLocationType(locationType);
		StartListeningForFires();
		ConstructDefaultPlayerActions();
	}

	protected BaseSettlement(SaveDataBaseSettlement data)
	{
		persistentID = data._persistentID;
		SetName(data.name);
		id = Utilities.SetID(this, data.id);
		isStoredAsTarget = data.isStoredAsTarget;
		areas = new List<Area>();
		residents = new List<Character>();
		structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
		firesToDouseInSettlement = new List<IPointOfInterest>();
		allStructures = new List<LocationStructure>();
		parties = new List<Party>();
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		SetLocationType(data.locationType);
		ConstructDefaultPlayerActions(broadcastSignal: false);
	}

	private void SetLocationType(LOCATION_TYPE locationType)
	{
		this.locationType = locationType;
	}

	public void SetName(string name)
	{
		this.name = name;
		Messenger.Broadcast(SettlementSignals.SETTLEMENT_CHANGED_NAME, this);
	}

	public virtual bool AddResident(Character character, LocationStructure chosenHome = null, bool ignoreCapacity = true)
	{
		if (!residents.Contains(character))
		{
			if (!ignoreCapacity && IsResidentsFull())
			{
				return false;
			}
			if (!CanCharacterBeAddedAsResidentBasedOnFaction(character))
			{
				return false;
			}
			residents.Add(character);
			AssignCharacterToDwellingInArea(character, chosenHome);
			if (owner == null && character.faction != null && (character.faction.isMajorNonPlayerOrBandits || character.faction.factionType.type == FACTION_TYPE.Ratmen))
			{
				LandmarkManager.Instance.OwnSettlement(character.faction, this);
			}
			return true;
		}
		return false;
	}

	public virtual bool RemoveResident(Character character)
	{
		if (residents.Remove(character))
		{
			if (character.homeStructure != null && character.homeSettlement == this)
			{
				character.ChangeHomeStructure(null);
			}
			if (character.structureComponent.workPlaceStructure != null)
			{
				character.structureComponent.workPlaceStructure.RemoveAssignedWorker(character);
			}
			if (owner != null)
			{
				if (owner.factionType.type == FACTION_TYPE.Ratmen)
				{
					bool flag = false;
					for (int i = 0; i < residents.Count; i++)
					{
						Character character2 = residents[i];
						if (character2.faction != null && character2.faction.factionType.type == FACTION_TYPE.Ratmen)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						LandmarkManager.Instance.UnownSettlement(this);
					}
				}
				else if (residents.Count <= 0)
				{
					LandmarkManager.Instance.UnownSettlement(this);
				}
			}
			return true;
		}
		return false;
	}

	public virtual void AssignCharacterToDwellingInArea(Character character, LocationStructure dwellingOverride = null)
	{
		if (structures != null && dwellingOverride != null)
		{
			character.ChangeHomeStructure(dwellingOverride);
		}
	}

	private bool CanCharacterBeAddedAsResidentBasedOnFaction(Character character)
	{
		if (character.isVagrantOrFactionless || character.isFactionless || (character.faction != null && !character.faction.isMajorFaction))
		{
			_ = owner;
			return true;
		}
		if (character.faction.isPlayerFaction && owner != null && owner.isPlayerFaction)
		{
			return true;
		}
		if (character.faction != null)
		{
			_ = character.faction;
			_ = owner;
			return true;
		}
		return true;
	}

	protected virtual bool IsResidentsFull()
	{
		if (structures.ContainsKey(STRUCTURE_TYPE.DWELLING))
		{
			List<LocationStructure> list = structures[STRUCTURE_TYPE.DWELLING];
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].IsOccupied())
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool HasResidentWithRace(RACE p_race, Character p_exception = null)
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if ((p_exception == null || p_exception != character) && character.race == p_race)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasResidentThatIsNotDead(Character p_exception = null)
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if ((p_exception == null || p_exception != character) && !character.isDead)
			{
				return true;
			}
		}
		return false;
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

	public bool HasResidentThatIsVillagerAndNotDead()
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (!character.isDead && character.isNormalCharacter)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasResidentThatIsSapientAndInsideSettlementOrHasJoinedQuest()
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (character.race.IsSapient() && ((character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(this)) || character.partyComponent.isMemberThatJoinedQuest))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasResidentThatIsAliveAndInsideSettlement()
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (!character.isDead && character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(this))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasResidentForExterminationPartyQuest(BaseSettlement p_settlement, Faction p_faction, Party p_party)
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (!character.isDead && !character.partyComponent.IsAMemberOfParty(p_party) && !character.isBeingSeized && character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(p_settlement) && (character.faction == null || p_faction == null || p_faction.IsHostileWith(character.faction)) && !character.traitContainer.HasTrait("Hibernating"))
			{
				return true;
			}
		}
		return false;
	}

	public Character GetRandomCharacterThatIsVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(Character p_character)
	{
		Character character = null;
		for (int i = 0; i < allStructures.Count; i++)
		{
			character = allStructures[i].GetRandomCharacterThatIsVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(p_character);
			if (character != null)
			{
				return character;
			}
		}
		return character;
	}

	public Character GetRandomCharacterThatIsAliveVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(Character p_character)
	{
		Character character = null;
		for (int i = 0; i < allStructures.Count; i++)
		{
			character = allStructures[i].GetRandomCharacterThatIsAliveVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(p_character);
			if (character != null)
			{
				return character;
			}
		}
		return character;
	}

	public Character GetRandomCharacterThatIsDead()
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character character = null;
		for (int i = 0; i < allStructures.Count; i++)
		{
			character = allStructures[i].GetRandomCharacterThatIsDead();
			if (character != null)
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			character = list[UnityEngine.Random.Range(0, list.Count)];
		}
		RuinarchListPool<Character>.Release(list);
		return character;
	}

	public Character GetRandomResidentForInvasionTargetThatIsInsideSettlement(BaseSettlement p_settlement, Character p_exception = null)
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if ((p_exception == null || p_exception != character) && !character.isDead && !character.isBeingSeized && character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(p_settlement) && !character.traitContainer.HasTrait("Hibernating", "Indestructible"))
			{
				list.Add(character);
			}
		}
		if (list != null && list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomResidentForRescue()
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (!character.isBeingSeized && !character.isDead && character.gridTileLocation != null && !character.gridTileLocation.IsNextToOrPartOfSettlement(this) && character.isBeingCarriedBy == null && character.traitContainer.HasTrait("Restrained", "Paralyzed") && !character.crimeComponent.IsWantedBy(owner))
			{
				list.Add(character);
			}
		}
		if (list != null && list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public int GetNumOfResidentsThatHasRaceAndClassOf(RACE p_race, string p_className, Type p_behaviourTypeException = null)
	{
		int num = 0;
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (character.race == p_race && character.characterClass.className == p_className && (p_behaviourTypeException == null || !character.behaviourComponent.HasBehaviour(p_behaviourTypeException)))
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumOfResidentsThatIsAliveCombatant()
	{
		int num = 0;
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (!character.isDead && character.characterClass.IsCombatant())
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfResidentsThatIsAliveMonsterAndMonsterTypeIs(SUMMON_TYPE p_summonType)
	{
		int num = 0;
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (!character.isDead && character is Summon summon && summon.summonType == p_summonType)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfResidentsThatIsAliveMonsterAndMonsterTypeIs(SUMMON_TYPE p_summonType1, SUMMON_TYPE p_summonType2)
	{
		int num = 0;
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (!character.isDead && character is Summon summon && (summon.summonType == p_summonType1 || summon.summonType == p_summonType2))
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfResidentsThatIsAliveVillager()
	{
		int num = 0;
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (!character.isDead && character.isNormalCharacter)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfResidentsForSpiderEggHatching()
	{
		int num = 0;
		if (HasResidentThatIsAliveAndMonsterTypeIs(SUMMON_TYPE.Broodmother))
		{
			for (int i = 0; i < residents.Count; i++)
			{
				Character character = residents[i];
				if (!character.isDead && (character is SmallSpider || character is GiantSpider))
				{
					num++;
				}
			}
		}
		else
		{
			for (int j = 0; j < residents.Count; j++)
			{
				if (!residents[j].isDead)
				{
					num++;
				}
			}
		}
		return num;
	}

	public bool HasResidents()
	{
		return residents.Count > 0;
	}

	public bool HasResidentForInvadeBehaviour()
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (character.isNormalCharacter && !character.isDead && !character.isAlliedWithPlayer && !character.traitContainer.HasTrait("Hibernating", "Indestructible") && !character.isInLimbo && !character.isBeingSeized && character.carryComponent.IsNotBeingCarried())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasResidentForGettingGeneralVillageTargets()
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (character.isNormalCharacter && !character.isAlliedWithPlayer && character.IsAble())
			{
				return true;
			}
		}
		return false;
	}

	public bool AreAllResidentsVagrantOrFactionless()
	{
		for (int i = 0; i < residents.Count; i++)
		{
			if (!residents[i].isVagrantOrFactionless)
			{
				return false;
			}
		}
		return true;
	}

	public void AlertSleepingCombatantResidents(int p_alertCount = -1)
	{
		int num = 0;
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (character.IsInHomeSettlement() && character.characterClass.IsCombatant() && character.traitContainer.HasTrait("Resting"))
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Alerted, character);
				num++;
			}
			if (p_alertCount != -1 && num >= p_alertCount)
			{
				break;
			}
		}
	}

	public virtual void SetOwner(Faction p_newOwner)
	{
		owner = p_newOwner;
		for (int i = 0; i < areas.Count; i++)
		{
			areas[i].areaItem.UpdatePathfindingGraph();
		}
	}

	public void GenerateStructures(params LocationStructure[] preCreatedStructures)
	{
		foreach (LocationStructure structure in preCreatedStructures)
		{
			AddStructure(structure);
		}
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
			structure.SetSettlementLocation(this);
			OnStructureAdded(structure);
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
			OnStructureRemoved(structure);
			structure.SetSettlementLocation(null);
		}
	}

	protected virtual void OnStructureAdded(LocationStructure structure)
	{
	}

	protected virtual void OnStructureRemoved(LocationStructure structure)
	{
	}

	public LocationStructure GetRandomStructureOfType(STRUCTURE_TYPE type)
	{
		if (HasStructure(type))
		{
			return structures[type][Utilities.Rng.Next(0, structures[type].Count)];
		}
		return null;
	}

	public LocationStructure GetRandomStructure()
	{
		return CollectionUtilities.GetRandomElement(allStructures);
	}

	public LocationStructure GetRandomDwellingOrResourceProducingStructure()
	{
		LocationStructure result = null;
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			if (locationStructure is Dwelling || locationStructure.structureType.IsFoodProducingStructure() || locationStructure.structureType.IsResourceProducingStructure())
			{
				list.Add(locationStructure);
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	public List<LocationStructure> GetStructuresOfType(STRUCTURE_TYPE structureType)
	{
		if (HasStructure(structureType))
		{
			return structures[structureType];
		}
		return null;
	}

	public LocationStructure GetFirstStructureOfType(STRUCTURE_TYPE type)
	{
		if (HasStructure(type))
		{
			List<LocationStructure> list = structures[type];
			if (list != null && list.Count > 0)
			{
				return list[0];
			}
		}
		return null;
	}

	public ManMadeStructure GetFirstManmadeStructureOfTypeThatHasNotReachedMaxWorkers(STRUCTURE_TYPE type)
	{
		if (HasStructure(type))
		{
			List<LocationStructure> list = structures[type];
			if (list != null && list.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] is ManMadeStructure manMadeStructure && !manMadeStructure.HasReachedMaxWorkerCapacity())
					{
						return manMadeStructure;
					}
				}
			}
		}
		return null;
	}

	public LocationStructure GetFirstUnoccupiedStructureOfType(STRUCTURE_TYPE type)
	{
		if (HasStructure(type))
		{
			List<LocationStructure> list = structures[type];
			for (int i = 0; i < list.Count; i++)
			{
				LocationStructure locationStructure = list[i];
				if (locationStructure.residents.Count == 0)
				{
					return locationStructure;
				}
			}
		}
		return null;
	}

	public LocationStructure GetFirstStructureThatIsUnoccupiedDwelling(LocationStructure p_exception = null)
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			if ((p_exception == null || p_exception != locationStructure) && !locationStructure.IsOccupied() && locationStructure is Dwelling)
			{
				return locationStructure;
			}
		}
		return null;
	}

	public LocationStructure GetFirstStructureWithStructureType(STRUCTURE_TYPE p_type, LocationStructure p_exception1 = null, LocationStructure p_exception2 = null)
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			if ((p_exception1 == null || p_exception1 != locationStructure) && (p_exception2 == null || p_exception2 != locationStructure) && locationStructure.structureType == p_type)
			{
				return locationStructure;
			}
		}
		return null;
	}

	public bool HasStructure(STRUCTURE_TYPE type1, STRUCTURE_TYPE type2, STRUCTURE_TYPE type3)
	{
		if (HasStructure(type1) || HasStructure(type2) || HasStructure(type3))
		{
			return true;
		}
		return false;
	}

	public bool HasStructure(STRUCTURE_TYPE type)
	{
		return structures.ContainsKey(type);
	}

	public bool HasHospiceClaimedByNonEnemyOrSelfAndNotBanned(Character p_character, out LocationStructure foundStructure)
	{
		if (HasStructure(STRUCTURE_TYPE.HOSPICE))
		{
			List<LocationStructure> list = structures[STRUCTURE_TYPE.HOSPICE];
			for (int i = 0; i < list.Count; i++)
			{
				LocationStructure locationStructure = list[i];
				if (locationStructure is Hospice hospice && hospice.HasAssignedWorker() && !hospice.IsBanned(p_character) && (hospice.DoesCharacterWorkHere(p_character) || hospice.HasWorkerThatIsNotAnEnemyOfCharacter(p_character)))
				{
					foundStructure = locationStructure;
					return true;
				}
			}
		}
		foundStructure = null;
		return false;
	}

	public LocationStructure GetRandomStructureThatCharacterCanBeResidentAndIsNot(Character p_character, STRUCTURE_TYPE p_exceptionType)
	{
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		LocationStructure result = null;
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			if (locationStructure.structureType != p_exceptionType && locationStructure.CanBeResidentHere(p_character))
			{
				list.Add(locationStructure);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	public LocationStructure GetRandomStructureWithTypeWhereAPartyHasPathTo(STRUCTURE_TYPE p_type, Party p_party)
	{
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		LocationStructure result = null;
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			if (locationStructure.structureType == STRUCTURE_TYPE.TAVERN && p_party.CanAMemberGoTo(locationStructure))
			{
				list.Add(locationStructure);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	public LocationStructure GetRandomStructureThatCharacterHasPathTo(Character p_character, LocationStructure p_exception1 = null, LocationStructure p_exception2 = null)
	{
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		LocationStructure result = null;
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			if ((p_exception1 == null || p_exception1 != locationStructure) && (p_exception2 == null || p_exception2 != locationStructure) && p_character.movementComponent.HasPathToEvenIfDiffRegion(locationStructure.GetRandomPassableTile()))
			{
				list.Add(locationStructure);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return result;
	}

	public int GetNumberOfStructures(STRUCTURE_TYPE structureType)
	{
		if (HasStructure(structureType))
		{
			return structures[structureType].Count;
		}
		return 0;
	}

	public int GetFacilityCount()
	{
		int num = 0;
		foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> structure in structures)
		{
			if (!structure.Key.IsFacilityStructure())
			{
				continue;
			}
			if (structure.Key == STRUCTURE_TYPE.HUNTER_LODGE || structure.Key == STRUCTURE_TYPE.BUTCHERS_SHOP || structure.Key == STRUCTURE_TYPE.FISHERY)
			{
				List<LocationStructure> value = structure.Value;
				for (int i = 0; i < value.Count; i++)
				{
					ManMadeStructure manMadeStructure = value[i] as ManMadeStructure;
					if (manMadeStructure.HasAssignedWorker() || manMadeStructure.HasSettlementOrLocalResidentThatCanWorkHere())
					{
						num++;
					}
				}
			}
			else
			{
				num += structure.Value.Count;
			}
		}
		return num;
	}

	public bool HasUnclaimedDwellingThatIsNotPreviousHome(Character p_character, out LocationStructure foundStructure)
	{
		List<LocationStructure> structuresOfType = GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
		if (structuresOfType != null)
		{
			for (int i = 0; i < structuresOfType.Count; i++)
			{
				LocationStructure locationStructure = structuresOfType[i];
				if (locationStructure != p_character.previousCharacterDataComponent.previousHomeStructure && locationStructure.residents.Count <= 0)
				{
					foundStructure = locationStructure;
					return true;
				}
			}
		}
		foundStructure = null;
		return false;
	}

	public void AddAreaToSettlement(Area p_area)
	{
		if (!HasArea(p_area))
		{
			areas.Add(p_area);
			p_area.AddSettlementOnArea(this);
		}
	}

	public void AddAreaToSettlement(List<Area> p_areas)
	{
		for (int i = 0; i < p_areas.Count; i++)
		{
			Area p_area = p_areas[i];
			AddAreaToSettlement(p_area);
		}
	}

	public virtual bool RemoveAreaFromSettlement(Area p_area)
	{
		if (areas.Remove(p_area))
		{
			p_area.RemoveSettlementFromArea(this);
			if (areas.Count <= 0)
			{
				SettlementWipedOut();
			}
			return true;
		}
		return false;
	}

	public bool HasArea(Area p_area)
	{
		return areas.Contains(p_area);
	}

	public Area GetRandomArea()
	{
		return CollectionUtilities.GetRandomElement(areas);
	}

	public LocationGridTile GetRandomPassableGridTileInSettlementStructuresThatCharacterHasPathTo(Character p_character)
	{
		LocationGridTile result = null;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			for (int j = 0; j < locationStructure.passableTiles.Count; j++)
			{
				LocationGridTile locationGridTile = locationStructure.passableTiles[j];
				if (p_character.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile))
				{
					list.Add(locationGridTile);
				}
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public void PopulatePassableTilesList(List<LocationGridTile> p_tiles)
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			LocationStructure locationStructure = allStructures[i];
			p_tiles.AddRange(locationStructure.passableTiles);
		}
	}

	private void PopulateGridTilesInSettlementThatIsPassable(List<LocationGridTile> gridTiles)
	{
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			for (int j = 0; j < area.gridTileComponent.passableTiles.Count; j++)
			{
				LocationGridTile item = area.gridTileComponent.passableTiles[j];
				gridTiles.Add(item);
			}
		}
	}

	public Area GetAPlainAdjacentArea()
	{
		List<Area> list = RuinarchListPool<Area>.Claim(10);
		Area result = null;
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			for (int j = 0; j < area.neighbourComponent.neighbours.Count; j++)
			{
				Area area2 = area.neighbourComponent.neighbours[j];
				if (area2.region == area.region && area2.elevationType != ELEVATION.MOUNTAIN && area2.elevationType != ELEVATION.WATER && !area2.HasSettlementOnArea() && !areas.Contains(area2))
				{
					list.Add(area2);
				}
			}
		}
		if (list != null && list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		RuinarchListPool<Area>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomPassableTileFromAreas(int p_searchLimit = 5)
	{
		LocationGridTile locationGridTile = null;
		int num = 0;
		while (locationGridTile == null)
		{
			locationGridTile = GetRandomArea().GetRandomPassableTile();
			num++;
			if (num >= p_searchLimit)
			{
				break;
			}
		}
		return locationGridTile;
	}

	public LocationGridTile GetRandomTileFromAreas()
	{
		return GetRandomArea().gridTileComponent.GetRandomTile();
	}

	public bool IsNearbyToArea(Area p_area)
	{
		for (int i = 0; i < areas.Count; i++)
		{
			if (areas[i].IsNearbyTo(p_area))
			{
				return true;
			}
		}
		return false;
	}

	private void StartListeningForFires()
	{
		Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		Messenger.AddListener<TileObject>(TileObjectSignals.DESTROY_TILE_OBJECT, OnTileObjectDestroyed);
		Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, OnCharacterEnteredArea);
		Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_EXITED_AREA, OnCharacterExitedArea);
	}

	private void OnTileObjectDestroyed(TileObject p_tileObject)
	{
		if (firesToDouseInSettlement.Contains(p_tileObject))
		{
			RemoveObjectOnFire(p_tileObject);
		}
	}

	private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy)
	{
		if (trait is Burning && firesToDouseInSettlement.Contains(traitable))
		{
			RemoveObjectOnFire(traitable);
		}
	}

	private void OnTraitableGainedTrait(ITraitable traitable, Trait trait)
	{
		if (trait is Burning burning && traitable.gridTileLocation != null && (traitable.gridTileLocation.IsPartOfSettlement(this) || traitable.gridTileLocation.structure.settlementLocation == this) && (!(traitable is Character { faction: not null } character) || owner == null || !character.faction.IsHostileWith(owner)) && burning.CanTriggerDouseFire())
		{
			AddObjectOnFire(traitable);
		}
	}

	private void AddObjectOnFire(ITraitable traitable)
	{
		if (traitable is IPointOfInterest item && !firesToDouseInSettlement.Contains(item))
		{
			firesToDouseInSettlement.Add(item);
		}
	}

	protected virtual bool RemoveObjectOnFire(ITraitable traitable)
	{
		if (traitable is IPointOfInterest item && firesToDouseInSettlement.Remove(item))
		{
			return true;
		}
		return false;
	}

	private void OnCharacterEnteredArea(Character character, Area p_area)
	{
		if (areas.Contains(p_area) && character.traitContainer.HasTrait("Burning"))
		{
			AddObjectOnFire(character);
		}
	}

	private void OnCharacterExitedArea(Character character, Area p_area)
	{
		if (areas.Contains(p_area) && character.traitContainer.HasTrait("Burning"))
		{
			RemoveObjectOnFire(character);
		}
	}

	protected virtual void SettlementWipedOut()
	{
	}

	public bool HasPathTowardsTileInSettlement(Character character, int tileCount)
	{
		bool flag = false;
		for (int i = 0; i < 3; i++)
		{
			LocationGridTile randomPassableTile = GetRandomArea().GetRandomPassableTile();
			flag = character.movementComponent.HasPathToEvenIfDiffRegion(randomPassableTile);
			if (flag)
			{
				break;
			}
		}
		return flag;
	}

	public void PopulateSurroundingAreas(List<Area> areas)
	{
		for (int i = 0; i < this.areas.Count; i++)
		{
			Area area = this.areas[i];
			if (this is NPCSettlement nPCSettlement && area.region != nPCSettlement.region)
			{
				continue;
			}
			for (int j = 0; j < area.neighbourComponent.neighbours.Count; j++)
			{
				Area area2 = area.neighbourComponent.neighbours[j];
				if (!area2.HasSettlementOnArea() || !area2.HasSettlementOnArea(this))
				{
					areas.Add(area2);
				}
			}
		}
	}

	public void PopulateSurroundingAreasWithPathTo(List<Area> areas, Character p_character)
	{
		for (int i = 0; i < this.areas.Count; i++)
		{
			Area area = this.areas[i];
			if (this is NPCSettlement nPCSettlement && area.region != nPCSettlement.region)
			{
				continue;
			}
			for (int j = 0; j < area.neighbourComponent.neighbours.Count; j++)
			{
				Area area2 = area.neighbourComponent.neighbours[j];
				if ((!area2.HasSettlementOnArea() || !area2.HasSettlementOnArea(this)) && p_character.movementComponent.HasPathTo(area2))
				{
					areas.Add(area2);
				}
			}
		}
	}

	public void PopulateSurroundingAreasInSameRegionWithLessThanNumOfFreezingTraps(List<Area> areas, Region region, int numOfFreezingTraps)
	{
		for (int i = 0; i < this.areas.Count; i++)
		{
			Area area = this.areas[i];
			if (this is NPCSettlement nPCSettlement && area.region != nPCSettlement.region)
			{
				continue;
			}
			for (int j = 0; j < area.neighbourComponent.neighbours.Count; j++)
			{
				Area area2 = area.neighbourComponent.neighbours[j];
				if ((!area2.HasSettlementOnArea() || !area2.HasSettlementOnArea(this)) && area2.region == region && area2.freezingTraps < numOfFreezingTraps)
				{
					areas.Add(area2);
				}
			}
		}
	}

	public void PopulateSurroundingAreasInSameRegionWithLessThanNumOfSnareTraps(List<Area> areas, Region region, int numOfSnareTraps)
	{
		for (int i = 0; i < this.areas.Count; i++)
		{
			Area area = this.areas[i];
			if (this is NPCSettlement nPCSettlement && area.region != nPCSettlement.region)
			{
				continue;
			}
			for (int j = 0; j < area.neighbourComponent.neighbours.Count; j++)
			{
				Area area2 = area.neighbourComponent.neighbours[j];
				if ((!area2.HasSettlementOnArea() || !area2.HasSettlementOnArea(this)) && area2.region == region && area2.snareTraps < numOfSnareTraps)
				{
					areas.Add(area2);
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

	public TileObject GetRandomTileObjectOfType(TILE_OBJECT_TYPE type)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		TileObject result = null;
		for (int i = 0; i < areas.Count; i++)
		{
			areas[i].tileObjectComponent.PopulateBuiltTileObjectsInArea(list, type);
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public T GetFirstTileObjectOfType<T>(TILE_OBJECT_TYPE type) where T : TileObject
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			T firstTileObjectOfType = allStructures[i].GetFirstTileObjectOfType<T>(type);
			if (firstTileObjectOfType != null)
			{
				return firstTileObjectOfType;
			}
		}
		return null;
	}

	public T GetFirstTileObjectOfType<T>(TILE_OBJECT_TYPE type1, TILE_OBJECT_TYPE type2, TILE_OBJECT_TYPE type3, TILE_OBJECT_TYPE type4) where T : TileObject
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			T firstTileObjectOfType = allStructures[i].GetFirstTileObjectOfType<T>(type1, type2, type3, type4);
			if (firstTileObjectOfType != null)
			{
				return firstTileObjectOfType;
			}
		}
		return null;
	}

	public Bed GetFirstBuiltBedThatIsAvailableAndNoActiveUsers()
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			Bed firstBuiltBedThatIsAvailableAndNoActiveUsers = allStructures[i].GetFirstBuiltBedThatIsAvailableAndNoActiveUsers();
			if (firstBuiltBedThatIsAvailableAndNoActiveUsers != null)
			{
				return firstBuiltBedThatIsAvailableAndNoActiveUsers;
			}
		}
		return null;
	}

	public void PopulateTileObjectsOfType<T>(List<TileObject> objs) where T : TileObject
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			allStructures[i].PopulateTileObjectsOfType<T>(objs);
		}
	}

	public void PopulateTileObjectsWithTraitThatActorCanReach(string p_traitName, List<TileObject> p_objects, Character p_actor)
	{
		for (int i = 0; i < allStructures.Count; i++)
		{
			allStructures[i].PopulateTileObjectsWithTraitThatActorCanReach(p_objects, p_traitName, p_actor);
		}
	}

	public int GetNumberOfTileObjectsInAllStructures(TILE_OBJECT_TYPE tileObjectType)
	{
		int num = 0;
		for (int i = 0; i < allStructures.Count; i++)
		{
			num += allStructures[i].GetNumberOfTileObjects(tileObjectType);
		}
		return num;
	}

	public int GetNumberOfTileObjectsInAllAreas(TILE_OBJECT_TYPE tileObjectType)
	{
		int num = 0;
		for (int i = 0; i < areas.Count; i++)
		{
			num += areas[i].tileObjectComponent.GetNumberOfBuiltTileObjects(tileObjectType);
		}
		return num;
	}

	public int GetNumberOfFoodInAllFoodProducingStructures()
	{
		int num = 0;
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			for (int j = 0; j < area.tileObjectComponent.resourcePiles.Count; j++)
			{
				ResourcePile resourcePile = area.tileObjectComponent.resourcePiles[j];
				if (resourcePile.gridTileLocation != null && resourcePile.gridTileLocation.structure.structureType.IsFoodProducingStructure() && resourcePile is FoodPile)
				{
					num += resourcePile.resourceInPile;
				}
			}
		}
		return num;
	}

	public int GetNumberOfFoodInWholeSettlement()
	{
		int num = 0;
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			for (int j = 0; j < area.tileObjectComponent.resourcePiles.Count; j++)
			{
				ResourcePile resourcePile = area.tileObjectComponent.resourcePiles[j];
				if (resourcePile.gridTileLocation != null && resourcePile is FoodPile)
				{
					num += resourcePile.resourceInPile;
				}
			}
		}
		return num;
	}

	public int GetNumberOfFoodInBanditCamp()
	{
		int num = 0;
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			for (int j = 0; j < area.tileObjectComponent.resourcePiles.Count; j++)
			{
				ResourcePile resourcePile = area.tileObjectComponent.resourcePiles[j];
				if (resourcePile.gridTileLocation != null && resourcePile.gridTileLocation.structure.structureType == STRUCTURE_TYPE.BANDIT_CAMP && resourcePile is FoodPile)
				{
					num += resourcePile.resourceInPile;
				}
			}
		}
		return num;
	}

	public int GetNumberOfMainBasicResourceInAllResourceProducingStructures()
	{
		bool flag = false;
		RESOURCE rESOURCE = RESOURCE.WOOD;
		if (owner == null)
		{
			flag = true;
		}
		else
		{
			flag = owner.factionType.usesBothWoodAndStoneResources;
			rESOURCE = owner.factionType.mainResource;
		}
		int num = 0;
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			for (int j = 0; j < area.tileObjectComponent.resourcePiles.Count; j++)
			{
				ResourcePile resourcePile = area.tileObjectComponent.resourcePiles[j];
				if (resourcePile.gridTileLocation == null || !resourcePile.gridTileLocation.structure.structureType.IsResourceProducingStructure())
				{
					continue;
				}
				if (flag)
				{
					if (resourcePile is StonePile || resourcePile is WoodPile)
					{
						num += resourcePile.resourceInPile;
					}
				}
				else if (resourcePile.providedResource == rESOURCE)
				{
					num += resourcePile.resourceInPile;
				}
			}
		}
		return num;
	}

	public int GetNumberOfMainBasicResourceInBanditCamp()
	{
		bool flag = false;
		RESOURCE rESOURCE = RESOURCE.WOOD;
		if (owner == null)
		{
			flag = true;
		}
		else
		{
			flag = owner.factionType.usesBothWoodAndStoneResources;
			rESOURCE = owner.factionType.mainResource;
		}
		int num = 0;
		for (int i = 0; i < areas.Count; i++)
		{
			Area area = areas[i];
			for (int j = 0; j < area.tileObjectComponent.resourcePiles.Count; j++)
			{
				ResourcePile resourcePile = area.tileObjectComponent.resourcePiles[j];
				if (resourcePile.gridTileLocation == null || resourcePile.gridTileLocation.structure.structureType != STRUCTURE_TYPE.BANDIT_CAMP)
				{
					continue;
				}
				if (flag)
				{
					if (resourcePile is StonePile || resourcePile is WoodPile)
					{
						num += resourcePile.resourceInPile;
					}
				}
				else if (resourcePile.providedResource == rESOURCE)
				{
					num += resourcePile.resourceInPile;
				}
			}
		}
		return num;
	}

	public void AddParty(Party party)
	{
		if (!parties.Contains(party))
		{
			parties.Add(party);
		}
	}

	public bool RemoveParty(Party party)
	{
		return parties.Remove(party);
	}

	public Party GetPreferredUnfullPartyThatCharacterCanJoin(Character p_character)
	{
		int num = int.MinValue;
		Party result = null;
		for (int i = 0; i < parties.Count; i++)
		{
			Party party = parties[i];
			if (party.isDisbanded && RemoveParty(party))
			{
				i--;
			}
			else if (party.members.Count < 5 && party.members.Count > 0 && !party.banningComponent.IsBanned(p_character))
			{
				int totalOpinionOfCharacterTowardsParty = party.GetTotalOpinionOfCharacterTowardsParty(p_character);
				int totalOpinionOfPartyMembersTowardsCharacterIgnoreNewcomer = party.GetTotalOpinionOfPartyMembersTowardsCharacterIgnoreNewcomer(p_character);
				int num2 = totalOpinionOfCharacterTowardsParty + totalOpinionOfPartyMembersTowardsCharacterIgnoreNewcomer;
				if (num2 >= 0 && num2 > num)
				{
					num = num2;
					result = party;
				}
			}
		}
		return result;
	}

	public int GetPartyCount()
	{
		return parties.Count;
	}

	public LocationGridTile GetRandomPassableTile()
	{
		LocationStructure locationStructure = GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
		if (locationStructure == null)
		{
			locationStructure = GetRandomStructure();
		}
		return locationStructure?.GetRandomPassableTile();
	}

	public LocationGridTile GetRandomPassableTileThatIsCorruptedWithPathTo(Character character)
	{
		LocationGridTile result = null;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < areas.Count; i++)
		{
			LocationGridTile randomPassableTileThatIsCorruptedWithPathTo = areas[i].gridTileComponent.GetRandomPassableTileThatIsCorruptedWithPathTo(character);
			if (randomPassableTileThatIsCorruptedWithPathTo != null)
			{
				list.Add(randomPassableTileThatIsCorruptedWithPathTo);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public bool IsAtTargetDestination(Character character)
	{
		return character.currentSettlement == this;
	}

	public virtual void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		actions = new List<PLAYER_SKILL_TYPE>();
	}

	public void AddPlayerAction(PLAYER_SKILL_TYPE action, bool broadcastSignal = true)
	{
		if (!actions.Contains(action))
		{
			actions.Add(action);
			if (broadcastSignal)
			{
				Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_ADDED_TO_TARGET, action, (IPlayerActionTarget)this);
			}
		}
	}

	public void RemovePlayerAction(PLAYER_SKILL_TYPE action, bool broadcastSignal = true)
	{
		if (actions.Remove(action) && broadcastSignal)
		{
			Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, (IPlayerActionTarget)this);
		}
	}

	public void ClearPlayerActions()
	{
		actions.Clear();
	}

	public bool IsValidForStoreTarget()
	{
		return true;
	}

	public bool CanBeStoredAsTarget()
	{
		return true;
	}

	public void SetAsStoredTarget(bool p_state)
	{
		isStoredAsTarget = p_state;
	}

	public bool IsValidTargetForPartyStructure(LocationStructure p_structure)
	{
		return true;
	}

	public Sprite GetPortraitSprite()
	{
		LocationStructure locationStructure = allStructures.FirstOrDefault();
		if (locationStructure != null)
		{
			return LandmarkManager.Instance.GetStructureData(locationStructure.structureType).structureSprite;
		}
		return LandmarkManager.Instance.GetStructureData(STRUCTURE_TYPE.CITY_CENTER).structureSprite;
	}

	public void OnSelectBookmark()
	{
		UIManager.Instance.ShowSettlementInfo(this);
	}

	public void RemoveBookmark()
	{
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this);
	}

	public void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
	}

	public void OnHoverOutBookmarkItem()
	{
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

	public virtual void LoadReferences(SaveDataBaseSettlement data)
	{
		if (!string.IsNullOrEmpty(data.factionOwnerID))
		{
			owner = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(data.factionOwnerID);
		}
	}

	public virtual void LoadReferencesMainThread(SaveDataBaseSettlement data)
	{
		StartListeningForFires();
		if (data.firesInSettlement != null)
		{
			firesToDouseInSettlement.Clear();
			firesToDouseInSettlement.AddRange(SaveUtilities.ConvertPOIDataListToPOIList(data.firesInSettlement));
		}
	}

	public virtual void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> structure in structures)
		{
			structure.Value.Contains(p_structure);
		}
		allStructures.Contains(p_structure);
		SettlementResources?.CheckIfStructureIsStillReferenced(p_structure);
	}

	public virtual void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		residents.Contains(p_character);
		firesToDouseInSettlement.Contains(p_character);
		SettlementResources?.CheckIfCharacterIsStillReferenced(p_character);
	}
}
