public class CorruptEntData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CORRUPT_ENT;

	public override string name => "Corrupt Ent";

	public override string description => "Corrupt Ent";

	public CorruptEntData()
	{
		base.summonType = SUMMON_TYPE.Corrupt_Ent;
		base.race = RACE.ENT;
		base.className = "Corrupt Ent";
	}
}
