public class WyvernData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WYVERN;

	public override string name => "Wyvern";

	public override string description => "Wyvern";

	public WyvernData()
	{
		base.summonType = SUMMON_TYPE.Wyvern;
		base.race = RACE.WYVERN;
		base.className = "Wyvern";
	}
}
