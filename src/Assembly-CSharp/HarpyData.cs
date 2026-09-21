public class HarpyData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.HARPY;

	public override string name => "Harpy";

	public override string description => "Harpy";

	public HarpyData()
	{
		base.summonType = SUMMON_TYPE.Harpy;
		base.race = RACE.HARPY;
		base.className = "Harpy";
	}
}
