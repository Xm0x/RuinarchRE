public class KoboldData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.KOBOLD;

	public override string name => "Kobold";

	public override string description => "Kobold";

	public KoboldData()
	{
		base.summonType = SUMMON_TYPE.Kobold;
		base.race = RACE.KOBOLD;
		base.className = "Kobold";
	}
}
