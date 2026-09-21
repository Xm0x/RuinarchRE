using System;

[Serializable]
public class SaveDataGatheringComponent : SaveData<GatheringComponent>
{
	public string currentGathering;

	public override void Save(GatheringComponent data)
	{
		if (data.hasGathering)
		{
			currentGathering = data.currentGathering.persistentID;
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentGathering);
		}
	}

	public override GatheringComponent Load()
	{
		return new GatheringComponent(this);
	}
}
