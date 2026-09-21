public class MinkData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MINK;

	public override string name => "Mink";

	public override string description => "Mink";

	public MinkData()
	{
		base.summonType = SUMMON_TYPE.Mink;
		base.race = RACE.MINK;
		base.className = "Mink";
	}
}
