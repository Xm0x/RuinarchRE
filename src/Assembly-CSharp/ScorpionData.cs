public class ScorpionData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SCORPION;

	public override string name => "Scorpion";

	public override string description => "Scorpion";

	public ScorpionData()
	{
		base.summonType = SUMMON_TYPE.Scorpion;
		base.race = RACE.SCORPION;
		base.className = "Scorpion";
	}
}
