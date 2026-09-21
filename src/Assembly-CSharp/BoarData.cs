public class BoarData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BOAR;

	public override string name => "Boar";

	public override string description => "Boar";

	public BoarData()
	{
		base.summonType = SUMMON_TYPE.Boar;
		base.race = RACE.BOAR;
		base.className = "Boar";
	}
}
