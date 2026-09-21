public class SaveDataAreaOtherData : SaveDataOtherData
{
	public string areaID;

	public override void Save(OtherData data)
	{
		base.Save(data);
		AreaOtherData areaOtherData = data as AreaOtherData;
		areaID = areaOtherData.area.persistentID;
	}

	public override OtherData Load()
	{
		return new AreaOtherData(this);
	}
}
