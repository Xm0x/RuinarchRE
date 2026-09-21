using System;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public abstract class ManMadeStructure : LocationStructure, TileObjectEventDispatcher.ITraitListener
{
	protected StructureTileObject _structureTileObject;

	private string m_expirationScheduleKey;

	private GameDate m_scheduledDirtProduction;

	private List<TileObject> m_dirtyObjects;

	private InnerMapLight m_innerMapLight;

	private bool _hasCheckedLight;

	private const int UncomfortableNeededDirtyObjects = 2;

	public List<ThinWall> structureWalls { get; private set; }

	public WALL_RESOURCE wallsAreMadeOf { get; protected set; }

	public LocationStructureObject structureObj { get; private set; }

	public List<string> assignedWorkerIDs { get; private set; }

	public Dictionary<string, GameDate> workerCheckins { get; private set; }

	public string templateName { get; private set; }

	public Vector3 structureObjectWorldPos { get; private set; }

	public bool hasStartedExpiration { get; private set; }

	public GameDate expirationDate { get; private set; }

	public override Vector2 selectableSize => structureObj.size;

	public override Type serializedData => typeof(SaveDataManMadeStructure);

	public List<TileObject> dirtyObjects => m_dirtyObjects;

	public GameDate scheduledDirtProduction => m_scheduledDirtProduction;

	public virtual int maxWorkerCapacity => 0;

	public StructureTileObject structureTileObject => _structureTileObject;

	public override bool shouldBeLoadedOnMainThread => hasStartedExpiration;

	public InnerMapLight structureObjectLight
	{
		get
		{
			if (!_hasCheckedLight && m_innerMapLight == null)
			{
				_hasCheckedLight = true;
				m_innerMapLight = structureObj.GetComponentInChildren<InnerMapLight>(includeInactive: true);
			}
			return m_innerMapLight;
		}
	}

	public bool structureObjectHasOwnLight => structureObjectLight != null;

	protected ManMadeStructure(STRUCTURE_TYPE structureType, Region location)
		: base(structureType, location)
	{
		m_dirtyObjects = new List<TileObject>();
		assignedWorkerIDs = new List<string>();
		workerCheckins = new Dictionary<string, GameDate>();
	}

	protected ManMadeStructure(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		assignedWorkerIDs = new List<string>(data.assignedWorkerIDs);
		m_dirtyObjects = new List<TileObject>();
		hasStartedExpiration = data.hasStartedExpiration;
		expirationDate = data.expirationDate;
		workerCheckins = new Dictionary<string, GameDate>(data.workerCheckins);
	}

	public void ProcessWorkerBehaviour(Character p_worker, out JobQueueItem producedJob)
	{
		ProcessWorkStructureJobsByWorker(p_worker, out producedJob);
		CheckInWorker(p_worker);
	}

	protected virtual void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob)
	{
		producedJob = null;
	}

	public override void Initialize()
	{
		base.Initialize();
		TryStartExpiry();
	}

	protected override void SubscribeListeners(bool shouldLock)
	{
		if (!base.hasBeenDestroyed)
		{
			base.SubscribeListeners(shouldLock);
			Messenger.AddListener<ThinWall, int, bool>(StructureSignals.WALL_DAMAGED, OnWallDamaged, shouldLock);
			Messenger.AddListener<ThinWall, int, Character, bool>(StructureSignals.WALL_DAMAGED_BY, OnWallDamagedBy, shouldLock);
			Messenger.AddListener<ThinWall, int>(StructureSignals.WALL_REPAIRED, OnWallRepaired, shouldLock);
			Messenger.AddListener<TileObject, int>(TileObjectSignals.TILE_OBJECT_REPAIRED, base.OnObjectRepaired, shouldLock);
		}
	}

	protected override void UnsubscribeListeners()
	{
		base.UnsubscribeListeners();
		Messenger.RemoveListener<ThinWall, int, bool>(StructureSignals.WALL_DAMAGED, OnWallDamaged);
		Messenger.RemoveListener<ThinWall, int, Character, bool>(StructureSignals.WALL_DAMAGED_BY, OnWallDamagedBy);
		Messenger.RemoveListener<ThinWall, int>(StructureSignals.WALL_REPAIRED, OnWallRepaired);
		Messenger.RemoveListener<TileObject, int>(TileObjectSignals.TILE_OBJECT_REPAIRED, base.OnObjectRepaired);
	}

	public override bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null)
	{
		if (base.AddPOI(poi, tileLocation))
		{
			if (poi is StructureTileObject structureTileObject)
			{
				SetStructureTileObject(structureTileObject);
			}
			if (poi is TileObject tileObject)
			{
				if (tileObject.traitContainer.HasTrait("Dirty"))
				{
					AddDirtyObject(tileObject);
				}
				tileObject.eventDispatcher.SubscribeToTileObjectGainedTrait(this);
				tileObject.eventDispatcher.SubscribeToTileObjectLostTrait(this);
				if (tileObject.gridTileLocation != null && (poi is Torch || poi is DivineOrb))
				{
					ProcessInnerLight();
				}
			}
			return true;
		}
		return false;
	}

	public override bool RemovePOI(IPointOfInterest poi, Character removedBy = null, bool isPlayerSource = false)
	{
		if (base.RemovePOI(poi, removedBy, isPlayerSource))
		{
			if (poi is TileObject tileObject)
			{
				if (tileObject.traitContainer.HasTrait("Dirty"))
				{
					RemoveDirtyObject(tileObject);
				}
				tileObject.eventDispatcher.UnsubscribeToTileObjectGainedTrait(this);
				tileObject.eventDispatcher.UnsubscribeToTileObjectLostTrait(this);
				if (tileObject.tileObjectType == TILE_OBJECT_TYPE.MONSTER_SPAWNER)
				{
					TryStartExpiry();
				}
				if (tileObject is Torch || tileObject is DivineOrb)
				{
					ProcessInnerLight();
				}
			}
			return true;
		}
		return false;
	}

	public override bool RemovePOIWithoutDestroying(IPointOfInterest poi)
	{
		if (base.RemovePOIWithoutDestroying(poi))
		{
			if (poi is TileObject tileObject)
			{
				if (tileObject.traitContainer.HasTrait("Dirty"))
				{
					RemoveDirtyObject(tileObject);
				}
				tileObject.eventDispatcher.UnsubscribeToTileObjectGainedTrait(this);
				tileObject.eventDispatcher.UnsubscribeToTileObjectLostTrait(this);
				if (tileObject.tileObjectType == TILE_OBJECT_TYPE.MONSTER_SPAWNER)
				{
					TryStartExpiry();
				}
				if (tileObject is Torch || tileObject is DivineOrb)
				{
					ProcessInnerLight();
				}
			}
			return true;
		}
		return false;
	}

	public override bool RemovePOIDestroyVisualOnly(IPointOfInterest poi, Character remover = null)
	{
		if (base.RemovePOIDestroyVisualOnly(poi, remover))
		{
			if (poi is TileObject tileObject)
			{
				if (tileObject.traitContainer.HasTrait("Dirty"))
				{
					RemoveDirtyObject(tileObject);
				}
				tileObject.eventDispatcher.UnsubscribeToTileObjectGainedTrait(this);
				tileObject.eventDispatcher.UnsubscribeToTileObjectLostTrait(this);
				if (tileObject.tileObjectType == TILE_OBJECT_TYPE.MONSTER_SPAWNER)
				{
					TryStartExpiry();
				}
				if (tileObject is Torch || tileObject is DivineOrb)
				{
					ProcessInnerLight();
				}
			}
			return true;
		}
		return false;
	}

	public override bool LoadPOI(TileObject poi, LocationGridTile tileLocation)
	{
		if (base.LoadPOI(poi, tileLocation))
		{
			if (poi is StructureTileObject structureTileObject)
			{
				SetStructureTileObject(structureTileObject);
			}
			else if (poi is Torch || poi is DivineOrb)
			{
				ProcessInnerLight();
			}
			poi.eventDispatcher.SubscribeToTileObjectGainedTrait(this);
			poi.eventDispatcher.SubscribeToTileObjectLostTrait(this);
			return true;
		}
		return false;
	}

	private void SetStructureTileObject(StructureTileObject structureTileObject)
	{
		_structureTileObject = structureTileObject;
	}

	public virtual void OnUseStructureConnector(LocationGridTile p_usedConnector)
	{
	}

	private void OnWallRepaired(ThinWall structureWall, int amount)
	{
		if (structureWalls != null && structureWalls.Contains(structureWall))
		{
			structureObj.RescanPathfindingGridOfStructure(base.region.innerMap);
			CheckInteriorState();
		}
	}

	private void OnWallDamaged(ThinWall structureWall, int amount, bool isPlayerSource)
	{
		if (structureWalls != null && structureWalls.Contains(structureWall))
		{
			structureObj.RescanPathfindingGridOfStructure(base.region.innerMap);
			int count = structureWalls.Count;
			if ((float)structureWalls.Count((ThinWall wall) => wall.currentHP >= wall.maxHP) / (float)count <= 0.66f && _structureTileObject != null)
			{
				TryCreateSettlementRepairJob(out var _);
			}
		}
	}

	private void OnWallDamagedBy(ThinWall structureWall, int amount, Character p_responsibleCharacter, bool isPlayerSource)
	{
		if (structureWalls != null && structureWalls.Contains(structureWall))
		{
			structureObj.RescanPathfindingGridOfStructure(base.region.innerMap);
		}
	}

	protected void OnStructureDamaged()
	{
		if (base.currentHP > 0 && !base.hasBeenDestroyed && base.structureType.IsVillageStructure())
		{
			CheckInteriorState();
			if (_structureTileObject != null && base.settlementLocation is NPCSettlement && Mathf.RoundToInt((float)base.currentHP / (float)base.maxHP * 100f) < 50 && !base.hasBeenDestroyed)
			{
				TryCreateSettlementRepairJob(out var _);
			}
		}
	}

	private void CheckInteriorState()
	{
		if (structureWalls != null)
		{
			int num = Mathf.FloorToInt((float)structureWalls.Count * 0.7f);
			int num2 = structureWalls.Count((ThinWall wall) => wall.currentHP > 0);
			SetInteriorState(num2 > num);
		}
	}

	public override void CenterOnStructure()
	{
		if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != base.region.innerMap)
		{
			InnerMapManager.Instance.HideAreaMap();
		}
		if (!base.region.innerMap.isShowing)
		{
			InnerMapManager.Instance.ShowInnerMap(base.region);
		}
		if (structureObj != null)
		{
			InnerMapCameraMove.Instance.CenterCameraOn(structureObj.gameObject);
		}
	}

	public override void ShowSelectorOnStructure()
	{
		Selector.Instance.Select(this);
	}

	public bool TryCreateSettlementRepairJob(out JobQueueItem p_job)
	{
		if (base.settlementLocation is NPCSettlement nPCSettlement && !nPCSettlement.HasJob(JOB_TYPE.REPAIR, _structureTileObject))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPAIR, INTERACTION_TYPE.REPAIR_STRUCTURE, _structureTileObject, nPCSettlement);
			JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(nPCSettlement, goapPlanJob, INTERACTION_TYPE.TAKE_RESOURCE);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { structureObj.repairCost });
			nPCSettlement.AddToAvailableJobs(goapPlanJob);
			p_job = goapPlanJob;
			return true;
		}
		p_job = null;
		return false;
	}

	public bool ShouldBeRepaired()
	{
		if (Mathf.RoundToInt((float)base.currentHP / (float)base.maxHP * 100f) < 50 && !base.hasBeenDestroyed)
		{
			return true;
		}
		int count = structureWalls.Count;
		if ((float)structureWalls.Count((ThinWall wall) => wall.currentHP >= wall.maxHP) / (float)count <= 0.66f && _structureTileObject != null)
		{
			return true;
		}
		return false;
	}

	protected override void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false, bool shouldBeCleanedUp = true)
	{
		if (base.hasBeenDestroyed)
		{
			return;
		}
		if (_structureTileObject != null && base.settlementLocation is NPCSettlement nPCSettlement)
		{
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)_structureTileObject, "");
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, (IPointOfInterest)_structureTileObject, "");
			JobQueueItem job = nPCSettlement.GetJob(JOB_TYPE.REPAIR, _structureTileObject);
			if (job != null)
			{
				nPCSettlement.RemoveFromAvailableJobs(job);
			}
			if (structureObj != null)
			{
				AkSoundEngine.PostEvent("Play_Structure_Destroyed", structureObj.gameObject);
			}
		}
		BaseParticleEffect component = GameManager.Instance.CreateParticleEffectAt(GetCenterTile(), PARTICLE_EFFECT.Destroy_Structure).GetComponent<BaseParticleEffect>();
		if (structureObj != null)
		{
			component.SetSize(structureObj.size);
		}
		base.DestroyStructure(p_responsibleCharacter, isPlayerSource, shouldBeCleanedUp);
	}

	protected override void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		structureObj.OnOwnerStructureDestroyed(base.region.innerMap, this);
		Area area = base.occupiedArea;
		base.AfterStructureDestruction(p_responsibleCharacter);
		area?.TryRemoveAreaFromSettlementIfItIsNoLongerPartOfIt();
		RemoveAllAssignedWorkers();
	}

	public void SetWallObjects(List<ThinWall> wallObjects, WALL_RESOURCE resource)
	{
		structureWalls = wallObjects;
		wallsAreMadeOf = resource;
	}

	public virtual void SetStructureObject(LocationStructureObject structureObj)
	{
		this.structureObj = structureObj;
		templateName = structureObj.name;
		structureObjectWorldPos = structureObj.transform.position;
		Vector3 position = structureObj.transform.position;
		position.x -= 0.5f;
		position.y -= 0.5f;
		worldPosition = position;
	}

	public override void OnBuiltNewStructure()
	{
		base.OnBuiltNewStructure();
		if (ProducesDirt())
		{
			ScheduleDirtProduction();
		}
		ProcessInnerLight();
	}

	public bool DoesCharacterWorkHere(Character p_character)
	{
		return assignedWorkerIDs.Contains(p_character.persistentID);
	}

	public virtual bool AddAssignedWorker(Character p_worker)
	{
		if (p_worker.structureComponent.workPlaceStructure != null)
		{
			return false;
		}
		if (HasReachedMaxWorkerCapacity())
		{
			return false;
		}
		if (!assignedWorkerIDs.Contains(p_worker.persistentID))
		{
			assignedWorkerIDs.Add(p_worker.persistentID);
			workerCheckins.Add(p_worker.persistentID, GameManager.Instance.Today());
			p_worker.structureComponent.SetWorkPlaceStructure(this);
			Messenger.Broadcast(StructureSignals.ON_WORKER_HIRED, p_worker, this);
			if (assignedWorkerIDs.Count == 1)
			{
				Messenger.AddListener(Signals.DAY_STARTED, CheckWorkersOnDayStart);
			}
			return true;
		}
		return false;
	}

	public virtual bool RemoveAssignedWorker(Character p_worker)
	{
		if (assignedWorkerIDs.Remove(p_worker.persistentID))
		{
			workerCheckins.Remove(p_worker.persistentID);
			p_worker.structureComponent.SetWorkPlaceStructure(null);
			if (assignedWorkerIDs.Count == 0)
			{
				Messenger.RemoveListener(Signals.DAY_STARTED, CheckWorkersOnDayStart);
			}
			return true;
		}
		return false;
	}

	private void RemoveAllAssignedWorkers()
	{
		List<string> list = RuinarchListPool<string>.Claim();
		list.AddRange(assignedWorkerIDs);
		for (int i = 0; i < list.Count; i++)
		{
			string text = list[i];
			Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(text);
			RemoveAssignedWorker(characterByPersistentID);
		}
		RuinarchListPool<string>.Release(list);
	}

	public bool HasAssignedWorker()
	{
		return assignedWorkerIDs.Count > 0;
	}

	public bool HasMaxWorkerCapacity()
	{
		return maxWorkerCapacity > 0;
	}

	public bool HasReachedMaxWorkerCapacity()
	{
		if (HasMaxWorkerCapacity())
		{
			return assignedWorkerIDs.Count >= maxWorkerCapacity;
		}
		return false;
	}

	public bool HasWorkerThatIsNotAnEnemyOfCharacter(Character p_character)
	{
		for (int i = 0; i < assignedWorkerIDs.Count; i++)
		{
			string text = assignedWorkerIDs[i];
			Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(text);
			if (!p_character.relationshipContainer.IsEnemiesWith(characterByPersistentID))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool CanHireAWorker()
	{
		return false;
	}

	public virtual bool CanPurchaseFromHere(Character p_buyer, out bool needsToPay, out int buyerOpinionOfWorker)
	{
		needsToPay = true;
		buyerOpinionOfWorker = -100;
		return false;
	}

	protected bool DefaultCanPurchaseFromHereForSingleWorkerStructures(Character p_buyer, out bool needsToPay, out int buyerOpinionOfWorker)
	{
		if (HasAssignedWorker())
		{
			string text = assignedWorkerIDs[0];
			Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(text);
			if (characterByPersistentID == p_buyer)
			{
				needsToPay = false;
				buyerOpinionOfWorker = 100;
				return true;
			}
			if (p_buyer.relationshipContainer.HasRelationshipWith(characterByPersistentID, RELATIONSHIP_TYPE.LOVER))
			{
				needsToPay = false;
				buyerOpinionOfWorker = p_buyer.relationshipContainer.GetTotalOpinion(characterByPersistentID);
				return true;
			}
			if (p_buyer.relationshipContainer.IsFamilyMember(characterByPersistentID) && p_buyer.relationshipContainer.GetOpinionLabel(characterByPersistentID) == "Close Friend")
			{
				needsToPay = characterByPersistentID.relationshipContainer.GetOpinionLabel(p_buyer) != "Close Friend";
				buyerOpinionOfWorker = p_buyer.relationshipContainer.GetTotalOpinion(characterByPersistentID);
				return true;
			}
			if (!p_buyer.relationshipContainer.IsEnemiesWith(characterByPersistentID))
			{
				needsToPay = characterByPersistentID.relationshipContainer.GetOpinionLabel(p_buyer) != "Close Friend";
				buyerOpinionOfWorker = p_buyer.relationshipContainer.GetTotalOpinion(characterByPersistentID);
				return true;
			}
			needsToPay = true;
			buyerOpinionOfWorker = -100;
			return false;
		}
		needsToPay = true;
		buyerOpinionOfWorker = -100;
		return true;
	}

	public bool HasSettlementOrLocalResidentThatCanWorkHere()
	{
		return HasSettlementOrLocalResidentThatCanWorkHereBase();
	}

	protected virtual bool HasSettlementOrLocalResidentThatCanWorkHereBase()
	{
		return true;
	}

	protected void CheckInWorker(Character p_worker)
	{
		if (workerCheckins.ContainsKey(p_worker.persistentID))
		{
			workerCheckins[p_worker.persistentID] = GameManager.Instance.Today();
		}
	}

	private void CheckWorkersOnDayStart()
	{
		GameDate otherDate = GameManager.Instance.Today();
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < assignedWorkerIDs.Count; i++)
		{
			string key = assignedWorkerIDs[i];
			if (workerCheckins.ContainsKey(key))
			{
				int num = workerCheckins[key].GetTickDifference(otherDate) / 480;
				Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(key);
				if (num >= 3)
				{
					list.Add(characterByPersistentID);
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Character p_worker = list[j];
			RemoveAssignedWorker(p_worker);
		}
		RuinarchListPool<Character>.Release(list);
	}

	public virtual bool CanBeDamagedByPlayerSpells()
	{
		return true;
	}

	public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource)
	{
		base.OnTileDamaged(tile, amount, isPlayerSource);
		if (DoesTileContributeToDamage(tile))
		{
			AdjustHP(amount, null, isPlayerSource);
			OnStructureDamaged();
		}
	}

	public override bool DoesTileContributeToDamage(LocationGridTile tile)
	{
		if (tile.groundType == LocationGridTile.Ground_Type.Structure_Stone || tile.groundType == LocationGridTile.Ground_Type.Wood || tile.groundType == LocationGridTile.Ground_Type.Demon_Stone || tile.groundType == LocationGridTile.Ground_Type.Cobble || tile.groundType == LocationGridTile.Ground_Type.Ruined_Stone)
		{
			return true;
		}
		return false;
	}

	public override void OnTileRepaired(LocationGridTile tile, int amount)
	{
		if (!base.hasBeenDestroyed)
		{
			AdjustHP(amount);
			if (tile.tileObjectComponent.genericTileObject.currentHP >= tile.tileObjectComponent.genericTileObject.maxHP)
			{
				structureObj?.ApplyGroundTileAssetForTile(tile);
				tile.CreateSeamlessEdgesForSelfAndNeighbours();
			}
		}
	}

	protected override void AfterCharacterAddedToLocation(Character p_character)
	{
		base.AfterCharacterAddedToLocation(p_character);
		if (p_character.isNormalCharacter && dirtyObjects.Count >= 2)
		{
			p_character.traitContainer.AddTrait(p_character, "Uncomfortable");
		}
	}

	protected override void OnRemoveResident(Character p_resident)
	{
		base.OnRemoveResident(p_resident);
		TryStartExpiry();
	}

	private void AddDirtyObject(TileObject p_object)
	{
		if (m_dirtyObjects.Contains(p_object))
		{
			return;
		}
		m_dirtyObjects.Add(p_object);
		if (m_dirtyObjects.Count != 2)
		{
			return;
		}
		for (int i = 0; i < base.charactersHere.Count; i++)
		{
			Character character = base.charactersHere[i];
			if (character.isNormalCharacter)
			{
				character.traitContainer.AddTrait(character, "Uncomfortable");
			}
		}
	}

	private void RemoveDirtyObject(TileObject p_object)
	{
		m_dirtyObjects.Remove(p_object);
	}

	private bool ProducesDirt()
	{
		if (base.structureType.IsVillageStructure() && base.structureType != STRUCTURE_TYPE.CITY_CENTER && base.structureType != STRUCTURE_TYPE.CEMETERY && base.structureType != STRUCTURE_TYPE.PRISON && base.structureType != STRUCTURE_TYPE.MINE)
		{
			return base.structureType != STRUCTURE_TYPE.LUMBERYARD;
		}
		return false;
	}

	private void ScheduleDirtProduction()
	{
		GameDate gameDate = GameManager.Instance.Today();
		gameDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(12));
		m_scheduledDirtProduction = gameDate;
		SchedulingManager.Instance.AddEntry(gameDate, ProduceDirtOnSchedule, this);
	}

	private void LoadDirtProduction(GameDate p_date)
	{
		m_scheduledDirtProduction = p_date;
		SchedulingManager.Instance.AddEntry(p_date, ProduceDirtOnSchedule, this);
	}

	private void ProduceDirtOnSchedule()
	{
		if (!base.hasBeenDestroyed)
		{
			List<TileObject> list = RuinarchListPool<TileObject>.Claim();
			PopulateBuiltTileObjects(list);
			if (list.Count > 0)
			{
				TileObject randomElement = CollectionUtilities.GetRandomElement(list);
				randomElement.traitContainer.AddTrait(randomElement, "Dirty");
			}
			ScheduleDirtProduction();
		}
	}

	protected bool TryCreateCleanJob(Character p_worker, out JobQueueItem producedJob)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		PopulateBuiltTileObjectsThatHaveTrait(list, "Wet");
		List<TileObject> list2 = RuinarchListPool<TileObject>.Claim();
		PopulateBuiltTileObjectsThatHaveTrait(list2, "Burnt");
		if (dirtyObjects.Count > 0 || list.Count > 0 || list2.Count > 0)
		{
			int chance = 100;
			if (p_worker.traitContainer.HasTrait("Lazy"))
			{
				chance = 6;
			}
			if (GameUtilities.RollChance(chance))
			{
				List<TileObject> list3 = RuinarchListPool<TileObject>.Claim();
				list3.AddRange(dirtyObjects);
				list3.AddRange(list);
				list3.AddRange(list2);
				TileObject randomElement = CollectionUtilities.GetRandomElement(list3);
				RuinarchListPool<TileObject>.Release(list3);
				if (p_worker.jobComponent.TryCreateCleanItemJob(randomElement, out producedJob))
				{
					RuinarchListPool<TileObject>.Release(list);
					RuinarchListPool<TileObject>.Release(list2);
					return true;
				}
			}
		}
		RuinarchListPool<TileObject>.Release(list);
		RuinarchListPool<TileObject>.Release(list2);
		producedJob = null;
		return false;
	}

	public override string GetTestingInfo()
	{
		string testingInfo = base.GetTestingInfo();
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < assignedWorkerIDs.Count; i++)
		{
			string text = assignedWorkerIDs[i];
			Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(text);
			list.Add(characterByPersistentID);
		}
		testingInfo = testingInfo + "\n Assigned Workers: " + list?.ComafyList();
		RuinarchListPool<Character>.Release(list);
		return testingInfo + "\n Dirty Objects: " + m_dirtyObjects?.ComafyList();
	}

	public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadReferences(saveDataLocationStructure);
		for (int i = 0; i < assignedWorkerIDs.Count; i++)
		{
			string text = assignedWorkerIDs[i];
			DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(text)?.structureComponent.SetWorkPlaceStructure(this);
		}
		if (assignedWorkerIDs.Count > 0)
		{
			Messenger.AddListener(Signals.DAY_STARTED, CheckWorkersOnDayStart);
		}
		SaveDataManMadeStructure saveDataManMadeStructure = saveDataLocationStructure as SaveDataManMadeStructure;
		if (saveDataManMadeStructure.dirtyObjects != null)
		{
			for (int j = 0; j < saveDataManMadeStructure.dirtyObjects.Length; j++)
			{
				string text2 = saveDataManMadeStructure.dirtyObjects[j];
				TileObject tileObjectByPersistentID = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(text2);
				dirtyObjects.Add(tileObjectByPersistentID);
			}
		}
		if (saveDataManMadeStructure.scheduledDirtProduction.hasValue)
		{
			LoadDirtProduction(saveDataManMadeStructure.scheduledDirtProduction);
		}
	}

	public override void LoadStructureSecondWaveInMainThread(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadStructureSecondWaveInMainThread(saveDataLocationStructure);
		if (hasStartedExpiration)
		{
			m_expirationScheduleKey = SchedulingManager.Instance.AddEntry(expirationDate, EvaluateExpiration, this);
		}
	}

	public override void OnDoneLoadStructure()
	{
		base.OnDoneLoadStructure();
		ProcessInnerLight();
	}

	public void OnTileObjectGainedTrait(TileObject p_tileObject, Trait p_trait)
	{
		if (p_trait is Dirty)
		{
			AddDirtyObject(p_tileObject);
		}
	}

	public void OnTileObjectLostTrait(TileObject p_tileObject, Trait p_trait)
	{
		if (p_trait is Dirty && !p_tileObject.traitContainer.HasTrait("Dirty"))
		{
			RemoveDirtyObject(p_tileObject);
		}
	}

	public override bool TryGetMinimapColorForTileInStructure(LocationGridTile p_tile, out Color p_color)
	{
		p_color = GameUtilities.StructureMinimapColor;
		return true;
	}

	protected override void OnTileAddedToStructure(LocationGridTile tile)
	{
		base.OnTileAddedToStructure(tile);
		tile.UpdateMinimapVisual(this);
	}

	protected override void OnTileRemovedFromStructure(LocationGridTile tile, LocationStructure removedFrom)
	{
		base.OnTileRemovedFromStructure(tile, removedFrom);
		tile.UpdateMinimapVisual(null);
	}

	public override LocationGridTile GetCenterTile()
	{
		if (structureObj != null)
		{
			return GridMap.Instance.mainRegion.innerMap.GetTileFromWorldPosition(structureObj.worldPosition);
		}
		return CollectionUtilities.GetRandomElement(base.tiles);
	}

	public void PopulateBorderTiles(List<LocationGridTile> p_borderTiles)
	{
		LocationGridTile centerTile = GetCenterTile();
		int num = centerTile.localPlace.x - structureObj.center.x;
		int num2 = centerTile.localPlace.y - structureObj.center.y;
		List<Vector2Int> borderCoordinates = structureObj.borderCoordinates;
		for (int i = 0; i < borderCoordinates.Count; i++)
		{
			Vector2Int vector2Int = borderCoordinates[i];
			int xPos = num + vector2Int.x;
			int yPos = num2 + vector2Int.y;
			LocationGridTile tileFromMapCoordinates = GridMap.Instance.mainRegion.innerMap.GetTileFromMapCoordinates(xPos, yPos);
			if (tileFromMapCoordinates != null)
			{
				p_borderTiles.Add(tileFromMapCoordinates);
			}
		}
	}

	private void TryStartExpiry()
	{
		if (!hasStartedExpiration && !base.hasBeenDestroyed && ShouldExpire())
		{
			expirationDate = GameManager.Instance.Today().AddDays(5);
			m_expirationScheduleKey = SchedulingManager.Instance.AddEntry(expirationDate, EvaluateExpiration, this);
			hasStartedExpiration = true;
		}
	}

	private void EvaluateExpiration()
	{
		hasStartedExpiration = false;
		if (ShouldExpire())
		{
			Expire();
		}
	}

	private void Expire()
	{
		AdjustHP(-base.maxHP);
	}

	private bool ShouldExpire()
	{
		if (base.structureType.IsSpecialStructure() && !HasAliveResident(null))
		{
			return !HasTileObjectOfType(TILE_OBJECT_TYPE.MONSTER_SPAWNER);
		}
		return false;
	}

	private void ProcessInnerLight()
	{
		if (!(structureObjectLight != null))
		{
			return;
		}
		List<TileObject> tileObjectsOfType = GetTileObjectsOfType(TILE_OBJECT_TYPE.TORCH);
		List<TileObject> tileObjectsOfType2 = GetTileObjectsOfType(TILE_OBJECT_TYPE.DIVINE_ORB);
		List<TileObject> tileObjectsOfType3 = GetTileObjectsOfType(TILE_OBJECT_TYPE.BRAZIER);
		bool active = false;
		if (base.structureType == STRUCTURE_TYPE.DWELLING)
		{
			if (tileObjectsOfType != null && tileObjectsOfType.Count > 0)
			{
				active = true;
			}
			else if (tileObjectsOfType2 != null && tileObjectsOfType2.Count > 0)
			{
				active = true;
			}
			else if (tileObjectsOfType3 != null && tileObjectsOfType3.Count > 0)
			{
				active = true;
			}
		}
		else if (base.structureType.IsVillageStructure())
		{
			active = true;
		}
		structureObjectLight.gameObject.SetActive(active);
	}

	public override void CleanUp()
	{
		if (DatabaseManager.Instance.structureDatabase.HasStructure(base.persistentID))
		{
			if (expirationDate.hasValue && !string.IsNullOrEmpty(m_expirationScheduleKey))
			{
				SchedulingManager.Instance.RemoveSpecificEntry(m_expirationScheduleKey);
			}
			SetStructureTileObject(null);
			structureWalls?.Clear();
			m_dirtyObjects?.Clear();
			assignedWorkerIDs?.Clear();
			structureObj = null;
			m_innerMapLight = null;
			base.CleanUp();
		}
	}
}
