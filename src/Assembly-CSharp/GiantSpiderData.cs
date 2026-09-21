public class GiantSpiderData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GIANT_SPIDER;

	public override string name => "Giant Spider";

	public override string description => "Giant Spider";

	public GiantSpiderData()
	{
		base.summonType = SUMMON_TYPE.Giant_Spider;
		base.race = RACE.SPIDER;
		base.className = "Giant Spider";
	}
}
