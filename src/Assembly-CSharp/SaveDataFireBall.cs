public class SaveDataFireBall : SaveDataMovingTileObject
{
	public GameDate expiryDate;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		FireBall fireBall = tileObject as FireBall;
		expiryDate = fireBall.expiryDate;
	}
}
