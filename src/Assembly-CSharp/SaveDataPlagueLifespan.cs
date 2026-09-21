using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataPlagueLifespan : SaveData<PlagueLifespan>
{
	public int tileObjectInfectionTimeInHours;

	public int monsterInfectionTimeInHours;

	public int undeadInfectionTimeInHours;

	public Dictionary<RACE, int> sapientInfectionTimeInHours;

	public override void Save(PlagueLifespan p_data)
	{
		tileObjectInfectionTimeInHours = p_data.tileObjectInfectionTimeInHours;
		monsterInfectionTimeInHours = p_data.monsterInfectionTimeInHours;
		undeadInfectionTimeInHours = p_data.undeadInfectionTimeInHours;
		sapientInfectionTimeInHours = new Dictionary<RACE, int>();
		foreach (KeyValuePair<RACE, int> sapientInfectionTimeInHour in p_data.sapientInfectionTimeInHours)
		{
			sapientInfectionTimeInHours.Add(sapientInfectionTimeInHour.Key, sapientInfectionTimeInHour.Value);
		}
	}

	public override PlagueLifespan Load()
	{
		return new PlagueLifespan(this);
	}
}
