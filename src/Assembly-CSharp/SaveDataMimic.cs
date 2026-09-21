using System;

[Serializable]
public class SaveDataMimic : SaveDataSummon
{
	public bool isTreasureChest;

	public override void Save(Character data)
	{
		base.Save(data);
		if (data is Mimic mimic)
		{
			isTreasureChest = mimic.isTreasureChest;
		}
	}
}
