using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures.Components;
using Locations;
using Locations.Settlements;
using Logs;
using Ruinarch;
using Traits;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

[Serializable]
public abstract class LocationStructure : IPlayerActionTarget, ISelectable, IPartyQuestTarget, IPartyTargetDestination, IGatheringTarget, ILogFiller, ILocation, ISavable, IStoredTarget, IBookmarkable, LocalizationManagerEventDispatcher.ILocaleChangeListener
{
	protected string _uiString;

	protected List<PLAYER_SKILL_TYPE> _actions = new List<PLAYER_SKILL_TYPE>();

	public string persistentID { get; }

	public int id { get; private set; }

	public string name { get; protected set; }

	public int maxResidentCapacity { get; protected set; }

	public STRUCTURE_TYPE structureType { get; private set; }

	public List<STRUCTURE_TAG> structureTags { get; protected set; }

	public List<Character> charactersHere { get; private set; }

	public Region region { get; private set; }

	public BaseSettlement settlementLocation { get; private set; }

	public HashSet<IPointOfInterest> pointsOfInterest { get; private set; }

	public Dictionary<TILE_OBJECT_TYPE, List<TileObject>> groupedTileObjects { get; private set; }

	public Area occupiedArea { get; private set; }

	public NPCSettlement linkedSettlement { get; private set; }

	public HashSet<LocationGridTile> tiles { get; private set; }

	public List<LocationGridTile> passableTiles { get; private set; }

	public List<LocationGridTile> unoccupiedTiles { get; private set; }

	public int differentFoodPileKindsInDwelling { get; private set; }

	public bool isInterior { get; private set; }

	public bool hasBeenDestroyed { get; private set; }

	public bool isStoredAsTarget { get; private set; }

	public bool isProtected { get; private set; }

	public int maxHP { get; protected set; }

	public int currentHP { get; protected set; }

	public HashSet<IDamageable> objectsThatContributeToDamage { get; private set; }

	public List<Character> residents { get; protected set; }

	public StructureRoom[] rooms { get; protected set; }

	public bool hasActiveSocialGathering { get; protected set; }

	public LocationAwareness locationAwareness { get; protected set; }

	public LocationStructureEventDispatcher eventDispatcher { get; private set; }

	public PartyStructureComponent partyStructureComponent { get; private set; }

	public Dictionary<Area, int> occupiedAreas { get; private set; }

	public BookmarkableEventDispatcher bookmarkEventDispatcher { get; private set; }

	public bool hasBeenCleanedUp { get; private set; }

	public string bookmarkName => iconRichText + " " + name;

	public BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Text_With_Cancel;

	public virtual string nameplateName => name;

	public string locationName => ToString();

	public virtual bool isDwelling => false;

	public virtual Vector3 worldPosition { get; protected set; }

	public virtual Vector2 selectableSize => Vector2.zero;

	public virtual Type serializedData => typeof(SaveDataLocationStructure);

	public LocationStructure currentStructure => this;

	public BaseSettlement currentSettlement => settlementLocation;

	public OBJECT_TYPE objectType => OBJECT_TYPE.Structure;

	public STORED_TARGET_TYPE storedTargetType => STORED_TARGET_TYPE.Structures;

	public bool isTargetted { get; set; }

	public string iconRichText => Utilities.StructureIcon();

	public PARTY_TARGET_DESTINATION_TYPE partyTargetDestinationType => PARTY_TARGET_DESTINATION_TYPE.Structure;

	public virtual TILE_OBJECT_TYPE[] preplacedObjectsToIgnoreWhenBuilding => null;

	public virtual bool shouldBeLoadedOnMainThread => false;

	public string extraInfo1Header => GetExtraInfo1Header();

	public string extraInfo1Description => GetExtraInfo1Description();

	public string extraInfo2Header => GetExtraInfo2Header();

	public string extraInfo2Description => GetExtraInfo2Description();

	public string customDescription => GetCustomDescription();

	public string uiString => GetUIString();

	public List<PLAYER_SKILL_TYPE> actions => _actions;

	protected LocationStructure(STRUCTURE_TYPE structureType, Region location)
	{
		persistentID = Guid.NewGuid().ToString();
		id = Utilities.SetID(this);
		this.structureType = structureType;
		region = location;
		charactersHere = new List<Character>(20);
		pointsOfInterest = new HashSet<IPointOfInterest>();
		groupedTileObjects = new Dictionary<TILE_OBJECT_TYPE, List<TileObject>>(50);
		tiles = new HashSet<LocationGridTile>();
		passableTiles = new List<LocationGridTile>();
		unoccupiedTiles = new List<LocationGridTile>();
		objectsThatContributeToDamage = new HashSet<IDamageable>();
		structureTags = new List<STRUCTURE_TAG>();
		residents = new List<Character>();
		occupiedAreas = new Dictionary<Area, int>();
		SetMaxHPAndReset(2000);
		SetInteriorState(structureType.IsInterior());
		maxResidentCapacity = 5;
		locationAwareness = new LocationAwareness();
		eventDispatcher = new LocationStructureEventDispatcher();
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		StructureData structureData = LandmarkManager.Instance.GetStructureData(structureType);
		if (structureData != null && structureData.structurePartyType != STRUCTURE_PARTY_TYPE.None)
		{
			CreatePartyStructureComponent(structureType);
		}
	}

	protected LocationStructure(Region location, SaveDataLocationStructure data)
	{
		charactersHere = new List<Character>(20);
		pointsOfInterest = new HashSet<IPointOfInterest>();
		groupedTileObjects = new Dictionary<TILE_OBJECT_TYPE, List<TileObject>>(50);
		structureTags = new List<STRUCTURE_TAG>(data.structureTags);
		tiles = new HashSet<LocationGridTile>();
		passableTiles = new List<LocationGridTile>();
		unoccupiedTiles = new List<LocationGridTile>();
		objectsThatContributeToDamage = new HashSet<IDamageable>();
		residents = new List<Character>();
		occupiedAreas = new Dictionary<Area, int>();
		persistentID = data.persistentID;
		region = location;
		id = Utilities.SetID(this, data.id);
		structureType = data.structureType;
		name = data.name;
		isStoredAsTarget = data.isStoredAsTarget;
		maxHP = data.maxHP;
		currentHP = data.currentHP;
		SetInteriorState(data.isInterior);
		maxResidentCapacity = 5;
		hasBeenDestroyed = data.hasBeenDestroyed;
		differentFoodPileKindsInDwelling = data.differentFoodPileKindsInDwelling;
		locationAwareness = new LocationAwareness();
		eventDispatcher = new LocationStructureEventDispatcher();
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		if (data.saveDataPartyStructureComponent != null)
		{
			partyStructureComponent = data.saveDataPartyStructureComponent.Load();
		}
	}

	protected virtual string GenerateName()
	{
		if (structureType == STRUCTURE_TYPE.WILDERNESS || structureType == STRUCTURE_TYPE.THE_PORTAL)
		{
			return structureType.LocalizedStructureName();
		}
		string text;
		if (this is DemonicStructure)
		{
			text = LocalizationManager.Instance.GetRandomEntry("DemonicStructureNouns_Table");
		}
		else if (this is ManMadeStructure)
		{
			if (settlementLocation == null || settlementLocation.owner == null)
			{
				text = ((!(settlementLocation is NPCSettlement { locationType: LOCATION_TYPE.VILLAGE } nPCSettlement)) ? ((!(settlementLocation is PlayerSettlement)) ? LocalizationManager.Instance.GetRandomEntry("MiscStructureNouns_Table") : LocalizationManager.Instance.GetRandomEntry("DemonicStructureNouns_Table")) : (nPCSettlement.settlementType.settlementType switch
				{
					SETTLEMENT_TYPE.Human_Village => LocalizationManager.Instance.GetRandomEntry("HumanStructureNouns_Table"), 
					SETTLEMENT_TYPE.Elven_Hamlet => LocalizationManager.Instance.GetRandomEntry("ElvenStructureNouns_Table"), 
					_ => LocalizationManager.Instance.GetRandomEntry("MiscStructureNouns_Table"), 
				}));
			}
			else
			{
				switch (settlementLocation.owner.factionType.type)
				{
				case FACTION_TYPE.Human_Empire:
				case FACTION_TYPE.Lycan_Clan:
				case FACTION_TYPE.Divine_Church:
					text = LocalizationManager.Instance.GetRandomEntry("HumanStructureNouns_Table");
					break;
				case FACTION_TYPE.Elven_Kingdom:
				case FACTION_TYPE.Vampire_Clan:
				case FACTION_TYPE.Wiccans:
					text = LocalizationManager.Instance.GetRandomEntry("ElvenStructureNouns_Table");
					break;
				case FACTION_TYPE.Demon_Cult:
					text = LocalizationManager.Instance.GetRandomEntry("DemonicStructureNouns_Table");
					break;
				default:
					text = LocalizationManager.Instance.GetRandomEntry("MiscStructureNouns_Table");
					break;
				}
			}
		}
		else
		{
			text = LocalizationManager.Instance.GetRandomEntry("MiscStructureNouns_Table");
		}
		if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Spanish (Latin America) (es)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("Portuguese (pt)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("Indonesian (id)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("Spanish (Spain) (es-ES)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("Italian (it)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("Thai (th)"))
		{
			return structureType.LocalizedStructureName() + " " + text;
		}
		return text + " " + structureType.LocalizedStructureName();
	}

	public void ReGenerateName()
	{
		name = GenerateName();
		_uiString = string.Empty;
		bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		Messenger.Broadcast(StructureSignals.STRUCTURE_NAME_UPDATED, this);
	}

	public virtual void LoadReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		residents = SaveUtilities.ConvertIDListToCharacters(saveDataLocationStructure.residentIDs);
		charactersHere = SaveUtilities.ConvertIDListToCharacters(saveDataLocationStructure.charactersHereIDs);
		if (saveDataLocationStructure.structureRoomSaveData != null && rooms != null)
		{
			for (int i = 0; i < rooms.Length; i++)
			{
				StructureRoom obj = rooms[i];
				SaveDataStructureRoom saveDataStructureRoom = saveDataLocationStructure.structureRoomSaveData[i];
				obj.LoadReferences(saveDataStructureRoom);
			}
		}
		if (saveDataLocationStructure.tileObjectDamageContributors != null)
		{
			for (int j = 0; j < saveDataLocationStructure.tileObjectDamageContributors.Count; j++)
			{
				string text = saveDataLocationStructure.tileObjectDamageContributors[j];
				objectsThatContributeToDamage.Add(DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(text));
			}
		}
		partyStructureComponent?.LoadReferences(this, saveDataLocationStructure.saveDataPartyStructureComponent);
	}

