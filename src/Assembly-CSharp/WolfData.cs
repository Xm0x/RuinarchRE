public class WolfData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WOLF;

	public override string name => "Wolf";

	public override string description => "Wolf";

	public WolfData()
	{
		base.summonType = SUMMON_TYPE.Wolf;
		base.race = RACE.WOLF;
		base.className = "Ravager";
	}
}
