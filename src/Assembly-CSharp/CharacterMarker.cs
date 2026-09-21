using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Object_Pools;
using Pathfinding;
using Traits;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;
using UtilityScripts;

public class CharacterMarker : MapObjectVisual<Character>
{
	[SerializeField]
	private CharacterVisionTrigger _characterVisionTrigger;

	[Header("Character Marker Assets")]
	public Transform visualsParent;

	[SerializeField]
	private SpriteRenderer mainImg;

	[SerializeField]
	private SpriteRenderer hairImg;

	[SerializeField]
	private SpriteRenderer bodyImg;

	[SerializeField]
	private SpriteRenderer knockedOutHairImg;

	[SerializeField]
	private ParticleSystem bloodSplatterEffect;

	[SerializeField]
	private ParticleSystemRenderer bloodSplatterEffectRenderer;

	[SerializeField]
	private SpriteRenderer additionalEffectsImg;

	[SerializeField]
	private Vector3 _normalHairPosition;

	[SerializeField]
	private Vector3 _shamanHairPosition;

	public Transform particleEffectParentAllowRotation;

	[Header("Animation")]
	public Animator animator;

	[FormerlySerializedAs("animationListener")]
	[SerializeField]
	private CharacterMarkerAnimationListener _animationListener;

	[SerializeField]
	private string currentAnimation;

	[SerializeField]
	private string currentMountAnimation;

	[SerializeField]
	private RuntimeAnimatorController defaultController;

	[SerializeField]
	private RuntimeAnimatorController monsterController;

	[SerializeField]
	private int _pauseAnimationCounter;

	[Header("Pathfinding")]
	public CharacterAIPath pathfindingAI;

	public AIDestinationSetter destinationSetter;

	public Seeker seeker;

	public Rigidbody2D rigidbody;

	[FormerlySerializedAs("collider")]
	public BoxCollider2D visionCollider;

	[FormerlySerializedAs("visionCollider")]
	[FormerlySerializedAs("visionCollision")]
	public CharacterMarkerVisionCollider visionColliderComponent;

	public float endReachedDistance;

	[Header("Lighting")]
	public InnerMapLight markerLight;

	[Header("Combat")]
	public Transform projectileParent;

	[Header("Mount")]
	[SerializeField]
	private Animator mountAnimator;

	[SerializeField]
	private SpriteRenderer mountImg;

	[SerializeField]
	private SpriteRenderer mountBodyImg;

	public Transform mountProjectileParent;

	private Area _previousAreaLocation;

	private CharacterMarkerNameplate _nameplate;

	private LocationGridTile _destinationTile;

	private string _destroySchedule;

	private GameDate _destroyDate;

	private List<Area> areasInWildernessForFlee;

	private List<Vector3> avoidThisPositions;

	private int _currentColliderSize;

	private RaycastHit2D[] _lineOfSightHitObjects = new RaycastHit2D[15];

	private AttackAnimationStateMachine _attackAnimStateMachine;

	private IdleAnimationStateMachine _idleAnimStateMachine;

	public Character character { get; private set; }

	public List<IPointOfInterest> inVisionPOIs { get; private set; }

	public List<IPointOfInterest> inVisionPOIsButDiffStructure { get; private set; }

	public List<IPointOfInterest> unprocessedVisionPOIs { get; private set; }

	private List<IPointOfInterest> unprocessedVisionPOIInterruptsOnly { get; set; }

	private List<ActualGoapNode> unprocessedActionsOnly { get; set; }

	public List<Character> inVisionCharacters { get; private set; }

	public List<TileObject> inVisionTileObjects { get; private set; }

	public Action arrivalAction { get; private set; }

	public Action arrivalActionBeforeDigging { get; private set; }

	public IPointOfInterest targetPOI { get; private set; }

	public LocationGridTile destinationTile { get; private set; }

	public float progressionSpeedMultiplier { get; private set; }

	public bool isMoving { get; private set; }

	public bool hasFleePath { get; private set; }

	private float attackSpeedMeter { get; set; }

	public LocationGridTile previousGridTile { get; set; }

	public GameDate destroyDate => _destroyDate;

	public bool hasExpiry => !string.IsNullOrEmpty(_destroySchedule);

	public bool isMainVisualActive => mainImg.gameObject.activeSelf;

	public CharacterMarkerAnimationListener animationListener => _animationListener;

	public int sortingOrder => mainImg.sortingOrder;

	public CharacterMarkerNameplate nameplate => _nameplate;

	public RaycastHit2D[] lineOfSightHitObjects => _lineOfSightHitObjects;

	public void SetCharacter(Character character)
	{
		base.Initialize(character);
		base.name = (LocalizationSettings.SelectedLocale.LocaleName.Equals("English (en)") ? (character.name + "'s Marker") : (character.persistentID + " Marker"));
		this.character = character;
		CreateNameplate();
		UpdateName();
		UpdateSortingOrder();
		UpdateMarkerVisuals();
		UpdateAnimatorController();
		UpdateActionIcon();
		ForceUpdateMarkerVisualsBasedOnAnimation();
		CreateCollisionTrigger();
		SetVisualState(state: true);
		SetLightState(p_state: true);
		unprocessedVisionPOIs = new List<IPointOfInterest>(50);
		unprocessedVisionPOIInterruptsOnly = new List<IPointOfInterest>(50);
		unprocessedActionsOnly = new List<ActualGoapNode>(50);
		inVisionPOIs = new List<IPointOfInterest>(50);
		inVisionPOIsButDiffStructure = new List<IPointOfInterest>(50);
		inVisionCharacters = new List<Character>(50);
		inVisionTileObjects = new List<TileObject>(50);
		attackSpeedMeter = 0f;
		OnProgressionSpeedChanged(GameManager.Instance.currProgressionSpeed);
		UpdateHairState();
		if (areasInWildernessForFlee == null)
		{
			areasInWildernessForFlee = new List<Area>();
		}
		AddListeners();
		PathfindingManager.Instance.AddAgent(pathfindingAI);
		if (GameManager.Instance.isPaused)
		{
			PauseAnimation();
		}
		List<Trait> traitOverrideFunctions = character.traitContainer.GetTraitOverrideFunctions("Initiate_Map_Visual_Trait");
		if (traitOverrideFunctions != null)
		{
			for (int i = 0; i < traitOverrideFunctions.Count; i++)
			{
				traitOverrideFunctions[i].OnInitiateMapObjectVisual(character);
			}
		}
		UpdateTraversableTags();
		UpdateTagPenalties();
		seeker.graphMask = InnerMapManager.Instance.mainGraphMask;
	}

	private void Start()
	{
		_attackAnimStateMachine = animator.GetBehaviour<AttackAnimationStateMachine>();
		_idleAnimStateMachine = animator.GetBehaviour<IdleAnimationStateMachine>();
		_attackAnimStateMachine.SetCharacterMarker(this);
		_idleAnimStateMachine.SetCharacterMarker(this);
	}

