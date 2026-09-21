public class DemonGluttonyData : MinionPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMON_GLUTTONY;

	public override string name => "Gluttony Demon";

	public override string description => "This Lesser Demon is a Magical Combatant that attacks with Electric-based projectiles. It is a good tank.";

	public DemonGluttonyData()
	{
		minionType = MINION_TYPE.Gluttony;
		base.className = "Gluttony";
	}
}
