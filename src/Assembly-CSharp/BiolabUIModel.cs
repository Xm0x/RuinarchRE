using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.UI;

public class BiolabUIModel : MVCUIModel
{
	public Action<bool> onTransmissionTabClicked;

	public Action<bool> onLifeSpanTabClicked;

	public Action<bool> onFatalityTabClicked;

	public Action<bool> onSymptomsTabClicked;

	public Action<bool> onOnDeathClicked;

	public Action onCloseClicked;

	public Action<UIHoverPosition> onPlaguedRatsHoveredOver;

	public Action onPlaguedRatsHoveredOut;

	public RuinarchToggle btnTransmissionTab;

	public RuinarchToggle btnLifeSpanTab;

	public RuinarchToggle btnFatalityTab;

	public RuinarchToggle btnSymptomsTab;

	public RuinarchToggle btnOnDeathTab;

	public Button btnClose;

	public RuinarchText txtActiveCasesValue;

	public RuinarchText txtDeathsValue;

	public RuinarchText txtRecoveriesValue;

	public RuinarchText txtPlagueRatsValue;

	public RuinarchText txtPlaguePoints;

	public HoverHandler hoverHandlerPlaguedRats;

	public UIHoverPosition hoverPositionPlaguedRats;

	public Transform tabPrent;

	private void OnEnable()
	{
		btnTransmissionTab.onValueChanged.AddListener(ClickTransmissionTab);
		btnLifeSpanTab.onValueChanged.AddListener(ClickLifeSpanTab);
		btnFatalityTab.onValueChanged.AddListener(ClickFatalityTab);
		btnSymptomsTab.onValueChanged.AddListener(ClickSymptomsTab);
		btnOnDeathTab.onValueChanged.AddListener(ClickOnDeathTab);
		btnClose.onClick.AddListener(ClickClose);
		hoverHandlerPlaguedRats.AddOnHoverOverAction(OnHoverOverPlaguedRats);
		hoverHandlerPlaguedRats.AddOnHoverOutAction(OnHoverOutPlaguedRats);
	}

	private void OnDisable()
	{
		btnTransmissionTab.onValueChanged.RemoveListener(ClickTransmissionTab);
		btnLifeSpanTab.onValueChanged.RemoveListener(ClickLifeSpanTab);
		btnFatalityTab.onValueChanged.RemoveListener(ClickFatalityTab);
		btnSymptomsTab.onValueChanged.RemoveListener(ClickSymptomsTab);
		btnOnDeathTab.onValueChanged.RemoveListener(ClickOnDeathTab);
		btnClose.onClick.RemoveListener(ClickClose);
		hoverHandlerPlaguedRats.RemoveOnHoverOverAction(OnHoverOverPlaguedRats);
		hoverHandlerPlaguedRats.RemoveOnHoverOutAction(OnHoverOutPlaguedRats);
	}

	private void ClickTransmissionTab(bool isOn)
	{
		onTransmissionTabClicked?.Invoke(isOn);
	}

	private void ClickLifeSpanTab(bool isOn)
	{
		onLifeSpanTabClicked?.Invoke(isOn);
	}

	private void ClickFatalityTab(bool isOn)
	{
		onFatalityTabClicked?.Invoke(isOn);
	}

	private void ClickSymptomsTab(bool isOn)
	{
		onSymptomsTabClicked?.Invoke(isOn);
	}

	private void ClickOnDeathTab(bool isOn)
	{
		onOnDeathClicked?.Invoke(isOn);
	}

	private void ClickClose()
	{
		onCloseClicked?.Invoke();
	}

	private void OnHoverOverPlaguedRats()
	{
		onPlaguedRatsHoveredOver?.Invoke(hoverPositionPlaguedRats);
	}

	private void OnHoverOutPlaguedRats()
	{
		onPlaguedRatsHoveredOut?.Invoke();
	}
}
