public class FireWispData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FIRE_WISP;

	public override string name => "Fire Wisp";

	public override string description => "Fire Wisp";

	public FireWispData()
	{
		base.summonType = SUMMON_TYPE.Fire_Wisp;
		base.race = RACE.WISP;
		base.className = "Fire Wisp";
	}
}
