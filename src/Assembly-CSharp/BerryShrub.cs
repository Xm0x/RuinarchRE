using System;

public class BerryShrub : Crops
{
	public override Type serializedData => typeof(SaveDataBerryShrub);

	public override TILE_OBJECT_TYPE producedObjectOnHarvest => TILE_OBJECT_TYPE.VEGETABLES;

	public override bool isFarmCrop => false;

	public BerryShrub()
	{
		Initialize(TILE_OBJECT_TYPE.BERRY_SHRUB);
	}

	public BerryShrub(SaveDataBerryShrub data)
		: base(data)
	{
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		base.ConstructDefaultPlayerActions(broadcastSignal);
	}

	public override int GetRipeningTicks()
	{
		return GameManager.Instance.GetTicksBasedOnHour(24);
	}

	public override string ToString()
	{
		return "Berry Shrub " + base.id;
	}
}
