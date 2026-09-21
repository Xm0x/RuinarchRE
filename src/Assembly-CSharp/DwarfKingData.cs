public class DwarfKingData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DWARF_KING;

	public override string name => "Dwarf King";

	public override string description => "Dwarf King";

	public DwarfKingData()
	{
		base.summonType = SUMMON_TYPE.Dwarf_King;
		base.race = RACE.DWARF;
		base.className = "Dwarf King";
	}
}
