using System;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine.UI;

public class TransmissionUIModel : MVCUIModel
{
	public Action onAirBorneUpgradeClicked;

	public Action onConsumptionUpgradeClicked;

	public Action onDirectContactUpgradeClicked;

	public Action onCombatUpgradeClicked;

	public Action<UIHoverPosition> onAirBorneHoveredOver;

	public Action<UIHoverPosition> onConsumptionHoveredOver;

	public Action<UIHoverPosition> onDirectContactHoveredOver;

	public Action<UIHoverPosition> onCombatHoveredOver;

	public Action onAirBorneHoveredOut;

	public Action onConsumptionHoveredOut;

	public Action onDirectContactHoveredOut;

	public Action onCombatHoveredOut;

	public Button btnAirBorneUpgrade;

	public Button btnConsumptionUpgrade;

	public Button btnDirectContactUpgrade;

	public Button btnCombatUpgrade;

	public TextMeshProUGUI txtAirBorneUpgrade;

	public TextMeshProUGUI txtConsumptionUpgrade;

	public TextMeshProUGUI txtDirectContactUpgrade;

	public TextMeshProUGUI txtCombatUpgrade;

	public RuinarchText txtAirBorneCost;

	public RuinarchText txtAirBorneRate;

	public RuinarchText txtConsumptionCost;

	public RuinarchText txtConsumptionRate;

	public RuinarchText txtDirectContactCost;

	public RuinarchText txtDirectContactRate;

	public RuinarchText txtCombatUpgradeCost;

	public RuinarchText txtCombatRate;

	public HoverHandler airBorneHoverHandler;

	public HoverHandler consumptionHoverHandler;

	public HoverHandler directContactHoverHandler;

	public HoverHandler combatHoverHandler;

	public UIHoverPosition tooltipPosition;

	private void OnEnable()
	{
		btnAirBorneUpgrade.onClick.AddListener(ClickAirBorneUpgrade);
		btnConsumptionUpgrade.onClick.AddListener(ClickConsumptionUpgrade);
		btnDirectContactUpgrade.onClick.AddListener(ClickDirectContactUpgrade);
		btnCombatUpgrade.onClick.AddListener(ClickCombatUpgrade);
		airBorneHoverHandler.AddOnHoverOverAction(OnHoverOverAirborne);
		consumptionHoverHandler.AddOnHoverOverAction(OnHoverOverConsumption);
		directContactHoverHandler.AddOnHoverOverAction(OnHoverOverDirectContact);
		combatHoverHandler.AddOnHoverOverAction(OnHoverOverCombat);
		airBorneHoverHandler.AddOnHoverOutAction(OnHoverOutAirborne);
		consumptionHoverHandler.AddOnHoverOutAction(OnHoverOutConsumption);
		directContactHoverHandler.AddOnHoverOutAction(OnHoverOutDirectContact);
		combatHoverHandler.AddOnHoverOutAction(OnHoverOutCombat);
	}

	private void OnDisable()
	{
		btnAirBorneUpgrade.onClick.RemoveListener(ClickAirBorneUpgrade);
		btnConsumptionUpgrade.onClick.RemoveListener(ClickConsumptionUpgrade);
		btnDirectContactUpgrade.onClick.RemoveListener(ClickDirectContactUpgrade);
		btnCombatUpgrade.onClick.RemoveListener(ClickCombatUpgrade);
		airBorneHoverHandler.RemoveOnHoverOverAction(OnHoverOverAirborne);
		consumptionHoverHandler.RemoveOnHoverOverAction(OnHoverOverConsumption);
		directContactHoverHandler.RemoveOnHoverOverAction(OnHoverOverDirectContact);
		combatHoverHandler.RemoveOnHoverOverAction(OnHoverOverCombat);
		airBorneHoverHandler.RemoveOnHoverOutAction(OnHoverOutAirborne);
		consumptionHoverHandler.RemoveOnHoverOutAction(OnHoverOutConsumption);
		directContactHoverHandler.RemoveOnHoverOutAction(OnHoverOutDirectContact);
		combatHoverHandler.RemoveOnHoverOutAction(OnHoverOutCombat);
	}

	private void ClickAirBorneUpgrade()
	{
		onAirBorneUpgradeClicked?.Invoke();
	}

	private void ClickConsumptionUpgrade()
	{
		onConsumptionUpgradeClicked?.Invoke();
	}

	private void ClickDirectContactUpgrade()
	{
		onDirectContactUpgradeClicked?.Invoke();
	}

	private void ClickCombatUpgrade()
	{
		onCombatUpgradeClicked?.Invoke();
	}

	private void OnHoverOverAirborne()
	{
		onAirBorneHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOverConsumption()
	{
		onConsumptionHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOverDirectContact()
	{
		onDirectContactHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOverCombat()
	{
		onCombatHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOutAirborne()
	{
		onAirBorneHoveredOut?.Invoke();
	}

	private void OnHoverOutConsumption()
	{
		onConsumptionHoveredOut?.Invoke();
	}

	private void OnHoverOutDirectContact()
	{
		onDirectContactHoveredOut?.Invoke();
	}

	private void OnHoverOutCombat()
	{
		onCombatHoveredOut?.Invoke();
	}
}
