public class SaveDataTornado : SaveDataMovingTileObject
{
	public GameDate expiryDate;

	public int radius;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		Tornado tornado = tileObject as Tornado;
		expiryDate = tornado.expiryDate;
		radius = tornado.radius;
	}
}
