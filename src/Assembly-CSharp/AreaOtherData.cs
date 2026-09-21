public class AreaOtherData : OtherData
{
	public Area area { get; private set; }

	public override object obj => area;

	public AreaOtherData(Area p_area)
	{
		area = p_area;
	}

	public AreaOtherData(SaveDataAreaOtherData p_area)
	{
		area = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(p_area.areaID);
	}

	public override SaveDataOtherData Save()
	{
		SaveDataAreaOtherData saveDataAreaOtherData = new SaveDataAreaOtherData();
		saveDataAreaOtherData.Save(this);
		return saveDataAreaOtherData;
	}

	public override void CleanUp()
	{
		area = null;
	}
}
