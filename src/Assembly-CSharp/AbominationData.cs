public class AbominationData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ABOMINATION;

	public override string name => "Abomination";

	public override string description => "Abomination";

	public AbominationData()
	{
		base.summonType = SUMMON_TYPE.Abomination;
		base.race = RACE.ABOMINATION;
		base.className = "Abomination";
	}
}
