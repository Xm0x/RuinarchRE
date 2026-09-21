using System;

[Serializable]
public class SaveDataEnt : SaveDataSummon
{
	public bool isTree;

	public override void Save(Character data)
	{
		base.Save(data);
		if (data is Ent ent)
		{
			isTree = ent.isTree;
		}
	}
}
