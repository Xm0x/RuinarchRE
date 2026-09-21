public class IceNymphData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ICE_NYMPH;

	public override string name => "Ice Nymph";

	public override string description => "Ice Nymph";

	public IceNymphData()
	{
		base.summonType = SUMMON_TYPE.Ice_Nymph;
		base.race = RACE.NYMPH;
		base.className = "Ice Nymph";
	}
}
