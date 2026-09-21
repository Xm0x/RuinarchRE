using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Villager_Wants;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations;
using Locations.Settlements;
using Logs;
using Object_Pools;
using Traits;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UtilityScripts;

public abstract class TileObject : MapObject<TileObject>, IPointOfInterest, ITraitable, IDamageable, ISelectable, ILogFiller, IGCollectable, IPlayerActionTarget, IPartyQuestTarget, IGatheringTarget, ISavable, IStoredTarget, IBookmarkable, LocalizationManagerEventDispatcher.ILocaleChangeListener
{
	private Character[] _users;

	private GameObject slotsParent;

	protected bool hasCreatedSlots;

	private string _uiString;

	public string persistentID { get; protected set; }

	public string name { get; protected set; }

	public string internalName { get; private set; }

	public int id { get; private set; }

	public TILE_OBJECT_TYPE tileObjectType { get; private set; }

	public Character characterOwner { get; protected set; }

	public List<INTERACTION_TYPE> advertisedActions { get; protected set; }

	public bool isPreplaced { get; private set; }

	public bool isStoredAsTarget { get; private set; }

	public bool isDeadReference { get; private set; }

	public bool hasSubscribedToSignals { get; protected set; }

	public bool isBuiltByPlayerBaseBuilding { get; private set; }

	public List<JobQueueItem> allJobsTargetingThis { get; private set; }

	public List<JobQueueItem> allExistingJobsTargetingThis { get; private set; }

	public List<Character> charactersThatAlreadyAssumed { get; private set; }

	public Character isBeingCarriedBy { get; private set; }

	public int maxHP { get; protected set; }

	public int currentHP { get; protected set; }

	private TileObjectSlotItem[] slots { get; set; }

	public virtual LocationGridTile gridTileLocation { get; protected set; }

	public POI_STATE state { get; private set; }

	public LocationGridTile previousTile { get; private set; }

	public List<PLAYER_SKILL_TYPE> actions { get; protected set; }

	public int repairCounter { get; protected set; }

	public int numOfNonSecretActionsBeingPerformedOnThis { get; private set; }

	public ILocationAwareness currentLocationAwareness { get; private set; }

	public bool isDamageContributorToStructure { get; private set; }

	public Character lastStolenBy { get; private set; }

	public LogComponent logComponent { get; protected set; }

	public TileObjectHiddenComponent hiddenComponent { get; private set; }

	public TileObjectEventDispatcher eventDispatcher { get; private set; }

	public TileObjectConstructionComponent constructionComponent { get; private set; }

	public ResourceStorageComponent resourceStorageComponent { get; }

	public TileObjectPartyComponent partyComponent { get; private set; }

	public BookmarkableEventDispatcher bookmarkEventDispatcher { get; private set; }

	public string bookmarkName => iconRichText + " " + name;

	public BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Text_With_Cancel;

	public virtual string nameplateName => name;

	public virtual string description => GetFlavorText();

	public OBJECT_TYPE objectType => OBJECT_TYPE.Tile_Object;

	public STORED_TARGET_TYPE storedTargetType => STORED_TARGET_TYPE.Tile_Objects;

	public bool isTargetted { get; set; }

	public string iconRichText => Utilities.TileObjectIcon();

	public virtual Type serializedData => typeof(SaveDataTileObject);

	public POINT_OF_INTEREST_TYPE poiType => POINT_OF_INTEREST_TYPE.TILE_OBJECT;

	public virtual Vector3 worldPosition => mapVisual.transform.position;

	public virtual Vector2 selectableSize => Vector2Int.one;

	public virtual Vector3 attackRangePosition => worldPosition;

	public bool isDead => gridTileLocation == null;

	public ProjectileReceiver projectileReceiver => mapVisual?.visionTrigger.projectileReceiver;

	public Transform worldObject
	{
		get
		{
			if (!(mapVisual != null))
			{
				return null;
			}
			return mapVisual.transform;
		}
	}

	public string nameWithID => name + " " + id;

	public GameObject visualGO => mapVisual.gameObject;

	public Faction factionOwner => characterOwner?.faction;

	public bool canBeRepaired
	{
		get
		{
			if (repairCounter <= 0)
			{
				return !traitContainer.HasTrait("Burning");
			}
			return false;
		}
	}

	public bool isBeingSeized
	{
		get
		{
			if (PlayerManager.Instance.player != null)
			{
				return PlayerManager.Instance.player.seizeComponent.seizedPOI == this;
			}
			return false;
		}
	}

	public bool isHidden => hiddenComponent.isHidden;

	public LocationStructure currentStructure => gridTileLocation?.structure;

	public Region currentRegion
	{
		get
		{
			if (isBeingCarriedBy != null)
			{
				return isBeingCarriedBy.currentRegion;
			}
			return gridTileLocation?.parentMap.region;
		}
	}

	public LocationStructure structureLocation => gridTileLocation?.structure;

	public BaseSettlement currentSettlement
	{
		get
		{
			BaseSettlement settlement = null;
			gridTileLocation?.IsPartOfSettlement(out settlement);
			return settlement;
		}
	}

	public BaseMapObjectVisual mapObjectVisual => mapVisual;

	public virtual string neutralizer => string.Empty;

	public virtual Character[] users => GetUsers();

	public virtual bool canBeSeized => true;

	public virtual bool canBeDirectlyUnseizedToCharacter => true;

	public string uiString => GetUIString();

	public ITraitContainer traitContainer { get; private set; }

	public TraitProcessor traitProcessor => TraitManager.tileObjectTraitProcessor;

	protected TileObject()
	{
		allJobsTargetingThis = new List<JobQueueItem>();
		allExistingJobsTargetingThis = new List<JobQueueItem>();
		charactersThatAlreadyAssumed = new List<Character>();
		logComponent = new LogComponent();
		hiddenComponent = new TileObjectHiddenComponent();
		eventDispatcher = new TileObjectEventDispatcher();
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		resourceStorageComponent = new ResourceStorageComponent();
		partyComponent = new TileObjectPartyComponent();
		partyComponent.SetOwner(this);
	}

	protected TileObject(SaveDataTileObject data)
	{
		allJobsTargetingThis = new List<JobQueueItem>();
		allExistingJobsTargetingThis = new List<JobQueueItem>();
		charactersThatAlreadyAssumed = new List<Character>();
		advertisedActions = new List<INTERACTION_TYPE>(data.advertisedActions);
		eventDispatcher = new TileObjectEventDispatcher();
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		resourceStorageComponent = data.resourceStorageComponent.Load();
		partyComponent = data.partyComponent.Load();
		partyComponent.SetOwner(this);
	}

