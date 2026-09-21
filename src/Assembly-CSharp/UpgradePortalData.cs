using Inner_Maps.Location_Structures;

public class UpgradePortalData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UPGRADE_PORTAL;

	public override string name => "Upgrade";

	public override string description => "Upgrade the Portal to permanently unlock new Powers.";

	public override string localizedName => LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", "Upgrade Portal");

	public override string localizedDescription => LocalizationManager.Instance.GetLocalizedValue("PlayerActions_Table", "Upgrade Portal_Description") ?? "";

	public UpgradePortalData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.STRUCTURE };
	}

	public override void ActivateAbility(LocationStructure structure)
	{
		if (structure is ThePortal portal)
		{
			UIManager.Instance.ShowUpgradePortalUI(portal);
			AkSoundEngine.PostEvent("Play_Release_Powers", InnerMapCameraMove.Instance.gameObject);
		}
		base.ActivateAbility(structure);
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		bool flag = base.IsValid(target);
		if (flag)
		{
			if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled)
			{
				return false;
			}
			if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Eradication)
			{
				return false;
			}
			return WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom;
		}
		return flag;
	}
}
