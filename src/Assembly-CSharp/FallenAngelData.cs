public class FallenAngelData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FALLEN_ANGEL;

	public override string name => "Fallen Angel";

	public override string description => "Fallen Angel";

	public FallenAngelData()
	{
		base.summonType = SUMMON_TYPE.Fallen_Angel;
		base.race = RACE.LESSER_DEMON;
		base.className = "Fallen Angel";
	}
}
