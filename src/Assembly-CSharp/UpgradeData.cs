using Inner_Maps.Location_Structures;

public class UpgradeData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UPGRADE;

	public override string name => "Upgrade";

	public override string description => "Use the Biolab to upgrade your Plague affliction.";

	public UpgradeData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.STRUCTURE };
	}

	public override void ActivateAbility(LocationStructure structure)
	{
		if (structure is Biolab)
		{
			UIManager.Instance.ShowBiolabUI();
		}
		base.ActivateAbility(structure);
	}
}
