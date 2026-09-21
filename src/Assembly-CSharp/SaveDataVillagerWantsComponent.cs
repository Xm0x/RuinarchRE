using System;

public class SaveDataVillagerWantsComponent : SaveData<VillagerWantsComponent>
{
	public Type[] wantsToProcess;

	public override void Save(VillagerWantsComponent data)
	{
		base.Save(data);
		wantsToProcess = new Type[data.wantsToProcess.Count];
		for (int i = 0; i < data.wantsToProcess.Count; i++)
		{
			wantsToProcess[i] = data.wantsToProcess[i].GetType();
		}
	}

	public override VillagerWantsComponent Load()
	{
		return new VillagerWantsComponent(this);
	}
}
