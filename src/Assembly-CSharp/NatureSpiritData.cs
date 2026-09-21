public class NatureSpiritData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.NATURE_SPIRIT;

	public override string name => "Nature Spirit";

	public override string description => "Nature Spirit";

	public NatureSpiritData()
	{
		base.summonType = SUMMON_TYPE.Nature_Spirit;
		base.race = RACE.SPIRIT;
		base.className = "Nature Spirit";
	}
}
