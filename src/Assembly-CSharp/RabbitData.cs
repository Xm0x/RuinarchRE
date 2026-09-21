public class RabbitData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RABBIT;

	public override string name => "Rabbit";

	public override string description => "Rabbit";

	public RabbitData()
	{
		base.summonType = SUMMON_TYPE.Rabbit;
		base.race = RACE.RABBIT;
		base.className = "Rabbit";
	}
}
