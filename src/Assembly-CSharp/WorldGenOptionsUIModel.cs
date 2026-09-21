using System;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldGenOptionsUIModel : MVCUIModel
{
	public Action<MAP_SIZE> onChangeMapSize;

	public Action<MIGRATION_SPEED> onChangeMigrationSpeed;

	public Action<VICTORY_CONDITION> onChangeVictoryCondition;

	public Action<SKILL_COOLDOWN_SPEED> onChangeSkillCooldownSpeed;

	public Action<SKILL_COST_AMOUNT> onChangeSkillCostAmount;

	public Action<SKILL_CHARGE_AMOUNT> onChangeSkillChargeAmount;

	public Action<RETALIATION> onChangeThreatAmount;

	public Action<OMNIPOTENT_MODE> onChangeOmnipotent;

	public Action<CORRUPTION_CHARGE_AMOUNT> onChangeCorruptionCharges;

	public Action onClickAddFaction;

	[Header("Map Size")]
	public GameObject goMapSize;

	public TMP_Dropdown dropDownMapSize;

	public HoverHandler hoverHandlerMapSize;

	public Action<UIHoverPosition> onHoverOverMapSizeDropdown;

	public Action onHoverOutMapSizeDropdown;

	[Header("Migration")]
	public GameObject goMigration;

	public TMP_Dropdown dropDownMigration;

	public HoverHandler hoverHandlerMigration;

	public Action<UIHoverPosition> onHoverOverMigrationDropdown;

	public Action onHoverOutMigrationDropdown;

	[Header("Victory Condition")]
	public TMP_Dropdown dropDownVictory;

	public HoverHandler hoverHandlerVictory;

	public TextMeshProUGUI txtVictoryConditionDescription;

	public Action<UIHoverPosition> onHoverOverVictoryDropdown;

	public Action onHoverOutVictoryDropdown;

	[Header("Cooldown")]
	public GameObject goCooldown;

	public TMP_Dropdown dropDownCooldown;

	public HoverHandler hoverHandlerCooldown;

	public Action<UIHoverPosition> onHoverOverCooldownDropdown;

	public Action onHoverOutCooldownDropdown;

	[Header("Costs")]
	public GameObject goCosts;

	public TMP_Dropdown dropDownCosts;

	public HoverHandler hoverHandlerCosts;

	public Action<UIHoverPosition> onHoverOverCostsDropdown;

	public Action onHoverOutCostsDropdown;

	[Header("Charges")]
	public GameObject goCharges;

	public TMP_Dropdown dropDownCharges;

	public HoverHandler hoverHandlerCharges;

	public Action<UIHoverPosition> onHoverOverChargesDropdown;

	public Action onHoverOutChargesDropdown;

	[Header("Threat")]
	public GameObject goThreat;

	public TMP_Dropdown dropDownThreat;

	public HoverHandler hoverHandlerThreat;

	public Action<UIHoverPosition> onHoverOverThreatDropdown;

	public Action onHoverOutThreatDropdown;

	[Header("Factions")]
	public FactionSettingUIItem[] factionSettingUIItems;

	public Button btnAddFaction;

	public TextMeshProUGUI txtVillages;

	public TextMeshProUGUI txtFactionsCount;

	[Header("Omnipotent")]
	public GameObject goOmnipotent;

	public TMP_Dropdown dropDownOmnipotent;

	public HoverHandler hoverHandlerOmnipotent;

	public Action<UIHoverPosition> onHoverOverOmnipotentDropdown;

	public Action onHoverOutOmnipotentDropdown;

	[Header("Corruption Charges")]
	public GameObject goCorruptionCharges;

	public TMP_Dropdown dropDownCorruptionCharges;

	public HoverHandler hoverHandlerCorruptionCharges;

	public Action<UIHoverPosition> onHoverOverCorruptionChargesDropdown;

	public Action onHoverOutCorruptionChargesDropdown;

	public UIHoverPosition tooltipPosition;

	private void OnEnable()
	{
		dropDownMapSize.onValueChanged.AddListener(OnChangeMapSize);
		dropDownMigration.onValueChanged.AddListener(OnChangeMigrationSpeed);
		dropDownVictory.onValueChanged.AddListener(OnChangeVictoryCondition);
		dropDownCooldown.onValueChanged.AddListener(OnChangeCooldownSpeed);
		dropDownCosts.onValueChanged.AddListener(OnChangeSkillCost);
		dropDownCharges.onValueChanged.AddListener(OnChangeChargesAmount);
		dropDownThreat.onValueChanged.AddListener(OnChangeThreatAmount);
		dropDownOmnipotent.onValueChanged.AddListener(OnChangeOmnipotent);
		dropDownCorruptionCharges.onValueChanged.AddListener(OnChangeCorruptionCharges);
		btnAddFaction.onClick.AddListener(OnClickAddFaction);
		hoverHandlerMapSize.AddOnHoverOverAction(OnHoverOverMapSize);
		hoverHandlerMapSize.AddOnHoverOutAction(OnHoverOutMapSize);
		hoverHandlerMigration.AddOnHoverOverAction(OnHoverOverMigration);
		hoverHandlerMigration.AddOnHoverOutAction(OnHoverOutMigration);
		hoverHandlerVictory.AddOnHoverOverAction(OnHoverOverVictory);
		hoverHandlerVictory.AddOnHoverOutAction(OnHoverOutVictory);
		hoverHandlerCooldown.AddOnHoverOverAction(OnHoverOverCooldown);
		hoverHandlerCooldown.AddOnHoverOutAction(OnHoverOutCooldown);
		hoverHandlerCosts.AddOnHoverOverAction(OnHoverOverCosts);
		hoverHandlerCosts.AddOnHoverOutAction(OnHoverOutCosts);
		hoverHandlerCharges.AddOnHoverOverAction(OnHoverOverCharges);
		hoverHandlerCharges.AddOnHoverOutAction(OnHoverOutCharges);
		hoverHandlerThreat.AddOnHoverOverAction(OnHoverOverThreat);
		hoverHandlerThreat.AddOnHoverOutAction(OnHoverOutThreat);
		hoverHandlerOmnipotent.AddOnHoverOverAction(OnHoverOverOmnipotent);
		hoverHandlerOmnipotent.AddOnHoverOutAction(OnHoverOutOmnipotent);
		hoverHandlerCorruptionCharges.AddOnHoverOverAction(OnHoverOverCorruptionCharges);
		hoverHandlerCorruptionCharges.AddOnHoverOutAction(OnHoverOutCorruptionCharges);
	}

	private void OnDisable()
	{
		dropDownMapSize.onValueChanged.RemoveListener(OnChangeMapSize);
		dropDownMigration.onValueChanged.RemoveListener(OnChangeMigrationSpeed);
		dropDownVictory.onValueChanged.RemoveListener(OnChangeVictoryCondition);
		dropDownCooldown.onValueChanged.RemoveListener(OnChangeCooldownSpeed);
		dropDownCosts.onValueChanged.RemoveListener(OnChangeSkillCost);
		dropDownCharges.onValueChanged.RemoveListener(OnChangeChargesAmount);
		dropDownThreat.onValueChanged.RemoveListener(OnChangeThreatAmount);
		dropDownOmnipotent.onValueChanged.RemoveListener(OnChangeOmnipotent);
		dropDownCorruptionCharges.onValueChanged.RemoveListener(OnChangeCorruptionCharges);
		btnAddFaction.onClick.RemoveListener(OnClickAddFaction);
		hoverHandlerMapSize.RemoveOnHoverOverAction(OnHoverOverMapSize);
		hoverHandlerMapSize.RemoveOnHoverOutAction(OnHoverOutMapSize);
		hoverHandlerMigration.RemoveOnHoverOverAction(OnHoverOverMigration);
		hoverHandlerMigration.RemoveOnHoverOutAction(OnHoverOutMigration);
		hoverHandlerVictory.RemoveOnHoverOverAction(OnHoverOverVictory);
		hoverHandlerVictory.RemoveOnHoverOutAction(OnHoverOutVictory);
		hoverHandlerCooldown.RemoveOnHoverOverAction(OnHoverOverCooldown);
		hoverHandlerCooldown.RemoveOnHoverOutAction(OnHoverOutCooldown);
		hoverHandlerCosts.RemoveOnHoverOverAction(OnHoverOverCosts);
		hoverHandlerCosts.RemoveOnHoverOutAction(OnHoverOutCosts);
		hoverHandlerCharges.RemoveOnHoverOverAction(OnHoverOverCharges);
		hoverHandlerCharges.RemoveOnHoverOutAction(OnHoverOutCharges);
		hoverHandlerThreat.RemoveOnHoverOverAction(OnHoverOverThreat);
		hoverHandlerThreat.RemoveOnHoverOutAction(OnHoverOutThreat);
		hoverHandlerOmnipotent.RemoveOnHoverOverAction(OnHoverOverOmnipotent);
		hoverHandlerOmnipotent.RemoveOnHoverOutAction(OnHoverOutOmnipotent);
	}

	private void OnChangeMapSize(int p_index)
	{
		MAP_SIZE value = (MAP_SIZE)dropDownMapSize.value;
		onChangeMapSize?.Invoke(value);
	}

	private void OnChangeMigrationSpeed(int p_index)
	{
		MIGRATION_SPEED value = (MIGRATION_SPEED)dropDownMigration.value;
		onChangeMigrationSpeed?.Invoke(value);
	}

	private void OnChangeVictoryCondition(int p_index)
	{
		VICTORY_CONDITION value = (VICTORY_CONDITION)dropDownVictory.value;
		onChangeVictoryCondition?.Invoke(value);
	}

	private void OnChangeCooldownSpeed(int p_index)
	{
		SKILL_COOLDOWN_SPEED value = (SKILL_COOLDOWN_SPEED)dropDownCooldown.value;
		onChangeSkillCooldownSpeed?.Invoke(value);
	}

	private void OnChangeSkillCost(int p_index)
	{
		SKILL_COST_AMOUNT value = (SKILL_COST_AMOUNT)dropDownCosts.value;
		onChangeSkillCostAmount?.Invoke(value);
	}

	private void OnChangeChargesAmount(int p_index)
	{
		SKILL_CHARGE_AMOUNT value = (SKILL_CHARGE_AMOUNT)dropDownCharges.value;
		onChangeSkillChargeAmount?.Invoke(value);
	}

	private void OnChangeThreatAmount(int p_index)
	{
		RETALIATION value = (RETALIATION)dropDownThreat.value;
		onChangeThreatAmount?.Invoke(value);
	}

	private void OnChangeOmnipotent(int p_index)
	{
		OMNIPOTENT_MODE value = (OMNIPOTENT_MODE)dropDownOmnipotent.value;
		onChangeOmnipotent?.Invoke(value);
	}

	private void OnChangeCorruptionCharges(int p_index)
	{
		CORRUPTION_CHARGE_AMOUNT value = (CORRUPTION_CHARGE_AMOUNT)dropDownCorruptionCharges.value;
		onChangeCorruptionCharges?.Invoke(value);
	}

	private void OnClickAddFaction()
	{
		onClickAddFaction?.Invoke();
	}

	private void OnUpdatePortalLevelTextField(string p_value)
	{
		int.TryParse(p_value, out var _);
	}

	private void OnSliderPortalLevelUpdated(float p_value)
	{
		Mathf.FloorToInt(p_value);
	}

	private void OnHoverOverMapSize()
	{
		onHoverOverMapSizeDropdown?.Invoke(tooltipPosition);
	}

	private void OnHoverOutMapSize()
	{
		onHoverOutMapSizeDropdown?.Invoke();
	}

	private void OnHoverOverMigration()
	{
		onHoverOverMigrationDropdown?.Invoke(tooltipPosition);
	}

	private void OnHoverOutMigration()
	{
		onHoverOutMigrationDropdown?.Invoke();
	}

	private void OnHoverOverVictory()
	{
		onHoverOverVictoryDropdown?.Invoke(tooltipPosition);
	}

	private void OnHoverOutVictory()
	{
		onHoverOutVictoryDropdown?.Invoke();
	}

	private void OnHoverOverCooldown()
	{
		onHoverOverCooldownDropdown?.Invoke(tooltipPosition);
	}

	private void OnHoverOutCooldown()
	{
		onHoverOutCooldownDropdown?.Invoke();
	}

	private void OnHoverOverCosts()
	{
		onHoverOverCostsDropdown?.Invoke(tooltipPosition);
	}

	private void OnHoverOutCosts()
	{
		onHoverOutCostsDropdown?.Invoke();
	}

	private void OnHoverOverCharges()
	{
		onHoverOverChargesDropdown?.Invoke(tooltipPosition);
	}

	private void OnHoverOutCharges()
	{
		onHoverOutChargesDropdown?.Invoke();
	}

	private void OnHoverOverThreat()
	{
		onHoverOverThreatDropdown?.Invoke(tooltipPosition);
	}

	private void OnHoverOutThreat()
	{
		onHoverOutThreatDropdown?.Invoke();
	}

	private void OnHoverOverOmnipotent()
	{
		onHoverOverOmnipotentDropdown?.Invoke(tooltipPosition);
	}

	private void OnHoverOutOmnipotent()
	{
		onHoverOutOmnipotentDropdown?.Invoke();
	}

	private void OnHoverOverCorruptionCharges()
	{
		onHoverOverCorruptionChargesDropdown?.Invoke(tooltipPosition);
	}

	private void OnHoverOutCorruptionCharges()
	{
		onHoverOutCorruptionChargesDropdown?.Invoke();
	}
}
