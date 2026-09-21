public class DemonCircle : TileObject
{
	public DemonCircle()
	{
		Initialize(TILE_OBJECT_TYPE.DEMON_CIRCLE);
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public DemonCircle(SaveDataTileObject data)
		: base(data)
	{
	}
}
