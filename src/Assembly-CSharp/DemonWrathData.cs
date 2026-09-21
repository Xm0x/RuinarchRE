public class DemonWrathData : MinionPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMON_WRATH;

	public override string name => "Wrath Demon";

	public override string description => "This Demon is a powerful melee combatant that deals Normal damage.";

	public DemonWrathData()
	{
		minionType = MINION_TYPE.Wrath;
		base.className = "Wrath";
	}
}
