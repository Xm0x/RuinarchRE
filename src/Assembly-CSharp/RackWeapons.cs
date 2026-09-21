public class RackWeapons : TileObject
{
	public RackWeapons()
	{
		Initialize(TILE_OBJECT_TYPE.RACK_WEAPONS);
	}

	public RackWeapons(SaveDataTileObject data)
		: base(data)
	{
	}
}
