using UnityEngine;

public class SaveDataStampede : SaveDataMovingTileObject
{
	public GameDate expiryDate;

	public Vector3 targetDirection;

	public float angle;

	public int width;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		Stampede stampede = tileObject as Stampede;
		expiryDate = stampede.expiryDate;
		targetDirection = stampede.targetDirection;
		angle = stampede.angle;
		width = stampede.width;
	}
}
