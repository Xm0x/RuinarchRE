public class CentaurData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CENTAUR;

	public override string name => "Centaur";

	public override string description => "Centaur";

	public CentaurData()
	{
		base.summonType = SUMMON_TYPE.Centaur;
		base.race = RACE.CENTAUR;
		base.className = "Centaur";
	}
}
