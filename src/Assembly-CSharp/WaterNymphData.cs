public class WaterNymphData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WATER_NYMPH;

	public override string name => "Water Nymph";

	public override string description => "Water Nymph";

	public WaterNymphData()
	{
		base.summonType = SUMMON_TYPE.Water_Nymph;
		base.race = RACE.NYMPH;
		base.className = "Water Nymph";
	}
}
