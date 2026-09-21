using System;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class PrimordialPoolStatsUpgradeUIView : MVCUIView
{
	public interface IListener
	{
		void OnStrengthUpgradeClicked();

		void OnIntelligenceUpgradeClicked();

		void OnPiercingUpgradeClicked();

		void OnMentalResistanceUpgradeClicked();

		void OnPhysicalResistanceUpgradeClicked();

		void OnElementalResistanceUpgradeClicked();

		void OnSecondaryResistanceUpgradeClicked();

		void OnStrengthHoveredOver(UIHoverPosition p_hoverPosition);

		void OnIntelligenceHoveredOver(UIHoverPosition p_hoverPosition);

		void OnPiercingHoveredOver(UIHoverPosition p_hoverPosition);

		void OnMentalResistanceHoveredOver(UIHoverPosition p_hoverPosition);

		void OnPhysicalResistanceHoveredOver(UIHoverPosition p_hoverPosition);

		void OnElementalResistanceHoveredOver(UIHoverPosition p_hoverPosition);

		void OnSecondaryResistanceHoveredOver(UIHoverPosition p_hoverPosition);

		void OnStrengthHoveredOut();

		void OnIntelligenceHoveredOut();

		void OnPiercingHoveredOut();

		void OnMentalResistanceHoveredOut();

		void OnPhysicalResistanceHoveredOut();

		void OnElementalResistanceHoveredOut();

		void OnSecondaryResistanceHoveredOut();

		void OnBtnUpgradeStrengthHoveredOver();

		void OnBtnUpgradeIntelligenceHoveredOver();

		void OnBtnUpgradePiercingHoveredOver();

		void OnBtnUpgradeMentalResistanceHoveredOver();

		void OnBtnUpgradePhysicalResistanceHoveredOver();

		void OnBtnUpgradeElementalResistanceHoveredOver();

		void OnBtnUpgradeSecondaryResistanceHoveredOver();

		void OnBtnUpgradeStrengthHoveredOut();

		void OnBtnUpgradeIntelligenceHoveredOut();

		void OnBtnUpgradePiercingHoveredOut();

		void OnBtnUpgradeMentalResistanceHoveredOut();

		void OnBtnUpgradePhysicalResistanceHoveredOut();

		void OnBtnUpgradeElementalResistanceHoveredOut();

		void OnBtnUpgradeSecondaryResistanceHoveredOut();
	}

	public PrimordialPoolStatsUpgradeUIModel UIModel => _baseAssetModel as PrimordialPoolStatsUpgradeUIModel;

	public static void Create(Canvas p_canvas, PrimordialPoolStatsUpgradeUIModel p_assets, Action<PrimordialPoolStatsUpgradeUIView> p_onCreate)
	{
		PrimordialPoolStatsUpgradeUIView primordialPoolStatsUpgradeUIView = new GameObject(typeof(PrimordialPoolStatsUpgradeUIView).ToString()).AddComponent<PrimordialPoolStatsUpgradeUIView>();
		PrimordialPoolStatsUpgradeUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		primordialPoolStatsUpgradeUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(primordialPoolStatsUpgradeUIView);
	}

	public void DisplayCategoryInfo(CHARACTER_CATEGORY p_targetCategory)
	{
		int num = 5;
		UIModel.txtStrengthRate.text = "<color=\"white\">" + PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentBonusForTextDisplay(PRIMORDIAL_STATS_BONUS.Str) + "%</color>";
		UIModel.txtStrengthCost.text = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentCostDisplay(PRIMORDIAL_STATS_BONUS.Str).ToString() + Utilities.ChaoticEnergyIcon();
		UIModel.txtIntelligenceRate.text = "<color=\"white\">" + PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentBonusForTextDisplay(PRIMORDIAL_STATS_BONUS.Int) + "%</color>";
		UIModel.txtIntelligenceCost.text = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentCostDisplay(PRIMORDIAL_STATS_BONUS.Int).ToString() + Utilities.ChaoticEnergyIcon();
		UIModel.txtPiercingRate.text = "<color=\"white\">+" + PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentBonusForTextDisplay(PRIMORDIAL_STATS_BONUS.Piercing) + "</color>";
		UIModel.txtPiercingCost.text = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentCostDisplay(PRIMORDIAL_STATS_BONUS.Piercing).ToString() + Utilities.ChaoticEnergyIcon();
		UIModel.txtMentalResistanceRate.text = "<color=\"white\">+" + PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentBonusForTextDisplay(PRIMORDIAL_STATS_BONUS.Mental_Res) + "</color>";
		UIModel.txtMentalResistanceCost.text = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentCostDisplay(PRIMORDIAL_STATS_BONUS.Mental_Res).ToString() + Utilities.ChaoticEnergyIcon();
		UIModel.txtPhysicalResistanceRate.text = "<color=\"white\">+" + PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentBonusForTextDisplay(PRIMORDIAL_STATS_BONUS.Physical_Res) + "</color>";
		UIModel.txtPhysicalResistanceCost.text = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentCostDisplay(PRIMORDIAL_STATS_BONUS.Physical_Res).ToString() + Utilities.ChaoticEnergyIcon();
		UIModel.txtElementalResistanceRate.text = "<color=\"white\">+" + PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentBonusForTextDisplay(PRIMORDIAL_STATS_BONUS.Elemental_Res) + "</color>";
		UIModel.txtElementalResistanceCost.text = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentCostDisplay(PRIMORDIAL_STATS_BONUS.Elemental_Res).ToString() + Utilities.ChaoticEnergyIcon();
		UIModel.txtSecondaryResistanceRate.text = "<color=\"white\">+" + PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentBonusForTextDisplay(PRIMORDIAL_STATS_BONUS.Secondary_Res) + "</color>";
		UIModel.txtSecondaryResistanceCost.text = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].GetCurrentCostDisplay(PRIMORDIAL_STATS_BONUS.Secondary_Res).ToString() + Utilities.ChaoticEnergyIcon();
		int chaoticEnergy = PlayerManager.Instance.player.currenciesComponent.chaoticEnergy;
		if (PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].CheckIfUpgradeable(chaoticEnergy, PRIMORDIAL_STATS_BONUS.Str) && PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].lvlStr < num)
		{
			UIModel.btnStrengthUpgrade.interactable = true;
		}
		else
		{
			UIModel.btnStrengthUpgrade.interactable = false;
		}
		if (PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].CheckIfUpgradeable(chaoticEnergy, PRIMORDIAL_STATS_BONUS.Int) && PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].lvlInt < num)
		{
			UIModel.btnIntelligenceUpgrade.interactable = true;
		}
		else
		{
			UIModel.btnIntelligenceUpgrade.interactable = false;
		}
		if (PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].CheckIfUpgradeable(chaoticEnergy, PRIMORDIAL_STATS_BONUS.Piercing) && PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].lvlPiercing < num)
		{
			UIModel.btnPiercingUpgrade.interactable = true;
		}
		else
		{
			UIModel.btnPiercingUpgrade.interactable = false;
		}
		if (PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].CheckIfUpgradeable(chaoticEnergy, PRIMORDIAL_STATS_BONUS.Mental_Res) && PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].lvlMentalResistance < num)
		{
			UIModel.btnMentalResistanceUpgrade.interactable = true;
		}
		else
		{
			UIModel.btnMentalResistanceUpgrade.interactable = false;
		}
		if (PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].CheckIfUpgradeable(chaoticEnergy, PRIMORDIAL_STATS_BONUS.Physical_Res) && PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].lvlPhysicalResistance < num)
		{
			UIModel.btnPhysicalResistanceUpgrade.interactable = true;
		}
		else
		{
			UIModel.btnPhysicalResistanceUpgrade.interactable = false;
		}
		if (PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].CheckIfUpgradeable(chaoticEnergy, PRIMORDIAL_STATS_BONUS.Elemental_Res) && PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].lvlElementalResistance < num)
		{
			UIModel.btnElementalResistanceUpgrade.interactable = true;
		}
		else
		{
			UIModel.btnElementalResistanceUpgrade.interactable = false;
		}
		if (PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].CheckIfUpgradeable(chaoticEnergy, PRIMORDIAL_STATS_BONUS.Secondary_Res) && PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[p_targetCategory].lvlSecondaryResistance < num)
		{
			UIModel.btnSecondaryResistanceUpgrade.interactable = true;
		}
		else
		{
			UIModel.btnSecondaryResistanceUpgrade.interactable = false;
		}
	}

	public void ShowNextUpgradeValue(RuinarchText p_text, string p_amount)
	{
		p_text.text = Utilities.ColorizeName(p_amount);
	}

	public void Subscribe(IListener p_listener)
	{
		PrimordialPoolStatsUpgradeUIModel uIModel = UIModel;
		uIModel.onStrengthUpgradeClicked = (Action)Delegate.Combine(uIModel.onStrengthUpgradeClicked, new Action(p_listener.OnStrengthUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel2 = UIModel;
		uIModel2.onIntelligenceUpgradeClicked = (Action)Delegate.Combine(uIModel2.onIntelligenceUpgradeClicked, new Action(p_listener.OnIntelligenceUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel3 = UIModel;
		uIModel3.onPiercingUpgradeClicked = (Action)Delegate.Combine(uIModel3.onPiercingUpgradeClicked, new Action(p_listener.OnPiercingUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel4 = UIModel;
		uIModel4.onMentalResistanceUpgradeClicked = (Action)Delegate.Combine(uIModel4.onMentalResistanceUpgradeClicked, new Action(p_listener.OnMentalResistanceUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel5 = UIModel;
		uIModel5.onPhysicalResistanceUpgradeClicked = (Action)Delegate.Combine(uIModel5.onPhysicalResistanceUpgradeClicked, new Action(p_listener.OnPhysicalResistanceUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel6 = UIModel;
		uIModel6.onElementalResistanceUpgradeClicked = (Action)Delegate.Combine(uIModel6.onElementalResistanceUpgradeClicked, new Action(p_listener.OnElementalResistanceUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel7 = UIModel;
		uIModel7.onSecondarylResistanceUpgradeClicked = (Action)Delegate.Combine(uIModel7.onSecondarylResistanceUpgradeClicked, new Action(p_listener.OnSecondaryResistanceUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel8 = UIModel;
		uIModel8.onStrengthHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel8.onStrengthHoveredOver, new Action<UIHoverPosition>(p_listener.OnStrengthHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel9 = UIModel;
		uIModel9.onIntelligenceHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel9.onIntelligenceHoveredOver, new Action<UIHoverPosition>(p_listener.OnIntelligenceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel10 = UIModel;
		uIModel10.onPiercingHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel10.onPiercingHoveredOver, new Action<UIHoverPosition>(p_listener.OnPiercingHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel11 = UIModel;
		uIModel11.onMentalResistanceHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel11.onMentalResistanceHoveredOver, new Action<UIHoverPosition>(p_listener.OnMentalResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel12 = UIModel;
		uIModel12.onPhysicalResistanceHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel12.onPhysicalResistanceHoveredOver, new Action<UIHoverPosition>(p_listener.OnPhysicalResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel13 = UIModel;
		uIModel13.onElementalresistanceHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel13.onElementalresistanceHoveredOver, new Action<UIHoverPosition>(p_listener.OnElementalResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel14 = UIModel;
		uIModel14.onSecondaryResistanceHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel14.onSecondaryResistanceHoveredOver, new Action<UIHoverPosition>(p_listener.OnSecondaryResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel15 = UIModel;
		uIModel15.onStrengthHoveredOut = (Action)Delegate.Combine(uIModel15.onStrengthHoveredOut, new Action(p_listener.OnStrengthHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel16 = UIModel;
		uIModel16.onIntelligenceHoveredOut = (Action)Delegate.Combine(uIModel16.onIntelligenceHoveredOut, new Action(p_listener.OnIntelligenceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel17 = UIModel;
		uIModel17.onPiercingHoveredOut = (Action)Delegate.Combine(uIModel17.onPiercingHoveredOut, new Action(p_listener.OnPiercingHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel18 = UIModel;
		uIModel18.onMentalResistanceHoveredOut = (Action)Delegate.Combine(uIModel18.onMentalResistanceHoveredOut, new Action(p_listener.OnMentalResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel19 = UIModel;
		uIModel19.onPhysicalResistanceHoveredOut = (Action)Delegate.Combine(uIModel19.onPhysicalResistanceHoveredOut, new Action(p_listener.OnPhysicalResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel20 = UIModel;
		uIModel20.onElementalResistanceHoveredOut = (Action)Delegate.Combine(uIModel20.onElementalResistanceHoveredOut, new Action(p_listener.OnElementalResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel21 = UIModel;
		uIModel21.onSecondaryResistanceHoveredOut = (Action)Delegate.Combine(uIModel21.onSecondaryResistanceHoveredOut, new Action(p_listener.OnSecondaryResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel22 = UIModel;
		uIModel22.onBtnStrengthHoveredOver = (Action)Delegate.Combine(uIModel22.onBtnStrengthHoveredOver, new Action(p_listener.OnBtnUpgradeStrengthHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel23 = UIModel;
		uIModel23.onBtnIntelligenceHoveredOver = (Action)Delegate.Combine(uIModel23.onBtnIntelligenceHoveredOver, new Action(p_listener.OnBtnUpgradeIntelligenceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel24 = UIModel;
		uIModel24.onBtnPiercingHoveredOver = (Action)Delegate.Combine(uIModel24.onBtnPiercingHoveredOver, new Action(p_listener.OnBtnUpgradePiercingHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel25 = UIModel;
		uIModel25.onBtnMentalResistanceHoveredOver = (Action)Delegate.Combine(uIModel25.onBtnMentalResistanceHoveredOver, new Action(p_listener.OnBtnUpgradeMentalResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel26 = UIModel;
		uIModel26.onBtnPhysicalResistanceHoveredOver = (Action)Delegate.Combine(uIModel26.onBtnPhysicalResistanceHoveredOver, new Action(p_listener.OnBtnUpgradePhysicalResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel27 = UIModel;
		uIModel27.onBtnElementalresistanceHoveredOver = (Action)Delegate.Combine(uIModel27.onBtnElementalresistanceHoveredOver, new Action(p_listener.OnBtnUpgradeElementalResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel28 = UIModel;
		uIModel28.onBtnSecondaryResistanceHoveredOver = (Action)Delegate.Combine(uIModel28.onBtnSecondaryResistanceHoveredOver, new Action(p_listener.OnBtnUpgradeSecondaryResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel29 = UIModel;
		uIModel29.onBtnStrengthHoveredOut = (Action)Delegate.Combine(uIModel29.onBtnStrengthHoveredOut, new Action(p_listener.OnBtnUpgradeStrengthHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel30 = UIModel;
		uIModel30.onBtnIntelligenceHoveredOut = (Action)Delegate.Combine(uIModel30.onBtnIntelligenceHoveredOut, new Action(p_listener.OnBtnUpgradeIntelligenceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel31 = UIModel;
		uIModel31.onBtnPiercingHoveredOut = (Action)Delegate.Combine(uIModel31.onBtnPiercingHoveredOut, new Action(p_listener.OnBtnUpgradePiercingHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel32 = UIModel;
		uIModel32.onBtnMentalResistanceHoveredOut = (Action)Delegate.Combine(uIModel32.onBtnMentalResistanceHoveredOut, new Action(p_listener.OnBtnUpgradeMentalResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel33 = UIModel;
		uIModel33.onBtnPhysicalResistanceHoveredOut = (Action)Delegate.Combine(uIModel33.onBtnPhysicalResistanceHoveredOut, new Action(p_listener.OnBtnUpgradePhysicalResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel34 = UIModel;
		uIModel34.onBtnElementalResistanceHoveredOut = (Action)Delegate.Combine(uIModel34.onBtnElementalResistanceHoveredOut, new Action(p_listener.OnBtnUpgradeElementalResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel35 = UIModel;
		uIModel35.onBtnSecondaryResistanceHoveredOut = (Action)Delegate.Combine(uIModel35.onBtnSecondaryResistanceHoveredOut, new Action(p_listener.OnBtnUpgradeSecondaryResistanceHoveredOut));
	}

	public void Unsubscribe(IListener p_listener)
	{
		PrimordialPoolStatsUpgradeUIModel uIModel = UIModel;
		uIModel.onStrengthUpgradeClicked = (Action)Delegate.Remove(uIModel.onStrengthUpgradeClicked, new Action(p_listener.OnStrengthUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel2 = UIModel;
		uIModel2.onIntelligenceUpgradeClicked = (Action)Delegate.Remove(uIModel2.onIntelligenceUpgradeClicked, new Action(p_listener.OnIntelligenceUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel3 = UIModel;
		uIModel3.onPiercingUpgradeClicked = (Action)Delegate.Remove(uIModel3.onPiercingUpgradeClicked, new Action(p_listener.OnPiercingUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel4 = UIModel;
		uIModel4.onMentalResistanceUpgradeClicked = (Action)Delegate.Remove(uIModel4.onMentalResistanceUpgradeClicked, new Action(p_listener.OnMentalResistanceUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel5 = UIModel;
		uIModel5.onPhysicalResistanceUpgradeClicked = (Action)Delegate.Remove(uIModel5.onPhysicalResistanceUpgradeClicked, new Action(p_listener.OnPhysicalResistanceUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel6 = UIModel;
		uIModel6.onElementalResistanceUpgradeClicked = (Action)Delegate.Remove(uIModel6.onElementalResistanceUpgradeClicked, new Action(p_listener.OnElementalResistanceUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel7 = UIModel;
		uIModel7.onSecondarylResistanceUpgradeClicked = (Action)Delegate.Remove(uIModel7.onSecondarylResistanceUpgradeClicked, new Action(p_listener.OnSecondaryResistanceUpgradeClicked));
		PrimordialPoolStatsUpgradeUIModel uIModel8 = UIModel;
		uIModel8.onStrengthHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel8.onStrengthHoveredOver, new Action<UIHoverPosition>(p_listener.OnStrengthHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel9 = UIModel;
		uIModel9.onIntelligenceHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel9.onIntelligenceHoveredOver, new Action<UIHoverPosition>(p_listener.OnIntelligenceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel10 = UIModel;
		uIModel10.onPiercingHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel10.onPiercingHoveredOver, new Action<UIHoverPosition>(p_listener.OnPiercingHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel11 = UIModel;
		uIModel11.onMentalResistanceHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel11.onMentalResistanceHoveredOver, new Action<UIHoverPosition>(p_listener.OnMentalResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel12 = UIModel;
		uIModel12.onPhysicalResistanceHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel12.onPhysicalResistanceHoveredOver, new Action<UIHoverPosition>(p_listener.OnPhysicalResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel13 = UIModel;
		uIModel13.onElementalresistanceHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel13.onElementalresistanceHoveredOver, new Action<UIHoverPosition>(p_listener.OnElementalResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel14 = UIModel;
		uIModel14.onSecondaryResistanceHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel14.onSecondaryResistanceHoveredOver, new Action<UIHoverPosition>(p_listener.OnSecondaryResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel15 = UIModel;
		uIModel15.onStrengthHoveredOut = (Action)Delegate.Remove(uIModel15.onStrengthHoveredOut, new Action(p_listener.OnStrengthHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel16 = UIModel;
		uIModel16.onIntelligenceHoveredOut = (Action)Delegate.Remove(uIModel16.onIntelligenceHoveredOut, new Action(p_listener.OnIntelligenceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel17 = UIModel;
		uIModel17.onPiercingHoveredOut = (Action)Delegate.Remove(uIModel17.onPiercingHoveredOut, new Action(p_listener.OnPiercingHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel18 = UIModel;
		uIModel18.onMentalResistanceHoveredOut = (Action)Delegate.Remove(uIModel18.onMentalResistanceHoveredOut, new Action(p_listener.OnMentalResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel19 = UIModel;
		uIModel19.onPhysicalResistanceHoveredOut = (Action)Delegate.Remove(uIModel19.onPhysicalResistanceHoveredOut, new Action(p_listener.OnPhysicalResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel20 = UIModel;
		uIModel20.onElementalResistanceHoveredOut = (Action)Delegate.Remove(uIModel20.onElementalResistanceHoveredOut, new Action(p_listener.OnElementalResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel21 = UIModel;
		uIModel21.onSecondaryResistanceHoveredOut = (Action)Delegate.Remove(uIModel21.onSecondaryResistanceHoveredOut, new Action(p_listener.OnSecondaryResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel22 = UIModel;
		uIModel22.onBtnStrengthHoveredOver = (Action)Delegate.Remove(uIModel22.onBtnStrengthHoveredOver, new Action(p_listener.OnBtnUpgradeStrengthHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel23 = UIModel;
		uIModel23.onBtnIntelligenceHoveredOver = (Action)Delegate.Remove(uIModel23.onBtnIntelligenceHoveredOver, new Action(p_listener.OnBtnUpgradeIntelligenceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel24 = UIModel;
		uIModel24.onBtnPiercingHoveredOver = (Action)Delegate.Remove(uIModel24.onBtnPiercingHoveredOver, new Action(p_listener.OnBtnUpgradePiercingHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel25 = UIModel;
		uIModel25.onBtnMentalResistanceHoveredOver = (Action)Delegate.Remove(uIModel25.onBtnMentalResistanceHoveredOver, new Action(p_listener.OnBtnUpgradeMentalResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel26 = UIModel;
		uIModel26.onBtnPhysicalResistanceHoveredOver = (Action)Delegate.Remove(uIModel26.onBtnPhysicalResistanceHoveredOver, new Action(p_listener.OnBtnUpgradePhysicalResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel27 = UIModel;
		uIModel27.onBtnElementalresistanceHoveredOver = (Action)Delegate.Remove(uIModel27.onBtnElementalresistanceHoveredOver, new Action(p_listener.OnBtnUpgradeElementalResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel28 = UIModel;
		uIModel28.onBtnSecondaryResistanceHoveredOver = (Action)Delegate.Remove(uIModel28.onBtnSecondaryResistanceHoveredOver, new Action(p_listener.OnBtnUpgradeSecondaryResistanceHoveredOver));
		PrimordialPoolStatsUpgradeUIModel uIModel29 = UIModel;
		uIModel29.onBtnStrengthHoveredOut = (Action)Delegate.Remove(uIModel29.onBtnStrengthHoveredOut, new Action(p_listener.OnBtnUpgradeStrengthHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel30 = UIModel;
		uIModel30.onBtnIntelligenceHoveredOut = (Action)Delegate.Remove(uIModel30.onBtnIntelligenceHoveredOut, new Action(p_listener.OnBtnUpgradeIntelligenceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel31 = UIModel;
		uIModel31.onBtnPiercingHoveredOut = (Action)Delegate.Remove(uIModel31.onBtnPiercingHoveredOut, new Action(p_listener.OnBtnUpgradePiercingHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel32 = UIModel;
		uIModel32.onBtnMentalResistanceHoveredOut = (Action)Delegate.Remove(uIModel32.onBtnMentalResistanceHoveredOut, new Action(p_listener.OnBtnUpgradeMentalResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel33 = UIModel;
		uIModel33.onBtnPhysicalResistanceHoveredOut = (Action)Delegate.Remove(uIModel33.onBtnPhysicalResistanceHoveredOut, new Action(p_listener.OnBtnUpgradePhysicalResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel34 = UIModel;
		uIModel34.onBtnElementalResistanceHoveredOut = (Action)Delegate.Remove(uIModel34.onBtnElementalResistanceHoveredOut, new Action(p_listener.OnBtnUpgradeElementalResistanceHoveredOut));
		PrimordialPoolStatsUpgradeUIModel uIModel35 = UIModel;
		uIModel35.onBtnSecondaryResistanceHoveredOut = (Action)Delegate.Remove(uIModel35.onBtnSecondaryResistanceHoveredOut, new Action(p_listener.OnBtnUpgradeSecondaryResistanceHoveredOut));
	}
}
