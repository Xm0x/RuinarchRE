public class SaveDataFrostyFog : SaveDataMovingTileObject
{
	public GameDate expiryDate;

	public int stacks;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		FrostyFog frostyFog = tileObject as FrostyFog;
		expiryDate = frostyFog.expiryDate;
		stacks = frostyFog.stacks;
	}
}
