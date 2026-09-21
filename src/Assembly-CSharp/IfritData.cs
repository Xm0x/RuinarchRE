public class IfritData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.IFRIT;

	public override string name => "Ifrit";

	public override string description => "Ifrit";

	public IfritData()
	{
		base.summonType = SUMMON_TYPE.Ifrit;
		base.race = RACE.IFRIT;
		base.className = "Ifrit";
	}
}
