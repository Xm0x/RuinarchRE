using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Cellular_Automata;
using Inner_Maps.Grid_Tile_Features;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Pathfinding;
using Perlin_Noise;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UtilityScripts;

namespace Inner_Maps;

public abstract class InnerTileMap : BaseMonoBehaviour
{
	private struct BiomeOrder
	{
		public BIOMES[] biomesInOrder;

		public override string ToString()
		{
			return biomesInOrder.ComafyList();
		}
	}

	[Header("Tile Maps")]
	[SerializeField]
	private Tilemap[] _allTilemaps;

	public Tilemap groundTilemap;

	public TilemapRenderer groundTilemapRenderer;

	public Tilemap detailsTilemap;

	public TilemapRenderer detailsTilemapRenderer;

	public Tilemap elevationTilemap;

	public TilemapRenderer elevationTilemapRenderer;

	public Tilemap shoreTilemap;

	public TilemapRenderer shoreTilemapRenderer;

	public Tilemap upperGroundTilemap;

	public TilemapRenderer upperGroundTilemapRenderer;

	public Tilemap perlinTilemap;

	public Tilemap minimapTilemap;

	public Transform tilemapsParent;

	[Header("Seamless Edges")]
	public Tilemap northEdgeTilemap;

	public TilemapRenderer northEdgeTilemapRenderer;

	public Tilemap southEdgeTilemap;

	public TilemapRenderer southEdgeTilemapRenderer;

	public Tilemap westEdgeTilemap;

	public TilemapRenderer westEdgeTilemapRenderer;

	public Tilemap eastEdgeTilemap;

	public TilemapRenderer eastEdgeTilemapRenderer;

	[Header("Parents")]
	public Transform objectsParent;

	public Transform structureParent;

	[FormerlySerializedAs("worldUICanvas")]
	public Canvas worldUiCanvas;

	public Grid grid;

	[Header("Other")]
	[FormerlySerializedAs("centerGOPrefab")]
	public GameObject centerGoPrefab;

	public Vector4 cameraBounds;

	[Header("Structures")]
	[SerializeField]
	protected GameObject areaItemPrefab;

	[Header("Perlin Noise")]
	[SerializeField]
	private float _xSeed;

	[SerializeField]
	private float _ySeed;

	[NonSerialized]
	public PerlinNoiseSettings elevationPerlinSettings = new PerlinNoiseSettings
	{
		noiseScale = 34.15f,
		octaves = 3,
		persistance = 0.2f,
		lacunarity = 2f,
		offset = new Vector2(13.09f, 12f),
		regions = new PerlinNoiseRegion[6]
		{
			new PerlinNoiseRegion
			{
				name = "Water_Deep",
				height = 0.1f
			},
			new PerlinNoiseRegion
			{
				name = "Water_Mid",
				height = 0.13f
			},
			new PerlinNoiseRegion
			{
				name = "Water_Shallow",
				height = 0.18f
			},
			new PerlinNoiseRegion
			{
				name = "Water_Shore",
				height = 0.23f
			},
			new PerlinNoiseRegion
			{
				name = "Plain",
				height = 0.78f
			},
			new PerlinNoiseRegion
			{
				name = "Cave",
				height = 1f
			}
		}
	};

	[NonSerialized]
	public PerlinNoiseSettings precipitationPerlinSettings = new PerlinNoiseSettings
	{
		noiseScale = 40.4f,
		octaves = 3,
		persistance = 0f,
		lacunarity = 2f,
		offset = new Vector2(13.09f, 12f)
	};

	[SerializeField]
	private WhittakerDiagram whittakerDiagram;

	[Header("Temperature")]
	public Gradient_Direction temperatureGradient;

	public float warpNoiseScale = 2f;

	public float warpSeed;

	public float warpStrength = 0.2f;

	public float warpWeight = 0.6f;

	public float temperatureSeed;

	[Header("Minimap")]
	public Camera minimapCamera;

	[SerializeField]
	private SpriteRenderer minimapPlayerCameraVisual;

	[SerializeField]
	private BoxCollider minimapCollider;

	[SerializeField]
	private RenderTexture minimapRenderTexture;

	[Header("For Testing")]
	[SerializeField]
	protected LineRenderer pathLineRenderer;

	[SerializeField]
	protected BoundDrawer _boundDrawer;

	public int width;

	public int height;

	private readonly BiomeOrder[] _biomeOrders = new BiomeOrder[4]
	{
		new BiomeOrder
		{
			biomesInOrder = new BIOMES[3]
			{
				BIOMES.DESERT,
				BIOMES.GRASSLAND,
				BIOMES.FOREST
			}
		},
		new BiomeOrder
		{
			biomesInOrder = new BIOMES[3]
			{
				BIOMES.DESERT,
				BIOMES.FOREST,
				BIOMES.SNOW
			}
		},
		new BiomeOrder
		{
			biomesInOrder = new BIOMES[3]
			{
				BIOMES.DESERT,
				BIOMES.GRASSLAND,
				BIOMES.SNOW
			}
		},
		new BiomeOrder
		{
			biomesInOrder = new BIOMES[3]
			{
				BIOMES.GRASSLAND,
				BIOMES.FOREST,
				BIOMES.SNOW
			}
		}
	};

	private readonly TILE_OBJECT_TYPE[] desertDecorChoices = new TILE_OBJECT_TYPE[4]
	{
		TILE_OBJECT_TYPE.FLOWER,
		TILE_OBJECT_TYPE.PLANT,
		TILE_OBJECT_TYPE.ROCK,
		TILE_OBJECT_TYPE.TRASH
	};

	private readonly TILE_OBJECT_TYPE[] snowDecorChoices = new TILE_OBJECT_TYPE[3]
	{
		TILE_OBJECT_TYPE.PLANT,
		TILE_OBJECT_TYPE.ROCK,
		TILE_OBJECT_TYPE.TRASH
	};

	private readonly TILE_OBJECT_TYPE[] grasslandDecorChoices = new TILE_OBJECT_TYPE[4]
	{
		TILE_OBJECT_TYPE.FLOWER,
		TILE_OBJECT_TYPE.PLANT,
		TILE_OBJECT_TYPE.ROCK,
		TILE_OBJECT_TYPE.TRASH
	};

	public LocationGridTile[,] map { get; private set; }

	public List<LocationGridTile> allTiles { get; private set; }

	public List<LocationGridTile> allEdgeTiles { get; private set; }

	public Region region { get; private set; }

	public GridGraph pathfindingGraph { get; set; }

	public GridGraph unwalkableGraph { get; set; }

	public Vector3 worldPos { get; private set; }

	public GameObject centerGo { get; private set; }

	public NNConstraint onlyUnwalkableGraph { get; private set; }

	public NNConstraint onlyPathfindingGraph { get; private set; }

	public Tile_Tag[,] tileTagMap { get; private set; }

	public bool isShowing => InnerMapManager.Instance.currentlyShowingMap == this;

	public float xSeed => _xSeed;

	public float ySeed => _ySeed;

	public void Initialize(Region location, float xSeed, float ySeed, PerlinNoiseSettings elevationSettings, float p_warpWeight, float p_temperatureSeed)
	{
		region = location;
		_xSeed = xSeed;
		_ySeed = ySeed;
		warpSeed = xSeed;
		precipitationPerlinSettings.seed = (int)ySeed;
		elevationPerlinSettings = elevationSettings;
		warpWeight = p_warpWeight;
		temperatureSeed = p_temperatureSeed;
		groundTilemapRenderer.sortingOrder = 10;
		detailsTilemapRenderer.sortingOrder = 40;
		elevationTilemapRenderer.sortingOrder = 30;
		shoreTilemapRenderer.sortingOrder = 29;
		northEdgeTilemapRenderer.sortingOrder = 31;
		southEdgeTilemapRenderer.sortingOrder = 31;
		westEdgeTilemapRenderer.sortingOrder = 32;
		eastEdgeTilemapRenderer.sortingOrder = 32;
		upperGroundTilemapRenderer.sortingOrder = 11;
		Messenger.AddListener<Camera, float>(ControlsSignals.CAMERA_ZOOM_CHANGED, UpdateOrtographicSize);
		perlinTilemap.gameObject.SetActive(value: false);
	}

