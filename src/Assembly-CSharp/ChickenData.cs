public class ChickenData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CHICKEN;

	public override string name => "Chicken";

	public override string description => "Chicken";

	public ChickenData()
	{
		base.summonType = SUMMON_TYPE.Chicken;
		base.race = RACE.CHICKEN;
		base.className = "Chicken";
	}
}
