using Inner_Maps;

public class BuildPlayerSkill : SkillData
{
	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.BUILD;

	public override string description => name;

	public override string localizedName => LocalizationManager.Instance.GetLocalizedValue("BuildPlayerSkill_Table", name);

	public override string localizedDescription => LocalizationManager.Instance.GetLocalizedValue("BuildPlayerSkill_Table", name + "_Description") ?? "";

	public BuildPlayerSkill()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.BASE_BUILDING };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		base.ActivateAbility(targetTile);
	}

	public override void OnSetAsCurrentActiveSpell()
	{
		base.OnSetAsCurrentActiveSpell();
		BaseBuildingManager.Instance.SetHasClickedLeft(p_state: false);
	}

	public override void OnNoLongerCurrentActiveSpell()
	{
		base.OnNoLongerCurrentActiveSpell();
		BaseBuildingManager.Instance.ResetOnMouseRelease();
		BaseBuildingManager.Instance.SetHasClickedLeft(p_state: false);
	}
}
