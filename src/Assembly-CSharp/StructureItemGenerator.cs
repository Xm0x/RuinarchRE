using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UtilityScripts;

public class StructureItemGenerator : MonoBehaviour
{
	[Header("Walls")]
	[Tooltip("This is only relevant if blockWallsTilemap is not null.")]
	[FormerlySerializedAs("_wallType")]
	[SerializeField]
	private WALL_TYPE _blockWallType;

	[Tooltip("This is only relevant if structure uses thin walls.")]
	[FormerlySerializedAs("_wallResource")]
	[SerializeField]
	private WALL_RESOURCE _thinWallResource;

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

	[Header("Objects")]
	[FormerlySerializedAs("_objectsParent")]
	public Transform objectsParent;

	private ThinWallGameObject[] wallVisuals;

	[ContextMenu("Convert Walls")]
	public void ConvertWalls()
	{
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
				if (thinWallGameObject != null)
				{
					Vector3 position = vector2;
					if (tile.name.Contains("BotLeft"))
					{
						position.x -= 0.5f;
						position.y -= 0.5f;
						UnityEngine.Object.Instantiate(cornerPrefab, position, Quaternion.identity, thinWallGameObject.transform);
					}
					else if (tile.name.Contains("BotRight"))
					{
						position.x += 0.5f;
						position.y -= 0.5f;
						UnityEngine.Object.Instantiate(cornerPrefab, position, Quaternion.identity, thinWallGameObject.transform);
					}
					else if (tile.name.Contains("TopLeft"))
					{
						position.x -= 0.5f;
						position.y += 0.5f;
						UnityEngine.Object.Instantiate(cornerPrefab, position, Quaternion.identity, thinWallGameObject.transform);
					}
					else if (tile.name.Contains("TopRight"))
					{
						position.x += 0.5f;
						position.y += 0.5f;
						UnityEngine.Object.Instantiate(cornerPrefab, position, Quaternion.identity, thinWallGameObject.transform);
					}
					if (_thinWallResource != WALL_RESOURCE.Wood)
					{
						thinWallGameObject.UpdateWallAssets(_thinWallResource);
					}
				}
			}
		}
	}

	private ThinWallGameObject InstantiateWall(GameObject wallPrefab, Vector3 centeredPos, Transform parent, bool updateWallAsset)
	{
		GameObject obj = UnityEngine.Object.Instantiate(wallPrefab, parent);
		obj.transform.position = centeredPos;
		ThinWallGameObject component = obj.GetComponent<ThinWallGameObject>();
		if (updateWallAsset)
		{
			component.UpdateWallAssets(_thinWallResource);
		}
		return component;
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
}
