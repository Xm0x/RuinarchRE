using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataRevenant : SaveDataSummon
{
	public List<string> betrayers;

	public override void Save(Character data)
	{
		base.Save(data);
		if (data is Revenant revenant)
		{
			betrayers = new List<string>();
			for (int i = 0; i < revenant.betrayers.Count; i++)
			{
				betrayers.Add(revenant.betrayers[i].persistentID);
			}
		}
	}
}