	protected virtual void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true)
	{
		persistentID = Utilities.GetNewUniqueID();
		id = Utilities.SetID(this);
		this.tileObjectType = tileObjectType;
		name = GenerateDisplayName();
		internalName = GenerateInternalName();
		hasCreatedSlots = false;
		maxHP = TileObjectDB.GetTileObjectData(tileObjectType).maxHP;
		currentHP = maxHP;
		constructionComponent = new TileObjectConstructionComponent(this);
		CreateTraitContainer();
		traitContainer.AddTrait(this, "Flammable");
		if (shouldAddCommonAdvertisements)
		{
			AddCommonAdvertisements();
		}
		ConstructDefaultPlayerActions();
		DatabaseManager.Instance.tileObjectDatabase.RegisterTileObject(this);
		SubscribeListeners(shouldLock: false);
	}

	public virtual void Initialize(SaveDataTileObject data)
	{
		persistentID = data.persistentID;
		id = Utilities.SetID(this, data.id);
		tileObjectType = data.tileObjectType;
		name = data.name;
		internalName = data.internalName;
		hasCreatedSlots = false;
		maxHP = data.maxHP;
		currentHP = data.currentHP;
		isPreplaced = data.isPreplaced;
		isDamageContributorToStructure = data.isDamageContributorToStructure;
		isStoredAsTarget = data.isStoredAsTarget;
		isDeadReference = data.isDeadReference;
		isBuiltByPlayerBaseBuilding = data.isBuiltByPlayerBaseBuilding;
		SetPOIState(data.poiState);
		CreateTraitContainer();
		ConstructDefaultPlayerActions(broadcastSignal: false);
		logComponent = data.logComponent.Load();
		hiddenComponent = data.hiddenComponent.Load();
		constructionComponent = data.constructionComponent.Load();
		DatabaseManager.Instance.tileObjectDatabase.RegisterTileObject(this);
		SubscribeListeners(shouldLock: true);
	}

	public virtual void LoadSecondWave(SaveDataTileObject data)
	{
		SetMapObjectState(data.mapObjectState);
		if (!string.IsNullOrEmpty(data.characterOwnerID))
		{
			characterOwner = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.characterOwnerID);
		}
		if (!string.IsNullOrEmpty(data.isBeingCarriedByID))
		{
			isBeingCarriedBy = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.isBeingCarriedByID);
		}
		if (!string.IsNullOrEmpty(data.lastStolenByID))
		{
			lastStolenBy = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.lastStolenByID);
		}
		hiddenComponent.LoadSecondWave(this);
		constructionComponent.LoadSecondWave(this, data.constructionComponent);
		partyComponent.LoadSecondWave(this, data.partyComponent);
	}

	public virtual void LoadAdditionalInfo(SaveDataTileObject data)
	{
	}

	private void AddCommonAdvertisements()
	{
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.REPAIR);
		AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
		AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		AddAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		AddAdvertisedAction(INTERACTION_TYPE.LICK_TILE_OBJECT);
		AddAdvertisedAction(INTERACTION_TYPE.RUB_TILE_OBJECT);
	}

	protected void RemoveCommonAdvertisements()
	{
		RemoveAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		RemoveAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		RemoveAdvertisedAction(INTERACTION_TYPE.REPAIR);
		RemoveAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
		RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		RemoveAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		RemoveAdvertisedAction(INTERACTION_TYPE.LICK_TILE_OBJECT);
		RemoveAdvertisedAction(INTERACTION_TYPE.RUB_TILE_OBJECT);
	}

	protected virtual void SubscribeListeners(bool shouldLock)
	{
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter, shouldLock);
		Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure, shouldLock);
	}

	protected virtual void UnsubscribeListeners()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
	}

	protected virtual void SubscribeListenersDuringSeize()
	{
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
	}

	protected virtual void UnsubscribeListenersDuringSeize()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
	}

	protected virtual void DisconnectFromCharacter(Character p_character)
	{
		charactersThatAlreadyAssumed?.Remove(p_character);
		traitContainer?.DisconnectFromCharacter(this, p_character);
		RemoveUser(p_character);
		if (isBeingCarriedBy == p_character)
		{
			SetInventoryOwner(p_character);
		}
	}

	protected virtual void DisconnectFromStructure(LocationStructure p_structure)
	{
	}

	public virtual void OnDoActionToObject(ActualGoapNode action)
	{
	}

	public virtual void OnDoneActionToObject(ActualGoapNode action)
	{
	}

	public virtual void OnCancelActionTowardsObject(ActualGoapNode action)
	{
	}

	public virtual bool OccupiesTile()
	{
		return true;
	}

	public virtual void OnDestroyPOI(Character p_destroyer = null)
	{
		LocationAwarenessUtility.RemoveFromAwarenessList(this);
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)this, "");
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, (IPointOfInterest)this, "Target_Destroyed");
		OnRemoveTileObject(null, previousTile);
		DestroyMapVisualGameObject();
		SetPOIState(POI_STATE.INACTIVE);
		if (TileObjectDB.TryGetTileObjectData(tileObjectType, out var data) && (data.occupiedSize.X > 1 || data.occupiedSize.Y > 1))
		{
			UnoccupyTiles(data.occupiedSize, previousTile);
		}
		Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, (IPointOfInterest)this);
		UnsubscribeListeners();
		eventDispatcher.ExecuteTileObjectDestroyed(this);
		if (UIManager.Instance.IsContextMenuShowingForTarget(this))
		{
			UIManager.Instance.HideContextMenu();
		}
		if (isBuiltByPlayerBaseBuilding && !PlayerSkillManager.Instance.unlimitedCast)
		{
			if (tileObjectType == TILE_OBJECT_TYPE.BLOCK_WALL)
			{
				if (!WorldSettings.Instance.worldSettingsData.playerSkillSettings.PowerHasUnlimitedCharges(PLAYER_SKILL_TYPE.DEMONIC_WALL))
				{
					PlayerSkillManager.Instance.GetBuildSkillData(PLAYER_SKILL_TYPE.DEMONIC_WALL).AdjustCharges(1);
				}
			}
			else if (!WorldSettings.Instance.worldSettingsData.playerSkillSettings.PowerHasUnlimitedCharges(PLAYER_SKILL_TYPE.DECORATIONS))
			{
				PlayerSkillManager.Instance.GetBuildSkillData(PLAYER_SKILL_TYPE.DECORATIONS).AdjustCharges(1);
			}
		}
		Messenger.Broadcast(TileObjectSignals.DESTROY_TILE_OBJECT, this);
	}

	public virtual void OnPlacePOI()
	{
		DefaultProcessOnPlacePOI();
	}

	public virtual void OnLoadPlacePOI()
	{
		OnPlacePOI();
	}

	protected void DefaultProcessOnPlacePOI()
	{
		SetPOIState(POI_STATE.ACTIVE);
		if (mapVisual == null)
		{
			InitializeMapObject(this);
			OnMapObjectStateChanged();
		}
		PlaceMapObjectAt(gridTileLocation);
		mapVisual.UpdateSortingOrders(this);
		OnPlaceTileObjectAtTile(gridTileLocation);
		if (TileObjectDB.TryGetTileObjectData(tileObjectType, out var data) && (data.occupiedSize.X > 1 || data.occupiedSize.Y > 1))
		{
			OccupyTiles(data.occupiedSize, gridTileLocation);
		}
		gridTileLocation.area.OnPlacePOIInHex(this);
		gridTileLocation.eventDispatcher.ExecuteTileObjectPlacedEvent(this, gridTileLocation);
		if (gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Poisoned"))
		{
			Poisoned traitOrStatus = gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
			traitContainer.AddTrait(this, "Poisoned", null, bypassElementalChance: false, -1, 0f, ELEMENTAL_TYPE.Poison);
			traitContainer.GetTraitOrStatus<Poisoned>("Poisoned")?.SetIsPlayerSource(traitOrStatus.isPlayerSource);
		}
	}

	public virtual void RemoveTileObject(Character removedBy)
	{
		SetGridTileLocation(null);
		OnDestroyPOI();
		if (previousTile != null)
		{
			previousTile.area.OnRemovePOIInHex(this);
		}
	}

	public virtual LocationGridTile GetNearestUnoccupiedTileFromThis()
	{
		LocationGridTile result = null;
		if (gridTileLocation != null)
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			gridTileLocation.PopulateUnoccupiedNeighbours(list, sameStructureOnly: true);
			if (list.Count > 0)
			{
				result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
			}
			RuinarchListPool<LocationGridTile>.Release(list);
		}
		return result;
	}

	public GoapAction AdvertiseActionsToActor(Character actor, GoapEffect precondition, GoapPlanJob job, ref int cost, ref string log)
	{
		GoapAction result = null;
		if (advertisedActions != null && advertisedActions.Count > 0)
		{
			bool flag = IsAvailable();
			GoapAction goapAction = null;
			int num = 0;
			log += $"\n--Choices for {precondition}";
			log += "\n--";
			for (int i = 0; i < advertisedActions.Count; i++)
			{
				INTERACTION_TYPE iNTERACTION_TYPE = advertisedActions[i];
				GoapAction goapAction2 = InteractionManager.Instance.goapActionData[iNTERACTION_TYPE];
				if (!flag && !goapAction2.canBeAdvertisedEvenIfTargetIsUnavailable)
				{
					continue;
				}
				LocationGridTile toTile = gridTileLocation;
				if (isBeingCarriedBy != null)
				{
					toTile = isBeingCarriedBy.gridTileLocation;
				}
				if ((!goapAction2.canBePerformedEvenIfPathImpossible && !actor.movementComponent.HasPathToEvenIfDiffRegion(toTile)) || !RaceManager.Instance.CanCharacterDoGoapAction(actor, iNTERACTION_TYPE))
				{
					continue;
				}
				OtherData[] otherDataFor = job.GetOtherDataFor(iNTERACTION_TYPE);
				if (goapAction2.CanSatisfyRequirements(actor, this, otherDataFor, job) && goapAction2.WillEffectsSatisfyPrecondition(precondition, actor, this, job))
				{
					int cost2 = goapAction2.GetCost(actor, this, job);
					log += $"({cost2}){goapAction2.goapName}-{nameWithID}, ";
					if (goapAction == null || cost2 < num)
					{
						goapAction = goapAction2;
						num = cost2;
					}
				}
			}
			cost = num;
			result = goapAction;
		}
		return result;
	}

	public bool CanAdvertiseActionToActor(Character actor, GoapAction action, GoapPlanJob job)
	{
		if ((IsAvailable() || action.canBeAdvertisedEvenIfTargetIsUnavailable) && actor.trapStructure.SatisfiesForcedStructure(this) && actor.trapStructure.SatisfiesForcedArea(this) && RaceManager.Instance.CanCharacterDoGoapAction(actor, action.goapType))
		{
			LocationGridTile toTile = gridTileLocation;
			if (isBeingCarriedBy != null)
			{
				toTile = isBeingCarriedBy.gridTileLocation;
			}
			if (action.canBePerformedEvenIfPathImpossible || actor.movementComponent.HasPathToEvenIfDiffRegion(toTile))
			{
				OtherData[] otherDataFor = job.GetOtherDataFor(action.goapType);
				if (action.CanSatisfyRequirements(actor, this, otherDataFor, job))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual void SetPOIState(POI_STATE state)
	{
		this.state = state;
	}

	public virtual void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		Messenger.Broadcast(GridTileSignals.TILE_OBJECT_REMOVED, this, removedBy, removedFrom);
		if (base.mapObjectState == MAP_OBJECT_STATE.UNBUILT)
		{
			Messenger.RemoveListener(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY, CheckUnbuiltObjectValidity);
		}
		else if (base.mapObjectState == MAP_OBJECT_STATE.BUILT)
		{
			removedFrom?.parentMap.region.tileObjectsComponent.RemoveTileObjectInRegion(this);
		}
		if (hasCreatedSlots && destroyTileSlots)
		{
			DestroyTileSlots();
		}
		if (removeTraits)
		{
			traitContainer.RemoveAllTraitsAndStatuses(this);
		}
	}

	public virtual void OnTileObjectGainedTrait(Trait trait)
	{
		if (trait is Status { isTangible: not false } && mapObjectVisual != null)
		{
			mapObjectVisual.visionTrigger.VoteToMakeVisibleToCharacters();
		}
	}

	public virtual void OnTileObjectLostTrait(Trait trait)
	{
		if (trait is Status { isTangible: not false } && mapObjectVisual != null)
		{
			mapObjectVisual.visionTrigger.VoteToMakeInvisibleToCharacters();
		}
	}

	public virtual bool IsValidCombatTargetFor(IPointOfInterest source)
	{
		if (gridTileLocation != null && source.gridTileLocation != null && source is Character character)
		{
			return character.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation);
		}
		return false;
	}

	public virtual bool IsStillConsideredPartOfAwarenessByCharacter(Character character)
	{
		if (mapVisual == null)
		{
			if (isBeingCarriedBy != null && isBeingCarriedBy == character)
			{
				return true;
			}
			return false;
		}
		if (gridTileLocation != null && currentRegion == character.currentRegion)
		{
			return true;
		}
		if (isBeingCarriedBy != null && isBeingCarriedBy.currentRegion == character.currentRegion)
		{
			return true;
		}
		return false;
	}

	protected virtual string GenerateInternalName()
	{
		return tileObjectType.ToStringEnumWithSpace();
	}

	protected virtual string GenerateDisplayName()
	{
		string text = tileObjectType.LocalizedName();
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return tileObjectType.ToStringEnumWithSpace();
	}

	public virtual void Neutralize()
	{
	}

	public virtual void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		actions = new List<PLAYER_SKILL_TYPE>(10);
		if (tileObjectType != TILE_OBJECT_TYPE.RAVENOUS_SPIRIT && tileObjectType != TILE_OBJECT_TYPE.FEEBLE_SPIRIT && tileObjectType != TILE_OBJECT_TYPE.FORLORN_SPIRIT && tileObjectType != TILE_OBJECT_TYPE.DEMON_EYE && tileObjectType != TILE_OBJECT_TYPE.HALLOWED_GROUND)
		{
			AddPlayerAction(PLAYER_SKILL_TYPE.DESTROY, broadcastSignal);
			AddPlayerAction(PLAYER_SKILL_TYPE.IGNITE, broadcastSignal);
			AddPlayerAction(PLAYER_SKILL_TYPE.POISON, broadcastSignal);
			AddPlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT, broadcastSignal);
			AddPlayerAction(PLAYER_SKILL_TYPE.SNATCH_OBJECT, broadcastSignal);
		}
	}

	public virtual void ActivateTileObject()
	{
		Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_ACTIVATED, this);
	}

	public virtual void OnTileObjectDroppedBy(Character inventoryOwner, LocationGridTile tile)
	{
	}

	protected virtual void OnSetGridTileLocation()
	{
	}

	public bool IsAvailable()
	{
		return state != POI_STATE.INACTIVE;
	}

	public void AddAdvertisedAction(INTERACTION_TYPE type, bool allowDuplicates = false)
	{
		if (advertisedActions == null)
		{
			advertisedActions = new List<INTERACTION_TYPE>(20);
		}
		if (allowDuplicates || !advertisedActions.Contains(type))
		{
			advertisedActions.Add(type);
			LocationAwarenessUtility.AddToAwarenessList(type, this);
		}
	}

	public void RemoveAdvertisedAction(INTERACTION_TYPE type)
	{
		if (advertisedActions.Remove(type))
		{
			LocationAwarenessUtility.RemoveFromAwarenessList(type, this);
		}
	}

	public void AddJobTargetingThis(JobQueueItem job)
	{
		allJobsTargetingThis.Add(job);
		mapObjectVisual?.visionTrigger.VoteToMakeVisibleToCharacters();
	}

	public bool RemoveJobTargetingThis(JobQueueItem job)
	{
		if (allJobsTargetingThis.Remove(job))
		{
			mapObjectVisual?.visionTrigger.VoteToMakeInvisibleToCharacters();
			return true;
		}
		return false;
	}

	public virtual void AddExistingJobTargetingThis(JobQueueItem job)
	{
		allExistingJobsTargetingThis.Add(job);
	}

	public bool RemoveExistingJobTargetingThis(JobQueueItem job)
	{
		return allExistingJobsTargetingThis.Remove(job);
	}

	public bool HasJobTargetingThis(JOB_TYPE jobType)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			if (allJobsTargetingThis[i].jobType == jobType)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJobTargetingThis(JOB_TYPE jobType1, JOB_TYPE jobType2)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			JobQueueItem jobQueueItem = allJobsTargetingThis[i];
			if (jobQueueItem.jobType == jobType1 || jobQueueItem.jobType == jobType2)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJobTargetingThis(JOB_TYPE jobType1, JOB_TYPE jobType2, JOB_TYPE jobType3)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			JobQueueItem jobQueueItem = allJobsTargetingThis[i];
			if (jobQueueItem.jobType == jobType1 || jobQueueItem.jobType == jobType2 || jobQueueItem.jobType == jobType3)
			{
				return true;
			}
		}
		return false;
	}

	public GoapPlanJob GetJobTargetingThisCharacter(JOB_TYPE jobType)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			if (allJobsTargetingThis[i] is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = allJobsTargetingThis[i] as GoapPlanJob;
				if (goapPlanJob.jobType == jobType)
				{
					return goapPlanJob;
				}
			}
		}
		return null;
	}

	public virtual void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false, bool isTrueDamage = false)
	{
		if ((currentHP == 0 && amount < 0) || ((tileObjectType.IsDemonicStructureTileObject() || tileObjectType == TILE_OBJECT_TYPE.STRUCTURE_BLOCKER_TILE_OBJECT) && CombatManager.Instance.IsDamageSourceFromPlayerSpell(source)))
		{
			return;
		}
		Character character = source as Character;
		if (character != null && character.faction != null && character.faction.isPlayerFaction)
		{
			isPlayerSource = true;
		}
		if (!isTrueDamage)
		{
			CombatManager.Instance.ModifyDamage(ref amount, elementalDamageType, piercingPower, this);
		}
		if ((amount < 0 && CanBeDamaged()) || amount > 0)
		{
			if (amount < 0 && Mathf.Abs(amount) > currentHP)
			{
				amount = -currentHP;
			}
			if (mapObjectVisual != null && currentHP > 0)
			{
				if (character == null || character.combatComponent == null)
				{
					InnerMapManager.Instance.ShowHealthAdjustmentEffect(amount, null, mapObjectVisual.transform.position);
				}
				else
				{
					InnerMapManager.Instance.ShowHealthAdjustmentEffect(amount, character.combatComponent, mapObjectVisual.transform.position);
				}
			}
			int num = currentHP;
			currentHP += amount;
			currentHP = Mathf.Clamp(currentHP, 0, maxHP);
			if ((bool)mapVisual && showHPBar)
			{
				if (mapVisual.hasHPBarGO && mapVisual.hpBarGO.activeSelf)
				{
					mapVisual.UpdateHP(this);
				}
				else if (amount < 0 && currentHP > 0)
				{
					mapVisual.QuickShowHPBar(this);
				}
			}
			if (source is Character character2 && character2.partyComponent.hasParty && character2.partyComponent.currentParty.isActive && character2.partyComponent.currentParty.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Demon_Raid)
			{
				int p_amount = amount;
				if (currentHP == 0)
				{
					p_amount = num;
				}
				if (character2.partyComponent.currentParty.isPlayerParty)
				{
					character2.partyComponent.currentParty.damageAccumulator.AccumulateDamageFromDemonRaid(p_amount, character2);
				}
				else
				{
					character2.partyComponent.currentParty.damageAccumulator.AccumulateDamageMonsterSpawnerRaid(p_amount, character2);
				}
			}
		}
		if (amount < 0)
		{
			CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, character, elementalTraitProcessor, createHitEffect: true, isPlayerSource, piercingPower);
		}
		LocationGridTile locationGridTile = gridTileLocation;
		if (currentHP <= 0)
		{
			if (this is ResourcePile && character != null && locationGridTile != null && character.partyComponent.hasParty && character.partyComponent.currentParty.currentQuest is DemonRaidPartyQuest demonRaidPartyQuest && locationGridTile.IsPartOfSettlement(demonRaidPartyQuest.targetSettlement))
			{
				LocationGridTile locationGridTile2 = character.gridTileLocation;
				if (locationGridTile2 != null)
				{
					if (character.partyComponent.currentParty.isPlayerParty)
					{
						if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.MARAUD).TryDecreaseRemainingChaosOrbs(1))
						{
							Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, locationGridTile2.centeredWorldLocation, 1, locationGridTile2.parentMap);
						}
					}
					else if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.MONSTER_SPAWNER).TryDecreaseRemainingChaosOrbs(1))
					{
						Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, locationGridTile2.centeredWorldLocation, 1, locationGridTile2.parentMap);
					}
				}
			}
			if (isBeingSeized)
			{
				PlayerManager.Instance.player.seizeComponent.UnseizeTileObjectThenDestroyIt();
			}
			else if (locationGridTile != null && locationGridTile.structure != null)
			{
				locationGridTile.structure.RemovePOI(this, character, isPlayerSource);
			}
			else if (isBeingCarriedBy != null)
			{
				isBeingCarriedBy.UncarryPOI(this, bringBackToInventory: false, addToLocation: false);
			}
		}
		if (amount < 0)
		{
			if (character != null)
			{
				Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_DAMAGED_BY, this, amount, character, isPlayerSource);
			}
			else
			{
				Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_DAMAGED, this, amount, isPlayerSource);
			}
		}
		else if (amount > 0)
		{
			Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_REPAIRED, this, amount);
		}
		if (currentHP == maxHP)
		{
			Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_FULLY_REPAIRED, this);
		}
	}

	public void OnHitByAttackFrom(Character characterThatAttacked, CombatState combatStateOfAttacker, ref string attackSummary)
	{
		OnHitByAttackFrom(characterThatAttacked, combatStateOfAttacker, characterThatAttacked.combatComponent.attack, characterThatAttacked.combatComponent.currentElement.type, ref attackSummary);
	}

	public void OnHitByAttackFrom(Character characterThatAttacked, CombatState combatStateOfAttacker, int p_attackPower, ELEMENTAL_TYPE p_elementType, ref string attackSummary)
	{
		if (characterThatAttacked == null || currentHP <= 0)
		{
			return;
		}
		bool isPlayerSource = characterThatAttacked.faction != null && characterThatAttacked.faction.isPlayerFaction;
		int attackWithCritAndModifications = characterThatAttacked.combatComponent.GetAttackWithCritAndModifications(this, p_attackPower);
		AdjustHP(-attackWithCritAndModifications, p_elementType, triggerDeath: false, characterThatAttacked, null, showHPBar: true, 0f, isPlayerSource);
		characterThatAttacked.combatComponent.ResetDamageDoneType();
		_ = currentHP;
		_ = 0;
		if ((bool)characterThatAttacked.marker && combatStateOfAttacker != null)
		{
			for (int i = 0; i < characterThatAttacked.marker.inVisionCharacters.Count; i++)
			{
				Character character = characterThatAttacked.marker.inVisionCharacters[i];
				character.needsComponent.WakeUpFromNoise();
				if (character.limiterComponent.canPerform && character.limiterComponent.canMove)
				{
					character.reactionComponent.ReactToCombat(combatStateOfAttacker, this);
				}
			}
		}
		if (characterThatAttacked is Dragon && this is GenericTileObject)
		{
			characterThatAttacked.combatComponent.RemoveHostileInRange(this);
		}
		if (characterThatAttacked.equipmentComponent.currentWeapon is WeaponItem weaponItem)
		{
			weaponItem.ApplyWeaponEffectsOnHit(this);
		}
	}

	public void SetGridTileLocation(LocationGridTile tile)
	{
		previousTile = gridTileLocation;
		gridTileLocation = tile;
		LocationAwarenessUtility.RemoveFromAwarenessList(this);
		if (gridTileLocation != null && !(this is GenericTileObject))
		{
			LocationAwarenessUtility.AddToAwarenessList(this, gridTileLocation);
		}
		OnSetGridTileLocation();
	}

	public void OnSeizePOI(bool wasUnseizedFromCharacter)
	{
		if (UIManager.Instance.tileObjectInfoUI.isShowing && UIManager.Instance.tileObjectInfoUI.activeTileObject == this)
		{
			UIManager.Instance.tileObjectInfoUI.CloseMenu();
		}
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)this, "");
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, (IPointOfInterest)this, "");
		if (this is ResourcePile && !wasUnseizedFromCharacter)
		{
			PlayerManager.Instance?.player?.retaliationComponent.ResourcePileRetaliation(this, gridTileLocation);
		}
		gridTileLocation.structure.RemovePOIWithoutDestroying(this);
		if (TileObjectDB.TryGetTileObjectData(tileObjectType, out var data) && (data.occupiedSize.X > 1 || data.occupiedSize.Y > 1))
		{
			UnoccupyTiles(data.occupiedSize, previousTile);
		}
		Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, (IPointOfInterest)this);
		if (this is ResourcePile || this is HerbPlant)
		{
			SetCharacterOwner(null);
		}
		UnsubscribeListeners();
		SubscribeListenersDuringSeize();
		TileObjectScriptableObject tileObjectScriptableObject = InnerMapManager.Instance.GetTileObjectScriptableObject<TileObjectScriptableObject>(tileObjectType);
		if (tileObjectScriptableObject.seizeSFX.IsValid())
		{
			tileObjectScriptableObject.seizeSFX.Post(InnerMapCameraMove.Instance.gameObject);
		}
	}

	public void OnUnseizePOI(LocationGridTile tileLocation)
	{
		DestroyMapVisualGameObject();
		tileLocation.structure.AddPOI(this, tileLocation);
		if (!traitContainer.HasTrait("Burning") && tileLocation.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Burning"))
		{
			Burning traitOrStatus = tileLocation.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Burning>("Burning");
			traitContainer.AddTrait(this, "Burning", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Fire);
			traitContainer.GetTraitOrStatus<Burning>("Burning")?.SetIsPlayerSource(traitOrStatus.isPlayerSource);
		}
		if (this is ResourcePile && tileLocation.structure.residents.Count > 0)
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			for (int i = 0; i < tileLocation.structure.residents.Count; i++)
			{
				Character character = tileLocation.structure.residents[i];
				if (character.isNormalCharacter)
				{
					list.Add(character);
				}
			}
			if (list.Count > 0)
			{
				Character randomElement = CollectionUtilities.GetRandomElement(list);
				SetCharacterOwner(randomElement);
			}
			RuinarchListPool<Character>.Release(list);
		}
		SubscribeListeners(shouldLock: false);
		UnsubscribeListenersDuringSeize();
		TileObjectScriptableObject tileObjectScriptableObject = InnerMapManager.Instance.GetTileObjectScriptableObject<TileObjectScriptableObject>(tileObjectType);
		if (tileObjectScriptableObject.unseizeSFX.IsValid())
		{
			tileObjectScriptableObject.unseizeSFX.Post(InnerMapCameraMove.Instance.gameObject);
		}
	}

	public void CancelRemoveStatusFeedAndRepairJobsTargetingThis()
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			JobQueueItem jobQueueItem = allJobsTargetingThis[i];
			if ((jobQueueItem.jobType == JOB_TYPE.REMOVE_STATUS || jobQueueItem.jobType == JOB_TYPE.REPAIR || jobQueueItem.jobType == JOB_TYPE.FEED) && jobQueueItem.CancelJob())
			{
				i--;
			}
		}
	}

	public void AdjustNumOfNonSecretActionsBeingPerformedOnThis(int amount)
	{
		numOfNonSecretActionsBeingPerformedOnThis += amount;
		numOfNonSecretActionsBeingPerformedOnThis = Mathf.Max(0, numOfNonSecretActionsBeingPerformedOnThis);
	}

	public bool IsPOICurrentlyTargetedByOtherCharacterPerformingAction()
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			if (allJobsTargetingThis[i] is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = allJobsTargetingThis[i] as GoapPlanJob;
				if (goapPlanJob.assignedPlan != null && goapPlanJob.assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsPOICurrentlyTargetedByAPerformingAction(params JOB_TYPE[] jobType)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			JobQueueItem jobQueueItem = allJobsTargetingThis[i];
			for (int j = 0; j < jobType.Length; j++)
			{
				if (jobType[j] == jobQueueItem.jobType && jobQueueItem is GoapPlanJob { assignedPlan: not null } goapPlanJob && goapPlanJob.assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE p_jobType, GoapEffect p_goapEffect)
	{
		for (int i = 0; i < allJobsTargetingThis.Count; i++)
		{
			if (allJobsTargetingThis[i] is GoapPlanJob { assignedPlan: not null } goapPlanJob && goapPlanJob.assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING && goapPlanJob.goal != null && goapPlanJob.goal.IsSameGoal(p_goapEffect))
			{
				return true;
			}
		}
		return false;
	}

	public void SetCurrentLocationAwareness(ILocationAwareness locationAwareness)
	{
		currentLocationAwareness = locationAwareness;
	}

	public virtual bool IsUnpassable()
	{
		return false;
	}

	public bool CanBeSeenBy(Character p_character)
	{
		if (p_character.hasMarker)
		{
			return p_character.marker.inVisionTileObjects.Contains(this);
		}
		return false;
	}

	public virtual void OnAddedAsUnprocessedPOI(Character p_characterThatAddedPOI)
	{
	}

	public void CreateTraitContainer()
	{
		traitContainer = new TraitContainer();
	}

	public virtual bool CanBeAffectedByElementalStatus(string traitName)
	{
		return true;
	}

	protected void ProcessTraitsOnTickStarted()
	{
		traitContainer.ProcessOnTickStarted(this);
	}

	private string GetUIString()
	{
		if (string.IsNullOrEmpty(_uiString))
		{
			string text = "TileObject" + "|" + persistentID;
			_uiString = "<link=" + text + ">" + Utilities.ColorizeName(name) + "</link>";
		}
		return _uiString;
	}

	public bool Advertises(INTERACTION_TYPE type)
	{
		if (advertisedActions != null)
		{
			for (int i = 0; i < advertisedActions.Count; i++)
			{
				if (advertisedActions[i] == type)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected virtual void OnPlaceTileObjectAtTile(LocationGridTile tile)
	{
		if (hasCreatedSlots)
		{
			RepositionTileSlots(tile);
		}
		else
		{
			CreateTileObjectSlots();
		}
		Messenger.Broadcast(GridTileSignals.TILE_OBJECT_PLACED, this, tile);
	}

	private bool HasSlotSettings()
	{
		if ((object)mapVisual.usedSprite != null)
		{
			return InnerMapManager.Instance.HasSettingForTileObjectAsset(mapVisual.usedSprite);
		}
		return false;
	}

	private void CreateTileObjectSlots()
	{
		if (tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT && HasSlotSettings())
		{
			Sprite usedSprite = mapVisual.usedSprite;
			List<TileObjectSlotSetting> tileObjectSlotSettings = InnerMapManager.Instance.GetTileObjectSlotSettings(usedSprite);
			if (slotsParent == null)
			{
				slotsParent = UnityEngine.Object.Instantiate(InnerMapManager.Instance.tileObjectSlotsParentPrefab, mapVisual.objectSpriteRenderer.transform);
				slotsParent.name = ToString() + " Slots";
				slotsParent.transform.localPosition = Vector3.zero;
				slotsParent.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
			slots = new TileObjectSlotItem[tileObjectSlotSettings.Count];
			_users = new Character[tileObjectSlotSettings.Count];
			for (int i = 0; i < tileObjectSlotSettings.Count; i++)
			{
				TileObjectSlotSetting settings = tileObjectSlotSettings[i];
				TileObjectSlotItem component = UnityEngine.Object.Instantiate(InnerMapManager.Instance.tileObjectSlotPrefab, Vector3.zero, Quaternion.identity, slotsParent.transform).GetComponent<TileObjectSlotItem>();
				component.ApplySettings(this, settings);
				slots[i] = component;
			}
		}
		hasCreatedSlots = true;
	}

	private void RepositionTileSlots(LocationGridTile tile)
	{
		if ((object)slotsParent != null)
		{
			slotsParent.transform.localPosition = Vector3.zero;
		}
	}

	protected void DestroyTileSlots()
	{
		if (slots != null)
		{
			for (int i = 0; i < slots.Length; i++)
			{
				UnityEngine.Object.Destroy(slots[i].gameObject);
			}
			if (slotsParent != null)
			{
				UnityEngine.Object.Destroy(slotsParent);
			}
			slots = null;
			slotsParent = null;
			_users = null;
			hasCreatedSlots = false;
		}
	}

	private TileObjectSlotItem GetNearestUnoccupiedSlot(Character character)
	{
		float num = 9999f;
		TileObjectSlotItem result = null;
		for (int i = 0; i < slots.Length; i++)
		{
			TileObjectSlotItem tileObjectSlotItem = slots[i];
			if (tileObjectSlotItem.user == null)
			{
				float num2 = Vector2.Distance(character.marker.transform.position, tileObjectSlotItem.transform.position);
				if (num2 < num)
				{
					num = num2;
					result = tileObjectSlotItem;
				}
			}
		}
		return result;
	}

	protected bool HasUnoccupiedSlot()
	{
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].user == null)
			{
				return true;
			}
		}
		return false;
	}

	private TileObjectSlotItem GetSlotUsedBy(Character character)
	{
		if (slots != null)
		{
			for (int i = 0; i < slots.Length; i++)
			{
				TileObjectSlotItem tileObjectSlotItem = slots[i];
				if (tileObjectSlotItem.user == character)
				{
					return tileObjectSlotItem;
				}
			}
		}
		return null;
	}

	public void SetSlotAlpha(float alpha)
	{
		if (slots != null)
		{
			for (int i = 0; i < slots.Length; i++)
			{
				TileObjectSlotItem obj = slots[i];
				Color color = obj.spriteRenderer.color;
				color.a = alpha;
				obj.SetSlotColor(color);
			}
		}
	}

	public void RevalidateTileObjectSlots()
	{
		if (hasCreatedSlots)
		{
			DestroyTileSlots();
			CreateTileObjectSlots();
		}
	}

	protected virtual bool AddUser(Character newUser)
	{
		if (newUser != null && users.Contains(newUser))
		{
			return true;
		}
		TileObjectSlotItem nearestUnoccupiedSlot = GetNearestUnoccupiedSlot(newUser);
		if (nearestUnoccupiedSlot != null)
		{
			newUser.SetTileObjectLocation(this);
			nearestUnoccupiedSlot.Use(newUser);
			if (!HasUnoccupiedSlot())
			{
				SetPOIState(POI_STATE.INACTIVE);
			}
			newUser.visuals?.UpdateAllVisuals(newUser);
			Messenger.Broadcast(TileObjectSignals.ADD_TILE_OBJECT_USER, this, newUser);
		}
		return true;
	}

	public virtual bool RemoveUser(Character user)
	{
		TileObjectSlotItem slotUsedBy = GetSlotUsedBy(user);
		if (slotUsedBy != null)
		{
			user.SetTileObjectLocation(null);
			slotUsedBy.StopUsing();
			SetPOIState(POI_STATE.ACTIVE);
			user.visuals?.UpdateAllVisuals(user);
			Messenger.Broadcast(TileObjectSignals.REMOVE_TILE_OBJECT_USER, this, user);
			return true;
		}
		return false;
	}

	public int GetUserCount()
	{
		int num = 0;
		Character[] array = users;
		if (array != null && array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
				{
					num++;
				}
			}
		}
		return num;
	}

	public Character GetFirstUser()
	{
		if (users != null && users.Length != 0)
		{
			for (int i = 0; i < users.Length; i++)
			{
				Character character = users[i];
				if (character != null)
				{
					return character;
				}
			}
		}
		return null;
	}

	public Character[] GetUsers()
	{
		if (slots != null)
		{
			for (int i = 0; i < slots.Length; i++)
			{
				TileObjectSlotItem tileObjectSlotItem = slots[i];
				_users[i] = tileObjectSlotItem.user;
			}
		}
		return _users;
	}

	public virtual bool CanBeDamaged()
	{
		if (base.mapObjectState == MAP_OBJECT_STATE.BUILT)
		{
			return !traitContainer.HasTrait("Indestructible");
		}
		return false;
	}

	public Vector3 GetProjectileTargetPosition()
	{
		Vector3 result = gridTileLocation.centeredWorldLocation;
		if (projectileReceiver != null)
		{
			result = projectileReceiver.transform.position;
		}
		else if (mapObjectVisual != null)
		{
			result = mapObjectVisual.transform.position;
		}
		return result;
	}

	protected virtual string GetFlavorText()
	{
		string text = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", name + "_Flavor_Text") ?? "";
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return string.Empty;
	}

	public void DoCleanup()
	{
		traitContainer?.RemoveAllTraitsAndStatuses(this);
	}

	public void UpdateOwners()
	{
		if (!(gridTileLocation.structure is Dwelling) && !(gridTileLocation.structure is VampireCastle))
		{
			return;
		}
		LocationStructure structure = gridTileLocation.structure;
		if (structure.residents.Count <= 0 || (characterOwner != null && (characterOwner == null || structure.residents.Contains(characterOwner))))
		{
			return;
		}
		Character character = null;
		Character character2 = null;
		for (int i = 0; i < structure.residents.Count; i++)
		{
			Character character3 = structure.residents[i];
			if (character3.faction != null && character3.faction.isMajorFaction)
			{
				if (GameUtilities.RollChance(50))
				{
					character2 = character3;
					break;
				}
				if (character == null)
				{
					character = character3;
				}
			}
		}
		if (character2 == null)
		{
			if (character != null)
			{
				SetCharacterOwner(character);
			}
			else
			{
				SetCharacterOwner(null);
			}
		}
		else
		{
			SetCharacterOwner(character2);
		}
	}

	private void OccupyTiles(Point size, LocationGridTile tile)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		tile.parentMap.PopulateTiles(list, size, tile);
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			locationGridTile.SetTileState(LocationGridTile.Tile_State.Occupied);
			locationGridTile.tileObjectComponent.SetOccupyingObject(this);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	private void UnoccupyTiles(Point size, LocationGridTile tile)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		tile.parentMap.PopulateTiles(list, size, tile);
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			locationGridTile.SetTileState(LocationGridTile.Tile_State.Empty);
			locationGridTile.tileObjectComponent.SetOccupyingObject(null);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	public bool IsOwnedBy(Character character)
	{
		return characterOwner == character;
	}

	public virtual void SetCharacterOwner(Character characterOwner)
	{
		if (this.characterOwner != characterOwner && (characterOwner == null || !(this is Heirloom)))
		{
			Character character = this.characterOwner;
			this.characterOwner = characterOwner;
			character?.RemoveOwnedItem(this);
			if (this.characterOwner == null)
			{
				RemoveAdvertisedAction(INTERACTION_TYPE.STEAL);
				return;
			}
			this.characterOwner.AddOwnedItem(this);
			AddAdvertisedAction(INTERACTION_TYPE.STEAL);
		}
	}

	public void SetLastStolenBy(Character p_character)
	{
		lastStolenBy = p_character;
	}

	public virtual void SetInventoryOwner(Character p_newOwner)
	{
		isBeingCarriedBy = p_newOwner;
		if (isBeingCarriedBy != null)
		{
			AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM, allowDuplicates: true);
		}
		else
		{
			RemoveAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		}
	}

	public bool CanBePickedUpNormallyUponVisionBy(Character character)
	{
		if (!tileObjectType.IsTileObjectAnItem())
		{
			return false;
		}
		if (!character.isNormalCharacter)
		{
			return false;
		}
		if (base.mapObjectState != MAP_OBJECT_STATE.BUILT)
		{
			return false;
		}
		if (numOfNonSecretActionsBeingPerformedOnThis > 0)
		{
			return false;
		}
		if (character.currentActionNode != null && character.currentActionNode.poiTarget == this)
		{
			return false;
		}
		if (!Advertises(INTERACTION_TYPE.PICK_UP))
		{
			return false;
		}
		if (IsOwnedBy(character) && gridTileLocation != null && gridTileLocation.structure == character.homeStructure)
		{
			return false;
		}
		if (traitContainer.HasTrait("Treasure") && !IsInterestedInThisTreasure(character))
		{
			return false;
		}
		if (characterOwner == null || IsOwnedBy(character))
		{
			return true;
		}
		if (character.traitContainer.HasTrait("Kleptomaniac"))
		{
			return true;
		}
		return false;
	}

	protected virtual bool IsInterestedInThisTreasure(Character p_character)
	{
		return true;
	}

	protected TileObject GetBase()
	{
		return this;
	}

	public void SetIsPreplaced(bool state)
	{
		isPreplaced = state;
	}

	public void AdjustRepairCounter(int amount)
	{
		repairCounter += amount;
	}

	public void AddCharacterThatAlreadyAssumed(Character character)
	{
		charactersThatAlreadyAssumed.Add(character);
	}

	public bool HasCharacterAlreadyAssumed(Character character)
	{
		return charactersThatAlreadyAssumed.Contains(character);
	}

	public bool IsInHomeStructureOfCharacterWithOpinion(Character character, params string[] opinion)
	{
		if (gridTileLocation != null && structureLocation != null)
		{
			for (int i = 0; i < structureLocation.residents.Count; i++)
			{
				Character target = structureLocation.residents[i];
				string opinionLabel = character.relationshipContainer.GetOpinionLabel(target);
				for (int j = 0; j < opinion.Length; j++)
				{
					if (opinionLabel == opinion[j])
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void SetAsDamageContributorToStructure(bool p_state)
	{
		isDamageContributorToStructure = p_state;
	}

	protected Vector3 GetAttackRangePosForDemonicStructureTileObject()
	{
		return gridTileLocation?.parentMap.GetTileFromWorldPosition(worldPosition).centeredWorldLocation ?? worldPosition;
	}

	public void CenterOnTileObject()
	{
		if (worldObject == null && isBeingCarriedBy != null)
		{
			InnerMapCameraMove.Instance.CenterCameraOn(isBeingCarriedBy.worldObject.gameObject);
		}
		else if (worldObject != null && isBeingCarriedBy == null)
		{
			InnerMapCameraMove.Instance.CenterCameraOn(worldObject.gameObject);
		}
		else if (worldObject != null && isBeingCarriedBy != null)
		{
			InnerMapCameraMove.Instance.CenterCameraOn(worldObject.gameObject);
		}
	}

	public void SetIsBuiltByPlayerBaseBuilding(bool p_state)
	{
		isBuiltByPlayerBaseBuilding = p_state;
	}

	public virtual void OnInspect(Character inspector)
	{
	}

	protected override void CreateMapObjectVisual()
	{
		GameObject gameObject = InnerMapManager.Instance.mapObjectFactory.CreateNewTileObjectMapVisual(tileObjectType);
		mapVisual = gameObject.GetComponent<TileObjectGameObject>();
	}

	protected override void OnMapObjectStateChanged()
	{
		if (!(mapVisual == null))
		{
			if (base.mapObjectState == MAP_OBJECT_STATE.UNBUILT)
			{
				OnSetObjectAsUnbuilt();
			}
			else if (base.mapObjectState == MAP_OBJECT_STATE.BUILDING)
			{
				OnSetObjectAsBuilding();
			}
			else
			{
				OnSetObjectAsBuilt();
			}
		}
	}

	protected virtual void OnSetObjectAsUnbuilt()
	{
		mapVisual.SetVisualAlpha(0f);
		SetSlotAlpha(0f);
		SetPOIState(POI_STATE.INACTIVE);
		AddAdvertisedAction(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
		UnsubscribeListeners();
		Messenger.AddListener(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY, CheckUnbuiltObjectValidity);
		if (gridTileLocation != null)
		{
			gridTileLocation.parentMap.region.tileObjectsComponent.RemoveTileObjectInRegion(this);
			if (gridTileLocation.structure is Dwelling dwelling)
			{
				dwelling.OnTileObjectInDwellingSetAsUnbuilt(this);
			}
			else if (gridTileLocation.structure is VampireCastle vampireCastle)
			{
				vampireCastle.OnTileObjectInDwellingSetAsUnbuilt(this);
			}
			else if (gridTileLocation.structure.structureType.IsSpecialStructure())
			{
				gridTileLocation.structure.OnTileObjectInDwellingSetAsUnbuilt(this);
			}
		}
		constructionComponent.OnObjectSetAsUnbuilt();
	}

	protected virtual void OnSetObjectAsBuilding()
	{
		mapVisual.SetVisualAlpha(0.5019608f);
		SetSlotAlpha(0.5019608f);
		if (mapVisual is TileObjectGameObject tileObjectGameObject)
		{
			tileObjectGameObject.ShowConstructionVisual();
		}
		Messenger.RemoveListener(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY, CheckUnbuiltObjectValidity);
	}

	protected virtual void OnSetObjectAsBuilt()
	{
		Messenger.RemoveListener(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY, CheckUnbuiltObjectValidity);
		hiddenComponent.OnSetHiddenState(this);
		mapVisual.SetVisualAlpha(1f);
		SetSlotAlpha(1f);
		if (mapVisual is TileObjectGameObject tileObjectGameObject)
		{
			tileObjectGameObject.HideConstructionVisual();
		}
		SetPOIState(POI_STATE.ACTIVE);
		if (advertisedActions != null && advertisedActions.Count > 0 && GameManager.Instance.gameHasStarted)
		{
			RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
		}
		SubscribeListeners(shouldLock: false);
		if (gridTileLocation != null)
		{
			gridTileLocation.parentMap.region.tileObjectsComponent.AddTileObjectInRegion(this);
			if (gridTileLocation.structure is Dwelling dwelling)
			{
				dwelling.OnTileObjectInDwellingSetAsBuilt(this);
			}
			else if (gridTileLocation.structure is VampireCastle vampireCastle)
			{
				vampireCastle.OnTileObjectInDwellingSetAsBuilt(this);
			}
			else if (gridTileLocation.structure.structureType.IsSpecialStructure())
			{
				gridTileLocation.structure.OnTileObjectInDwellingSetAsBuilt(this);
			}
		}
		constructionComponent.OnObjectSetAsBuilt();
	}

	private void CheckUnbuiltObjectValidity()
	{
		if (allExistingJobsTargetingThis.Count <= 0)
		{
			Messenger.RemoveListener(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY, CheckUnbuiltObjectValidity);
			Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, (IPointOfInterest)this);
			List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim();
			list.AddRange(allExistingJobsTargetingThis);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].CancelJob();
			}
			RuinarchListPool<JobQueueItem>.Release(list);
			gridTileLocation?.structure.RemovePOI(this);
		}
	}

	public override string ToString()
	{
		return $"{name} {id}";
	}

	public virtual void OnLocaleChanged(Locale locale)
	{
		name = GenerateDisplayName();
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

	public virtual bool IsCurrentlySelected()
	{
		if (UIManager.Instance.tileObjectInfoUI.isShowing)
		{
			return UIManager.Instance.tileObjectInfoUI.activeTileObject == this;
		}
		return false;
	}

	public virtual void LeftSelectAction()
	{
		if (mapObjectVisual != null)
		{
			mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Left);
		}
		else
		{
			UIManager.Instance.ShowTileObjectInfo(this);
		}
	}

	public virtual void RightSelectAction()
	{
		mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Right);
	}

	public virtual void MiddleSelectAction()
	{
		mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Middle);
	}

	public virtual bool CanBeSelected()
	{
		return true;
	}

	public virtual void OnReferencedInALog()
	{
	}

	public bool IsValidForStoreTarget()
	{
		if (gridTileLocation == null)
		{
			return isBeingCarriedBy != null;
		}
		return true;
	}

	public bool CanBeStoredAsTarget()
	{
		if (traitContainer.HasTrait("Temporal"))
		{
			return false;
		}
		if (traitContainer.HasTrait("Slick"))
		{
			return false;
		}
		return true;
	}

	public void SetAsStoredTarget(bool p_state)
	{
		isStoredAsTarget = p_state;
	}

	public bool IsValidTargetForPartyStructure(LocationStructure p_structure)
	{
		return false;
	}

	public Sprite GetPortraitSprite()
	{
		return UIManager.Instance.tileObjectPortrait;
	}

	public void OnSelectBookmark()
	{
		LeftSelectAction();
	}

	public void RemoveBookmark()
	{
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this, BOOKMARK_CATEGORY.Targets);
	}

	public void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		UIManager.Instance.ShowTileObjectNameplateTooltip(this, p_pos);
	}

	public void OnHoverOutBookmarkItem()
	{
		UIManager.Instance.HideTileObjectNameplateTooltip();
	}

	public virtual void DestroyPermanently()
	{
		DatabaseManager.Instance.tileObjectDatabase.UnRegisterTileObject(this);
	}

	public void SetIsDeadReference(bool p_state)
	{
		isDeadReference = p_state;
	}

	public virtual void GeneralReactionToTileObject(Character actor, ref string debugLog)
	{
		if (!isDamageContributorToStructure)
		{
			return;
		}
		LocationStructure locationStructure = currentStructure;
		if (locationStructure != null && locationStructure.structureType.IsPlayerStructure())
		{
			if (actor.partyComponent.isMemberThatJoinedQuest && actor.partyComponent.currentParty.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Counterattack)
			{
				actor.combatComponent.Fight(this, "Clear_Demonic_Intrusion");
			}
			else if (actor.behaviourComponent.isAttackingDemonicStructure)
			{
				actor.combatComponent.Fight(this, "Clear_Demonic_Intrusion");
			}
		}
	}

	public virtual void VillagerReactionToTileObject(Character actor, ref string debugLog)
	{
		if (traitContainer.HasTrait("Dangerous") && gridTileLocation != null && (this is Tornado || actor.currentStructure == gridTileLocation.structure || (!actor.currentStructure.isInterior && !gridTileLocation.structure.isInterior)))
		{
			if (actor.traitContainer.HasTrait("Berserked"))
			{
				actor.combatComponent.FightOrFlight(this, "Berserked");
			}
			else if (actor.stateComponent.currentState == null)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "CharacterCombat_Table", "Saw_Object", LOG_TAG.Party);
				log.AddToFillers(null, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				string logText = log.logText;
				LogPool.Release(log);
				if (actor.moodComponent.moodState == MOOD_STATE.Normal)
				{
					string neutralizingTraitFor = TraitManager.Instance.GetNeutralizingTraitFor(this);
					if (neutralizingTraitFor != string.Empty)
					{
						if (actor.traitContainer.HasTrait(neutralizingTraitFor))
						{
							if (!actor.jobQueue.HasJob(JOB_TYPE.NEUTRALIZE_DANGER, this))
							{
								GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.NEUTRALIZE_DANGER, INTERACTION_TYPE.NEUTRALIZE, this, actor);
								actor.jobQueue.AddJobInQueue(job);
							}
						}
						else
						{
							actor.combatComponent.Flight(this, logText);
						}
					}
					else
					{
						actor.combatComponent.Flight(this, logText);
					}
				}
				else
				{
					actor.combatComponent.Flight(this, logText);
				}
			}
		}
		if (traitContainer.HasTrait("Danger Remnant", "Lightning Remnant") && !actor.traitContainer.HasTrait("Berserked"))
		{
			if (gridTileLocation != null && gridTileLocation.corruptionComponent.isCorrupted)
			{
				CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, this, REACTION_STATUS.WITNESSED);
			}
			else if (actor.traitContainer.HasTrait("Coward"))
			{
				CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, this, REACTION_STATUS.WITNESSED);
			}
			else
			{
				int num = 30;
				if (actor.traitContainer.HasTrait("Combatant"))
				{
					num = 70;
				}
				if (UnityEngine.Random.Range(0, 100) < num)
				{
					CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, actor, this, REACTION_STATUS.WITNESSED);
				}
				else
				{
					CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, this, REACTION_STATUS.WITNESSED);
				}
			}
		}
		if (traitContainer.HasTrait("Surprised Remnant") && !actor.traitContainer.HasTrait("Berserked"))
		{
			if (gridTileLocation != null && gridTileLocation.corruptionComponent.isCorrupted)
			{
				CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, this, REACTION_STATUS.WITNESSED);
			}
			else if (actor.traitContainer.HasTrait("Coward"))
			{
				CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, this, REACTION_STATUS.WITNESSED);
			}
			else if (UnityEngine.Random.Range(0, 100) < 95)
			{
				CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, actor, this, REACTION_STATUS.WITNESSED);
			}
			else
			{
				CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, this, REACTION_STATUS.WITNESSED);
			}
		}
		if (IsOwnedBy(actor) && gridTileLocation != null && gridTileLocation.structure != null && gridTileLocation.structure is Dwelling && gridTileLocation.structure != actor.homeStructure)
		{
			if (gridTileLocation.structure.residents.Count > 0 && !HasCharacterAlreadyAssumed(actor))
			{
				actor.reactionComponent.assumptionSuspects.Clear();
				for (int i = 0; i < gridTileLocation.structure.residents.Count; i++)
				{
					Character character = gridTileLocation.structure.residents[i];
					if (actor.relationshipContainer.GetAwarenessState(actor, character) == AWARENESS_STATE.Available && !character.isDead)
					{
						actor.reactionComponent.assumptionSuspects.Add(character);
					}
				}
				if (actor.reactionComponent.assumptionSuspects.Count > 0)
				{
					Character character2 = actor.reactionComponent.assumptionSuspects[UnityEngine.Random.Range(0, actor.reactionComponent.assumptionSuspects.Count)];
					actor.assumptionComponent.CreateAndReactToNewAssumption(character2, this, INTERACTION_TYPE.STEAL, REACTION_STATUS.WITNESSED, lastStolenBy != character2);
					actor.jobComponent.RetrieveStolenItem(this);
				}
			}
			if (tileObjectType.IsTileObjectAnItem() && !actor.jobQueue.HasJob(JOB_TYPE.TAKE_ITEM_ON_SIGHT, this) && Advertises(INTERACTION_TYPE.PICK_UP) && actor.limiterComponent.canMove)
			{
				actor.jobComponent.CreateTakeItemOnSightJob(this);
			}
		}
		List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions("Villager_Reaction");
		if (traitOverrideFunctions != null)
		{
			for (int j = 0; j < traitOverrideFunctions.Count; j++)
			{
				traitOverrideFunctions[j].VillagerReactionToTileObjectTrait(this, actor, ref debugLog);
			}
		}
	}

	protected void TryCreateObtainFurnitureWantOnReactionJob<T>(Character actor) where T : VillagerWant
	{
		if (actor.villagerWantsComponent != null && actor.villagerWantsComponent.IsWantToggledOn<T>() && !actor.jobQueue.HasJob(JOB_TYPE.OBTAIN_WANTED_ITEM) && !actor.movementComponent.structuresToAvoid.Contains(structureLocation) && !actor.jobQueue.HasJob(JOB_TYPE.RETURN_STOLEN_THING, this))
		{
			bool flag = false;
			if ((!structureLocation.structureType.IsVillageStructure() || structureLocation.structureType == STRUCTURE_TYPE.CITY_CENTER || structureLocation.structureType == STRUCTURE_TYPE.CEMETERY) ? (characterOwner == null || IsOwnedBy(actor)) : (IsOwnedBy(actor) && structureLocation != actor.homeStructure))
			{
				actor.jobComponent.CreateDropItemJob(JOB_TYPE.OBTAIN_WANTED_ITEM, this, actor.homeStructure);
			}
		}
	}

	public override string GetAdditionalTestingData()
	{
		string additionalTestingData = base.GetAdditionalTestingData();
		additionalTestingData = additionalTestingData + "\n\tConstruction Progress: " + constructionComponent.currentConstructionTick + "/" + constructionComponent.totalNeededConstructionTicks;
		additionalTestingData += "\n\tResources:";
		if (resourceStorageComponent != null && resourceStorageComponent.specificStoredResources != null)
		{
			foreach (KeyValuePair<CONCRETE_RESOURCES, int> specificStoredResource in resourceStorageComponent.specificStoredResources)
			{
				if (specificStoredResource.Value > 0)
				{
					additionalTestingData = additionalTestingData + "\n\t\t" + specificStoredResource.Key.ToString() + " - " + specificStoredResource.Value;
				}
			}
		}
		return additionalTestingData;
	}

	public virtual void ProcessOnSetAsActiveInTileObjectInfo()
	{
	}

	public virtual void ProcessOnSetAsInactiveInTileObjectInfo()
	{
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		logComponent?.CheckIfStructureIsStillReferenced(p_structure);
		hiddenComponent?.CheckIfStructureIsStillReferenced(p_structure);
		eventDispatcher?.CheckIfStructureIsStillReferenced(p_structure);
		constructionComponent?.CheckIfStructureIsStillReferenced(p_structure);
		resourceStorageComponent?.CheckIfStructureIsStillReferenced(p_structure);
		partyComponent?.CheckIfStructureIsStillReferenced(p_structure);
		bookmarkEventDispatcher?.CheckIfStructureIsStillReferenced(p_structure);
	}

	public virtual void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = characterOwner;
		charactersThatAlreadyAssumed.Contains(p_character);
		_ = isBeingCarriedBy;
		if (slots != null)
		{
			for (int i = 0; i < slots.Length; i++)
			{
				slots[i].CheckIfCharacterIsStillReferenced(p_character);
			}
		}
		users?.Contains(p_character);
		_ = lastStolenBy;
		logComponent?.CheckIfCharacterIsStillReferenced(p_character);
		hiddenComponent?.CheckIfCharacterIsStillReferenced(p_character);
		eventDispatcher?.CheckIfCharacterIsStillReferenced(p_character);
		constructionComponent?.CheckIfCharacterIsStillReferenced(p_character);
		resourceStorageComponent?.CheckIfCharacterIsStillReferenced(p_character);
		partyComponent?.CheckIfCharacterIsStillReferenced(p_character);
		bookmarkEventDispatcher?.CheckIfCharacterIsStillReferenced(p_character);
	}
}
