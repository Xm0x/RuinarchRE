public class TableAlchemy : TileObject
{
	public override string description => LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", "NoValue_Flavor_Text");

	public TableAlchemy()
	{
		Initialize(TILE_OBJECT_TYPE.TABLE_ALCHEMY);
	}

	public TableAlchemy(SaveDataTileObject data)
		: base(data)
	{
	}
}
