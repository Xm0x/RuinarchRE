using System;
using Ruinarch.MVCFramework;
using UnityEngine.UI;

public class PrimordialPoolStatsUpgradeUIModel : MVCUIModel
{
	public Action onStrengthUpgradeClicked;

	public Action onIntelligenceUpgradeClicked;

	public Action onPiercingUpgradeClicked;

	public Action onMentalResistanceUpgradeClicked;

	public Action onPhysicalResistanceUpgradeClicked;

	public Action onElementalResistanceUpgradeClicked;

	public Action onSecondarylResistanceUpgradeClicked;

	public Action<UIHoverPosition> onStrengthHoveredOver;

	public Action<UIHoverPosition> onIntelligenceHoveredOver;

	public Action<UIHoverPosition> onPiercingHoveredOver;

	public Action<UIHoverPosition> onMentalResistanceHoveredOver;

	public Action<UIHoverPosition> onPhysicalResistanceHoveredOver;

	public Action<UIHoverPosition> onElementalresistanceHoveredOver;

	public Action<UIHoverPosition> onSecondaryResistanceHoveredOver;

	public Action onStrengthHoveredOut;

	public Action onIntelligenceHoveredOut;

	public Action onPiercingHoveredOut;

	public Action onMentalResistanceHoveredOut;

	public Action onPhysicalResistanceHoveredOut;

	public Action onElementalResistanceHoveredOut;

	public Action onSecondaryResistanceHoveredOut;

	public Action onBtnStrengthHoveredOver;

	public Action onBtnIntelligenceHoveredOver;

	public Action onBtnPiercingHoveredOver;

	public Action onBtnMentalResistanceHoveredOver;

	public Action onBtnPhysicalResistanceHoveredOver;

	public Action onBtnElementalresistanceHoveredOver;

	public Action onBtnSecondaryResistanceHoveredOver;

	public Action onBtnStrengthHoveredOut;

	public Action onBtnIntelligenceHoveredOut;

	public Action onBtnPiercingHoveredOut;

	public Action onBtnMentalResistanceHoveredOut;

	public Action onBtnPhysicalResistanceHoveredOut;

	public Action onBtnElementalResistanceHoveredOut;

	public Action onBtnSecondaryResistanceHoveredOut;

	public Button btnStrengthUpgrade;

	public Button btnIntelligenceUpgrade;

	public Button btnPiercingUpgrade;

	public Button btnMentalResistanceUpgrade;

	public Button btnPhysicalResistanceUpgrade;

	public Button btnElementalResistanceUpgrade;

	public Button btnSecondaryResistanceUpgrade;

	public RuinarchText txtStrengthCost;

	public RuinarchText txtStrengthRate;

	public RuinarchText txtIntelligenceCost;

	public RuinarchText txtIntelligenceRate;

	public RuinarchText txtPiercingCost;

	public RuinarchText txtPiercingRate;

	public RuinarchText txtMentalResistanceCost;

	public RuinarchText txtMentalResistanceRate;

	public RuinarchText txtPhysicalResistanceCost;

	public RuinarchText txtPhysicalResistanceRate;

	public RuinarchText txtElementalResistanceCost;

	public RuinarchText txtElementalResistanceRate;

	public RuinarchText txtSecondaryResistanceCost;

	public RuinarchText txtSecondaryResistanceRate;

	public HoverHandler strengthHoverHandler;

	public HoverHandler intelligenceHoverHandler;

	public HoverHandler piercingHoverHandler;

	public HoverHandler mentalResistanceHoverHandler;

	public HoverHandler physicalResistanceHoverHandler;

	public HoverHandler elementalResistanceHoverHandler;

	public HoverHandler secondaryResistanceHoverHandler;

	public HoverHandler btnUpgradeStrengthHoverHandler;

	public HoverHandler btnUpgradeIntelligenceHoverHandler;

	public HoverHandler btnUpgradePiercingHoverHandler;

	public HoverHandler btnUpgradeMentalResistanceHoverHandler;

	public HoverHandler btnUpgradePhysicalResistanceHoverHandler;

	public HoverHandler btnUpgradeElementalResistanceHoverHandler;

	public HoverHandler btnUpgradeSecondaryResistanceHoverHandler;

	public UIHoverPosition tooltipPosition;

