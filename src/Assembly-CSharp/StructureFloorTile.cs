using UnityEngine;
using UnityEngine.Tilemaps;

public class StructureFloorTile : Tile
{
	public Sprite[] evenSprite;

	public Sprite[] oddSprite;

	public override void RefreshTile(Vector3Int location, ITilemap tilemap)
	{
	}

	public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
	{
		if (location.y % 2 == 0)
		{
			if (location.x % 2 == 0)
			{
				tileData.sprite = evenSprite[Random.Range(0, evenSprite.Length)];
			}
			else
			{
				tileData.sprite = oddSprite[Random.Range(0, oddSprite.Length)];
			}
		}
		else if (location.x % 2 == 0)
		{
			tileData.sprite = oddSprite[Random.Range(0, oddSprite.Length)];
		}
		else
		{
			tileData.sprite = evenSprite[Random.Range(0, evenSprite.Length)];
		}
		tileData.color = Color.white;
		Matrix4x4 matrix4x = tileData.transform;
		matrix4x.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
		tileData.transform = matrix4x;
		tileData.flags = TileFlags.LockTransform;
		tileData.colliderType = ColliderType.None;
	}
}
