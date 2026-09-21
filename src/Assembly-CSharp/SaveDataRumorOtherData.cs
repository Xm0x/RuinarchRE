using Interrupts;

public class SaveDataRumorOtherData : SaveDataOtherData
{
	public string rumorableID;

	public OBJECT_TYPE rumorableObjectType;

	public override void Save(OtherData data)
	{
		base.Save(data);
		RumorOtherData rumorOtherData = data as RumorOtherData;
		rumorableID = rumorOtherData.rumor.rumorable.persistentID;
		rumorableObjectType = rumorOtherData.rumor.rumorable.objectType;
		if (rumorOtherData.rumor.rumorable is ActualGoapNode data2)
		{
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data2);
		}
		else if (rumorOtherData.rumor.rumorable is InterruptHolder data3)
		{
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data3);
		}
	}

	public override OtherData Load()
	{
		return new RumorOtherData(this);
	}
}
