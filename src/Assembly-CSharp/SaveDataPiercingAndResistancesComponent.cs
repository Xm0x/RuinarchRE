using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataPiercingAndResistancesComponent : SaveData<PiercingAndResistancesComponent>
{
	public float piercingPower;

	public Dictionary<RESISTANCE, float> resistances;

	public Dictionary<RESISTANCE, float> resistancesMultipliers;

	public float piercingMultiplier;

	public float basePiercing;

	public override void Save(PiercingAndResistancesComponent data)
	{
		piercingPower = data.piercingPower;
		resistances = new Dictionary<RESISTANCE, float>();
		foreach (KeyValuePair<RESISTANCE, float> resistance in data.resistances)
		{
			resistances.Add(resistance.Key, resistance.Value);
		}
		resistancesMultipliers = new Dictionary<RESISTANCE, float>();
		foreach (KeyValuePair<RESISTANCE, float> resistancesMultiplier in data.resistancesMultipliers)
		{
			resistancesMultipliers.Add(resistancesMultiplier.Key, resistancesMultiplier.Value);
		}
		piercingMultiplier = data.piercingMultiplier;
		basePiercing = data.basePiercing;
	}

	public override PiercingAndResistancesComponent Load()
	{
		return new PiercingAndResistancesComponent(this);
	}

	public override void CleanUp()
	{
		resistances?.Clear();
		resistances = null;
		resistancesMultipliers?.Clear();
		resistancesMultipliers = null;
	}
}