	public void Initialize(Region location, float xSeed, float ySeed, int biomeSeed, int elevationSeed)
	{
		region = location;
		_xSeed = xSeed;
		_ySeed = ySeed;
		warpSeed = xSeed;
		precipitationPerlinSettings.seed = (int)ySeed;
		temperatureSeed = UnityEngine.Random.Range(0f, 0.25f);
		if (GameUtilities.RollChance(50))
		{
			warpWeight = -0.39f;
		}
		else
		{
			warpWeight = 0.39f;
		}
		elevationPerlinSettings.seed = elevationSeed;
		groundTilemapRenderer.sortingOrder = 10;
		detailsTilemapRenderer.sortingOrder = 40;
		elevationTilemapRenderer.sortingOrder = 30;
		shoreTilemapRenderer.sortingOrder = 29;
		northEdgeTilemapRenderer.sortingOrder = 31;
		southEdgeTilemapRenderer.sortingOrder = 31;
		westEdgeTilemapRenderer.sortingOrder = 32;
		eastEdgeTilemapRenderer.sortingOrder = 32;
		upperGroundTilemapRenderer.sortingOrder = 11;
		Messenger.AddListener<Camera, float>(ControlsSignals.CAMERA_ZOOM_CHANGED, UpdateOrtographicSize);
		perlinTilemap.gameObject.SetActive(value: false);
	}

