using System.Collections.Generic;

public class SaveDataFactionCharactersComponent : SaveData<FactionCharactersComponent>
{
	public List<AwarenessData> characterAwarenessData;

	public override void Save(FactionCharactersComponent data)
	{
		characterAwarenessData = new List<AwarenessData>();
		foreach (AwarenessData value in data.characterAwarenessData.Values)
		{
			characterAwarenessData.Add(value);
		}
	}

	public override FactionCharactersComponent Load()
	{
		return new FactionCharactersComponent(this);
	}
}
