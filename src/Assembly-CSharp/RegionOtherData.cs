public class RegionOtherData : OtherData
{
	public Region region { get; private set; }

	public override object obj => region;

	public RegionOtherData(Region region)
	{
		this.region = region;
	}

	public RegionOtherData(SaveDataRegionOtherData saveData)
	{
		region = DatabaseManager.Instance.regionDatabase.mainRegion;
	}

	public override SaveDataOtherData Save()
	{
		SaveDataRegionOtherData saveDataRegionOtherData = new SaveDataRegionOtherData();
		saveDataRegionOtherData.Save(this);
		return saveDataRegionOtherData;
	}

	public override void CleanUp()
	{
		region = null;
	}
}
