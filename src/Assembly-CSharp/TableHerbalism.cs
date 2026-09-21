public class TableHerbalism : TileObject
{
	public override string description => LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", "NoValue_Flavor_Text");

	public TableHerbalism()
	{
		Initialize(TILE_OBJECT_TYPE.TABLE_HERBALISM);
	}

	public TableHerbalism(SaveDataTileObject data)
		: base(data)
	{
	}
}
