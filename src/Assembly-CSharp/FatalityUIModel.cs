using System;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FatalityUIModel : MVCUIModel
{
	public Action onSepticShockUpgradeClicked;

	public Action onHeartAttackUpgradeClicked;

	public Action onStrokeUpgradeClicked;

	public Action onTotalOrganFailureUpgradeClicked;

	public Action onPneumoniaUpgradeClicked;

	public Action<UIHoverPosition> onSepticShockHoveredOver;

	public Action<UIHoverPosition> onHeartAttackHoveredOver;

	public Action<UIHoverPosition> onStrokeHoveredOver;

	public Action<UIHoverPosition> onTotalOrganFailureHoveredOver;

	public Action<UIHoverPosition> onPneumoniaHoveredOver;

	public Action onSepticShockHoveredOut;

	public Action onHeartAttackHoveredOut;

	public Action onStrokeHoveredOut;

	public Action onTotalOrganFailureHoveredOut;

	public Action onPneumoniaHoveredOut;

	public Button btnSepticShockUpgrade;

	public Button btnHeartAttackUpgrade;

	public Button btnStrokeUpgrade;

	public Button btnTotalOrganFailureUpgrade;

	public Button btnPneumoniaUpgrade;

	public TextMeshProUGUI txtSepticShockUpgrade;

	public TextMeshProUGUI txtHeartAttackUpgrade;

	public TextMeshProUGUI txtStrokeUpgrade;

	public TextMeshProUGUI txtTotalOrganFailureUpgrade;

	public TextMeshProUGUI txtPneumoniaUpgrade;

	public RuinarchText txtSepticShockCost;

	public RuinarchText txtHeartAttackCost;

	public RuinarchText txtStrokeCost;

	public RuinarchText txtTotalOrganFailureCost;

	public RuinarchText txtPneumoniaCost;

	public HoverHandler septicShockHoverHandler;

	public HoverHandler heartAttackHoverHandler;

	public HoverHandler strokeHoverHandler;

	public HoverHandler totalOrganFailureHoverHandler;

	public HoverHandler pneumoniaHoverHandler;

	public GameObject checkMarkSepticShockUpgrade;

	public GameObject checkMarkHeartAttackUpgrade;

	public GameObject checkMarkStrokeUpgrade;

	public GameObject checkMarkTotalOrganFailureUpgrade;

	public GameObject checkMarkPneumoniaUpgrade;

	public UIHoverPosition hoverPosition;

	private void OnEnable()
	{
		btnSepticShockUpgrade.onClick.AddListener(ClickSepticShockUpgrade);
		btnHeartAttackUpgrade.onClick.AddListener(ClickHeartAttackUpgrade);
		btnStrokeUpgrade.onClick.AddListener(ClickStrokeUpgrade);
		btnTotalOrganFailureUpgrade.onClick.AddListener(ClickTotalOrganFailureUpgrade);
		btnPneumoniaUpgrade.onClick.AddListener(ClickPneumoniaUpgrade);
		septicShockHoverHandler.AddOnHoverOverAction(OnHoverOverSepticShock);
		septicShockHoverHandler.AddOnHoverOutAction(OnHoverOutSepticShock);
		heartAttackHoverHandler.AddOnHoverOverAction(OnHoverOverHeartAttack);
		heartAttackHoverHandler.AddOnHoverOutAction(OnHoverOutHeartAttack);
		strokeHoverHandler.AddOnHoverOverAction(OnHoverOverStroke);
		strokeHoverHandler.AddOnHoverOutAction(OnHoverOutStroke);
		totalOrganFailureHoverHandler.AddOnHoverOverAction(OnHoverOverTotalOrganFailure);
		totalOrganFailureHoverHandler.AddOnHoverOutAction(OnHoverOutTotalOrganFailure);
		pneumoniaHoverHandler.AddOnHoverOverAction(OnHoverOverPneumonia);
		pneumoniaHoverHandler.AddOnHoverOutAction(OnHoverOutPneumonia);
	}

	private void OnDisable()
	{
		btnSepticShockUpgrade.onClick.RemoveListener(ClickSepticShockUpgrade);
		btnHeartAttackUpgrade.onClick.RemoveListener(ClickHeartAttackUpgrade);
		btnStrokeUpgrade.onClick.RemoveListener(ClickStrokeUpgrade);
		btnTotalOrganFailureUpgrade.onClick.RemoveListener(ClickTotalOrganFailureUpgrade);
		btnPneumoniaUpgrade.onClick.RemoveListener(ClickPneumoniaUpgrade);
		septicShockHoverHandler.RemoveOnHoverOverAction(OnHoverOverSepticShock);
		septicShockHoverHandler.RemoveOnHoverOutAction(OnHoverOutSepticShock);
	}

	private void ClickSepticShockUpgrade()
	{
		onSepticShockUpgradeClicked?.Invoke();
	}

	private void ClickHeartAttackUpgrade()
	{
		onHeartAttackUpgradeClicked?.Invoke();
	}

	private void ClickStrokeUpgrade()
	{
		onStrokeUpgradeClicked?.Invoke();
	}

	private void ClickTotalOrganFailureUpgrade()
	{
		onTotalOrganFailureUpgradeClicked?.Invoke();
	}

	private void ClickPneumoniaUpgrade()
	{
		onPneumoniaUpgradeClicked?.Invoke();
	}

	private void OnHoverOverSepticShock()
	{
		onSepticShockHoveredOver?.Invoke(hoverPosition);
	}

	private void OnHoverOutSepticShock()
	{
		onSepticShockHoveredOut?.Invoke();
	}

	private void OnHoverOverHeartAttack()
	{
		onHeartAttackHoveredOver?.Invoke(hoverPosition);
	}

	private void OnHoverOutHeartAttack()
	{
		onHeartAttackHoveredOut?.Invoke();
	}

	private void OnHoverOverStroke()
	{
		onStrokeHoveredOver?.Invoke(hoverPosition);
	}

	private void OnHoverOutStroke()
	{
		onStrokeHoveredOut?.Invoke();
	}

	private void OnHoverOverTotalOrganFailure()
	{
		onTotalOrganFailureHoveredOver?.Invoke(hoverPosition);
	}

	private void OnHoverOutTotalOrganFailure()
	{
		onTotalOrganFailureHoveredOut?.Invoke();
	}

	private void OnHoverOverPneumonia()
	{
		onPneumoniaHoveredOver?.Invoke(hoverPosition);
	}

	private void OnHoverOutPneumonia()
	{
		onPneumoniaHoveredOut?.Invoke();
	}
}
