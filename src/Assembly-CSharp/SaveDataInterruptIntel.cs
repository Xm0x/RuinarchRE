using System;

[Serializable]
public class SaveDataInterruptIntel : SaveData<InterruptIntel>
{
	public string interruptHolder;

	public override void Save(InterruptIntel data)
	{
		interruptHolder = data.interruptHolder.persistentID;
		SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.interruptHolder);
	}

	public override InterruptIntel Load()
	{
		return new InterruptIntel(this);
	}
}
