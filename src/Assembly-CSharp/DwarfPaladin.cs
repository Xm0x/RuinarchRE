using UtilityScripts;

public class DwarfPaladin : Summon
{
	public override bool defaultDigMode => true;

	public DwarfPaladin()
		: base(SUMMON_TYPE.Dwarf_Paladin, "Dwarf Paladin", RACE.DWARF, Utilities.GetRandomGender())
	{
	}

	public DwarfPaladin(string className)
		: base(SUMMON_TYPE.Dwarf_Paladin, className, RACE.DWARF, Utilities.GetRandomGender())
	{
	}

	public DwarfPaladin(SaveDataSummon data)
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
