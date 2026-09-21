public class HerbPlant : TileObject
{
	public HerbPlant()
	{
		Initialize(TILE_OBJECT_TYPE.HERB_PLANT, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		AddAdvertisedAction(INTERACTION_TYPE.GATHER_HERB);
		AddAdvertisedAction(INTERACTION_TYPE.CREATE_WORKPLACE_POTION);
		AddAdvertisedAction(INTERACTION_TYPE.CREATE_HOSPICE_ANTIDOTE);
		AddAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
	}

	public HerbPlant(SaveDataTileObject data)
		: base(data)
	{
	}
}
