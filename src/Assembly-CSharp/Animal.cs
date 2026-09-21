using UtilityScripts;

public abstract class Animal : Summon
{
	public Animal(SUMMON_TYPE summonType, string className, RACE race)
		: base(summonType, className, race, Utilities.GetRandomGender())
	{
	}

	public Animal(SaveDataSummon data)
		: base(data)
	{
	}
}
