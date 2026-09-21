public class MimicData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MIMIC;

	public override string name => "Mimic";

	public override string description => "Mimic";

	public MimicData()
	{
		base.summonType = SUMMON_TYPE.Mimic;
		base.race = RACE.MIMIC;
		base.className = "Mimic";
	}
}
