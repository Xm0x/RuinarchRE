using System;

namespace UnityEngine.Tilemaps;

[Serializable]
public class WeightedRandomTile : Tile
{
	[SerializeField]
	public WeightedSprite[] Sprites;

	public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
	{
		base.GetTileData(location, tileMap, ref tileData);
		if (Sprites == null || Sprites.Length == 0)
		{
			return;
		}
		long num = location.x;
		num = num + 2882343476u + (num << 15);
		num = (num + 159903659) ^ (num >> 11);
		num ^= location.y;
		num = num + 1185682173 + (num << 7);
		num = (num + 3197579439u) ^ (num << 11);
		Random.InitState((int)num);
		int num2 = 0;
		WeightedSprite[] sprites = Sprites;
		for (int i = 0; i < sprites.Length; i++)
		{
			WeightedSprite weightedSprite = sprites[i];
			num2 += weightedSprite.Weight;
		}
		int num3 = Random.Range(0, num2);
		sprites = Sprites;
		for (int i = 0; i < sprites.Length; i++)
		{
			WeightedSprite weightedSprite2 = sprites[i];
			num3 -= weightedSprite2.Weight;
			if (num3 < 0)
			{
				tileData.sprite = weightedSprite2.Sprite;
				break;
			}
		}
	}
}