	private void OnDisable()
	{
		if (!(LevelLoaderManager.Instance == null) && !LevelLoaderManager.Instance.isLoadingNewScene && !(UIManager.Instance == null))
		{
			if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == character)
			{
				UIManager.Instance.characterInfoUI.CloseMenu();
			}
			if (UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == character)
			{
				UIManager.Instance.monsterInfoUI.CloseMenu();
			}
			if (character != null && character != null && InnerMapCameraMove.Instance != null && InnerMapCameraMove.Instance.target == base.transform)
			{
				InnerMapCameraMove.Instance.CenterCameraOn(null);
			}
		}
	}

	private void OnEnable()
	{
		if (character != null)
		{
			UpdateAnimation();
		}
	}

	public void ManualUpdate()
	{
		if (!GameManager.Instance.gameHasStarted || GameManager.Instance.isPaused || character.isBeingSeized)
		{
			return;
		}
		if (attackSpeedMeter < (float)character.combatComponent.attackSpeed)
		{
			attackSpeedMeter += Time.deltaTime * 1000f * progressionSpeedMultiplier;
			if (base.hasHPBarGO && base.hpBarGO.activeSelf)
			{
				UpdateAttackSpeedMeter();
			}
		}
		pathfindingAI.UpdateMe();
	}

	private void LateUpdate()
	{
		if (character != null && !character.hasBeenCleanedUp)
		{
			UpdateVisualBasedOnCurrentAnimationFrame();
			UpdateMountVisualBasedOnCurrentAnimationFrame();
			if (character.stateComponent.currentState is CombatState combatState)
			{
				combatState.LateUpdate();
			}
		}
	}

	public void InitialPlaceMarkerAt(LocationGridTile tile)
	{
		visionColliderComponent.Initialize();
		PlaceMarkerAt(tile);
		pathfindingAI.UpdateMe();
		character.movementComponent.UpdateSpeed();
	}

	public void LoadMarkerPlacement(SaveDataCharacter data, Region region)
	{
		visionColliderComponent.Initialize();
		SetCollidersState(state: true);
		Transform obj = base.transform;
		obj.SetParent(region.innerMap.objectsParent);
		obj.position = data.worldPos;
		visualsParent.transform.localRotation = data.rotation;
		UpdateActionIcon();
		UpdateAnimation();
		character.SetGridTilePosition(base.transform.localPosition);
		if (character.gridTileLocation != null)
		{
			LocationAwarenessUtility.AddToAwarenessList(character, character.gridTileLocation);
		}
		if (data.hasExpiry)
		{
			ScheduleExpiry(data.markerExpiryDate);
		}
	}

	public void PlaceMarkerAt(LocationGridTile tile)
	{
		base.gameObject.transform.SetParent(tile.parentMap.objectsParent);
		tile.structure.region.AddCharacterToLocation(character);
		SetActiveState(state: true);
		UpdateAnimation();
		pathfindingAI.Teleport(tile.centeredWorldLocation);
		UpdatePosition();
		if (!(tile.tileObjectComponent.objHere is WurmHole) && character.currentStructure != tile.structure)
		{
			character.currentStructure?.RemoveCharacterAtLocation(character);
			tile.structure.AddCharacterAtLocation(character);
		}
		UpdateActionIcon();
		SetCollidersState(state: true);
		LocationAwarenessUtility.AddToAwarenessList(character, character.gridTileLocation);
		character.reactionComponent.UpdateHiddenState();
		if ((bool)_nameplate)
		{
			_nameplate.UpdateNameActiveState();
		}
	}

	protected override void OnPointerLeftClick(Character poi)
	{
		base.OnPointerLeftClick(poi);
		UIManager.Instance.ShowCharacterInfo(character, centerOnCharacter: true);
	}

	protected override void OnPointerRightClick(Character poi)
	{
		base.OnPointerRightClick(poi);
		UIManager.Instance.ShowPlayerActionContextMenu(poi, poi.worldPosition, p_isScreenPosition: false);
	}

	protected override void OnPointerMiddleClick(Character poi)
	{
		base.OnPointerMiddleClick(poi);
		Character obj = UIManager.Instance.characterInfoUI.activeCharacter ?? UIManager.Instance.monsterInfoUI.activeMonster;
		LocationStructure activeStructure = UIManager.Instance.structureInfoUI.activeStructure;
		if (obj != null)
		{
		}
	}

	protected override void OnPointerEnter(Character character)
	{
		base.OnPointerEnter(character);
		InnerMapManager.Instance.SetCurrentlyHoveredPOI(character);
		InnerMapManager.Instance.ShowTileData(this.character.gridTileLocation, this.character);
		if (UIManager.Instance.GetCurrentlySelectedCharacter() != character)
		{
			ShowThoughtsAndNameplate();
		}
		if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActiveIntel != null)
		{
			string text = string.Empty;
			if (PlayerManager.Instance.player.currentActiveIntel.actor != character)
			{
				if (character.relationshipContainer.HasRelationshipWith(PlayerManager.Instance.player.currentActiveIntel.actor))
				{
					string localizedRelationshipNameWith = character.relationshipContainer.GetLocalizedRelationshipNameWith(PlayerManager.Instance.player.currentActiveIntel.actor);
					text = text + PlayerManager.Instance.player.currentActiveIntel.actor.visuals.GetCharacterNameWithIconAndColor() + " - " + localizedRelationshipNameWith + " - " + character.visuals.GetCharacterNameWithIconAndColor() + "\n";
				}
			}
			else
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Actor_Of_Intel");
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				text = text + log.logText + "\n";
				LogPool.Release(log);
			}
			if (PlayerManager.Instance.player.currentActiveIntel.target is Character character2 && character2 != PlayerManager.Instance.player.currentActiveIntel.actor)
			{
				if (character2 != character)
				{
					if (character.relationshipContainer.HasRelationshipWith(character2))
					{
						string localizedRelationshipNameWith2 = character.relationshipContainer.GetLocalizedRelationshipNameWith(character2);
						text = text + character2.visuals.GetCharacterNameWithIconAndColor() + " - " + localizedRelationshipNameWith2 + " - " + character.visuals.GetCharacterNameWithIconAndColor() + "\n";
					}
				}
				else
				{
					Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Target_Of_Intel");
					log2.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					text = text + log2.logText + "\n";
					LogPool.Release(log2);
				}
			}
			if (!string.IsNullOrEmpty(text) && _nameplate != null)
			{
				_nameplate.ShowIntelHelper(text);
			}
		}
		if ((bool)_nameplate)
		{
			_nameplate.UpdateNameActiveState();
		}
	}

	private bool HasRelationshipWithIntel(IIntel intel)
	{
		if (intel.actor != this.character)
		{
			if (this.character.relationshipContainer.HasRelationshipWith(intel.actor))
			{
				return true;
			}
			if (intel.target is Character character)
			{
				if (character == this.character)
				{
					return true;
				}
				if (this.character.relationshipContainer.HasRelationshipWith(character))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override void OnPointerExit(Character poi)
	{
		base.OnPointerExit(poi);
		if (InnerMapManager.Instance != null && InnerMapManager.Instance.currentlyHoveredPoi == poi)
		{
			InnerMapManager.Instance.SetCurrentlyHoveredPOI(null);
		}
		if (UIManager.Instance != null)
		{
			UIManager.Instance.HideSmallInfo();
			if (UIManager.Instance.GetCurrentlySelectedCharacter() != character)
			{
				HideThoughtsAndNameplate();
			}
		}
		if (_nameplate != null)
		{
			_nameplate.HideIntelHelper();
			_nameplate.UpdateNameActiveState();
		}
	}

	private void AddListeners()
	{
		Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_REMOVED, OnCharacterLostTrait);
		Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);
		Messenger.AddListener<IPointOfInterest>(CharacterSignals.REPROCESS_POI, ReprocessPOI);
		Messenger.AddListener<IIntel>(PlayerSignals.ACTIVE_INTEL_SET, OnActiveIntelSet);
		Messenger.AddListener(PlayerSignals.ACTIVE_INTEL_REMOVED, OnActiveIntelRemoved);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
		Messenger.AddListener<bool>(CharacterSignals.TOGGLE_CHARACTER_MARKER_NAMEPLATE, OnToggleCharacterMarkerNameplate);
		Messenger.AddListener<Character>(PlayerSignals.PLAYER_STORED_CHARACTER, OnPlayerStoredCharacterAsTarget);
		Messenger.AddListener<Character>(PlayerSignals.PLAYER_REMOVED_STORED_CHARACTER, OnPlayerRemoveStoredCharacterAsTarget);
		Messenger.AddListener<MovingTileObject>(TileObjectSignals.MOVING_TILE_OBJECT_EXPIRED, OnMovingTileObjectExpired);
	}

	private void RemoveListeners()
	{
		Messenger.RemoveListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
		Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
		Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_REMOVED, OnCharacterLostTrait);
		Messenger.RemoveListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);
		Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.REPROCESS_POI, ReprocessPOI);
		Messenger.RemoveListener<IIntel>(PlayerSignals.ACTIVE_INTEL_SET, OnActiveIntelSet);
		Messenger.RemoveListener(PlayerSignals.ACTIVE_INTEL_REMOVED, OnActiveIntelRemoved);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
		Messenger.RemoveListener<bool>(CharacterSignals.TOGGLE_CHARACTER_MARKER_NAMEPLATE, OnToggleCharacterMarkerNameplate);
		Messenger.RemoveListener<Character>(PlayerSignals.PLAYER_STORED_CHARACTER, OnPlayerStoredCharacterAsTarget);
		Messenger.RemoveListener<Character>(PlayerSignals.PLAYER_REMOVED_STORED_CHARACTER, OnPlayerRemoveStoredCharacterAsTarget);
		Messenger.RemoveListener<MovingTileObject>(TileObjectSignals.MOVING_TILE_OBJECT_EXPIRED, OnMovingTileObjectExpired);
	}

	private void OnCharacterChangedName(Character p_character)
	{
		if (p_character == character)
		{
			UpdateName();
		}
	}

	private void OnCharacterGainedTrait(Character characterThatGainedTrait, Trait trait)
	{
		if (characterThatGainedTrait == character)
		{
			SelfGainedTrait(characterThatGainedTrait, trait);
		}
		else
		{
			OtherCharacterGainedTrait(characterThatGainedTrait, trait);
		}
	}

	private void OnCharacterLostTrait(Character character, Trait trait)
	{
		if (character == this.character)
		{
			UpdateAnimation();
			UpdateActionIcon();
		}
	}

	private void OnToggleCharacterMarkerNameplate(bool state)
	{
		if ((bool)_nameplate)
		{
			_nameplate.UpdateNameActiveState();
		}
	}

	private void SelfGainedTrait(Character characterThatGainedTrait, Trait trait)
	{
		if (!characterThatGainedTrait.limiterComponent.canPerform)
		{
			if (character.combatComponent.isInCombat)
			{
				characterThatGainedTrait.stateComponent.ExitCurrentState();
			}
			character.combatComponent.ClearHostilesInRange(processCombatBehavior: false);
			character.combatComponent.ClearAvoidInRange(processCombatBehavior: false);
		}
		UpdateAnimation();
		UpdateActionIcon();
	}

	private void OtherCharacterGainedTrait(Character otherCharacter, Trait trait)
	{
		if (character == null || character.hasBeenCleanedUp || otherCharacter.hasBeenCleanedUp)
		{
			return;
		}
		if (trait.name == "Invisible")
		{
			character.combatComponent.RemoveHostileInRange(otherCharacter);
			character.combatComponent.RemoveAvoidInRange(otherCharacter);
			RemovePOIFromInVisionRange(otherCharacter);
			return;
		}
		if (IsPOIInVision(otherCharacter))
		{
			character.CreateJobsOnTargetGainTrait(otherCharacter, trait);
		}
		if (!character.combatComponent.GetLethalityFromCombatData(otherCharacter) && otherCharacter.traitContainer.HasTrait("Unconscious", "Paralyzed", "Restrained"))
		{
			character.combatComponent.RemoveHostileInRange(otherCharacter);
			character.combatComponent.RemoveAvoidInRange(otherCharacter);
		}
	}

	public void OnTileObjectRemovedFromTile(TileObject obj, Character removedBy, LocationGridTile removedFrom)
	{
		character.combatComponent.RemoveHostileInRange(obj);
		character.combatComponent.RemoveAvoidInRange(obj);
		RemovePOIFromInVisionRange(obj);
		RemovePOIAsInRangeButDifferentStructure(obj);
	}

	private void OnActiveIntelSet(IIntel intel)
	{
		if (PlayerManager.Instance.player.CanShareIntelTo(character, intel) && HasRelationshipWithIntel(intel) && (bool)_nameplate)
		{
			_nameplate.SetHighlighterState(state: true);
		}
	}

	private void OnActiveIntelRemoved()
	{
		if ((bool)_nameplate)
		{
			_nameplate.HideIntelHelper();
			_nameplate.SetHighlighterState(state: false);
		}
	}

	private void OnPlayerStoredCharacterAsTarget(Character p_character)
	{
		if (character != null && (bool)_nameplate)
		{
			_nameplate.UpdateNameActiveState();
		}
	}

	private void OnPlayerRemoveStoredCharacterAsTarget(Character p_character)
	{
		if (character != null && (bool)_nameplate)
		{
			_nameplate.UpdateNameActiveState();
		}
	}

	private void OnMovingTileObjectExpired(MovingTileObject p_tileObject)
	{
		if (inVisionPOIs.Contains(p_tileObject))
		{
			RemovePOIFromInVisionRange(p_tileObject);
		}
	}

	private void OnProgressionSpeedChanged(PROGRESSION_SPEED progSpeed)
	{
		switch (progSpeed)
		{
		case PROGRESSION_SPEED.X1:
			progressionSpeedMultiplier = 1f;
			break;
		case PROGRESSION_SPEED.X2:
			progressionSpeedMultiplier = 1.5f;
			break;
		case PROGRESSION_SPEED.X4:
			progressionSpeedMultiplier = 2f;
			break;
		}
		character.movementComponent.UpdateSpeed();
		UpdateAnimationSpeed();
	}

	public void UpdateName()
	{
		_nameplate?.UpdateName();
	}

	private void CreateNameplate()
	{
		GameObject gameObject = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterMarkerNameplate", base.transform.position, Quaternion.identity, UIManager.Instance.characterMarkerNameplateParent);
		_nameplate = gameObject.GetComponent<CharacterMarkerNameplate>();
		_nameplate.Initialize(this);
	}

	public void UpdateActionIcon()
	{
		if ((bool)_nameplate && isMainVisualActive)
		{
			_nameplate.UpdateActionIcon();
		}
	}

	public override void Reset()
	{
		TryCancelExpiry();
		destinationTile = null;
		SetMainVisualScale(Vector2.one);
		SetMainVisualTint(Color.white);
		SetVisionColliderState(p_state: true);
		SetVisionTriggerState(p_state: true);
		SetVisionColliderSize(8);
		_pauseAnimationCounter = 0;
		SetMainVisualTint(Color.white);
		if (_nameplate != null)
		{
			ObjectPoolManager.Instance.DestroyObject(_nameplate);
		}
		_nameplate = null;
		PathfindingManager.Instance.RemoveAgent(pathfindingAI);
		RemoveListeners();
		HideHPBar();
		HideAdditionalEffect();
		_previousAreaLocation?.locationCharacterTracker.RemoveCharacterFromLocation(character, _previousAreaLocation);
		Messenger.Broadcast(CharacterSignals.CHARACTER_EXITED_AREA, character, _previousAreaLocation);
		Messenger.Broadcast(CharacterSignals.UPDATE_CHARACTER_AWARENESS_STATE, character);
		visionColliderComponent.Reset();
		if (visionTrigger != null)
		{
			UnityEngine.Object.Destroy(visionTrigger.gameObject);
		}
		visionTrigger = null;
		SetCollidersState(state: false);
		pathfindingAI.ResetThis();
		character = null;
		_previousAreaLocation = null;
		areasInWildernessForFlee.Clear();
		if (animationListener != null)
		{
			animationListener.Reset();
		}
		mainImg.transform.localPosition = Vector3.zero;
		SetLightState(p_state: true);
		base.Reset();
	}

	protected override void OnDestroy()
	{
		pathfindingAI = null;
		destinationSetter = null;
		seeker = null;
		visionCollider = null;
		character = null;
		_attackAnimStateMachine?.SetCharacterMarker(null);
		_idleAnimStateMachine?.SetCharacterMarker(null);
		base.OnDestroy();
	}

	protected override void DestroyAllParticleEffects()
	{
		Transform[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<Transform>(particleEffectParent.gameObject);
		if (componentsInDirectChildren != null)
		{
			for (int i = 0; i < componentsInDirectChildren.Length; i++)
			{
				ObjectPoolManager.Instance.DestroyObject(componentsInDirectChildren[i].gameObject);
			}
		}
		componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<Transform>(particleEffectParentAllowRotation.gameObject);
		if (componentsInDirectChildren != null)
		{
			for (int j = 0; j < componentsInDirectChildren.Length; j++)
			{
				ObjectPoolManager.Instance.DestroyObject(componentsInDirectChildren[j].gameObject);
			}
		}
	}

	public void GoTo(LocationGridTile destinationTile, Action arrivalAction = null)
	{
		if (!character.movementComponent.isStationary)
		{
			if (character.trapStructure.IsTrappedAndTrapStructureIsNot(destinationTile.structure))
			{
				character.trapStructure.ResetAllTrapStructures();
			}
			if (character.trapStructure.IsTrappedAndTrapAreaIsNot(destinationTile.area))
			{
				character.trapStructure.ResetTrapArea();
			}
			pathfindingAI.ClearAllCurrentPathData();
			this.destinationTile = destinationTile;
			this.arrivalAction = arrivalAction;
			targetPOI = null;
			if (destinationTile == character.gridTileLocation)
			{
				Action action = this.arrivalAction;
				ClearArrivalAction();
				action?.Invoke();
			}
			else
			{
				SetDestination(destinationTile.GetPositionWithinTileThatIsOnAWalkableNode(), destinationTile);
				StartMovement();
			}
		}
	}

	public void GoToPOI(IPointOfInterest targetPOI, Action arrivalAction = null, Action p_arrivalActionBeforeDigging = null)
	{
		if (this.character.movementComponent.isStationary)
		{
			return;
		}
		pathfindingAI.ClearAllCurrentPathData();
		this.arrivalAction = arrivalAction;
		arrivalActionBeforeDigging = p_arrivalActionBeforeDigging;
		this.targetPOI = targetPOI;
		if (targetPOI.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
		{
			Character character = targetPOI as Character;
			if (character.hasMarker && !character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld)
			{
				LocationGridTile gridTileLocation = character.gridTileLocation;
				if (gridTileLocation != null)
				{
					if (gridTileLocation.HasUnwalkableNode() && (character.traitContainer.HasTrait("Unconscious") || character.traitContainer.HasTrait("Paralyzed") || character.traitContainer.HasTrait("Restrained")))
					{
						SetDestination(gridTileLocation.GetPositionWithinTileThatIsOnAWalkableNode(), gridTileLocation);
					}
					else
					{
						SetTargetTransform(character.marker.transform);
					}
				}
				else
				{
					SetTargetTransform(character.marker.transform);
				}
			}
		}
		else if (targetPOI is MovingTileObject)
		{
			SetTargetTransform(targetPOI.mapObjectVisual.transform);
		}
		else
		{
			if (targetPOI.gridTileLocation == null)
			{
				throw new Exception($"{this.character.name} is trying to go to a {targetPOI} but its tile location is null");
			}
			LocationGridTile locationGridTile;
			if (targetPOI is TileObject tileObject && tileObject.mapObjectVisual != null && tileObject.gridTileLocation != null && TileObjectDB.OccupiesMoreThan1Tile(tileObject.tileObjectType))
			{
				locationGridTile = targetPOI.gridTileLocation.parentMap.GetTileFromWorldPosition(targetPOI.worldPosition);
				if (locationGridTile == null)
				{
					locationGridTile = targetPOI.gridTileLocation;
				}
			}
			else
			{
				locationGridTile = targetPOI.gridTileLocation;
			}
			SetDestination(locationGridTile.GetPositionWithinTileThatIsOnAWalkableNode(), locationGridTile);
		}
		StartMovement();
	}

	public void GoTo(ITraitable target, Action arrivalAction = null)
	{
		if (!character.movementComponent.isStationary)
		{
			if (target is IPointOfInterest pointOfInterest)
			{
				GoToPOI(pointOfInterest, arrivalAction);
				return;
			}
			pathfindingAI.ClearAllCurrentPathData();
			this.arrivalAction = arrivalAction;
			targetPOI = null;
			SetTargetTransform(target.worldObject);
			StartMovement();
		}
	}

	public void GoTo(Vector3 destination, Action arrivalAction = null)
	{
		pathfindingAI.ClearAllCurrentPathData();
		destinationTile = destinationTile;
		this.arrivalAction = arrivalAction;
		SetTargetTransform(null);
		SetDestination(destination);
		StartMovement();
	}

	public void ArrivedAtTarget(ref bool shouldRecomputePath)
	{
		StopMovement();
		LocationGridTile locationGridTile = null;
		LocationGridTile attainedDestinationTile = null;
		ProcessDestinationAndAttainedDestinationTile(ref locationGridTile, ref attainedDestinationTile);
		Action action = arrivalActionBeforeDigging;
		arrivalActionBeforeDigging = null;
		action?.Invoke();
		if (character.traitContainer.HasTrait("Vampire") && attainedDestinationTile != null && locationGridTile != null && locationGridTile != attainedDestinationTile)
		{
			Vampire traitOrStatus = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
			if (traitOrStatus.CanTransformIntoBat())
			{
				if (!traitOrStatus.isInVampireBatForm && !traitOrStatus.isTraversingUnwalkableAsBat && !character.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Vampire))
				{
					if (!PathfindingManager.Instance.HasPathEvenDiffRegion(attainedDestinationTile, locationGridTile) && character.interruptComponent.TriggerInterrupt(INTERRUPT.Transform_To_Bat, character))
					{
						traitOrStatus.SetIsTraversingUnwalkableAsBat(state: true);
						shouldRecomputePath = true;
						return;
					}
				}
				else if (traitOrStatus.isTraversingUnwalkableAsBat)
				{
					shouldRecomputePath = true;
					return;
				}
			}
		}
		if (character.combatComponent.isInCombat)
		{
			CombatState combatState = character.stateComponent.currentState as CombatState;
			if (combatState.isAttacking && combatState.currentClosestHostile != null && !character.movementComponent.HasPathToEvenIfDiffRegion(combatState.currentClosestHostile.gridTileLocation))
			{
				if (attainedDestinationTile != null && locationGridTile != null && locationGridTile != attainedDestinationTile && character.movementComponent.AttackBlockersOnReachEndPath(pathfindingAI.currentPath, attainedDestinationTile, locationGridTile))
				{
					targetPOI = null;
				}
				else if (character.combatComponent.RemoveHostileInRange(combatState.currentClosestHostile))
				{
					targetPOI = null;
				}
				return;
			}
		}
		if (character.movementComponent.CanDig() && attainedDestinationTile != null && locationGridTile != null && locationGridTile != attainedDestinationTile && !PathfindingManager.Instance.HasPathEvenDiffRegion(attainedDestinationTile, locationGridTile) && character.movementComponent.DigOnReachEndPath(pathfindingAI.currentPath, attainedDestinationTile, locationGridTile))
		{
			targetPOI = null;
			return;
		}
		Action action2 = arrivalAction;
		ClearArrivalAction();
		action2?.Invoke();
		targetPOI = null;
	}

	private void ProcessDestinationAndAttainedDestinationTile(ref LocationGridTile destinationTile, ref LocationGridTile attainedDestinationTile)
	{
		destinationTile = GetDestinationTile();
		if (character.gridTileLocation == destinationTile || character.gridTileLocation.IsNeighbour(destinationTile))
		{
			attainedDestinationTile = destinationTile;
		}
		else
		{
			attainedDestinationTile = character.gridTileLocation;
		}
	}

	public LocationGridTile GetDestinationTile()
	{
		if (targetPOI != null)
		{
			return targetPOI.gridTileLocation;
		}
		if (destinationTile != null)
		{
			return destinationTile;
		}
		return null;
	}

	public void StartMovement()
	{
		if (!character.movementComponent.isStationary)
		{
			isMoving = true;
			character.movementComponent.UpdateSpeed();
			pathfindingAI.SetIsStopMovement(state: false);
			UpdateAnimation();
		}
	}

	public void StopMovement()
	{
		isMoving = false;
		pathfindingAI.SetIsStopMovement(state: true);
		UpdateAnimation();
	}

	public void PerTickMovement()
	{
		if (isMoving)
		{
			character.PerTickDuringMovement();
		}
	}

	public override void LookAt(Vector3 target, bool force = false)
	{
		if (force || (character.limiterComponent.canPerform && character.limiterComponent.canMove))
		{
			Vector3 vector = target - base.transform.position;
			vector.Normalize();
			float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			Rotate(Quaternion.Euler(0f, 0f, num - 90f), force);
		}
	}

	public override void Rotate(Quaternion target, bool force = false)
	{
		if (force || (character.limiterComponent.canPerform && character.limiterComponent.canMove))
		{
			visualsParent.rotation = target;
		}
	}

	public void SetDestination(Vector3 destination, LocationGridTile destinationTile)
	{
		this.destinationTile = destinationTile;
		pathfindingAI.destination = destination;
		pathfindingAI.canSearch = true;
	}

	public void SetDestination(Vector3 destination)
	{
		destinationTile = null;
		pathfindingAI.destination = destination;
		pathfindingAI.canSearch = true;
	}

	public void SetTargetTransform(Transform target)
	{
		destinationSetter.target = target;
		pathfindingAI.canSearch = true;
	}

	public bool IsTargetPOIInPathfinding(IPointOfInterest poi)
	{
		return destinationSetter.target == poi.mapObjectVisual.transform;
	}

	public void ClearArrivalAction()
	{
		arrivalAction = null;
	}

	[ContextMenu("Force Update Visuals")]
	public void ForceUpdateVisuals()
	{
		character.visuals.UpdateAllVisuals(character);
	}

	private void ForceUpdateMarkerVisualsBasedOnAnimation()
	{
		CharacterSpritesPerAnimation markerAnimations = character.visuals.markerAnimations;
		if (markerAnimations != null)
		{
			string markerAnimationSpriteName = CharacterManager.Instance.GetMarkerAnimationSpriteName(bodyImg.sprite);
			Sprite sprite = markerAnimations.GetSprite(currentAnimation, markerAnimationSpriteName);
			if (sprite != null)
			{
				mainImg.sprite = sprite;
			}
			else
			{
				mainImg.sprite = character.visuals.defaultSprite;
			}
		}
	}

	private void ForceUpdateMountMarkerVisualsBasedOnAnimation()
	{
		Character mountedCharacter = character.mountComponent.mountedCharacter;
		if (mountedCharacter == null)
		{
			return;
		}
		string markerAnimationSpriteName = CharacterManager.Instance.GetMarkerAnimationSpriteName(mountBodyImg.sprite);
		if (mountedCharacter.visuals != null && mountedCharacter.visuals.markerAnimations != null)
		{
			Sprite sprite = mountedCharacter.visuals.markerAnimations.GetSprite(currentMountAnimation, markerAnimationSpriteName);
			if (sprite != null)
			{
				mountImg.sprite = sprite;
			}
			else
			{
				mountImg.sprite = character.visuals.defaultSprite;
			}
		}
	}

	public void ForceUpdateMarkerVisualsBasedOnAnimationName()
	{
		string p_spriteID = "idle_1";
		if (currentAnimation == "Sleep")
		{
			p_spriteID = "sleep_1";
		}
		Sprite sprite = character.visuals.markerAnimations.GetSprite(currentAnimation, p_spriteID);
		if (sprite != null)
		{
			mainImg.sprite = sprite;
		}
		else
		{
			mainImg.sprite = character.visuals.defaultSprite;
		}
	}

	private void UpdateAnimatorController()
	{
		if (character.combatComponent.rangeType == RANGE_TYPE.RANGED)
		{
			animator.runtimeAnimatorController = defaultController;
		}
		else if (character.characterClass.lungeOnMeleeAttack)
		{
			animator.runtimeAnimatorController = monsterController;
		}
		else
		{
			animator.runtimeAnimatorController = defaultController;
		}
	}

	private void UpdateMountAnimatorController()
	{
		if (character.combatComponent.rangeType == RANGE_TYPE.RANGED)
		{
			mountAnimator.runtimeAnimatorController = defaultController;
		}
		else if (character.characterClass.lungeOnMeleeAttack)
		{
			mountAnimator.runtimeAnimatorController = monsterController;
		}
		else
		{
			mountAnimator.runtimeAnimatorController = defaultController;
		}
	}

	private void UpdateVisualBasedOnCurrentAnimationFrame()
	{
		if (bodyImg.sprite == null)
		{
			return;
		}
		CharacterSpritesPerAnimation markerAnimations = character.visuals.markerAnimations;
		if (markerAnimations == null)
		{
			return;
		}
		string markerAnimationSpriteName = CharacterManager.Instance.GetMarkerAnimationSpriteName(bodyImg.sprite);
		Sprite sprite = null;
		if (character.isHidden && currentAnimation == "Idle")
		{
			switch (markerAnimationSpriteName)
			{
			case "idle_1":
				sprite = markerAnimations.GetSprite("Hidden", "hidden_1");
				break;
			case "idle_2":
				sprite = markerAnimations.GetSprite("Hidden", "hidden_2");
				break;
			case "idle_3":
				sprite = markerAnimations.GetSprite("Hidden", "hidden_3");
				break;
			case "idle_4":
				sprite = markerAnimations.GetSprite("Hidden", "hidden_4");
				break;
			}
			if (sprite == null)
			{
				sprite = markerAnimations.GetSprite("Hidden", "hidden_1");
			}
		}
		if (sprite != null)
		{
			mainImg.sprite = sprite;
			return;
		}
		sprite = markerAnimations.GetSprite(currentAnimation, markerAnimationSpriteName);
		if (sprite != null)
		{
			mainImg.sprite = sprite;
		}
		else
		{
			mainImg.sprite = character.visuals.defaultSprite;
		}
	}

	private void UpdateMountVisualBasedOnCurrentAnimationFrame()
	{
		if (mountImg.sprite == null || !mountImg.gameObject.activeSelf)
		{
			return;
		}
		Character mountedCharacter = character.mountComponent.mountedCharacter;
		if (mountedCharacter != null && mountedCharacter.visuals != null && mountedCharacter.visuals.markerAnimations != null)
		{
			string markerAnimationSpriteName = CharacterManager.Instance.GetMarkerAnimationSpriteName(mountBodyImg.sprite);
			Sprite sprite = mountedCharacter.visuals.markerAnimations.GetSprite(currentMountAnimation, markerAnimationSpriteName);
			if (sprite != null)
			{
				mountImg.sprite = sprite;
			}
			else
			{
				mountImg.sprite = character.visuals.defaultSprite;
			}
		}
	}

	private void PlayWalkingAnimation()
	{
		if (!character.mountComponent.IsMounting())
		{
			PlayAnimation("Walk");
		}
		else
		{
			PlayAnimation("Idle");
		}
		PlayMountAnimation("Walk");
	}

	private void PlayIdleAnimation()
	{
		PlayAnimation("Idle");
		PlayMountAnimation("Idle");
	}

	private void PlaySleepAnimation()
	{
		PlayAnimation("Sleep");
		PlayMountAnimation("Sleep");
	}

	private void PlayDeadAnimation()
	{
		PlayAnimation("Dead");
		PlayMountAnimation("Dead");
	}

	public void PlayAttackAnimation()
	{
		PlayAnimation("Attack");
		PlayMountAnimation("Idle");
		_animationListener.StartAttackExecution();
	}

	private void PlayAnimationName(string p_animation)
	{
		PlayAnimation(p_animation);
		PlayMountAnimation(p_animation);
	}

	private void PlayAnimation(string animation)
	{
		if (base.gameObject.activeSelf && animator.gameObject.activeSelf)
		{
			currentAnimation = animation;
			animator.Play(animation, 0, 0.5f);
		}
	}

	private void PlayMountAnimation(string animation)
	{
		if (mountImg.gameObject.activeSelf)
		{
			currentMountAnimation = animation;
			mountAnimator.Play(animation, 0, 0.5f);
		}
	}

	public void SetCurrentAnimationName(string p_name)
	{
		currentAnimation = p_name;
	}

	public void SetCurrentMountAnimationName(string p_name)
	{
		currentMountAnimation = p_name;
	}

	public void UpdateAnimation()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (!character.carryComponent.IsNotBeingCarried())
		{
			PlaySleepAnimation();
			ResetBlood();
			return;
		}
		if (character.isDead)
		{
			PlayDeadAnimation();
			if (character.visuals.HasBlood())
			{
				StartCoroutine(StartBlood());
			}
		}
		else
		{
			ResetBlood();
			if (character.numOfNonSecretActionsBeingPerformedOnThis > 0)
			{
				if ((!character.limiterComponent.canMove || (!character.limiterComponent.canPerform && !character.limiterComponent.canWitness)) && (!character.traitContainer.HasTrait("Hibernating", "Stoned") || (!(character is Golem) && !(character is Troll))))
				{
					PlaySleepAnimation();
				}
				else
				{
					PlayIdleAnimation();
				}
			}
			else if ((!character.limiterComponent.canMove || (!character.limiterComponent.canPerform && !character.limiterComponent.canWitness)) && (!character.traitContainer.HasTrait("Hibernating", "Stoned") || (!(character is Golem) && !(character is Troll))))
			{
				PlaySleepAnimation();
			}
			else if (isMoving)
			{
				PlayWalkingAnimation();
			}
			else if (character.currentActionNode != null && !string.IsNullOrEmpty(character.currentActionNode.currentStateName) && !string.IsNullOrEmpty(character.currentActionNode.currentState.animationName))
			{
				PlayAnimationName(character.currentActionNode.currentState.animationName);
			}
			else if (character.currentActionNode != null && !string.IsNullOrEmpty(character.currentActionNode.action.animationName))
			{
				PlayAnimationName(character.currentActionNode.action.animationName);
			}
			else
			{
				PlayIdleAnimation();
			}
		}
		UpdateHairState();
		UpdateMountVisuals();
	}

	private IEnumerator StartBlood()
	{
		bloodSplatterEffect.gameObject.SetActive(value: true);
		yield return GameUtilities.waitFor5Seconds;
		bloodSplatterEffect.Pause();
	}

	private void ResetBlood()
	{
		if (bloodSplatterEffect.gameObject.activeSelf)
		{
			bloodSplatterEffect.gameObject.SetActive(value: false);
			bloodSplatterEffect.Clear();
		}
	}

	public void PauseAnimation()
	{
		_pauseAnimationCounter++;
		UpdatePauseAnimationSpeed();
	}

	public void UnpauseAnimation()
	{
		_pauseAnimationCounter--;
		UpdatePauseAnimationSpeed();
	}

	public void UpdatePauseAnimationSpeed()
	{
		if (_pauseAnimationCounter > 0)
		{
			animator.speed = 0f;
			mountAnimator.speed = 0f;
		}
		else
		{
			animator.speed = 1f;
			mountAnimator.speed = 1f;
		}
	}

	public void SetAnimationTrigger(string triggerName)
	{
		if (!(triggerName == "Attack") || character.stateComponent.currentState is CombatState)
		{
			if ((object)animator.runtimeAnimatorController != null)
			{
				animator.SetTrigger(triggerName);
			}
			if (triggerName == "Attack")
			{
				_animationListener.StartAttackExecution();
			}
		}
	}

	public void DoDummyAttack(IPointOfInterest p_target)
	{
		PlayAttackAnimation();
		_animationListener.SetDummyAttackTarget(p_target);
	}

	public void SetAnimationBool(string name, bool value)
	{
		animator.SetBool(name, value);
	}

	public bool GetAnimationBool(string p_name)
	{
		return animator.GetBool(p_name);
	}

	private void UpdateAnimationSpeed()
	{
		if (animator.speed != 0f)
		{
			animator.speed = 1f * progressionSpeedMultiplier;
		}
	}

	public void BerserkedMarker()
	{
		if (mainImg.color == Color.white)
		{
			SetMainVisualTint(Color.red);
			hairImg.color = Color.red;
			knockedOutHairImg.color = Color.red;
		}
	}

	public void UnberserkedMarker()
	{
		if (mainImg.color == Color.red)
		{
			SetMainVisualTint(Color.white);
			Color hairColor = CharacterManager.Instance.GetHairColor(character.hairColorType);
			hairImg.color = hairColor;
			knockedOutHairImg.color = hairColor;
		}
	}

	public void ForceUpdateSortingOrder()
	{
		UpdateSortingOrder();
	}

	private void UpdateSortingOrder()
	{
		int num = 82 + character.id;
		if (character.isDead && !character.IsCurrentlySelected())
		{
			num = 60;
		}
		mainImg.sortingOrder = num;
		mountImg.sortingOrder = num - 1;
		hairImg.sortingOrder = num + 1;
		knockedOutHairImg.sortingOrder = num + 1;
		additionalEffectsImg.sortingOrder = num + 2;
		bloodSplatterEffectRenderer.sortingOrder = 45;
		base.hpBarGO.GetComponent<Canvas>().sortingOrder = 81;
	}

	private new void SetActiveState(bool state)
	{
		base.gameObject.SetActive(state);
	}

	public void SetVisualState(bool state)
	{
		mainImg.gameObject.SetActive(state);
		if ((bool)_nameplate)
		{
			_nameplate.SetVisualsState(state);
		}
		particleEffectParent.gameObject.SetActive(state);
		particleEffectParentAllowRotation.gameObject.SetActive(state);
		if (!state)
		{
			HideHPBar();
		}
	}

	public bool IsShowingVisuals()
	{
		return mainImg.gameObject.activeSelf;
	}

	private void UpdateHairVisuals()
	{
		Character disguisedCharacter = character;
		if (disguisedCharacter.reactionComponent.disguisedCharacter != null)
		{
			disguisedCharacter = disguisedCharacter.reactionComponent.disguisedCharacter;
		}
		Color hairColor = CharacterManager.Instance.GetHairColor(disguisedCharacter.hairColorType);
		Sprite markerHairSprite = CharacterManager.Instance.GetMarkerHairSprite(disguisedCharacter.gender);
		hairImg.sprite = markerHairSprite;
		hairImg.color = hairColor;
		if (disguisedCharacter.characterClass.className == "Shaman")
		{
			hairImg.transform.localPosition = _shamanHairPosition;
		}
		else
		{
			hairImg.transform.localPosition = _normalHairPosition;
		}
		Sprite markerKnockedOutHairSprite = CharacterManager.Instance.GetMarkerKnockedOutHairSprite(disguisedCharacter.gender);
		knockedOutHairImg.sprite = markerKnockedOutHairSprite;
		knockedOutHairImg.color = hairColor;
	}

	private void UpdateMountVisuals()
	{
		Character character = this.character;
		if (character.mountComponent.IsMounting() && character.tileObjectLocation == null)
		{
			UpdateMountAnimatorController();
			ForceUpdateMountMarkerVisualsBasedOnAnimation();
			UpdateMountVisualScaleAndTint();
			UpdateMountPosition(character.mountComponent.mountedCharacter);
			mountImg.gameObject.SetActive(value: true);
		}
		else
		{
			mountImg.gameObject.SetActive(value: false);
		}
	}

	private void UpdateMountPosition(Character mount)
	{
		mountImg.gameObject.transform.localPosition = Vector3.zero;
		mountProjectileParent.transform.localPosition = Vector3.zero;
		if (mount != null && mount is Wyvern)
		{
			mountImg.gameObject.transform.localPosition = new Vector3(0f, -0.3f, 0f);
			mountProjectileParent.gameObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);
		}
	}

	public void ShowAdditionalEffect(Sprite sprite)
	{
		additionalEffectsImg.sprite = sprite;
		additionalEffectsImg.gameObject.SetActive(value: true);
	}

	public void HideAdditionalEffect()
	{
		additionalEffectsImg.gameObject.SetActive(value: false);
	}

	public bool IsShowingAdditionEffectImage(Sprite sprite)
	{
		if (additionalEffectsImg.sprite == sprite)
		{
			return additionalEffectsImg.gameObject.activeSelf;
		}
		return false;
	}

	public void UpdateMarkerVisuals()
	{
		UpdateHairVisuals();
		UpdateMountVisuals();
		SetMainVisualScale(character.visuals.markerVisualScale);
		SetMainVisualTint(character.visuals.markerVisualTint);
	}

	private void UpdateHairState()
	{
		Character disguisedCharacter = character;
		if (disguisedCharacter.reactionComponent.disguisedCharacter != null)
		{
			disguisedCharacter = disguisedCharacter.reactionComponent.disguisedCharacter;
		}
		if (!disguisedCharacter.characterClass.showHair)
		{
			knockedOutHairImg.gameObject.SetActive(value: false);
			hairImg.gameObject.SetActive(value: false);
		}
		else if (disguisedCharacter.visuals != null && disguisedCharacter.visuals.HasHeadHair())
		{
			if (currentAnimation == "Sleep" || currentAnimation == "Dead")
			{
				knockedOutHairImg.gameObject.SetActive(value: true);
				hairImg.gameObject.SetActive(value: false);
			}
			else
			{
				knockedOutHairImg.gameObject.SetActive(value: false);
				hairImg.gameObject.SetActive(value: true);
			}
		}
		else
		{
			hairImg.gameObject.SetActive(value: false);
			knockedOutHairImg.gameObject.SetActive(value: false);
		}
	}

	public void SetMainVisualScale(Vector2 p_scale)
	{
		mainImg.transform.localScale = p_scale;
	}

	public void SetMainVisualTint(Color p_color)
	{
		mainImg.color = p_color;
	}

	public void UpdateMountVisualScaleAndTint()
	{
		if (character != null)
		{
			Character mountedCharacter = character.mountComponent.mountedCharacter;
			if (mountedCharacter != null && mountedCharacter.visuals != null)
			{
				mountImg.transform.localScale = mountedCharacter.visuals.markerVisualScale;
				mountImg.color = mountedCharacter.visuals.markerVisualTint;
			}
		}
	}

	public void UpdatePosition()
	{
		character.SetGridTilePosition(base.transform.localPosition);
		character.SetGridTileWorldPosition(base.transform.position);
		LocationGridTile locationGridTile = previousGridTile;
		if (previousGridTile == character.gridTileLocation || character.gridTileLocation == null)
		{
			return;
		}
		if (character != null)
		{
			previousGridTile = character.gridTileLocation;
			if (_previousAreaLocation == null || _previousAreaLocation != character.areaLocation)
			{
				if (_previousAreaLocation != null)
				{
					_previousAreaLocation.locationCharacterTracker.RemoveCharacterFromLocation(character, _previousAreaLocation);
					Messenger.Broadcast(CharacterSignals.CHARACTER_EXITED_AREA, character, _previousAreaLocation);
					if (character.currentLocationAwareness == _previousAreaLocation.locationAwareness)
					{
						LocationAwarenessUtility.RemoveFromAwarenessList(character);
					}
				}
				_previousAreaLocation = character.areaLocation;
				_previousAreaLocation.locationCharacterTracker.AddCharacterAtLocation(character, _previousAreaLocation);
				character.movementComponent.OnCharacterEntersArea(_previousAreaLocation);
				Messenger.Broadcast(CharacterSignals.CHARACTER_ENTERED_AREA, character, _previousAreaLocation);
				LocationAwarenessUtility.AddToAwarenessList(character, character.gridTileLocation);
			}
		}
		character.gridTileLocation.parentMap.OnCharacterMovedTo(character, character.gridTileLocation, locationGridTile);
	}

	public void OnDeath(LocationGridTile deathTileLocation)
	{
		HideHPBar();
		if (character.minion != null || character.destroyMarkerOnDeath)
		{
			character.DestroyMarker(deathTileLocation);
			return;
		}
		ScheduleExpiry();
		SetCollidersState(state: false);
		pathfindingAI.ClearAllCurrentPathData();
		UpdateAnimation();
		UpdateActionIcon();
		UpdateSortingOrder();
		base.gameObject.transform.SetParent(deathTileLocation.parentMap.objectsParent);
		base.transform.position = deathTileLocation.centeredWorldLocation;
		character.combatComponent.ClearHostilesInRange();
		ClearPOIsInVisionRange();
		character.combatComponent.ClearAvoidInRange();
		visionColliderComponent.OnDeath();
		StartCoroutine(UpdatePositionNextFrame());
	}

	private IEnumerator UpdatePositionNextFrame()
	{
		yield return null;
		UpdatePosition();
	}

	public void OnReturnToLife()
	{
		TryCancelExpiry();
		base.gameObject.SetActive(value: true);
		SetCollidersState(state: true);
		UpdateAnimation();
		UpdateActionIcon();
	}

	public void SetTargetPOI(IPointOfInterest poi)
	{
		targetPOI = poi;
	}

	private bool CanDoStealthCrimeToTarget(Character target, CRIME_TYPE crimeType)
	{
		if (!target.isDead)
		{
			if (target.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(crimeType, character))
			{
				return false;
			}
		}
		else if (character.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(crimeType, target))
		{
			return false;
		}
		return true;
	}

	public bool CanDoStealthCrimeToTarget(IPointOfInterest target, CRIME_TYPE crimeType)
	{
		if (crimeType != CRIME_TYPE.Unset && crimeType != CRIME_TYPE.None)
		{
			if (target is Character target2)
			{
				return CanDoStealthCrimeToTarget(target2, crimeType);
			}
			if (character.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(crimeType))
			{
				return false;
			}
		}
		return true;
	}

	public void PopulateCharactersThatIsNotDeadVillagerAndNotConversedInMinutes(List<Character> characters, int minutes)
	{
		for (int i = 0; i < inVisionCharacters.Count; i++)
		{
			Character character = inVisionCharacters[i];
			if (CharacterManager.Instance.HasCharacterNotConversedInMinutes(character, minutes) && character.isNormalCharacter && !character.isDead)
			{
				characters.Add(character);
			}
		}
	}

	public bool IsStillInRange(IPointOfInterest poi)
	{
		if (!IsPOIInVision(poi))
		{
			return inVisionPOIsButDiffStructure.Contains(poi);
		}
		return true;
	}

	public bool HasEnemyOrRivalInVision()
	{
		for (int i = 0; i < inVisionCharacters.Count; i++)
		{
			Character character = inVisionCharacters[i];
			if (this.character.relationshipContainer.IsEnemiesWith(character))
			{
				return true;
			}
		}
		return false;
	}

	public void OnOtherCharacterDied(Character otherCharacter)
	{
		character.combatComponent.RemoveHostileInRange(otherCharacter);
		character.combatComponent.RemoveAvoidInRange(otherCharacter);
		if (targetPOI == otherCharacter)
		{
			Action action = arrivalAction;
			ClearArrivalAction();
			action?.Invoke();
		}
	}

	private void CreateCollisionTrigger()
	{
		visionTrigger = InnerMapManager.Instance.mapObjectFactory.CreateAndInitializeCharacterVisionTrigger(character);
	}

	public void SetVisionColliderState(bool p_state)
	{
		visionColliderComponent.gameObject.SetActive(p_state);
	}

	public void SetVisionTriggerState(bool p_state)
	{
		if (visionTrigger != null)
		{
			visionTrigger.gameObject.SetActive(p_state);
		}
	}

	public void AddPOIAsInVisionRange(IPointOfInterest poi)
	{
		if (!IsPOIInVision(poi))
		{
			inVisionPOIs.Add(poi);
			AddUnprocessedPOI(poi);
			if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				inVisionCharacters.Add(poi as Character);
			}
			else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
			{
				inVisionTileObjects.Add(poi as TileObject);
			}
			OnAddPOIAsInVisionRange(poi);
			if (character.limiterComponent.canWitness)
			{
				Messenger.Broadcast(CharacterSignals.CHARACTER_SAW, character, poi);
			}
		}
	}

	public bool RemovePOIFromInVisionRange(IPointOfInterest poi)
	{
		if (inVisionPOIs.Remove(poi))
		{
			RemoveUnprocessedPOI(poi);
			if (!inVisionPOIsButDiffStructure.Contains(poi))
			{
				this.character.combatComponent.RemoveHostileInRangeSchedule(poi);
				this.character.combatComponent.RemoveAvoidInRangeSchedule(poi);
			}
			if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				Character character = poi as Character;
				inVisionCharacters.Remove(character);
				character.defaultCharacterTrait.RemoveCharacterThatHasReactedToThis(this.character);
				Messenger.Broadcast(CharacterSignals.CHARACTER_REMOVED_FROM_VISION, this.character, character);
			}
			else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
			{
				inVisionTileObjects.Remove(poi as TileObject);
			}
			return true;
		}
		return false;
	}

	public void AddPOIAsInRangeButDifferentStructure(IPointOfInterest poi)
	{
		if (!inVisionPOIsButDiffStructure.Contains(poi))
		{
			inVisionPOIsButDiffStructure.Add(poi);
		}
	}

	public bool RemovePOIAsInRangeButDifferentStructure(IPointOfInterest poi)
	{
		return inVisionPOIsButDiffStructure.Remove(poi);
	}

	public void ClearPOIsInVisionRange()
	{
		inVisionPOIs.Clear();
		inVisionCharacters.Clear();
		inVisionTileObjects.Clear();
		ClearUnprocessedPOI();
		ClearUnprocessedActions();
	}

	private void OnAddPOIAsInVisionRange(IPointOfInterest poi)
	{
		if (character.currentActionNode != null && !character.currentActionNode.hasBeenReset && character.currentActionNode.target == poi && character.currentActionNode.action.IsInvalidOnVision(character.currentActionNode, out var reason))
		{
			if (!string.IsNullOrEmpty(reason))
			{
				ActualGoapNode currentActionNode = character.currentActionNode;
				GoapActionInvalidity invalidity = currentActionNode.invalidity;
				invalidity.isInvalid = true;
				invalidity.stateName = "Target Missing";
				invalidity.reason = reason;
				currentActionNode.action.LogActionInvalid(invalidity, character.currentActionNode, isInvalidStealth: false);
			}
			character.currentActionNode.associatedJob?.CancelJob();
		}
		if (character.currentActionNode != null && !character.currentActionNode.hasBeenReset && character.currentActionNode.action.actionLocationType == ACTION_LOCATION_TYPE.TARGET_IN_VISION && character.currentActionNode.poiTarget == poi)
		{
			pathfindingAI.ClearAllCurrentPathData();
			character.PerformGoapAction();
		}
	}

	public void AddUnprocessedPOI(IPointOfInterest poi, bool reactToInterruptOnly = false)
	{
		if (reactToInterruptOnly && !unprocessedVisionPOIs.Contains(poi) && !unprocessedVisionPOIInterruptsOnly.Contains(poi))
		{
			unprocessedVisionPOIInterruptsOnly.Add(poi);
		}
		if (!unprocessedVisionPOIs.Contains(poi))
		{
			unprocessedVisionPOIs.Add(poi);
			poi.OnAddedAsUnprocessedPOI(character);
		}
	}

	public void AddUnprocessedAction(ActualGoapNode action)
	{
		if (!unprocessedActionsOnly.Contains(action))
		{
			unprocessedActionsOnly.Add(action);
			action.IncreaseReactionCounter();
		}
	}

	public void RemoveUnprocessedAction(ActualGoapNode action)
	{
		if (unprocessedActionsOnly.Remove(action))
		{
			action.DecreaseReactionCounter();
		}
	}

	public void RemoveUnprocessedPOI(IPointOfInterest poi)
	{
		unprocessedVisionPOIs.Remove(poi);
		unprocessedVisionPOIInterruptsOnly.Remove(poi);
	}

	public void ClearUnprocessedPOI()
	{
		unprocessedVisionPOIs.Clear();
		unprocessedVisionPOIInterruptsOnly.Clear();
	}

	public void ClearUnprocessedActions()
	{
		for (int i = 0; i < unprocessedActionsOnly.Count; i++)
		{
			unprocessedActionsOnly[i].DecreaseReactionCounter();
		}
		unprocessedActionsOnly.Clear();
	}

	public bool HasUnprocessedPOI(IPointOfInterest poi)
	{
		return unprocessedVisionPOIs.Contains(poi);
	}

	private void ReprocessPOI(IPointOfInterest poi)
	{
		if (!HasUnprocessedPOI(poi) && IsPOIInVision(poi))
		{
			AddUnprocessedPOI(poi);
		}
	}

	public void ProcessAllUnprocessedVisionPOIs()
	{
		if (this.character == null)
		{
			return;
		}
		if (unprocessedVisionPOIs.Count > 0)
		{
			if (!this.character.isDead && this.character.reactionComponent.disguisedCharacter == null)
			{
				for (int i = 0; i < unprocessedVisionPOIs.Count; i++)
				{
					IPointOfInterest pointOfInterest = unprocessedVisionPOIs[i];
					if (pointOfInterest.mapObjectVisual == null || pointOfInterest.isHidden)
					{
						continue;
					}
					if (pointOfInterest is Character character && character.carryComponent.prevCarriedBy != null && character.carryComponent.prevCarriedBy == this.character)
					{
						character.carryComponent.SetPrevCarriedBy(null);
					}
					else if (visionColliderComponent.IsTheSameStructureOrSameOpenSpaceWithPOI(pointOfInterest) || IsCharacterInLineOfSightWith(pointOfInterest))
					{
						bool reactToActionOnly = false;
						if (unprocessedVisionPOIInterruptsOnly.Count > 0)
						{
							reactToActionOnly = unprocessedVisionPOIInterruptsOnly.Contains(pointOfInterest);
						}
						this.character.ThisCharacterSaw(pointOfInterest, reactToActionOnly);
					}
				}
			}
			ClearUnprocessedPOI();
		}
		if (unprocessedActionsOnly.Count > 0)
		{
			if (!this.character.isDead)
			{
				for (int j = 0; j < unprocessedActionsOnly.Count; j++)
				{
					ActualGoapNode actualGoapNode = unprocessedActionsOnly[j];
					Character actor = actualGoapNode.actor;
					if (visionColliderComponent.IsTheSameStructureOrSameOpenSpaceWithPOI(actor) || IsCharacterInLineOfSightWith(actor))
					{
						this.character.ThisCharacterSawAction(actualGoapNode);
					}
				}
			}
			ClearUnprocessedActions();
		}
		this.character.defaultCharacterTrait.SetHasSeenFire(state: false);
		this.character.defaultCharacterTrait.SetHasSeenWet(state: false);
		this.character.defaultCharacterTrait.SetHasSeenPoisoned(state: false);
		this.character.combatComponent.CheckCombatPerTickEnded();
	}

	public bool IsPOIInVision(IPointOfInterest poi)
	{
		if (character == null)
		{
			return false;
		}
		return poi.CanBeSeenBy(character);
	}

	public void OnStartFlee()
	{
		if (!hasFleePath)
		{
			pathfindingAI.ClearAllCurrentPathData();
		}
		List<Area> areas = PlayerManager.Instance.player.playerSettlement.areas;
		if (avoidThisPositions == null)
		{
			avoidThisPositions = new List<Vector3>();
		}
		else
		{
			avoidThisPositions.Clear();
		}
		if (character.combatComponent.avoidInRange.Count > 0)
		{
			for (int i = 0; i < character.combatComponent.avoidInRange.Count; i++)
			{
				IPointOfInterest pointOfInterest = character.combatComponent.avoidInRange[i];
				if (IsStillInRange(pointOfInterest))
				{
					avoidThisPositions.Add(pointOfInterest.gridTileLocation.worldLocation);
				}
			}
		}
		if (character.isNormalCharacter && areas.Count > 0)
		{
			for (int j = 0; j < areas.Count; j++)
			{
				Area area = areas[j];
				if (area.region == character.currentRegion && character.gridTileLocation != null && character.areaLocation == area)
				{
					avoidThisPositions.Add(area.gridTileComponent.centerGridTile.worldLocation);
				}
			}
		}
		ReconstructFleePath();
	}

	public void OnStartFleeToPartyMate()
	{
		Character character = null;
		Character character2 = null;
		LocationGridTile gridTileLocation = this.character.gridTileLocation;
		if (this.character.partyComponent.isMemberThatJoinedQuest && gridTileLocation != null)
		{
			for (int i = 0; i < this.character.partyComponent.currentParty.membersThatJoinedQuest.Count; i++)
			{
				Character character3 = this.character.partyComponent.currentParty.membersThatJoinedQuest[i];
				LocationGridTile gridTileLocation2 = character3.gridTileLocation;
				if (this.character != character3 && character3.limiterComponent.canPerform && character3.limiterComponent.canMove && character3.hasMarker && !character3.isBeingSeized && character3.carryComponent.IsNotBeingCarried() && gridTileLocation2 != null && this.character.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation2) && gridTileLocation.GetDistanceTo(gridTileLocation2) <= 20f)
				{
					if (character3.combatComponent.combatBehaviourParent.IsCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR.Tank))
					{
						character = character3;
						break;
					}
					if (character2 == null)
					{
						character2 = character3;
					}
				}
			}
		}
		if (character != null || character2 != null)
		{
			if (!hasFleePath)
			{
				pathfindingAI.ClearAllCurrentPathData();
			}
			SetHasFleePath(state: true);
			pathfindingAI.canSearch = false;
			if (character != null)
			{
				GoTo(character, OnFinishedTraversingFleePath);
			}
			else if (character2 != null)
			{
				GoTo(character2, OnFinishedTraversingFleePath);
			}
		}
		else
		{
			OnStartFlee();
		}
	}

	public void OnStartFleeToHome()
	{
		if (!hasFleePath)
		{
			pathfindingAI.ClearAllCurrentPathData();
		}
		LocationGridTile chosenTile = null;
		if (character.homeStructure != null)
		{
			chosenTile = character.homeStructure.GetRandomTile();
		}
		else if (character.HasTerritory())
		{
			chosenTile = character.GetRandomLocationGridTileWithPath();
		}
		if (chosenTile != null)
		{
			if (character.currentRegion != chosenTile.structure.region)
			{
				if (!character.movementComponent.MoveToAnotherRegion(chosenTile.structure.region, delegate
				{
					GoTo(chosenTile, OnFinishedTraversingFleePath);
				}))
				{
					OnStartFlee();
				}
			}
			else
			{
				SetHasFleePath(state: true);
				pathfindingAI.canSearch = false;
				GoTo(chosenTile, OnFinishedTraversingFleePath);
			}
		}
		else
		{
			OnStartFlee();
		}
	}

	public void OnStartFleeToOutside()
	{
		if (!hasFleePath)
		{
			pathfindingAI.ClearAllCurrentPathData();
		}
		LocationGridTile locationGridTile = null;
		if (character.currentStructure != null && character.currentStructure.structureType.IsSpecialStructure())
		{
			Area areaLocation = character.areaLocation;
			areasInWildernessForFlee.Clear();
			for (int i = 0; i < areaLocation.neighbourComponent.neighbours.Count; i++)
			{
				Area area = areaLocation.neighbourComponent.neighbours[i];
				LocationGridTile centerGridTile = area.gridTileComponent.centerGridTile;
				if (character.movementComponent.HasPathTo(centerGridTile) && centerGridTile.structure.structureType == STRUCTURE_TYPE.WILDERNESS)
				{
					BaseSettlement settlement = null;
					character.gridTileLocation?.IsPartOfSettlement(out settlement);
					if (settlement == null || !area.HasSettlementOnArea() || !area.HasSettlementOnArea(settlement))
					{
						areasInWildernessForFlee.Add(area);
					}
				}
			}
			if (areasInWildernessForFlee.Count > 0)
			{
				locationGridTile = CollectionUtilities.GetRandomElement(areasInWildernessForFlee).gridTileComponent.centerGridTile;
			}
		}
		if (locationGridTile != null)
		{
			SetHasFleePath(state: true);
			pathfindingAI.canSearch = false;
			GoTo(locationGridTile, OnFinishedTraversingFleePath);
		}
		else
		{
			OnStartFlee();
		}
	}

	public void ReconstructFleePath()
	{
		if (avoidThisPositions.Count > 0)
		{
			FleeMultiplePath fleeMultiplePath = FleeMultiplePath.Construct(base.transform.position, avoidThisPositions, CombatManager.Instance.searchLength);
			fleeMultiplePath.aimStrength = CombatManager.Instance.aimStrength;
			fleeMultiplePath.spread = CombatManager.Instance.spread;
			seeker.StartPath(fleeMultiplePath, OnFleePathComputed);
		}
		else
		{
			OnFinishedTraversingFleePath();
		}
	}

	public void OnFleePathComputed(Path path)
	{
		if (character != null && character.limiterComponent.canPerform && character.limiterComponent.canMove)
		{
			SetHasFleePath(state: true);
			pathfindingAI.canSearch = false;
			arrivalAction = OnFinishedTraversingFleePath;
			StartMovement();
		}
	}

	public void OnFinishedTraversingFleePath()
	{
		SetHasFleePath(state: false);
		if (character.combatComponent.isInCombat)
		{
			(character.stateComponent.currentState as CombatState).FinishedTravellingFleePath();
		}
	}

	public void SetHasFleePath(bool state)
	{
		hasFleePath = state;
		UpdateActionIcon();
	}

	public void AddAvoidPositions(Vector3 avoid)
	{
		if (avoidThisPositions == null)
		{
			avoidThisPositions = new List<Vector3>();
		}
		avoidThisPositions.Add(avoid);
		ReconstructFleePath();
	}

	private void UpdateAttackSpeedMeter()
	{
		aspeedFill.fillAmount = attackSpeedMeter / (float)character.combatComponent.attackSpeed;
	}

	public void ResetAttackSpeed()
	{
		attackSpeedMeter = 0f;
		if (base.hasHPBarGO && base.hpBarGO.activeSelf)
		{
			UpdateAttackSpeedMeter();
		}
	}

	public bool CanAttackByAttackSpeed()
	{
		return attackSpeedMeter >= (float)character.combatComponent.attackSpeed;
	}

	public bool IsCharacterInLineOfSightWith(IPointOfInterest target, float rayDistance = 5f)
	{
		if (character == null)
		{
			return false;
		}
		if (target is GenericTileObject && character.gridTileLocation == target.gridTileLocation)
		{
			return true;
		}
		Vector3 position = base.transform.position;
		return GameUtilities.IsInLineOfSight(target, position, rayDistance, GameUtilities.Line_Of_Sight_Layer_Mask, _lineOfSightHitObjects);
	}

	private void SetCollidersState(bool state)
	{
		visionCollider.enabled = state;
	}

	public void SetVisionColliderSize(int size)
	{
		if (_currentColliderSize != size)
		{
			_currentColliderSize = size;
			visionCollider.size = new Vector2(size, size);
		}
	}

	public override void UpdateTileObjectVisual(Character obj)
	{
	}

	public override void SetVisualAlpha(float alpha)
	{
		base.SetVisualAlpha(alpha);
		Color color = hairImg.color;
		color.a = alpha;
		hairImg.color = color;
		color = knockedOutHairImg.color;
		color.a = alpha;
		knockedOutHairImg.color = color;
	}

	public void OnSeize()
	{
		Character character = this.character;
		bool activeSelf = additionalEffectsImg.gameObject.activeSelf;
		Reset();
		if (activeSelf)
		{
			ShowAdditionalEffect(additionalEffectsImg.sprite);
		}
		this.character = character;
		clickCollider.enabled = false;
	}

	public void OnUnseize()
	{
		clickCollider.enabled = true;
	}

	public void TryCancelExpiry()
	{
		if (!string.IsNullOrEmpty(_destroySchedule))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_destroySchedule);
			_destroySchedule = string.Empty;
		}
	}

	public void ScheduleExpiry()
	{
		if (string.IsNullOrEmpty(_destroySchedule))
		{
			_destroyDate = GameManager.Instance.Today();
			_destroyDate.AddDays(3);
			_destroySchedule = SchedulingManager.Instance.AddEntry(_destroyDate, TryExpire, character);
		}
	}

	private void ScheduleExpiry(GameDate gameDate)
	{
		if (string.IsNullOrEmpty(_destroySchedule))
		{
			_destroyDate = gameDate;
			_destroySchedule = SchedulingManager.Instance.AddEntry(_destroyDate, TryExpire, character);
		}
	}

	public void TryExpire()
	{
		bool flag = character.numOfNonSecretActionsBeingPerformedOnThis <= 0;
		if (character.isBeingCarriedBy != null || character.isBeingSeized || character.traitContainer.HasTrait("Mummified") || !character.isDead || character.grave != null)
		{
			flag = false;
		}
		if (flag)
		{
			Expire();
			return;
		}
		_destroyDate = GameManager.Instance.Today();
		if (!character.isDead)
		{
			_destroyDate.AddDays(3);
		}
		else
		{
			_destroyDate.AddTicks(20);
		}
		_destroySchedule = SchedulingManager.Instance.AddEntry(_destroyDate, TryExpire, character);
	}

	private void Expire()
	{
		character.ForceCancelAllJobsTargetingThisCharacter(shouldDoAfterEffect: false);
		Messenger.Broadcast(CharacterSignals.CHARACTER_MARKER_EXPIRED, character);
		character.DestroyMarker();
	}

	private void ShowThoughtsAndNameplate()
	{
		if ((bool)_nameplate)
		{
			_nameplate.ShowThoughts();
		}
	}

	private void HideThoughtsAndNameplate()
	{
		if ((bool)_nameplate)
		{
			_nameplate.HideThoughts();
		}
	}

	public void UpdateNameplateElementsState()
	{
		if ((bool)_nameplate)
		{
			_nameplate.UpdateElementsStateBasedOnActiveCharacter();
		}
	}

	public void DoStrollMovement()
	{
		StopMovement();
		pathfindingAI.ClearAllCurrentPathData();
		AstarPath.StartPath(ConstantPath.Construct(base.transform.position, 10000, OnStrollPathComputed));
		UpdateAnimation();
	}

	public void OnStrollPathComputed(Path path)
	{
		if (character != null && path is ConstantPath constantPath && character.stateComponent.currentState is StrollOutsideState strollOutsideState)
		{
			if (character.jobQueue.jobsInQueue.Count > 1 && character.jobQueue.jobsInQueue[0] != strollOutsideState.job)
			{
				character.stateComponent.ExitCurrentState();
			}
			else if (constantPath.allNodes != null && constantPath.allNodes.Count > 0)
			{
				GoTo(PathUtilities.GetPointsOnNodes(constantPath.allNodes, 1, 5f).Last(), strollOutsideState.StartStrollMovement);
			}
			else
			{
				character.stateComponent.ExitCurrentState();
			}
		}
	}

	public void UpdateTraversableTags()
	{
		seeker.traversableTags = character.movementComponent.traversableTags;
	}

	public void UpdateTagPenalties()
	{
		seeker.tagPenalties = character.movementComponent.tagPenalties;
	}

	public void SetLightState(bool p_state)
	{
		markerLight.gameObject.SetActive(p_state);
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		unprocessedVisionPOIs.Remove(p_character);
		inVisionPOIs.Remove(p_character);
		inVisionPOIsButDiffStructure.Remove(p_character);
		unprocessedVisionPOIInterruptsOnly.Remove(p_character);
		inVisionCharacters.Remove(p_character);
	}
}
