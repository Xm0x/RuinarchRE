public class VengefulGhostData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.VENGEFUL_GHOST;

	public override string name => "Vengeful Ghost";

	public override string description => "Vengeful Ghost";

	public VengefulGhostData()
	{
		base.summonType = SUMMON_TYPE.Vengeful_Ghost;
		base.race = RACE.GHOST;
		base.className = "Vengeful Ghost";
	}
}
