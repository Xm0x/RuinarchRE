public class RevenantData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.REVENANT;

	public override string name => "Revenant";

	public override string description => "Revenant";

	public RevenantData()
	{
		base.summonType = SUMMON_TYPE.Revenant;
		base.race = RACE.REVENANT;
		base.className = "Revenant";
	}
}
