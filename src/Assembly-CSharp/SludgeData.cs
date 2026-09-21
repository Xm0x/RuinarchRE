public class SludgeData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SLUDGE;

	public override string name => "Sludge";

	public override string description => "Sludge";

	public SludgeData()
	{
		base.summonType = SUMMON_TYPE.Sludge;
		base.race = RACE.SLUDGE;
		base.className = "Sludge";
	}
}
