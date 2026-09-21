using Inner_Maps.Location_Structures;

public class UpgradeAbilitiesData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UPGRADE_ABILITIES;

	public override string name => "Upgrade Powers";

	public override string description => "Spend Chaotic Energy to improve your Spells, Afflictions and other Abilities.";

	public UpgradeAbilitiesData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.STRUCTURE };
	}

	public override void ActivateAbility(LocationStructure structure)
	{
		UIManager.Instance.ShowUpgradeAbilitiesUI();
		base.ActivateAbility(structure);
	}
}
