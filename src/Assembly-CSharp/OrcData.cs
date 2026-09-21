public class OrcData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ORC;

	public override string name => "Orc";

	public override string description => "Orc";

	public OrcData()
	{
		base.summonType = SUMMON_TYPE.Orc;
		base.race = RACE.ORC;
		base.className = "Orc";
	}
}
