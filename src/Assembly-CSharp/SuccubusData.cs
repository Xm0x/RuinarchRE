public class SuccubusData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SUCCUBUS;

	public override string name => "Succubus";

	public override string description => "Succubus";

	public SuccubusData()
	{
		base.summonType = SUMMON_TYPE.Succubus;
		base.race = RACE.LESSER_DEMON;
		base.className = "Succubus";
	}
}
