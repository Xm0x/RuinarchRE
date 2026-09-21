public class WhispererData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WHISPERER;

	public override string name => "Whisperer";

	public override string description => "Whisperer";

	public WhispererData()
	{
		base.summonType = SUMMON_TYPE.Whisperer;
		base.race = RACE.WHISPERER;
		base.className = "Whisperer";
	}
}
