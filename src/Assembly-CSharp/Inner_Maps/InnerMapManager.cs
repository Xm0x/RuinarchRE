using System;
using System.Collections.Generic;
using System.Linq;
using Cellular_Automata;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using Necromancy.UI;
using Pathfinding;
using Ruinarch;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UtilityScripts;

namespace Inner_Maps;

public class InnerMapManager : BaseMonoBehaviour
{
	public static InnerMapManager Instance;

	public static readonly Vector2Int BuildingSpotSize = new Vector2Int(7, 7);

	public static readonly Vector2Int AreaLocationGridTileSize = new Vector2Int(14, 14);

	public const int DefaultCharacterSortingOrder = 82;

	public const int DeadCharacterSortingOrder = 60;

	public const int GroundTilemapSortingOrder = 10;

	public const int ElevationTilemapSortingOrder = 30;

	public const int DetailsTilemapSortingOrder = 40;

	public const int SelectedSortingOrder = 900;

	public const int BlueprintGroundTilemapSortingOrder = 50;

	public const int Starting_Tag_Index = 20;

	public uint currentTagIndex = 20u;

	public const int All_Tags = -1;

	public const int Ground_Tag = 1;

	public const int Obstacle_Tag = 2;

	public const int Demonic_Faction = 3;

	public const int Demonic_Faction_Doors = 4;

	public const int Undead_Faction = 5;

	public const int Undead_Faction_Doors = 6;

	public const int Ratmen_Faction = 7;

	public const int Ratmen_Faction_Doors = 8;

	public const int Bandit_Faction = 9;

	public const int Bandit_Faction_Doors = 10;

	public const int Demon_Cult_Faction = 11;

	public const int Demon_Cult_Faction_Doors = 12;

	public const int Divine_Cult_Faction = 13;

	public const int Divine_Cult_Faction_Doors = 14;

	public const int Nature_Cult_Faction = 15;

	public const int Nature_Cult_Faction_Doors = 16;

	public const int Roads = 17;

	public const int Caves = 18;

	public const int Special_Structures = 19;

	private Vector3 _nextMapPos = Vector3.zero;

	public GameObject characterCollisionTriggerPrefab;

	public GameObject dragonCollisionTriggerPrefab;

	[Header("Pathfinding")]
	[SerializeField]
	private AstarPath pathfinder;

	[Header("Tile Object")]
	[SerializeField]
	private TileObjectSlotDictionary tileObjectSlotSettings;

	public GameObject tileObjectSlotsParentPrefab;

	public GameObject tileObjectSlotPrefab;

	[SerializeField]
	private SpriteRenderer _mapObjectHover;

	[Header("Tilemap Assets")]
	public InnerMapAssetManager assetManager;

	[SerializeField]
	private WallResourceAssetDictionary wallResourceAssets;

	[Header("Effects")]
	[SerializeField]
	private GameObject pfAreaMapTextPopup;

	[FormerlySerializedAs("areaMapObjectFactory")]
	public MapVisualFactory mapObjectFactory;

	private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

	public readonly TILE_OBJECT_TYPE[] foodPileTypes = new TILE_OBJECT_TYPE[11]
	{
		TILE_OBJECT_TYPE.ANIMAL_MEAT,
		TILE_OBJECT_TYPE.ELF_MEAT,
		TILE_OBJECT_TYPE.HUMAN_MEAT,
		TILE_OBJECT_TYPE.RAT_MEAT,
		TILE_OBJECT_TYPE.CORN,
		TILE_OBJECT_TYPE.FISH_PILE,
		TILE_OBJECT_TYPE.HYPNO_HERB,
		TILE_OBJECT_TYPE.ICEBERRY,
		TILE_OBJECT_TYPE.PINEAPPLE,
		TILE_OBJECT_TYPE.POTATO,
		TILE_OBJECT_TYPE.VEGETABLES
	};

	private IPointOfInterest _closestHoveredPOI;

	private LocationGridTile _currentlyHoveredTile;

	private LocationGridTile lastClickedTile;

	private Dictionary<TILE_OBJECT_TYPE, TileObjectScriptableObject> _tileObjectScriptableObjects;

	private readonly float[] _monsterLairSeeds = new float[4] { 4f, 22f, 69f, 96f };

	public InnerTileMap currentlyShowingMap { get; private set; }

	public Region currentlyShowingLocation { get; private set; }

	public List<InnerTileMap> innerMaps { get; private set; }

	public IPointOfInterest currentlyHoveredPoi { get; private set; }

	public List<LocationStructure> worldKnownDemonicStructures { get; private set; }

	public List<MonsterSpawner> currentMonsterSpawners { get; private set; }

	public GraphMask mainGraphMask { get; private set; }

	public List<PathfindingTagPair> unusedPathfindingTags { get; private set; }

	public bool isAnInnerMapShowing => currentlyShowingMap != null;

	private void Awake()
	{
		Instance = this;
		mainGraphMask = 0;
		_tileObjectScriptableObjects = new Dictionary<TILE_OBJECT_TYPE, TileObjectScriptableObject>(50);
		currentMonsterSpawners = new List<MonsterSpawner>();
	}

	public void LateUpdate()
	{
		if (!GameManager.Instance.gameHasStarted)
		{
			return;
		}
		if (currentlyShowingMap == null)
		{
			ExitHoverOnClosestHoveredPOI();
			return;
		}
		LocationGridTile tileFromMousePosition = GetTileFromMousePosition();
		if (tileFromMousePosition != null)
		{
			IPointOfInterest pointOfInterest = null;
			if (tileFromMousePosition.structure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS)
			{
				List<TileObject> tileObjectsOfType = tileFromMousePosition.structure.GetTileObjectsOfType(TILE_OBJECT_TYPE.TORTURE_CHAMBERS_TILE_OBJECT);
				if (tileObjectsOfType != null && tileObjectsOfType.Count > 0)
				{
					pointOfInterest = tileObjectsOfType[0];
				}
			}
			else if (tileFromMousePosition.structure.structureType == STRUCTURE_TYPE.KENNEL)
			{
				List<TileObject> tileObjectsOfType2 = tileFromMousePosition.structure.GetTileObjectsOfType(TILE_OBJECT_TYPE.KENNEL_TILE_OBJECT);
				if (tileObjectsOfType2 != null && tileObjectsOfType2.Count > 0)
				{
					pointOfInterest = tileObjectsOfType2[0];
				}
			}
			if (pointOfInterest == null)
			{
				Vector3 p_originDistance = currentlyShowingMap.worldUiCanvas.worldCamera.ScreenToWorldPoint(InputManager.Instance.mousePosition);
				pointOfInterest = GetPOIOnTileClosestToDistance(p_originDistance, tileFromMousePosition);
			}
			if (_closestHoveredPOI != pointOfInterest && !UIManager.Instance.IsMouseOnUI())
			{
				ExitHoverOnClosestHoveredPOI();
				_closestHoveredPOI = pointOfInterest;
				if (_closestHoveredPOI != null)
				{
					_closestHoveredPOI.mapObjectVisual.ExecuteHoverEnterAction();
				}
			}
		}
		else
		{
			ExitHoverOnClosestHoveredPOI();
		}
		if (GameManager.showAllTilesTooltip && tileFromMousePosition != null && tileFromMousePosition.tileObjectComponent.objHere == null)
		{
			ShowTileData(tileFromMousePosition);
		}
	}

