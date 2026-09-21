public class SnowEntData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNOW_ENT;

	public override string name => "Snow Ent";

	public override string description => "Snow Ent";

	public SnowEntData()
	{
		base.summonType = SUMMON_TYPE.Snow_Ent;
		base.race = RACE.ENT;
		base.className = "Snow Ent";
	}
}
