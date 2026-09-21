public class BearData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BEAR;

	public override string name => "Bear";

	public override string description => "Bear";

	public BearData()
	{
		base.summonType = SUMMON_TYPE.Bear;
		base.race = RACE.BEAR;
		base.className = "Bear";
	}
}
