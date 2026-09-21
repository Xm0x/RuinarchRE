public class MothmanData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MOTHMAN;

	public override string name => "Mothman";

	public override string description => "Mothman";

	public MothmanData()
	{
		base.summonType = SUMMON_TYPE.Mothman;
		base.race = RACE.MOTHMAN;
		base.className = "Mothman";
	}
}
