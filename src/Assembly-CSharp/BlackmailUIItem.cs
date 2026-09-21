using System;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;

public class BlackmailUIItem : MonoBehaviour
{
	public static Action<IIntel, bool> onChooseBlackmail;

	public static Action<IIntel, UIHoverPosition> onHoverOverBlackmail;

	public static Action<IIntel> onHoverOutBlackmail;

	public TextMeshProUGUI txtLog;

	public RuinarchToggle tglChoose;

	public GameObject cover;

	public HoverHandler blackmailHoverHandler;

	public UIHoverPosition hoverPosition;

	private IIntel _intel;

	private void OnEnable()
	{
		tglChoose.onValueChanged.AddListener(OnToggleIntel);
		blackmailHoverHandler.AddOnHoverOverAction(OnHoverOverBlackmail);
		blackmailHoverHandler.AddOnHoverOutAction(OnHoverOutBlackmail);
	}

	private void OnDisable()
	{
		tglChoose.onValueChanged.RemoveListener(OnToggleIntel);
		blackmailHoverHandler.RemoveOnHoverOverAction(OnHoverOverBlackmail);
		blackmailHoverHandler.RemoveOnHoverOutAction(OnHoverOutBlackmail);
	}

	public void SetInitialItemDetails(IIntel p_intel, bool p_interactable)
	{
		_intel = p_intel;
		txtLog.text = p_intel.log.logText;
		tglChoose.interactable = p_interactable;
		cover.SetActive(!p_interactable);
		tglChoose.SetIsOnWithoutNotify(value: false);
	}

	private void OnToggleIntel(bool p_isOn)
	{
		onChooseBlackmail?.Invoke(_intel, p_isOn);
	}

	private void OnHoverOverBlackmail()
	{
		onHoverOverBlackmail?.Invoke(_intel, hoverPosition);
	}

	private void OnHoverOutBlackmail()
	{
		onHoverOutBlackmail?.Invoke(_intel);
	}
}
