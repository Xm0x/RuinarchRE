public class UnicornData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UNICORN;

	public override string name => "Unicorn";

	public override string description => "Unicorn";

	public UnicornData()
	{
		base.summonType = SUMMON_TYPE.Unicorn;
		base.race = RACE.UNICORN;
		base.className = "Unicorn";
	}
}
