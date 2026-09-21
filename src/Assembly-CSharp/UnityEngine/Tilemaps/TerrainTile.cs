using System;

namespace UnityEngine.Tilemaps;

[Serializable]
public class TerrainTile : TileBase
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
		num += (TileValue(tileMap, location + new Vector3Int(1, 1, 0)) ? 2 : 0);
		num += (TileValue(tileMap, location + new Vector3Int(1, 0, 0)) ? 4 : 0);
		num += (TileValue(tileMap, location + new Vector3Int(1, -1, 0)) ? 8 : 0);
		num += (TileValue(tileMap, location + new Vector3Int(0, -1, 0)) ? 16 : 0);
		num += (TileValue(tileMap, location + new Vector3Int(-1, -1, 0)) ? 32 : 0);
		num += (TileValue(tileMap, location + new Vector3Int(-1, 0, 0)) ? 64 : 0);
		num += (TileValue(tileMap, location + new Vector3Int(-1, 1, 0)) ? 128 : 0);
		byte num2 = (byte)num;
		if ((num2 | 0xFE) < 255)
		{
			num &= 0x7D;
		}
		if ((num2 | 0xFB) < 255)
		{
			num &= 0xF5;
		}
		if ((num2 | 0xEF) < 255)
		{
			num &= 0xD7;
		}
		if ((num2 | 0xBF) < 255)
		{
			num &= 0x5F;
		}
		int index = GetIndex((byte)num);
		if (index >= 0 && index < m_Sprites.Length && TileValue(tileMap, location))
		{
			tileData.sprite = m_Sprites[index];
			tileData.transform = GetTransform((byte)num);
			tileData.color = Color.white;
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
		case 1:
		case 4:
		case 16:
		case 64:
			return 1;
		case 5:
		case 20:
		case 65:
		case 80:
			return 2;
		case 7:
		case 28:
		case 112:
		case 193:
			return 3;
		case 17:
		case 68:
			return 4;
		case 21:
		case 69:
		case 81:
		case 84:
			return 5;
		case 23:
		case 92:
		case 113:
		case 197:
			return 6;
		case 29:
		case 71:
		case 116:
		case 209:
			return 7;
		case 31:
		case 124:
		case 199:
		case 241:
			return 8;
		case 85:
			return 9;
		case 87:
		case 93:
		case 117:
		case 213:
			return 10;
		case 95:
		case 125:
		case 215:
		case 245:
			return 11;
		case 119:
		case 221:
			return 12;
		case 127:
		case 223:
		case 247:
		case 253:
			return 13;
		case byte.MaxValue:
			return 14;
		default:
			return -1;
		}
	}

	private Matrix4x4 GetTransform(byte mask)
	{
		switch (mask)
		{
		case 4:
		case 20:
		case 28:
		case 68:
		case 84:
		case 92:
		case 93:
		case 116:
		case 124:
		case 125:
		case 221:
		case 253:
			return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -90f), Vector3.one);
		case 16:
		case 80:
		case 81:
		case 112:
		case 113:
		case 117:
		case 209:
		case 241:
		case 245:
		case 247:
			return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -180f), Vector3.one);
		case 64:
		case 65:
		case 69:
		case 71:
		case 193:
		case 197:
		case 199:
		case 213:
		case 215:
		case 223:
			return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -270f), Vector3.one);
		default:
			return Matrix4x4.identity;
		}
	}
}
