public class SaveDataCrops : SaveDataTileObject
{
	public Crops.Growth_State growthState;

	public int remainingRipeningTicks;

	public int growthRate;

	public int count;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		Crops crops = tileObject as Crops;
		growthState = crops.currentGrowthState;
		remainingRipeningTicks = crops.remainingRipeningTicks;
		growthRate = crops.growthRate;
		count = crops.count;
	}

	public override TileObject Load()
	{
		TileObject tileObject = base.Load();
		(tileObject as Crops).count = count;
		return tileObject;
	}
}
