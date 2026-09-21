using System.Collections.Generic;

public class SaveDataFactionOpinionComponent : SaveData<FactionOpinionComponent>
{
	public Dictionary<string, List<string>> factionOpinions;

	public override void Save(FactionOpinionComponent data)
	{
		base.Save(data);
		factionOpinions = new Dictionary<string, List<string>>();
		foreach (KeyValuePair<Character, List<SharedOpinionModifier>> factionOpinion in data.factionOpinions)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < factionOpinion.Value.Count; i++)
			{
				SharedOpinionModifier sharedOpinionModifier = factionOpinion.Value[i];
				list.Add(sharedOpinionModifier.persistentID);
			}
			factionOpinions.Add(factionOpinion.Key.persistentID, list);
		}
	}

	public override FactionOpinionComponent Load()
	{
		return new FactionOpinionComponent();
	}
}
