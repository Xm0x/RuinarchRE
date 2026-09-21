using System;

public class CornCrop : Crops
{
	public override Type serializedData => typeof(SaveDataCornCrop);

	public override TILE_OBJECT_TYPE producedObjectOnHarvest => TILE_OBJECT_TYPE.CORN;

	public override bool isFarmCrop => true;

	public CornCrop()
	{
		Initialize(TILE_OBJECT_TYPE.CORN_CROP);
	}

	public CornCrop(SaveDataCornCrop data)
		: base(data)
	{
	}

	public override int GetRipeningTicks()
	{
		return DefaultRipeningTicksBasedOnLocation();
	}
}
