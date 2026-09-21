public class WyvernlingData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WYVERNLING;

	public override string name => "Wyvernling";

	public override string description => "Wyvernling";

	public WyvernlingData()
	{
		base.summonType = SUMMON_TYPE.Wyvernling;
		base.race = RACE.WYVERN;
		base.className = "Wyvernling";
	}
}
