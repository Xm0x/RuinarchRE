public class TrollData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TROLL;

	public override string name => "Troll";

	public override string description => "Troll";

	public TrollData()
	{
		base.summonType = SUMMON_TYPE.Troll;
		base.race = RACE.TROLL;
		base.className = "Troll";
	}
}
