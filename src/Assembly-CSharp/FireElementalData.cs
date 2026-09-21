public class FireElementalData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FIRE_ELEMENTAL;

	public override string name => "Fire Elemental";

	public override string description => "Fire Elemental";

	public FireElementalData()
	{
		base.summonType = SUMMON_TYPE.Fire_Elemental;
		base.race = RACE.ELEMENTAL;
		base.className = "Fire Elemental";
	}
}
