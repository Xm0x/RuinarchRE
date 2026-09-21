using System;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine.UI;

public class OnDeathUIModel : MVCUIModel
{
	public Action onIgniteUpgradeClicked;

	public Action onWalkerZombieUpgradeClicked;

	public Action onMana2_3UpgradeClicked;

	public Action onRandomSpirit_1UpgradeClicked;

	public Action<UIHoverPosition> onIgniteHoveredOver;

	public Action<UIHoverPosition> onWalkerZombieHoveredOver;

	public Action<UIHoverPosition> onManaHoveredOver;

	public Action<UIHoverPosition> onSpiritHoveredOver;

	public Action onIgniteHoveredOut;

	public Action onWalkerZombieHoveredOut;

	public Action onManaHoveredOut;

	public Action onSpiritHoveredOut;

	public Button btnIgniteUpgrade;

	public Button btnWalkerZombieUpgrade;

	public Button btnMana2_3Upgrade;

	public Button btnRandomSpirit_1Upgrade;

	public TextMeshProUGUI txtIgniteUpgrade;

	public TextMeshProUGUI txtWalkerZombieUpgrade;

	public TextMeshProUGUI txtMana2_3Upgrade;

	public TextMeshProUGUI txtRandomSpirit_1Upgrade;

	public RuinarchText txtIgniteUpgradeCost;

	public RuinarchText txtIgniteEffect;

	public RuinarchText txtWalkerZombieUpgradeCost;

	public RuinarchText txtWalkerZombieEffect;

	public RuinarchText txtMana2_3UpgradeCost;

	public RuinarchText txtMana2_3Effect;

	public RuinarchText txtRandomSpirit_1UpgradeCost;

	public RuinarchText txtRandomSpirit_1Effect;

	public HoverHandler igniteHoverHandler;

	public HoverHandler walkerZombieHoverHandler;

	public HoverHandler manaHoverHandler;

	public HoverHandler spiritHoverHandler;

	public UIHoverPosition tooltipPosition;

	private void OnEnable()
	{
		btnIgniteUpgrade.onClick.AddListener(ClickIgniteUpgrade);
		btnWalkerZombieUpgrade.onClick.AddListener(ClickWalkerZombieUpgrade);
		btnMana2_3Upgrade.onClick.AddListener(ClickMana2_3Upgrade);
		btnRandomSpirit_1Upgrade.onClick.AddListener(ClickRandomSpirit_1Upgrade);
		igniteHoverHandler.AddOnHoverOverAction(OnHoverOverIgnite);
		igniteHoverHandler.AddOnHoverOutAction(OnHoverOutIgnite);
		walkerZombieHoverHandler.AddOnHoverOverAction(OnHoverOverWalkerZombie);
		walkerZombieHoverHandler.AddOnHoverOutAction(OnHoverOutWalkerZombie);
		manaHoverHandler.AddOnHoverOverAction(OnHoverOverMana);
		manaHoverHandler.AddOnHoverOutAction(OnHoverOutMana);
		spiritHoverHandler.AddOnHoverOverAction(OnHoverOverSpirit);
		spiritHoverHandler.AddOnHoverOutAction(OnHoverOutSpirit);
	}

	private void OnDisable()
	{
		btnIgniteUpgrade.onClick.RemoveListener(ClickIgniteUpgrade);
		btnWalkerZombieUpgrade.onClick.RemoveListener(ClickWalkerZombieUpgrade);
		btnMana2_3Upgrade.onClick.RemoveListener(ClickMana2_3Upgrade);
		btnRandomSpirit_1Upgrade.onClick.RemoveListener(ClickRandomSpirit_1Upgrade);
		igniteHoverHandler.RemoveOnHoverOverAction(OnHoverOverIgnite);
		igniteHoverHandler.RemoveOnHoverOutAction(OnHoverOutIgnite);
		walkerZombieHoverHandler.RemoveOnHoverOverAction(OnHoverOverWalkerZombie);
		walkerZombieHoverHandler.RemoveOnHoverOutAction(OnHoverOutWalkerZombie);
		manaHoverHandler.RemoveOnHoverOverAction(OnHoverOverMana);
		manaHoverHandler.RemoveOnHoverOutAction(OnHoverOutMana);
		spiritHoverHandler.RemoveOnHoverOverAction(OnHoverOverSpirit);
		spiritHoverHandler.RemoveOnHoverOutAction(OnHoverOutSpirit);
	}

	private void ClickIgniteUpgrade()
	{
		onIgniteUpgradeClicked?.Invoke();
	}

	private void ClickWalkerZombieUpgrade()
	{
		onWalkerZombieUpgradeClicked?.Invoke();
	}

	private void ClickMana2_3Upgrade()
	{
		onMana2_3UpgradeClicked?.Invoke();
	}

	private void ClickRandomSpirit_1Upgrade()
	{
		onRandomSpirit_1UpgradeClicked?.Invoke();
	}

	private void OnHoverOverIgnite()
	{
		onIgniteHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOverWalkerZombie()
	{
		onWalkerZombieHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOverMana()
	{
		onManaHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOverSpirit()
	{
		onSpiritHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOutIgnite()
	{
		onIgniteHoveredOut?.Invoke();
	}

	private void OnHoverOutWalkerZombie()
	{
		onWalkerZombieHoveredOut?.Invoke();
	}

	private void OnHoverOutMana()
	{
		onManaHoveredOut?.Invoke();
	}

	private void OnHoverOutSpirit()
	{
		onSpiritHoveredOut?.Invoke();
	}
}
