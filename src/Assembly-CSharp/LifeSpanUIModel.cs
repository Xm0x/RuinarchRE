using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using TMPro;

public class LifeSpanUIModel : MVCUIModel
{
	public Action onObjectsUpgradeClicked;

	public Action onElvesUpgradeClicked;

	public Action onHumansUpgradeClicked;

	public Action onMonstersUpgradeClicked;

	public Action onUndeadUpgradeClicked;

	public Action<UIHoverPosition> onObjectsHoveredOver;

	public Action<UIHoverPosition> onElvesHoveredOver;

	public Action<UIHoverPosition> onHumansHoveredOver;

	public Action<UIHoverPosition> onMonstersHoveredOver;

	public Action<UIHoverPosition> onUndeadHoveredOver;

	public Action onObjectsHoveredOut;

	public Action onElvesHoveredOut;

	public Action onHumansHoveredOut;

	public Action onMonstersHoveredOut;

	public Action onUndeadHoveredOut;

	public Action onObjectsUpgradeBtnHoveredOver;

	public Action onElvesUpgradeBtnHoveredOver;

	public Action onHumansUpgradeBtnHoveredOver;

	public Action onMonstersUpgradeBtnHoveredOver;

	public Action onUndeadUpgradeBtnHoveredOver;

	public Action onObjectsUpgradeBtnHoveredOut;

	public Action onElvesUpgradeBtnHoveredOut;

	public Action onHumansUpgradeBtnHoveredOut;

	public Action onMonstersUpgradeBtnHoveredOut;

	public Action onUndeadUpgradeBtnHoveredOut;

	public RuinarchButton btnObjectsUpgrade;

	public RuinarchButton btnElvesUpgrade;

	public RuinarchButton btnHumansUpgrade;

	public RuinarchButton btnMonstersUpgrade;

	public RuinarchButton btnUndeadUpgrade;

	public TextMeshProUGUI txtObjectsUpgrade;

	public TextMeshProUGUI txtElvesUpgrade;

	public TextMeshProUGUI txtHumansUpgrade;

	public TextMeshProUGUI txtMonstersUpgrade;

	public TextMeshProUGUI txtUndeadUpgrade;

	public RuinarchText txtTileObjectCost;

	public RuinarchText txtTileObjectInfectionTime;

	public RuinarchText txtElvesCost;

	public RuinarchText txtElvesInfectionTime;

	public RuinarchText txtHumansCost;

	public RuinarchText txtHumansInfectionTime;

	public RuinarchText txtMonstersCost;

	public RuinarchText txtMonstersInfectionTime;

	public RuinarchText txtUndeadCost;

	public RuinarchText txtUndeadInfectionTime;

	public HoverHandler objectsHoverHandler;

	public HoverHandler elvesHoverHandler;

	public HoverHandler humansHoverHandler;

	public HoverHandler monstersHoverHandler;

	public HoverHandler undeadHoverHandler;

	public UIHoverPosition tooltipPosition;

	private void OnEnable()
	{
		btnObjectsUpgrade.onClick.AddListener(ClickObjectsUpgrade);
		btnElvesUpgrade.onClick.AddListener(ClickElvesUpgrade);
		btnHumansUpgrade.onClick.AddListener(ClickHumansUpgrade);
		btnMonstersUpgrade.onClick.AddListener(ClickMonstersUpgrade);
		btnUndeadUpgrade.onClick.AddListener(ClickUndeadUpgrade);
		objectsHoverHandler.AddOnHoverOverAction(OnHoverOverTileObject);
		objectsHoverHandler.AddOnHoverOutAction(OnHoverOutTileObject);
		elvesHoverHandler.AddOnHoverOverAction(OnHoverOverElves);
		elvesHoverHandler.AddOnHoverOutAction(OnHoverOutElves);
		humansHoverHandler.AddOnHoverOverAction(OnHoverOverHumans);
		humansHoverHandler.AddOnHoverOutAction(OnHoverOutHumans);
		monstersHoverHandler.AddOnHoverOverAction(OnHoverOverMonsters);
		monstersHoverHandler.AddOnHoverOutAction(OnHoverOutMonsters);
		undeadHoverHandler.AddOnHoverOverAction(OnHoverOverUndead);
		undeadHoverHandler.AddOnHoverOutAction(OnHoverOutUndead);
		btnObjectsUpgrade.AddHoverOverAction(OnHoverOverObjectsUpgradeBtn);
		btnElvesUpgrade.AddHoverOverAction(OnHoverOverElvesUpgradeBtn);
		btnHumansUpgrade.AddHoverOverAction(OnHoverOverHumansUpgradeBtn);
		btnMonstersUpgrade.AddHoverOverAction(OnHoverOverMonstersUpgradeBtn);
		btnUndeadUpgrade.AddHoverOverAction(OnHoverOverUndeadUpgradeBtn);
		btnObjectsUpgrade.AddHoverOutAction(OnHoverOutObjectsUpgradeBtn);
		btnElvesUpgrade.AddHoverOutAction(OnHoverOutElvesUpgradeBtn);
		btnHumansUpgrade.AddHoverOutAction(OnHoverOutHumansUpgradeBtn);
		btnMonstersUpgrade.AddHoverOutAction(OnHoverOutMonstersUpgradeBtn);
		btnUndeadUpgrade.AddHoverOutAction(OnHoverOutUndeadUpgradeBtn);
	}

