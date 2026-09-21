using Inner_Maps.Location_Structures;

public class UpgradeMonstersStatsData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UPGRADE_MONSTERS_STATS;

	public override string name => "Upgrade Monsters";

	public override string description => "Upgrade your Monsters stats.";

	public UpgradeMonstersStatsData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.STRUCTURE };
	}

	public override void ActivateAbility(LocationStructure structure)
	{
		UIManager.Instance.ShowMonstersUpgradeAbilitiesUI();
		base.ActivateAbility(structure);
	}
}
