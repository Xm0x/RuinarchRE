public class MagicalAngelData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MAGICAL_ANGEL;

	public override string name => "Magical Angel";

	public override string description => "Magical Angel";

	public MagicalAngelData()
	{
		base.summonType = SUMMON_TYPE.Magical_Angel;
		base.race = RACE.ANGEL;
		base.className = "Magical Angel";
	}
}
