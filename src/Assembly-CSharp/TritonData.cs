public class TritonData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TRITON;

	public override string name => "Triton";

	public override string description => "Triton";

	public TritonData()
	{
		base.summonType = SUMMON_TYPE.Triton;
		base.race = RACE.TRITON;
		base.className = "Triton";
	}
}