	private void OnEnable()
	{
		btnStrengthUpgrade.onClick.AddListener(ClickStrengthUpgrade);
		btnIntelligenceUpgrade.onClick.AddListener(ClickIntelligenceUpgrade);
		btnPiercingUpgrade.onClick.AddListener(ClickPiercingUpgrade);
		btnMentalResistanceUpgrade.onClick.AddListener(ClickMentalResistanceUpgrade);
		btnPhysicalResistanceUpgrade.onClick.AddListener(ClickPhysicalResistanceUpgrade);
		btnElementalResistanceUpgrade.onClick.AddListener(ClickElementalResistanceUpgrade);
		btnSecondaryResistanceUpgrade.onClick.AddListener(ClickSecondaryResistanceUpgrade);
		strengthHoverHandler.AddOnHoverOverAction(OnHoverOverStrength);
		intelligenceHoverHandler.AddOnHoverOverAction(OnHoverOverIntelligence);
		piercingHoverHandler.AddOnHoverOverAction(OnHoverOverPiercing);
		mentalResistanceHoverHandler.AddOnHoverOverAction(OnHoverOverMentalResistance);
		physicalResistanceHoverHandler.AddOnHoverOverAction(OnHoverOverPhysicalResistance);
		elementalResistanceHoverHandler.AddOnHoverOverAction(OnHoverOverElementalResistance);
		secondaryResistanceHoverHandler.AddOnHoverOverAction(OnHoverOverSecondaryResistance);
		strengthHoverHandler.AddOnHoverOutAction(OnHoverOutStrength);
		intelligenceHoverHandler.AddOnHoverOutAction(OnHoverOutIntelligence);
		piercingHoverHandler.AddOnHoverOutAction(OnHoverOutPiercing);
		mentalResistanceHoverHandler.AddOnHoverOutAction(OnHoverOutMentalResistance);
		physicalResistanceHoverHandler.AddOnHoverOutAction(OnHoverOutPhysicalResistance);
		elementalResistanceHoverHandler.AddOnHoverOutAction(OnHoverOutElementalResistance);
		secondaryResistanceHoverHandler.AddOnHoverOutAction(OnHoverOutSecondaryResistance);
		btnUpgradeStrengthHoverHandler.AddOnHoverOverAction(OnHoverOverBtnUpgradeStrength);
		btnUpgradeIntelligenceHoverHandler.AddOnHoverOverAction(OnHoverOverBtnUpgradeIntelligence);
		btnUpgradePiercingHoverHandler.AddOnHoverOverAction(OnHoverOverBtnUpgradePiercing);
		btnUpgradeMentalResistanceHoverHandler.AddOnHoverOverAction(OnHoverOverBtnUpgradeMentalResistance);
		btnUpgradePhysicalResistanceHoverHandler.AddOnHoverOverAction(OnHoverOverBtnUpgradePhysicalResistance);
		btnUpgradeElementalResistanceHoverHandler.AddOnHoverOverAction(OnHoverOverBtnUpgradeElementalResistance);
		btnUpgradeSecondaryResistanceHoverHandler.AddOnHoverOverAction(OnHoverOverBtnUpgradeSecondaryResistance);
		btnUpgradeStrengthHoverHandler.AddOnHoverOutAction(OnHoverOutBtnUpgradeStrength);
		btnUpgradeIntelligenceHoverHandler.AddOnHoverOutAction(OnHoverOutBtnUpgradeIntelligence);
		btnUpgradePiercingHoverHandler.AddOnHoverOutAction(OnHoverOutBtnUpgradePiercing);
		btnUpgradeMentalResistanceHoverHandler.AddOnHoverOutAction(OnHoverOutBtnUpgradeMentalResistance);
		btnUpgradePhysicalResistanceHoverHandler.AddOnHoverOutAction(OnHoverOutBtnUpgradePhysicalResistance);
		btnUpgradeElementalResistanceHoverHandler.AddOnHoverOutAction(OnHoverOutBtnUpgradeElementalResistance);
		btnUpgradeSecondaryResistanceHoverHandler.AddOnHoverOutAction(OnHoverOutBtnUpgradeSecondaryResistance);
	}

	private void OnDisable()
	{
		btnStrengthUpgrade.onClick.RemoveListener(ClickStrengthUpgrade);
		btnIntelligenceUpgrade.onClick.RemoveListener(ClickIntelligenceUpgrade);
		btnPiercingUpgrade.onClick.RemoveListener(ClickPiercingUpgrade);
		btnMentalResistanceUpgrade.onClick.RemoveListener(ClickMentalResistanceUpgrade);
		btnPhysicalResistanceUpgrade.onClick.RemoveListener(ClickPhysicalResistanceUpgrade);
		btnElementalResistanceUpgrade.onClick.RemoveListener(ClickElementalResistanceUpgrade);
		btnSecondaryResistanceUpgrade.onClick.RemoveListener(ClickSecondaryResistanceUpgrade);
		strengthHoverHandler.RemoveOnHoverOverAction(OnHoverOverStrength);
		intelligenceHoverHandler.RemoveOnHoverOverAction(OnHoverOverIntelligence);
		piercingHoverHandler.RemoveOnHoverOverAction(OnHoverOverPiercing);
		mentalResistanceHoverHandler.RemoveOnHoverOverAction(OnHoverOverMentalResistance);
		physicalResistanceHoverHandler.RemoveOnHoverOverAction(OnHoverOverPhysicalResistance);
		elementalResistanceHoverHandler.RemoveOnHoverOverAction(OnHoverOverElementalResistance);
		secondaryResistanceHoverHandler.RemoveOnHoverOverAction(OnHoverOverSecondaryResistance);
		strengthHoverHandler.RemoveOnHoverOutAction(OnHoverOutStrength);
		intelligenceHoverHandler.RemoveOnHoverOutAction(OnHoverOutIntelligence);
		piercingHoverHandler.RemoveOnHoverOutAction(OnHoverOutPiercing);
		mentalResistanceHoverHandler.RemoveOnHoverOutAction(OnHoverOutMentalResistance);
		physicalResistanceHoverHandler.RemoveOnHoverOutAction(OnHoverOutPhysicalResistance);
		elementalResistanceHoverHandler.RemoveOnHoverOutAction(OnHoverOutElementalResistance);
		secondaryResistanceHoverHandler.RemoveOnHoverOutAction(OnHoverOutSecondaryResistance);
	}

