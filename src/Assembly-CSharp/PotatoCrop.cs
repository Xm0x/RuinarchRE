public class PotatoCrop : Crops
{
	public override TILE_OBJECT_TYPE producedObjectOnHarvest => TILE_OBJECT_TYPE.POTATO;

	public override bool isFarmCrop => true;

	public PotatoCrop()
	{
		Initialize(TILE_OBJECT_TYPE.POTATO_CROP);
	}

	public PotatoCrop(SaveDataCrops data)
		: base(data)
	{
	}

	public override int GetRipeningTicks()
	{
		return DefaultRipeningTicksBasedOnLocation();
	}
}
