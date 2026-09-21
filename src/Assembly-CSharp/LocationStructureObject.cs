using System;
using System.Collections.Generic;
using System.Linq;
using EZObjectPools;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UtilityScripts;

public class LocationStructureObject : PooledObject, ISelectable
{
	public enum Structure_Visual_Mode
	{
		Blueprint,
		Built,
		Demonic_Structure_Blueprint
	}

	public STRUCTURE_TYPE structureType;

	[Header("Tilemaps")]
	[SerializeField]
	protected Tilemap _groundTileMap;

	[SerializeField]
	protected Tilemap _detailTileMap;

	[SerializeField]
	protected TilemapRenderer _groundTileMapRenderer;

	[SerializeField]
	protected TilemapRenderer _detailTileMapRenderer;

	[SerializeField]
	protected Tilemap _blockWallsTilemap;

	[Header("Template Data")]
	[SerializeField]
	private Vector2Int _size;

	[SerializeField]
	private Vector3Int _center;

	[SerializeField]
	private int _repairCost = 5;

	[Header("Objects")]
	[FormerlySerializedAs("_objectsParent")]
	public Transform objectsParent;

	[Header("Walls")]
	[Tooltip("This is only relevant if blockWallsTilemap is not null.")]
	[FormerlySerializedAs("_wallType")]
	[SerializeField]
	private WALL_TYPE _blockWallType;

	[Tooltip("This is only relevant if structure uses thin walls.")]
	[FormerlySerializedAs("_wallResource")]
	[SerializeField]
	private WALL_RESOURCE _thinWallResource;

	[Header("Helpers")]
	[FormerlySerializedAs("predeterminedOccupiedCoordinates")]
	[SerializeField]
	private List<Vector3Int> _predeterminedOccupiedCoordinates;

	[SerializeField]
	private List<Vector2Int> _borderCoordinates;

	[Header("Connectors")]
	[SerializeField]
	private StructureConnector[] _connectors;

	[SerializeField]
	private WardLightSpot[] _wardLightSpots;

	[FormerlySerializedAs("rooms")]
	[Header("Rooms")]
	public RoomTemplate[] roomTemplates;

	[Header("Interaction")]
	[SerializeField]
	private LocationStructureObjectClickCollider _clickCollider;

	private Tilemap[] allTilemaps;

	private ThinWallGameObject[] wallVisuals;

	private TilemapCollider2D _blockWallsTilemapCollider;

	private StructureTemplate _parentTemplate;

	private StructureTemplateObjectData[] _preplacedObjs;

	private int _totalBlockWallsCount;

	public bool wallsContributeToDamage = true;

	[Header("Wall Converter")]
	[SerializeField]
	private Tilemap wallTileMap;

	[SerializeField]
	private GameObject leftWall;

	[SerializeField]
	private GameObject rightWall;

	[SerializeField]
	private GameObject topWall;

	[SerializeField]
	private GameObject bottomWall;

	[SerializeField]
	private GameObject cornerPrefab;

	[Header("Predetermine Structure Tiles")]
	[SerializeField]
	private TileBase[] assetsToConsiderAsStructure;

	public LocationGridTile[] tiles { get; private set; }

	public Structure_Visual_Mode currentVisualMode { get; private set; }

	public Vector2Int size => _size;

	public Vector3Int center => _center;

	public StructureConnector[] connectors => _connectors;

	public List<Vector3Int> localOccupiedCoordinates
	{
		get
		{
			if (_predeterminedOccupiedCoordinates.Count == 0)
			{
				DetermineOccupiedTileCoordinates();
			}
			return _predeterminedOccupiedCoordinates;
		}
	}

	public WALL_RESOURCE thinWallResource => _thinWallResource;

	public int craftCost => structureType.GetResourceBuildCost(thinWallResource.GetResourceForWall());

	public int repairCost => _repairCost;

	public List<Vector2Int> borderCoordinates => _borderCoordinates;

	public Vector3 worldPosition
	{
		get
		{
			Vector3 position = base.transform.position;
			position.x -= 0.5f;
			position.y -= 0.5f;
			return position;
		}
	}

	public Vector2 selectableSize => size;

