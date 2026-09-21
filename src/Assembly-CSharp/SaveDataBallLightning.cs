public class SaveDataBallLightning : SaveDataMovingTileObject
{
	public GameDate expiryDate;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		BallLightning ballLightning = tileObject as BallLightning;
		expiryDate = ballLightning.expiryDate;
	}
}
