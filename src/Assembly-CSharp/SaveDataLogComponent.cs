using System;

[Serializable]
public class SaveDataLogComponent : SaveData<LogComponent>
{
	public override void Save(LogComponent data)
	{
	}

	public override LogComponent Load()
	{
		return new LogComponent(this);
	}
}
