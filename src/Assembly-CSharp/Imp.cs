using UtilityScripts;

public class Imp : Summon
{
	public override Faction defaultFaction => FactionManager.Instance.undeadFaction;

	public Imp()
		: base(SUMMON_TYPE.Imp, "Imp", RACE.IMP, Utilities.GetRandomGender())
	{
	}

	public Imp(string className)
		: base(SUMMON_TYPE.Imp, className, RACE.IMP, Utilities.GetRandomGender())
	{
	}

	public Imp(SaveDataSummon data)
		: base(data)
	{
	}
}
