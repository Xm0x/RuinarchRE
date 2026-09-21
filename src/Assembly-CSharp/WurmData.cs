public class WurmData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WURM;

	public override string name => "Wurm";

	public override string description => "Wurm";

	public WurmData()
	{
		base.summonType = SUMMON_TYPE.Wurm;
		base.race = RACE.WURM;
		base.className = "Wurm";
	}
}
