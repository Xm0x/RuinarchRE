public class BroodmotherData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BROODMOTHER;

	public override string name => "Broodmother";

	public override string description => "Broodmother";

	public BroodmotherData()
	{
		base.summonType = SUMMON_TYPE.Broodmother;
		base.race = RACE.SPIDER;
		base.className = "Broodmother";
	}
}
