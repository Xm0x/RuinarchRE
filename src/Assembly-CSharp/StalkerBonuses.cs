using System;

[Serializable]
public class StalkerBonuses
{
	public bool cannotBeTurned;

	public bool dealDoubleDamage;

	public bool canIdentifyVampireAndLycans;

	public bool canIdentifyCultists;

	public StalkerBonuses()
	{
	}

	public StalkerBonuses(StalkerBonuses p_copy)
	{
		cannotBeTurned = p_copy.cannotBeTurned;
		dealDoubleDamage = p_copy.dealDoubleDamage;
		canIdentifyVampireAndLycans = p_copy.canIdentifyVampireAndLycans;
		canIdentifyCultists = p_copy.canIdentifyCultists;
	}
}
