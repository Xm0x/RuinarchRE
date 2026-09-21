public class SaveDataCrimeDataOtherData : SaveDataOtherData
{
	public string crimeDataID;

	public override void Save(OtherData data)
	{
		base.Save(data);
		CrimeDataOtherData crimeDataOtherData = data as CrimeDataOtherData;
		crimeDataID = crimeDataOtherData.crimeData.persistentID;
		SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(crimeDataOtherData.crimeData);
	}

	public override OtherData Load()
	{
		return new CrimeDataOtherData(this);
	}
}
