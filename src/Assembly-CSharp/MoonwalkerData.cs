public class MoonwalkerData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MOONWALKER;

	public override string name => "Moonwalker";

	public override string description => "Moonwalker";

	public MoonwalkerData()
	{
		base.summonType = SUMMON_TYPE.Moonwalker;
		base.race = RACE.MOONWALKER;
		base.className = "Moonwalker";
	}
}
