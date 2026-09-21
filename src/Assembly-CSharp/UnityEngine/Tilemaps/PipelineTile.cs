using System;

namespace UnityEngine.Tilemaps;

[Serializable]
public class PipelineTile : TileBase
{
	[SerializeField]
	public Sprite[] m_Sprites;

	public override void RefreshTile(Vector3Int location, ITilemap tileMap)
	{
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				Vector3Int position = new Vector3Int(location.x + j, location.y + i, location.z);
				if (TileValue(tileMap, position))
				{
					tileMap.RefreshTile(position);
				}
			}
		}
	}

	public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
	{
		UpdateTile(location, tileMap, ref tileData);
	}

	private void UpdateTile(Vector3Int location, ITilemap tileMap, ref TileData tileData)
	{
		tileData.transform = Matrix4x4.identity;
		tileData.color = Color.white;
		int num = (TileValue(tileMap, location + new Vector3Int(0, 1, 0)) ? 1 : 0);
		num += (TileValue(tileMap, location + new Vector3Int(1, 0, 0)) ? 2 : 0);
		num += (TileValue(tileMap, location + new Vector3Int(0, -1, 0)) ? 4 : 0);
		num += (TileValue(tileMap, location + new Vector3Int(-1, 0, 0)) ? 8 : 0);
		int index = GetIndex((byte)num);
		if (index >= 0 && index < m_Sprites.Length && TileValue(tileMap, location))
		{
			tileData.sprite = m_Sprites[index];
			tileData.transform = GetTransform((byte)num);
			tileData.flags = TileFlags.LockAll;
			tileData.colliderType = Tile.ColliderType.Sprite;
		}
	}

	private bool TileValue(ITilemap tileMap, Vector3Int position)
	{
		TileBase tile = tileMap.GetTile(position);
		if (tile != null)
		{
			return tile == this;
		}
		return false;
	}

	private int GetIndex(byte mask)
	{
		switch (mask)
		{
		case 0:
			return 0;
		case 3:
		case 6:
		case 9:
		case 12:
			return 1;
		case 1:
		case 2:
		case 4:
		case 5:
		case 8:
		case 10:
			return 2;
		case 7:
		case 11:
		case 13:
		case 14:
			return 3;
		case 15:
			return 4;
		default:
			return -1;
		}
	}

	private Matrix4x4 GetTransform(byte mask)
	{
		switch (mask)
		{
		case 2:
		case 7:
		case 8:
		case 9:
		case 10:
			return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -90f), Vector3.one);
		case 3:
		case 14:
			return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -180f), Vector3.one);
		case 6:
		case 13:
			return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -270f), Vector3.one);
		default:
			return Matrix4x4.identity;
		}
	}
}
