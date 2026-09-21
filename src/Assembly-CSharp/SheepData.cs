public class SheepData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SHEEP;

	public override string name => "Sheep";

	public override string description => "Sheep";

	public SheepData()
	{
		base.summonType = SUMMON_TYPE.Sheep;
		base.race = RACE.SHEEP;
		base.className = "Sheep";
	}
}
