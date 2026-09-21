using System;

[Serializable]
public class SaveDataInterruptComponent : SaveData<InterruptComponent>
{
	public string currentInterruptID;

	public int currentDuration;

	public string triggeredSimultaneousInterruptID;

	public int currentSimultaneousInterruptDuration;

	public override void Save(InterruptComponent data)
	{
		currentDuration = data.currentDuration;
		currentSimultaneousInterruptDuration = data.currentSimultaneousInterruptDuration;
		if (data.currentInterrupt != null && data.currentInterrupt.interrupt != null)
		{
			currentInterruptID = data.currentInterrupt.persistentID;
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentInterrupt);
		}
		if (data.triggeredSimultaneousInterrupt != null && data.triggeredSimultaneousInterrupt.interrupt != null)
		{
			triggeredSimultaneousInterruptID = data.triggeredSimultaneousInterrupt.persistentID;
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.triggeredSimultaneousInterrupt);
		}
	}

	public override InterruptComponent Load()
	{
		return new InterruptComponent(this);
	}
}