	protected IEnumerator GenerateGrid(int width, int height, MapGenerationComponent mapGenerationComponent, Stopwatch stopwatch)
	{
		stopwatch.Reset();
		stopwatch.Start();
		this.width = width;
		this.height = height;
		map = new LocationGridTile[width, height];
		allTiles = new List<LocationGridTile>();
		allEdgeTiles = new List<LocationGridTile>();
		int batchCount = 0;
		LocationStructure wilderness = region.wilderness;
		Vector3Int[] positionArray = new Vector3Int[width * height];
		TileBase[] groundTilesArray = new TileBase[width * height];
		int count = 0;
		TileBase regionOutsideTile = InnerMapManager.Instance.assetManager.outsideTile;
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Vector3Int vector3Int = new Vector3Int(x, y, 0);
				positionArray[count] = vector3Int;
				groundTilesArray[count] = regionOutsideTile;
				Area area = DetermineAreaGivenCoordinates(x, y);
				LocationGridTile locationGridTile = new LocationGridTile(x, y, groundTilemap, this, area);
				area.gridTileComponent.AddGridTile(locationGridTile);
				area.elevationComponent.OnTileAddedToArea(locationGridTile);
				area.biomeComponent.OnTileAddedToArea(locationGridTile);
				locationGridTile.tileObjectComponent.CreateGenericTileObject();
				locationGridTile.SetStructure(wilderness);
				locationGridTile.tileObjectComponent.genericTileObject.ManualInitialize(locationGridTile);
				allTiles.Add(locationGridTile);
				if (locationGridTile.IsAtEdgeOfWalkableMap())
				{
					allEdgeTiles.Add(locationGridTile);
				}
				map[x, y] = locationGridTile;
				count++;
				batchCount++;
				if (batchCount == MapGenerationData.InnerMapTileGenerationBatches)
				{
					batchCount = 0;
					yield return null;
				}
			}
		}
		groundTilemap.SetTiles(positionArray, groundTilesArray);
		stopwatch.Stop();
		mapGenerationComponent.AddLog(region.name + " GenerateGrid took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
		stopwatch.Reset();
		stopwatch.Start();
		Parallel.ForEach(allTiles, delegate(LocationGridTile currentTile)
		{
			currentTile.FindNeighbours(map);
		});
		stopwatch.Stop();
		mapGenerationComponent.AddLog(region.name + " GridFindNeighbours took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
	}

	private Area DetermineAreaGivenCoordinates(int x, int y)
	{
		int num = Mathf.FloorToInt((float)x / (float)InnerMapManager.AreaLocationGridTileSize.x);
		int num2 = Mathf.FloorToInt((float)y / (float)InnerMapManager.AreaLocationGridTileSize.y);
		return GridMap.Instance.map[num, num2];
	}

	protected IEnumerator LoadGrid(int width, int height, MapGenerationComponent mapGenerationComponent, SaveDataInnerMap saveDataInnerMap, SaveDataCurrentProgress saveData)
	{
		this.width = width;
		this.height = height;
		map = new LocationGridTile[width, height];
		allTiles = new List<LocationGridTile>();
		allEdgeTiles = new List<LocationGridTile>();
		int batchCount = 0;
		Vector3Int[] positionArray = new Vector3Int[width * height];
		TileBase[] groundTilesArray = new TileBase[width * height];
		int count = 0;
		LocationStructure wilderness = region.wilderness;
		TileBase regionOutsideTile = InnerMapManager.Instance.assetManager.outsideTile;
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				SaveDataLocationGridTile saveDataForTile = saveDataInnerMap.GetSaveDataForTile(new Point(x, y));
				Vector3Int vector3Int = new Vector3Int(x, y, 0);
				positionArray[count] = vector3Int;
				groundTilesArray[count] = regionOutsideTile;
				Area area = DetermineAreaGivenCoordinates(x, y);
				bool num = saveDataForTile != null;
				LocationGridTile locationGridTile;
				if (num)
				{
					locationGridTile = saveDataForTile.InitialLoad(groundTilemap, this, saveData, area);
				}
				else
				{
					locationGridTile = new LocationGridTile(x, y, groundTilemap, this, area);
					locationGridTile.tileObjectComponent.CreateGenericTileObject();
				}
				area.gridTileComponent.AddGridTile(locationGridTile);
				area.elevationComponent.OnTileAddedToArea(locationGridTile);
				area.biomeComponent.OnTileAddedToArea(locationGridTile);
				locationGridTile.SetStructure(wilderness);
				if (!num)
				{
					locationGridTile.tileObjectComponent.genericTileObject.ManualInitialize(locationGridTile);
				}
				locationGridTile.tileObjectComponent.genericTileObject.SetGridTileLocation(locationGridTile);
				allTiles.Add(locationGridTile);
				if (locationGridTile.IsAtEdgeOfWalkableMap())
				{
					allEdgeTiles.Add(locationGridTile);
				}
				map[x, y] = locationGridTile;
				batchCount++;
				if (batchCount == MapGenerationData.InnerMapTileGenerationBatches)
				{
					batchCount = 0;
					yield return null;
				}
			}
		}
		groundTilemap.SetTiles(positionArray, groundTilesArray);
		Parallel.ForEach(allTiles, delegate(LocationGridTile currentTile)
		{
			currentTile.FindNeighbours(map);
		});
	}

	public IEnumerator LoadTileVisuals(MapGenerationComponent mapGenerationComponent, SaveDataInnerMap saveDataInnerMap, Dictionary<string, TileBase> tileAssetDB)
	{
		int batchCount = 0;
		for (int i = 0; i < saveDataInnerMap.tileSaves.Values.Count; i++)
		{
			SaveDataLocationGridTile saveDataLocationGridTile = saveDataInnerMap.tileSaves.Values.ElementAt(i);
			LocationGridTile locationGridTile = map[(int)saveDataLocationGridTile.localPlace.x, (int)saveDataLocationGridTile.localPlace.y];
			if (!string.IsNullOrEmpty(saveDataLocationGridTile.groundTileMapAssetName))
			{
				locationGridTile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.TryGetTileAsset(saveDataLocationGridTile.groundTileMapAssetName, tileAssetDB));
			}
			if (!string.IsNullOrEmpty(saveDataLocationGridTile.wallTileMapAssetName))
			{
				locationGridTile.parentMap.elevationTilemap.SetTile(locationGridTile.localPlace, InnerMapManager.Instance.assetManager.TryGetTileAsset(saveDataLocationGridTile.wallTileMapAssetName, tileAssetDB));
				locationGridTile.UpdateGroundTypeBasedOnAsset();
			}
			if (!string.IsNullOrEmpty(saveDataLocationGridTile.shoreTileMapAssetName))
			{
				locationGridTile.parentMap.shoreTilemap.SetTile(locationGridTile.localPlace, InnerMapManager.Instance.assetManager.TryGetTileAsset(saveDataLocationGridTile.shoreTileMapAssetName, tileAssetDB));
				locationGridTile.UpdateGroundTypeBasedOnAsset();
			}
			batchCount++;
			if (batchCount == MapGenerationData.InnerMapTileGenerationBatches)
			{
				batchCount = 0;
				yield return null;
			}
		}
	}

	protected void ClearAllTileMaps()
	{
		for (int i = 0; i < _allTilemaps.Length; i++)
		{
			_allTilemaps[i].ClearAllTiles();
		}
	}

	public IEnumerator CreateSeamlessEdges()
	{
		int batchCount = 0;
		for (int i = 0; i < allTiles.Count; i++)
		{
			LocationGridTile locationGridTile = allTiles[i];
			if (locationGridTile.structure == null || locationGridTile.structure.structureType.IsOpenSpace() || locationGridTile.structure.structureType == STRUCTURE_TYPE.MONSTER_LAIR || locationGridTile.structure.structureType == STRUCTURE_TYPE.DEAD_GROUNDS)
			{
				locationGridTile.CreateSeamlessEdgesForTile(this);
				batchCount++;
				if (batchCount == MapGenerationData.InnerMapSeamlessEdgeBatches)
				{
					batchCount = 0;
					yield return null;
				}
			}
		}
	}

	public void SetUpperGroundVisual(Vector3Int location, TileBase asset, float alpha = 1f)
	{
		upperGroundTilemap.SetTile(location, asset);
		Color color = upperGroundTilemap.GetColor(location);
		color.a = alpha;
		upperGroundTilemap.SetColor(location, color);
	}

	public LocationGridTile GetRandomPassableEdgeTile()
	{
		LocationGridTile result = null;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < allEdgeTiles.Count; i++)
		{
			LocationGridTile locationGridTile = allEdgeTiles[i];
			if (locationGridTile.IsPassable())
			{
				list.Add(locationGridTile);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public void PlaceObject(TileObject obj, LocationGridTile tile, bool placeAsset = true)
	{
		tile.tileObjectComponent.SetObjectHere(obj);
	}

	public void LoadObject(TileObject obj, LocationGridTile tile)
	{
		tile.tileObjectComponent.LoadObjectHere(obj);
	}

	public void RemoveObject(LocationGridTile tile, Character removedBy = null, bool isPlayerSource = false)
	{
		tile.tileObjectComponent.RemoveObjectHere(removedBy, isPlayerSource);
	}

	public void RemoveObjectWithoutDestroying(LocationGridTile tile)
	{
		tile.tileObjectComponent.RemoveObjectHereWithoutDestroying();
	}

	public void RemoveObjectDestroyVisualOnly(LocationGridTile tile, Character remover = null)
	{
		tile.tileObjectComponent.RemoveObjectHereDestroyVisualOnly(remover);
	}

	public void OnCharacterMovedTo(Character character, LocationGridTile to, LocationGridTile from)
	{
		if (from == null)
		{
			to.AddCharacterHere(character);
			to.structure.AddCharacterAtLocation(character);
		}
		else
		{
			if (to.structure == null)
			{
				return;
			}
			if (from.structure != to.structure)
			{
				from.structure?.RemoveCharacterAtLocation(character);
				if (to.structure == null)
				{
					throw new Exception($"{character.name} is going to tile {to} which does not have a structure!");
				}
				to.structure.AddCharacterAtLocation(character);
			}
			else if (character.currentStructure != to.structure)
			{
				character.currentStructure?.RemoveCharacterAtLocation(character);
				if (to.structure == null)
				{
					throw new Exception($"{character.name} is going to tile {to} which does not have a structure!");
				}
				to.structure.AddCharacterAtLocation(character);
			}
			if (from.area != to.area)
			{
				from.area.OnRemovePOIInHex(character);
				to.area.OnPlacePOIInHex(character);
			}
			BaseSettlement settlement2;
			BaseSettlement settlement3;
			if (!from.IsPartOfSettlement() && to.IsPartOfSettlement(out var settlement) && settlement is NPCSettlement p_settlement)
			{
				character.eventDispatcher.ExecuteCharacterArrivedAtSettlement(character, p_settlement);
			}
			else if (from.IsPartOfSettlement(out settlement2) && to.IsPartOfSettlement(out settlement3) && settlement2 != settlement3 && settlement3 is NPCSettlement p_settlement2)
			{
				character.eventDispatcher.ExecuteCharacterArrivedAtSettlement(character, p_settlement2);
			}
			from.RemoveCharacterHere(character);
			to.AddCharacterHere(character);
		}
	}

	public void UpdateTilesWorldPosition()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				map[i, j].UpdateWorldLocation();
			}
		}
		SetWorldPosition();
	}

	private void SetWorldPosition()
	{
		worldPos = base.transform.position;
	}

	private void UpdateOrtographicSize(Camera p_cam, float p_float)
	{
		_boundDrawer.ManualUpdateBounds(groundTilemap.localBounds);
		worldUiCanvas.worldCamera = InnerMapCameraMove.Instance.camera;
		float orthographicSize = InnerMapCameraMove.Instance.camera.orthographicSize;
		cameraBounds = new Vector4
		{
			x = -189.6f
		};
		cameraBounds.y = orthographicSize - 1.5f;
		cameraBounds.z = cameraBounds.x + (float)width - 21.1f;
		cameraBounds.w = (float)height - orthographicSize - 4f + 12.85f;
		InnerMapCameraMove.Instance.SetCameraBordersForMap(this);
	}

	public void Open()
	{
	}

	public void Close()
	{
	}

	public virtual void OnMapGenerationFinished()
	{
		base.name = region.name + "'s Inner Map";
		groundTilemap.CompressBounds();
		_boundDrawer.ManualUpdateBounds(groundTilemap.localBounds);
		worldUiCanvas.worldCamera = InnerMapCameraMove.Instance.camera;
		float orthographicSize = InnerMapCameraMove.Instance.camera.orthographicSize;
		cameraBounds = new Vector4
		{
			x = -189.6f
		};
		cameraBounds.y = orthographicSize - 1.5f;
		cameraBounds.z = cameraBounds.x + (float)width - 21.1f;
		cameraBounds.w = (float)height - orthographicSize - 4f + 6.85f;
		Vector3 vector = new Vector3((float)width / 2f, (float)height / 2f);
		SpawnCenterGo(vector);
		SetMinimapCameraPosition(vector);
		SetMinimapCameraOrthographicSizeBasedOnMapSize(width, height);
		minimapCollider.center = new Vector3(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
		minimapCollider.size = new Vector3(width, height, 0f);
		onlyUnwalkableGraph = NNConstraint.Default;
		onlyUnwalkableGraph.constrainArea = false;
		onlyUnwalkableGraph.constrainDistance = false;
		onlyUnwalkableGraph.constrainTags = false;
		onlyUnwalkableGraph.constrainWalkability = false;
		onlyUnwalkableGraph.graphMask = GraphMask.FromGraph(unwalkableGraph);
		onlyPathfindingGraph = NNConstraint.Default;
		onlyPathfindingGraph.graphMask = GraphMask.FromGraph(pathfindingGraph);
	}

	private void SpawnCenterGo(Vector3 centerPosition)
	{
		centerGo = UnityEngine.Object.Instantiate(centerGoPrefab, base.transform);
		centerGo.transform.localPosition = centerPosition;
	}

	private void ShowPath(List<Vector3> points)
	{
		pathLineRenderer.gameObject.SetActive(value: true);
		pathLineRenderer.positionCount = points.Count;
		for (int i = 0; i < points.Count; i++)
		{
			pathLineRenderer.SetPosition(i, points[i]);
		}
	}

	private void ShowPath(Character character)
	{
		GraphNode nearestGridNode = character.movementComponent.GetNearestGridNode();
		if (nearestGridNode != null)
		{
			List<Vector3> vectorPath = character.marker.pathfindingAI.currentPath.vectorPath;
			pathLineRenderer.positionCount = 0;
			int num = vectorPath.Count - 1;
			for (int num2 = num; num2 >= 0; num2--)
			{
				pathLineRenderer.positionCount++;
				pathLineRenderer.SetPosition(num - num2, vectorPath[num2]);
				if ((Vector3)nearestGridNode.position == vectorPath[num2])
				{
					break;
				}
			}
		}
		if (pathLineRenderer.positionCount > 0)
		{
			pathLineRenderer.gameObject.SetActive(value: true);
		}
	}

	private void HidePath()
	{
		if (pathLineRenderer.gameObject.activeSelf)
		{
			pathLineRenderer.gameObject.SetActive(value: false);
		}
	}

	public LocationGridTile GetTileFromWorldPosition(Vector3 worldPosition)
	{
		Vector3Int vector3Int = groundTilemap.WorldToCell(worldPosition);
		return GetTileFromMapCoordinates(vector3Int.x, vector3Int.y);
	}

	public LocationGridTile GetTileFromMapCoordinatesRaw(int xPos, int yPos)
	{
		return map[xPos, yPos];
	}

	public LocationGridTile GetTileFromMapCoordinates(int xPos, int yPos)
	{
		if (Utilities.IsInRange(xPos, 0, width) && Utilities.IsInRange(yPos, 0, height))
		{
			return GetTileFromMapCoordinatesRaw(xPos, yPos);
		}
		return null;
	}

	public LocationGridTile GetTileFromMapCoordinates(Vector3Int p_vector)
	{
		return GetTileFromMapCoordinates(p_vector.x, p_vector.y);
	}

	public LocationGridTile GetNearestTileFromWorldPosition(Vector3 p_worldPosition)
	{
		Vector3Int vector3Int = groundTilemap.WorldToCell(p_worldPosition);
		int num = vector3Int.x;
		int num2 = vector3Int.y;
		if (num < 0)
		{
			num = 0;
		}
		else if (num > width)
		{
			num = width - 1;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		else if (num2 > height)
		{
			num2 = height - 1;
		}
		return GetTileFromMapCoordinates(num, num2);
	}

	public LocationGridTile GetTileFromScreenPosition(Vector3 p_screenPosition)
	{
		Vector3 worldPosition = worldUiCanvas.worldCamera.ScreenToWorldPoint(p_screenPosition);
		return GetTileFromWorldPosition(worldPosition);
	}

	public Vector3 GetWorldPositionFromScreenPosition(Vector3 p_screenPosition)
	{
		return worldUiCanvas.worldCamera.ScreenToWorldPoint(p_screenPosition);
	}

	public Vector3 GetScreenPositionFromWorldPosition(Vector3 p_worldPosition)
	{
		return worldUiCanvas.worldCamera.WorldToScreenPoint(p_worldPosition);
	}

	public Vector3 GetViewportPositionFromWorldPosition(Vector3 p_worldPosition)
	{
		return worldUiCanvas.worldCamera.WorldToViewportPoint(p_worldPosition);
	}

	public List<LocationStructure> PlaceBuiltStructureTemplateAt(GameObject p_structurePrefab, Area p_area, BaseSettlement p_settlement)
	{
		GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(p_structurePrefab.name, p_area.gridTileComponent.centerGridTile.centeredLocalLocation, Quaternion.identity, structureParent);
		List<LocationStructure> list = new List<LocationStructure>();
		StructureTemplate component = obj.GetComponent<StructureTemplate>();
		component.transform.localScale = Vector3.one;
		for (int i = 0; i < component.structureObjects.Length; i++)
		{
			LocationStructureObject locationStructureObject = component.structureObjects[i];
			if (locationStructureObject == null)
			{
				throw new Exception("No LocationStructureObject for " + p_structurePrefab.name);
			}
			locationStructureObject.RefreshAllTilemaps();
			List<LocationGridTile> tilesOccupiedByStructure = locationStructureObject.GetTilesOccupiedByStructure(this);
			locationStructureObject.SetTilesInStructure(tilesOccupiedByStructure.ToArray());
			locationStructureObject.ResetWallsBeforePlacement();
			locationStructureObject.ClearOutUnimportantObjectsBeforePlacement();
			LocationStructure locationStructure = LandmarkManager.Instance.CreateNewStructureAt(p_area.region, locationStructureObject.structureType, p_settlement);
			list.Add(locationStructure);
			for (int j = 0; j < tilesOccupiedByStructure.Count; j++)
			{
				tilesOccupiedByStructure[j].SetStructure(locationStructure);
			}
			if (locationStructure is DemonicStructure demonicStructure)
			{
				demonicStructure.SetStructureObject(locationStructureObject);
			}
			else if (locationStructure is ManMadeStructure manMadeStructure)
			{
				manMadeStructure.SetStructureObject(locationStructureObject);
			}
			else if (locationStructure is NaturalStructureWithStructureObject naturalStructureWithStructureObject)
			{
				naturalStructureWithStructureObject.SetStructureObject(locationStructureObject);
			}
			locationStructure.SetOccupiedArea(p_area);
			locationStructureObject.OnBuiltStructureObjectPlaced(this, locationStructure, out var createdWalls, out var totalWalls);
			locationStructure.CreateRoomsBasedOnStructureObject(locationStructureObject);
			locationStructure.OnBuiltNewStructure();
			if (createdWalls < totalWalls)
			{
				int num = totalWalls - createdWalls;
				TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.BLOCK_WALL);
				locationStructure.AdjustHP(-(num * tileObjectData.maxHP));
			}
		}
		return list;
	}

	public LocationStructure PlaceBuiltStructureTemplateAt(GameObject p_structurePrefab, LocationGridTile centerTile, BaseSettlement p_settlement)
	{
		LocationStructureObject component = ObjectPoolManager.Instance.InstantiateObjectFromPool(p_structurePrefab.name, centerTile.centeredLocalLocation, Quaternion.identity, structureParent).GetComponent<LocationStructureObject>();
		if (component == null)
		{
			throw new Exception("No LocationStructureObject for " + p_structurePrefab.name);
		}
		Area area = centerTile.area;
		p_settlement.AddAreaToSettlement(area);
		component.RefreshAllTilemaps();
		List<LocationGridTile> tilesOccupiedByStructure = component.GetTilesOccupiedByStructure(this);
		component.SetTilesInStructure(tilesOccupiedByStructure.ToArray());
		component.ResetWallsBeforePlacement();
		component.ClearOutUnimportantObjectsBeforePlacement();
		LocationStructure locationStructure = LandmarkManager.Instance.CreateNewStructureAt(centerTile.parentMap.region, component.structureType, p_settlement);
		for (int i = 0; i < tilesOccupiedByStructure.Count; i++)
		{
			tilesOccupiedByStructure[i].SetStructure(locationStructure);
		}
		if (locationStructure is DemonicStructure demonicStructure)
		{
			demonicStructure.SetStructureObject(component);
		}
		else if (locationStructure is ManMadeStructure manMadeStructure)
		{
			manMadeStructure.SetStructureObject(component);
		}
		else if (locationStructure is NaturalStructureWithStructureObject naturalStructureWithStructureObject)
		{
			naturalStructureWithStructureObject.SetStructureObject(component);
		}
		locationStructure.SetOccupiedArea(centerTile.area);
		component.OnBuiltStructureObjectPlaced(this, locationStructure, out var createdWalls, out var totalWalls);
		locationStructure.CreateRoomsBasedOnStructureObject(component);
		locationStructure.OnBuiltNewStructure();
		if (createdWalls < totalWalls)
		{
			int num = totalWalls - createdWalls;
			TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.BLOCK_WALL);
			locationStructure.AdjustHP(-(num * tileObjectData.maxHP));
		}
		return locationStructure;
	}

	private void ConvertDetailToTileObject(LocationGridTile tile)
	{
		Sprite sprite = detailsTilemap.GetSprite(tile.localPlace);
		TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(InnerMapManager.Instance.GetTileObjectTypeFromTileAsset(sprite));
		tile.structure.AddPOI(tileObject, tile);
		tileObject.mapVisual.SetVisual(sprite);
		detailsTilemap.SetTile(tile.localPlace, null);
	}

	public void PopulateTiles(List<LocationGridTile> p_tiles, Point size, LocationGridTile startingTile, List<LocationGridTile> mustBeIn = null)
	{
		for (int i = startingTile.localPlace.x; i < startingTile.localPlace.x + size.X; i++)
		{
			for (int j = startingTile.localPlace.y; j < startingTile.localPlace.y + size.Y; j++)
			{
				if (i < width && j < height && (mustBeIn == null || mustBeIn.Contains(map[i, j])))
				{
					p_tiles.Add(map[i, j]);
				}
			}
		}
	}

	protected IEnumerator GroundPerlin(List<LocationGridTile> tiles, int xSize, int ySize, float xSeed, float ySeed, MapGenerationData p_data)
	{
		yield return StartCoroutine(BiomePerlin(p_data));
		Vector3Int[] array = new Vector3Int[tiles.Count];
		TileBase[] array2 = new TileBase[tiles.Count];
		for (int i = 0; i < tiles.Count; i++)
		{
			LocationGridTile locationGridTile = tiles[i];
			array[i] = locationGridTile.localPlace;
			array2[i] = InnerMapManager.Instance.assetManager.GetGroundAssetForTile(locationGridTile);
		}
		groundTilemap.SetTiles(array, array2);
		for (int j = 0; j < array.Length; j++)
		{
			map[array[j].x, array[j].y].InitialUpdateGroundTypeBasedOnAsset();
		}
		yield return null;
	}

	private IEnumerator BiomePerlin(MapGenerationData p_data)
	{
		float[,] temperatureMap = Noise.GenerateTemperatureGradient(width, height, temperatureGradient, warpNoiseScale, warpSeed, warpStrength, warpWeight, temperatureSeed);
		float[,] precipitationMap = Noise.GenerateNoiseMap(precipitationPerlinSettings, width, height);
		tileTagMap = new Tile_Tag[width, height];
		p_data?.InitializeGeneratedMapPerlinDetails(width, height);
		List<LocationGridTile> tilesToSkipTileTag = RuinarchListPool<LocationGridTile>.Claim();
		int batchCount = 0;
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				float precipitation = precipitationMap[x, y];
				float temperature = temperatureMap[x, y];
				LocationGridTile locationGridTile = map[x, y];
				Biome_Tile_Type tileType = whittakerDiagram.GetTileType(precipitation, temperature);
				BIOMES mainBiomeForTileType = tileType.GetMainBiomeForTileType();
				locationGridTile.SetIndividualBiomeType(mainBiomeForTileType);
				locationGridTile.SetSpecificBiomeType(tileType);
				SetMinimapTileVisual(locationGridTile.localPlace, InnerMapManager.Instance.assetManager.minimapTile);
				minimapTilemap.SetTileFlags(locationGridTile.localPlace, TileFlags.None);
				locationGridTile.UpdateMinimapVisual(locationGridTile.structure);
				if (p_data != null)
				{
					Tile_Tag tile_Tag = (tilesToSkipTileTag.Contains(locationGridTile) ? Tile_Tag.None : RandomizeTileTag(tileType));
					tileTagMap[x, y] = tile_Tag;
					if (locationGridTile.tileObjectComponent.objHere == null && tile_Tag != Tile_Tag.None && !locationGridTile.HasNeighbouringWalledStructure())
					{
						TILE_OBJECT_TYPE randomTileObjectTypeForTileTag = GetRandomTileObjectTypeForTileTag(tile_Tag, locationGridTile, p_data);
						if (randomTileObjectTypeForTileTag == TILE_OBJECT_TYPE.BIG_TREE_OBJECT)
						{
							TileObject poi = InnerMapManager.Instance.CreateNewTileObject<TileObject>(randomTileObjectTypeForTileTag);
							locationGridTile.structure.AddPOI(poi, locationGridTile);
						}
						else
						{
							p_data.SetGeneratedMapPerlinDetails(locationGridTile, randomTileObjectTypeForTileTag);
						}
						switch (randomTileObjectTypeForTileTag)
						{
						case TILE_OBJECT_TYPE.SMALL_TREE_OBJECT:
							GridMap.Instance.mainRegion.gridTileFeatureComponent.AddFeatureToTile<SmallTreeSpotFeature>(locationGridTile);
							break;
						case TILE_OBJECT_TYPE.BIG_TREE_OBJECT:
							GridMap.Instance.mainRegion.gridTileFeatureComponent.AddFeatureToTile<BigTreeSpotFeature>(locationGridTile);
							break;
						}
					}
					else
					{
						p_data.SetGeneratedMapPerlinDetails(locationGridTile, TILE_OBJECT_TYPE.NONE);
					}
				}
				batchCount++;
				if (batchCount == MapGenerationData.InnerMapTileGenerationBatches)
				{
					batchCount = 0;
					yield return null;
				}
			}
		}
		RuinarchListPool<LocationGridTile>.Release(tilesToSkipTileTag);
	}

	protected IEnumerator GraduallyGenerateTileObjects(MapGenerationData p_data)
	{
		p_data.SetGeneratingTileObjectsState(p_state: true);
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		int count = 0;
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				LocationGridTile tile = map[x, y];
				TILE_OBJECT_TYPE tILE_OBJECT_TYPE = p_data.generatedMapPerlinDetailsMap[x][y];
				if (tILE_OBJECT_TYPE != TILE_OBJECT_TYPE.NONE && (tile.structure is Wilderness || tile.structure is Ocean || tile.structure is Cave) && tile.tileObjectComponent.objHere == null)
				{
					Sprite sprite = detailsTilemap.GetSprite(tile.localPlace);
					TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(tILE_OBJECT_TYPE);
					if (tileObject is BlockWall blockWall)
					{
						blockWall.SetWallType(WALL_TYPE.Stone);
					}
					tile.structure.AddPOI(tileObject, tile);
					if (tileObject.mapObjectVisual != null)
					{
						tileObject.mapObjectVisual.SetVisual(sprite);
					}
					count++;
					int num = ((!p_data.hasFinishedMapGenerationCoroutine) ? MapGenerationData.TileObjectCreationBatches : ((!UIManager.Instance.IsWaitingForTileObjectGenerationToComplete()) ? MapGenerationData.TileObjectCreationBatchesAfterWorldGeneration : MapGenerationData.TileObjectCreationBatchesWhileWaiting));
					if (count >= num)
					{
						count = 0;
						yield return null;
					}
				}
				detailsTilemap.SetTile(tile.localPlace, null);
			}
		}
		stopwatch.Stop();
		p_data.SetGeneratingTileObjectsState(p_state: false);
		Messenger.Broadcast(Signals.TILE_OBJECT_GENERATION_FINISHED);
	}

	private Tile_Tag RandomizeTileTag(Biome_Tile_Type p_tileType)
	{
		switch (p_tileType)
		{
		case Biome_Tile_Type.Desert:
			if (!GameUtilities.RollChance(5))
			{
				return Tile_Tag.None;
			}
			return Tile_Tag.Decor;
		case Biome_Tile_Type.Oasis:
		{
			float num = GameUtilities.RollFloat();
			if (num < 8f)
			{
				return Tile_Tag.Tree;
			}
			if (num < 13f)
			{
				return Tile_Tag.Decor;
			}
			if (num < 13.5f)
			{
				return Tile_Tag.Berry_Shrub;
			}
			return Tile_Tag.None;
		}
		case Biome_Tile_Type.Grassland:
		{
			float num5 = GameUtilities.RollFloat();
			if (num5 < 2f)
			{
				return Tile_Tag.Tree;
			}
			if (num5 < 7f)
			{
				return Tile_Tag.Decor;
			}
			if (num5 < 7.5f)
			{
				return Tile_Tag.Berry_Shrub;
			}
			return Tile_Tag.None;
		}
		case Biome_Tile_Type.Jungle:
		{
			float num2 = GameUtilities.RollFloat();
			if (num2 < 5f)
			{
				return Tile_Tag.Decor;
			}
			if (num2 < 5.5f)
			{
				return Tile_Tag.Berry_Shrub;
			}
			return Tile_Tag.Tree;
		}
		case Biome_Tile_Type.Taiga:
		{
			float num4 = GameUtilities.RollFloat();
			if (num4 < 12f)
			{
				return Tile_Tag.Tree;
			}
			if (num4 < 17f)
			{
				return Tile_Tag.Decor;
			}
			if (num4 < 17.5f)
			{
				return Tile_Tag.Berry_Shrub;
			}
			return Tile_Tag.None;
		}
		case Biome_Tile_Type.Tundra:
		{
			float num3 = GameUtilities.RollFloat();
			if (num3 < 2f)
			{
				return Tile_Tag.Tree;
			}
			if (num3 < 7f)
			{
				return Tile_Tag.Decor;
			}
			if (num3 < 7.5f)
			{
				return Tile_Tag.Berry_Shrub;
			}
			return Tile_Tag.None;
		}
		case Biome_Tile_Type.Snow:
			if (!GameUtilities.RollChance(5f))
			{
				return Tile_Tag.None;
			}
			return Tile_Tag.Decor;
		default:
			throw new ArgumentOutOfRangeException("p_tileType", p_tileType, null);
		}
	}

	private TILE_OBJECT_TYPE GetRandomTileObjectTypeForTileTag(Tile_Tag p_tag, LocationGridTile p_tile, MapGenerationData p_data)
	{
		switch (p_tag)
		{
		case Tile_Tag.Decor:
			switch (p_tile.mainBiomeType)
			{
			case BIOMES.GRASSLAND:
			case BIOMES.FOREST:
				return CollectionUtilities.GetRandomElement(grasslandDecorChoices);
			case BIOMES.SNOW:
				return CollectionUtilities.GetRandomElement(snowDecorChoices);
			case BIOMES.DESERT:
				return CollectionUtilities.GetRandomElement(desertDecorChoices);
			default:
				throw new ArgumentOutOfRangeException();
			}
		case Tile_Tag.Tree:
			if (InnerMapManager.Instance.CanBigTreeBePlacedOnTileInRandomGeneration(p_tile, p_data))
			{
				if (!GameUtilities.RollChance(50))
				{
					return TILE_OBJECT_TYPE.SMALL_TREE_OBJECT;
				}
				return TILE_OBJECT_TYPE.BIG_TREE_OBJECT;
			}
			return TILE_OBJECT_TYPE.SMALL_TREE_OBJECT;
		case Tile_Tag.Berry_Shrub:
			return TILE_OBJECT_TYPE.BERRY_SHRUB;
		case Tile_Tag.None:
			return TILE_OBJECT_TYPE.NONE;
		default:
			throw new ArgumentOutOfRangeException("p_tag", p_tag, null);
		}
	}

	public static TileBase GetGroundAssetPerlin(float floorSample, BIOMES biomeType)
	{
		switch (biomeType)
		{
		case BIOMES.SNOW:
		case BIOMES.TUNDRA:
			if (floorSample < 0.5f)
			{
				return InnerMapManager.Instance.assetManager.snowTile;
			}
			if (floorSample >= 0.5f && floorSample < 0.8f)
			{
				return InnerMapManager.Instance.assetManager.snowDirt;
			}
			return InnerMapManager.Instance.assetManager.stoneTile;
		case BIOMES.DESERT:
			if (floorSample < 0.5f)
			{
				return InnerMapManager.Instance.assetManager.desertGrassTile;
			}
			if (floorSample >= 0.5f && floorSample < 0.8f)
			{
				return InnerMapManager.Instance.assetManager.desertSandTile;
			}
			return InnerMapManager.Instance.assetManager.desertStoneGroundTile;
		case BIOMES.FOREST:
			if (floorSample < 0.8f)
			{
				return InnerMapManager.Instance.assetManager.grassTile;
			}
			return InnerMapManager.Instance.assetManager.stoneTile;
		default:
			if (floorSample < 0.5f)
			{
				return InnerMapManager.Instance.assetManager.grassTile;
			}
			if (floorSample >= 0.5f && floorSample < 0.8f)
			{
				return InnerMapManager.Instance.assetManager.soilTile;
			}
			return InnerMapManager.Instance.assetManager.stoneTile;
		}
	}

	public void Update()
	{
		Character currentlySelectedCharacter = UIManager.Instance.GetCurrentlySelectedCharacter();
		if (currentlySelectedCharacter != null && currentlySelectedCharacter.hasMarker && currentlySelectedCharacter.currentRegion == region && !currentlySelectedCharacter.isDead && currentlySelectedCharacter.marker.pathfindingAI.hasPath && currentlySelectedCharacter.carryComponent.IsNotBeingCarried())
		{
			if (currentlySelectedCharacter.marker.pathfindingAI.currentPath != null && currentlySelectedCharacter.marker.isMoving)
			{
				ShowPath(currentlySelectedCharacter);
			}
			else
			{
				HidePath();
			}
		}
		else
		{
			HidePath();
		}
		float num = 2f * InnerMapCameraMove.Instance.camera.orthographicSize;
		float x = num * InnerMapCameraMove.Instance.camera.aspect;
		Vector2 size = new Vector2(x, num);
		minimapPlayerCameraVisual.size = size;
		minimapPlayerCameraVisual.transform.position = InnerMapCameraMove.Instance.camera.transform.position;
	}

	protected override void OnDestroy()
	{
		minimapCamera.targetTexture = null;
		minimapRenderTexture.Release();
		minimapRenderTexture = null;
		base.OnDestroy();
	}

	private void Awake()
	{
		minimapCamera.targetTexture = minimapRenderTexture;
	}

	public void CleanUp()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				map[i, j]?.CleanUp();
			}
		}
		map = null;
		allTiles?.Clear();
		allTiles = null;
		allEdgeTiles?.Clear();
		allEdgeTiles = null;
		pathfindingGraph = null;
		UnityEngine.Object.Destroy(centerGo);
		centerGo = null;
	}

	private void SetMinimapTileVisual(Vector3Int p_localPlace, TileBase p_tileBase)
	{
		minimapTilemap.SetTile(p_localPlace, p_tileBase);
	}

	public void SetMinimapTileColor(Vector3Int p_localPlace, Color p_color)
	{
		minimapTilemap.SetColor(p_localPlace, p_color);
	}

	private void SetMinimapCameraPosition(Vector3 p_pos)
	{
		p_pos.z = -20f;
		minimapCamera.transform.localPosition = p_pos;
	}

	private void SetMinimapCameraOrthographicSizeBasedOnMapSize(float p_width, float p_height)
	{
		float num = 1f;
		float num2 = p_width / p_height;
		if (num >= num2)
		{
			minimapCamera.orthographicSize = p_height / 2f;
			return;
		}
		float num3 = num2 / num;
		minimapCamera.orthographicSize = p_height / 2f * num3;
	}

	private ELEVATION GetElevationFromMap(int x, int y, float[,] noiseMap)
	{
		float num = noiseMap[x, y];
		PerlinNoiseRegion perlinNoiseRegion = elevationPerlinSettings.GetPerlinNoiseRegion(num);
		if (perlinNoiseRegion.name.Contains("Water"))
		{
			return ELEVATION.WATER;
		}
		if (perlinNoiseRegion.name.Equals("Cave", StringComparison.InvariantCultureIgnoreCase))
		{
			return ELEVATION.MOUNTAIN;
		}
		return ELEVATION.PLAIN;
	}

	protected IEnumerator GenerateElevationMap(MapGenerationComponent mapGenerationComponent, MapGenerationData data, Stopwatch stopwatch)
	{
		float[,] noiseMap = Noise.GenerateNoiseMap(elevationPerlinSettings, width, height);
		ElevationIsland[][] elevationMap = new ElevationIsland[width][];
		for (int i = 0; i < width; i++)
		{
			elevationMap[i] = new ElevationIsland[height];
		}
		List<ElevationIsland> allElevationIslands = new List<ElevationIsland>();
		int batchCount = 0;
		stopwatch.Reset();
		stopwatch.Start();
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				ELEVATION elevationFromMap = GetElevationFromMap(x, y, noiseMap);
				LocationGridTile locationGridTile = map[x, y];
				if (elevationFromMap == ELEVATION.PLAIN)
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < locationGridTile.neighbourList.Count; j++)
				{
					LocationGridTile locationGridTile2 = locationGridTile.neighbourList[j];
					ELEVATION elevationFromMap2 = GetElevationFromMap(locationGridTile2.localPlace.x, locationGridTile2.localPlace.y, noiseMap);
					if (elevationFromMap == elevationFromMap2)
					{
						ElevationIsland elevationIsland = elevationMap[locationGridTile2.localPlace.x][locationGridTile2.localPlace.y];
						if (elevationIsland != null)
						{
							elevationIsland.AddTile(locationGridTile, data);
							elevationMap[x][y] = elevationIsland;
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					ElevationIsland elevationIsland2 = new ElevationIsland(elevationFromMap);
					elevationIsland2.AddTile(locationGridTile, data);
					allElevationIslands.Add(elevationIsland2);
					elevationMap[x][y] = elevationIsland2;
				}
				batchCount++;
				if (batchCount == MapGenerationData.InnerMapTileGenerationBatches)
				{
					batchCount = 0;
					yield return null;
				}
			}
		}
		stopwatch.Stop();
		mapGenerationComponent.AddLog(region.name + " GenerateElevationMap part 1 took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
		stopwatch.Reset();
		stopwatch.Start();
		for (int x = 0; x < 2; x++)
		{
			for (int y = 0; y < allElevationIslands.Count; y++)
			{
				ElevationIsland elevationIsland3 = allElevationIslands[y];
				if (elevationIsland3.tiles.Count == 0)
				{
					continue;
				}
				for (int k = 0; k < allElevationIslands.Count; k++)
				{
					ElevationIsland elevationIsland4 = allElevationIslands[k];
					if (elevationIsland4.tiles.Count != 0)
					{
						if (elevationIsland3 != elevationIsland4 && elevationIsland3.elevation == elevationIsland4.elevation && elevationIsland3.IsAdjacentToIsland(elevationIsland4))
						{
							elevationIsland3.MergeWithIsland(elevationIsland4, data);
						}
						batchCount++;
						if (batchCount == MapGenerationData.InnerMapTileGenerationBatches)
						{
							batchCount = 0;
							yield return null;
						}
					}
				}
			}
		}
		stopwatch.Stop();
		mapGenerationComponent.AddLog(region.name + " GenerateElevationMap part 2 took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
		stopwatch.Reset();
		stopwatch.Start();
		for (int l = 0; l < allElevationIslands.Count; l++)
		{
			ElevationIsland elevationIsland5 = allElevationIslands[l];
			if (elevationIsland5.tiles.Count >= 100)
			{
				continue;
			}
			ElevationIsland firstAdjacentIsland = elevationIsland5.GetFirstAdjacentIsland(allElevationIslands);
			if (firstAdjacentIsland != null)
			{
				firstAdjacentIsland.MergeWithIsland(elevationIsland5, data);
				continue;
			}
			for (int m = 0; m < elevationIsland5.tiles.Count; m++)
			{
				elevationIsland5.tiles.ElementAt(m).SetElevation(ELEVATION.PLAIN);
			}
			elevationIsland5.RemoveAllTiles();
		}
		stopwatch.Stop();
		mapGenerationComponent.AddLog(region.name + " GenerateElevationMap part 3 took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
		stopwatch.Reset();
		for (int n = 0; n < allElevationIslands.Count; n++)
		{
			ElevationIsland elevationIsland6 = allElevationIslands[n];
			if (elevationIsland6.elevation != ELEVATION.WATER)
			{
				continue;
			}
			List<LocationGridTile> borderTiles = elevationIsland6.borderTiles;
			for (int num = 0; num < borderTiles.Count; num++)
			{
				LocationGridTile locationGridTile3 = borderTiles[num];
				if (locationGridTile3.GetDifferentElevationNeighboursCount() == 5)
				{
					elevationIsland6.RemoveTile(locationGridTile3, data);
				}
			}
		}
		for (int num2 = 0; num2 < allElevationIslands.Count; num2++)
		{
			if (allElevationIslands[num2].tiles.Count == 0)
			{
				allElevationIslands.RemoveAt(num2);
				num2--;
			}
		}
		yield return StartCoroutine(CreateAndDrawElevationStructures(allElevationIslands, mapGenerationComponent, data, noiseMap));
	}

	public IEnumerator CreateAndDrawElevationStructures(List<ElevationIsland> allElevationIslands, MapGenerationComponent mapGenerationComponent, MapGenerationData mapGenerationData, float[,] noiseMap)
	{
		Stopwatch stopwatch = new Stopwatch();
		for (int i = 0; i < allElevationIslands.Count; i++)
		{
			ElevationIsland elevationIsland = allElevationIslands[i];
			if (elevationIsland.elevation != ELEVATION.PLAIN)
			{
				STRUCTURE_TYPE structureTypeForElevation = elevationIsland.elevation.GetStructureTypeForElevation();
				NPCSettlement settlement = null;
				if (structureTypeForElevation == STRUCTURE_TYPE.CAVE)
				{
					settlement = LandmarkManager.Instance.CreateNewSettlementMultipleAreas(region, LOCATION_TYPE.DUNGEON, elevationIsland.occupiedAreas);
				}
				LocationStructure elevationStructure = LandmarkManager.Instance.CreateNewStructureAt(region, structureTypeForElevation, settlement);
				switch (structureTypeForElevation)
				{
				case STRUCTURE_TYPE.CAVE:
					yield return StartCoroutine(DrawCave(elevationIsland, elevationStructure, mapGenerationComponent, stopwatch, mapGenerationData));
					break;
				case STRUCTURE_TYPE.OCEAN:
					yield return StartCoroutine(DrawOcean(elevationIsland, elevationStructure, mapGenerationComponent, stopwatch, mapGenerationData, noiseMap));
					break;
				}
				elevationStructure.SetOccupiedArea(elevationIsland.occupiedAreas.First());
				yield return null;
			}
		}
	}

	private IEnumerator DrawCave(ElevationIsland p_island, LocationStructure p_caveStructure, MapGenerationComponent mapGenerationComponent, Stopwatch stopwatch, MapGenerationData mapGenerationData)
	{
		int batchCount = 0;
		Vector3Int[] positionArray = new Vector3Int[p_island.tiles.Count];
		TileBase[] tileBaseArray = new TileBase[p_island.tiles.Count];
		List<LocationGridTile> groundTiles = RuinarchListPool<LocationGridTile>.Claim();
		groundTiles.AddRange(p_island.tiles);
		stopwatch.Reset();
		stopwatch.Start();
		for (int i = 0; i < groundTiles.Count; i++)
		{
			LocationGridTile locationGridTile = groundTiles[i];
			positionArray[i] = locationGridTile.localPlace;
			tileBaseArray[i] = InnerMapManager.Instance.assetManager.caveGroundTile;
			SetAsMountainGround(locationGridTile, p_caveStructure, mapGenerationData);
			batchCount++;
			if (batchCount == MapGenerationData.InnerMapElevationBatches)
			{
				batchCount = 0;
				yield return null;
			}
		}
		groundTilemap.SetTiles(positionArray, tileBaseArray);
		for (int i = 0; i < groundTiles.Count; i++)
		{
			groundTiles[i].UpdateGroundTileMapAssetNameForBatchedTileSetting();
			batchCount++;
			if (batchCount == MapGenerationData.InnerMapElevationBatches)
			{
				batchCount = 0;
				yield return null;
			}
		}
		stopwatch.Stop();
		mapGenerationComponent.AddLog(region.name + " Draw Cave Ground took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
		stopwatch.Reset();
		RuinarchListPool<LocationGridTile>.Release(groundTiles);
		stopwatch.Start();
		yield return StartCoroutine(MountainCellAutomata(p_island.tiles.ToList(), p_caveStructure, mapGenerationData));
		stopwatch.Stop();
		mapGenerationComponent.AddLog(region.name + " Draw Cave Cell Automata took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
	}

	public void SetAsMountainWall(LocationGridTile tile, LocationStructure structure, MapGenerationData mapGenerationData)
	{
		if (!(tile.tileObjectComponent.objHere is BlockWall))
		{
			tile.SetTileType(LocationGridTile.Tile_Type.Wall);
			tile.SetTileState(LocationGridTile.Tile_State.Occupied);
			mapGenerationData.SetGeneratedMapPerlinDetails(tile, TILE_OBJECT_TYPE.NONE);
			tile.SetIsDefault(state: false);
			BlockWall blockWall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
			blockWall.SetWallType(WALL_TYPE.Stone);
			structure.AddPOI(blockWall, tile);
		}
	}

	private void SetAsMountainGround(LocationGridTile tile, LocationStructure structure, MapGenerationData mapGenerationData)
	{
		if (tile.tileObjectComponent.objHere != null)
		{
			tile.structure.RemovePOI(tile.tileObjectComponent.objHere);
		}
		if (tile.structure != structure)
		{
			tile.SetStructure(structure);
		}
		tile.SetGroundType(LocationGridTile.Ground_Type.Cave);
		tile.SetIsDefault(state: false);
		mapGenerationData.SetGeneratedMapPerlinDetails(tile, TILE_OBJECT_TYPE.NONE);
		GridMap.Instance.mainRegion.gridTileFeatureComponent.RemoveFeatureFromTile<SmallTreeSpotFeature>(tile);
		GridMap.Instance.mainRegion.gridTileFeatureComponent.RemoveFeatureFromTile<BigTreeSpotFeature>(tile);
	}

	private IEnumerator MountainCellAutomata(List<LocationGridTile> locationGridTiles, LocationStructure p_caveStructure, MapGenerationData mapGenerationData)
	{
		LocationGridTile[,] tileMap = CellularAutomataGenerator.ConvertListToGridMap(locationGridTiles);
		int randomFillPercent = 20;
		int smoothing = 1;
		if (locationGridTiles.Count > 200)
		{
			randomFillPercent = 30;
		}
		else if (locationGridTiles.Count > 300)
		{
			randomFillPercent = 50;
		}
		else if (locationGridTiles.Count > 500)
		{
			randomFillPercent = 60;
		}
		int[,] cellAutomata = CellularAutomataGenerator.GenerateMap(tileMap, locationGridTiles, smoothing, randomFillPercent);
		yield return MapGenerator.Instance.StartCoroutine(CellularAutomataGenerator.DrawElevationMapCoroutine(tileMap, cellAutomata, InnerMapManager.Instance.assetManager.caveWallTile, null, ELEVATION.MOUNTAIN, p_caveStructure, mapGenerationData));
		Cave cave = p_caveStructure as Cave;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		list.AddRange(p_caveStructure.passableTiles);
		int num = GameUtilities.RandomBetweenTwoNumbers(4, 6);
		for (int i = 0; i < num; i++)
		{
			if (list.Count == 0)
			{
				break;
			}
			LocationGridTile randomElement = CollectionUtilities.GetRandomElement(list);
			cave.AddStoneSpot(randomElement);
			GridMap.Instance.mainRegion.gridTileFeatureComponent.AddFeatureToTile<StoneSpotFeature>(randomElement);
			list.Remove(randomElement);
		}
		int num2 = GameUtilities.RandomBetweenTwoNumbers(2, 4);
		for (int j = 0; j < num2; j++)
		{
			if (list.Count == 0)
			{
				break;
			}
			LocationGridTile randomElement2 = CollectionUtilities.GetRandomElement(list);
			cave.AddOreSpot(randomElement2);
			GridMap.Instance.mainRegion.gridTileFeatureComponent.AddFeatureToTile<MetalOreSpotFeature>(randomElement2);
			list.Remove(randomElement2);
		}
		int hexTileCountOfCave = GetHexTileCountOfCave(cave);
		int num3 = GameUtilities.RandomBetweenTwoNumbers(1, 3) * hexTileCountOfCave;
		for (int k = 0; k < num3; k++)
		{
			if (list.Count == 0)
			{
				break;
			}
			LocationGridTile randomElement3 = CollectionUtilities.GetRandomElement(list);
			cave.AddMushroomSpot(randomElement3);
			GridMap.Instance.mainRegion.gridTileFeatureComponent.AddFeatureToTile<MushroomSpotFeature>(randomElement3);
			list.Remove(randomElement3);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	private int GetHexTileCountOfCave(LocationStructure caveStructure)
	{
		List<Area> list = new List<Area>();
		for (int i = 0; i < caveStructure.unoccupiedTiles.Count; i++)
		{
			LocationGridTile locationGridTile = caveStructure.unoccupiedTiles.ElementAt(i);
			if (!list.Contains(locationGridTile.area))
			{
				list.Add(locationGridTile.area);
			}
		}
		return list.Count;
	}

	public void CreateOreVein(LocationGridTile tile)
	{
		if (tile != null)
		{
			if (tile.tileObjectComponent.objHere != null)
			{
				tile.structure.RemovePOI(tile.tileObjectComponent.objHere);
			}
			tile.tileObjectComponent.genericTileObject.CreateMineShackSpotStructureConnector();
			TileObject poi = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ORE_VEIN);
			tile.structure.AddPOI(poi, tile);
		}
	}

	private IEnumerator DrawOcean(ElevationIsland p_island, LocationStructure p_oceanStructure, MapGenerationComponent mapGenerationComponent, Stopwatch stopwatch, MapGenerationData mapGenerationData, float[,] noiseMap)
	{
		stopwatch.Reset();
		stopwatch.Start();
		int batchCount = 0;
		Vector3Int[] positionArray = new Vector3Int[p_island.tiles.Count];
		TileBase[] tileBaseArray = new TileBase[p_island.tiles.Count];
		TileBase[] shoreTileArray = new TileBase[p_island.tiles.Count];
		List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
		tiles.AddRange(p_island.tiles);
		for (int i = 0; i < tiles.Count; i++)
		{
			LocationGridTile locationGridTile = tiles[i];
			positionArray[i] = locationGridTile.localPlace;
			LocationGridTile.Ground_Type groundType = LocationGridTile.Ground_Type.Water_Shallow;
			float num = noiseMap[locationGridTile.localPlace.x, locationGridTile.localPlace.y];
			PerlinNoiseRegion perlinNoiseRegion = elevationPerlinSettings.GetPerlinNoiseRegion(num);
			if (perlinNoiseRegion.name == "Water_Deep")
			{
				groundType = LocationGridTile.Ground_Type.Water_Deep;
				TileBase deepestWaterTile = InnerMapManager.Instance.assetManager.deepestWaterTile;
				tileBaseArray[i] = deepestWaterTile;
			}
			else if (perlinNoiseRegion.name == "Water_Mid")
			{
				groundType = LocationGridTile.Ground_Type.Water_Mid;
				TileBase midWaterTile = InnerMapManager.Instance.assetManager.midWaterTile;
				tileBaseArray[i] = midWaterTile;
			}
			else if (perlinNoiseRegion.name == "Water_Shallow")
			{
				groundType = LocationGridTile.Ground_Type.Water_Shallow;
				TileBase shallowWaterTile = InnerMapManager.Instance.assetManager.shallowWaterTile;
				tileBaseArray[i] = shallowWaterTile;
			}
			else if (perlinNoiseRegion.name == "Water_Shore")
			{
				groundType = LocationGridTile.Ground_Type.Water_Shore;
			}
			shoreTileArray[i] = InnerMapManager.Instance.assetManager.shoreWaterTile;
			SetAsWater(locationGridTile, p_oceanStructure, mapGenerationData, groundType);
			batchCount++;
			if (batchCount == MapGenerationData.InnerMapElevationBatches)
			{
				batchCount = 0;
				yield return null;
			}
		}
		elevationTilemap.SetTiles(positionArray, tileBaseArray);
		shoreTilemap.SetTiles(positionArray, shoreTileArray);
		for (int i = 0; i < tiles.Count; i++)
		{
			LocationGridTile locationGridTile2 = tiles[i];
			locationGridTile2.UpdateWallTileMapAssetNameForBatchedTileSetting();
			locationGridTile2.UpdateShoreTileMapAssetName();
			batchCount++;
			if (batchCount == MapGenerationData.InnerMapElevationBatches)
			{
				batchCount = 0;
				yield return null;
			}
		}
		RuinarchListPool<LocationGridTile>.Release(tiles);
		stopwatch.Stop();
		mapGenerationComponent.AddLog(region.name + " Draw Ocean took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
		stopwatch.Reset();
		stopwatch.Start();
		int westMost = p_oceanStructure.tiles.Min((LocationGridTile t) => t.localPlace.x);
		int eastMost = p_oceanStructure.tiles.Max((LocationGridTile t) => t.localPlace.x);
		int southMost = p_oceanStructure.tiles.Min((LocationGridTile t) => t.localPlace.y);
		int northMost = p_oceanStructure.tiles.Max((LocationGridTile t) => t.localPlace.y);
		LocationGridTile randomElement = CollectionUtilities.GetRandomElement(p_oceanStructure.tiles.Where((LocationGridTile t) => t.localPlace.y == northMost && t.tileObjectComponent.objHere == null));
		if (randomElement != null && !randomElement.IsAtEdgeOfMap())
		{
			CreateFishingSpot(randomElement);
		}
		LocationGridTile randomElement2 = CollectionUtilities.GetRandomElement(p_oceanStructure.tiles.Where((LocationGridTile t) => t.localPlace.y == southMost && t.tileObjectComponent.objHere == null));
		if (randomElement2 != null && !randomElement2.IsAtEdgeOfMap())
		{
			CreateFishingSpot(randomElement2);
		}
		LocationGridTile randomElement3 = CollectionUtilities.GetRandomElement(p_oceanStructure.tiles.Where((LocationGridTile t) => t.localPlace.x == westMost && t.tileObjectComponent.objHere == null));
		if (randomElement3 != null && !randomElement3.IsAtEdgeOfMap())
		{
			CreateFishingSpot(randomElement3);
		}
		LocationGridTile randomElement4 = CollectionUtilities.GetRandomElement(p_oceanStructure.tiles.Where((LocationGridTile t) => t.localPlace.x == eastMost && t.tileObjectComponent.objHere == null));
		if (randomElement4 != null && !randomElement4.IsAtEdgeOfMap())
		{
			CreateFishingSpot(randomElement4);
		}
		stopwatch.Stop();
		mapGenerationComponent.AddLog(region.name + " Create Fishing Spots took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
		stopwatch.Reset();
	}

	public void CreateFishingSpot(LocationGridTile tile)
	{
		if (tile != null)
		{
			TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.FISHING_SPOT);
			tile.structure.AddPOI(tileObject, tile);
			tileObject.mapObjectVisual.SetVisual(null);
		}
	}

	private void SetAsWater(LocationGridTile tile, LocationStructure structure, MapGenerationData mapGenerationData, LocationGridTile.Ground_Type groundType)
	{
		tile.SetGroundType(groundType);
		tile.SetTileState(LocationGridTile.Tile_State.Occupied);
		if (tile.tileObjectComponent.objHere != null)
		{
			tile.structure.RemovePOI(tile.tileObjectComponent.objHere);
		}
		tile.SetStructure(structure);
		for (int i = 0; i < 10; i++)
		{
			tile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(tile.tileObjectComponent.genericTileObject, "Wet", null, bypassElementalChance: false, 0, 0f, ELEMENTAL_TYPE.Water);
		}
		mapGenerationData.SetGeneratedMapPerlinDetails(tile, TILE_OBJECT_TYPE.NONE);
		GridMap.Instance.mainRegion.gridTileFeatureComponent.RemoveFeatureFromTile<SmallTreeSpotFeature>(tile);
		GridMap.Instance.mainRegion.gridTileFeatureComponent.RemoveFeatureFromTile<BigTreeSpotFeature>(tile);
		tile.area.gridTileComponent.RemovePassableTile(tile);
	}
}
