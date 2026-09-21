public class DireWolfData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DIRE_WOLF;

	public override string name => "Dire Wolf";

	public override string description => "Dire Wolf";

	public DireWolfData()
	{
		base.summonType = SUMMON_TYPE.Dire_Wolf;
		base.race = RACE.WOLF;
		base.className = "Dire";
	}
}
