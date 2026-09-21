public class SmithingForge : TileObject
{
	public override string description => LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", "NoValue_Flavor_Text");

	public SmithingForge()
	{
		Initialize(TILE_OBJECT_TYPE.SMITHING_FORGE);
	}

	public SmithingForge(SaveDataTileObject data)
		: base(data)
	{
	}
}
