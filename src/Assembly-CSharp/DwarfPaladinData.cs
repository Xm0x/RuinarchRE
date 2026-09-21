public class DwarfPaladinData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DWARF_PALADIN;

	public override string name => "Dwarf Paladin";

	public override string description => "Dwarf Paladin";

	public DwarfPaladinData()
	{
		base.summonType = SUMMON_TYPE.Dwarf_Paladin;
		base.race = RACE.DWARF;
		base.className = "Dwarf Paladin";
	}
}
