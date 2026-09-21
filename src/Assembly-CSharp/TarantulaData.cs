public class TarantulaData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TARANTULA;

	public override string name => "Tarantula";

	public override string description => "Tarantula";

	public TarantulaData()
	{
		base.summonType = SUMMON_TYPE.Tarantula;
		base.race = RACE.SPIDER;
		base.className = "Tarantula";
	}
}
