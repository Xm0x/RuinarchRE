public class PigData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PIG;

	public override string name => "Pig";

	public override string description => "Pig";

	public PigData()
	{
		base.summonType = SUMMON_TYPE.Pig;
		base.race = RACE.PIG;
		base.className = "Pig";
	}
}
