using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Jobs;
using Locations.Settlements;
using Locations.Settlements.Components;
using Traits;
using UnityEngine;
using UtilityScripts;

public class SettlementJobTriggerComponent : JobTriggerComponent, NPCSettlementEventDispatcher.ITileListener
{
	private readonly NPCSettlement _owner;

	private Dictionary<SETTLEMENT_JOB_TRIGGER, SettlementJobTrigger> _jobTriggers;

	private const int MAX_ACTIVE_BUILD_JOBS = 3;

	private static STRUCTURE_TYPE[] _miscStructureTypes = new STRUCTURE_TYPE[4]
	{
		STRUCTURE_TYPE.TAVERN,
		STRUCTURE_TYPE.HOSPICE,
		STRUCTURE_TYPE.PRISON,
		STRUCTURE_TYPE.CEMETERY
	};

	public List<LocationGridTile> poisonedTiles { get; private set; }

	public List<Character> poisonCleansers { get; private set; }

	public List<Character> tileDryers { get; private set; }

	public List<Character> dousers { get; private set; }

	public SettlementJobTriggerComponent(NPCSettlement owner)
	{
		_owner = owner;
		poisonedTiles = new List<LocationGridTile>(20);
		poisonCleansers = new List<Character>(10);
		tileDryers = new List<Character>(10);
		dousers = new List<Character>(10);
		_jobTriggers = new Dictionary<SETTLEMENT_JOB_TRIGGER, SettlementJobTrigger>();
	}

	public void AddJobTrigger(NPCSettlement p_settlement, SETTLEMENT_JOB_TRIGGER p_jobTriggerType)
	{
		if (!_jobTriggers.ContainsKey(p_jobTriggerType))
		{
			SettlementJobTrigger settlementJobTrigger = CreateSettlementJobTrigger<SettlementJobTrigger>(p_jobTriggerType);
			_jobTriggers.Add(p_jobTriggerType, settlementJobTrigger);
			settlementJobTrigger.HookTriggerToSettlement(p_settlement);
		}
	}

	public void RemoveJobTrigger(NPCSettlement p_settlement, SETTLEMENT_JOB_TRIGGER p_jobTriggerType)
	{
		if (_jobTriggers.ContainsKey(p_jobTriggerType))
		{
			_jobTriggers[p_jobTriggerType].UnhookTriggerToSettlement(p_settlement);
			_jobTriggers.Remove(p_jobTriggerType);
		}
	}

	private T CreateSettlementJobTrigger<T>(SETTLEMENT_JOB_TRIGGER p_jobTriggerType) where T : SettlementJobTrigger
	{
		return Activator.CreateInstance(Type.GetType("Jobs." + p_jobTriggerType.ToStringEnum() + "_Job_Trigger, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")) as T;
	}

	public void SubscribeToVillageListeners()
	{
		Messenger.AddListener(Signals.HOUR_STARTED, HourlyJobActions);
		Messenger.AddListener<TileObject, int, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED, OnTileObjectDamaged);
		Messenger.AddListener<TileObject>(TileObjectSignals.TILE_OBJECT_FULLY_REPAIRED, OnTileObjectFullyRepaired);
		Messenger.AddListener<TileObject, LocationGridTile>(GridTileSignals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
		Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		Messenger.AddListener<Table>(StructureSignals.FOOD_IN_DWELLING_CHANGED, OnFoodInDwellingChanged);
		Messenger.AddListener<BurningSource>(InnerMapSignals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
		Messenger.AddListener(Signals.GAME_LOADED, OnGameLoadedVillage);
		Messenger.AddListener<BaseSettlement>(CharacterSignals.TRY_CREATE_BURY_JOBS, TryCreateBuryJobs);
		Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
		_owner.npcSettlementEventDispatcher.SubscribeToTileRemovedEvent(this);
	}

	public void UnsubscribeFromVillageListeners()
	{
		Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyJobActions);
		Messenger.RemoveListener<TileObject, int, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED, OnTileObjectDamaged);
		Messenger.RemoveListener<TileObject>(TileObjectSignals.TILE_OBJECT_FULLY_REPAIRED, OnTileObjectFullyRepaired);
		Messenger.RemoveListener<TileObject, LocationGridTile>(GridTileSignals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
		Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.RemoveListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.RemoveListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		Messenger.RemoveListener<Table>(StructureSignals.FOOD_IN_DWELLING_CHANGED, OnFoodInDwellingChanged);
		Messenger.RemoveListener<BurningSource>(InnerMapSignals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
		Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoadedVillage);
		Messenger.RemoveListener<BaseSettlement>(CharacterSignals.TRY_CREATE_BURY_JOBS, TryCreateBuryJobs);
		Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
		_owner.npcSettlementEventDispatcher.UnsubscribeToTileRemovedEvent(this);
		StopCraftWaterWellCheck();
		StopCraftTownMessageBoardCheck();
	}

	public void OnSettlementDestroyed()
	{
		poisonedTiles.Clear();
		poisonCleansers.Clear();
		tileDryers.Clear();
		dousers.Clear();
	}

	public void OnSettlementAbandoned()
	{
		List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim(10);
		_owner.PopulateJobsOfType(list, JOB_TYPE.CREATE_WARD_LIGHT);
		for (int i = 0; i < list.Count; i++)
		{
			_owner.RemoveFromAvailableJobs(list[i]);
		}
		RuinarchListPool<JobQueueItem>.Release(list);
		List<JobQueueItem> list2 = RuinarchListPool<JobQueueItem>.Claim(10);
		_owner.PopulateJobsOfType(list2, JOB_TYPE.PLACE_BLUEPRINT);
		for (int j = 0; j < list2.Count; j++)
		{
			_owner.RemoveFromAvailableJobs(list2[j]);
		}
		RuinarchListPool<JobQueueItem>.Release(list2);
	}

	private void OnDayStarted()
	{
		TryCreateCraftWardLightJob();
	}

	private void TryCreateBuryJobs(BaseSettlement p_settlement)
	{
		if (p_settlement != _owner)
		{
			return;
		}
		for (int i = 0; i < _owner.areas.Count; i++)
		{
			Area area = _owner.areas[i];
			for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
			{
				Character character = area.locationCharacterTracker.charactersAtLocation[j];
				if (character.isDead)
				{
					character.jobComponent.TriggerBuryMe();
				}
			}
		}
	}

	public void SubscribeToDungeonListeners()
	{
		Messenger.AddListener(Signals.GAME_LOADED, OnGameLoadedDungeon);
	}

	public void UnsubscribeFromDungeonListeners()
	{
	}

