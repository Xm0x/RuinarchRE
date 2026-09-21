using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;
using UnityEngine;

public class GenericTileObject : TileObject
{
	private LocationGridTile _owner;

	private string _expiryKey;

	private ConstructionProgress _constructionProgress;

	private bool _hasStartedBuildingBlueprintOnTile;

	private BuildStructureParticleEffect _buildStructureParticles;

	private bool hasBeenInitialized { get; set; }

	public LocationStructureObject blueprintOnTile { get; private set; }

	public string blueprintTemplateName { get; private set; }

	public GameDate blueprintExpiryDate { get; private set; }

	public bool isCurrentlyBuilding { get; private set; }

	public GameDate selfBuildingStructureDueDate { get; private set; }

	public StructureConnector structureConnector { get; private set; }

	public BaseSettlement selfBuildingStructureSettlement { get; private set; }

	public override Type serializedData => typeof(SaveDataGenericTileObject);

	public override LocationGridTile gridTileLocation => _owner;

	public override Vector3 worldPosition => _owner.centeredWorldLocation;

	public ConstructionProgress constructionProgress => _constructionProgress;

	public bool hasStartedBuildingBlueprintOnTile => _hasStartedBuildingBlueprintOnTile;

	public GenericTileObject(LocationGridTile locationGridTile)
	{
		SetTileOwner(locationGridTile);
	}

	public GenericTileObject(SaveDataGenericTileObject data)
		: base(data)
	{
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		Messenger.Broadcast(GridTileSignals.TILE_OBJECT_REMOVED, (TileObject)this, removedBy, removedFrom, destroyTileSlots);
		if (hasCreatedSlots && destroyTileSlots)
		{
			DestroyTileSlots();
		}
	}

	public override void OnPlacePOI()
	{
		SetPOIState(POI_STATE.ACTIVE);
	}

	protected override string GenerateInternalName()
	{
		return base.GenerateInternalName();
	}

	protected override void OnPlaceTileObjectAtTile(LocationGridTile tile)
	{
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		DisableGameObject();
		OnRemoveTileObject(null, base.previousTile);
		SetPOIState(POI_STATE.INACTIVE);
	}

	public override void RemoveTileObject(Character removedBy)
	{
		LocationGridTile removedFrom = gridTileLocation;
		DisableGameObject();
		OnRemoveTileObject(removedBy, removedFrom);
		SetPOIState(POI_STATE.INACTIVE);
	}

	public override bool IsValidCombatTargetFor(IPointOfInterest source)
	{
		return true;
	}

	public override void OnTileObjectGainedTrait(Trait trait)
	{
		if (trait.name != "Flammable")
		{
			_owner.SetIsDefault(state: false);
		}
		if (trait is Status { isTangible: not false } status)
		{
			if (status is Wet && gridTileLocation.structure is Ocean)
			{
				return;
			}
			GetOrCreateMapVisual();
		}
		base.OnTileObjectGainedTrait(trait);
	}

	public override void OnTileObjectLostTrait(Trait trait)
	{
		base.OnTileObjectLostTrait(trait);
		TryDestroyMapVisual();
	}

	public override string ToString()
	{
		return $"Generic Obj at tile {gridTileLocation}";
	}

	public override void AddExistingJobTargetingThis(JobQueueItem job)
	{
		base.AddExistingJobTargetingThis(job);
		_owner.SetIsDefault(state: false);
	}

