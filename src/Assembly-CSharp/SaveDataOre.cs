public class SaveDataOre : SaveDataTileObject
{
	public int yield;

	public int count;

	public CONCRETE_RESOURCES providedMetal;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		Ore ore = tileObject as Ore;
		yield = ore.yield;
		count = ore.count;
		providedMetal = ore.providedMetal;
	}

	public override TileObject Load()
	{
		Ore obj = base.Load() as Ore;
		obj.count = count;
		return obj;
	}
}
