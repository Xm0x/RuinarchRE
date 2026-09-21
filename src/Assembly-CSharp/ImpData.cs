public class ImpData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.IMP;

	public override string name => "Imp";

	public override string description => "Imp";

	public ImpData()
	{
		base.summonType = SUMMON_TYPE.Imp;
		base.race = RACE.IMP;
		base.className = "Imp";
	}
}
