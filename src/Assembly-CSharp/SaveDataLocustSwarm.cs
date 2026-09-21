public class SaveDataLocustSwarm : SaveDataMovingTileObject
{
	public GameDate expiryDate;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		LocustSwarm locustSwarm = tileObject as LocustSwarm;
		expiryDate = locustSwarm.expiryDate;
	}
}