	protected virtual void Awake()
	{
		currentVisualMode = Structure_Visual_Mode.Built;
		allTilemaps = base.transform.GetComponentsInChildren<Tilemap>();
		_groundTileMapRenderer.sortingOrder = 50;
		wallVisuals = base.transform.GetComponentsInChildren<ThinWallGameObject>();
		_parentTemplate = GetComponentInParent<StructureTemplate>();
		if (_blockWallsTilemap != null)
		{
			_blockWallsTilemapCollider = _blockWallsTilemap.GetComponent<TilemapCollider2D>();
		}
		DetermineOccupiedTileCoordinates();
		InnerMapLight componentInChildren = GetComponentInChildren<InnerMapLight>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.gameObject.SetActive(value: false);
		}
	}

	public void RefreshAllTilemaps()
	{
		for (int i = 0; i < allTilemaps.Length; i++)
		{
			allTilemaps[i].RefreshAllTiles();
		}
	}

	protected virtual void UpdateSortingOrders()
	{
		_groundTileMapRenderer.sortingOrder = 15;
		_detailTileMapRenderer.sortingOrder = 40;
		for (int i = 0; i < wallVisuals.Length; i++)
		{
			wallVisuals[i].UpdateSortingOrders(41);
		}
	}

	public void OverrideDefaultSortingOrder(int p_sortingOrder)
	{
		_groundTileMapRenderer.sortingOrder = p_sortingOrder;
		_blockWallsTilemap.GetComponent<TilemapRenderer>().sortingOrder = p_sortingOrder + 1;
		_detailTileMapRenderer.sortingOrder = p_sortingOrder + 2;
		StructureTemplateObjectData[] preplacedObjects = GetPreplacedObjects();
		for (int i = 0; i < preplacedObjects.Length; i++)
		{
			preplacedObjects[i].SetSortingOrder(p_sortingOrder + 2);
		}
	}

	private void SetStructureColor(Color color)
	{
		for (int i = 0; i < allTilemaps.Length; i++)
		{
			allTilemaps[i].color = color;
		}
		for (int j = 0; j < wallVisuals.Length; j++)
		{
			wallVisuals[j].SetWallColor(color);
		}
	}

	public void SetTilesInStructure(LocationGridTile[] tiles)
	{
		this.tiles = tiles;
	}

	private void DetermineOccupiedTileCoordinates()
	{
		if (_predeterminedOccupiedCoordinates.Count != 0)
		{
			return;
		}
		BoundsInt cellBounds = _groundTileMap.cellBounds;
		for (int i = cellBounds.xMin; i < cellBounds.xMax; i++)
		{
			for (int j = cellBounds.yMin; j < cellBounds.yMax; j++)
			{
				Vector3Int vector3Int = new Vector3Int(i, j, 0);
				if (_groundTileMap.GetTile(vector3Int) != null)
				{
					_predeterminedOccupiedCoordinates.Add(vector3Int);
				}
			}
		}
	}

	private void RegisterPreplacedObjects(LocationStructure structure, InnerTileMap innerMap, TILE_OBJECT_TYPE[] typesToIgnore = null)
	{
		StructureTemplateObjectData[] preplacedObjects = GetPreplacedObjects();
		foreach (StructureTemplateObjectData structureTemplateObjectData in preplacedObjects)
		{
			Vector3Int vector3Int = innerMap.groundTilemap.WorldToCell(structureTemplateObjectData.transform.position);
			LocationGridTile locationGridTile = innerMap.map[vector3Int.x, vector3Int.y];
			if (locationGridTile.tileObjectComponent.objHere != null)
			{
				if (locationGridTile.tileObjectComponent.objHere.traitContainer.HasTrait("Indestructible"))
				{
					continue;
				}
				locationGridTile.structure.RemovePOI(locationGridTile.tileObjectComponent.objHere);
			}
			if (typesToIgnore == null || !typesToIgnore.Contains(structureTemplateObjectData.tileObjectType))
			{
				TileObject tileObject = InstantiatePreplacedObject(structureTemplateObjectData.tileObjectType, locationGridTile);
				tileObject.SetIsPreplaced(state: true);
				PreplacedObjectProcessing(structureTemplateObjectData, locationGridTile, structure, tileObject);
			}
		}
		SetPreplacedObjectsState(state: false);
	}

	protected virtual TileObject InstantiatePreplacedObject(TILE_OBJECT_TYPE p_type, LocationGridTile p_tile)
	{
		return InnerMapManager.Instance.CreateNewTileObject<TileObject>(p_type);
	}

	protected virtual void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj, LocationGridTile tile, LocationStructure structure, TileObject newTileObject)
	{
		tile.structure.AddPOI(newTileObject, tile);
		newTileObject.mapVisual.SetVisual(preplacedObj.spriteRenderer.sprite);
		newTileObject.mapVisual.SetRotation(preplacedObj.transform.localEulerAngles.z);
		newTileObject.RevalidateTileObjectSlots();
	}

	private StructureTemplateObjectData GetStructureTemplateObjectData(LocationGridTile tile, InnerTileMap areaMap)
	{
		StructureTemplateObjectData[] preplacedObjects = GetPreplacedObjects();
		foreach (StructureTemplateObjectData structureTemplateObjectData in preplacedObjects)
		{
			Vector3Int vector3Int = areaMap.groundTilemap.WorldToCell(structureTemplateObjectData.transform.position);
			if (vector3Int.x == tile.localPlace.x && vector3Int.y == tile.localPlace.y)
			{
				return structureTemplateObjectData;
			}
		}
		return null;
	}

	private StructureTemplateObjectData[] GetPreplacedObjects()
	{
		if (objectsParent != null)
		{
			if (_preplacedObjs == null)
			{
				_preplacedObjs = GameUtilities.GetComponentsInDirectChildren<StructureTemplateObjectData>(objectsParent.gameObject);
			}
			return _preplacedObjs;
		}
		return null;
	}

	public bool HasPreplacedObjectOfType(TILE_OBJECT_TYPE p_tileObjectType)
	{
		StructureTemplateObjectData[] preplacedObjects = GetPreplacedObjects();
		if (preplacedObjects != null)
		{
			for (int i = 0; i < preplacedObjects.Length; i++)
			{
				if (preplacedObjects[i].tileObjectType == p_tileObjectType)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void PopulateMissingPreplacedObjectsOfTypeThatIsOnUnoccupiedTile(List<StructureTemplateObjectData> p_objects, TILE_OBJECT_TYPE p_tileObjectType, InnerTileMap innerMap)
	{
		StructureTemplateObjectData[] preplacedObjects = GetPreplacedObjects();
		if (preplacedObjects == null)
		{
			return;
		}
		foreach (StructureTemplateObjectData structureTemplateObjectData in preplacedObjects)
		{
			if (structureTemplateObjectData.tileObjectType == p_tileObjectType)
			{
				Vector3Int vector3Int = innerMap.groundTilemap.WorldToCell(structureTemplateObjectData.transform.position);
				if (innerMap.map[vector3Int.x, vector3Int.y].tileObjectComponent.objHere == null)
				{
					p_objects.Add(structureTemplateObjectData);
				}
			}
		}
	}

	public void PopulateMissingPreplacedObjectsOfType(List<StructureTemplateObjectData> p_objects, TILE_OBJECT_TYPE p_tileObjectType, InnerTileMap innerMap)
	{
		StructureTemplateObjectData[] preplacedObjects = GetPreplacedObjects();
		if (preplacedObjects == null)
		{
			return;
		}
		foreach (StructureTemplateObjectData structureTemplateObjectData in preplacedObjects)
		{
			if (structureTemplateObjectData.tileObjectType == p_tileObjectType)
			{
				Vector3Int vector3Int = innerMap.groundTilemap.WorldToCell(structureTemplateObjectData.transform.position);
				LocationGridTile locationGridTile = innerMap.map[vector3Int.x, vector3Int.y];
				if (locationGridTile.tileObjectComponent.objHere == null || locationGridTile.tileObjectComponent.objHere.tileObjectType != p_tileObjectType)
				{
					p_objects.Add(structureTemplateObjectData);
				}
			}
		}
	}

	public LocationGridTile GetTileLocationOfPreplacedObject(StructureTemplateObjectData p_templateObject, InnerTileMap innerTileMap)
	{
		Vector3Int vector3Int = innerTileMap.groundTilemap.WorldToCell(p_templateObject.transform.position);
		return innerTileMap.map[vector3Int.x, vector3Int.y];
	}

	private void SetPreplacedObjectsState(bool state)
	{
		StructureTemplateObjectData[] preplacedObjects = GetPreplacedObjects();
		if (preplacedObjects != null)
		{
			for (int i = 0; i < preplacedObjects.Length; i++)
			{
				preplacedObjects[i].gameObject.SetActive(state);
			}
		}
	}

	private void SetPreplacedObjectsColor(Color p_color)
	{
		StructureTemplateObjectData[] preplacedObjects = GetPreplacedObjects();
		if (preplacedObjects != null)
		{
			for (int i = 0; i < preplacedObjects.Length; i++)
			{
				preplacedObjects[i].SetVisualColor(p_color);
			}
		}
	}

	public void ResetWallsBeforePlacement()
	{
		for (int i = 0; i < wallVisuals.Length; i++)
		{
			ThinWallGameObject obj = wallVisuals[i];
			obj.ResetWallAssets(_thinWallResource);
			obj.Reset();
		}
	}

	public void ClearOutUnimportantObjectsBeforePlacement()
	{
		bool flag = structureType.IsPlayerStructure();
		for (int i = 0; i < tiles.Length; i++)
		{
			LocationGridTile locationGridTile = tiles[i];
			StructureTemplateObjectData structureTemplateObjectData = GetStructureTemplateObjectData(locationGridTile, locationGridTile.parentMap);
			TileObject objHere = locationGridTile.tileObjectComponent.objHere;
			if (objHere != null && !(objHere is StructureTileObject))
			{
				if (objHere.traitContainer.HasTrait("Indestructible") && !flag)
				{
					locationGridTile.structure.OnlyRemovePOIFromList(objHere);
				}
				else
				{
					if (flag && objHere is Tombstone tombstone)
					{
						tombstone.SetRespawnCorpseOnDestroy(state: false);
					}
					bool flag2 = !(_blockWallsTilemap == null) && (bool)_blockWallsTilemap.GetTile(_blockWallsTilemap.WorldToCell(locationGridTile.worldLocation));
					if (!objHere.tileObjectType.IsTileObjectImportant() || structureTemplateObjectData != null || flag2 || flag)
					{
						locationGridTile.structure.RemovePOI(objHere);
					}
				}
			}
			if (!GameManager.Instance.gameHasStarted)
			{
				WorldConfigManager.Instance.mapGenerationData?.SetGeneratedMapPerlinDetails(locationGridTile, TILE_OBJECT_TYPE.NONE);
			}
			locationGridTile.parentMap.detailsTilemap.SetTile(locationGridTile.localPlace, null);
			locationGridTile.parentMap.northEdgeTilemap.SetTile(locationGridTile.localPlace, null);
			locationGridTile.parentMap.southEdgeTilemap.SetTile(locationGridTile.localPlace, null);
			locationGridTile.parentMap.eastEdgeTilemap.SetTile(locationGridTile.localPlace, null);
			locationGridTile.parentMap.westEdgeTilemap.SetTile(locationGridTile.localPlace, null);
			List<LocationGridTile> list = locationGridTile.neighbourList.Where((LocationGridTile x) => !tiles.Contains(x)).ToList();
			for (int num = 0; num < list.Count; num++)
			{
				LocationGridTile locationGridTile2 = list[num];
				if (!GameManager.Instance.gameHasStarted)
				{
					WorldConfigManager.Instance.mapGenerationData?.SetGeneratedMapPerlinDetails(locationGridTile2, TILE_OBJECT_TYPE.NONE);
				}
				locationGridTile2.parentMap.detailsTilemap.SetTile(locationGridTile2.localPlace, null);
				if (locationGridTile2.TryGetNeighbourDirection(locationGridTile, out var dir))
				{
					switch (dir)
					{
					case GridNeighbourDirection.North:
						locationGridTile2.parentMap.northEdgeTilemap.SetTile(locationGridTile2.localPlace, null);
						break;
					case GridNeighbourDirection.South:
						locationGridTile2.parentMap.southEdgeTilemap.SetTile(locationGridTile2.localPlace, null);
						break;
					case GridNeighbourDirection.West:
						locationGridTile2.parentMap.westEdgeTilemap.SetTile(locationGridTile2.localPlace, null);
						break;
					case GridNeighbourDirection.East:
						locationGridTile2.parentMap.eastEdgeTilemap.SetTile(locationGridTile2.localPlace, null);
						break;
					}
				}
			}
		}
	}

	public virtual void OnBuiltStructureObjectPlaced(InnerTileMap innerMap, LocationStructure structure, out int createdWalls, out int totalWalls, TILE_OBJECT_TYPE[] objectTypesToNotBuild = null)
	{
		for (int i = 0; i < tiles.Length; i++)
		{
			LocationGridTile locationGridTile = tiles[i];
			ApplyGroundTileAssetForTile(locationGridTile);
			locationGridTile.CreateSeamlessEdgesForSelfAndNeighbours();
			locationGridTile.parentMap.detailsTilemap.SetTile(locationGridTile.localPlace, null);
		}
		ProcessConnectors(structure);
		ProcessWardLightSpots(innerMap, structure);
		RegisterWalls(innerMap, structure, out createdWalls, out totalWalls);
		_groundTileMap.gameObject.SetActive(value: false);
		RegisterPreplacedObjects(structure, innerMap, objectTypesToNotBuild);
		RescanPathfindingGridOfStructure(innerMap);
		UpdateSortingOrders();
		Messenger.Broadcast(StructureSignals.STRUCTURE_OBJECT_PLACED, structure);
	}

	public void OnLoadStructureObjectPlaced(InnerTileMap innerMap, LocationStructure structure, SaveDataLocationStructure saveData)
	{
		if (structure is ManMadeStructure && _blockWallsTilemap == null)
		{
			RegisterWalls(innerMap, structure, out var _, out var _);
		}
		if (saveData is SaveDataManMadeStructure { structureConnectors: not null } saveDataManMadeStructure)
		{
			LoadConnectors(saveDataManMadeStructure.structureConnectors, innerMap);
		}
		_groundTileMap.gameObject.SetActive(value: false);
		if (_blockWallsTilemap != null)
		{
			_blockWallsTilemap.gameObject.SetActive(value: false);
		}
		RescanPathfindingGridOfStructure(innerMap);
		UpdateSortingOrders();
		SetPreplacedObjectsState(state: false);
		SetClickColliderState(p_state: false);
		Messenger.Broadcast(StructureSignals.STRUCTURE_OBJECT_PLACED, structure);
	}

	public void OnOwnerStructureDestroyed(InnerTileMap p_map, LocationStructure p_structure)
	{
		if (p_structure.settlementLocation is NPCSettlement nPCSettlement)
		{
			for (int i = 0; i < _wardLightSpots.Length; i++)
			{
				WardLightSpot wardLightSpot = _wardLightSpots[i];
				LocationGridTile tileFromWorldPosition = p_map.GetTileFromWorldPosition(wardLightSpot.transform.position);
				if (tileFromWorldPosition != null)
				{
					nPCSettlement.tileObjectComponent.RemoveWardLightLocations(tileFromWorldPosition);
				}
			}
		}
		RescanPathfindingGridOfStructure(p_map, 0);
		base.gameObject.SetActive(value: false);
		_parentTemplate.CheckForDestroy();
	}

	public List<LocationGridTile> GetTilesOccupiedByStructure(InnerTileMap map)
	{
		List<LocationGridTile> list = new List<LocationGridTile>();
		Vector3 vector = map.transform.InverseTransformPoint(base.transform.position);
		Vector3Int vector3Int = new Vector3Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), 0);
		for (int i = 0; i < localOccupiedCoordinates.Count; i++)
		{
			Vector3Int vector3Int2 = localOccupiedCoordinates[i];
			Vector3Int vector3Int3 = vector3Int;
			int num = vector3Int2.x - center.x;
			int num2 = vector3Int2.y - center.y;
			vector3Int3.x += num;
			vector3Int3.y += num2;
			if (Utilities.IsInRange(vector3Int3.x, 0, map.width) && Utilities.IsInRange(vector3Int3.y, 0, map.height))
			{
				LocationGridTile item = map.map[vector3Int3.x, vector3Int3.y];
				list.Add(item);
				continue;
			}
			throw new Exception("IndexOutOfRangeException when trying to place structure object " + base.name + " at " + map.region.name);
		}
		return list;
	}

	protected LocationGridTile ConvertLocalPointInStructureToTile(Vector3Int coordinates, InnerTileMap map)
	{
		Vector3 vector = map.transform.InverseTransformPoint(base.transform.position);
		Vector3Int vector3Int = new Vector3Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), 0);
		int num = coordinates.x - center.x;
		int num2 = coordinates.y - center.y;
		vector3Int.x += num;
		vector3Int.y += num2;
		return map.map[vector3Int.x, vector3Int.y];
	}

	public List<LocationGridTile> GetTilesOccupiedByRoom(InnerTileMap map, RoomTemplate roomTemplate)
	{
		List<LocationGridTile> list = new List<LocationGridTile>();
		List<Vector3Int> list2 = new List<Vector3Int>(roomTemplate.coordinatesInRoom);
		Vector3 vector = map.transform.InverseTransformPoint(base.transform.position);
		Vector3Int vector3Int = new Vector3Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), 0);
		for (int i = 0; i < list2.Count; i++)
		{
			Vector3Int vector3Int2 = list2[i];
			Vector3Int vector3Int3 = vector3Int;
			int num = vector3Int2.x - center.x;
			int num2 = vector3Int2.y - center.y;
			vector3Int3.x += num;
			vector3Int3.y += num2;
			if (Utilities.IsInRange(vector3Int3.x, 0, map.width) && Utilities.IsInRange(vector3Int3.y, 0, map.height))
			{
				LocationGridTile item = map.map[vector3Int3.x, vector3Int3.y];
				list.Add(item);
				continue;
			}
			throw new Exception("IndexOutOfRangeException when trying to place structure object " + base.name + " at " + map.region.name);
		}
		return list;
	}

	public void SetVisualMode(Structure_Visual_Mode mode, InnerTileMap innerTileMap)
	{
		Color white = Color.white;
		currentVisualMode = mode;
		switch (mode)
		{
		case Structure_Visual_Mode.Blueprint:
			white.a = 0.5019608f;
			SetStructureColor(white);
			SetPreplacedObjectsState(state: false);
			SetWallCollidersState(state: false);
			SetClickColliderState(p_state: true);
			break;
		case Structure_Visual_Mode.Demonic_Structure_Blueprint:
			white.a = 0.5019608f;
			SetStructureColor(white);
			SetPreplacedObjectsState(state: true);
			SetPreplacedObjectsColor(white);
			SetWallCollidersState(state: false);
			OverrideDefaultSortingOrder(60);
			SetClickColliderState(p_state: true);
			break;
		default:
			white = Color.white;
			SetStructureColor(white);
			SetWallCollidersState(state: true);
			RescanPathfindingGridOfStructure(innerTileMap);
			SetClickColliderState(p_state: false);
			break;
		}
	}

	public void ApplyGroundTileAssetForTile(LocationGridTile tile)
	{
		TileBase groundTileAssetForTile = GetGroundTileAssetForTile(tile);
		if (groundTileAssetForTile != null)
		{
			tile.SetGroundTilemapVisual(groundTileAssetForTile);
		}
	}

	private TileBase GetGroundTileAssetForTile(LocationGridTile tile)
	{
		return _groundTileMap.GetTile(_groundTileMap.WorldToCell(tile.worldLocation));
	}

	public override void Reset()
	{
		base.Reset();
		currentVisualMode = Structure_Visual_Mode.Built;
		SetPreplacedObjectsState(state: true);
		if (_groundTileMap != null)
		{
			_groundTileMap.gameObject.SetActive(value: true);
		}
		if (_blockWallsTilemap != null)
		{
			_blockWallsTilemap.gameObject.SetActive(value: true);
		}
		SetWallCollidersState(state: true);
		tiles = null;
		_preplacedObjs = null;
		for (int i = 0; i < connectors.Length; i++)
		{
			connectors[i].Reset();
		}
		for (int j = 0; j < wallVisuals.Length; j++)
		{
			ThinWallGameObject obj = wallVisuals[j];
			obj.ResetWallAssets(_thinWallResource);
			obj.Reset();
		}
		_groundTileMapRenderer.sortingOrder = 50;
	}

	private void RegisterWalls(InnerTileMap map, LocationStructure structure, out int createdWalls, out int totalWalls)
	{
		createdWalls = 0;
		totalWalls = 0;
		if (_blockWallsTilemap != null)
		{
			_blockWallsTilemap.gameObject.SetActive(value: true);
			for (int i = 0; i < tiles.Length; i++)
			{
				LocationGridTile locationGridTile = tiles[i];
				TileBase tile = _blockWallsTilemap.GetTile(_blockWallsTilemap.WorldToCell(locationGridTile.worldLocation));
				if (!(tile != null))
				{
					continue;
				}
				if (tile.name.Contains("Wall"))
				{
					bool flag = true;
					if (locationGridTile.tileObjectComponent.objHere != null)
					{
						if (locationGridTile.tileObjectComponent.objHere.traitContainer.HasTrait("Indestructible"))
						{
							flag = false;
							locationGridTile.structure.OnlyAddPOIToList(locationGridTile.tileObjectComponent.objHere);
						}
						else
						{
							locationGridTile.structure.RemovePOI(locationGridTile.tileObjectComponent.objHere);
						}
					}
					if (flag)
					{
						createdWalls++;
						BlockWall blockWall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
						blockWall.SetWallType(_blockWallType);
						structure.AddPOI(blockWall, locationGridTile);
						if (wallsContributeToDamage)
						{
							structure.AddObjectAsDamageContributor(blockWall);
						}
					}
					totalWalls++;
				}
				else
				{
					map.elevationTilemap.SetTile(locationGridTile.localPlace, tile);
				}
			}
			_blockWallsTilemap.gameObject.SetActive(value: false);
		}
		else if (wallVisuals != null && wallVisuals.Length != 0 && structure is ManMadeStructure manMadeStructure)
		{
			List<ThinWall> list = new List<ThinWall>();
			for (int j = 0; j < wallVisuals.Length; j++)
			{
				ThinWallGameObject thinWallGameObject = wallVisuals[j];
				ThinWall thinWall = InnerMapManager.Instance.CreateNewTileObject<ThinWall>(TILE_OBJECT_TYPE.THIN_WALL);
				thinWall.SetVisualGO(thinWallGameObject);
				thinWall.SetResourceMadeOf(_thinWallResource);
				thinWall.InitializeThinWall();
				Vector3Int vector3Int = map.groundTilemap.WorldToCell(thinWallGameObject.transform.position);
				LocationGridTile locationGridTile2 = map.map[vector3Int.x, vector3Int.y];
				locationGridTile2.SetTileType(LocationGridTile.Tile_Type.Wall);
				thinWall.SetGridTileLocation(locationGridTile2);
				locationGridTile2.tileObjectComponent.AddWallObject(thinWall);
				thinWallGameObject.UpdateWallState(thinWall);
				createdWalls++;
				totalWalls++;
				list.Add(thinWall);
			}
			manMadeStructure.SetWallObjects(list, _thinWallResource);
		}
	}

	private void SetWallCollidersState(bool state)
	{
		if (_blockWallsTilemapCollider != null)
		{
			_blockWallsTilemapCollider.enabled = state;
		}
		for (int i = 0; i < wallVisuals.Length; i++)
		{
			wallVisuals[i].SetUnpassableColliderState(state);
		}
	}

	[ContextMenu("Rescan Pathfinding Grid Of Structure")]
	public void RescanPathfindingGridOfStructure(InnerTileMap innerTileMap, int tag = 1)
	{
		GraphUpdateObject guo = new GraphUpdateObject(_groundTileMapRenderer.bounds)
		{
			nnConstraint = innerTileMap.onlyUnwalkableGraph
		};
		PathfindingManager.Instance.UpdatePathfindingGraphPartialCoroutine(guo);
		guo = new TagGraphUpdateObject(_groundTileMapRenderer.bounds)
		{
			nnConstraint = innerTileMap.onlyPathfindingGraph,
			updatePhysics = true,
			modifyWalkability = false
		};
		PathfindingManager.Instance.UpdatePathfindingGraphPartialCoroutine(guo);
	}

	public StructureConnector GetFirstValidConnector(List<StructureConnector> connectionChoices, InnerTileMap innerTileMap, BaseSettlement p_settlement, out int usedConnectorIndex, out LocationGridTile tileToPlaceStructure, out LocationGridTile connectorTile, StructureSetting p_structureSetting, out string functionLog)
	{
		string empty = string.Empty;
		for (int i = 0; i < connectionChoices.Count; i++)
		{
			StructureConnector structureConnector = connectionChoices[i];
			if (IsConnectorValid(structureConnector, innerTileMap, p_settlement, out usedConnectorIndex, out tileToPlaceStructure, out connectorTile, p_structureSetting, out functionLog))
			{
				return structureConnector;
			}
		}
		functionLog = empty;
		tileToPlaceStructure = null;
		usedConnectorIndex = -1;
		connectorTile = null;
		return null;
	}

	public bool IsConnectorValid(StructureConnector connectorA, InnerTileMap innerTileMap, BaseSettlement p_settlement, out int usedConnectorIndex, out LocationGridTile tileToPlaceStructure, out LocationGridTile connectorTile, StructureSetting p_structureSetting, out string functionLog)
	{
		string text = string.Empty;
		LocationGridTile tileFromWorldPosition = innerTileMap.GetTileFromWorldPosition(connectorA.transform.position);
		if (tileFromWorldPosition == null)
		{
			functionLog = text;
			tileToPlaceStructure = null;
			usedConnectorIndex = -1;
			connectorTile = null;
			return false;
		}
		for (int i = 0; i < connectors.Length; i++)
		{
			Vector3 localPosition = connectors[i].transform.localPosition;
			Vector2Int vector2Int = new Vector2Int(Mathf.FloorToInt(localPosition.x), Mathf.FloorToInt(localPosition.y));
			Vector2Int vector2Int2 = new Vector2Int(center.x - vector2Int.x, center.y - vector2Int.y);
			Vector2Int vector2Int3 = new Vector2Int(tileFromWorldPosition.localPlace.x + vector2Int2.x, tileFromWorldPosition.localPlace.y + vector2Int2.y);
			LocationGridTile tileFromMapCoordinates = innerTileMap.GetTileFromMapCoordinates(vector2Int3.x, vector2Int3.y);
			if (tileFromMapCoordinates != null)
			{
				bool flag = p_structureSetting.structureType.IsValidCenterTileForStructure(tileFromMapCoordinates, p_settlement);
				string o_cannotPlaceReason = string.Empty;
				if (flag && HasEnoughSpaceIfPlacedOn(tileFromMapCoordinates, out o_cannotPlaceReason))
				{
					tileToPlaceStructure = tileFromMapCoordinates;
					usedConnectorIndex = i;
					connectorTile = innerTileMap.GetTileFromWorldPosition(connectorA.transform.position);
					functionLog = text;
					return true;
				}
				text = $"{text}\n\t- Cannot place {base.name} connector {i} on {connectorA}. isValidCenterTileForStructure: {flag.ToString()} Reason: {o_cannotPlaceReason}";
			}
		}
		functionLog = text;
		tileToPlaceStructure = null;
		usedConnectorIndex = -1;
		connectorTile = null;
		return false;
	}

	public bool HasAffectedCorruptedTilesIfPlacedOn(LocationGridTile centerTile)
	{
		if (centerTile.corruptionComponent.isCorrupted || centerTile.corruptionComponent.isCurrentlyBeingCorrupted)
		{
			return true;
		}
		InnerTileMap parentMap = centerTile.parentMap;
		for (int i = 0; i < localOccupiedCoordinates.Count; i++)
		{
			Vector3Int vector3Int = localOccupiedCoordinates[i];
			Vector3Int localPlace = centerTile.localPlace;
			int num = vector3Int.x - center.x;
			int num2 = vector3Int.y - center.y;
			localPlace.x += num;
			localPlace.y += num2;
			if (Utilities.IsInRange(localPlace.x, 0, parentMap.width) && Utilities.IsInRange(localPlace.y, 0, parentMap.height))
			{
				LocationGridTile locationGridTile = parentMap.map[localPlace.x, localPlace.y];
				if (locationGridTile.corruptionComponent.isCorrupted || locationGridTile.corruptionComponent.isCurrentlyBeingCorrupted)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasEnoughSpaceIfPlacedOn(LocationGridTile centerTile)
	{
		if (!CanPlaceStructureOnTile(centerTile, out var o_cannotPlaceReason))
		{
			return false;
		}
		InnerTileMap parentMap = centerTile.parentMap;
		for (int i = 0; i < localOccupiedCoordinates.Count; i++)
		{
			Vector3Int vector3Int = localOccupiedCoordinates[i];
			Vector3Int localPlace = centerTile.localPlace;
			int num = vector3Int.x - center.x;
			int num2 = vector3Int.y - center.y;
			localPlace.x += num;
			localPlace.y += num2;
			if (Utilities.IsInRange(localPlace.x, 0, parentMap.width) && Utilities.IsInRange(localPlace.y, 0, parentMap.height))
			{
				LocationGridTile tile = parentMap.map[localPlace.x, localPlace.y];
				if (!CanPlaceStructureOnTile(tile, out o_cannotPlaceReason))
				{
					return false;
				}
				continue;
			}
			return false;
		}
		return true;
	}

	public bool HasEnoughSpaceIfPlacedOn(LocationGridTile centerTile, out string o_cannotPlaceReason)
	{
		if (!CanPlaceStructureOnTile(centerTile, out o_cannotPlaceReason))
		{
			return false;
		}
		InnerTileMap parentMap = centerTile.parentMap;
		for (int i = 0; i < localOccupiedCoordinates.Count; i++)
		{
			Vector3Int vector3Int = localOccupiedCoordinates[i];
			Vector3Int localPlace = centerTile.localPlace;
			int num = vector3Int.x - center.x;
			int num2 = vector3Int.y - center.y;
			localPlace.x += num;
			localPlace.y += num2;
			if (Utilities.IsInRange(localPlace.x, 0, parentMap.width) && Utilities.IsInRange(localPlace.y, 0, parentMap.height))
			{
				LocationGridTile tile = parentMap.map[localPlace.x, localPlace.y];
				if (!CanPlaceStructureOnTile(tile, out o_cannotPlaceReason))
				{
					return false;
				}
				continue;
			}
			return false;
		}
		return true;
	}

	private bool CanPlaceStructureOnTile(LocationGridTile tile, out string o_cannotPlaceReason)
	{
		if (tile.structure.structureType != STRUCTURE_TYPE.WILDERNESS)
		{
			o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_not_wilderness");
			return false;
		}
		if (tile.elevationType == ELEVATION.WATER || tile.elevationType == ELEVATION.MOUNTAIN)
		{
			o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_neighbour_not_wilderness");
			return false;
		}
		if (tile.hasBlueprint)
		{
			o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_has_blueprint");
			return false;
		}
		if (tile.IsAtEdgeOfMap())
		{
			o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_edge");
			return false;
		}
		if (!AreTilesInRadiusTileValidForStructurePlacement(structureType, tile, out o_cannotPlaceReason))
		{
			return false;
		}
		o_cannotPlaceReason = string.Empty;
		return true;
	}

	public static bool AreTilesInRadiusTileValidForStructurePlacement(STRUCTURE_TYPE p_structureType, LocationGridTile p_tile, out string o_cannotPlaceReason)
	{
		List<LocationGridTile> list = null;
		List<LocationGridTile> list2;
		if (p_structureType.IsPlayerStructure())
		{
			list2 = p_tile.neighbourList;
		}
		else
		{
			list = RuinarchListPool<LocationGridTile>.Claim();
			int radius = 2;
			p_tile.PopulateTilesInRadius(list, radius, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
			list2 = list;
		}
		bool flag = true;
		o_cannotPlaceReason = string.Empty;
		for (int i = 0; i < list2.Count; i++)
		{
			LocationGridTile locationGridTile = list2[i];
			if (locationGridTile.hasBlueprint)
			{
				o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_has_blueprint");
				flag = false;
				break;
			}
			switch (p_structureType)
			{
			case STRUCTURE_TYPE.MINE:
				if (locationGridTile.structure.structureType == STRUCTURE_TYPE.WILDERNESS || locationGridTile.structure.structureType == STRUCTURE_TYPE.CITY_CENTER || locationGridTile.structure.structureType == STRUCTURE_TYPE.CAVE)
				{
					continue;
				}
				o_cannotPlaceReason = string.Empty;
				flag = false;
				break;
			case STRUCTURE_TYPE.FISHERY:
				if (locationGridTile.structure.structureType == STRUCTURE_TYPE.WILDERNESS || locationGridTile.structure.structureType == STRUCTURE_TYPE.CITY_CENTER || locationGridTile.structure.structureType == STRUCTURE_TYPE.OCEAN)
				{
					continue;
				}
				o_cannotPlaceReason = string.Empty;
				flag = false;
				break;
			default:
				if (p_structureType.IsPlayerStructure())
				{
					if (locationGridTile.structure.structureType == STRUCTURE_TYPE.WILDERNESS || locationGridTile.structure.structureType == STRUCTURE_TYPE.CAVE || locationGridTile.structure.structureType == STRUCTURE_TYPE.OCEAN || locationGridTile.structure.structureType.IsPlayerStructure())
					{
						continue;
					}
					o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_neighbour_not_wilderness");
					flag = false;
				}
				else
				{
					if (locationGridTile.structure.structureType == STRUCTURE_TYPE.WILDERNESS || locationGridTile.structure.structureType == STRUCTURE_TYPE.CITY_CENTER)
					{
						continue;
					}
					o_cannotPlaceReason = string.Empty;
					flag = false;
				}
				break;
			}
			break;
		}
		if (list != null)
		{
			RuinarchListPool<LocationGridTile>.Release(list);
		}
		if (flag && p_structureType != STRUCTURE_TYPE.CITY_CENTER && p_structureType != STRUCTURE_TYPE.MINE && p_structureType != STRUCTURE_TYPE.FISHERY && p_structureType.IsVillageStructure())
		{
			list = RuinarchListPool<LocationGridTile>.Claim();
			int radius2 = 5;
			p_tile.PopulateTilesInRadius(list, radius2, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
			for (int j = 0; j < list.Count; j++)
			{
				LocationGridTile locationGridTile2 = list[j];
				if (locationGridTile2.tileObjectComponent.objHere is FishingSpot || locationGridTile2.tileObjectComponent.objHere is OreVein || locationGridTile2.tileObjectComponent.genericTileObject.structureConnector != null)
				{
					o_cannotPlaceReason = string.Empty;
					return false;
				}
			}
			RuinarchListPool<LocationGridTile>.Release(list);
		}
		return flag;
	}

	private void ProcessConnectors(LocationStructure structure)
	{
		for (int i = 0; i < _connectors.Length; i++)
		{
			StructureConnector obj = _connectors[i];
			obj.OnPlaceConnector(structure.region.innerMap);
			obj.SetIsPartOfLocationStructureObject(p_state: true);
		}
	}

	private void ProcessWardLightSpots(InnerTileMap p_map, LocationStructure p_structure)
	{
		if (!(p_structure.settlementLocation is NPCSettlement nPCSettlement))
		{
			return;
		}
		for (int i = 0; i < _wardLightSpots.Length; i++)
		{
			WardLightSpot wardLightSpot = _wardLightSpots[i];
			LocationGridTile tileFromWorldPosition = p_map.GetTileFromWorldPosition(wardLightSpot.transform.position);
			if (tileFromWorldPosition != null)
			{
				nPCSettlement.tileObjectComponent.AddWardLightLocation(tileFromWorldPosition);
			}
		}
	}

	private void LoadConnectors(SaveDataStructureConnector[] connectorSaves, InnerTileMap innerTileMap)
	{
		for (int i = 0; i < _connectors.Length; i++)
		{
			StructureConnector obj = _connectors[i];
			SaveDataStructureConnector saveData = connectorSaves[i];
			obj.LoadReferencesForStructureObjects(saveData, innerTileMap);
		}
	}

	[ContextMenu("Convert Walls")]
	public void ConvertWalls()
	{
		Utilities.DestroyChildren(wallTileMap.transform);
		wallTileMap.CompressBounds();
		BoundsInt cellBounds = wallTileMap.cellBounds;
		for (int i = cellBounds.xMin; i < cellBounds.xMax; i++)
		{
			for (int j = cellBounds.yMin; j < cellBounds.yMax; j++)
			{
				Vector3Int vector3Int = new Vector3Int(i, j, 0);
				TileBase tile = wallTileMap.GetTile(vector3Int);
				Vector3 vector = wallTileMap.CellToWorld(vector3Int);
				if (!(tile != null))
				{
					continue;
				}
				Vector2 vector2 = new Vector2(vector.x + 0.5f, vector.y + 0.5f);
				if (tile.name.Contains("Door"))
				{
					continue;
				}
				ThinWallGameObject thinWallGameObject = null;
				if (tile.name.Contains("Left"))
				{
					thinWallGameObject = InstantiateWall(leftWall, vector2, wallTileMap.transform, _thinWallResource != WALL_RESOURCE.Wood);
				}
				if (tile.name.Contains("Right"))
				{
					thinWallGameObject = InstantiateWall(rightWall, vector2, wallTileMap.transform, _thinWallResource != WALL_RESOURCE.Wood);
				}
				if (tile.name.Contains("Bot"))
				{
					thinWallGameObject = InstantiateWall(bottomWall, vector2, wallTileMap.transform, _thinWallResource != WALL_RESOURCE.Wood);
				}
				if (tile.name.Contains("Top"))
				{
					thinWallGameObject = InstantiateWall(topWall, vector2, wallTileMap.transform, _thinWallResource != WALL_RESOURCE.Wood);
				}
				if (!(thinWallGameObject != null))
				{
					continue;
				}
				Vector3 p_pos = vector2;
				if (tile.name.Contains("BotLeft"))
				{
					p_pos.x -= 0.5f;
					p_pos.y -= 0.5f;
					InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					Vector3Int position = new Vector3Int(i, j + 1, 0);
					if (wallTileMap.GetTile(position) == null)
					{
						p_pos = vector2;
						p_pos.x -= 0.5f;
						p_pos.y += 0.5f;
						InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					}
					Vector3Int position2 = new Vector3Int(i + 1, j, 0);
					if (wallTileMap.GetTile(position2) == null)
					{
						p_pos = vector2;
						p_pos.x += 0.5f;
						p_pos.y -= 0.5f;
						InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					}
				}
				else if (tile.name.Contains("BotRight"))
				{
					p_pos.x += 0.5f;
					p_pos.y -= 0.5f;
					InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					Vector3Int position3 = new Vector3Int(i, j + 1, 0);
					if (wallTileMap.GetTile(position3) == null)
					{
						p_pos = vector2;
						p_pos.x += 0.5f;
						p_pos.y += 0.5f;
						InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					}
					Vector3Int position4 = new Vector3Int(i - 1, j, 0);
					if (wallTileMap.GetTile(position4) == null)
					{
						p_pos = vector2;
						p_pos.x -= 0.5f;
						p_pos.y -= 0.5f;
						InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					}
				}
				else if (tile.name.Contains("TopLeft"))
				{
					p_pos.x -= 0.5f;
					p_pos.y += 0.5f;
					InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					Vector3Int position5 = new Vector3Int(i, j - 1, 0);
					if (wallTileMap.GetTile(position5) == null)
					{
						p_pos = vector2;
						p_pos.x -= 0.5f;
						p_pos.y -= 0.5f;
						InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					}
					Vector3Int position6 = new Vector3Int(i + 1, j, 0);
					if (wallTileMap.GetTile(position6) == null)
					{
						p_pos = vector2;
						p_pos.x += 0.5f;
						p_pos.y += 0.5f;
						InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					}
				}
				else if (tile.name.Contains("TopRight"))
				{
					p_pos.x += 0.5f;
					p_pos.y += 0.5f;
					InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					Vector3Int position7 = new Vector3Int(i, j - 1, 0);
					if (wallTileMap.GetTile(position7) == null)
					{
						p_pos = vector2;
						p_pos.x += 0.5f;
						p_pos.y -= 0.5f;
						InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					}
					Vector3Int position8 = new Vector3Int(i - 1, j, 0);
					if (wallTileMap.GetTile(position8) == null)
					{
						p_pos = vector2;
						p_pos.x -= 0.5f;
						p_pos.y += 0.5f;
						InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					}
				}
				else if (tile.name.Contains("Left") || tile.name.Contains("Right"))
				{
					if (tile.name.Contains("Right"))
					{
						p_pos.x = vector2.x + 0.5f;
					}
					else
					{
						p_pos.x = vector2.x - 0.5f;
					}
					Vector3Int position9 = new Vector3Int(i, j + 1, 0);
					if (wallTileMap.GetTile(position9) == null)
					{
						p_pos.y = vector2.y + 0.5f;
						InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					}
					Vector3Int position10 = new Vector3Int(i, j - 1, 0);
					if (wallTileMap.GetTile(position10) == null)
					{
						p_pos.y = vector2.y - 0.5f;
						InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					}
				}
				else if (tile.name.Contains("Top") || tile.name.Contains("Bot"))
				{
					if (tile.name.Contains("Top"))
					{
						p_pos.y = vector2.y + 0.5f;
					}
					else
					{
						p_pos.y = vector2.y - 0.5f;
					}
					Vector3Int position11 = new Vector3Int(i + 1, j, 0);
					if (wallTileMap.GetTile(position11) == null)
					{
						p_pos.x = vector2.x + 0.5f;
						InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					}
					Vector3Int position12 = new Vector3Int(i - 1, j, 0);
					if (wallTileMap.GetTile(position12) == null)
					{
						p_pos.x = vector2.x - 0.5f;
						InstantiateCorner(p_pos, thinWallGameObject.transform, thinWallGameObject);
					}
				}
				thinWallGameObject.UpdateWallAssets(_thinWallResource);
			}
		}
		wallTileMap.enabled = false;
		wallTileMap.GetComponent<TilemapRenderer>().enabled = false;
	}

	[ContextMenu("Update Center Based on Size")]
	public void UpdateCenter()
	{
		float f = (float)size.x / 2f;
		float f2 = (float)size.y / 2f;
		Vector3Int zero = Vector3Int.zero;
		zero.x = Mathf.FloorToInt(f);
		zero.y = Mathf.FloorToInt(f2);
		_center = zero;
		base.transform.Find("Content").transform.localPosition = new Vector3(((float)center.x + 0.5f) * -1f, ((float)center.y + 0.5f) * -1f, 0f);
	}

	private ThinWallGameObject InstantiateWall(GameObject wallPrefab, Vector3 centeredPos, Transform parent, bool updateWallAsset)
	{
		return null;
	}

	private void InstantiateCorner(Vector3 p_pos, Transform parent, ThinWallGameObject p_wallVisual)
	{
	}

	[ContextMenu("Convert Objects")]
	public void ConvertObjects()
	{
		Utilities.DestroyChildren(objectsParent);
		_detailTileMap.CompressBounds();
		BoundsInt cellBounds = _detailTileMap.cellBounds;
		for (int i = cellBounds.xMin; i < cellBounds.xMax; i++)
		{
			for (int j = cellBounds.yMin; j < cellBounds.yMax; j++)
			{
				Vector3Int vector3Int = new Vector3Int(i, j, 0);
				TileBase tile = _detailTileMap.GetTile(vector3Int);
				Vector3 vector = _detailTileMap.CellToWorld(vector3Int);
				if (tile != null)
				{
					Matrix4x4 transformMatrix = _detailTileMap.GetTransformMatrix(vector3Int);
					Vector2 vector2 = new Vector2(vector.x + 0.5f, vector.y + 0.5f);
					GameObject obj = new GameObject("StructureTemplateObjectData");
					obj.layer = LayerMask.NameToLayer("Area Maps");
					obj.transform.SetParent(objectsParent);
					obj.transform.position = vector2;
					obj.transform.localRotation = transformMatrix.rotation;
					StructureTemplateObjectData structureTemplateObjectData = obj.AddComponent<StructureTemplateObjectData>();
					SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
					spriteRenderer.sortingLayerName = "Area Maps";
					spriteRenderer.sortingOrder = 60;
					int num = tile.name.IndexOf("#", StringComparison.Ordinal);
					string text = tile.name;
					if (num != -1)
					{
						text = tile.name.Substring(0, num);
					}
					text = text.ToUpper();
					TILE_OBJECT_TYPE tileObjectType = (TILE_OBJECT_TYPE)Enum.Parse(typeof(TILE_OBJECT_TYPE), text);
					structureTemplateObjectData.tileObjectType = tileObjectType;
					structureTemplateObjectData.spriteRenderer = spriteRenderer;
					spriteRenderer.sprite = _detailTileMap.GetSprite(vector3Int);
				}
			}
		}
		_detailTileMap.enabled = false;
		_detailTileMapRenderer.enabled = false;
	}

	[ContextMenu("Predetermine Structure Tiles")]
	public void PredetermineOccupiedCoordinates()
	{
		BoundsInt cellBounds = _groundTileMap.cellBounds;
		_predeterminedOccupiedCoordinates = new List<Vector3Int>();
		for (int i = cellBounds.xMin; i < cellBounds.xMax; i++)
		{
			for (int j = cellBounds.yMin; j < cellBounds.yMax; j++)
			{
				Vector3Int vector3Int = new Vector3Int(i, j, 0);
				TileBase tile = _groundTileMap.GetTile(vector3Int);
				if (tile != null && assetsToConsiderAsStructure.Contains(tile))
				{
					_predeterminedOccupiedCoordinates.Add(vector3Int);
				}
			}
		}
	}

	[ContextMenu("Log Ground Tile Map Assets")]
	public void LogGroundTileMapAssets()
	{
		BoundsInt cellBounds = _groundTileMap.cellBounds;
		for (int i = cellBounds.xMin; i < cellBounds.xMax; i++)
		{
			for (int j = cellBounds.yMin; j < cellBounds.yMax; j++)
			{
				Vector3Int position = new Vector3Int(i, j, 0);
				_groundTileMap.GetTile(position);
			}
		}
	}

	[ContextMenu("Determine Border Coordinates")]
	public void DetermineBorderCoordinates()
	{
		_borderCoordinates = new List<Vector2Int>();
		if (_predeterminedOccupiedCoordinates.Count > 0)
		{
			for (int i = 0; i < _predeterminedOccupiedCoordinates.Count; i++)
			{
				Vector3Int vector3Int = _predeterminedOccupiedCoordinates[i];
				Point[] possibleGridNeighbours = Utilities.PossibleGridNeighbours;
				for (int j = 0; j < possibleGridNeighbours.Length; j++)
				{
					Point point = possibleGridNeighbours[j];
					int x = vector3Int.x + point.X;
					int y = vector3Int.y + point.Y;
					Vector3Int vector3Int2 = new Vector3Int(x, y, 0);
					if (!HasPredeterminedCoordinate(vector3Int2))
					{
						Vector2Int vector2Int = new Vector2Int(vector3Int2.x, vector3Int2.y);
						if (!HasBorderCoordinate(vector2Int))
						{
							_borderCoordinates.Add(vector2Int);
							Debug.Log($"{vector3Int2}");
						}
					}
				}
			}
			return;
		}
		BoundsInt cellBounds = _groundTileMap.cellBounds;
		for (int k = cellBounds.xMin; k < cellBounds.xMax; k++)
		{
			for (int l = cellBounds.yMin; l < cellBounds.yMax; l++)
			{
				Vector3Int position = new Vector3Int(k, l, 0);
				_groundTileMap.GetTile(position);
			}
		}
	}

	private bool HasBorderCoordinate(Vector2Int p_vInt)
	{
		for (int i = 0; i < _borderCoordinates.Count; i++)
		{
			if (_borderCoordinates[i].x == p_vInt.x && _borderCoordinates[i].y == p_vInt.y)
			{
				return true;
			}
		}
		return false;
	}

	private bool HasPredeterminedCoordinate(Vector3Int p_vInt)
	{
		for (int i = 0; i < _predeterminedOccupiedCoordinates.Count; i++)
		{
			if (_predeterminedOccupiedCoordinates[i].x == p_vInt.x && _predeterminedOccupiedCoordinates[i].y == p_vInt.y)
			{
				return true;
			}
		}
		return false;
	}

	[ContextMenu("Update Wall Collider Sizes")]
	private void UpdateWallColliderSizes()
	{
		ThinWallGameObject[] componentsInChildren = wallTileMap.transform.GetComponentsInChildren<ThinWallGameObject>();
		foreach (ThinWallGameObject thinWallGameObject in componentsInChildren)
		{
			if (thinWallGameObject.IsVerticalWall())
			{
				thinWallGameObject.SetSizeAndOffsetOfUnpassableColliders(Vector2.zero, new Vector2(0.3f, 1f));
			}
			else if (thinWallGameObject.IsHorizontalWall())
			{
				thinWallGameObject.SetSizeAndOffsetOfUnpassableColliders(Vector2.zero, new Vector2(1f, 0.3f));
			}
		}
	}

	private void SetClickColliderState(bool p_state)
	{
		if (_clickCollider != null)
		{
			if (p_state)
			{
				_clickCollider.Enable();
			}
			else
			{
				_clickCollider.Disable();
			}
		}
	}

	public bool IsCurrentlySelected()
	{
		if (UIManager.Instance.unbuiltStructureInfoUI.isShowing)
		{
			return UIManager.Instance.unbuiltStructureInfoUI.activeStructureObject == this;
		}
		return false;
	}

	public void LeftSelectAction()
	{
		UIManager.Instance.ShowUnbuiltStructureInfo(this);
	}

	public void RightSelectAction()
	{
	}

	public void MiddleSelectAction()
	{
	}

	public bool CanBeSelected()
	{
		return currentVisualMode != Structure_Visual_Mode.Built;
	}
}