	public virtual void LoadAdditionalReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		if (saveDataLocationStructure.structureRoomSaveData != null && rooms != null)
		{
			for (int i = 0; i < rooms.Length; i++)
			{
				StructureRoom obj = rooms[i];
				SaveDataStructureRoom saveDataStructureRoom = saveDataLocationStructure.structureRoomSaveData[i];
				obj.LoadAdditionalReferences(saveDataStructureRoom);
			}
		}
	}

	public virtual void LoadStructureSecondWaveInMainThread(SaveDataLocationStructure saveDataLocationStructure)
	{
		if (saveDataLocationStructure.structureRoomSaveData != null && rooms != null)
		{
			for (int i = 0; i < rooms.Length; i++)
			{
				StructureRoom obj = rooms[i];
				SaveDataStructureRoom saveDataStructureRoom = saveDataLocationStructure.structureRoomSaveData[i];
				obj.LoadStructureRoomSecondWaveInMainThread(saveDataStructureRoom);
			}
		}
	}

	public virtual void OnBuiltNewStructure()
	{
		if (UIManager.Instance != null)
		{
			UIManager.Instance.unbuiltStructureInfoUI.OnBuiltStructure(this);
		}
	}

	public virtual void OnDoneLoadStructure()
	{
	}

	protected virtual void OnAddResident(Character newResident)
	{
		if (residents.Count == 1 && structureType.IsSpecialStructure() && newResident.isMonsterOrRatmanOrUndead)
		{
			if (linkedSettlement != null)
			{
				linkedSettlement.structureComponent.RemoveLinkedStructure(this);
			}
			LinkThisStructureToAVillage();
		}
		partyStructureComponent?.OnResidentAdded(newResident);
	}

	protected virtual void OnRemoveResident(Character p_resident)
	{
		p_resident.UnownOrTransferOwnershipOfItemsIn(this);
	}

	public virtual bool CanBeResidentHere(Character character)
	{
		return true;
	}

	protected virtual void AfterSetSettlementLocation()
	{
	}

	protected virtual string GetExtraInfo1Header()
	{
		if (HasTileObjectOfType(TILE_OBJECT_TYPE.MONSTER_SPAWNER) && partyStructureComponent?.availableBehaviours != null && partyStructureComponent.availableBehaviours.Length != 0)
		{
			return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Spawner_Actions_Title") ?? "";
		}
		return string.Empty;
	}

	protected virtual string GetExtraInfo1Description()
	{
		if (HasTileObjectOfType(TILE_OBJECT_TYPE.MONSTER_SPAWNER) && partyStructureComponent?.availableBehaviours != null && partyStructureComponent.availableBehaviours.Length != 0)
		{
			return partyStructureComponent.availableBehaviours.ComafyAndLocalizeList("PlayerActions_Table");
		}
		return string.Empty;
	}

	protected virtual string GetExtraInfo2Header()
	{
		return string.Empty;
	}

	protected virtual string GetExtraInfo2Description()
	{
		return string.Empty;
	}

	public virtual void Initialize()
	{
		name = GenerateName();
		SubscribeListeners(shouldLock: false);
		ConstructDefaultPlayerActions();
	}

	public virtual void InitializeFromSave()
	{
		SubscribeListeners(shouldLock: true);
		ConstructDefaultPlayerActions(broadcastSignal: false);
	}

	protected virtual void SubscribeListeners(bool shouldLock)
	{
		Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure, shouldLock);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter, shouldLock);
	}

	protected virtual void UnsubscribeListeners()
	{
		Messenger.RemoveListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
	}

	private void DisconnectFromStructure(LocationStructure p_structure)
	{
		partyStructureComponent?.DisconnectFromStructure(p_structure);
	}

	protected virtual void DisconnectFromCharacter(Character p_character)
	{
		partyStructureComponent?.DisconnectFromCharacter(p_character);
		RemoveCharacterAtLocation(p_character);
		RemoveResident(p_character);
		RemovePOI(p_character);
	}

	public virtual string GetNameRelativeTo(Character character)
	{
		string text;
		switch (structureType)
		{
		case STRUCTURE_TYPE.TAVERN:
		case STRUCTURE_TYPE.WAREHOUSE:
		case STRUCTURE_TYPE.CEMETERY:
		case STRUCTURE_TYPE.PRISON:
			text = ((settlementLocation != null) ? ((!LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)")) ? (settlementLocation.name + " " + name) : (name + " w " + settlementLocation.name + " ")) : ((!LocalizationSettings.SelectedLocale.LocaleName.Equals("Spanish (Latin America) (es)") && !LocalizationSettings.SelectedLocale.LocaleName.Equals("Portuguese (pt)") && !LocalizationSettings.SelectedLocale.LocaleName.Equals("Spanish (Spain) (es-ES)")) ? (LocalizationManager.The + " " + name) : name));
			break;
		case STRUCTURE_TYPE.CITY_CENTER:
			text = ((settlementLocation != null) ? (settlementLocation.name ?? "") : ((!LocalizationSettings.SelectedLocale.LocaleName.Equals("Spanish (Latin America) (es)") && !LocalizationSettings.SelectedLocale.LocaleName.Equals("Portuguese (pt)") && !LocalizationSettings.SelectedLocale.LocaleName.Equals("Spanish (Spain) (es-ES)")) ? (LocalizationManager.The + " " + name) : name));
			break;
		default:
			text = ((!LocalizationSettings.SelectedLocale.LocaleName.Equals("Spanish (Latin America) (es)") && !LocalizationSettings.SelectedLocale.LocaleName.Equals("Portuguese (pt)") && !LocalizationSettings.SelectedLocale.LocaleName.Equals("Spanish (Spain) (es-ES)")) ? (LocalizationManager.The + " " + name) : name);
			break;
		}
		return text.Trim();
	}

	public void PopulateEdgeTiles(List<LocationGridTile> p_edgeTiles)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			LocationGridTile locationGridTile = tiles.ElementAt(i);
			if (locationGridTile.HasDifferentDwellingOrOutsideNeighbour())
			{
				p_edgeTiles.Add(locationGridTile);
			}
		}
	}

	public void DoCleanup()
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is TileObject tileObject)
			{
				tileObject.DoCleanup();
			}
		}
	}

	public void SetSettlementLocation(BaseSettlement npcSettlement)
	{
		settlementLocation = npcSettlement;
		AfterSetSettlementLocation();
	}

	public void SetInteriorState(bool _isInterior)
	{
		isInterior = _isInterior;
	}

	public abstract void CenterOnStructure();

	public abstract void ShowSelectorOnStructure();

	public virtual void ProcessOnSetAsActiveInStructureInfo()
	{
	}

	public virtual void ProcessOnSetAsInactiveInStructureInfo()
	{
		partyStructureComponent?.ProcessOnSetAsInactiveInStructureInfo();
	}

	public void AddStructureTag(STRUCTURE_TAG tag)
	{
		structureTags.Add(tag);
	}

	public bool RemoveStructureTag(STRUCTURE_TAG tag)
	{
		return structureTags.Remove(tag);
	}

	public bool HasStructureTag(STRUCTURE_TAG tag)
	{
		for (int i = 0; i < structureTags.Count; i++)
		{
			if (structureTags[i] == tag)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasStructureTags()
	{
		return structureTags.Count > 0;
	}

	public override string ToString()
	{
		return structureType.ToString() + " " + id + " at " + region?.name;
	}

	public void SetHasActiveSocialGathering(bool state)
	{
		hasActiveSocialGathering = state;
	}

	public virtual bool HasTileOnArea(Area p_area)
	{
		if (occupiedArea == null || occupiedArea != p_area)
		{
			if (occupiedAreas != null)
			{
				return occupiedAreas.ContainsKey(p_area);
			}
			return false;
		}
		return true;
	}

	protected virtual string GetCustomDescription()
	{
		string text = structureType.ToStringEnumWithSpace();
		return LocalizationManager.Instance.GetLocalizedValue("DemonicStructures_Table", text + "_Scenario_Description");
	}

	public bool IsStructureValidForPurifyingGround()
	{
		if (settlementLocation != null && settlementLocation.locationType == LOCATION_TYPE.VILLAGE && settlementLocation.owner != null && !settlementLocation.owner.isPlayerFaction && settlementLocation is NPCSettlement nPCSettlement && !nPCSettlement.HasJob(JOB_TYPE.PURIFY_GROUND))
		{
			return true;
		}
		return false;
	}

	public virtual void OnCharacterUnSeizedHere(Character character)
	{
	}

	public virtual void AddCharacterAtLocation(Character character, LocationGridTile tile = null)
	{
		if (character.hasBeenCleanedUp)
		{
			return;
		}
		bool flag = false;
		if (!charactersHere.Contains(character))
		{
			flag = true;
			charactersHere.Add(character);
			AddPOI(character, tile);
			if (IsResident(character))
			{
				partyStructureComponent?.OnResidentArrived(character);
			}
		}
		character.SetCurrentStructureLocation(this);
		if (flag)
		{
			AfterCharacterAddedToLocation(character);
		}
	}

	public void RemoveCharacterAtLocation(Character character)
	{
		if (charactersHere.Remove(character))
		{
			character.SetCurrentStructureLocation(null);
			RemovePOI(character);
			AfterCharacterRemovedFromLocation(character);
		}
	}

	protected virtual void AfterCharacterAddedToLocation(Character p_character)
	{
	}

	protected virtual void AfterCharacterRemovedFromLocation(Character p_character)
	{
	}

	public int GetNumberOfSummonsHere()
	{
		int num = 0;
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			if (character.gridTileLocation != null && character is Summon && !character.isDead)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfAliveAndCanWitnessSapientsHere()
	{
		int num = 0;
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			if (character.gridTileLocation != null && !character.isDead && character.limiterComponent.canWitness && character.race.IsSapient())
			{
				num++;
			}
		}
		return num;
	}

	public void PopulateCharacterListThatIsWebbed(List<Character> characterList)
	{
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			if (character.traitContainer.HasTrait("Webbed"))
			{
				characterList.Add(character);
			}
		}
	}

	public void PopulateCharacterListThatHasTraitAndIsNotRace(List<Character> characterList, string p_traitName, RACE p_race)
	{
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			if (!character.isDead && character.traitContainer.HasTrait(p_traitName) && character.race != p_race)
			{
				characterList.Add(character);
			}
		}
	}

	public Character GetRandomCharacterThatCanBeRecruitedBy(Character p_recruiter)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			if (character.behaviourComponent.CanCharacterBeRecruitedBy(p_recruiter))
			{
				list.Add(character);
			}
		}
		Character result = null;
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterThatIsAliveCanPerformAndWitnessAndNotInCombatExcept(Character p_character)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			if (!character.combatComponent.isInCombat && character.limiterComponent.canPerform && character.limiterComponent.canWitness && !character.isDead && character != p_character)
			{
				list.Add(character);
			}
		}
		Character result = null;
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterThatIsVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(Character p_character)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			if (p_character != character && character.isNormalCharacter && !character.isBeingSeized && character.isBeingCarriedBy == null && !character.HasJobTargetingThis(JOB_TYPE.PRODUCE_FOOD) && character.traitContainer.HasTrait("Restrained"))
			{
				list.Add(character);
			}
		}
		Character result = null;
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterThatIsAliveVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(Character p_character)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			if (p_character != character && character.isNormalCharacter && !character.isBeingSeized && character.isBeingCarriedBy == null && !character.isDead && !character.HasJobTargetingThis(JOB_TYPE.PRODUCE_FOOD) && character.traitContainer.HasTrait("Restrained"))
			{
				list.Add(character);
			}
		}
		Character result = null;
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomCharacterThatIsDead()
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			if (character.isDead)
			{
				list.Add(character);
			}
		}
		Character result = null;
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	public Character GetRandomResidentForInvasionTargetThatIsInsideStructureAndHostileWithFaction(Faction p_faction, Character p_exception = null)
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if ((p_exception == null || p_exception != character) && !character.isBeingSeized && !character.isDead && character.gridTileLocation != null && character.gridTileLocation.structure == this && (character.faction == null || p_faction == null || p_faction.IsHostileWith(character.faction)) && !character.traitContainer.HasTrait("Hibernating", "Indestructible"))
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

	public Character GetFirstCharacterInsideStructureThatIsAliveHostileThatHasPathTo(Character p_character, Character p_exception = null)
	{
		for (int i = 0; i < charactersHere.Count; i++)
		{
			Character character = charactersHere[i];
			if (character.gridTileLocation != null && (p_exception == null || p_exception != character) && p_character != character && p_character.IsHostileWith(character) && !character.isDead && !character.isAlliedWithPlayer && (bool)character.marker && character.marker.isMainVisualActive && p_character.movementComponent.HasPathTo(character.gridTileLocation) && !character.isInLimbo && !character.isBeingSeized && character.carryComponent.IsNotBeingCarried() && !character.traitContainer.HasTrait("Hibernating", "Indestructible"))
			{
				return character;
			}
		}
		return null;
	}

	protected void ProcessAllTileObjects(Action<TileObject> action)
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is TileObject obj)
			{
				action(obj);
			}
		}
	}

	public void PopulateTileObjectsThatAdvertise(List<TileObject> p_objectList, INTERACTION_TYPE type)
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is TileObject tileObject && tileObject.IsAvailable() && tileObject.Advertises(type))
			{
				p_objectList.Add(tileObject);
			}
		}
		for (int j = 0; j < tiles.Count; j++)
		{
			LocationGridTile locationGridTile = tiles.ElementAt(j);
			if (locationGridTile.tileObjectComponent.genericTileObject.IsAvailable() && locationGridTile.tileObjectComponent.genericTileObject.Advertises(type))
			{
				p_objectList.Add(locationGridTile.tileObjectComponent.genericTileObject);
			}
		}
	}

	public void PopulateTileObjectsWithTraitThatActorCanReach(List<TileObject> p_objectList, string p_traitName, Character p_actor)
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is TileObject tileObject && tileObject.traitContainer.HasTrait(p_traitName) && p_actor.movementComponent.HasPathToEvenIfDiffRegion(tileObject.gridTileLocation))
			{
				p_objectList.Add(tileObject);
			}
		}
	}

	public TileObject GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE type1, TILE_OBJECT_TYPE type2)
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			IPointOfInterest pointOfInterest = pointsOfInterest.ElementAt(i);
			if (pointOfInterest.IsAvailable() && pointOfInterest is TileObject tileObject && (type1 == tileObject.tileObjectType || type2 == tileObject.tileObjectType) && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT)
			{
				return tileObject;
			}
		}
		return null;
	}

	public TileObject GetUnoccupiedTileObject(TILE_OBJECT_TYPE type)
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			IPointOfInterest pointOfInterest = pointsOfInterest.ElementAt(i);
			if (pointOfInterest.IsAvailable() && pointOfInterest is TileObject tileObject && type == tileObject.tileObjectType && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT)
			{
				return tileObject;
			}
		}
		return null;
	}

	public IDamageable GetNearestDamageableThatContributeToHP(LocationGridTile fromTile)
	{
		IDamageable damageable = null;
		float num = 9999f;
		for (int i = 0; i < objectsThatContributeToDamage.Count; i++)
		{
			IDamageable damageable2 = objectsThatContributeToDamage.ElementAt(i);
			if (damageable2.gridTileLocation != null)
			{
				float distanceTo = fromTile.GetDistanceTo(damageable2.gridTileLocation);
				if (damageable == null || distanceTo < num)
				{
					damageable = damageable2;
					num = distanceTo;
				}
			}
		}
		return damageable;
	}

	public List<TileObject> GetTileObjectsOfType(TILE_OBJECT_TYPE type)
	{
		if (groupedTileObjects.ContainsKey(type))
		{
			return groupedTileObjects[type];
		}
		return null;
	}

	public bool HasTileObjectOfType(TILE_OBJECT_TYPE type)
	{
		if (groupedTileObjects.ContainsKey(type))
		{
			return groupedTileObjects[type].Count > 0;
		}
		return false;
	}

	public bool HasBuiltTileObjectOfType(TILE_OBJECT_TYPE type)
	{
		if (groupedTileObjects.ContainsKey(type))
		{
			return groupedTileObjects[type].Any((TileObject t) => t.mapObjectState == MAP_OBJECT_STATE.BUILT);
		}
		return false;
	}

	public bool HasBuildingTileObjectOfType(TILE_OBJECT_TYPE type)
	{
		if (groupedTileObjects.ContainsKey(type))
		{
			return groupedTileObjects[type].Any((TileObject t) => t.mapObjectState == MAP_OBJECT_STATE.BUILDING);
		}
		return false;
	}

	public bool HasBuiltResourcePileOfTypeThatHasResourceAmount(TILE_OBJECT_TYPE type, int amount)
	{
		if (groupedTileObjects.ContainsKey(type))
		{
			List<TileObject> list = groupedTileObjects[type];
			for (int i = 0; i < list.Count; i++)
			{
				TileObject tileObject = list[i];
				if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT && tileObject is ResourcePile resourcePile && resourcePile.resourceInPile >= amount)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasTileObjectThatIsBuiltFoodPile()
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is FoodPile { mapObjectState: MAP_OBJECT_STATE.BUILT })
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTileObjectThatIsBuiltFoodPileThatCharacterCanEatAndDoesntHaveAtHome(Character p_character)
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is FoodPile { mapObjectState: MAP_OBJECT_STATE.BUILT } foodPile && !p_character.homeStructure.HasBuiltTileObjectOfType(foodPile.tileObjectType))
			{
				if (p_character.traitContainer.HasTrait("Cannibal"))
				{
					return true;
				}
				if (foodPile.tileObjectType != TILE_OBJECT_TYPE.HUMAN_MEAT && foodPile.tileObjectType != TILE_OBJECT_TYPE.ELF_MEAT)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void PopulateCropsThatAreNotRipe(List<TileObject> tileObjects)
	{
		PopulateCropsThatAreNotRipe(TILE_OBJECT_TYPE.CORN_CROP, tileObjects);
		PopulateCropsThatAreNotRipe(TILE_OBJECT_TYPE.HYPNO_HERB_CROP, tileObjects);
		PopulateCropsThatAreNotRipe(TILE_OBJECT_TYPE.ICEBERRY_CROP, tileObjects);
		PopulateCropsThatAreNotRipe(TILE_OBJECT_TYPE.PINEAPPLE_CROP, tileObjects);
		PopulateCropsThatAreNotRipe(TILE_OBJECT_TYPE.POTATO_CROP, tileObjects);
	}

	private void PopulateCropsThatAreNotRipe(TILE_OBJECT_TYPE p_type, List<TileObject> p_tileObjects)
	{
		if (!groupedTileObjects.ContainsKey(p_type))
		{
			return;
		}
		List<TileObject> list = groupedTileObjects[p_type];
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			TileObject tileObject = list[i];
			if (tileObject is Crops { currentGrowthState: not Crops.Growth_State.Ripe })
			{
				p_tileObjects.Add(tileObject);
			}
		}
	}

	public void PopulateTileObjectsOfType<T>(List<TileObject> objs) where T : TileObject
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T item)
			{
				objs.Add(item);
			}
		}
	}

	public void PopulateBuiltTileObjectsOfType<T>(List<TileObject> objs) where T : TileObject
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T val && val.mapObjectState == MAP_OBJECT_STATE.BUILT)
			{
				objs.Add(val);
			}
		}
	}

	public void PopulateTileObjectsOfType(List<TileObject> objs, TILE_OBJECT_TYPE p_type)
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is TileObject tileObject && tileObject.tileObjectType == p_type)
			{
				objs.Add(tileObject);
			}
		}
	}

	public void PopulateValidTileObjectsForPoisonNeighbour<T>(List<TileObject> objs, Character p_actor) where T : TileObject
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T val && val.gridTileLocation != null && (bool)val.mapVisual && val.advertisedActions.Contains(INTERACTION_TYPE.EAT))
			{
				Poisoned traitOrStatus = val.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
				if (traitOrStatus?.responsibleCharacters == null || !traitOrStatus.responsibleCharacters.Contains(p_actor))
				{
					objs.Add(val);
				}
			}
		}
	}

	public void PopulateTileObjectsOfTypeThatIsBlockWallValidForOreVein(List<LocationGridTile> p_tiles, MapGenerationData p_data)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			LocationGridTile locationGridTile = tiles.ElementAt(i);
			if (p_data.GetGeneratedObjectOnTile(locationGridTile) == TILE_OBJECT_TYPE.BLOCK_WALL)
			{
				int countOfNeighboursInStructureType = locationGridTile.GetCountOfNeighboursInStructureType(STRUCTURE_TYPE.CAVE);
				if ((countOfNeighboursInStructureType == 2 || countOfNeighboursInStructureType == 5) && locationGridTile.GetCountOfNeighboursInStructureType(STRUCTURE_TYPE.WILDERNESS) >= 3)
				{
					p_tiles.Add(locationGridTile);
				}
			}
		}
	}

	public void PopulateTileObjectsOfTypeThatIsBlockWallValidForOreVein2<T>(List<T> objs) where T : TileObject
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (!(pointsOfInterest.ElementAt(i) is T val) || val.gridTileLocation == null)
			{
				continue;
			}
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			val.gridTileLocation.PopulateFourNeighboursThatHasTileObjectOfType(list, TILE_OBJECT_TYPE.BLOCK_WALL);
			int countOfFourNeighboursInStructureType = val.gridTileLocation.GetCountOfFourNeighboursInStructureType(STRUCTURE_TYPE.WILDERNESS);
			if (list.Count == 2 && countOfFourNeighboursInStructureType == 1)
			{
				List<GridNeighbourDirection> list2 = RuinarchListPool<GridNeighbourDirection>.Claim();
				for (int j = 0; j < list.Count; j++)
				{
					LocationGridTile tile = list[j];
					val.gridTileLocation.TryGetNeighbourDirection(tile, out var dir);
					list2.Add(dir);
				}
				if ((list2[0] == GridNeighbourDirection.North && list2[1] == GridNeighbourDirection.South) || (list2[0] == GridNeighbourDirection.South && list2[1] == GridNeighbourDirection.North) || (list2[0] == GridNeighbourDirection.East && list2[1] == GridNeighbourDirection.West) || (list2[0] == GridNeighbourDirection.West && list2[1] == GridNeighbourDirection.East))
				{
					objs.Add(val);
				}
				RuinarchListPool<GridNeighbourDirection>.Release(list2);
			}
			else if (list.Count == 3 && countOfFourNeighboursInStructureType == 1 && val.gridTileLocation.GetCountOfNeighboursThatHasTileObjectOfType(TILE_OBJECT_TYPE.BLOCK_WALL) == 5 && val.gridTileLocation.GetCountOfNeighboursInStructureType(STRUCTURE_TYPE.WILDERNESS) == 3)
			{
				objs.Add(val);
			}
			RuinarchListPool<LocationGridTile>.Release(list);
		}
	}

	public void PopulateBuiltTileObjects(List<TileObject> p_tileObjects)
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is TileObject { mapObjectState: MAP_OBJECT_STATE.BUILT } tileObject && tileObject.OccupiesTile() && !tileObject.isHidden)
			{
				p_tileObjects.Add(tileObject);
			}
		}
	}

	public void PopulateBuildingTileObjectsOfType(List<TileObject> p_tileObjects, TILE_OBJECT_TYPE p_type)
	{
		if (!groupedTileObjects.ContainsKey(p_type))
		{
			return;
		}
		List<TileObject> list = groupedTileObjects[p_type];
		for (int i = 0; i < list.Count; i++)
		{
			TileObject tileObject = list[i];
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILDING)
			{
				p_tileObjects.Add(tileObject);
			}
		}
	}

	public void PopulateBuiltTileObjectsThatHaveTrait(List<TileObject> p_tileObjects, string p_trait)
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is TileObject { mapObjectState: MAP_OBJECT_STATE.BUILT } tileObject && tileObject.OccupiesTile() && tileObject.traitContainer.HasTrait(p_trait))
			{
				p_tileObjects.Add(tileObject);
			}
		}
	}

	public void PopulateBuiltTileObjectsThatHaveTrait<T>(List<TileObject> p_tileObjects, string p_trait) where T : TileObject
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T val && val.mapObjectState == MAP_OBJECT_STATE.BUILT && val.OccupiesTile() && val.traitContainer.HasTrait(p_trait))
			{
				p_tileObjects.Add(val);
			}
		}
	}

	public T GetRandomTileObjectOfTypeThatHasTileLocation<T>() where T : TileObject
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T val && val.gridTileLocation != null)
			{
				list.Add(val);
			}
		}
		T result = null;
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)] as T;
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public T GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuilt<T>() where T : TileObject
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T val && val.mapObjectState == MAP_OBJECT_STATE.BUILT && val.gridTileLocation != null)
			{
				list.Add(val);
			}
		}
		T result = null;
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)] as T;
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public T GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsUnowned<T>(Character p_owner) where T : TileObject
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T val && val.mapObjectState == MAP_OBJECT_STATE.BUILT && (val.characterOwner == null || val.characterOwner == p_owner) && val.gridTileLocation != null)
			{
				list.Add(val);
			}
		}
		T result = null;
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)] as T;
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public T GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombine<T>() where T : TileObject
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T val && val.mapObjectState == MAP_OBJECT_STATE.BUILT && val.characterOwner == null && val.gridTileLocation != null && !val.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				list.Add(val);
			}
		}
		T result = null;
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)] as T;
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public T GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombineAndHasTrait<T>(string traitName) where T : TileObject
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T val && val.traitContainer.HasTrait(traitName) && val.mapObjectState == MAP_OBJECT_STATE.BUILT && val.characterOwner == null && val.gridTileLocation != null && !val.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				list.Add(val);
			}
		}
		T result = null;
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)] as T;
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public T GetFirstTileObjectOfTypeThatIsAvailable<T>() where T : TileObject
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T val && val.IsAvailable())
			{
				return val;
			}
		}
		return null;
	}

	public Bed GetFirstBuiltBedThatIsAvailableAndNoActiveUsers()
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is Bed { mapObjectState: MAP_OBJECT_STATE.BUILT } bed && bed.IsAvailable() && bed.GetActiveUserCount() == 0)
			{
				return bed;
			}
		}
		return null;
	}

	public BedClinic GetFirstBedClinicThatCanBeUsedBy(Character p_character)
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is BedClinic bedClinic && bedClinic.CanUseBed(p_character))
			{
				return bedClinic;
			}
		}
		return null;
	}

	public T GetFirstTileObjectOfTypeWithName<T>(string p_name) where T : TileObject
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T val && val.name == p_name)
			{
				return val;
			}
		}
		return null;
	}

	public T GetFirstTileObjectOfType<T>(TILE_OBJECT_TYPE type, TileObject exception = null) where T : TileObject
	{
		if (groupedTileObjects.ContainsKey(type))
		{
			List<TileObject> list = groupedTileObjects[type];
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					TileObject tileObject = list[i];
					if ((exception == null || exception != tileObject) && tileObject is T result)
					{
						return result;
					}
				}
			}
		}
		return null;
	}

	public T GetFirstBuiltTileObjectOfType<T>(TILE_OBJECT_TYPE type, TileObject exception = null) where T : TileObject
	{
		if (groupedTileObjects.ContainsKey(type))
		{
			List<TileObject> list = groupedTileObjects[type];
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					TileObject tileObject = list[i];
					if ((exception == null || exception != tileObject) && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT && tileObject is T result)
					{
						return result;
					}
				}
			}
		}
		return null;
	}

	public T GetFirstTileObjectOfType<T>(TILE_OBJECT_TYPE type1, TILE_OBJECT_TYPE type2, TILE_OBJECT_TYPE type3, TILE_OBJECT_TYPE type4) where T : TileObject
	{
		T firstTileObjectOfType = GetFirstTileObjectOfType<T>(type1);
		if (firstTileObjectOfType == null)
		{
			firstTileObjectOfType = GetFirstTileObjectOfType<T>(type2);
			if (firstTileObjectOfType == null)
			{
				firstTileObjectOfType = GetFirstTileObjectOfType<T>(type3);
				if (firstTileObjectOfType == null)
				{
					firstTileObjectOfType = GetFirstTileObjectOfType<T>(type4);
				}
			}
		}
		return firstTileObjectOfType;
	}

	public T GetTileObjectOfType<T>() where T : TileObject
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T result)
			{
				return result;
			}
		}
		return null;
	}

	public bool AnyBuiltTileObjectsOfType(TILE_OBJECT_TYPE tileObjectType)
	{
		if (groupedTileObjects.ContainsKey(tileObjectType))
		{
			List<TileObject> list = groupedTileObjects[tileObjectType];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].mapObjectState == MAP_OBJECT_STATE.BUILT)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AnyUnbuiltTileObjectsOfType(TILE_OBJECT_TYPE tileObjectType)
	{
		if (groupedTileObjects.ContainsKey(tileObjectType))
		{
			List<TileObject> list = groupedTileObjects[tileObjectType];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].mapObjectState == MAP_OBJECT_STATE.UNBUILT)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetNumberOfTileObjects(TILE_OBJECT_TYPE type)
	{
		if (groupedTileObjects.ContainsKey(type))
		{
			List<TileObject> list = groupedTileObjects[type];
			if (list != null)
			{
				return list.Count;
			}
		}
		return 0;
	}

	public int GetNumberOfBuiltTileObjects(TILE_OBJECT_TYPE type)
	{
		int num = 0;
		if (groupedTileObjects.ContainsKey(type))
		{
			List<TileObject> list = groupedTileObjects[type];
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].mapObjectState == MAP_OBJECT_STATE.BUILT)
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	public T GetResourcePileObjectWithLowestCount<T>(bool excludeMaximum = true) where T : ResourcePile
	{
		T val = null;
		int num = 0;
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T val2 && (!excludeMaximum || !val2.resourceStorageComponent.IsAtMaxResource(val2.providedResource)) && (val == null || val2.resourceInPile <= num))
			{
				val = val2;
				num = val2.resourceInPile;
			}
		}
		return val;
	}

	public T GetResourcePileObjectWithEnoughAmount<T>(int minimumAmount) where T : ResourcePile
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T val && val.resourceInPile >= minimumAmount)
			{
				return val;
			}
		}
		return null;
	}

	public void PopulateResourcesInStructure<T>(List<T> p_foundResources, RESOURCE p_neededResource) where T : ResourcePile
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is T val && val.providedResource == p_neededResource && val.mapObjectState == MAP_OBJECT_STATE.BUILT)
			{
				p_foundResources.Add(val);
			}
		}
	}

	public int GetTotalResourceInStructure(RESOURCE p_resource)
	{
		int num = 0;
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		PopulateTileObjectsOfType<ResourcePile>(list);
		for (int i = 0; i < list.Count; i++)
		{
			TileObject tileObject = list[i];
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT)
			{
				num += tileObject.resourceStorageComponent.GetResourceValue(p_resource);
			}
		}
		RuinarchListPool<TileObject>.Release(list);
		return num;
	}

	public ResourcePile GetResourcePileObjectWithLowestCount(TILE_OBJECT_TYPE tileObjectType, bool excludeMaximum = true)
	{
		ResourcePile resourcePile = null;
		int num = 0;
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) is ResourcePile resourcePile2 && resourcePile2.tileObjectType == tileObjectType && (!excludeMaximum || !resourcePile2.resourceStorageComponent.IsAtMaxResource(resourcePile2.providedResource)) && (resourcePile == null || resourcePile2.resourceInPile <= num))
			{
				resourcePile = resourcePile2;
				num = resourcePile2.resourceInPile;
			}
		}
		return resourcePile;
	}

	public void PopulateTileObjectsListWithAllTileObjects(List<TileObject> p_tileObjects)
	{
		foreach (KeyValuePair<TILE_OBJECT_TYPE, List<TileObject>> groupedTileObject in groupedTileObjects)
		{
			p_tileObjects.AddRange(groupedTileObject.Value);
		}
	}

	public virtual bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null)
	{
		if (hasBeenDestroyed || hasBeenCleanedUp)
		{
			return false;
		}
		if (!pointsOfInterest.Contains(poi))
		{
			pointsOfInterest.Add(poi);
			if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
			{
				TileObject tileObject = poi as TileObject;
				if (!PlaceAreaObjectAtAppropriateTile(tileObject, tileLocation))
				{
					pointsOfInterest.Remove(poi);
					return false;
				}
				if (groupedTileObjects.ContainsKey(tileObject.tileObjectType))
				{
					groupedTileObjects[tileObject.tileObjectType].Add(tileObject);
				}
				else
				{
					groupedTileObjects.Add(tileObject.tileObjectType, new List<TileObject>(50) { tileObject });
				}
				if (tileObject.gridTileLocation != null)
				{
					List<BaseSettlement> settlementsOnArea = tileObject.gridTileLocation.area.settlementsOnArea;
					for (int i = 0; i < settlementsOnArea.Count; i++)
					{
						if (settlementsOnArea[i] is NPCSettlement nPCSettlement)
						{
							nPCSettlement.OnItemAddedToLocation(tileObject, this);
						}
					}
				}
				if (structureType != STRUCTURE_TYPE.WILDERNESS && tileObject.gridTileLocation != null)
				{
					if (tileObject is MonsterSpawner)
					{
						Messenger.Broadcast(UISignals.UPDATE_STRUCTURE_EXTRA_INFO, this);
						Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
					}
					else if (tileObject is FoodPile foodPile && GetTileObjectsOfType(foodPile.tileObjectType).Count == 1)
					{
						differentFoodPileKindsInDwelling++;
					}
					for (int j = 0; j < residents.Count; j++)
					{
						Character character = residents[j];
						character.eventDispatcher.ExecuteObjectPlacedInCharactersHome(character, this, tileObject);
						character.tileObjectComponent.OnPlacedTileObjectInHomeStructure(this, tileObject);
					}
				}
			}
			return true;
		}
		return false;
	}

	public void OnlyAddPOIToList(IPointOfInterest p_poi)
	{
		if (pointsOfInterest.Contains(p_poi))
		{
			return;
		}
		pointsOfInterest.Add(p_poi);
		if (p_poi is TileObject tileObject)
		{
			if (groupedTileObjects.ContainsKey(tileObject.tileObjectType))
			{
				groupedTileObjects[tileObject.tileObjectType].Add(tileObject);
				return;
			}
			groupedTileObjects.Add(tileObject.tileObjectType, new List<TileObject>(50) { tileObject });
		}
	}

	private bool HasPOI(IPointOfInterest p_poi)
	{
		for (int i = 0; i < pointsOfInterest.Count; i++)
		{
			if (pointsOfInterest.ElementAt(i) == p_poi)
			{
				return true;
			}
		}
		return false;
	}

	public void OnlyRemovePOIFromList(IPointOfInterest p_poi)
	{
		if (pointsOfInterest.Remove(p_poi) && p_poi is TileObject tileObject && groupedTileObjects.ContainsKey(tileObject.tileObjectType))
		{
			groupedTileObjects[tileObject.tileObjectType].Remove(tileObject);
		}
	}

	public virtual bool LoadPOI(TileObject poi, LocationGridTile tileLocation)
	{
		if (!pointsOfInterest.Contains(poi))
		{
			pointsOfInterest.Add(poi);
			if (poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				region.innerMap.LoadObject(poi, tileLocation);
			}
			if (poi != null)
			{
				if (groupedTileObjects.ContainsKey(poi.tileObjectType))
				{
					groupedTileObjects[poi.tileObjectType].Add(poi);
				}
				else
				{
					groupedTileObjects.Add(poi.tileObjectType, new List<TileObject>(50) { poi });
				}
			}
			return true;
		}
		return false;
	}

	public virtual bool RemovePOI(IPointOfInterest poi, Character removedBy = null, bool isPlayerSource = false)
	{
		LocationGridTile gridTileLocation = poi.gridTileLocation;
		TileObject tileObject = poi as TileObject;
		if (tileObject != null && tileObject.isHidden)
		{
			if (pointsOfInterest.Remove(poi))
			{
				groupedTileObjects[tileObject.tileObjectType].Remove(tileObject);
			}
			if (gridTileLocation != null)
			{
				if (gridTileLocation.tileObjectComponent.hiddenObjHere == tileObject)
				{
					gridTileLocation.tileObjectComponent.RemoveHiddenObjectHere(removedBy);
					return true;
				}
				if (!tileObject.OccupiesTile())
				{
					gridTileLocation.tileObjectComponent.RemoveHiddenObjectThatDoesntOccupyTile(tileObject);
					return true;
				}
			}
		}
		else if (pointsOfInterest.Remove(poi))
		{
			if (tileObject != null)
			{
				groupedTileObjects[tileObject.tileObjectType].Remove(tileObject);
			}
			if (gridTileLocation != null)
			{
				if (poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER)
				{
					region.innerMap.RemoveObject(gridTileLocation, removedBy, isPlayerSource);
				}
				if (tileObject != null)
				{
					List<BaseSettlement> settlementsOnArea = gridTileLocation.area.settlementsOnArea;
					for (int i = 0; i < settlementsOnArea.Count; i++)
					{
						if (settlementsOnArea[i] is NPCSettlement nPCSettlement)
						{
							nPCSettlement.OnItemRemovedFromLocation(tileObject, this, gridTileLocation);
						}
					}
				}
				if (structureType != STRUCTURE_TYPE.WILDERNESS)
				{
					if (tileObject is MonsterSpawner)
					{
						Messenger.Broadcast(UISignals.UPDATE_STRUCTURE_EXTRA_INFO, this);
						Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
					}
					else if (tileObject is FoodPile foodPile && GetTileObjectsOfType(foodPile.tileObjectType).Count <= 0)
					{
						differentFoodPileKindsInDwelling--;
					}
					for (int j = 0; j < residents.Count; j++)
					{
						Character character = residents[j];
						character.eventDispatcher.ExecuteObjectRemovedFromCharactersHome(character, this, tileObject);
					}
				}
			}
			return true;
		}
		return false;
	}

	public virtual bool RemovePOIWithoutDestroying(IPointOfInterest poi)
	{
		if (pointsOfInterest.Remove(poi))
		{
			if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
			{
				TileObject tileObject = poi as TileObject;
				groupedTileObjects[tileObject.tileObjectType].Remove(tileObject);
				if (structureType != STRUCTURE_TYPE.WILDERNESS)
				{
					if (tileObject is MonsterSpawner)
					{
						Messenger.Broadcast(UISignals.UPDATE_STRUCTURE_EXTRA_INFO, this);
						Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
					}
					else if (tileObject is FoodPile foodPile && GetTileObjectsOfType(foodPile.tileObjectType).Count <= 0)
					{
						differentFoodPileKindsInDwelling--;
					}
					for (int i = 0; i < residents.Count; i++)
					{
						Character character = residents[i];
						character.eventDispatcher.ExecuteObjectRemovedFromCharactersHome(character, this, tileObject);
					}
				}
			}
			if (poi.gridTileLocation != null && poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				region.innerMap.RemoveObjectWithoutDestroying(poi.gridTileLocation);
			}
			return true;
		}
		return false;
	}

	public virtual bool RemovePOIDestroyVisualOnly(IPointOfInterest poi, Character remover = null)
	{
		if (pointsOfInterest.Remove(poi))
		{
			if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
			{
				TileObject tileObject = poi as TileObject;
				groupedTileObjects[tileObject.tileObjectType].Remove(tileObject);
				LocationGridTile gridTileLocation = poi.gridTileLocation;
				List<BaseSettlement> settlementsOnArea = gridTileLocation.area.settlementsOnArea;
				for (int i = 0; i < settlementsOnArea.Count; i++)
				{
					if (settlementsOnArea[i] is NPCSettlement nPCSettlement)
					{
						nPCSettlement.OnItemRemovedFromLocation(tileObject, this, gridTileLocation);
					}
				}
				if (structureType != STRUCTURE_TYPE.WILDERNESS)
				{
					if (tileObject is MonsterSpawner)
					{
						Messenger.Broadcast(UISignals.UPDATE_STRUCTURE_EXTRA_INFO, this);
						Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
					}
					else if (tileObject is FoodPile foodPile && GetTileObjectsOfType(foodPile.tileObjectType).Count <= 0)
					{
						differentFoodPileKindsInDwelling--;
					}
					for (int j = 0; j < residents.Count; j++)
					{
						Character character = residents[j];
						character.eventDispatcher.ExecuteObjectRemovedFromCharactersHome(character, this, tileObject);
					}
				}
			}
			if (poi.gridTileLocation != null && poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				region.innerMap.RemoveObjectDestroyVisualOnly(poi.gridTileLocation, remover);
			}
			return true;
		}
		return false;
	}

	private bool PlaceAreaObjectAtAppropriateTile(TileObject poi, LocationGridTile tile)
	{
		if (tile != null)
		{
			region.innerMap.PlaceObject(poi, tile);
			return true;
		}
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		PopulateValidTilesToPlace(list, poi);
		LocationGridTile locationGridTile = null;
		if (list.Count > 0)
		{
			locationGridTile = list[UnityEngine.Random.Range(0, list.Count)];
		}
		else if (HasUnoccupiedTile())
		{
			locationGridTile = unoccupiedTiles[UnityEngine.Random.Range(0, unoccupiedTiles.Count)];
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		if (locationGridTile != null)
		{
			region.innerMap.PlaceObject(poi, locationGridTile);
			return true;
		}
		return false;
	}

	private void PopulateValidTilesToPlace(List<LocationGridTile> tiles, IPointOfInterest poi)
	{
		if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
		{
			if (poi is MagicCircle)
			{
				for (int i = 0; i < unoccupiedTiles.Count; i++)
				{
					LocationGridTile locationGridTile = unoccupiedTiles[i];
					if (locationGridTile.HasUnoccupiedNeighbour() && locationGridTile.groundType != LocationGridTile.Ground_Type.Cave && !locationGridTile.IsWater() && locationGridTile.area.elevationType == ELEVATION.PLAIN && !locationGridTile.HasNeighbourOfType(LocationGridTile.Tile_Type.Wall) && !locationGridTile.HasNeighbourOfType(LocationGridTile.Ground_Type.Cave) && !locationGridTile.HasNeighbourThatIsWater() && !locationGridTile.HasNeighbourOfElevation(ELEVATION.MOUNTAIN) && !locationGridTile.HasNeighbourOfElevation(ELEVATION.WATER))
					{
						tiles.Add(locationGridTile);
					}
				}
			}
			else if (poi is WaterWell)
			{
				for (int j = 0; j < unoccupiedTiles.Count; j++)
				{
					LocationGridTile locationGridTile2 = unoccupiedTiles[j];
					if (locationGridTile2.HasUnoccupiedNeighbour() && !locationGridTile2.HasNeighbouringWalledStructure())
					{
						List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
						locationGridTile2.PopulateTilesInRadius(list, 3);
						if (!list.Any((LocationGridTile y) => y.tileObjectComponent.objHere is WaterWell))
						{
							tiles.Add(locationGridTile2);
						}
						RuinarchListPool<LocationGridTile>.Release(list);
					}
				}
			}
			else if (poi is GoddessStatue)
			{
				for (int num = 0; num < unoccupiedTiles.Count; num++)
				{
					LocationGridTile locationGridTile3 = unoccupiedTiles[num];
					if (locationGridTile3.HasUnoccupiedNeighbour() && !locationGridTile3.HasNeighbouringWalledStructure())
					{
						List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
						locationGridTile3.PopulateTilesInRadius(list2, 3);
						if (!list2.Any((LocationGridTile y) => y.tileObjectComponent.objHere is GoddessStatue))
						{
							tiles.Add(locationGridTile3);
						}
						RuinarchListPool<LocationGridTile>.Release(list2);
					}
				}
			}
			else if (poi is TreasureChest || poi is ElementalCrystal)
			{
				for (int num2 = 0; num2 < unoccupiedTiles.Count; num2++)
				{
					LocationGridTile locationGridTile4 = unoccupiedTiles[num2];
					if (!locationGridTile4.IsPartOfSettlement())
					{
						tiles.Add(locationGridTile4);
					}
				}
			}
			else
			{
				if (!(poi is Guitar) && !(poi is Bed) && !(poi is Table))
				{
					return;
				}
				List<LocationGridTile> list3 = RuinarchListPool<LocationGridTile>.Claim();
				PopulateEdgeTiles(list3);
				for (int num3 = 0; num3 < list3.Count; num3++)
				{
					LocationGridTile locationGridTile5 = list3[num3];
					if (locationGridTile5.tileState == LocationGridTile.Tile_State.Empty)
					{
						tiles.Add(locationGridTile5);
					}
				}
				RuinarchListPool<LocationGridTile>.Release(list3);
			}
			return;
		}
		for (int num4 = 0; num4 < unoccupiedTiles.Count; num4++)
		{
			LocationGridTile locationGridTile6 = unoccupiedTiles[num4];
			if (!locationGridTile6.IsAdjacentTo(typeof(MagicCircle)))
			{
				tiles.Add(locationGridTile6);
			}
		}
	}

	protected virtual void OnTileAddedToStructure(LocationGridTile tile)
	{
		if (structureType != STRUCTURE_TYPE.WILDERNESS)
		{
			tile.area.structureComponent.AddStructureInArea(this);
		}
	}

	protected virtual void OnTileRemovedFromStructure(LocationGridTile tile, LocationStructure removedFrom)
	{
		if (structureType != STRUCTURE_TYPE.WILDERNESS && !HasTileOnArea(tile.area))
		{
			tile.area.structureComponent.RemoveStructureInArea(this);
		}
	}

	public void AddTile(LocationGridTile tile)
	{
		if (!tiles.Contains(tile))
		{
			tiles.Add(tile);
			if (tile.tileState == LocationGridTile.Tile_State.Empty)
			{
				AddUnoccupiedTile(tile);
			}
			else
			{
				RemoveUnoccupiedTile(tile);
			}
			if (tile.IsPassable())
			{
				AddPassableTile(tile);
			}
			else
			{
				RemovePassableTile(tile);
			}
			if (structureType != STRUCTURE_TYPE.WILDERNESS)
			{
				AddOccupiedAreaVote(tile.area);
			}
			OnTileAddedToStructure(tile);
		}
	}

	public void RemoveTile(LocationGridTile tile)
	{
		if (tiles.Remove(tile))
		{
			if (structureType != STRUCTURE_TYPE.WILDERNESS)
			{
				ReduceOccupiedAreaVote(tile.area);
			}
			OnTileRemovedFromStructure(tile, this);
		}
		RemovePassableTile(tile);
		RemoveUnoccupiedTile(tile);
	}

	public void AddPassableTile(LocationGridTile tile)
	{
		passableTiles.Add(tile);
	}

	public void RemovePassableTile(LocationGridTile tile)
	{
		passableTiles.Remove(tile);
	}

	public void AddUnoccupiedTile(LocationGridTile tile)
	{
		unoccupiedTiles.Add(tile);
	}

	public void RemoveUnoccupiedTile(LocationGridTile tile)
	{
		unoccupiedTiles.Remove(tile);
	}

	public bool HasUnoccupiedTile()
	{
		return unoccupiedTiles.Count > 0;
	}

	public LocationGridTile GetRandomTile()
	{
		if (tiles.Count <= 0)
		{
			return null;
		}
		return CollectionUtilities.GetRandomElement(tiles);
	}

	public LocationGridTile GetRandomPassableTile()
	{
		if (passableTiles.Count <= 0)
		{
			return null;
		}
		return passableTiles[Utilities.Rng.Next(0, passableTiles.Count)];
	}

	public LocationGridTile GetRandomPassableTileThatIsNotOccupied()
	{
		if (passableTiles.Count <= 0)
		{
			return null;
		}
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < passableTiles.Count; i++)
		{
			LocationGridTile locationGridTile = passableTiles[i];
			if (!locationGridTile.isOccupied && locationGridTile.tileObjectComponent.objHere == null)
			{
				list.Add(locationGridTile);
			}
		}
		LocationGridTile result = null;
		if (list.Count > 0)
		{
			result = list[Utilities.Rng.Next(0, list.Count)];
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomPassableTileThatIsNotPartOfSettlement()
	{
		if (passableTiles.Count <= 0)
		{
			return null;
		}
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < passableTiles.Count; i++)
		{
			LocationGridTile locationGridTile = passableTiles[i];
			if (!locationGridTile.IsPartOfSettlement())
			{
				list.Add(locationGridTile);
			}
		}
		LocationGridTile result = null;
		if (list.Count > 0)
		{
			result = list[Utilities.Rng.Next(0, list.Count)];
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomUnoccupiedTile()
	{
		if (!HasUnoccupiedTile())
		{
			return null;
		}
		return unoccupiedTiles[UnityEngine.Random.Range(0, unoccupiedTiles.Count)];
	}

	public LocationGridTile GetRandomUnoccupiedTileThatHasNoCharacters()
	{
		LocationGridTile result = null;
		if (HasUnoccupiedTile())
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			for (int i = 0; i < unoccupiedTiles.Count; i++)
			{
				LocationGridTile locationGridTile = unoccupiedTiles[i];
				if (locationGridTile.charactersHere.Count <= 0 && locationGridTile.IsPassable() && locationGridTile.HasWalkableNode())
				{
					list.Add(locationGridTile);
				}
			}
			if (list.Count > 0)
			{
				result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
			}
			RuinarchListPool<LocationGridTile>.Release(list);
		}
		return result;
	}

	public LocationGridTile GetFirstTileWithObject()
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			LocationGridTile locationGridTile = tiles.ElementAt(i);
			if (locationGridTile.tileObjectComponent.objHere != null)
			{
				return locationGridTile;
			}
		}
		return null;
	}

	public virtual bool DoesTileContributeToDamage(LocationGridTile tile)
	{
		return false;
	}

	public virtual void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource)
	{
	}

	public virtual void OnTileRepaired(LocationGridTile tile, int amount)
	{
	}

	private void AddOccupiedAreaVote(Area p_area)
	{
		if (!occupiedAreas.ContainsKey(p_area))
		{
			occupiedAreas.Add(p_area, 0);
		}
		occupiedAreas[p_area]++;
	}

	private void ReduceOccupiedAreaVote(Area p_area)
	{
		if (occupiedAreas.ContainsKey(p_area))
		{
			occupiedAreas[p_area]--;
			if (occupiedAreas[p_area] <= 0)
			{
				occupiedAreas.Remove(p_area);
			}
		}
	}

	public virtual LocationGridTile GetCenterTile()
	{
		return CollectionUtilities.GetRandomElement(tiles);
	}

	public void SetOccupiedArea(Area p_area)
	{
		occupiedArea = p_area;
	}

	private void OnClickStructure()
	{
		Selector.Instance.Select(this);
	}

	protected virtual void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false, bool shouldBeCleanedUp = true)
	{
		if (hasBeenDestroyed)
		{
			return;
		}
		hasBeenDestroyed = true;
		Messenger.Broadcast(StructureSignals.BEFORE_STRUCTURE_DESTROYED, this);
		SetOccupiedArea(null);
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		list.AddRange(tiles);
		LocationStructure wilderness = region.wilderness;
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			locationGridTile.tileObjectComponent.ClearWallObjects();
			IPointOfInterest objHere = locationGridTile.tileObjectComponent.objHere;
			if (objHere != null)
			{
				if (objHere is TileObject tileObject && tileObject.traitContainer.HasTrait("Indestructible"))
				{
					if (tileObject.isPreplaced)
					{
						LocationStructureObject locationStructureObject = null;
						if (this is DemonicStructure demonicStructure)
						{
							locationStructureObject = demonicStructure.structureObj;
						}
						else if (this is ManMadeStructure manMadeStructure)
						{
							locationStructureObject = manMadeStructure.structureObj;
						}
						else if (this is NaturalStructureWithStructureObject naturalStructureWithStructureObject)
						{
							locationStructureObject = naturalStructureWithStructureObject.structureObj;
						}
						if (locationStructureObject != null)
						{
							if (locationStructureObject.HasPreplacedObjectOfType(tileObject.tileObjectType))
							{
								objHere.gridTileLocation?.structure.RemovePOI(objHere);
							}
							else
							{
								OnlyRemovePOIFromList(tileObject);
								wilderness.OnlyAddPOIToList(tileObject);
							}
						}
					}
					else
					{
						OnlyRemovePOIFromList(tileObject);
						wilderness.OnlyAddPOIToList(tileObject);
					}
				}
				else
				{
					objHere.gridTileLocation?.structure.RemovePOI(objHere);
				}
			}
			locationGridTile.SetStructure(wilderness);
			locationGridTile.SetTileType(LocationGridTile.Tile_Type.Empty);
			if (locationGridTile.groundType.IsStructureType())
			{
				locationGridTile.tileObjectComponent.genericTileObject.AdjustHP(-locationGridTile.tileObjectComponent.genericTileObject.maxHP, ELEMENTAL_TYPE.Normal);
			}
			if (structureType.IsPlayerStructure())
			{
				locationGridTile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.corruptedTile);
			}
			locationGridTile.ScheduleRevertTileToOriginalGround();
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		for (int j = 0; j < charactersHere.Count; j++)
		{
			Character character = charactersHere[j];
			RemoveCharacterAtLocation(character);
			character.gridTileLocation?.structure.AddCharacterAtLocation(character);
		}
		charactersHere.Clear();
		if (rooms != null)
		{
			for (int k = 0; k < rooms.Length; k++)
			{
				rooms[k].OnParentStructureDestroyed();
			}
		}
		if (isPlayerSource)
		{
			PlayerManager.Instance?.player?.retaliationComponent.StructureDestroyedRetaliation(this);
			Messenger.Broadcast(StructureSignals.STRUCTURE_DESTROYED_BY_PLAYER, this, settlementLocation);
		}
		AfterStructureDestruction(p_responsibleCharacter);
		if (shouldBeCleanedUp)
		{
			MarkForCleanup();
		}
	}

	protected virtual void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		BaseSettlement baseSettlement = settlementLocation;
		region.RemoveStructure(this);
		settlementLocation?.RemoveStructure(this);
		Messenger.Broadcast(StructureSignals.STRUCTURE_OBJECT_REMOVED, this, occupiedArea);
		UnsubscribeListeners();
		Messenger.Broadcast(StructureSignals.STRUCTURE_DESTROYED, this);
		if (p_responsibleCharacter != null)
		{
			if (p_responsibleCharacter.partyComponent.hasParty && p_responsibleCharacter.partyComponent.currentParty.currentQuest is DemonRaidPartyQuest demonRaidPartyQuest && demonRaidPartyQuest.targetSettlement == baseSettlement)
			{
				LocationGridTile gridTileLocation = p_responsibleCharacter.gridTileLocation;
				if (gridTileLocation != null)
				{
					int p_amount = 2;
					if (p_responsibleCharacter.partyComponent.currentParty.isPlayerParty)
					{
						if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.MARAUD).TryDecreaseRemainingChaosOrbs(ref p_amount))
						{
							Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, gridTileLocation.centeredWorldLocation, p_amount, gridTileLocation.parentMap);
						}
					}
					else if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.MONSTER_SPAWNER).TryDecreaseRemainingChaosOrbs(ref p_amount))
					{
						Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, gridTileLocation.centeredWorldLocation, p_amount, gridTileLocation.parentMap);
					}
				}
			}
			Messenger.Broadcast(StructureSignals.STRUCTURE_DESTROYED_BY, this, p_responsibleCharacter);
		}
		eventDispatcher?.ExecuteStructureDestroyed(this);
		if (linkedSettlement != null)
		{
			linkedSettlement.structureComponent.RemoveLinkedStructure(this);
		}
		partyStructureComponent?.AfterStructureDestruction(p_responsibleCharacter);
	}

	public void DestroyStructureFromVillageExpiry()
	{
		DestroyStructure();
	}

	public void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		if (structureType != STRUCTURE_TYPE.WILDERNESS && structureType != STRUCTURE_TYPE.OCEAN)
		{
			StructureData structureData = LandmarkManager.Instance.GetStructureData(structureType);
			for (int i = 0; i < structureData.playerActions.Length; i++)
			{
				PLAYER_SKILL_TYPE action = structureData.playerActions[i];
				AddPlayerAction(action, broadcastSignal);
			}
		}
	}

	public void AddPlayerAction(PLAYER_SKILL_TYPE action, bool broadcastSignal = true)
	{
		if (!_actions.Contains(action))
		{
			_actions.Add(action);
			if (broadcastSignal)
			{
				Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_ADDED_TO_TARGET, action, (IPlayerActionTarget)this);
			}
		}
	}

	public void RemovePlayerAction(PLAYER_SKILL_TYPE action, bool broadcastSignal = true)
	{
		if (_actions.Remove(action) && broadcastSignal)
		{
			Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, (IPlayerActionTarget)this);
		}
	}

	public void ClearPlayerActions()
	{
		_actions.Clear();
	}

	public bool IsCurrentlySelected()
	{
		if (UIManager.Instance.structureInfoUI.isShowing)
		{
			return UIManager.Instance.structureInfoUI.activeStructure == this;
		}
		return false;
	}

	public void LeftSelectAction()
	{
		UIManager.Instance.ShowStructureInfo(this);
	}

	public void LeftSelectActionNoCenter()
	{
		UIManager.Instance.ShowStructureInfo(this, centerOnStructure: false);
	}

	public void RightSelectAction()
	{
		if (structureType.IsPlayerStructure() || structureType == STRUCTURE_TYPE.CITY_CENTER || structureType.IsSpecialStructure())
		{
			Vector3 p_followTarget = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(InputManager.Instance.mousePosition);
			IPlayerActionTarget p_target = this;
			if (structureType == STRUCTURE_TYPE.CITY_CENTER)
			{
				p_target = settlementLocation;
			}
			UIManager.Instance.ShowPlayerActionContextMenu(p_target, p_followTarget, p_isScreenPosition: false);
		}
	}

	public void MiddleSelectAction()
	{
	}

	public bool CanBeSelected()
	{
		return true;
	}

	public void AddObjectAsDamageContributor(IDamageable damageable)
	{
		objectsThatContributeToDamage.Add(damageable);
		if (damageable is TileObject tileObject)
		{
			tileObject.SetAsDamageContributorToStructure(p_state: true);
		}
	}

	protected void OnObjectDamaged(TileObject tileObject, int amount, bool isPlayerSource)
	{
		if (objectsThatContributeToDamage.Contains(tileObject))
		{
			AdjustHP(amount, null, isPlayerSource);
			if (!hasBeenDestroyed && !objectsThatContributeToDamage.Any((IDamageable o) => o.currentHP > 0))
			{
				AdjustHP(-currentHP, null, isPlayerSource);
			}
		}
	}

	protected void OnObjectDamagedBy(TileObject tileObject, int amount, Character p_responsibleCharacter, bool isPlayerSource)
	{
		if (objectsThatContributeToDamage.Contains(tileObject))
		{
			AdjustHP(amount, p_responsibleCharacter, isPlayerSource);
			if (!hasBeenDestroyed && !objectsThatContributeToDamage.Any((IDamageable o) => o.currentHP > 0))
			{
				AdjustHP(-currentHP, p_responsibleCharacter, isPlayerSource);
			}
		}
	}

	protected void OnObjectRepaired(TileObject tileObject, int amount)
	{
		if (objectsThatContributeToDamage.Contains(tileObject))
		{
			AdjustHP(amount);
		}
	}

	public void AdjustHP(int amount, Character p_responsibleCharacter = null, bool isPlayerSource = false)
	{
		if (!hasBeenDestroyed)
		{
			currentHP += amount;
			currentHP = Mathf.Clamp(currentHP, 0, maxHP);
			AfterStructureHPAdjusted();
			Messenger.Broadcast(StructureSignals.STRUCTURE_HP_CHANGED, this);
			if (currentHP == 0)
			{
				DestroyStructure(p_responsibleCharacter, isPlayerSource);
			}
		}
	}

	protected virtual void AfterStructureHPAdjusted()
	{
	}

	public void SetMaxHP(int amount)
	{
		maxHP = amount;
	}

	public void SetMaxHPAndReset(int amount)
	{
		SetMaxHP(amount);
		ResetHP();
	}

	public void ResetHP()
	{
		currentHP = maxHP;
	}

	public bool AddResident(Character character)
	{
		if (!residents.Contains(character))
		{
			residents.Add(character);
			character.SetHomeStructure(this);
			OnAddResident(character);
			Messenger.Broadcast(StructureSignals.ADDED_STRUCTURE_RESIDENT, character, this);
			return true;
		}
		return false;
	}

	public void RemoveResident(Character character)
	{
		if (residents.Remove(character))
		{
			character.SetHomeStructure(null);
			OnRemoveResident(character);
			Messenger.Broadcast(StructureSignals.REMOVED_STRUCTURE_RESIDENT, character, this);
		}
	}

	public bool IsResident(Character character)
	{
		return character.homeStructure == this;
	}

	public bool HasPositiveRelationshipWithAnyResident(Character character)
	{
		if (residents.Contains(character))
		{
			return true;
		}
		for (int i = 0; i < residents.Count; i++)
		{
			Character character2 = residents[i];
			if (character.relationshipContainer.GetRelationshipEffectWith(character2) == RELATIONSHIP_EFFECT.POSITIVE)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEnemyOrNoRelationshipWithAnyResident(Character character)
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character2 = residents[i];
			RELATIONSHIP_EFFECT relationshipEffectWith = character.relationshipContainer.GetRelationshipEffectWith(character2);
			if (relationshipEffectWith == RELATIONSHIP_EFFECT.NEGATIVE || relationshipEffectWith == RELATIONSHIP_EFFECT.NONE)
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

	public bool IsOccupied()
	{
		return residents.Count > 0;
	}

	public int GetNumberOfResidentsAndPopulateListExcluding(List<Character> validResidents, Character excludedCharacter)
	{
		if (residents != null)
		{
			int num = 0;
			for (int i = 0; i < residents.Count; i++)
			{
				Character character = residents[i];
				if (excludedCharacter != character)
				{
					num++;
					validResidents.Add(character);
				}
			}
			return num;
		}
		return 0;
	}

	public int GetNumberOfResidentsThatIsNotInStructureType(STRUCTURE_TYPE structureType)
	{
		int num = 0;
		for (int i = 0; i < residents.Count; i++)
		{
			LocationGridTile gridTileLocation = residents[i].gridTileLocation;
			if (gridTileLocation == null || gridTileLocation.structure.structureType != structureType)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfResidentsThatIsAlive()
	{
		int num = 0;
		for (int i = 0; i < residents.Count; i++)
		{
			if (!residents[i].isDead)
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

	public bool HasCloseFriendOrNonEnemyRivalRelativeInSameFaction(Character character)
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character2 = residents[i];
			if (character != character2 && character2.faction == character.faction)
			{
				if (character.relationshipContainer.GetOpinionLabel(character2) == "Close Friend")
				{
					return true;
				}
				if (!character.relationshipContainer.IsEnemiesWith(character2) && character.relationshipContainer.IsFamilyMember(character2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasReachedMaxResidentCapacity()
	{
		return residents.Count >= maxResidentCapacity;
	}

	public bool HasVillagerResidentOfDifferentRace(RACE p_race)
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (character.isNormalCharacter && character.race != p_race)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasVillagerResident()
	{
		for (int i = 0; i < residents.Count; i++)
		{
			if (residents[i].isNormalCharacter)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAliveResident(Character p_exception)
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

	public bool HasResidentForExtermination()
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			if (!character.isDead && character.isMonsterOrRatmanOrUndead && !character.traitContainer.HasTrait("Hibernating", "Indestructible") && (!character.traitContainer.HasTrait("Prisoner") || character.IsAtHome()))
			{
				return true;
			}
		}
		return false;
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

	public void CreateRoomsBasedOnStructureObject(LocationStructureObject structureObject)
	{
		if (structureObject.roomTemplates != null && structureObject.roomTemplates.Length != 0)
		{
			rooms = new StructureRoom[structureObject.roomTemplates.Length];
			for (int i = 0; i < rooms.Length; i++)
			{
				RoomTemplate roomTemplate = structureObject.roomTemplates[i];
				StructureRoom structureRoom = CreteNewRoomForStructure(structureObject.GetTilesOccupiedByRoom(region.innerMap, roomTemplate));
				rooms[i] = structureRoom;
			}
		}
	}

	protected virtual StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom)
	{
		return null;
	}

	public bool IsTilePartOfARoom(LocationGridTile tile, out StructureRoom room)
	{
		if (rooms == null)
		{
			room = null;
			return false;
		}
		for (int i = 0; i < rooms.Length; i++)
		{
			StructureRoom structureRoom = rooms[i];
			if (structureRoom.tilesInRoom.Contains(tile))
			{
				room = structureRoom;
				return true;
			}
		}
		room = null;
		return false;
	}

	public virtual bool IsAtTargetDestination(Character character)
	{
		return character.currentStructure == this;
	}

	public void OnBuiltNewStructureFromBlueprint()
	{
		if (settlementLocation is NPCSettlement nPCSettlement)
		{
			nPCSettlement.OnStructureBuilt(this);
		}
	}

	public virtual string GetTestingInfo()
	{
		string text = name + " Info:";
		text = text + "\nPersistent ID: " + persistentID;
		if (linkedSettlement != null)
		{
			text = text + "\nLinked Settlement: " + linkedSettlement.name;
		}
		if (settlementLocation != null)
		{
			text = text + "\nSettlement Location: " + settlementLocation.name;
			text = text + "\nSettlement Faction Owner: " + settlementLocation.owner?.name;
		}
		text += "\nDamage Contributing Objects:";
		for (int i = 0; i < objectsThatContributeToDamage.Count; i++)
		{
			IDamageable arg = objectsThatContributeToDamage.ElementAt(i);
			text += $"{arg}, ";
		}
		text += "\nPOIs:";
		foreach (IPointOfInterest item in pointsOfInterest)
		{
			text += $"{item}, ";
		}
		text += "\nCharacters Here:";
		foreach (Character item2 in charactersHere)
		{
			text += $"{item2}, ";
		}
		if (partyStructureComponent != null)
		{
			text = text + "\n" + partyStructureComponent.GetTestingInfo();
		}
		return text;
	}

	public bool IsValidForStoreTarget()
	{
		return !hasBeenDestroyed;
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
		return LandmarkManager.Instance.GetStructureData(structureType).structureSprite;
	}

	public void OnSelectBookmark()
	{
		LeftSelectAction();
	}

	public void RemoveBookmark()
	{
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this);
	}

	public void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		UIManager.Instance.ShowStructureNameplateTooltip(this, p_pos);
	}

	public void OnHoverOutBookmarkItem()
	{
		UIManager.Instance.HideStructureNameplateTooltip();
	}

	public void SetLinkedSettlement(NPCSettlement p_settlement)
	{
		linkedSettlement = p_settlement;
	}

	public void LinkThisStructureToAVillage(BaseSettlement exception = null)
	{
		NPCSettlement nPCSettlement = null;
		int num = 0;
		if (this is Cave { connectedMines: not null } cave && cave.connectedMines.Count > 0)
		{
			for (int i = 0; i < cave.connectedMines.Count; i++)
			{
				BaseSettlement baseSettlement = cave.connectedMines[i].settlementLocation;
				if ((exception != null && exception == baseSettlement) || baseSettlement.locationType != LOCATION_TYPE.VILLAGE || !(baseSettlement is NPCSettlement nPCSettlement2) || !baseSettlement.HasResidentThatIsNotDead())
				{
					continue;
				}
				LocationStructure structureForDistance = GetStructureForDistance(nPCSettlement2);
				if (structureForDistance != null)
				{
					int areaDistanceTo = occupiedArea.GetAreaDistanceTo(structureForDistance.occupiedArea);
					if (nPCSettlement == null || areaDistanceTo < num)
					{
						nPCSettlement = nPCSettlement2;
						num = areaDistanceTo;
					}
				}
			}
			nPCSettlement?.structureComponent.AddLinkedStructure(this);
			return;
		}
		for (int j = 0; j < region.settlementsInRegion.Count; j++)
		{
			BaseSettlement baseSettlement2 = region.settlementsInRegion[j];
			if ((exception != null && exception == baseSettlement2) || baseSettlement2.locationType != LOCATION_TYPE.VILLAGE || !(baseSettlement2 is NPCSettlement nPCSettlement3) || !baseSettlement2.HasResidentThatIsNotDead())
			{
				continue;
			}
			LocationStructure structureForDistance2 = GetStructureForDistance(nPCSettlement3);
			if (structureForDistance2 != null)
			{
				int areaDistanceTo2 = occupiedArea.GetAreaDistanceTo(structureForDistance2.occupiedArea);
				if (nPCSettlement == null || areaDistanceTo2 < num)
				{
					nPCSettlement = nPCSettlement3;
					num = areaDistanceTo2;
				}
			}
		}
		nPCSettlement?.structureComponent.AddLinkedStructure(this);
	}

	private LocationStructure GetStructureForDistance(NPCSettlement p_settlement)
	{
		LocationStructure locationStructure = p_settlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
		if (locationStructure == null)
		{
			locationStructure = p_settlement.mainStorage;
		}
		if (locationStructure == null && p_settlement.allStructures.Count > 0)
		{
			for (int i = 0; i < p_settlement.allStructures.Count; i++)
			{
				LocationStructure locationStructure2 = p_settlement.allStructures[i];
				if (locationStructure2.occupiedArea != null && !locationStructure2.hasBeenDestroyed)
				{
					locationStructure = locationStructure2;
					break;
				}
			}
		}
		return locationStructure;
	}

	protected virtual string GetUIString()
	{
		if (string.IsNullOrEmpty(_uiString))
		{
			string text = "Inner_Maps.Location_Structures.LocationStructure" + "|" + persistentID;
			_uiString = "<link=" + text + ">" + name + "</link>";
		}
		return _uiString;
	}

	public virtual bool TryGetMinimapColorForTileInStructure(LocationGridTile p_tile, out Color p_color)
	{
		p_color = Color.white;
		return false;
	}

	public void SetIsProtected(bool p_state)
	{
		if (isProtected != p_state)
		{
			isProtected = p_state;
			OnSetProtected();
		}
	}

	public void SetIsProtected(bool p_state, Character p_protector)
	{
		if (isProtected != p_state)
		{
			isProtected = p_state;
			OnSetProtected(p_protector);
		}
	}

	protected virtual void OnSetProtected()
	{
	}

	protected virtual void OnSetProtected(Character p_protector)
	{
	}

	public void OnLocaleChanged(Locale locale)
	{
		name = GenerateName();
	}

	public void MarkForCleanup()
	{
		LandmarkManager.Instance.AddStructureToBeCleanedUp(this);
	}

	public virtual void CleanUp()
	{
		if (!DatabaseManager.Instance.structureDatabase.HasStructure(persistentID))
		{
			return;
		}
		hasBeenCleanedUp = true;
		partyStructureComponent?.CleanUp();
		DatabaseManager.Instance.structureDatabase.UnregisterStructure(this);
		Messenger.Broadcast(StructureSignals.DISCONNECT_FROM_STRUCTURE, this);
		charactersHere.Clear();
		region = null;
		settlementLocation = null;
		pointsOfInterest.Clear();
		groupedTileObjects.Clear();
		occupiedArea = null;
		linkedSettlement = null;
		tiles?.Clear();
		passableTiles?.Clear();
		unoccupiedTiles?.Clear();
		objectsThatContributeToDamage?.Clear();
		residents?.Clear();
		if (rooms != null)
		{
			for (int i = 0; i < rooms.Length; i++)
			{
				rooms[i].CleanUp();
			}
			rooms = null;
		}
		locationAwareness?.CleanUp();
		eventDispatcher?.CleanUp();
		occupiedAreas?.Clear();
		bookmarkEventDispatcher?.ClearAll();
		_ = ConsoleBase.checkStructureReferences;
	}

	public void OnTileObjectInDwellingSetAsUnbuilt(TileObject p_tileObject)
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			character.eventDispatcher.ExecuteObjectRemovedFromCharactersHome(character, this, p_tileObject);
		}
	}

	public void OnTileObjectInDwellingSetAsBuilt(TileObject p_tileObject)
	{
		for (int i = 0; i < residents.Count; i++)
		{
			Character character = residents[i];
			character.eventDispatcher.ExecuteObjectPlacedInCharactersHome(character, this, p_tileObject);
		}
	}

	private void CreatePartyStructureComponent(STRUCTURE_TYPE p_structureType)
	{
		switch (p_structureType)
		{
		case STRUCTURE_TYPE.KENNEL:
			partyStructureComponent = new KennelPartyStructureComponent(this);
			break;
		case STRUCTURE_TYPE.TORTURE_CHAMBERS:
			partyStructureComponent = new TortureChambersStructureComponent(this);
			break;
		case STRUCTURE_TYPE.MARAUD:
			partyStructureComponent = new MaraudPartyStructureComponent(this);
			break;
		default:
			partyStructureComponent = new PartyStructureComponent(this);
			break;
		}
	}

	public virtual void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		if (rooms != null)
		{
			for (int i = 0; i < rooms.Length; i++)
			{
				rooms[i].CheckIfStructureIsStillReferenced(p_structure);
			}
		}
		partyStructureComponent?.CheckIfStructureIsStillReferenced(p_structure);
	}

	public virtual void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		charactersHere.Contains(p_character);
		pointsOfInterest.Contains(p_character);
		residents.Contains(p_character);
		if (rooms != null)
		{
			for (int i = 0; i < rooms.Length; i++)
			{
				rooms[i].CheckIfCharacterIsStillReferenced(p_character);
			}
		}
		locationAwareness?.CheckIfCharacterIsStillReferenced(p_character);
		partyStructureComponent?.CheckIfCharacterIsStillReferenced(p_character);
	}
}
