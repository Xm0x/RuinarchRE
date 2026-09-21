using Inner_Maps.Location_Structures;

public class ReleaseAbilitiesData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RELEASE_ABILITIES;

	public override string name => "Release Powers";

	public override string description => "Gain consumable Bonus Charges for one of three random Powers.";

	public ReleaseAbilitiesData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.STRUCTURE };
	}

	public override void ActivateAbility(LocationStructure structure)
	{
		if (structure is ThePortal)
		{
			UIManager.Instance.ShowPurchaseSkillUI();
			AkSoundEngine.PostEvent("Play_Release_Powers", InnerMapCameraMove.Instance.gameObject);
		}
		base.ActivateAbility(structure);
	}
}
