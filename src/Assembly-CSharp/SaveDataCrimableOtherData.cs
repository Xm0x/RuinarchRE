using Interrupts;

public class SaveDataCrimableOtherData : SaveDataOtherData
{
	public string rumorableID;

	public OBJECT_TYPE rumorableObjectType;

	public override void Save(OtherData data)
	{
		base.Save(data);
		CrimeableOtherData crimeableOtherData = data as CrimeableOtherData;
		rumorableID = crimeableOtherData.crimeable.persistentID;
		rumorableObjectType = crimeableOtherData.crimeable.objectType;
		if (crimeableOtherData.crimeable is ActualGoapNode data2)
		{
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data2);
		}
		else if (crimeableOtherData.crimeable is InterruptHolder data3)
		{
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data3);
		}
	}

	public override OtherData Load()
	{
		return new CrimeableOtherData(this);
	}
}
