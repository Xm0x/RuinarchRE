public class GhoulData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GHOUL;

	public override string name => "Ghoul";

	public override string description => "Ghoul";

	public GhoulData()
	{
		base.summonType = SUMMON_TYPE.Ghoul;
		base.race = RACE.GHOUL;
		base.className = "Ghoul";
	}
}
