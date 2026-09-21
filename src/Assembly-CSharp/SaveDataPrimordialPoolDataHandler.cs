using System.Collections.Generic;

public class SaveDataPrimordialPoolDataHandler : SaveData<PrimordialPoolDataHandler>
{
	public Dictionary<CHARACTER_CATEGORY, PrimordialStatsData> bonusPerCategory = new Dictionary<CHARACTER_CATEGORY, PrimordialStatsData>();

	public override void Save(PrimordialPoolDataHandler data)
	{
		base.Save(data);
		int num = 0;
		foreach (KeyValuePair<CHARACTER_CATEGORY, PrimordialStatsData> item in data.bonusPerCategory)
		{
			CHARACTER_CATEGORY key = (CHARACTER_CATEGORY)num;
			bonusPerCategory.Add(key, new PrimordialStatsData());
			bonusPerCategory[key].lvlStr = item.Value.lvlStr;
			bonusPerCategory[key].lvlInt = item.Value.lvlInt;
			bonusPerCategory[key].lvlPiercing = item.Value.lvlPiercing;
			bonusPerCategory[key].lvlMentalResistance = item.Value.lvlMentalResistance;
			bonusPerCategory[key].lvlPhysicalResistance = item.Value.lvlPhysicalResistance;
			bonusPerCategory[key].lvlElementalResistance = item.Value.lvlElementalResistance;
			bonusPerCategory[key].lvlSecondaryResistance = item.Value.lvlSecondaryResistance;
			num++;
		}
	}

	public override PrimordialPoolDataHandler Load()
	{
		return new PrimordialPoolDataHandler(this);
	}
}
