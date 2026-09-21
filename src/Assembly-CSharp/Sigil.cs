public class Sigil : TileObject
{
	public Sigil()
	{
		Initialize(TILE_OBJECT_TYPE.SIGIL);
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public Sigil(SaveDataTileObject data)
		: base(data)
	{
	}
}
