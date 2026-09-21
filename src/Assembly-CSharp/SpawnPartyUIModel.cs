using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SpawnPartyUIModel : MVCUIModel
{
	[Header("General")]
	public Vector2 defaultPos;

	public RectTransform rtWindow;

	public RuinarchButton btnSpawnParty;

	public RuinarchButton btnClose;

	public TextMeshProUGUI lblTitle;

	public TextMeshProUGUI lblSpawnParty;

	public MonsterUnderlingQuantityNameplateItem underlingTooltip;

	public MonsterToolTipUI monsterToolTipUI;

	public CharacterNameplateItem characterNameplateTooltip;

	public UIHoverPosition tooltipPos;

	[Header("Target")]
	public RuinarchDropdown dropDownTargets;

	public TextMeshProUGUI lblTargetName;

	public TextMeshProUGUI lblSelectTargetTitle;

	[Header("Behaviour")]
	public RuinarchDropdown dropDownBehaviours;

	public HoverHandler hoveHandlerBehaviourDropDown;

	[Header("Leaders")]
	[FormerlySerializedAs("leaderSlotItems")]
	public SpawnPartySlotItem leaderSlotItem;

	[Header("Demonic Summons")]
	public GameObject goDemonicSummonsAndLeader;

	[FormerlySerializedAs("partySlotItems")]
	public SpawnPartySlotItem[] summonSlotItems;

	public RuinarchButton btnAddPartySlot;

	public HoverHandler hoverHandlerBtnAddPartySlot;

	[Header("Location")]
	public GameObject goTargetLocation;

	public RuinarchDropdown dropDownTargetLocations;

	public TextMeshProUGUI lblTargetLocation;

	[Header("Object Picker")]
	public GameObject goObjectPicker;

	public ScrollRect scrollRectObjectPicker;

	public GameObject prefabObjectPicker;

	public RuinarchButton btnObjectPickerClose;

	[Header("Attacker Residents")]
	public GameObject goStructureResidents;

	public RectTransform structureResidentsParent;

	public Action<int> onChooseTarget;

	public Action<Transform> onHoverOverTargetInDropdown;

	public Action<Transform> onHoverOutTargetInDropdown;

	public Action<int> onChooseTargetLocation;

	public Action onClickAddPartySlot;

	public Action onHoverOverAddPartySlot;

	public Action onHoverOutAddPartySlot;

	public Action onClickSpawnParty;

	public Action onClickClose;

	public Action onClickCloseObjectPicker;

	public Action<int> onChooseBehaviour;

	public Action<int> onHoverOverBehaviourDropdown;

	public Action<int> onHoverOutBehaviourDropdown;

	public Action<Transform> onHoverOverBehaviourDropdownItem;

	public Action<Transform> onHoverOutBehaviourDropdownItem;

	private void OnEnable()
	{
		dropDownTargets.onValueChanged.AddListener(OnChooseTarget);
		btnAddPartySlot.onClick.AddListener(OnClickAddPartySlot);
		btnSpawnParty.onClick.AddListener(OnClickSpawnParty);
		btnClose.onClick.AddListener(OnClickClose);
		hoverHandlerBtnAddPartySlot.AddOnHoverOverAction(OnHoverOverAddSummonSlotBtn);
		hoverHandlerBtnAddPartySlot.AddOnHoverOutAction(OnHoverOutAddSummonSlotBtn);
		dropDownTargetLocations.onValueChanged.AddListener(OnChooseTargetLocation);
		btnObjectPickerClose.onClick.AddListener(OnClickCloseObjectPicker);
		dropDownBehaviours.onValueChanged.AddListener(OnChooseBehaviour);
		hoveHandlerBehaviourDropDown.AddOnHoverOverAction(OnHoverOverBehaviourDropdown);
		hoveHandlerBehaviourDropDown.AddOnHoverOutAction(OnHoverOutBehaviourDropdown);
	}

	private void OnDisable()
	{
		dropDownTargets.onValueChanged.RemoveListener(OnChooseTarget);
		btnAddPartySlot.onClick.RemoveListener(OnClickAddPartySlot);
		btnSpawnParty.onClick.RemoveListener(OnClickSpawnParty);
		btnClose.onClick.RemoveListener(OnClickClose);
		hoverHandlerBtnAddPartySlot.RemoveOnHoverOverAction(OnHoverOverAddSummonSlotBtn);
		hoverHandlerBtnAddPartySlot.RemoveOnHoverOutAction(OnHoverOutAddSummonSlotBtn);
		dropDownTargetLocations.onValueChanged.RemoveListener(OnChooseTargetLocation);
		btnObjectPickerClose.onClick.RemoveListener(OnClickCloseObjectPicker);
		hoveHandlerBehaviourDropDown.RemoveOnHoverOverAction(OnHoverOverBehaviourDropdown);
		hoveHandlerBehaviourDropDown.RemoveOnHoverOutAction(OnHoverOutBehaviourDropdown);
	}

	private void OnChooseTarget(int p_value)
	{
		onChooseTarget?.Invoke(p_value);
	}

	public void OnHoverOverTargetInDropdown(Transform p_transform)
	{
		onHoverOverTargetInDropdown?.Invoke(p_transform);
	}

	public void OnHoverOutTargetInDropdown(Transform p_transform)
	{
		onHoverOutTargetInDropdown?.Invoke(p_transform);
	}

	private void OnClickAddPartySlot()
	{
		onClickAddPartySlot?.Invoke();
	}

	private void OnHoverOverAddSummonSlotBtn()
	{
		onHoverOverAddPartySlot?.Invoke();
	}

	private void OnHoverOutAddSummonSlotBtn()
	{
		onHoverOutAddPartySlot?.Invoke();
	}

	private void OnClickSpawnParty()
	{
		onClickSpawnParty?.Invoke();
	}

	private void OnClickClose()
	{
		onClickClose?.Invoke();
	}

	private void OnChooseTargetLocation(int p_value)
	{
		onChooseTargetLocation?.Invoke(p_value);
	}

	private void OnClickCloseObjectPicker()
	{
		onClickCloseObjectPicker?.Invoke();
	}

	private void OnChooseBehaviour(int p_value)
	{
		onChooseBehaviour?.Invoke(p_value);
	}

	private void OnHoverOverBehaviourDropdown()
	{
		onHoverOverBehaviourDropdown?.Invoke(dropDownBehaviours.value);
	}

	private void OnHoverOutBehaviourDropdown()
	{
		onHoverOutBehaviourDropdown?.Invoke(dropDownBehaviours.value);
	}

	public void OnHoverOverBehaviourDropdownItem(Transform p_transform)
	{
		onHoverOverBehaviourDropdownItem?.Invoke(p_transform);
	}

	public void OnHoverOutBehaviourDropdownItem(Transform p_transform)
	{
		onHoverOutBehaviourDropdownItem?.Invoke(p_transform);
	}
}
