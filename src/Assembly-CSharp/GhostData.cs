public class GhostData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GHOST;

	public override string name => "Ghost";

	public override string description => "Ghost";

	public GhostData()
	{
		base.summonType = SUMMON_TYPE.Ghost;
		base.race = RACE.GHOST;
		base.className = "Ghost";
	}
}
