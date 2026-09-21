public class ElectricWispData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ELECTRIC_WISP;

	public override string name => "Electric Wisp";

	public override string description => "Electric Wisp";

	public ElectricWispData()
	{
		base.summonType = SUMMON_TYPE.Electric_Wisp;
		base.race = RACE.WISP;
		base.className = "Electric Wisp";
	}
}
