public class IceberryCrop : Crops
{
	public override TILE_OBJECT_TYPE producedObjectOnHarvest => TILE_OBJECT_TYPE.ICEBERRY;

	public override bool isFarmCrop => true;

	public IceberryCrop()
	{
		Initialize(TILE_OBJECT_TYPE.ICEBERRY_CROP);
	}

	public IceberryCrop(SaveDataCrops data)
		: base(data)
	{
	}

	public override int GetRipeningTicks()
	{
		return DefaultRipeningTicksBasedOnLocation();
	}
}
