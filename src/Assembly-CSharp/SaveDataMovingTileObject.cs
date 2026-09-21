using UnityEngine;

public class SaveDataMovingTileObject : SaveDataTileObject
{
	public Vector3 mapVisualWorldPosition;

	public bool hasExpired;

	public bool isPlayerSource;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		MovingTileObject movingTileObject = tileObject as MovingTileObject;
		if (movingTileObject.movingMapVisual != null)
		{
			mapVisualWorldPosition = movingTileObject.movingMapVisual.worldPos;
		}
		hasExpired = movingTileObject.hasExpired;
		isPlayerSource = movingTileObject.isPlayerSource;
	}
}
