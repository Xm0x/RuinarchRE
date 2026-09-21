public class SaveDataPoisonCloud : SaveDataMovingTileObject
{
	public GameDate expiryDate;

	public int stacks;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		PoisonCloud poisonCloud = tileObject as PoisonCloud;
		expiryDate = poisonCloud.expiryDate;
		stacks = poisonCloud.stacks;
	}

	public override TileObject Load()
	{
		TileObject tileObject = base.Load();
		(tileObject as PoisonCloud).SetStacks(stacks);
		return tileObject;
	}
}
