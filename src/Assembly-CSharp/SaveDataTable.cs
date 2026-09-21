public class SaveDataTable : SaveDataTileObject
{
	public CONCRETE_RESOURCES lastAddedFoodType;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		Table table = tileObject as Table;
		lastAddedFoodType = table.lastAddedFoodType;
	}
}