	public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false, bool isTrueDamage = false)
	{
		if (!isTrueDamage)
		{
			CombatManager.Instance.ModifyDamage(ref amount, elementalDamageType, piercingPower, this);
		}
		base.currentHP += amount;
		base.currentHP = Mathf.Clamp(base.currentHP, 0, base.maxHP);
		_owner.SetIsDefault(state: false);
		if (amount < 0)
		{
			Character characterResponsible = null;
			if (source is Character character)
			{
				characterResponsible = character;
			}
			CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, characterResponsible, elementalTraitProcessor, createHitEffect: false, isPlayerSource, piercingPower);
		}
		if (amount < 0)
		{
			base.structureLocation.OnTileDamaged(gridTileLocation, amount, isPlayerSource);
		}
		else if (amount > 0)
		{
			base.structureLocation.OnTileRepaired(gridTileLocation, amount);
		}
		if (base.currentHP <= 0)
		{
			if (gridTileLocation.structure.structureType.IsPlayerStructure())
			{
				gridTileLocation.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.corruptedTile);
			}
			else
			{
				gridTileLocation.DetermineNextGroundTypeAfterDestruction();
			}
			Messenger.Broadcast(TileObjectSignals.TILE_DESTROYED, this);
			base.currentHP = base.maxHP;
		}
	}

	public override bool CanBeDamaged()
	{
		return !base.structureLocation.structureType.IsOpenSpace();
	}

	public override bool CanBeSelected()
	{
		return false;
	}

	public override void OnReferencedInALog()
	{
		base.OnReferencedInALog();
		_owner.SetIsDefault(state: false);
	}

	public override void OnDoActionToObject(ActualGoapNode action)
	{
		base.OnDoActionToObject(action);
		if (action.goapType == INTERACTION_TYPE.BUILD_BLUEPRINT)
		{
			isCurrentlyBuilding = true;
		}
	}

	public override void OnCancelActionTowardsObject(ActualGoapNode action)
	{
		base.OnCancelActionTowardsObject(action);
		if (action.goapType == INTERACTION_TYPE.BUILD_BLUEPRINT)
		{
			isCurrentlyBuilding = false;
		}
	}

	public override bool IsUnpassable()
	{
		if (_owner != null)
		{
			return !_owner.IsPassable();
		}
		return false;
	}

	public override void OnAddedAsUnprocessedPOI(Character p_characterThatAddedPOI)
	{
		base.OnAddedAsUnprocessedPOI(p_characterThatAddedPOI);
		if (base.traitContainer.HasTrait("Snare Trapped", "Freezing Trapped", "Landmined") && p_characterThatAddedPOI.limiterComponent.canWitness)
		{
			SnareTrapped traitOrStatus = base.traitContainer.GetTraitOrStatus<SnareTrapped>("Snare Trapped");
			FreezingTrapped traitOrStatus2 = base.traitContainer.GetTraitOrStatus<FreezingTrapped>("Freezing Trapped");
			Landmined traitOrStatus3 = base.traitContainer.GetTraitOrStatus<Landmined>("Landmined");
			GameDate gameDate = GameManager.Instance.Today();
			if (traitOrStatus != null && traitOrStatus.dateAdded == gameDate)
			{
				traitOrStatus.AddAwareCharacter(p_characterThatAddedPOI);
			}
			if (traitOrStatus2 != null && traitOrStatus2.dateAdded == gameDate)
			{
				traitOrStatus2.AddAwareCharacter(p_characterThatAddedPOI);
			}
			if (traitOrStatus3 != null && traitOrStatus3.dateAdded == gameDate)
			{
				traitOrStatus3.AddAwareCharacter(p_characterThatAddedPOI);
			}
		}
	}

	public BaseMapObjectVisual GetOrCreateMapVisual()
	{
		if ((object)mapVisual == null)
		{
			InitializeMapObject(this);
			PlaceMapObjectAt(gridTileLocation);
			OnPlaceTileObjectAtTile(gridTileLocation);
		}
		return mapVisual;
	}

	public bool TryDestroyMapVisual()
	{
		if (!base.traitContainer.HasTangibleStatus() && structureConnector == null)
		{
			if ((object)mapVisual != null)
			{
				DestroyMapVisualGameObject();
			}
			return true;
		}
		return false;
	}

	public void SetTileOwner(LocationGridTile owner)
	{
		_owner = owner;
	}

	public void ManualInitialize(LocationGridTile tile)
	{
		if (!hasBeenInitialized)
		{
			hasBeenInitialized = true;
			Initialize(TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT, shouldAddCommonAdvertisements: false);
			SetGridTileLocation(tile);
			AddAdvertisedAction(INTERACTION_TYPE.PLACE_FREEZING_TRAP);
			AddAdvertisedAction(INTERACTION_TYPE.PLACE_SNARE_TRAP);
			AddAdvertisedAction(INTERACTION_TYPE.ABOMINATION_GERM_MUSHROOM);
			AddAdvertisedAction(INTERACTION_TYPE.GO_TO_TILE);
			AddAdvertisedAction(INTERACTION_TYPE.GO_TO_SPECIFIC_TILE);
			AddAdvertisedAction(INTERACTION_TYPE.FLEE_CRIME);
			AddAdvertisedAction(INTERACTION_TYPE.PLACE_BLUEPRINT);
			AddAdvertisedAction(INTERACTION_TYPE.BUILD_BLUEPRINT);
			AddAdvertisedAction(INTERACTION_TYPE.BUILD_VAMPIRE_CASTLE);
			AddAdvertisedAction(INTERACTION_TYPE.BUILD_NEW_VILLAGE);
			AddAdvertisedAction(INTERACTION_TYPE.TILL_TILE);
			AddAdvertisedAction(INTERACTION_TYPE.DRAW_MAGIC_CIRCLE);
			AddAdvertisedAction(INTERACTION_TYPE.DARK_RITUAL);
			AddAdvertisedAction(INTERACTION_TYPE.RECONCILIATION_RITUAL);
			AddAdvertisedAction(INTERACTION_TYPE.GUARDIAN_RITUAL);
			AddAdvertisedAction(INTERACTION_TYPE.GOD_DAY_RITUAL);
			base.constructionComponent.SetNeededConstructionTicks(GoapActionStateDB.BuildBlueprintDuration);
		}
	}

	public void ManualInitializeLoad(LocationGridTile tile, SaveDataTileObject saveDataTileObject)
	{
		if (!hasBeenInitialized)
		{
			hasBeenInitialized = true;
			Initialize(saveDataTileObject);
		}
	}

	public bool PlaceExpiringBlueprintOnTile(string prefabName)
	{
		if (PlaceBlueprintOnTile(prefabName, out var _))
		{
			ScheduleBlueprintExpiry();
			return true;
		}
		return false;
	}

	private bool PlaceBlueprintOnTile(string p_prefabName, out LocationStructureObject o_placedBlueprint)
	{
		GameObject gameObject = ObjectPoolManager.Instance.InstantiateObjectFromPool(p_prefabName, Vector3.zero, Quaternion.identity, gridTileLocation.parentMap.structureParent);
		LocationStructureObject component = gameObject.GetComponent<LocationStructureObject>();
		if (component.HasEnoughSpaceIfPlacedOn(gridTileLocation))
		{
			gameObject.transform.position = gridTileLocation.centeredWorldLocation;
			component.RefreshAllTilemaps();
			component.ResetWallsBeforePlacement();
			List<LocationGridTile> tilesOccupiedByStructure = component.GetTilesOccupiedByStructure(gridTileLocation.parentMap);
			for (int i = 0; i < tilesOccupiedByStructure.Count; i++)
			{
				tilesOccupiedByStructure[i].SetHasBlueprint(hasBlueprint: true, _owner.localPlace.x, _owner.localPlace.y);
			}
			LocationStructureObject.Structure_Visual_Mode mode = LocationStructureObject.Structure_Visual_Mode.Blueprint;
			if (component.structureType.IsPlayerStructure())
			{
				mode = LocationStructureObject.Structure_Visual_Mode.Demonic_Structure_Blueprint;
			}
			component.SetVisualMode(mode, gridTileLocation.parentMap);
			component.SetTilesInStructure(tilesOccupiedByStructure.ToArray());
			blueprintOnTile = component;
			blueprintTemplateName = component.name;
			gridTileLocation.SetIsDefault(state: false);
			o_placedBlueprint = component;
			return true;
		}
		o_placedBlueprint = null;
		ObjectPoolManager.Instance.DestroyObject(gameObject);
		return false;
	}

	private void ScheduleBlueprintExpiry()
	{
		blueprintExpiryDate = GameManager.Instance.Today();
		blueprintExpiryDate = blueprintExpiryDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(24));
		_expiryKey = SchedulingManager.Instance.AddEntry(blueprintExpiryDate, ExpireBlueprint, this);
	}

	private void CancelBlueprintExpiry()
	{
		if (!string.IsNullOrEmpty(_expiryKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
			_expiryKey = string.Empty;
		}
	}

	private void ExpireBlueprint()
	{
		if (blueprintOnTile == null)
		{
			return;
		}
		if (isCurrentlyBuilding)
		{
			blueprintExpiryDate = GameManager.Instance.Today();
			blueprintExpiryDate = blueprintExpiryDate.AddTicks(20);
			_expiryKey = SchedulingManager.Instance.AddEntry(blueprintExpiryDate, ExpireBlueprint, this);
			return;
		}
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, (IPointOfInterest)this, "", JOB_TYPE.BUILD_BLUEPRINT);
		ObjectPoolManager.Instance.DestroyObject(blueprintOnTile);
		blueprintOnTile = null;
		blueprintTemplateName = string.Empty;
		_expiryKey = string.Empty;
		_hasStartedBuildingBlueprintOnTile = false;
		if (_constructionProgress != null)
		{
			ObjectPoolManager.Instance.DestroyObject(_constructionProgress);
			_constructionProgress = null;
		}
		base.resourceStorageComponent.SpawnResourcesInStorage(gridTileLocation);
	}

	public void ForceExpireBlueprint()
	{
		if (blueprintOnTile == null)
		{
			return;
		}
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, (IPointOfInterest)this, "", JOB_TYPE.BUILD_BLUEPRINT);
		if (blueprintOnTile.tiles != null)
		{
			for (int i = 0; i < blueprintOnTile.tiles.Length; i++)
			{
				blueprintOnTile.tiles[i].SetHasBlueprint(hasBlueprint: false, -1, -1);
			}
		}
		ObjectPoolManager.Instance.DestroyObject(blueprintOnTile);
		blueprintOnTile = null;
		blueprintTemplateName = string.Empty;
		_expiryKey = string.Empty;
		_hasStartedBuildingBlueprintOnTile = false;
		if (_constructionProgress != null)
		{
			ObjectPoolManager.Instance.DestroyObject(_constructionProgress);
			_constructionProgress = null;
		}
		base.resourceStorageComponent.SpawnResourcesInStorage(gridTileLocation);
	}

	public void BuildBlueprintOnTile(BaseSettlement p_settlement, LocationGridTile p_usedConnector)
	{
		BuildBlueprint(blueprintOnTile, p_settlement, p_usedConnector);
		CancelBlueprintExpiry();
		FinishedBuilding();
		isCurrentlyBuilding = false;
	}

	public LocationStructure InstantPlaceStructure(string p_structurePrefabName, BaseSettlement p_settlement)
	{
		GameObject gameObject = ObjectPoolManager.Instance.InstantiateObjectFromPool(p_structurePrefabName, Vector3.zero, Quaternion.identity, gridTileLocation.parentMap.structureParent);
		LocationStructureObject component = gameObject.GetComponent<LocationStructureObject>();
		if (component.HasEnoughSpaceIfPlacedOn(gridTileLocation))
		{
			gameObject.transform.position = gridTileLocation.centeredWorldLocation;
			component.ResetWallsBeforePlacement();
			component.RefreshAllTilemaps();
			List<LocationGridTile> tilesOccupiedByStructure = component.GetTilesOccupiedByStructure(gridTileLocation.parentMap);
			component.SetTilesInStructure(tilesOccupiedByStructure.ToArray());
			gridTileLocation.SetIsDefault(state: false);
			return BuildBlueprint(component, p_settlement, null);
		}
		throw new Exception($"Could not place {p_structurePrefabName} at {gridTileLocation}");
	}

	private LocationStructure BuildBlueprint(LocationStructureObject p_blueprint, BaseSettlement npcSettlement, LocationGridTile p_usedConnector)
	{
		Area area = gridTileLocation.area;
		npcSettlement.AddAreaToSettlement(area);
		p_blueprint.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Built, gridTileLocation.parentMap);
		LocationStructure locationStructure = LandmarkManager.Instance.CreateNewStructureAt(gridTileLocation.parentMap.region, p_blueprint.structureType, npcSettlement);
		p_blueprint.ResetWallsBeforePlacement();
		p_blueprint.ClearOutUnimportantObjectsBeforePlacement();
		for (int i = 0; i < p_blueprint.tiles.Length; i++)
		{
			LocationGridTile locationGridTile = p_blueprint.tiles[i];
			locationGridTile.SetStructure(locationStructure);
			locationGridTile.SetHasBlueprint(hasBlueprint: false, -1, -1);
			if (locationStructure is DemonicStructure)
			{
				locationGridTile.corruptionComponent.CorruptTile();
			}
			else
			{
				locationGridTile.corruptionComponent.UncorruptTile();
			}
		}
		if (locationStructure is DemonicStructure demonicStructure && (demonicStructure.structureType == STRUCTURE_TYPE.THE_PORTAL || demonicStructure.structureType == STRUCTURE_TYPE.KENNEL || demonicStructure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS))
		{
			for (int j = 0; j < locationStructure.tiles.Count; j++)
			{
				LocationGridTile locationGridTile2 = locationStructure.tiles.ElementAt(j);
				for (int k = 0; k < locationGridTile2.neighbourList.Count; k++)
				{
					LocationGridTile locationGridTile3 = locationGridTile2.neighbourList[k];
					if (!(locationGridTile3.structure is Cave) && !(locationGridTile3.structure is Ocean) && locationGridTile3.structure != demonicStructure)
					{
						if (demonicStructure is ThePortal)
						{
							locationGridTile3.corruptionComponent.CorruptTileAndDestroyDestructibleObject();
						}
						else if (locationStructure is TortureChambers tortureChambers)
						{
							tortureChambers.AddBorderTile(locationGridTile3);
						}
						else if (locationStructure is Kennel kennel)
						{
							kennel.AddBorderTile(locationGridTile3);
						}
					}
				}
			}
		}
		if (locationStructure is DemonicStructure demonicStructure2)
		{
			demonicStructure2.SetStructureObject(p_blueprint);
		}
		else if (locationStructure is ManMadeStructure manMadeStructure)
		{
			manMadeStructure.SetStructureObject(p_blueprint);
		}
		else if (locationStructure is NaturalStructureWithStructureObject naturalStructureWithStructureObject)
		{
			naturalStructureWithStructureObject.SetStructureObject(p_blueprint);
		}
		locationStructure.SetOccupiedArea(area);
		p_blueprint.OnBuiltStructureObjectPlaced(gridTileLocation.parentMap, locationStructure, out var _, out var _, locationStructure.preplacedObjectsToIgnoreWhenBuilding);
		locationStructure.CreateRoomsBasedOnStructureObject(p_blueprint);
		locationStructure.OnBuiltNewStructure();
		locationStructure.OnBuiltNewStructureFromBlueprint();
		if (p_usedConnector != null && locationStructure is ManMadeStructure manMadeStructure2)
		{
			manMadeStructure2.OnUseStructureConnector(p_usedConnector);
		}
		blueprintOnTile = null;
		blueprintTemplateName = string.Empty;
		return locationStructure;
	}

	public void PlaceSelfBuildingStructure(string p_structurePrefabName, BaseSettlement p_settlement, int p_buildingTimeInTicks)
	{
		if (PlaceBlueprintOnTile(p_structurePrefabName, out var o_placedBlueprint))
		{
			AkSoundEngine.PostEvent("Play_Place_Demonic_Structure", blueprintOnTile.gameObject);
			GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Place_Demonic_Structure).GetComponent<BaseParticleEffect>().SetSize(o_placedBlueprint.size);
			selfBuildingStructureSettlement = p_settlement;
			GameDate p_completionDate = GameManager.Instance.Today();
			p_completionDate.AddTicks(p_buildingTimeInTicks);
			CreateBuildParticlesAndScheduleBuildingCompletion(o_placedBlueprint, p_settlement, p_completionDate);
			return;
		}
		throw new Exception($"Could not place self building structure {p_structurePrefabName} on {gridTileLocation}!");
	}

	private void CreateBuildParticlesAndScheduleBuildingCompletion(LocationStructureObject p_blueprint, BaseSettlement p_settlement, GameDate p_completionDate)
	{
		_buildStructureParticles = GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Build_Demonic_Structure).GetComponent<BuildStructureParticleEffect>();
		_buildStructureParticles.SetSize(p_blueprint.size);
		selfBuildingStructureDueDate = p_completionDate;
		SchedulingManager.Instance.AddEntry(p_completionDate, delegate
		{
			DoneSelfBuildingStructure(p_settlement);
		}, this);
	}

	private void DoneSelfBuildingStructure(BaseSettlement p_settlement)
	{
		if (_buildStructureParticles != null)
		{
			ObjectPoolManager.Instance.DestroyObject(_buildStructureParticles);
		}
		BuildBlueprint(blueprintOnTile, p_settlement, null);
		selfBuildingStructureSettlement = null;
	}

	public void StartBuilding()
	{
		_hasStartedBuildingBlueprintOnTile = true;
		GameObject gameObject = ObjectPoolManager.Instance.InstantiateObjectFromPool("Construction Progress", gridTileLocation.centeredWorldLocation, Quaternion.identity, gridTileLocation.parentMap.objectsParent, isWorldPosition: true);
		_constructionProgress = gameObject.GetComponent<ConstructionProgress>();
		constructionProgress.ShowConstructionVisual();
		base.constructionComponent.OnObjectSetAsUnbuilt();
	}

	public void ResumeBuilding()
	{
		CancelBlueprintExpiry();
	}

	public void CancelBuilding()
	{
		ScheduleBlueprintExpiry();
	}

	private void FinishedBuilding()
	{
		_hasStartedBuildingBlueprintOnTile = false;
		if (_constructionProgress != null)
		{
			ObjectPoolManager.Instance.DestroyObject(_constructionProgress);
			_constructionProgress = null;
		}
		base.constructionComponent.OnObjectSetAsBuilt();
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		SaveDataGenericTileObject saveDataGenericTileObject = data as SaveDataGenericTileObject;
		_hasStartedBuildingBlueprintOnTile = saveDataGenericTileObject.hasStartedBuildingBlueprintOnTile;
		if (_hasStartedBuildingBlueprintOnTile)
		{
			GameObject gameObject = ObjectPoolManager.Instance.InstantiateObjectFromPool("Construction Progress", gridTileLocation.centeredWorldLocation, Quaternion.identity, gridTileLocation.parentMap.objectsParent, isWorldPosition: true);
			_constructionProgress = gameObject.GetComponent<ConstructionProgress>();
			constructionProgress.ShowConstructionVisual();
		}
		base.LoadSecondWave(data);
		if (!string.IsNullOrEmpty(saveDataGenericTileObject.blueprintOnTileName))
		{
			LoadBlueprintOnTile(saveDataGenericTileObject.blueprintOnTileName);
			if (saveDataGenericTileObject.blueprintExpiryDate.hasValue)
			{
				blueprintExpiryDate = saveDataGenericTileObject.blueprintExpiryDate;
				_expiryKey = SchedulingManager.Instance.AddEntry(blueprintExpiryDate, ExpireBlueprint, this);
				isCurrentlyBuilding = saveDataGenericTileObject.isCurrentlyBuilding;
			}
			else if (saveDataGenericTileObject.blueprintAutoBuildDate.hasValue && !string.IsNullOrEmpty(saveDataGenericTileObject.selfBuildingStructureSettlement))
			{
				selfBuildingStructureSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataGenericTileObject.selfBuildingStructureSettlement);
				CreateBuildParticlesAndScheduleBuildingCompletion(blueprintOnTile, selfBuildingStructureSettlement, saveDataGenericTileObject.blueprintAutoBuildDate);
			}
		}
		if (saveDataGenericTileObject.hasStructureConnector)
		{
			LoadMineShackSpotStructureConnector();
		}
		if (!base.traitContainer.HasTangibleStatus())
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < base.traitContainer.statuses.Count; i++)
		{
			Status status = base.traitContainer.statuses[i];
			if (status.isTangible && (!(status is Wet) || !(gridTileLocation.structure is Ocean)))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			GetOrCreateMapVisual();
		}
	}

	private void LoadBlueprintOnTile(string prefabName)
	{
		GameObject gameObject = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabName, Vector3.zero, Quaternion.identity, gridTileLocation.parentMap.structureParent);
		LocationStructureObject component = gameObject.GetComponent<LocationStructureObject>();
		gameObject.transform.position = gridTileLocation.centeredWorldLocation;
		component.RefreshAllTilemaps();
		List<LocationGridTile> tilesOccupiedByStructure = component.GetTilesOccupiedByStructure(gridTileLocation.parentMap);
		for (int i = 0; i < tilesOccupiedByStructure.Count; i++)
		{
			tilesOccupiedByStructure[i].SetHasBlueprint(hasBlueprint: true, _owner.localPlace.x, _owner.localPlace.y);
		}
		LocationStructureObject.Structure_Visual_Mode mode = LocationStructureObject.Structure_Visual_Mode.Blueprint;
		if (component.structureType.IsPlayerStructure())
		{
			mode = LocationStructureObject.Structure_Visual_Mode.Demonic_Structure_Blueprint;
		}
		component.SetVisualMode(mode, gridTileLocation.parentMap);
		component.SetTilesInStructure(tilesOccupiedByStructure.ToArray());
		blueprintOnTile = component;
		blueprintTemplateName = component.name;
	}

	public void CreateMineShackSpotStructureConnector()
	{
		StructureConnector structureConnector = GetOrCreateMapVisual().gameObject.AddComponent<StructureConnector>();
		structureConnector.OnPlaceConnector(gridTileLocation.parentMap);
		this.structureConnector = structureConnector;
		if (gridTileLocation.area != null)
		{
			gridTileLocation.area.tileObjectComponent.AddMineShackSpot(gridTileLocation);
		}
		gridTileLocation.SetIsDefault(state: false);
	}

	private void LoadMineShackSpotStructureConnector()
	{
		StructureConnector structureConnector = GetOrCreateMapVisual().gameObject.AddComponent<StructureConnector>();
		this.structureConnector = structureConnector;
		if (gridTileLocation.area != null)
		{
			gridTileLocation.area.tileObjectComponent.AddMineShackSpot(gridTileLocation);
		}
		if (this.structureConnector != null && gridTileLocation != null)
		{
			this.structureConnector.LoadConnectorForTileObjects(gridTileLocation.parentMap);
		}
	}

	public override string GetAdditionalTestingData()
	{
		string text = base.GetAdditionalTestingData();
		if (blueprintOnTile != null)
		{
			text = text + "\nBlueprint: " + blueprintOnTile.name;
			text = text + "\nBlueprint Expiry: " + blueprintExpiryDate.ConvertToContinuousDaysWithTime();
		}
		return text;
	}
}
