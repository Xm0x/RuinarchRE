public class DemonSlothData : MinionPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEMON_SLOTH;

	public override string name => "Sloth Demon";

	public override string description => "This Demon is a tough melee magic-user that deals Ice damage. It is effective as a tank.";

	public DemonSlothData()
	{
		minionType = MINION_TYPE.Sloth;
		base.className = "Sloth";
	}
}
