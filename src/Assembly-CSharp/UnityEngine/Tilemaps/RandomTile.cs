using System;

namespace UnityEngine.Tilemaps;

[Serializable]
public class RandomTile : Tile
{
	[SerializeField]
	public Sprite[] m_Sprites;

	public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
	{
		base.GetTileData(location, tileMap, ref tileData);
		if (m_Sprites != null && m_Sprites.Length != 0)
		{
			long num = location.x;
			num = num + 2882343476u + (num << 15);
			num = (num + 159903659) ^ (num >> 11);
			num ^= location.y;
			num = num + 1185682173 + (num << 7);
			num = (num + 3197579439u) ^ (num << 11);
			Random.InitState((int)num);
			tileData.sprite = m_Sprites[(int)((float)m_Sprites.Length * Random.value)];
		}
	}
}
