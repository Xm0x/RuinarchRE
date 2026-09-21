using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.UI;

public class PrimordialPoolUIModel : MVCUIModel
{
	public Action<bool> onHumanoidTabClicked;

	public Action<bool> onDemonicTabClicked;

	public Action<bool> onUndeadTabClicked;

	public Action<bool> onBeastTabClicked;

	public Action onCloseClicked;

	public Action<UIHoverPosition> onPlaguedRatsHoveredOver;

	public Action onPlaguedRatsHoveredOut;

	public RuinarchToggle btnHumanoidTab;

	public RuinarchToggle btnDemonicTab;

	public RuinarchToggle btnUndeadTab;

	public RuinarchToggle btnBeastTab;

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
		btnHumanoidTab.onValueChanged.AddListener(ClickHumanoidTab);
		btnDemonicTab.onValueChanged.AddListener(ClickDemonicTab);
		btnUndeadTab.onValueChanged.AddListener(ClickUndeadTab);
		btnBeastTab.onValueChanged.AddListener(ClickBeastTab);
		btnClose.onClick.AddListener(ClickClose);
	}

	private void OnDisable()
	{
		btnHumanoidTab.onValueChanged.RemoveListener(ClickHumanoidTab);
		btnDemonicTab.onValueChanged.RemoveListener(ClickDemonicTab);
		btnUndeadTab.onValueChanged.RemoveListener(ClickUndeadTab);
		btnBeastTab.onValueChanged.RemoveListener(ClickBeastTab);
		btnClose.onClick.RemoveListener(ClickClose);
	}

	private void ClickHumanoidTab(bool isOn)
	{
		onHumanoidTabClicked?.Invoke(isOn);
	}

	private void ClickDemonicTab(bool isOn)
	{
		onDemonicTabClicked?.Invoke(isOn);
	}

	private void ClickUndeadTab(bool isOn)
	{
		onUndeadTabClicked?.Invoke(isOn);
	}

	private void ClickBeastTab(bool isOn)
	{
		onBeastTabClicked?.Invoke(isOn);
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
