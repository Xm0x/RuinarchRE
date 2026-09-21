public class SaveDataActualGoapNodeOtherData : SaveDataOtherData
{
	public string actionID;

	public override void Save(OtherData data)
	{
		base.Save(data);
		ActualGoapNodeOtherData actualGoapNodeOtherData = data as ActualGoapNodeOtherData;
		actionID = actualGoapNodeOtherData.action.persistentID;
		SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(actualGoapNodeOtherData.action);
	}

	public override OtherData Load()
	{
		return new ActualGoapNodeOtherData(this);
	}
}
