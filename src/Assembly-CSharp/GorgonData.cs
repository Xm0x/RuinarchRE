public class GorgonData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GORGON;

	public override string name => "Gorgon";

	public override string description => "Gorgon";

	public GorgonData()
	{
		base.summonType = SUMMON_TYPE.Gorgon;
		base.race = RACE.GORGON;
		base.className = "Gorgon";
	}
}