	private void OnDisable()
	{
		btnObjectsUpgrade.onClick.RemoveListener(ClickObjectsUpgrade);
		btnElvesUpgrade.onClick.RemoveListener(ClickElvesUpgrade);
		btnHumansUpgrade.onClick.RemoveListener(ClickHumansUpgrade);
		btnMonstersUpgrade.onClick.RemoveListener(ClickMonstersUpgrade);
		btnUndeadUpgrade.onClick.RemoveListener(ClickUndeadUpgrade);
		objectsHoverHandler.RemoveOnHoverOverAction(OnHoverOverTileObject);
		objectsHoverHandler.RemoveOnHoverOutAction(OnHoverOutTileObject);
		elvesHoverHandler.RemoveOnHoverOverAction(OnHoverOverElves);
		elvesHoverHandler.RemoveOnHoverOutAction(OnHoverOutElves);
		humansHoverHandler.RemoveOnHoverOverAction(OnHoverOverHumans);
		humansHoverHandler.RemoveOnHoverOutAction(OnHoverOutHumans);
		monstersHoverHandler.RemoveOnHoverOverAction(OnHoverOverMonsters);
		monstersHoverHandler.RemoveOnHoverOutAction(OnHoverOutMonsters);
		undeadHoverHandler.RemoveOnHoverOverAction(OnHoverOverUndead);
		undeadHoverHandler.RemoveOnHoverOutAction(OnHoverOutUndead);
		btnObjectsUpgrade.RemoveHoverOverAction(OnHoverOverObjectsUpgradeBtn);
		btnElvesUpgrade.RemoveHoverOverAction(OnHoverOverElvesUpgradeBtn);
		btnHumansUpgrade.RemoveHoverOverAction(OnHoverOverHumansUpgradeBtn);
		btnMonstersUpgrade.RemoveHoverOverAction(OnHoverOverMonstersUpgradeBtn);
		btnUndeadUpgrade.RemoveHoverOverAction(OnHoverOverUndeadUpgradeBtn);
		btnObjectsUpgrade.RemoveHoverOutAction(OnHoverOutObjectsUpgradeBtn);
		btnElvesUpgrade.RemoveHoverOutAction(OnHoverOutElvesUpgradeBtn);
		btnHumansUpgrade.RemoveHoverOutAction(OnHoverOutHumansUpgradeBtn);
		btnMonstersUpgrade.RemoveHoverOutAction(OnHoverOutMonstersUpgradeBtn);
		btnUndeadUpgrade.RemoveHoverOutAction(OnHoverOutUndeadUpgradeBtn);
	}

	private void ClickObjectsUpgrade()
	{
		onObjectsUpgradeClicked?.Invoke();
	}

	private void ClickElvesUpgrade()
	{
		onElvesUpgradeClicked?.Invoke();
	}

	private void ClickHumansUpgrade()
	{
		onHumansUpgradeClicked?.Invoke();
	}

	private void ClickMonstersUpgrade()
	{
		onMonstersUpgradeClicked?.Invoke();
	}

	private void ClickUndeadUpgrade()
	{
		onUndeadUpgradeClicked?.Invoke();
	}

	private void OnHoverOverTileObject()
	{
		onObjectsHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOutTileObject()
	{
		onObjectsHoveredOut?.Invoke();
	}

	private void OnHoverOverElves()
	{
		onElvesHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOutElves()
	{
		onElvesHoveredOut?.Invoke();
	}

	private void OnHoverOverHumans()
	{
		onHumansHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOutHumans()
	{
		onHumansHoveredOut?.Invoke();
	}

	private void OnHoverOverMonsters()
	{
		onMonstersHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOutMonsters()
	{
		onMonstersHoveredOut?.Invoke();
	}

	private void OnHoverOverUndead()
	{
		onUndeadHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOutUndead()
	{
		onUndeadHoveredOut?.Invoke();
	}

	private void OnHoverOverObjectsUpgradeBtn()
	{
		onObjectsUpgradeBtnHoveredOver?.Invoke();
	}

	private void OnHoverOverElvesUpgradeBtn()
	{
		onElvesUpgradeBtnHoveredOver?.Invoke();
	}

	private void OnHoverOverHumansUpgradeBtn()
	{
		onHumansUpgradeBtnHoveredOver?.Invoke();
	}

	private void OnHoverOverMonstersUpgradeBtn()
	{
		onMonstersUpgradeBtnHoveredOver?.Invoke();
	}

	private void OnHoverOverUndeadUpgradeBtn()
	{
		onUndeadUpgradeBtnHoveredOver?.Invoke();
	}

	private void OnHoverOutObjectsUpgradeBtn()
	{
		onObjectsUpgradeBtnHoveredOut?.Invoke();
	}

	private void OnHoverOutElvesUpgradeBtn()
	{
		onElvesUpgradeBtnHoveredOut?.Invoke();
	}

	private void OnHoverOutHumansUpgradeBtn()
	{
		onHumansUpgradeBtnHoveredOut?.Invoke();
	}

	private void OnHoverOutMonstersUpgradeBtn()
	{
		onMonstersUpgradeBtnHoveredOut?.Invoke();
	}

	private void OnHoverOutUndeadUpgradeBtn()
	{
		onUndeadUpgradeBtnHoveredOut?.Invoke();
	}
}
