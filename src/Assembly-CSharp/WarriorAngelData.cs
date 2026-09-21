public class WarriorAngelData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WARRIOR_ANGEL;

	public override string name => "Warrior Angel";

	public override string description => "Warrior Angel";

	public WarriorAngelData()
	{
		base.summonType = SUMMON_TYPE.Warrior_Angel;
		base.race = RACE.ANGEL;
		base.className = "Warrior Angel";
	}
}
