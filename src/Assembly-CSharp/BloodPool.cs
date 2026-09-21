public class BloodPool : TileObject
{
	public BloodPool()
	{
		Initialize(TILE_OBJECT_TYPE.BLOOD_POOL);
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public BloodPool(SaveDataTileObject data)
		: base(data)
	{
	}
}
