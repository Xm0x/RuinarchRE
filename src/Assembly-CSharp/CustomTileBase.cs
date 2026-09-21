using UnityEngine;
using UnityEngine.Tilemaps;

public class CustomTileBase : TileBase
{
	public Sprite sprite;

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		tileData.sprite = sprite;
	}
}
