public class SaveDataTreasureChest : SaveDataTileObject
{
	public OBJECT_TYPE objectInsideType;

	public string objectInsideID;

	public override void Save(TileObject data)
	{
		base.Save(data);
		TreasureChest treasureChest = data as TreasureChest;
		if (treasureChest.objectInside != null)
		{
			objectInsideType = treasureChest.objectInside.objectType;
			objectInsideID = treasureChest.objectInside.persistentID;
		}
	}
}
