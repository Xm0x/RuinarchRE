using System;

[Serializable]
public class SaveDataSeizeComponent : SaveData<SeizeComponent>
{
	public override void Save(SeizeComponent component)
	{
	}

	public override SeizeComponent Load()
	{
		return new SeizeComponent();
	}
}
