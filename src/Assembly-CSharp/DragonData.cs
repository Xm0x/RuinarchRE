public class DragonData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DRAGON;

	public override string name => "Dragon";

	public override string description => "Dragon";

	public DragonData()
	{
		base.summonType = SUMMON_TYPE.Dragon;
		base.race = RACE.DRAGON;
		base.className = "Dragon";
	}
}
