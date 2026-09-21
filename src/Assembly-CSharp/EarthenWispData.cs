public class EarthenWispData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EARTHEN_WISP;

	public override string name => "Earthen Wisp";

	public override string description => "Earthen Wisp";

	public EarthenWispData()
	{
		base.summonType = SUMMON_TYPE.Earthen_Wisp;
		base.race = RACE.WISP;
		base.className = "Earthen Wisp";
	}
}
