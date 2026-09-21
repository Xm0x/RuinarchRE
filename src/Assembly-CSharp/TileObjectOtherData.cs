public class TileObjectOtherData : OtherData
{
	public TileObject tileObject { get; private set; }

	public override object obj => tileObject;

	public TileObjectOtherData(TileObject tileObject)
	{
		this.tileObject = tileObject;
		if (tileObject is GenericTileObject genericTileObject)
		{
			genericTileObject.gridTileLocation.SetIsDefault(state: false);
		}
	}

	public TileObjectOtherData(SaveDataTileObjectOtherData saveData)
	{
		tileObject = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(saveData.tileObjectID);
	}

	public override SaveDataOtherData Save()
	{
		SaveDataTileObjectOtherData saveDataTileObjectOtherData = new SaveDataTileObjectOtherData();
		saveDataTileObjectOtherData.Save(this);
		return saveDataTileObjectOtherData;
	}

	public override void CleanUp()
	{
		tileObject = null;
	}
}