	private void OnGameLoadedVillage()
	{
		Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoadedVillage);
	}

	private void OnGameLoadedDungeon()
	{
		Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoadedDungeon);
	}

	private void HourlyJobActions()
	{
		CheckIfThereAreTilesToPurify();
		CheckForPlaceBlueprint();
	}

	private void OnTileObjectDamaged(TileObject tileObject, int amount, bool isPlayerSource)
	{
		if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.IsPartOfSettlement(_owner) && tileObject.tileObjectType.CanBeRepaired())
		{
			TryCreateRepairTileObjectJob(tileObject);
		}
	}

	private void OnTileObjectFullyRepaired(TileObject tileObject)
	{
		if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.IsPartOfSettlement(_owner))
		{
			Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.REPAIR, (IPointOfInterest)tileObject);
		}
	}

	private void OnTileObjectPlaced(TileObject tileObject, LocationGridTile tile)
	{
		if (tileObject is ResourcePile { resourceInPile: >0 } resourcePile)
		{
			Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.HAUL, (IPointOfInterest)resourcePile);
		}
	}

	private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure)
	{
		if (structure.settlementLocation == _owner && structure.settlementLocation is NPCSettlement nPCSettlement && structure == nPCSettlement.prison)
		{
			TryCreateJudgePrisoner(character);
		}
	}

	private void OnTraitableGainedTrait(ITraitable traitable, Trait trait)
	{
		if (traitable is Character target)
		{
			if (trait is Restrained)
			{
				TryCreateJudgePrisoner(target);
			}
		}
		else if (traitable is TileObject && traitable is GenericTileObject && traitable.gridTileLocation.IsPartOfSettlement(_owner) && trait is Poisoned)
		{
			AddPoisonedTile(traitable.gridTileLocation);
		}
	}

	private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character character)
	{
		if (traitable is TileObject && traitable is GenericTileObject && traitable.gridTileLocation.IsPartOfSettlement(_owner) && trait is Poisoned)
		{
			RemovePoisonedTile(traitable.gridTileLocation);
		}
	}

	private void OnFoodInDwellingChanged(Table table)
	{
		if (table.gridTileLocation.IsPartOfSettlement(_owner))
		{
			TryTriggerObtainPersonalFood(table);
		}
	}

	private void OnBurningSourceInactive(BurningSource burningSource)
	{
		CheckDouseFireJobsValidity();
	}

	public void OnItemRemovedFromStructure(TileObject item, LocationStructure structure, LocationGridTile removedFrom)
	{
		if (structure is CityCenter && !structure.hasBeenDestroyed)
		{
			if (item is WaterWell)
			{
				TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_WELL);
				structure.AddPOI(tileObject, removedFrom);
				tileObject.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
				StartCraftWaterWellCheck();
			}
			else if (item is TownMessageBoard)
			{
				TileObject tileObject2 = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.TOWN_MESSAGE_BOARD);
				structure.AddPOI(tileObject2, removedFrom);
				tileObject2.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
				StartCraftTownMessageBoardCheck();
			}
		}
	}

	public void OnItemAddedToStructure(TileObject item, LocationStructure structure)
	{
		if (structure is CityCenter)
		{
			if (item is WaterWell)
			{
				CheckIfShouldStopWaterWellCheck();
			}
			else if (item is TownMessageBoard)
			{
				CheckIfShouldStopTownMessageBoardCheck();
			}
		}
	}

	public void OnSettlementAreaRemoved(Area p_area, NPCSettlement p_settlement)
	{
		for (int i = 0; i < poisonedTiles.Count; i++)
		{
			LocationGridTile locationGridTile = poisonedTiles[i];
			if (locationGridTile.area == p_area)
			{
				RemovePoisonedTile(locationGridTile);
				i--;
			}
		}
	}

	public bool HasTotalResource(RESOURCE resourceType, int neededResource)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		_owner.mainStorage.PopulateBuiltTileObjectsOfType<ResourcePile>(list);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is ResourcePile resourcePile && resourcePile.providedResource == resourceType && resourcePile.resourceInPile >= neededResource)
			{
				RuinarchListPool<TileObject>.Release(list);
				return true;
			}
		}
		RuinarchListPool<TileObject>.Release(list);
		List<LocationStructure> structuresOfType = _owner.GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);
		if (structuresOfType != null)
		{
			for (int j = 0; j < structuresOfType.Count; j++)
			{
				LocationStructure locationStructure = structuresOfType[j];
				list = RuinarchListPool<TileObject>.Claim();
				locationStructure.PopulateBuiltTileObjectsOfType<ResourcePile>(list);
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k] is ResourcePile resourcePile2 && resourcePile2.resourceInPile >= neededResource)
					{
						RuinarchListPool<TileObject>.Release(list);
						return true;
					}
				}
				RuinarchListPool<TileObject>.Release(list);
			}
		}
		List<LocationStructure> structuresOfType2 = _owner.GetStructuresOfType(STRUCTURE_TYPE.MINE);
		if (structuresOfType2 != null)
		{
			for (int l = 0; l < structuresOfType2.Count; l++)
			{
				LocationStructure locationStructure2 = structuresOfType2[l];
				list = RuinarchListPool<TileObject>.Claim();
				locationStructure2.PopulateBuiltTileObjectsOfType<ResourcePile>(list);
				for (int m = 0; m < list.Count; m++)
				{
					if (list[m] is ResourcePile resourcePile3 && resourcePile3.resourceInPile >= neededResource)
					{
						RuinarchListPool<TileObject>.Release(list);
						return true;
					}
				}
				RuinarchListPool<TileObject>.Release(list);
			}
		}
		return false;
	}

	public bool HasAccessToResource(RESOURCE p_resource)
	{
		return p_resource switch
		{
			RESOURCE.STONE => _owner.HasStructure(STRUCTURE_TYPE.MINE), 
			RESOURCE.WOOD => _owner.HasStructure(STRUCTURE_TYPE.LUMBERYARD), 
			RESOURCE.FOOD => _owner.HasFoodProducingStructure(), 
			_ => false, 
		};
	}

	private void TryCreateRepairTileObjectJob(TileObject target)
	{
		if (!_owner.HasJob(JOB_TYPE.REPAIR, target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPAIR, INTERACTION_TYPE.REPAIR, target, _owner);
			goapPlanJob.SetCanTakeThisJobChecker("CanTakeRepair");
			goapPlanJob.SetStillApplicableChecker("IsRepairApplicable");
			JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(_owner, goapPlanJob, INTERACTION_TYPE.TAKE_RESOURCE);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { TileObjectDB.GetTileObjectData(target.tileObjectType).repairCost });
			_owner.AddToAvailableJobs(goapPlanJob);
		}
	}

	public void TryCreateHaulJob(ResourcePile target)
	{
		if (!_owner.HasJob(JOB_TYPE.HAUL, target) && target.gridTileLocation.parentMap.region == _owner.region && (!target.gridTileLocation.IsPartOfSettlement(out var settlement) || _owner == settlement || settlement.owner == null || (!settlement.owner.isMajorNonPlayer && settlement.owner.factionType.type != FACTION_TYPE.Ratmen)))
		{
			ResourcePile resourcePileObjectWithLowestCount = _owner.mainStorage.GetResourcePileObjectWithLowestCount(target.tileObjectType);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), target, _owner);
			if (resourcePileObjectWithLowestCount != null)
			{
				goapPlanJob.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[1] { resourcePileObjectWithLowestCount });
			}
			goapPlanJob.SetStillApplicableChecker("IsHaulApplicable");
			goapPlanJob.SetCanTakeThisJobChecker("CanTakeHaul");
			_owner.AddToAvailableJobs(goapPlanJob);
		}
	}

	public void TryCreateHaulJobForItems(TileObject target, LocationStructure dropLocation)
	{
		if (!_owner.HasJob(JOB_TYPE.HAUL, target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, INTERACTION_TYPE.DROP_ITEM, target, _owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_ITEM, new object[1] { dropLocation });
			goapPlanJob.SetStillApplicableChecker("IsHaulApplicable");
			goapPlanJob.SetCanTakeThisJobChecker("CanTakeHaul");
			_owner.AddToAvailableJobs(goapPlanJob);
		}
	}

	public void TryCreateJudgePrisoner(Character target)
	{
		if (target.traitContainer.HasTrait("Restrained") && target.traitContainer.HasTrait("Criminal") && target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(_owner) && _owner.owner != null)
		{
			NPCSettlement owner = _owner;
			if (owner != null && owner.prison == target.currentStructure && !owner.HasJob(JOB_TYPE.JUDGE_PRISONER, target) && !target.HasJobTargetingThis(JOB_TYPE.JUDGE_PRISONER) && target.crimeComponent.IsWantedBy(_owner.owner))
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.JUDGE_CHARACTER, target, _owner);
				goapPlanJob.SetCanTakeThisJobChecker("CanTakeJudgement");
				goapPlanJob.SetStillApplicableChecker("IsJudgeApplicable");
				_owner.AddToAvailableJobs(goapPlanJob);
			}
		}
	}

	public void TryCreateApprehend(Character target)
	{
		if (target.currentSettlement == _owner && _owner.owner != null && target.traitContainer.HasTrait("Criminal") && !target.isDead && !_owner.HasJob(JOB_TYPE.APPREHEND, target) && target.crimeComponent.IsWantedBy(_owner.owner) && target.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(_owner) && (target.gridTileLocation.structure != _owner.prison || !target.traitContainer.HasTrait("Restrained")))
		{
			CreateApprehendJob(JOB_TYPE.APPREHEND, target);
		}
	}

	public JobQueueItem CreateApprehendJob(JOB_TYPE p_jobType, Character p_target)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(p_jobType, INTERACTION_TYPE.DROP_RESTRAINED, p_target, _owner);
		goapPlanJob.SetCanTakeThisJobChecker("CanTakeApprehend");
		goapPlanJob.SetStillApplicableChecker("IsApprehendSettlementApplicable");
		goapPlanJob.SetShouldBeRemovedFromSettlementWhenUnassigned(state: true);
		goapPlanJob.SetDoNotRecalculate(state: true);
		goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { _owner.prison });
		_owner.AddToAvailableJobs(goapPlanJob);
		return goapPlanJob;
	}

	private void TryTriggerObtainPersonalFood(Table table)
	{
		if (table.food < 20 && !_owner.HasJob(JOB_TYPE.OBTAIN_PERSONAL_FOOD, table))
		{
			int num = 30 - table.food;
			GoapEffect goapEffectData = InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Food Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OBTAIN_PERSONAL_FOOD, goapEffectData, table, _owner);
			goapPlanJob.SetCanTakeThisJobChecker("CanTakeObtainPersonalFood");
			goapPlanJob.SetStillApplicableChecker("IsObtainPersonalFoodApplicable");
			goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { num });
			JobUtilities.PopulatePriorityLocationsForTakingPersonalItem(_owner, goapPlanJob, INTERACTION_TYPE.TAKE_RESOURCE);
			_owner.AddToAvailableJobs(goapPlanJob);
		}
	}

	public GoapPlanJob CreateRestrainJob(Character target)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RESTRAIN, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Restrained", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), target, _owner);
		goapPlanJob.SetStillApplicableChecker("IsRestrainApplicable");
		goapPlanJob.SetCanTakeThisJobChecker("CanTakeRestrain");
		goapPlanJob.SetShouldBeRemovedFromSettlementWhenUnassigned(state: true);
		goapPlanJob.SetDoNotRecalculate(state: true);
		_owner.AddToAvailableJobs(goapPlanJob, 0);
		return goapPlanJob;
	}

	public void TriggerDouseFire()
	{
		if (_owner.firesToDouseInSettlement.Count <= 0)
		{
			return;
		}
		int num = dousers.Count + _owner.GetNumberOfJobsWith(JOB_TYPE.DOUSE_FIRE);
		int num2 = 3;
		if (num < num2)
		{
			int num3 = num2 - num;
			for (int i = 0; i < num3; i++)
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DOUSE_FIRE, INTERACTION_TYPE.START_DOUSE, null, _owner);
				goapPlanJob.SetCanTakeThisJobChecker("CanTakeRemoveFire");
				_owner.AddToAvailableJobs(goapPlanJob, 0);
			}
		}
	}

	public void CheckDouseFireJobsValidity()
	{
		if (_owner.firesToDouseInSettlement.Count != 0)
		{
			return;
		}
		List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim();
		_owner.PopulateJobsOfType(list, JOB_TYPE.DOUSE_FIRE);
		for (int i = 0; i < list.Count; i++)
		{
			JobQueueItem jobQueueItem = list[i];
			if (jobQueueItem.assignedCharacter == null)
			{
				jobQueueItem.ForceCancelJob("No_More_Fires");
			}
		}
		RuinarchListPool<JobQueueItem>.Release(list);
	}

	public void OnTakeDouseFireJob(Character character)
	{
		character.behaviourComponent.SetDouseFireSettlement(_owner);
	}

	public void AddDouser(Character character)
	{
		dousers.Add(character);
	}

	public void RemoveDouser(Character character)
	{
		dousers.Remove(character);
	}

	private void AddPoisonedTile(LocationGridTile tile)
	{
		if (!poisonedTiles.Contains(tile))
		{
			poisonedTiles.Add(tile);
		}
	}

	private void RemovePoisonedTile(LocationGridTile tile)
	{
		if (poisonedTiles.Remove(tile))
		{
			CheckCleanseTilesValidity();
		}
	}

	public void TriggerCleanseTiles()
	{
		if (poisonedTiles.Count <= 0)
		{
			return;
		}
		int num = poisonCleansers.Count + _owner.GetNumberOfJobsWith(JOB_TYPE.CLEANSE_TILES);
		int num2 = 1;
		if (num < num2)
		{
			int num3 = num2 - num;
			for (int i = 0; i < num3; i++)
			{
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CLEANSE_TILES, INTERACTION_TYPE.START_CLEANSE, null, _owner);
				_owner.AddToAvailableJobs(job);
			}
		}
	}

	public void OnTakeCleanseTileJob(Character character)
	{
		character.behaviourComponent.SetCleansingTilesForSettlement(_owner);
	}

	private void CheckCleanseTilesValidity()
	{
		if (poisonedTiles.Count != 0)
		{
			return;
		}
		List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim();
		_owner.PopulateJobsOfType(list, JOB_TYPE.CLEANSE_TILES);
		for (int i = 0; i < list.Count; i++)
		{
			JobQueueItem jobQueueItem = list[i];
			if (jobQueueItem.assignedCharacter == null)
			{
				jobQueueItem.ForceCancelJob("No_Poisoned_Floors");
			}
		}
		RuinarchListPool<JobQueueItem>.Release(list);
	}

	public void AddPoisonCleanser(Character character)
	{
		poisonCleansers.Add(character);
	}

	public void RemovePoisonCleanser(Character character)
	{
		poisonCleansers.Remove(character);
	}

	private void CheckIfThereAreTilesToPurify()
	{
		if ((_owner.owner == null || !_owner.owner.IsFriendlyWith(PlayerManager.Instance.player.playerFaction)) && AreThereTilesToPurify())
		{
			TriggerStartPurifyGround();
		}
	}

	public LocationGridTile GetFirstTileToPurify(Character p_relativeTo = null)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		LocationGridTile locationGridTile = null;
		for (int i = 0; i < _owner.allStructures.Count; i++)
		{
			if (locationGridTile != null)
			{
				break;
			}
			LocationStructure locationStructure = _owner.allStructures[i];
			list.Clear();
			if (locationStructure is ManMadeStructure manMadeStructure)
			{
				manMadeStructure.PopulateBorderTiles(list);
			}
			else if (locationStructure is NaturalStructureWithStructureObject naturalStructureWithStructureObject)
			{
				naturalStructureWithStructureObject.PopulateBorderTiles(list);
			}
			if (list.Count > 0)
			{
				for (int j = 0; j < list.Count; j++)
				{
					LocationGridTile locationGridTile2 = list[j];
					if (locationGridTile2.corruptionComponent.isCorrupted && !locationGridTile2.HasNeighbourStructure(STRUCTURE_TYPE.THE_PORTAL) && locationGridTile2.structure.structureType != STRUCTURE_TYPE.THE_PORTAL && locationGridTile2.IsPassable())
					{
						if (p_relativeTo == null || p_relativeTo.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile2))
						{
							locationGridTile = locationGridTile2;
							break;
						}
						continue;
					}
					for (int k = 0; k < locationGridTile2.neighbourList.Count; k++)
					{
						LocationGridTile locationGridTile3 = locationGridTile2.neighbourList[k];
						if (locationGridTile3.structure != locationStructure && locationGridTile3.corruptionComponent.isCorrupted && !locationGridTile3.HasNeighbourStructure(STRUCTURE_TYPE.THE_PORTAL) && locationGridTile3.structure.structureType != STRUCTURE_TYPE.THE_PORTAL && locationGridTile3.IsPassable() && (p_relativeTo == null || p_relativeTo.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile3)))
						{
							locationGridTile = locationGridTile3;
							break;
						}
					}
					if (locationGridTile != null)
					{
						break;
					}
				}
			}
			if (locationGridTile != null)
			{
				break;
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return locationGridTile;
	}

	private bool AreThereTilesToPurify()
	{
		return GetFirstTileToPurify() != null;
	}

	private void TriggerStartPurifyGround()
	{
		if (!_owner.HasJob(JOB_TYPE.PURIFY_GROUND))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PURIFY_GROUND, INTERACTION_TYPE.START_PURIFYING_GROUND, null, _owner);
			goapPlanJob.SetCanTakeThisJobChecker("CanTakePurifyJob");
			_owner.AddToAvailableJobs(goapPlanJob);
		}
	}

	public void TriggerJoinGatheringJob(Gathering gathering)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JOIN_GATHERING, INTERACTION_TYPE.JOIN_GATHERING, gathering.host, _owner);
		goapPlanJob.SetCanTakeThisJobChecker("CanTakeJoinGathering");
		_owner.AddToAvailableJobs(goapPlanJob);
	}

	private void StartCraftWaterWellCheck()
	{
		CheckIfShouldCraftWaterWell();
		Messenger.AddListener(Signals.HOUR_STARTED, CheckIfShouldCraftWaterWell);
	}

	private void CheckIfShouldStopWaterWellCheck()
	{
		TileObject firstTileObjectOfType = _owner.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER).GetFirstTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.WATER_WELL);
		if (firstTileObjectOfType != null && firstTileObjectOfType.mapObjectState == MAP_OBJECT_STATE.BUILT)
		{
			StopCraftWaterWellCheck();
		}
	}

	private void StopCraftWaterWellCheck()
	{
		Messenger.RemoveListener(Signals.HOUR_STARTED, CheckIfShouldCraftWaterWell);
	}

	private void CheckIfShouldCraftWaterWell()
	{
		LocationStructure randomStructureOfType = _owner.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
		if (randomStructureOfType == null || randomStructureOfType.hasBeenDestroyed)
		{
			StopCraftWaterWellCheck();
			return;
		}
		TileObject firstTileObjectOfType = randomStructureOfType.GetFirstTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.WATER_WELL);
		if (firstTileObjectOfType.mapObjectState != MAP_OBJECT_STATE.BUILT && !_owner.HasJob(JOB_TYPE.CRAFT_OBJECT, firstTileObjectOfType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, firstTileObjectOfType, _owner);
			JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(_owner, goapPlanJob, INTERACTION_TYPE.TAKE_RESOURCE);
			TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.WATER_WELL).TryGetPossibleRecipe(_owner, out var possibleRecipe);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { possibleRecipe });
			goapPlanJob.AddOtherData(INTERACTION_TYPE.CRAFT_TILE_OBJECT, new object[1] { possibleRecipe });
			_owner.AddToAvailableJobs(goapPlanJob);
		}
	}

	public bool CreateStealCorpseJob(LocationStructure dropLocation)
	{
		if (!_owner.HasJob(JOB_TYPE.STEAL_CORPSE))
		{
			LocationGridTile locationGridTile = dropLocation.GetRandomUnoccupiedTile();
			if (HasStealCorpseTarget())
			{
				IPointOfInterest stealCorpseTarget = GetStealCorpseTarget();
				if (stealCorpseTarget != null)
				{
					if (stealCorpseTarget is Character)
					{
						locationGridTile = dropLocation.GetRandomPassableTile();
					}
					if (locationGridTile != null)
					{
						GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STEAL_CORPSE, INTERACTION_TYPE.DROP_CORPSE, stealCorpseTarget, _owner);
						goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_CORPSE, new object[2] { dropLocation, locationGridTile });
						goapPlanJob.SetCanTakeThisJobChecker("CanStealCorpse");
						_owner.AddToAvailableJobs(goapPlanJob);
					}
				}
			}
		}
		return false;
	}

	private bool HasStealCorpseTarget()
	{
		Region region = _owner.region;
		for (int i = 0; i < region.charactersAtLocation.Count; i++)
		{
			Character character = region.charactersAtLocation[i];
			IPointOfInterest pointOfInterest = character;
			if (character.grave != null)
			{
				pointOfInterest = character.grave;
			}
			if (character.isDead && pointOfInterest.gridTileLocation != null && (bool)pointOfInterest.mapObjectVisual)
			{
				return true;
			}
		}
		return false;
	}

	private IPointOfInterest GetStealCorpseTarget()
	{
		Region region = _owner.region;
		Faction owner = _owner.owner;
		WeightedDictionary<IPointOfInterest> weightedDictionary = null;
		for (int i = 0; i < region.charactersAtLocation.Count; i++)
		{
			Character character = region.charactersAtLocation[i];
			IPointOfInterest pointOfInterest = character;
			if (character.grave != null)
			{
				pointOfInterest = character.grave;
			}
			if (!character.isDead || pointOfInterest.gridTileLocation == null || !pointOfInterest.mapObjectVisual)
			{
				continue;
			}
			if (pointOfInterest.gridTileLocation.IsPartOfSettlement(_owner))
			{
				if (pointOfInterest.gridTileLocation.structure.structureType != STRUCTURE_TYPE.CULT_TEMPLE)
				{
					if (weightedDictionary == null)
					{
						weightedDictionary = new WeightedDictionary<IPointOfInterest>();
					}
					weightedDictionary.AddElement(pointOfInterest, 50);
				}
				continue;
			}
			BaseSettlement settlement = null;
			Faction faction = null;
			if (pointOfInterest.gridTileLocation.IsPartOfSettlement(out settlement))
			{
				faction = settlement.owner;
			}
			int weight = 50;
			if (owner != null && faction != null && owner.IsHostileWith(faction))
			{
				weight = 10;
			}
			if (weightedDictionary == null)
			{
				weightedDictionary = new WeightedDictionary<IPointOfInterest>();
			}
			weightedDictionary.AddElement(pointOfInterest, weight);
		}
		if (weightedDictionary != null && weightedDictionary.Count > 0)
		{
			return weightedDictionary.PickRandomElementGivenWeights();
		}
		return null;
	}

	public bool CreateSummonBoneGolemJob(LocationStructure cultTemple)
	{
		if (!_owner.HasJob(JOB_TYPE.SUMMON_BONE_GOLEM))
		{
			CultAltar firstBuiltTileObjectOfType = cultTemple.GetFirstBuiltTileObjectOfType<CultAltar>(TILE_OBJECT_TYPE.CULT_ALTAR);
			if (firstBuiltTileObjectOfType != null)
			{
				object[] array = Get3CorpsesToSummonBoneGolem(cultTemple);
				if (array != null)
				{
					GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SUMMON_BONE_GOLEM, INTERACTION_TYPE.SUMMON_BONE_GOLEM, firstBuiltTileObjectOfType, _owner);
					goapPlanJob.AddOtherData(INTERACTION_TYPE.SUMMON_BONE_GOLEM, array);
					goapPlanJob.SetCanTakeThisJobChecker("CanSummonBoneGolem");
					goapPlanJob.SetForceCancelOnInvalid(state: true);
					_owner.AddToAvailableJobs(goapPlanJob);
				}
			}
		}
		return false;
	}

	public object[] Get3CorpsesToSummonBoneGolem(LocationStructure cultTemple)
	{
		Character character = null;
		Character character2 = null;
		Character character3 = null;
		for (int i = 0; i < cultTemple.charactersHere.Count; i++)
		{
			Character character4 = cultTemple.charactersHere[i];
			if (character4.isDead && character4.gridTileLocation != null)
			{
				if (character == null)
				{
					character = character4;
				}
				else if (character2 == null)
				{
					character2 = character4;
				}
				else if (character3 == null)
				{
					character3 = character4;
					break;
				}
			}
		}
		if (character == null || character2 == null || character3 == null)
		{
			return null;
		}
		return new object[3] { character, character2, character3 };
	}

	public void TriggerQuarantineJob(Character target)
	{
		if (!_owner.HasJob(JOB_TYPE.QUARANTINE, target))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.QUARANTINE, INTERACTION_TYPE.QUARANTINE, target, _owner);
			_owner.AddToAvailableJobs(job);
		}
	}

	public void TriggerChangeClassJob(string className, LocationStructure p_reservedStructure)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, null, _owner);
		goapPlanJob.SetCanTakeThisJobChecker("CanTakeChangeClass");
		if (p_reservedStructure != null)
		{
			goapPlanJob.AddOtherData(INTERACTION_TYPE.CHANGE_CLASS, new object[2] { className, p_reservedStructure });
		}
		else
		{
			goapPlanJob.AddOtherData(INTERACTION_TYPE.CHANGE_CLASS, new object[1] { className });
		}
		_owner.AddToAvailableJobs(goapPlanJob);
	}

	public void TriggerExtraChangeClassJob(string className, LocationStructure p_reservedStructure)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, null, _owner);
		goapPlanJob.SetCanTakeThisJobChecker("CanTakeExtraChangeClass");
		if (p_reservedStructure != null)
		{
			goapPlanJob.AddOtherData(INTERACTION_TYPE.CHANGE_CLASS, new object[2] { className, p_reservedStructure });
		}
		else
		{
			goapPlanJob.AddOtherData(INTERACTION_TYPE.CHANGE_CLASS, new object[1] { className });
		}
		_owner.AddToAvailableJobs(goapPlanJob);
	}

	private void StartCraftTownMessageBoardCheck()
	{
		CheckIfShouldCraftTownMessageBoard();
		Messenger.AddListener(Signals.HOUR_STARTED, CheckIfShouldCraftTownMessageBoard);
	}

	private void CheckIfShouldStopTownMessageBoardCheck()
	{
		TileObject firstTileObjectOfType = _owner.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER).GetFirstTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.TOWN_MESSAGE_BOARD);
		if (firstTileObjectOfType != null && firstTileObjectOfType.mapObjectState == MAP_OBJECT_STATE.BUILT)
		{
			StopCraftTownMessageBoardCheck();
		}
	}

	private void StopCraftTownMessageBoardCheck()
	{
		Messenger.RemoveListener(Signals.HOUR_STARTED, CheckIfShouldCraftTownMessageBoard);
	}

	private void CheckIfShouldCraftTownMessageBoard()
	{
		LocationStructure randomStructureOfType = _owner.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
		if (randomStructureOfType == null || randomStructureOfType.hasBeenDestroyed)
		{
			StopCraftTownMessageBoardCheck();
			return;
		}
		TileObject firstTileObjectOfType = randomStructureOfType.GetFirstTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.TOWN_MESSAGE_BOARD);
		if (firstTileObjectOfType.mapObjectState != MAP_OBJECT_STATE.BUILT && !_owner.HasJob(JOB_TYPE.CRAFT_OBJECT, firstTileObjectOfType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, firstTileObjectOfType, _owner);
			JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(_owner, goapPlanJob, INTERACTION_TYPE.TAKE_RESOURCE);
			TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.TOWN_MESSAGE_BOARD).TryGetPossibleRecipe(_owner, out var possibleRecipe);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { possibleRecipe });
			goapPlanJob.AddOtherData(INTERACTION_TYPE.CRAFT_TILE_OBJECT, new object[1] { possibleRecipe });
			_owner.AddToAvailableJobs(goapPlanJob);
		}
	}

	public void CreateTendWyvernCoopJob(WyvernCoop coop)
	{
		if (coop != null)
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TEND_WYVERN_COOP, INTERACTION_TYPE.TEND_WYVERN_COOP, coop.structureTileObject, _owner);
			goapPlanJob.SetCanTakeThisJobChecker("CanTakeTendWyvernCoop");
			_owner.AddToAvailableJobs(goapPlanJob);
		}
	}

	public void CreateSearchForDemonicAreaJob()
	{
		if (!_owner.HasJob(JOB_TYPE.SEARCH_FOR_DEMONIC_AREA))
		{
			PortalTileObject tileObjectOfType = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL).GetTileObjectOfType<PortalTileObject>();
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SEARCH_FOR_DEMONIC_AREA, INTERACTION_TYPE.SEARCH_FOR_DEMONIC_AREA, tileObjectOfType, _owner);
			goapPlanJob.SetCanTakeThisJobChecker("CanTakeSearchForDemonicArea");
			goapPlanJob.SetShouldBeRemovedFromSettlementWhenUnassigned(state: true);
			_owner.AddToAvailableJobs(goapPlanJob);
		}
	}

	private void TryCreateCraftWardLightJob()
	{
		if (_owner.owner == null || !_owner.owner.isMajorFaction)
		{
			return;
		}
		int facilityCount = _owner.GetFacilityCount();
		if (_owner.tileObjectComponent.wardLights.Count >= facilityCount || _owner.HasJob(JOB_TYPE.CREATE_WARD_LIGHT))
		{
			return;
		}
		LocationGridTile locationGridTile = null;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		list.AddRange(_owner.tileObjectComponent.wardLightLocations);
		list.Shuffle();
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile2 = list[i];
			if ((locationGridTile2.tileObjectComponent.objHere == null || !locationGridTile2.tileObjectComponent.objHere.traitContainer.HasTrait("Indestructible")) && locationGridTile2.structure.structureType == STRUCTURE_TYPE.WILDERNESS)
			{
				locationGridTile = locationGridTile2;
				break;
			}
		}
		if (locationGridTile != null)
		{
			if (locationGridTile.tileObjectComponent.objHere != null)
			{
				locationGridTile.structure.RemovePOI(locationGridTile.tileObjectComponent.objHere);
			}
			TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WARD_LIGHT);
			locationGridTile.structure.AddPOI(tileObject, locationGridTile);
			tileObject.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CREATE_WARD_LIGHT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, tileObject, _owner);
			JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(_owner, goapPlanJob, INTERACTION_TYPE.TAKE_RESOURCE);
			TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.WARD_LIGHT).TryGetPossibleRecipe(_owner, out var possibleRecipe);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { possibleRecipe });
			goapPlanJob.AddOtherData(INTERACTION_TYPE.CRAFT_TILE_OBJECT, new object[2] { possibleRecipe, _owner });
			_owner.AddToAvailableJobs(goapPlanJob);
		}
	}

	private void CheckForPlaceBlueprint()
	{
		if (_owner.owner == null || !_owner.owner.isMajorFaction || _owner.HasJob(JOB_TYPE.PLACE_BLUEPRINT))
		{
			return;
		}
		string log = GameManager.Instance.TodayLogString() + "Checking if " + _owner.name + " should create place blueprint job";
		Faction owner = _owner.owner;
		if (_owner.settlementType == null || _owner.locationType != LOCATION_TYPE.VILLAGE)
		{
			return;
		}
		List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim();
		_owner.PopulateJobsOfType(list, JOB_TYPE.BUILD_BLUEPRINT);
		int count = list.Count;
		int jobsThatWillBuildDwelling = GetJobsThatWillBuildDwelling(list);
		int jobsThatWillBuildFacility = GetJobsThatWillBuildFacility(list);
		RuinarchListPool<JobQueueItem>.Release(list);
		int numberOfStructures = _owner.GetNumberOfStructures(STRUCTURE_TYPE.DWELLING);
		int num = numberOfStructures + jobsThatWillBuildDwelling;
		if (jobsThatWillBuildDwelling <= 0 && num < _owner.settlementType.maxDwellings && _owner.GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE.DWELLING) < 2)
		{
			int chance = 3;
			if (count >= 3)
			{
				chance = 0;
			}
			else
			{
				if (numberOfStructures < _owner.settlementType.maxDwellings / 2)
				{
					chance = 10;
				}
				if (_owner.HasHomelessResident())
				{
					chance = 20;
				}
			}
			if (GameUtilities.RollChance(chance, ref log))
			{
				StructureSetting structureSetting = _owner.settlementType.GetDwellingSetting(_owner.owner);
				if (_owner.owner != null)
				{
					structureSetting = _owner.owner.factionType.ProcessStructureSetting(structureSetting, _owner);
				}
				if (LandmarkManager.Instance.CanPlaceStructureBlueprint(_owner.owner.factionType.type, _owner, structureSetting, out var targetTile, out var structurePrefabName, out var _, out var connectorTile))
				{
					TriggerPlaceBlueprint(structurePrefabName, structureSetting, targetTile, connectorTile);
					return;
				}
			}
		}
		int num2 = _owner.GetFacilityCount() + jobsThatWillBuildFacility;
		if (jobsThatWillBuildFacility <= 0 && num2 < _owner.settlementType.maxFacilities)
		{
			STRUCTURE_TYPE sTRUCTURE_TYPE = STRUCTURE_TYPE.NONE;
			int chance2 = ChanceData.GetChance(CHANCE_TYPE.Settlement_Ruler_Default_Facility_Chance);
			if (!HasActiveWorkStructureOfType(_owner, STRUCTURE_TYPE.BUTCHERS_SHOP) && !HasActiveWorkStructureOfType(_owner, STRUCTURE_TYPE.FISHERY) && !HasActiveWorkStructureOfType(_owner, STRUCTURE_TYPE.FARM))
			{
				chance2 = 85;
				sTRUCTURE_TYPE = (ShouldBuildButchersShop(_owner) ? STRUCTURE_TYPE.BUTCHERS_SHOP : ((!ShouldBuildFishery(_owner)) ? STRUCTURE_TYPE.FARM : STRUCTURE_TYPE.FISHERY));
			}
			else if (_owner.owner != null && _owner.owner.factionType.type == FACTION_TYPE.Elven_Kingdom && !_owner.HasStructure(STRUCTURE_TYPE.LUMBERYARD) && !_owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.LUMBERYARD))
			{
				chance2 = 85;
				sTRUCTURE_TYPE = STRUCTURE_TYPE.LUMBERYARD;
			}
			else if (_owner.owner != null && _owner.owner.factionType.type == FACTION_TYPE.Human_Empire && !_owner.HasStructure(STRUCTURE_TYPE.MINE) && !_owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.MINE) && _owner.occupiedVillageSpot.HasUnusedMiningSpots())
			{
				chance2 = 85;
				sTRUCTURE_TYPE = STRUCTURE_TYPE.MINE;
			}
			else if (!_owner.HasStructure(STRUCTURE_TYPE.MINE) && !_owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.MINE) && !_owner.HasStructure(STRUCTURE_TYPE.LUMBERYARD) && !_owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.LUMBERYARD))
			{
				chance2 = 85;
				sTRUCTURE_TYPE = ((!_owner.occupiedVillageSpot.HasUnusedMiningSpots()) ? STRUCTURE_TYPE.LUMBERYARD : STRUCTURE_TYPE.MINE);
			}
			else if (!_owner.HasStructure(STRUCTURE_TYPE.WORKSHOP) && !_owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.WORKSHOP))
			{
				chance2 = 50;
				sTRUCTURE_TYPE = STRUCTURE_TYPE.WORKSHOP;
			}
			if (GameUtilities.RollChance(chance2, ref log))
			{
				if (sTRUCTURE_TYPE == STRUCTURE_TYPE.NONE)
				{
					int numberOfResidentsThatIsAliveVillager = _owner.GetNumberOfResidentsThatIsAliveVillager();
					int potentialFoodSupplyCapacity = _owner.resourcesComponent.GetPotentialFoodSupplyCapacity();
					int potentialResourceSupplyCapacity = _owner.resourcesComponent.GetPotentialResourceSupplyCapacity();
					if (numberOfResidentsThatIsAliveVillager > potentialFoodSupplyCapacity)
					{
						sTRUCTURE_TYPE = (ShouldBuildButchersShop(_owner) ? STRUCTURE_TYPE.BUTCHERS_SHOP : ((!ShouldBuildFishery(_owner)) ? STRUCTURE_TYPE.FARM : STRUCTURE_TYPE.FISHERY));
					}
					else if (numberOfResidentsThatIsAliveVillager > potentialResourceSupplyCapacity)
					{
						bool flag = _owner.occupiedVillageSpot.HasUnusedMiningSpotsThatSettlementHasNotYetConnectedTo(_owner);
						if (_owner.owner != null && _owner.owner.factionType.type == FACTION_TYPE.Elven_Kingdom)
						{
							if (!_owner.HasStructure(STRUCTURE_TYPE.LUMBERYARD) && !_owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.LUMBERYARD))
							{
								sTRUCTURE_TYPE = STRUCTURE_TYPE.LUMBERYARD;
							}
						}
						else if (_owner.owner != null && _owner.owner.factionType.type == FACTION_TYPE.Human_Empire)
						{
							if (flag && !_owner.HasStructure(STRUCTURE_TYPE.MINE) && !_owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.MINE))
							{
								sTRUCTURE_TYPE = STRUCTURE_TYPE.MINE;
							}
						}
						else if (flag && !_owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.MINE))
						{
							sTRUCTURE_TYPE = STRUCTURE_TYPE.MINE;
						}
						else if (!_owner.HasStructure(STRUCTURE_TYPE.LUMBERYARD) && !_owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.LUMBERYARD))
						{
							sTRUCTURE_TYPE = STRUCTURE_TYPE.LUMBERYARD;
						}
					}
					if (sTRUCTURE_TYPE == STRUCTURE_TYPE.NONE)
					{
						if (ShouldBuildSkinnersLodge(_owner))
						{
							sTRUCTURE_TYPE = STRUCTURE_TYPE.HUNTER_LODGE;
						}
						else
						{
							int num3 = Mathf.CeilToInt((float)numberOfResidentsThatIsAliveVillager / 16f);
							if (_owner.GetNumberOfStructures(STRUCTURE_TYPE.WORKSHOP) + _owner.GetNumberOfBlueprintOnTileForStructure(STRUCTURE_TYPE.WORKSHOP) < num3)
							{
								sTRUCTURE_TYPE = STRUCTURE_TYPE.WORKSHOP;
							}
						}
					}
					if (sTRUCTURE_TYPE == STRUCTURE_TYPE.NONE && owner != null && owner.factionType.type == FACTION_TYPE.Demon_Cult && !_owner.HasStructure(STRUCTURE_TYPE.CULT_TEMPLE) && !_owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.CULT_TEMPLE))
					{
						sTRUCTURE_TYPE = STRUCTURE_TYPE.CULT_TEMPLE;
					}
					if (sTRUCTURE_TYPE == STRUCTURE_TYPE.NONE)
					{
						List<STRUCTURE_TYPE> list2 = RuinarchListPool<STRUCTURE_TYPE>.Claim(_miscStructureTypes.Length);
						list2.AddRange(_miscStructureTypes);
						while (sTRUCTURE_TYPE == STRUCTURE_TYPE.NONE && list2.Count > 0)
						{
							STRUCTURE_TYPE randomElement = CollectionUtilities.GetRandomElement(list2);
							if (!_owner.HasStructure(randomElement) && !_owner.HasBlueprintOnTileForStructure(randomElement))
							{
								sTRUCTURE_TYPE = randomElement;
							}
							else
							{
								list2.Remove(randomElement);
							}
						}
						RuinarchListPool<STRUCTURE_TYPE>.Release(list2);
					}
				}
				if (sTRUCTURE_TYPE != STRUCTURE_TYPE.NONE && owner != null)
				{
					if (count < 3 || sTRUCTURE_TYPE == STRUCTURE_TYPE.LUMBERYARD || sTRUCTURE_TYPE == STRUCTURE_TYPE.MINE)
					{
						TryCreatePlaceBlueprintJob(owner.factionType.type, sTRUCTURE_TYPE, _owner, ref log);
					}
					return;
				}
			}
		}
		if (owner != null && owner.factionType.HasIdeology(FACTION_IDEOLOGY.Wyvern_Tamers) && !_owner.HasStructure(STRUCTURE_TYPE.WYVERN_COOP) && !_owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.WYVERN_COOP) && ChanceData.RollChance(CHANCE_TYPE.Build_Wyvern_Coop))
		{
			if (count < 3)
			{
				TryCreatePlaceBlueprintJob(owner.factionType.type, STRUCTURE_TYPE.WYVERN_COOP, _owner, ref log);
			}
			return;
		}
		if (owner != null && owner.factionType.HasIdeology(FACTION_IDEOLOGY.Breeders))
		{
			if (count >= 3)
			{
				return;
			}
			int num4 = _owner.GetNumberOfStructures(STRUCTURE_TYPE.BEAST_PEN) + _owner.GetNumberOfBlueprintOnTileForStructure(STRUCTURE_TYPE.BEAST_PEN);
			int chance3 = 0;
			switch (num4)
			{
			case 0:
				chance3 = 12;
				break;
			case 1:
				chance3 = 4;
				break;
			case 2:
				chance3 = 1;
				break;
			}
			if (GameUtilities.RollChance(chance3, ref log))
			{
				TryCreatePlaceBlueprintJob(owner.factionType.type, STRUCTURE_TYPE.BEAST_PEN, _owner, ref log);
				return;
			}
		}
		if (owner != null && owner.factionType.HasIdeology(FACTION_IDEOLOGY.Lightning_Tower_Defense))
		{
			if (count >= 3)
			{
				return;
			}
			int num5 = _owner.GetNumberOfStructures(STRUCTURE_TYPE.LIGHTNING_TOWER) + _owner.GetNumberOfBlueprintOnTileForStructure(STRUCTURE_TYPE.LIGHTNING_TOWER);
			int chance4 = 0;
			switch (num5)
			{
			case 0:
				chance4 = 30;
				break;
			case 1:
				chance4 = 10;
				break;
			case 2:
				chance4 = 3;
				break;
			}
			if (GameUtilities.RollChance(chance4, ref log))
			{
				TryCreatePlaceBlueprintJob(owner.factionType.type, STRUCTURE_TYPE.LIGHTNING_TOWER, _owner, ref log);
				return;
			}
		}
		if (owner != null && owner.factionType.HasIdeology(FACTION_IDEOLOGY.Tower_Defense) && count < 3)
		{
			int num6 = _owner.GetNumberOfStructures(STRUCTURE_TYPE.ARROW_TOWER) + _owner.GetNumberOfBlueprintOnTileForStructure(STRUCTURE_TYPE.ARROW_TOWER);
			int chance5 = 0;
			switch (num6)
			{
			case 0:
				chance5 = 30;
				break;
			case 1:
				chance5 = 10;
				break;
			case 2:
				chance5 = 3;
				break;
			}
			if (GameUtilities.RollChance(chance5, ref log))
			{
				TryCreatePlaceBlueprintJob(owner.factionType.type, STRUCTURE_TYPE.ARROW_TOWER, _owner, ref log);
			}
		}
	}

	private int GetJobsThatWillBuildFacility(List<JobQueueItem> jobs)
	{
		int num = 0;
		for (int i = 0; i < jobs.Count; i++)
		{
			if (jobs[i] is GoapPlanJob { poiTarget: GenericTileObject poiTarget } && poiTarget.blueprintOnTile != null && poiTarget.blueprintOnTile.structureType.IsFacilityStructure())
			{
				num++;
			}
		}
		return num;
	}

	private int GetJobsThatWillBuildDwelling(List<JobQueueItem> jobs)
	{
		int num = 0;
		for (int i = 0; i < jobs.Count; i++)
		{
			if (jobs[i] is GoapPlanJob { poiTarget: GenericTileObject poiTarget } && poiTarget.blueprintOnTile != null && poiTarget.blueprintOnTile.structureType == STRUCTURE_TYPE.DWELLING)
			{
				num++;
			}
		}
		return num;
	}

	private bool ShouldBuildFishery(NPCSettlement p_settlement)
	{
		if (p_settlement.owner != null && p_settlement.owner.factionType.IsActionConsideredACrime(CRIME_TYPE.Animal_Killing))
		{
			return false;
		}
		if (!p_settlement.occupiedVillageSpot.HasUnusedFishingSpot())
		{
			return false;
		}
		if (!p_settlement.HasResidentThatCanWorkAt(STRUCTURE_TYPE.FISHERY))
		{
			return false;
		}
		return true;
	}

	private bool ShouldBuildButchersShop(NPCSettlement p_settlement)
	{
		if (p_settlement.owner != null && p_settlement.owner.factionType.IsActionConsideredACrime(CRIME_TYPE.Animal_Killing))
		{
			return false;
		}
		if (!p_settlement.occupiedVillageSpot.HasAccessToButcherAnimals())
		{
			return false;
		}
		if (HasActiveWorkStructureOfType(p_settlement, STRUCTURE_TYPE.BUTCHERS_SHOP))
		{
			return false;
		}
		if (!p_settlement.HasResidentThatCanWorkAt(STRUCTURE_TYPE.BUTCHERS_SHOP))
		{
			return false;
		}
		return true;
	}

	private bool ShouldBuildSkinnersLodge(NPCSettlement p_settlement)
	{
		if (HasActiveWorkStructureOfType(p_settlement, STRUCTURE_TYPE.HUNTER_LODGE))
		{
			return false;
		}
		if (!p_settlement.occupiedVillageSpot.HasAccessToSkinnerAnimals())
		{
			return false;
		}
		if (!p_settlement.HasResidentThatCanWorkAt(STRUCTURE_TYPE.HUNTER_LODGE))
		{
			return false;
		}
		return true;
	}

	private bool HasActiveWorkStructureOfType(NPCSettlement p_settlement, STRUCTURE_TYPE p_structureType)
	{
		if (p_settlement.HasStructure(p_structureType))
		{
			if (p_structureType == STRUCTURE_TYPE.HUNTER_LODGE || p_structureType == STRUCTURE_TYPE.BUTCHERS_SHOP || p_structureType == STRUCTURE_TYPE.FISHERY)
			{
				List<LocationStructure> structuresOfType = p_settlement.GetStructuresOfType(p_structureType);
				if (structuresOfType != null)
				{
					for (int i = 0; i < structuresOfType.Count; i++)
					{
						ManMadeStructure manMadeStructure = structuresOfType[i] as ManMadeStructure;
						if (manMadeStructure.HasAssignedWorker() || manMadeStructure.HasSettlementOrLocalResidentThatCanWorkHere())
						{
							return true;
						}
					}
				}
				return false;
			}
			return true;
		}
		if (p_settlement.HasBlueprintOnTileForStructure(p_structureType))
		{
			if (p_structureType == STRUCTURE_TYPE.HUNTER_LODGE || p_structureType == STRUCTURE_TYPE.BUTCHERS_SHOP || p_structureType == STRUCTURE_TYPE.FISHERY)
			{
				return p_settlement.HasResidentThatCanWorkAt(p_structureType);
			}
			return true;
		}
		return false;
	}

	private void TriggerPlaceBlueprint(string structurePrefabName, StructureSetting structureSetting, LocationGridTile centerTile, LocationGridTile connectorTile)
	{
		if (!_owner.HasJob(JOB_TYPE.PLACE_BLUEPRINT))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_BLUEPRINT, INTERACTION_TYPE.PLACE_BLUEPRINT, centerTile.tileObjectComponent.genericTileObject, _owner);
			OtherData[] data = new OtherData[3]
			{
				new StringOtherData(structurePrefabName),
				new LocationGridTileOtherData(connectorTile),
				new StructureSettingOtherData(structureSetting)
			};
			goapPlanJob.AddOtherData(INTERACTION_TYPE.PLACE_BLUEPRINT, data);
			goapPlanJob.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCanTakeThisJobChecker("CanTakePlaceBlueprintJob");
			_owner.AddToAvailableJobs(goapPlanJob);
		}
	}

	private void TryCreatePlaceBlueprintJob(FACTION_TYPE p_factionType, STRUCTURE_TYPE p_structureType, NPCSettlement p_settlement, ref string log)
	{
		StructureSetting structureSetting = p_settlement.owner.factionType.CreateStructureSettingForStructure(p_structureType, p_settlement);
		if (structureSetting.hasValue && LandmarkManager.Instance.CanPlaceStructureBlueprint(p_factionType, p_settlement, structureSetting, out var targetTile, out var structurePrefabName, out var _, out var connectorTile))
		{
			TriggerPlaceBlueprint(structurePrefabName, structureSetting, targetTile, connectorTile);
		}
	}

	public void TriggerCreateGolemJob()
	{
		if (!_owner.HasJob(JOB_TYPE.CREATE_GOLEM))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CREATE_GOLEM, INTERACTION_TYPE.CREATE_GOLEM, null, _owner);
			goapPlanJob.SetConnectedFactionIdeology(FACTION_IDEOLOGY.Golem_Makers);
			goapPlanJob.SetDoNotRecalculate(state: true);
			goapPlanJob.SetCanTakeThisJobChecker("CanTakeFactionIdeologyJob");
			_owner.AddToAvailableJobs(goapPlanJob);
		}
	}

	public void TryCreateBloodSacrificeJob(Character p_targetCharacter)
	{
		if (_owner.HasJob(JOB_TYPE.BLOOD_SACRIFICE))
		{
			return;
		}
		BaseSettlement owner = _owner;
		if (owner == null)
		{
			return;
		}
		LocationGridTile locationGridTile = owner.GetRandomTileObjectOfType(TILE_OBJECT_TYPE.MAGIC_CIRCLE)?.gridTileLocation;
		bool flag = locationGridTile != null;
		if (locationGridTile == null)
		{
			List<Area> list = RuinarchListPool<Area>.Claim();
			for (int i = 0; i < owner.areas.Count; i++)
			{
				Area item = owner.areas[i];
				list.Add(item);
			}
			List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
			while (locationGridTile == null && list.Count > 0)
			{
				int index = GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1);
				Area area = list[index];
				list2.Clear();
				for (int j = 0; j < area.gridTileComponent.passableTiles.Count; j++)
				{
					LocationGridTile locationGridTile2 = area.gridTileComponent.passableTiles[j];
					if (!locationGridTile2.isOccupied && locationGridTile2.tileObjectComponent.objHere == null && locationGridTile2.tileObjectComponent.hiddenObjHere == null && locationGridTile2.structure.structureType == STRUCTURE_TYPE.WILDERNESS)
					{
						list2.Add(locationGridTile2);
					}
				}
				if (list2.Count > 0)
				{
					locationGridTile = CollectionUtilities.GetRandomElement(list2);
				}
			}
			RuinarchListPool<LocationGridTile>.Release(list2);
			RuinarchListPool<Area>.Release(list);
		}
		if (locationGridTile != null)
		{
			GenericTileObject genericTileObject = locationGridTile.tileObjectComponent.genericTileObject;
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BLOOD_SACRIFICE, INTERACTION_TYPE.BLOOD_SACRIFICE, p_targetCharacter, _owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.BLOOD_SACRIFICE, new OtherData[1]
			{
				new TileObjectOtherData(genericTileObject)
			});
			if (!flag)
			{
				GoapPlanJob goapPlanJob2 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BLOOD_SACRIFICE, INTERACTION_TYPE.DRAW_MAGIC_CIRCLE, genericTileObject, _owner);
				goapPlanJob2.SetConnectedFactionIdeology(FACTION_IDEOLOGY.Blood_Sacrifices);
				goapPlanJob2.SetCanTakeThisJobChecker("CanTakeFactionIdeologyJob");
				_owner.AddToAvailableJobs(goapPlanJob2);
			}
			else
			{
				goapPlanJob.SetDoNotRecalculate(state: true);
			}
			goapPlanJob.SetConnectedFactionIdeology(FACTION_IDEOLOGY.Blood_Sacrifices);
			goapPlanJob.SetCanTakeThisJobChecker("CanTakeFactionIdeologyJob");
			_owner.AddToAvailableJobs(goapPlanJob);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		poisonCleansers.Contains(p_character);
		tileDryers.Contains(p_character);
		dousers.Contains(p_character);
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		poisonCleansers.Remove(p_character);
		tileDryers.Remove(p_character);
		dousers.Remove(p_character);
	}
}
