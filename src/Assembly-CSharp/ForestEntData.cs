public class ForestEntData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FOREST_ENT;

	public override string name => "Forest Ent";

	public override string description => "Forest Ent";

	public ForestEntData()
	{
		base.summonType = SUMMON_TYPE.Forest_Ent;
		base.race = RACE.ENT;
		base.className = "Forest Ent";
	}
}
