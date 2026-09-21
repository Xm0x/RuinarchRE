public class SaveDataRegionOtherData : SaveDataOtherData
{
	public string regionID;

	public override void Save(OtherData data)
	{
		base.Save(data);
		RegionOtherData regionOtherData = data as RegionOtherData;
		regionID = regionOtherData.region.persistentID;
	}

	public override OtherData Load()
	{
		return new RegionOtherData(this);
	}
}
