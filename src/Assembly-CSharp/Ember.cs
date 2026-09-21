public class Ember : TileObject
{
	public Ember()
	{
		Initialize(TILE_OBJECT_TYPE.EMBER, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
	}

	public Ember(SaveDataTileObject data)
		: base(data)
	{
	}
}
