public class TrollCauldron : TileObject
{
	public override string description => LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", "NoValue_Flavor_Text");

	public TrollCauldron()
	{
		Initialize(TILE_OBJECT_TYPE.TROLL_CAULDRON);
	}

	public TrollCauldron(SaveDataTileObject data)
		: base(data)
	{
	}
}
