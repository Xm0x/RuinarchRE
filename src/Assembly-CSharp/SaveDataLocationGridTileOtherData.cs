public class SaveDataLocationGridTileOtherData : SaveDataOtherData
{
	public TileLocationSave tileID;

	public override void Save(OtherData data)
	{
		base.Save(data);
		LocationGridTileOtherData locationGridTileOtherData = data as LocationGridTileOtherData;
		if (locationGridTileOtherData.tile != null)
		{
			tileID = new TileLocationSave(locationGridTileOtherData.tile);
		}
	}

	public override OtherData Load()
	{
		return new LocationGridTileOtherData(this);
	}
}
