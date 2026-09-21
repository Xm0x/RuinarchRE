public class Cauldron : TileObject
{
	public override string description => LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", "NoValue_Flavor_Text");

	public Cauldron()
	{
		Initialize(TILE_OBJECT_TYPE.CAULDRON);
	}

	public Cauldron(SaveDataTileObject data)
		: base(data)
	{
	}
}
