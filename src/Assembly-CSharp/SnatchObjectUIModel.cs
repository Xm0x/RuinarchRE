using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SnatchObjectUIModel : MVCUIModel
{
	[Header("General")]
	public Vector2 defaultPos;

	public RectTransform rtWindow;

	public RuinarchButton btnSnatchObject;

	public RuinarchButton btnClose;

	public TextMeshProUGUI lblTitle;

	public TextMeshProUGUI lblSnatchObject;

	public MonsterUnderlingQuantityNameplateItem underlingTooltip;

	public CharacterNameplateItem characterNameplateTooltip;

	public MonsterToolTipUI monsterToolTipUI;

	[Header("Target")]
	public TextMeshProUGUI lblTargetName;

	[Header("Leaders")]
	public SnatchObjectSlotItem leaderSlotItem;

	[Header("Summons")]
	public SnatchObjectSlotItem[] summonSlotItems;

	public RuinarchButton btnAddPartySlot;

	public HoverHandler hoverHandlerBtnAddPartySlot;

	public GameObject goNoSummons;

	[Header("Location")]
	public GameObject goTargetLocation;

	public RuinarchDropdown dropDownTargetLocations;

	public TextMeshProUGUI lblTargetLocation;

	[Header("Object Picker")]
	public GameObject goObjectPicker;

	public ScrollRect scrollRectObjectPicker;

	public GameObject prefabObjectPicker;

	public RuinarchButton btnObjectPickerClose;

	public Action<int> onChooseTargetLocation;

	public Action onClickAddPartySlot;

	public Action onHoverOverAddPartySlot;

	public Action onHoverOutAddPartySlot;

	public Action onClickSnatchObject;

	public Action onClickClose;

	public Action onClickCloseObjectPicker;

	private void OnEnable()
	{
		btnAddPartySlot.onClick.AddListener(OnClickAddPartySlot);
		btnSnatchObject.onClick.AddListener(OnClickSnatchObject);
		btnClose.onClick.AddListener(OnClickClose);
		hoverHandlerBtnAddPartySlot.AddOnHoverOverAction(OnHoverOverAddSummonSlotBtn);
		hoverHandlerBtnAddPartySlot.AddOnHoverOutAction(OnHoverOutAddSummonSlotBtn);
		dropDownTargetLocations.onValueChanged.AddListener(OnChooseTargetLocation);
		btnObjectPickerClose.onClick.AddListener(OnClickCloseObjectPicker);
	}

	private void OnDisable()
	{
		btnAddPartySlot.onClick.RemoveListener(OnClickAddPartySlot);
		btnSnatchObject.onClick.RemoveListener(OnClickSnatchObject);
		btnClose.onClick.RemoveListener(OnClickClose);
		hoverHandlerBtnAddPartySlot.RemoveOnHoverOverAction(OnHoverOverAddSummonSlotBtn);
		hoverHandlerBtnAddPartySlot.RemoveOnHoverOutAction(OnHoverOutAddSummonSlotBtn);
		dropDownTargetLocations.onValueChanged.RemoveListener(OnChooseTargetLocation);
		btnObjectPickerClose.onClick.RemoveListener(OnClickCloseObjectPicker);
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

	private void OnClickSnatchObject()
	{
		onClickSnatchObject?.Invoke();
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
}
