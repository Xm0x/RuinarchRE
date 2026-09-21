public class GrassEntData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GRASS_ENT;

	public override string name => "Grass Ent";

	public override string description => "Grass Ent";

	public GrassEntData()
	{
		base.summonType = SUMMON_TYPE.Grass_Ent;
		base.race = RACE.ENT;
		base.className = "Grass Ent";
	}
}
