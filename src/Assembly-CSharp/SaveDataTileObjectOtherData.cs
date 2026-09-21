public class SaveDataTileObjectOtherData : SaveDataOtherData
{
	public string tileObjectID;

	public override void Save(OtherData data)
	{
		base.Save(data);
		TileObjectOtherData tileObjectOtherData = data as TileObjectOtherData;
		tileObjectID = tileObjectOtherData.tileObject.persistentID;
	}

	public override OtherData Load()
	{
		return new TileObjectOtherData(this);
	}
}
