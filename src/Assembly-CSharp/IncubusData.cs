public class IncubusData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.INCUBUS;

	public override string name => "Incubus";

	public override string description => "Incubus";

	public IncubusData()
	{
		base.summonType = SUMMON_TYPE.Incubus;
		base.race = RACE.LESSER_DEMON;
		base.className = "Incubus";
	}
}
