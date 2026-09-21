public class Anvil : TileObject
{
	public Anvil()
	{
		Initialize(TILE_OBJECT_TYPE.ANVIL);
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public Anvil(SaveDataTileObject data)
		: base(data)
	{
	}
}
