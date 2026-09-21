public class SaveDataVapor : SaveDataMovingTileObject
{
	public GameDate expiryDate;

	public int stacks;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		Vapor vapor = tileObject as Vapor;
		expiryDate = vapor.expiryDate;
		stacks = vapor.stacks;
	}
}
