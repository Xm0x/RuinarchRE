public class RatData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RAT;

	public override string name => "Rat";

	public override string description => "Rat";

	public RatData()
	{
		base.summonType = SUMMON_TYPE.Rat;
		base.race = RACE.RAT;
		base.className = "Rat";
	}
}
