public class SmallSpiderData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SMALL_SPIDER;

	public override string name => "Small Spider";

	public override string description => "Small Spider";

	public SmallSpiderData()
	{
		base.summonType = SUMMON_TYPE.Small_Spider;
		base.race = RACE.SPIDER;
		base.className = "Small Spider";
	}
}
