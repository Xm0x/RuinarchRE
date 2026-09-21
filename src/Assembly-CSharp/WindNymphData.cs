public class WindNymphData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WIND_NYMPH;

	public override string name => "Wind Nymph";

	public override string description => "Wind Nymph";

	public WindNymphData()
	{
		base.summonType = SUMMON_TYPE.Wind_Nymph;
		base.race = RACE.NYMPH;
		base.className = "Wind Nymph";
	}
}
