using System.Collections.Generic;

public class PrimordialPoolDataHandler
{
	public Dictionary<CHARACTER_CATEGORY, PrimordialStatsData> bonusPerCategory = new Dictionary<CHARACTER_CATEGORY, PrimordialStatsData>
	{
		{
			CHARACTER_CATEGORY.Villager,
			new PrimordialStatsData()
		},
		{
			CHARACTER_CATEGORY.Beast,
			new PrimordialStatsData()
		},
		{
			CHARACTER_CATEGORY.Humanoid,
			new PrimordialStatsData()
		},
		{
			CHARACTER_CATEGORY.Demonic,
			new PrimordialStatsData()
		},
		{
			CHARACTER_CATEGORY.Undead,
			new PrimordialStatsData()
		}
	};

	public PrimordialPoolDataHandler(SaveDataPrimordialPoolDataHandler data)
	{
		bonusPerCategory.Clear();
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

	public PrimordialPoolDataHandler()
	{
	}
}
