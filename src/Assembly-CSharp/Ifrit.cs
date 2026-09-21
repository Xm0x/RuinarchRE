using UtilityScripts;

public class Ifrit : Summon
{
	public override bool defaultDigMode => true;

	public Ifrit()
		: base(SUMMON_TYPE.Ifrit, "Ifrit", RACE.IFRIT, Utilities.GetRandomGender())
	{
	}

	public Ifrit(string className)
		: base(SUMMON_TYPE.Ifrit, className, RACE.IFRIT, Utilities.GetRandomGender())
	{
	}

	public Ifrit(SaveDataSummon data)
		: base(data)
	{
	}
}
