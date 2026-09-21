public class Plant : TileObject
{
	public Plant()
	{
		Initialize(TILE_OBJECT_TYPE.PLANT, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
	}

	public Plant(SaveDataTileObject data)
		: base(data)
	{
	}
}
