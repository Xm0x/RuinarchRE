public class DesertEntData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DESERT_ENT;

	public override string name => "Desert Ent";

	public override string description => "Desert Ent";

	public DesertEntData()
	{
		base.summonType = SUMMON_TYPE.Desert_Ent;
		base.race = RACE.ENT;
		base.className = "Desert Ent";
	}
}
