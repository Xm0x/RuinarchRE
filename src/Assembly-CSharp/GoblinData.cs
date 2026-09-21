public class GoblinData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GOBLIN;

	public override string name => "Goblin";

	public override string description => "Goblin";

	public GoblinData()
	{
		base.summonType = SUMMON_TYPE.Goblin;
		base.race = RACE.GOBLIN;
		base.className = "Goblin";
	}
}