	private void ClickStrengthUpgrade()
	{
		onStrengthUpgradeClicked?.Invoke();
	}

	private void ClickIntelligenceUpgrade()
	{
		onIntelligenceUpgradeClicked?.Invoke();
	}

	private void ClickPiercingUpgrade()
	{
		onPiercingUpgradeClicked?.Invoke();
	}

	private void ClickMentalResistanceUpgrade()
	{
		onMentalResistanceUpgradeClicked?.Invoke();
	}

	private void ClickPhysicalResistanceUpgrade()
	{
		onPhysicalResistanceUpgradeClicked?.Invoke();
	}

	private void ClickElementalResistanceUpgrade()
	{
		onElementalResistanceUpgradeClicked?.Invoke();
	}

	private void ClickSecondaryResistanceUpgrade()
	{
		onSecondarylResistanceUpgradeClicked?.Invoke();
	}

	private void OnHoverOverStrength()
	{
		onStrengthHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOverIntelligence()
	{
		onIntelligenceHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOverPiercing()
	{
		onPiercingHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOverMentalResistance()
	{
		onMentalResistanceHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOverPhysicalResistance()
	{
		onPhysicalResistanceHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOverElementalResistance()
	{
		onElementalresistanceHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOverSecondaryResistance()
	{
		onSecondaryResistanceHoveredOver?.Invoke(tooltipPosition);
	}

	private void OnHoverOutStrength()
	{
		onStrengthHoveredOut?.Invoke();
	}

	private void OnHoverOutIntelligence()
	{
		onIntelligenceHoveredOut?.Invoke();
	}

	private void OnHoverOutPiercing()
	{
		onPiercingHoveredOut?.Invoke();
	}

	private void OnHoverOutMentalResistance()
	{
		onMentalResistanceHoveredOut?.Invoke();
	}

	private void OnHoverOutPhysicalResistance()
	{
		onPhysicalResistanceHoveredOut?.Invoke();
	}

	private void OnHoverOutElementalResistance()
	{
		onElementalResistanceHoveredOut?.Invoke();
	}

	private void OnHoverOutSecondaryResistance()
	{
		onSecondaryResistanceHoveredOut?.Invoke();
	}

	private void OnHoverOverBtnUpgradeStrength()
	{
		onBtnStrengthHoveredOver?.Invoke();
	}

	private void OnHoverOverBtnUpgradeIntelligence()
	{
		onBtnIntelligenceHoveredOver?.Invoke();
	}

	private void OnHoverOverBtnUpgradePiercing()
	{
		onBtnPiercingHoveredOver?.Invoke();
	}

	private void OnHoverOverBtnUpgradeMentalResistance()
	{
		onBtnMentalResistanceHoveredOver?.Invoke();
	}

	private void OnHoverOverBtnUpgradePhysicalResistance()
	{
		onBtnPhysicalResistanceHoveredOver?.Invoke();
	}

	private void OnHoverOverBtnUpgradeElementalResistance()
	{
		onBtnElementalresistanceHoveredOver?.Invoke();
	}

	private void OnHoverOverBtnUpgradeSecondaryResistance()
	{
		onBtnSecondaryResistanceHoveredOver?.Invoke();
	}

	private void OnHoverOutBtnUpgradeStrength()
	{
		onBtnStrengthHoveredOut?.Invoke();
	}

	private void OnHoverOutBtnUpgradeIntelligence()
	{
		onBtnIntelligenceHoveredOut?.Invoke();
	}

	private void OnHoverOutBtnUpgradePiercing()
	{
		onBtnPiercingHoveredOut?.Invoke();
	}

	private void OnHoverOutBtnUpgradeMentalResistance()
	{
		onBtnMentalResistanceHoveredOut?.Invoke();
	}

	private void OnHoverOutBtnUpgradePhysicalResistance()
	{
		onBtnPhysicalResistanceHoveredOut?.Invoke();
	}

	private void OnHoverOutBtnUpgradeElementalResistance()
	{
		onBtnElementalResistanceHoveredOut?.Invoke();
	}

	private void OnHoverOutBtnUpgradeSecondaryResistance()
	{
		onBtnSecondaryResistanceHoveredOut?.Invoke();
	}
}
