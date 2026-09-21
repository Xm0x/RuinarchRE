public class DemonGreedData : MinionPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMON_GREED;

	public override string name => "Greed Demon";

	public override string description => "This Lesser Demon is a melee Physical Combatant that deals Wind damage. It deals bonus damage when attacking objects and structures.";

	public DemonGreedData()
	{
		minionType = MINION_TYPE.Greed;
		base.className = "Greed";
	}
}
