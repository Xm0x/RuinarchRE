using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataFactionCrimeComponent : SaveData<FactionCrimeComponent>
{
	public List<string> wantedCharacters;

	public override void Save(FactionCrimeComponent data)
	{
		if (data.wantedCharacters != null)
		{
			wantedCharacters = SaveUtilities.ConvertSavableListToIDs(data.wantedCharacters);
		}
	}

	public override FactionCrimeComponent Load()
	{
		return new FactionCrimeComponent(this);
	}
}
