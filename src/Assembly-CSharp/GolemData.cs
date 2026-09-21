public class GolemData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GOLEM;

	public override string name => "Golem";

	public override string description => "Golem";

	public GolemData()
	{
		base.summonType = SUMMON_TYPE.Golem;
		base.race = RACE.GOLEM;
		base.className = "Golem";
	}
}