	private void ExitHoverOnClosestHoveredPOI()
	{
		if (_closestHoveredPOI != null)
		{
			_closestHoveredPOI.mapObjectVisual?.ExecuteHoverExitAction();
			_closestHoveredPOI = null;
		}
	}

	protected override void OnDestroy()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (innerMaps != null)
		{
			for (int i = 0; i < innerMaps.Count; i++)
			{
				InnerTileMap innerTileMap = innerMaps[i];
				pathfinder.data.RemoveGraph(innerTileMap.pathfindingGraph);
				innerTileMap?.CleanUp();
			}
			innerMaps?.Clear();
		}
		UnityEngine.Object.Destroy(pathfinder);
		tileObjectSlotSettings?.Clear();
		wallResourceAssets?.Clear();
		_tileObjectScriptableObjects.Clear();
		_tileObjectScriptableObjects = null;
		base.OnDestroy();
		Instance = null;
	}

	private void OnReceivePlayerInputAction(SHORTCUT_ACTION p_action)
	{
		switch (p_action)
		{
		case SHORTCUT_ACTION.Left_Click:
			OnClickMapObject();
			break;
		case SHORTCUT_ACTION.Right_Click:
			if (InputManager.Instance.isUsingGamepad)
			{
				OnRightClickGamepad();
			}
			else
			{
				OnRightClick();
			}
			break;
		case SHORTCUT_ACTION.Middle_Click:
			OnMiddleClick();
			break;
		}
	}

	public void OnShiftRightClickOnTile(LocationGridTile p_tile)
	{
		p_tile?.mouseEventsComponent.OnTileEvents();
	}

	private void OnRightClick()
	{
		if (UIManager.Instance.IsMouseOnUI() || (object)currentlyShowingMap == null || !GameManager.Instance.gameHasStarted)
		{
			return;
		}
		LocationGridTile tileFromMousePosition = GetTileFromMousePosition();
		if (tileFromMousePosition != null)
		{
			IPointOfInterest currentlySelectedPOI = UIManager.Instance.GetCurrentlySelectedPOI();
			if (currentlySelectedPOI != null && IsSelectableOnTile(tileFromMousePosition, currentlySelectedPOI))
			{
				currentlySelectedPOI.RightSelectAction();
			}
			else
			{
				GetFirstSelectableOnTile(tileFromMousePosition)?.RightSelectAction();
			}
		}
	}

	private void OnRightClickGamepad()
	{
		if (UIManager.Instance.IsMouseOnUI() || (object)currentlyShowingMap == null || !GameManager.Instance.gameHasStarted)
		{
			return;
		}
		LocationGridTile tileFromMousePosition = GetTileFromMousePosition();
		if (tileFromMousePosition == null)
		{
			return;
		}
		IPointOfInterest currentlySelectedPOI = UIManager.Instance.GetCurrentlySelectedPOI();
		if (currentlySelectedPOI != null && currentlySelectedPOI.mapObjectVisual != null && IsSelectableOnTile(tileFromMousePosition, currentlySelectedPOI))
		{
			currentlySelectedPOI.RightSelectAction();
			return;
		}
		ISelectable firstSelectableOnTile = GetFirstSelectableOnTile(tileFromMousePosition);
		if (firstSelectableOnTile != null)
		{
			if (firstSelectableOnTile is IPointOfInterest pointOfInterest)
			{
				if (pointOfInterest.mapObjectVisual != null)
				{
					pointOfInterest.RightSelectAction();
				}
			}
			else
			{
				firstSelectableOnTile.RightSelectAction();
			}
		}
		else if (currentlySelectedPOI?.mapObjectVisual != null)
		{
			currentlySelectedPOI?.RightSelectAction();
		}
	}

	private void OnMiddleClick()
	{
		if (!UIManager.Instance.IsMouseOnUI() && (object)currentlyShowingMap != null)
		{
			LocationGridTile tileFromMousePosition = GetTileFromMousePosition();
			GetFirstSelectableOnTile(tileFromMousePosition)?.MiddleSelectAction();
		}
	}

	private void OnClickMapObject()
	{
		if (UIManager.Instance.IsMouseOnUI() || (object)currentlyShowingMap == null || !GameManager.Instance.gameHasStarted || PlayerManager.Instance.player == null || PlayerManager.Instance.player.IsPerformingPlayerAction())
		{
			return;
		}
		LocationGridTile tileFromMousePosition = GetTileFromMousePosition();
		List<ISelectable> list = RuinarchListPool<ISelectable>.Claim();
		if (tileFromMousePosition != null && TryPopulateSelectablesOnTile(tileFromMousePosition, list))
		{
			if (list.Count > 0)
			{
				ISelectable selectable = null;
				if (lastClickedTile != tileFromMousePosition)
				{
					selectable = list[0];
				}
				else
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].IsCurrentlySelected())
						{
							selectable = CollectionUtilities.GetNextElementCyclic(list, i);
							break;
						}
					}
				}
				if (selectable == null)
				{
					selectable = list[0];
				}
				InputManager.Instance.Select(selectable);
			}
			lastClickedTile = tileFromMousePosition;
		}
		RuinarchListPool<ISelectable>.Release(list);
	}

	private bool TryPopulateSelectablesOnTile(LocationGridTile tile, List<ISelectable> selectables)
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = InputManager.Instance.mousePosition
		};
		raycastResults.Clear();
		EventSystem.current.RaycastAll(eventData, raycastResults);
		if (raycastResults.Count > 0)
		{
			foreach (RaycastResult raycastResult in raycastResults)
			{
				if (raycastResult.gameObject.CompareTag("Character Marker"))
				{
					BaseMapObjectVisual component = raycastResult.gameObject.GetComponent<BaseMapObjectVisual>();
					if (!component.IsInvisibleToPlayer() && component.selectable != null && component.selectable.CanBeSelected())
					{
						selectables.Add(component.selectable);
					}
				}
			}
		}
		TileObject objHere = tile.tileObjectComponent.objHere;
		if (objHere != null && objHere.CanBeSelected() && objHere.mapObjectVisual != null && !objHere.mapObjectVisual.IsInvisibleToPlayer())
		{
			selectables.Add(objHere);
		}
		objHere = tile.tileObjectComponent.hiddenObjHere;
		if (objHere != null && objHere.CanBeSelected() && objHere.mapObjectVisual != null && !objHere.mapObjectVisual.IsInvisibleToPlayer())
		{
			selectables.Add(objHere);
		}
		if (tile.hasBlueprint)
		{
			LocationGridTile tileFromMapCoordinates = GridMap.Instance.mainRegion.innerMap.GetTileFromMapCoordinates(tile.blueprintTileReferenceXPos, tile.blueprintTileReferenceYPos);
			if (tileFromMapCoordinates != null)
			{
				LocationStructureObject blueprintOnTile = tileFromMapCoordinates.tileObjectComponent.genericTileObject.blueprintOnTile;
				if (blueprintOnTile != null && blueprintOnTile.currentVisualMode == LocationStructureObject.Structure_Visual_Mode.Demonic_Structure_Blueprint)
				{
					selectables.Add(blueprintOnTile);
				}
			}
		}
		if (tile.structure != null)
		{
			if (tile.structure.IsTilePartOfARoom(tile, out var room) && room.CanBeSelected() && !selectables.Contains(tile.structure))
			{
				selectables.Add(room);
			}
			if (tile.structure.structureType.IsPlayerStructure())
			{
				if (!selectables.Contains(tile.structure))
				{
					selectables.Add(tile.structure);
				}
			}
			else if (tile.structure is ManMadeStructure { structureObj: not null } || (tile.structure is DemonicStructure { structureObj: not null } && !(tile.structure is CityCenter)))
			{
				if (!selectables.Contains(tile.structure))
				{
					selectables.Add(tile.structure);
				}
			}
			else if ((tile.structure.structureType.IsSpecialStructure() || tile.structure.structureType == STRUCTURE_TYPE.BANDIT_CAMP) && !selectables.Contains(tile.structure))
			{
				selectables.Add(tile.structure);
			}
		}
		return true;
	}

	private bool IsSelectableOnTile(LocationGridTile tile, ISelectable p_selectable)
	{
		for (int i = 0; i < tile.charactersHere.Count; i++)
		{
			Character character = tile.charactersHere[i];
			if (character.CanBeSelected() && character.hasMarker && !character.marker.IsInvisibleToPlayer() && p_selectable == character)
			{
				return true;
			}
		}
		TileObject objHere = tile.tileObjectComponent.objHere;
		if (objHere != null && objHere.CanBeSelected() && objHere.mapObjectVisual != null && !objHere.mapObjectVisual.IsInvisibleToPlayer() && p_selectable == objHere)
		{
			return true;
		}
		objHere = tile.tileObjectComponent.hiddenObjHere;
		if (objHere != null && objHere.CanBeSelected() && objHere.mapObjectVisual != null && !objHere.mapObjectVisual.IsInvisibleToPlayer() && p_selectable == objHere)
		{
			return true;
		}
		if (tile.hasBlueprint)
		{
			LocationGridTile tileFromMapCoordinates = GridMap.Instance.mainRegion.innerMap.GetTileFromMapCoordinates(tile.blueprintTileReferenceXPos, tile.blueprintTileReferenceYPos);
			if (tileFromMapCoordinates != null)
			{
				LocationStructureObject blueprintOnTile = tileFromMapCoordinates.tileObjectComponent.genericTileObject.blueprintOnTile;
				if (blueprintOnTile != null && blueprintOnTile.currentVisualMode == LocationStructureObject.Structure_Visual_Mode.Demonic_Structure_Blueprint)
				{
					return true;
				}
			}
		}
		if (tile.structure != null)
		{
			if (tile.structure.IsTilePartOfARoom(tile, out var room) && room.CanBeSelected() && p_selectable == room)
			{
				return true;
			}
			if (tile.structure.structureType.IsPlayerStructure())
			{
				if (tile.tileState == LocationGridTile.Tile_State.Occupied && p_selectable == tile.structure)
				{
					return true;
				}
			}
			else if (tile.structure is ManMadeStructure { structureObj: not null } || (tile.structure is DemonicStructure { structureObj: not null } && !(tile.structure is CityCenter)))
			{
				if (p_selectable == tile.structure)
				{
					return true;
				}
			}
			else if ((tile.structure.structureType.IsSpecialStructure() || tile.structure.structureType == STRUCTURE_TYPE.BANDIT_CAMP) && p_selectable == tile.structure)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasSelectablesOnTile(LocationGridTile tile)
	{
		return GetFirstSelectableOnTile(tile) != null;
	}

	public ISelectable GetFirstSelectableOnTile(LocationGridTile tile)
	{
		if (tile == null)
		{
			return null;
		}
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = InputManager.Instance.mousePosition
		};
		raycastResults.Clear();
		EventSystem.current.RaycastAll(eventData, raycastResults);
		if (raycastResults.Count > 0)
		{
			foreach (RaycastResult raycastResult in raycastResults)
			{
				if (raycastResult.gameObject.CompareTag("Character Marker"))
				{
					BaseMapObjectVisual component = raycastResult.gameObject.GetComponent<BaseMapObjectVisual>();
					if (!component.IsInvisibleToPlayer() && component.selectable != null && component.selectable.CanBeSelected())
					{
						return component.selectable;
					}
				}
			}
		}
		TileObject objHere = tile.tileObjectComponent.objHere;
		if (objHere != null && objHere.CanBeSelected() && objHere.mapObjectVisual != null && !objHere.mapObjectVisual.IsInvisibleToPlayer())
		{
			return objHere;
		}
		objHere = tile.tileObjectComponent.hiddenObjHere;
		if (objHere != null && objHere.CanBeSelected() && objHere.mapObjectVisual != null && !objHere.mapObjectVisual.IsInvisibleToPlayer())
		{
			return objHere;
		}
		if (tile.hasBlueprint)
		{
			LocationGridTile tileFromMapCoordinates = GridMap.Instance.mainRegion.innerMap.GetTileFromMapCoordinates(tile.blueprintTileReferenceXPos, tile.blueprintTileReferenceYPos);
			if (tileFromMapCoordinates != null)
			{
				LocationStructureObject blueprintOnTile = tileFromMapCoordinates.tileObjectComponent.genericTileObject.blueprintOnTile;
				if (blueprintOnTile != null && blueprintOnTile.currentVisualMode == LocationStructureObject.Structure_Visual_Mode.Demonic_Structure_Blueprint)
				{
					return blueprintOnTile;
				}
			}
		}
		if (tile.structure != null)
		{
			if (tile.structure.IsTilePartOfARoom(tile, out var room) && room.CanBeSelected())
			{
				return room;
			}
			if (tile.structure.structureType.IsPlayerStructure())
			{
				if (tile.tileState == LocationGridTile.Tile_State.Occupied)
				{
					return tile.structure;
				}
			}
			else
			{
				if (tile.structure is ManMadeStructure { structureObj: not null } || (tile.structure is DemonicStructure { structureObj: not null } && !(tile.structure is CityCenter)))
				{
					return tile.structure;
				}
				if (tile.structure.structureType.IsSpecialStructure() || tile.structure.structureType == STRUCTURE_TYPE.BANDIT_CAMP)
				{
					return tile.structure;
				}
			}
		}
		return null;
	}

	private IPointOfInterest GetPOIOnTileClosestToDistance(Vector3 p_originDistance, LocationGridTile p_tile)
	{
		IPointOfInterest pointOfInterest = null;
		float num = 0f;
		for (int i = 0; i < p_tile.charactersHere.Count; i++)
		{
			Character character = p_tile.charactersHere[i];
			if (character.CanBeSelected() && character.hasMarker && !character.marker.IsInvisibleToPlayer())
			{
				float num2 = Vector3.Distance(p_originDistance, character.marker.transform.position);
				if (pointOfInterest == null || num2 < num)
				{
					pointOfInterest = character;
					num = num2;
				}
			}
		}
		TileObject objHere = p_tile.tileObjectComponent.objHere;
		if (objHere != null && objHere.CanBeSelected() && objHere.mapObjectVisual != null && !objHere.mapObjectVisual.IsInvisibleToPlayer())
		{
			float num3 = Vector3.Distance(p_originDistance, objHere.mapObjectVisual.transform.position);
			if (pointOfInterest == null || num3 < num)
			{
				pointOfInterest = objHere;
				num = num3;
			}
		}
		objHere = p_tile.tileObjectComponent.hiddenObjHere;
		if (objHere != null && objHere.CanBeSelected() && objHere.mapObjectVisual != null && !objHere.mapObjectVisual.IsInvisibleToPlayer())
		{
			float num4 = Vector3.Distance(p_originDistance, objHere.mapObjectVisual.transform.position);
			if (pointOfInterest == null || num4 < num)
			{
				pointOfInterest = objHere;
				num = num4;
			}
		}
		return pointOfInterest;
	}

	public void Initialize()
	{
		innerMaps = new List<InnerTileMap>();
		worldKnownDemonicStructures = new List<LocationStructure>();
		mapObjectFactory = new MapVisualFactory();
		InnerMapCameraMove.Instance.Initialize();
		ConstructInitialUnusedPathfindingTags();
		_mapObjectHover.sortingLayerName = "Area Maps";
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
	}

	public void TryShowLocationMap(Region location)
	{
		ShowInnerMap(location);
	}

	public void ShowInnerMap(Region location, bool centerCameraOnMapCenter = true, bool instantCenter = true)
	{
		location.innerMap.Open();
		currentlyShowingMap = location.innerMap;
		currentlyShowingLocation = location;
		Messenger.Broadcast(RegionSignals.REGION_MAP_OPENED, location);
		if (centerCameraOnMapCenter)
		{
			InnerMapCameraMove.Instance.JustCenterCamera(instantCenter);
		}
	}

	public Region HideAreaMap()
	{
		if (currentlyShowingMap == null)
		{
			return null;
		}
		currentlyShowingMap.Close();
		Region region = currentlyShowingLocation;
		InnerMapCameraMove.Instance.CenterCameraOn(null);
		currentlyShowingMap = null;
		currentlyShowingLocation = null;
		Messenger.Broadcast(RegionSignals.REGION_MAP_CLOSED, region);
		return region;
	}

	public void OnCreateInnerMap(InnerTileMap newMap)
	{
		innerMaps.Add(newMap);
		newMap.transform.localPosition = _nextMapPos;
		newMap.UpdateTilesWorldPosition();
		PathfindingManager.Instance.CreatePathfindingGraphForLocation(newMap);
		mainGraphMask |= GraphMask.FromGraph(newMap.pathfindingGraph);
		_nextMapPos = new Vector3(_nextMapPos.x, _nextMapPos.y + (float)newMap.height + 50f, _nextMapPos.z);
		newMap.OnMapGenerationFinished();
	}

	public LocationGridTile GetTileFromMousePosition()
	{
		return GridMap.Instance.mainRegion.innerMap.GetTileFromScreenPosition(InputManager.Instance.mousePosition);
	}

	public bool IsShowingInnerMap(Region location)
	{
		if (location != null && isAnInnerMapShowing)
		{
			return location.innerMap == currentlyShowingMap;
		}
		return false;
	}

	public T GetTileObjectScriptableObject<T>(TILE_OBJECT_TYPE p_tileObjectType) where T : TileObjectScriptableObject
	{
		if (!_tileObjectScriptableObjects.ContainsKey(p_tileObjectType))
		{
			TileObjectScriptableObject tileObjectScriptableObject = Resources.Load<TileObjectScriptableObject>($"Tile Object Data/{p_tileObjectType}");
			if (tileObjectScriptableObject == null)
			{
				throw new Exception($"{p_tileObjectType} has no scriptable object!");
			}
			_tileObjectScriptableObjects.Add(p_tileObjectType, tileObjectScriptableObject);
		}
		if (_tileObjectScriptableObjects[p_tileObjectType] is T result)
		{
			return result;
		}
		return null;
	}

	public void ShowTileData(LocationGridTile tile, Character character = null)
	{
		if (!ConsoleBase.showPOIHoverData || tile == null || (UIManager.Instance.poiTestingUI.gameObject.activeSelf && (UIManager.Instance.poiTestingUI.gridTile == tile || UIManager.Instance.poiTestingUI.poi == tile.tileObjectComponent.objHere || UIManager.Instance.poiTestingUI.poi == character)))
		{
			return;
		}
		TileObject objHere = tile.tileObjectComponent.objHere;
		if (character != null || objHere == null)
		{
			return;
		}
		string text = objHere.name;
		if (objHere.users != null && objHere.GetUserCount() > 0)
		{
			text = text + " " + LocalizationManager.Object_Used_By;
			for (int i = 0; i < objHere.users.Length; i++)
			{
				Character character2 = objHere.users[i];
				if (character2 != null)
				{
					text = text + "\n\t" + character2.name;
				}
			}
		}
		UIManager.Instance.ShowSmallInfo(text, "", autoReplaceText: false);
	}

	private string GetCharacterHoverData(Character character)
	{
		string text = "Character: " + character.name;
		text = text + "\n<b>Mood:</b>" + character.moodComponent.moodState;
		text = text + " <b>Can Move:</b>" + character.limiterComponent.canMove;
		text = text + " <b>Can Witness:</b>" + character.limiterComponent.canWitness;
		text = text + " <b>Can Be Attacked:</b>" + character.limiterComponent.canBeAttacked;
		text = text + " <b>Move Speed:</b>" + character.marker.pathfindingAI.speed;
		text = text + " <b>Attack Range:</b>" + character.combatComponent.attackRange;
		text = text + " <b>Attack Speed:</b>" + character.combatComponent.attackSpeed;
		text = text + " <b>Target POI:</b>" + (character.marker.targetPOI?.name ?? "None");
		text = text + " <b>Base Structure:</b>" + ((character.trapStructure.structure != null) ? character.trapStructure.structure.ToString() : "None");
		text = text + " <b>Actions Being Performed on this:</b>" + character.numOfNonSecretActionsBeingPerformedOnThis;
		text += "Destination Tile: ";
		text = ((character.marker.destinationTile == null) ? (text + "None") : $"{text}{character.marker.destinationTile} at {character.marker.destinationTile.parentMap.region.name}");
		text += "\n\tCharacters that have reacted to me: ";
		text = ((character.defaultCharacterTrait.charactersThatHaveReactedToThis.Count > 0) ? character.defaultCharacterTrait.charactersThatHaveReactedToThis.Aggregate(text, (string current, Character c) => current + c.name + ", ") : (text + "None"));
		text += "\n\tHostiles in Range: ";
		text = ((character.combatComponent.hostilesInRange.Count > 0) ? character.combatComponent.hostilesInRange.Aggregate(text, (string current, IPointOfInterest poi) => $"{current}{poi.name}({character.combatComponent.GetCombatData(poi)?.isLethal}), ") : (text + "None"));
		text += "\n\tAvoid in Range: ";
		text = ((character.combatComponent.avoidInRange.Count > 0) ? character.combatComponent.avoidInRange.Aggregate(text, (string current, IPointOfInterest poi) => $"{current}{poi.name}({character.combatComponent.GetCombatData(poi)?.isLethal}), ") : (text + "None"));
		text += "\n\tPOI's in Vision: ";
		text = ((character.marker.inVisionPOIs.Count > 0) ? character.marker.inVisionPOIs.Aggregate(text, (string current, IPointOfInterest poi) => $"{current}{poi}, ") : (text + "None"));
		text += "\n\tCharacters in Vision: ";
		text = ((character.marker.inVisionCharacters.Count > 0) ? character.marker.inVisionCharacters.Select((Character t, int i) => character.marker.inVisionCharacters.ElementAt(i)).Aggregate(text, (string current, Character poi) => current + poi.name + ", ") : (text + "None"));
		text += "\n\tPOI's in Range but different structures: ";
		return (character.marker.inVisionPOIsButDiffStructure.Count > 0) ? character.marker.inVisionPOIsButDiffStructure.Aggregate(text, (string current, IPointOfInterest poi) => $"{current}{poi}, ") : (text + "None");
	}

	public bool HasSettingForTileObjectAsset(Sprite asset)
	{
		return tileObjectSlotSettings.ContainsKey(asset);
	}

	public List<TileObjectSlotSetting> GetTileObjectSlotSettings(Sprite asset)
	{
		return tileObjectSlotSettings[asset];
	}

	public TileObject GetTileObject(TILE_OBJECT_TYPE type, int id)
	{
		return DatabaseManager.Instance.tileObjectDatabase.GetTileObject(type, id);
	}

	public TileObject GetTileObjectByPersistentID(string id)
	{
		return DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(id);
	}

	public TileObject GetFirstTileObject(TILE_OBJECT_TYPE type)
	{
		return DatabaseManager.Instance.tileObjectDatabase.GetFirstTileObject(type);
	}

	public TileObject GetFirstArtifact(ARTIFACT_TYPE artifactType)
	{
		return DatabaseManager.Instance.tileObjectDatabase.GetFirstArtifact(artifactType);
	}

	public T CreateNewTileObject<T>(TILE_OBJECT_TYPE tileObjectType) where T : TileObject
	{
		Type type = Type.GetType(tileObjectType.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
		if (type != null)
		{
			return Activator.CreateInstance(type) as T;
		}
		throw new Exception("Could not create new instance of tile object of type " + tileObjectType);
	}

	public T LoadTileObject<T>(SaveDataTileObject saveDataTileObject) where T : TileObject
	{
		Type type = Type.GetType(saveDataTileObject.tileObjectType.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
		if (type != null)
		{
			return Activator.CreateInstance(type, saveDataTileObject) as T;
		}
		throw new Exception($"Could not create new instance of tile object of type {saveDataTileObject.tileObjectType}");
	}

	public T LoadTileObject<T>(SaveDataArtifact saveDataTileObject) where T : TileObject
	{
		Type type = Type.GetType(Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(saveDataTileObject.artifactType.ToString()) + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
		if (type != null)
		{
			return Activator.CreateInstance(type, saveDataTileObject) as T;
		}
		throw new Exception($"Could not create new instance of tile object of type {saveDataTileObject.tileObjectType}");
	}

	public T LoadTileObject<T>(SaveDataEquipmentItem saveDataTileObject) where T : TileObject
	{
		Type type = Type.GetType(saveDataTileObject.tileObjectType.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
		if (type != null)
		{
			return Activator.CreateInstance(type, saveDataTileObject) as T;
		}
		throw new Exception($"Could not create new instance of tile object of type {saveDataTileObject.tileObjectType}");
	}

	public TILE_OBJECT_TYPE GetTileObjectTypeFromTileAsset(Sprite sprite)
	{
		int num = sprite.name.IndexOf("#", StringComparison.Ordinal);
		string s = sprite.name;
		if (num != -1)
		{
			s = sprite.name.Substring(0, num);
		}
		s = Utilities.NotNormalizedConversionStringToEnum(s);
		return (TILE_OBJECT_TYPE)Enum.Parse(typeof(TILE_OBJECT_TYPE), s);
	}

	public void LoadInitialSettlementItems(NPCSettlement npcSettlement)
	{
		LocationStructure mainStorage = npcSettlement.mainStorage;
		for (int i = 0; i < 4; i++)
		{
			mainStorage.AddPOI(CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HEALING_POTION));
		}
		for (int j = 0; j < 2; j++)
		{
			mainStorage.AddPOI(CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ANTIDOTE));
		}
	}

	public T CreateNewResourcePileAndTryCreateHaulJob<T>(TILE_OBJECT_TYPE tileObjectType, int resourcesInPile, [NotNull] Character creator, [NotNull] LocationGridTile locationGridTile) where T : ResourcePile
	{
		T val = CreateNewTileObject<T>(tileObjectType);
		val.SetResourceInPile(resourcesInPile);
		locationGridTile.structure.AddPOI(val, locationGridTile);
		if (creator.homeSettlement != null)
		{
			creator.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(val);
			creator.marker.AddPOIAsInVisionRange(val);
		}
		return val;
	}

	public void CreateWurmHoles(LocationGridTile point1, LocationGridTile point2)
	{
		WurmHole wurmHole = CreateNewTileObject<WurmHole>(TILE_OBJECT_TYPE.WURM_HOLE);
		WurmHole wurmHole2 = CreateNewTileObject<WurmHole>(TILE_OBJECT_TYPE.WURM_HOLE);
		wurmHole.SetWurmHoleConnection(wurmHole2);
		wurmHole2.SetWurmHoleConnection(wurmHole);
		point1.structure.AddPOI(wurmHole, point1);
		point2.structure.AddPOI(wurmHole2, point2);
	}

	public void ShowHoverVisual(BaseMapObjectVisual p_visual)
	{
		Transform obj = _mapObjectHover.transform;
		Transform transform = p_visual.objectSpriteRenderer.transform;
		Sprite usedSprite = p_visual.usedSprite;
		_mapObjectHover.sprite = usedSprite;
		_mapObjectHover.sortingOrder = p_visual.objectSpriteRenderer.sortingOrder - 1;
		_mapObjectHover.gameObject.SetActive(value: true);
		obj.SetParent(transform.parent);
		Vector2 vector = transform.localPosition;
		if (usedSprite != null)
		{
			Bounds bounds = usedSprite.bounds;
			float num = (0f - bounds.center.x) / bounds.extents.x / 2f + 0.5f;
			float num2 = (0f - bounds.center.y) / bounds.extents.y / 2f + 0.5f;
			if (num < 0.5f && num2 < 0.5f)
			{
				vector.x -= 0.1f;
				vector.y -= 0.1f;
			}
		}
		obj.localPosition = vector;
		obj.localRotation = transform.localRotation;
		Vector2 vector2 = transform.localScale;
		vector2.x += 0.1f;
		vector2.y += 0.1f;
		obj.localScale = vector2;
	}

	public void HideHoverVisual()
	{
		_mapObjectHover.gameObject.SetActive(value: false);
		_mapObjectHover.transform.SetParent(base.transform);
	}

	public List<GameObject> GetStructurePrefabsForStructure(FACTION_TYPE p_factionType, STRUCTURE_TYPE p_structureType, RESOURCE resource)
	{
		StructureSetting structureSetting = new StructureSetting(p_structureType, resource);
		return GetStructurePrefabsForStructure(p_factionType, structureSetting);
	}

	public List<GameObject> GetStructurePrefabsForStructure(FACTION_TYPE p_factionType, StructureSetting structureSetting)
	{
		return LandmarkManager.Instance.GetStructureData(structureSetting.structureType).GetStructurePrefabs(p_factionType, structureSetting);
	}

	public GameObject GetFirstStructurePrefabForStructure(FACTION_TYPE p_factionType, StructureSetting structureSetting)
	{
		return LandmarkManager.Instance.GetStructureData(structureSetting.structureType).GetStructurePrefabs(p_factionType, structureSetting).First();
	}

	public void AddWorldKnownDemonicStructure(LocationStructure structure)
	{
		worldKnownDemonicStructures.Add(structure);
	}

	public void RemoveWorldKnownDemonicStructure(LocationStructure structure)
	{
		worldKnownDemonicStructures.Remove(structure);
	}

	public bool HasWorldKnownDemonicStructure(LocationStructure structure)
	{
		return worldKnownDemonicStructures.Contains(structure);
	}

	public bool HasExistingWorldKnownDemonicStructure()
	{
		return worldKnownDemonicStructures.Count > 0;
	}

	public Sprite GetTileObjectAsset(TileObject tileObject, POI_STATE state, BIOMES biome, bool corrupted = false)
	{
		if (tileObject.tileObjectType == TILE_OBJECT_TYPE.ARTIFACT)
		{
			Artifact artifact = tileObject as Artifact;
			if (ScriptableObjectsManager.Instance.artifactDataDictionary.ContainsKey(artifact.type))
			{
				return ScriptableObjectsManager.Instance.artifactDataDictionary[artifact.type].sprite;
			}
			return null;
		}
		TileObjectScriptableObject tileObjectScriptableObject = GetTileObjectScriptableObject<TileObjectScriptableObject>(tileObject.tileObjectType);
		TileObjectTileSetting tileObjectTileSetting = (corrupted ? tileObjectScriptableObject.corruptedTileObjectAssets : tileObjectScriptableObject.tileObjectAssets);
		if (tileObjectTileSetting.biomeAssets.Count <= 0)
		{
			tileObjectTileSetting = tileObjectScriptableObject.tileObjectAssets;
		}
		BiomeTileObjectTileSetting biomeTileObjectTileSetting = (tileObjectTileSetting.biomeAssets.ContainsKey(biome) ? tileObjectTileSetting.biomeAssets[biome] : tileObjectTileSetting.biomeAssets[BIOMES.NONE]);
		return CollectionUtilities.GetRandomElement((state == POI_STATE.ACTIVE) ? biomeTileObjectTileSetting.activeTile : biomeTileObjectTileSetting.inactiveTile);
	}

	public Sprite GetTileObjectAsset(TileObject tileObject, POI_STATE state, bool corrupted = false)
	{
		if (tileObject.tileObjectType == TILE_OBJECT_TYPE.ARTIFACT)
		{
			Artifact artifact = tileObject as Artifact;
			if (ScriptableObjectsManager.Instance.artifactDataDictionary.ContainsKey(artifact.type))
			{
				return ScriptableObjectsManager.Instance.artifactDataDictionary[artifact.type].sprite;
			}
			return null;
		}
		TileObjectScriptableObject tileObjectScriptableObject = GetTileObjectScriptableObject<TileObjectScriptableObject>(tileObject.tileObjectType);
		TileObjectTileSetting tileObjectTileSetting = (corrupted ? tileObjectScriptableObject.corruptedTileObjectAssets : tileObjectScriptableObject.tileObjectAssets);
		if (tileObjectTileSetting.biomeAssets.Count <= 0)
		{
			tileObjectTileSetting = tileObjectScriptableObject.tileObjectAssets;
		}
		BiomeTileObjectTileSetting biomeTileObjectTileSetting = tileObjectTileSetting.biomeAssets[BIOMES.NONE];
		return CollectionUtilities.GetRandomElement((state == POI_STATE.ACTIVE) ? biomeTileObjectTileSetting.activeTile : biomeTileObjectTileSetting.inactiveTile);
	}

	public Sprite GetCorruptedTileObjectAsset(TILE_OBJECT_TYPE p_tileObjectType, POI_STATE state, bool p_getFirstAsset = false)
	{
		TileObjectScriptableObject tileObjectScriptableObject = GetTileObjectScriptableObject<TileObjectScriptableObject>(p_tileObjectType);
		TileObjectTileSetting tileObjectTileSetting = tileObjectScriptableObject.corruptedTileObjectAssets;
		if (tileObjectTileSetting.biomeAssets.Count <= 0)
		{
			tileObjectTileSetting = tileObjectScriptableObject.tileObjectAssets;
		}
		BiomeTileObjectTileSetting biomeTileObjectTileSetting = tileObjectTileSetting.biomeAssets[BIOMES.NONE];
		if (p_getFirstAsset)
		{
			if (state == POI_STATE.ACTIVE)
			{
				if (biomeTileObjectTileSetting.activeTile != null && biomeTileObjectTileSetting.activeTile.Length != 0)
				{
					return biomeTileObjectTileSetting.activeTile[0];
				}
			}
			else if (biomeTileObjectTileSetting.inactiveTile != null && biomeTileObjectTileSetting.inactiveTile.Length != 0)
			{
				return biomeTileObjectTileSetting.inactiveTile[0];
			}
			return null;
		}
		return CollectionUtilities.GetRandomElement((state == POI_STATE.ACTIVE) ? biomeTileObjectTileSetting.activeTile : biomeTileObjectTileSetting.inactiveTile);
	}

	public WallAsset GetWallAsset(WALL_RESOURCE wallResource, string assetName)
	{
		return wallResourceAssets[wallResource].GetWallAsset(assetName);
	}

	public void SetCurrentlyHoveredPOI(IPointOfInterest poi)
	{
		currentlyHoveredPoi = poi;
	}

	public bool IsPOIConsideredTheCurrentHoveredPOI(IPointOfInterest poi)
	{
		if (currentlyHoveredPoi == poi)
		{
			return true;
		}
		if (currentlyHoveredPoi is Tombstone tombstone)
		{
			return tombstone.character == poi;
		}
		return false;
	}

	public void FaceTarget(IPointOfInterest actor, IPointOfInterest target)
	{
		if (actor != target && actor != null && target != null && actor.gridTileLocation != null && target.gridTileLocation != null)
		{
			BaseMapObjectVisual mapObjectVisual = target.mapObjectVisual;
			if (target.isBeingCarriedBy != null)
			{
				mapObjectVisual = target.isBeingCarriedBy.mapObjectVisual;
			}
			if (target.isBeingCarriedBy != actor && (object)mapObjectVisual != null)
			{
				actor.mapObjectVisual.LookAt(mapObjectVisual.transform.position);
			}
		}
	}

	public void FaceTarget(IPointOfInterest actor, LocationGridTile target)
	{
		if (actor != null && target != null && actor.gridTileLocation != null && target != actor.gridTileLocation)
		{
			actor.mapObjectVisual.LookAt(target.centeredWorldLocation);
		}
	}

	public Artifact CreateNewArtifact(ARTIFACT_TYPE artifactType)
	{
		return CreateNewArtifactFromType(artifactType);
	}

	private Artifact CreateNewArtifactFromType(ARTIFACT_TYPE artifactType)
	{
		return Activator.CreateInstance(Type.GetType(Utilities.NotNormalizedConversionEnumToStringNoSpaces(artifactType.ToString()) + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")) as Artifact;
	}

	public void MonsterLairCellAutomata(List<LocationGridTile> locationGridTiles, LocationStructure structure, Region region, LocationStructure wilderness)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < locationGridTiles.Count; i++)
		{
			LocationGridTile locationGridTile = locationGridTiles[i];
			if (!locationGridTile.area.gridTileComponent.borderTiles.Contains(locationGridTile))
			{
				list.Add(locationGridTile);
			}
		}
		LocationGridTile[,] tileMap = CellularAutomataGenerator.ConvertListToGridMap(list);
		float randomElement = CollectionUtilities.GetRandomElement(_monsterLairSeeds);
		int[,] cellAutomata = CellularAutomataGenerator.GenerateMap(tileMap, list, 2, 10, randomElement.ToString());
		CellularAutomataGenerator.DrawMap(tileMap, cellAutomata, assetManager.monsterLairWallTile, null, delegate(LocationGridTile tile)
		{
			SetAsWall(tile, structure);
		}, delegate(LocationGridTile tile)
		{
			SetAsGround(tile, structure);
		});
		List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
		list2.AddRange(list);
		for (int num = 0; num < list2.Count; num++)
		{
			LocationGridTile locationGridTile2 = list2[num];
			if (!locationGridTile2.HasNeighbourOfType(LocationGridTile.Ground_Type.Flesh))
			{
				locationGridTile2.SetStructureTilemapVisual(null);
				locationGridTile2.SetTileType(LocationGridTile.Tile_Type.Empty);
				locationGridTile2.SetTileState(LocationGridTile.Tile_State.Empty);
				locationGridTile2.RevertTileToOriginalPerlin();
				locationGridTile2.SetStructure(wilderness);
				list.Remove(locationGridTile2);
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list2);
		MonsterLairPerlin(list, structure, randomElement, randomElement);
		List<LocationGridTile> list3 = RuinarchListPool<LocationGridTile>.Claim();
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			LocationGridTile locationGridTile3 = list[num2];
			if (locationGridTile3.tileType == LocationGridTile.Tile_Type.Wall && !locationGridTile3.IsAtEdgeOfMap() && locationGridTile3.HasDifferentStructureNeighbour(useFourNeighbours: true) && locationGridTile3.GetCountNeighboursOfType(LocationGridTile.Tile_Type.Wall, useFourNeighbours: true) == 2 && locationGridTile3.GetCountNeighboursOfType(LocationGridTile.Tile_Type.Empty, useFourNeighbours: true) == 2)
			{
				list3.Add(locationGridTile3);
			}
		}
		for (int num3 = 0; num3 < 5; num3++)
		{
			if (list3.Count > 0)
			{
				LocationGridTile randomElement2 = CollectionUtilities.GetRandomElement(list3);
				randomElement2.SetStructureTilemapVisual(null);
				randomElement2.SetTileType(LocationGridTile.Tile_Type.Empty);
				randomElement2.SetStructure(wilderness);
				randomElement2.RevertTileToOriginalPerlin();
				list3.Remove(randomElement2);
				randomElement2.UpdateMinimapVisual(randomElement2.structure);
				continue;
			}
			Debug.LogWarning($"Could not find entrance for {structure}");
			break;
		}
		RuinarchListPool<LocationGridTile>.Release(list3);
		for (int num4 = 0; num4 < list.Count; num4++)
		{
			LocationGridTile locationGridTile4 = list[num4];
			if (locationGridTile4.tileObjectComponent.objHere == null && locationGridTile4.tileType == LocationGridTile.Tile_Type.Wall)
			{
				BlockWall blockWall = CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
				blockWall.SetWallType(WALL_TYPE.Flesh);
				structure.AddPOI(blockWall, locationGridTile4);
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	private void SetAsWall(LocationGridTile tile, LocationStructure structure)
	{
		if (tile.tileObjectComponent.objHere != null)
		{
			tile.structure.RemovePOI(tile.tileObjectComponent.objHere);
		}
		tile.CreateSeamlessEdgesForSelfAndNeighbours();
		if (!GameManager.Instance.gameHasStarted)
		{
			tile.parentMap.detailsTilemap.SetTile(tile.localPlace, null);
		}
		tile.SetGroundTilemapVisual(assetManager.monsterLairGroundTile);
		tile.SetTileType(LocationGridTile.Tile_Type.Wall);
		tile.SetTileState(LocationGridTile.Tile_State.Occupied);
		tile.SetStructure(structure);
	}

	private void SetAsGround(LocationGridTile tile, LocationStructure structure)
	{
		if (tile.tileObjectComponent.objHere != null)
		{
			tile.structure.RemovePOI(tile.tileObjectComponent.objHere);
		}
		tile.CreateSeamlessEdgesForSelfAndNeighbours();
		if (!GameManager.Instance.gameHasStarted)
		{
			tile.parentMap.detailsTilemap.SetTile(tile.localPlace, null);
		}
		tile.SetStructure(structure);
		tile.SetGroundTilemapVisual(assetManager.monsterLairGroundTile);
	}

	private void MonsterLairPerlin(List<LocationGridTile> tiles, LocationStructure structure, float seedX = -1f, float seedY = -1f)
	{
		float num = seedX;
		float num2 = seedY;
		if (num <= 0f)
		{
			num = UnityEngine.Random.Range(0f, 99999f);
		}
		if (num2 <= 0f)
		{
			num2 = UnityEngine.Random.Range(0f, 99999f);
		}
		int num3 = tiles.Min((LocationGridTile t) => t.localPlace.x);
		int num4 = tiles.Max((LocationGridTile t) => t.localPlace.x);
		int num5 = tiles.Min((LocationGridTile t) => t.localPlace.y);
		int num6 = tiles.Max((LocationGridTile t) => t.localPlace.y);
		int num7 = num4 - num3;
		int num8 = num6 - num5;
		for (int num9 = 0; num9 < tiles.Count; num9++)
		{
			LocationGridTile locationGridTile = tiles[num9];
			float x = (float)(locationGridTile.localPlace.x - num3) / (float)num7 * 5f + num;
			float y = (float)(locationGridTile.localPlace.y - num5) / (float)num8 * 5f + num2;
			if (Mathf.PerlinNoise(x, y) <= 0.4f)
			{
				SetAsWall(locationGridTile, structure);
			}
			locationGridTile.UpdateMinimapVisual(structure);
		}
	}

	private void CycleRegions()
	{
		TryShowLocationMap(GridMap.Instance.mainRegion);
	}

	public PathfindingTagPair ClaimNextPathfindingTagPair()
	{
		if (unusedPathfindingTags.Count > 0)
		{
			PathfindingTagPair result = unusedPathfindingTags[0];
			unusedPathfindingTags.RemoveAt(0);
			return result;
		}
		throw new Exception("No more pathfinding tags found!");
	}

	public void SetPathfindingTagPairAsClaimed(PathfindingTagPair p_pair)
	{
		for (int i = 0; i < unusedPathfindingTags.Count; i++)
		{
			if (unusedPathfindingTags[i].Equals(p_pair))
			{
				unusedPathfindingTags.RemoveAt(i);
				break;
			}
		}
	}

	public void ReturnPathfindingPair(Faction p_faction)
	{
		PathfindingTagPair item = new PathfindingTagPair(p_faction.pathfindingTag, p_faction.pathfindingDoorTag);
		if (!unusedPathfindingTags.Contains(item))
		{
			unusedPathfindingTags.Add(item);
		}
	}

	private void ConstructInitialUnusedPathfindingTags()
	{
		unusedPathfindingTags = new List<PathfindingTagPair>();
		for (int i = 20; i < 32; i += 2)
		{
			PathfindingTagPair item = new PathfindingTagPair((uint)i, (uint)(i + 1));
			unusedPathfindingTags.Add(item);
		}
	}

	public PoisonCloud SpawnPoisonCloud(LocationGridTile gridTileLocation, int stacks)
	{
		PoisonCloud poisonCloud = new PoisonCloud();
		poisonCloud.SetGridTileLocation(gridTileLocation);
		poisonCloud.OnPlacePOI();
		poisonCloud.SetStacks(stacks);
		return poisonCloud;
	}

	public PoisonCloud SpawnPoisonCloud(LocationGridTile gridTileLocation, int stacks, GameDate expiryDate)
	{
		PoisonCloud poisonCloud = SpawnPoisonCloud(gridTileLocation, stacks);
		poisonCloud.SetExpiryDate(expiryDate);
		return poisonCloud;
	}

	public void ShowAreaMapTextPopup(string p_text, Vector3 p_worldPos, Color p_color)
	{
		ObjectPoolManager.Instance.InstantiateObjectFromPool(pfAreaMapTextPopup.name, p_worldPos, Quaternion.identity, base.transform, isWorldPosition: true).GetComponent<AreaMapTextPopup>().Show(p_text, p_worldPos, p_color);
	}

	public bool CanBigTreeBePlacedOnTile(LocationGridTile tile)
	{
		if (tile.isOccupied)
		{
			return false;
		}
		if (tile.groundType == LocationGridTile.Ground_Type.Bone)
		{
			return false;
		}
		if (tile.corruptionComponent.isCorrupted)
		{
			return false;
		}
		if (tile.structure != null && !tile.structure.structureType.IsOpenSpace())
		{
			return false;
		}
		if (tile.HasNeighbourOfType(LocationGridTile.Tile_Type.Wall))
		{
			return false;
		}
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		tile.parentMap.PopulateTiles(list, new Point(2, 2), tile);
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			if (locationGridTile.tileObjectComponent.objHere != null || locationGridTile.tileType == LocationGridTile.Tile_Type.Wall || locationGridTile.elevationType == ELEVATION.WATER || locationGridTile.corruptionComponent.isCorrupted)
			{
				num++;
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return num <= 0;
	}

	public bool CanBigTreeBePlacedOnTileInRandomGeneration(LocationGridTile tile, MapGenerationData p_data)
	{
		if (tile.isOccupied)
		{
			return false;
		}
		if (tile.groundType == LocationGridTile.Ground_Type.Bone)
		{
			return false;
		}
		if (tile.corruptionComponent.isCorrupted)
		{
			return false;
		}
		if (tile.structure != null && !tile.structure.structureType.IsOpenSpace())
		{
			return false;
		}
		if (tile.HasNeighbourOfType(LocationGridTile.Tile_Type.Wall))
		{
			return false;
		}
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		tile.parentMap.PopulateTiles(list, new Point(2, 2), tile);
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			if (locationGridTile.tileObjectComponent.objHere != null || locationGridTile.tileType == LocationGridTile.Tile_Type.Wall || p_data.GetGeneratedObjectOnTile(locationGridTile) != TILE_OBJECT_TYPE.NONE || locationGridTile.elevationType == ELEVATION.WATER || locationGridTile.corruptionComponent.isCorrupted)
			{
				num++;
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return num <= 0;
	}

	public bool AddMonsterSpawner(MonsterSpawner spawner, bool shouldUpdateSpellItem = true)
	{
		if (!currentMonsterSpawners.Contains(spawner))
		{
			currentMonsterSpawners.Add(spawner);
			if (shouldUpdateSpellItem)
			{
				PlayerUI.Instance.GetSpellItem(PLAYER_SKILL_TYPE.MONSTER_SPAWNER)?.ForceUpdateInteractableState();
			}
			return true;
		}
		return false;
	}

	public bool RemoveMonsterSpawner(MonsterSpawner spawner, bool shouldUpdateSpellItem = true)
	{
		if (currentMonsterSpawners.Remove(spawner))
		{
			if (shouldUpdateSpellItem)
			{
				PlayerUI.Instance.GetSpellItem(PLAYER_SKILL_TYPE.MONSTER_SPAWNER)?.ForceUpdateInteractableState();
			}
			return true;
		}
		return false;
	}

	public bool HasMonsterSpawnerInStructure(LocationStructure p_structure)
	{
		for (int i = 0; i < currentMonsterSpawners.Count; i++)
		{
			if (currentMonsterSpawners[i].gridTileLocation?.structure == p_structure)
			{
				return true;
			}
		}
		return false;
	}

	public int GetMonsterSpawnerSpawnHour()
	{
		return PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.MONSTER_SPAWNER).currentLevel switch
		{
			0 => 8, 
			1 => 8, 
			2 => 8, 
			3 => 8, 
			_ => 8, 
		};
	}

	public int GetMonsterSpawnerMaxHP()
	{
		return PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.MONSTER_SPAWNER).currentLevel switch
		{
			0 => 3700, 
			1 => 3700, 
			2 => 4900, 
			3 => 4900, 
			_ => 3700, 
		};
	}

	public void ShowHealthAdjustmentEffect(int damage, CombatComponent p_combatComponent, Vector3 p_worldPosition)
	{
		TextRendererParticleSystem component = ObjectPoolManager.Instance.InstantiateObjectFromPool("Text_Particles", p_worldPosition, Quaternion.identity, currentlyShowingMap.transform, isWorldPosition: true).GetComponent<TextRendererParticleSystem>();
		Color color = Color.green;
		float value = 1.5f;
		if (p_combatComponent == null)
		{
			color = ((damage > 0) ? Color.green : Color.red);
			component.SpawnParticle(p_worldPosition, damage, color, value);
			return;
		}
		switch (p_combatComponent.damageDone.damageType)
		{
		case CombatComponent.DamageDoneType.DamageType.Normal:
			color = ((damage > 0) ? Color.green : Color.red);
			break;
		case CombatComponent.DamageDoneType.DamageType.Crit:
			color = Color.yellow;
			value = 2.5f;
			break;
		}
		component.SpawnParticle(p_worldPosition, damage, color, value);
	}

	public void ShowTextEffect(string p_text, Color p_color, Vector3 p_worldPosition)
	{
		TextRendererParticleSystem component = ObjectPoolManager.Instance.InstantiateObjectFromPool("Text_Particles", p_worldPosition, Quaternion.identity, currentlyShowingMap.transform, isWorldPosition: true).GetComponent<TextRendererParticleSystem>();
		float value = 1.5f;
		component.SpawnParticle(base.transform.position, p_text, p_color, value);
	}
}
