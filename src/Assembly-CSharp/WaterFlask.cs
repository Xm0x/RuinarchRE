public class WaterFlask : TileObject
{
	public WaterFlask()
	{
		Initialize(TILE_OBJECT_TYPE.WATER_FLASK);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public WaterFlask(SaveDataTileObject data)
		: base(data)
	{
	}

	public override string ToString()
	{
		return "Water Flask " + base.id;
	}
}
