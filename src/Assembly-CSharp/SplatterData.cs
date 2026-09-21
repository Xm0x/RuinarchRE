public class SplatterData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPLATTER;

	public override string name => "Splatter";

	public override string description => "Splatter";

	public SplatterData()
	{
		base.summonType = SUMMON_TYPE.Splatter;
		base.race = RACE.SPLATTER;
		base.className = "Splatter";
	}
}
