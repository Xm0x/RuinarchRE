using UtilityScripts;

public class DwarfKing : Summon
{
	public override bool defaultDigMode => true;

	public DwarfKing()
		: base(SUMMON_TYPE.Dwarf_King, "Dwarf King", RACE.DWARF, Utilities.GetRandomGender())
	{
	}

	public DwarfKing(string className)
		: base(SUMMON_TYPE.Dwarf_King, className, RACE.DWARF, Utilities.GetRandomGender())
	{
	}

	public DwarfKing(SaveDataSummon data)
		: base(data)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.piercingAndResistancesComponent.SetResistance(RESISTANCE.Physical, 80f);
		base.piercingAndResistancesComponent.SetResistance(RESISTANCE.Mental, 80f);
	}
}
