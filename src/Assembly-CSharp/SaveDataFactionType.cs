using System;
using System.Collections.Generic;
using Factions.Faction_Types;

[Serializable]
public class SaveDataFactionType : SaveData<FactionType>
{
	public FACTION_TYPE type;

	public List<SaveDataFactionIdeology> ideologies;

	public Dictionary<CRIME_TYPE, CRIME_SEVERITY> crimes;

	public bool hasCrimes;

	public override void Save(FactionType data)
	{
		type = data.type;
		ideologies = new List<SaveDataFactionIdeology>();
		if (data.ideologies != null)
		{
			for (int i = 0; i < data.ideologies.Count; i++)
			{
				FactionIdeology data2 = data.ideologies[i];
				SaveDataFactionIdeology saveDataFactionIdeology = new SaveDataFactionIdeology();
				saveDataFactionIdeology.Save(data2);
				ideologies.Add(saveDataFactionIdeology);
			}
		}
		crimes = new Dictionary<CRIME_TYPE, CRIME_SEVERITY>(data.crimes);
		hasCrimes = data.hasCrimes;
	}

	public override FactionType Load()
	{
		FactionType factionType = FactionManager.Instance.CreateFactionType(type, this);
		factionType.SetFixedData();
		return factionType;
	}
}
