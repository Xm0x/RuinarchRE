public class PineappleCrop : Crops
{
	public override TILE_OBJECT_TYPE producedObjectOnHarvest => TILE_OBJECT_TYPE.PINEAPPLE;

	public override bool isFarmCrop => true;

	public PineappleCrop()
	{
		Initialize(TILE_OBJECT_TYPE.PINEAPPLE_CROP);
	}

	public PineappleCrop(SaveDataCrops data)
		: base(data)
	{
	}

	public override int GetRipeningTicks()
	{
		return DefaultRipeningTicksBasedOnLocation();
	}
}
