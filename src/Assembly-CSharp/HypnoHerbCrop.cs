public class HypnoHerbCrop : Crops
{
	public override TILE_OBJECT_TYPE producedObjectOnHarvest => TILE_OBJECT_TYPE.HYPNO_HERB;

	public override bool isFarmCrop => true;

	public HypnoHerbCrop()
	{
		Initialize(TILE_OBJECT_TYPE.HYPNO_HERB_CROP);
	}

	public HypnoHerbCrop(SaveDataCrops data)
		: base(data)
	{
	}

	public override int GetRipeningTicks()
	{
		return DefaultRipeningTicksBasedOnLocation();
	}
}
