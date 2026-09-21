using System;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class LifeSpanUIView : MVCUIView
{
	public interface IListener
	{
		void OnObjectsUpgradeClicked();

		void OnElvesUpgradeClicked();

		void OnHumansUpgradeClicked();

		void OnMonstersUpgradeClicked();

		void OnUndeadUpgradeClicked();

		void OnObjectsHoveredOver(UIHoverPosition hoverPosition);

		void OnElvesHoveredOver(UIHoverPosition hoverPosition);

		void OnHumansHoveredOver(UIHoverPosition hoverPosition);

		void OnMonstersHoveredOver(UIHoverPosition hoverPosition);

		void OnUndeadHoveredOver(UIHoverPosition hoverPosition);

		void OnObjectsHoveredOut();

		void OnElvesHoveredOut();

		void OnHumansHoveredOut();

		void OnMonstersHoveredOut();

		void OnUndeadHoveredOut();

		void OnUpgradeBtnObjectsHoveredOver();

		void OnUpgradeBtnElvesHoveredOver();

		void OnUpgradeBtnHumansHoveredOver();

		void OnUpgradeBtnMonstersHoveredOver();

		void OnUpgradeBtnUndeadHoveredOver();

		void OnUpgradeBtnObjectsHoveredOut();

		void OnUpgradeBtnElvesHoveredOut();

		void OnUpgradeBtnHumansHoveredOut();

		void OnUpgradeBtnMonstersHoveredOut();

		void OnUpgradeBtnUndeadHoveredOut();
	}

	public LifeSpanUIModel UIModel => _baseAssetModel as LifeSpanUIModel;

	public static void Create(Canvas p_canvas, LifeSpanUIModel p_assets, Action<LifeSpanUIView> p_onCreate)
	{
		LifeSpanUIView lifeSpanUIView = new GameObject(typeof(LifeSpanUIView).ToString()).AddComponent<LifeSpanUIView>();
		LifeSpanUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		lifeSpanUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(lifeSpanUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		LifeSpanUIModel uIModel = UIModel;
		uIModel.onObjectsUpgradeClicked = (Action)Delegate.Combine(uIModel.onObjectsUpgradeClicked, new Action(p_listener.OnObjectsUpgradeClicked));
		LifeSpanUIModel uIModel2 = UIModel;
		uIModel2.onElvesUpgradeClicked = (Action)Delegate.Combine(uIModel2.onElvesUpgradeClicked, new Action(p_listener.OnElvesUpgradeClicked));
		LifeSpanUIModel uIModel3 = UIModel;
		uIModel3.onHumansUpgradeClicked = (Action)Delegate.Combine(uIModel3.onHumansUpgradeClicked, new Action(p_listener.OnHumansUpgradeClicked));
		LifeSpanUIModel uIModel4 = UIModel;
		uIModel4.onMonstersUpgradeClicked = (Action)Delegate.Combine(uIModel4.onMonstersUpgradeClicked, new Action(p_listener.OnMonstersUpgradeClicked));
		LifeSpanUIModel uIModel5 = UIModel;
		uIModel5.onUndeadUpgradeClicked = (Action)Delegate.Combine(uIModel5.onUndeadUpgradeClicked, new Action(p_listener.OnUndeadUpgradeClicked));
		LifeSpanUIModel uIModel6 = UIModel;
		uIModel6.onObjectsHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel6.onObjectsHoveredOver, new Action<UIHoverPosition>(p_listener.OnObjectsHoveredOver));
		LifeSpanUIModel uIModel7 = UIModel;
		uIModel7.onElvesHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel7.onElvesHoveredOver, new Action<UIHoverPosition>(p_listener.OnElvesHoveredOver));
		LifeSpanUIModel uIModel8 = UIModel;
		uIModel8.onHumansHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel8.onHumansHoveredOver, new Action<UIHoverPosition>(p_listener.OnHumansHoveredOver));
		LifeSpanUIModel uIModel9 = UIModel;
		uIModel9.onMonstersHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel9.onMonstersHoveredOver, new Action<UIHoverPosition>(p_listener.OnMonstersHoveredOver));
		LifeSpanUIModel uIModel10 = UIModel;
		uIModel10.onUndeadHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel10.onUndeadHoveredOver, new Action<UIHoverPosition>(p_listener.OnUndeadHoveredOver));
		LifeSpanUIModel uIModel11 = UIModel;
		uIModel11.onObjectsHoveredOut = (Action)Delegate.Combine(uIModel11.onObjectsHoveredOut, new Action(p_listener.OnObjectsHoveredOut));
		LifeSpanUIModel uIModel12 = UIModel;
		uIModel12.onElvesHoveredOut = (Action)Delegate.Combine(uIModel12.onElvesHoveredOut, new Action(p_listener.OnElvesHoveredOut));
		LifeSpanUIModel uIModel13 = UIModel;
		uIModel13.onHumansHoveredOut = (Action)Delegate.Combine(uIModel13.onHumansHoveredOut, new Action(p_listener.OnHumansHoveredOut));
		LifeSpanUIModel uIModel14 = UIModel;
		uIModel14.onMonstersHoveredOut = (Action)Delegate.Combine(uIModel14.onMonstersHoveredOut, new Action(p_listener.OnMonstersHoveredOut));
		LifeSpanUIModel uIModel15 = UIModel;
		uIModel15.onUndeadHoveredOut = (Action)Delegate.Combine(uIModel15.onUndeadHoveredOut, new Action(p_listener.OnUndeadHoveredOut));
		LifeSpanUIModel uIModel16 = UIModel;
		uIModel16.onObjectsUpgradeBtnHoveredOver = (Action)Delegate.Combine(uIModel16.onObjectsUpgradeBtnHoveredOver, new Action(p_listener.OnUpgradeBtnObjectsHoveredOver));
		LifeSpanUIModel uIModel17 = UIModel;
		uIModel17.onElvesUpgradeBtnHoveredOver = (Action)Delegate.Combine(uIModel17.onElvesUpgradeBtnHoveredOver, new Action(p_listener.OnUpgradeBtnElvesHoveredOver));
		LifeSpanUIModel uIModel18 = UIModel;
		uIModel18.onHumansUpgradeBtnHoveredOver = (Action)Delegate.Combine(uIModel18.onHumansUpgradeBtnHoveredOver, new Action(p_listener.OnUpgradeBtnHumansHoveredOver));
		LifeSpanUIModel uIModel19 = UIModel;
		uIModel19.onMonstersUpgradeBtnHoveredOver = (Action)Delegate.Combine(uIModel19.onMonstersUpgradeBtnHoveredOver, new Action(p_listener.OnUpgradeBtnMonstersHoveredOver));
		LifeSpanUIModel uIModel20 = UIModel;
		uIModel20.onUndeadUpgradeBtnHoveredOver = (Action)Delegate.Combine(uIModel20.onUndeadUpgradeBtnHoveredOver, new Action(p_listener.OnUpgradeBtnUndeadHoveredOver));
		LifeSpanUIModel uIModel21 = UIModel;
		uIModel21.onObjectsUpgradeBtnHoveredOut = (Action)Delegate.Combine(uIModel21.onObjectsUpgradeBtnHoveredOut, new Action(p_listener.OnUpgradeBtnObjectsHoveredOut));
		LifeSpanUIModel uIModel22 = UIModel;
		uIModel22.onElvesUpgradeBtnHoveredOut = (Action)Delegate.Combine(uIModel22.onElvesUpgradeBtnHoveredOut, new Action(p_listener.OnUpgradeBtnElvesHoveredOut));
		LifeSpanUIModel uIModel23 = UIModel;
		uIModel23.onHumansUpgradeBtnHoveredOut = (Action)Delegate.Combine(uIModel23.onHumansUpgradeBtnHoveredOut, new Action(p_listener.OnUpgradeBtnHumansHoveredOut));
		LifeSpanUIModel uIModel24 = UIModel;
		uIModel24.onMonstersUpgradeBtnHoveredOut = (Action)Delegate.Combine(uIModel24.onMonstersUpgradeBtnHoveredOut, new Action(p_listener.OnUpgradeBtnMonstersHoveredOut));
		LifeSpanUIModel uIModel25 = UIModel;
		uIModel25.onUndeadUpgradeBtnHoveredOut = (Action)Delegate.Combine(uIModel25.onUndeadUpgradeBtnHoveredOut, new Action(p_listener.OnUpgradeBtnUndeadHoveredOut));
	}

	public void Unsubscribe(IListener p_listener)
	{
		LifeSpanUIModel uIModel = UIModel;
		uIModel.onObjectsUpgradeClicked = (Action)Delegate.Remove(uIModel.onObjectsUpgradeClicked, new Action(p_listener.OnObjectsUpgradeClicked));
		LifeSpanUIModel uIModel2 = UIModel;
		uIModel2.onElvesUpgradeClicked = (Action)Delegate.Remove(uIModel2.onElvesUpgradeClicked, new Action(p_listener.OnElvesUpgradeClicked));
		LifeSpanUIModel uIModel3 = UIModel;
		uIModel3.onHumansUpgradeClicked = (Action)Delegate.Remove(uIModel3.onHumansUpgradeClicked, new Action(p_listener.OnHumansUpgradeClicked));
		LifeSpanUIModel uIModel4 = UIModel;
		uIModel4.onMonstersUpgradeClicked = (Action)Delegate.Remove(uIModel4.onMonstersUpgradeClicked, new Action(p_listener.OnMonstersUpgradeClicked));
		LifeSpanUIModel uIModel5 = UIModel;
		uIModel5.onUndeadUpgradeClicked = (Action)Delegate.Remove(uIModel5.onUndeadUpgradeClicked, new Action(p_listener.OnUndeadUpgradeClicked));
		LifeSpanUIModel uIModel6 = UIModel;
		uIModel6.onObjectsHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel6.onObjectsHoveredOver, new Action<UIHoverPosition>(p_listener.OnObjectsHoveredOver));
		LifeSpanUIModel uIModel7 = UIModel;
		uIModel7.onElvesHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel7.onElvesHoveredOver, new Action<UIHoverPosition>(p_listener.OnElvesHoveredOver));
		LifeSpanUIModel uIModel8 = UIModel;
		uIModel8.onHumansHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel8.onHumansHoveredOver, new Action<UIHoverPosition>(p_listener.OnHumansHoveredOver));
		LifeSpanUIModel uIModel9 = UIModel;
		uIModel9.onMonstersHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel9.onMonstersHoveredOver, new Action<UIHoverPosition>(p_listener.OnMonstersHoveredOver));
		LifeSpanUIModel uIModel10 = UIModel;
		uIModel10.onUndeadHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel10.onUndeadHoveredOver, new Action<UIHoverPosition>(p_listener.OnUndeadHoveredOver));
		LifeSpanUIModel uIModel11 = UIModel;
		uIModel11.onObjectsHoveredOut = (Action)Delegate.Remove(uIModel11.onObjectsHoveredOut, new Action(p_listener.OnObjectsHoveredOut));
		LifeSpanUIModel uIModel12 = UIModel;
		uIModel12.onElvesHoveredOut = (Action)Delegate.Remove(uIModel12.onElvesHoveredOut, new Action(p_listener.OnElvesHoveredOut));
		LifeSpanUIModel uIModel13 = UIModel;
		uIModel13.onHumansHoveredOut = (Action)Delegate.Remove(uIModel13.onHumansHoveredOut, new Action(p_listener.OnHumansHoveredOut));
		LifeSpanUIModel uIModel14 = UIModel;
		uIModel14.onMonstersHoveredOut = (Action)Delegate.Remove(uIModel14.onMonstersHoveredOut, new Action(p_listener.OnMonstersHoveredOut));
		LifeSpanUIModel uIModel15 = UIModel;
		uIModel15.onUndeadHoveredOut = (Action)Delegate.Remove(uIModel15.onUndeadHoveredOut, new Action(p_listener.OnUndeadHoveredOut));
		LifeSpanUIModel uIModel16 = UIModel;
		uIModel16.onObjectsUpgradeBtnHoveredOver = (Action)Delegate.Remove(uIModel16.onObjectsUpgradeBtnHoveredOver, new Action(p_listener.OnUpgradeBtnObjectsHoveredOver));
		LifeSpanUIModel uIModel17 = UIModel;
		uIModel17.onElvesUpgradeBtnHoveredOver = (Action)Delegate.Remove(uIModel17.onElvesUpgradeBtnHoveredOver, new Action(p_listener.OnUpgradeBtnElvesHoveredOver));
		LifeSpanUIModel uIModel18 = UIModel;
		uIModel18.onHumansUpgradeBtnHoveredOver = (Action)Delegate.Remove(uIModel18.onHumansUpgradeBtnHoveredOver, new Action(p_listener.OnUpgradeBtnHumansHoveredOver));
		LifeSpanUIModel uIModel19 = UIModel;
		uIModel19.onMonstersUpgradeBtnHoveredOver = (Action)Delegate.Remove(uIModel19.onMonstersUpgradeBtnHoveredOver, new Action(p_listener.OnUpgradeBtnMonstersHoveredOver));
		LifeSpanUIModel uIModel20 = UIModel;
		uIModel20.onUndeadUpgradeBtnHoveredOver = (Action)Delegate.Remove(uIModel20.onUndeadUpgradeBtnHoveredOver, new Action(p_listener.OnUpgradeBtnUndeadHoveredOver));
		LifeSpanUIModel uIModel21 = UIModel;
		uIModel21.onObjectsUpgradeBtnHoveredOut = (Action)Delegate.Remove(uIModel21.onObjectsUpgradeBtnHoveredOut, new Action(p_listener.OnUpgradeBtnObjectsHoveredOut));
		LifeSpanUIModel uIModel22 = UIModel;
		uIModel22.onElvesUpgradeBtnHoveredOut = (Action)Delegate.Remove(uIModel22.onElvesUpgradeBtnHoveredOut, new Action(p_listener.OnUpgradeBtnElvesHoveredOut));
		LifeSpanUIModel uIModel23 = UIModel;
		uIModel23.onHumansUpgradeBtnHoveredOut = (Action)Delegate.Remove(uIModel23.onHumansUpgradeBtnHoveredOut, new Action(p_listener.OnUpgradeBtnHumansHoveredOut));
		LifeSpanUIModel uIModel24 = UIModel;
		uIModel24.onMonstersUpgradeBtnHoveredOut = (Action)Delegate.Remove(uIModel24.onMonstersUpgradeBtnHoveredOut, new Action(p_listener.OnUpgradeBtnMonstersHoveredOut));
		LifeSpanUIModel uIModel25 = UIModel;
		uIModel25.onUndeadUpgradeBtnHoveredOut = (Action)Delegate.Remove(uIModel25.onUndeadUpgradeBtnHoveredOut, new Action(p_listener.OnUpgradeBtnUndeadHoveredOut));
	}

	public void UpdateTileObjectUpgradePrice(string p_newPrice)
	{
		UIModel.txtTileObjectCost.text = p_newPrice;
	}

	public void UpdateTileObjectInfectionTime(string p_rate)
	{
		UIModel.txtTileObjectInfectionTime.text = p_rate;
	}

	public void UpdateTileObjectUpgradeButtonInteractable(bool p_interactable)
	{
		UIModel.btnObjectsUpgrade.interactable = p_interactable;
		UIModel.txtObjectsUpgrade.color = GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}

	public void UpdateElvesUpgradePrice(string p_newPrice)
	{
		UIModel.txtElvesCost.text = p_newPrice;
	}

	public void UpdateElvesInfectionTime(string p_rate)
	{
		UIModel.txtElvesInfectionTime.text = p_rate;
	}

	public void UpdateElvesUpgradeButtonInteractable(bool p_interactable)
	{
		UIModel.btnElvesUpgrade.interactable = p_interactable;
		UIModel.txtElvesUpgrade.color = GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}

	public void UpdateHumansUpgradePrice(string p_newPrice)
	{
		UIModel.txtHumansCost.text = p_newPrice;
	}

	public void UpdateHumansInfectionTime(string p_rate)
	{
		UIModel.txtHumansInfectionTime.text = p_rate;
	}

	public void UpdateHumansUpgradeButtonInteractable(bool p_interactable)
	{
		UIModel.btnHumansUpgrade.interactable = p_interactable;
		UIModel.txtHumansUpgrade.color = GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}

	public void UpdateMonstersUpgradePrice(string p_newPrice)
	{
		UIModel.txtMonstersCost.text = p_newPrice;
	}

	public void UpdateMonstersInfectionTime(string p_rate)
	{
		UIModel.txtMonstersInfectionTime.text = p_rate;
	}

	public void UpdateMonstersUpgradeButtonInteractable(bool p_interactable)
	{
		UIModel.btnMonstersUpgrade.interactable = p_interactable;
		UIModel.txtMonstersUpgrade.color = GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}

	public void UpdateUndeadUpgradePrice(string p_newPrice)
	{
		UIModel.txtUndeadCost.text = p_newPrice;
	}

	public void UpdateUndeadInfectionTime(string p_rate)
	{
		UIModel.txtUndeadInfectionTime.text = p_rate;
	}

	public void UpdateUndeadUpgradeButtonInteractable(bool p_interactable)
	{
		UIModel.btnUndeadUpgrade.interactable = p_interactable;
		UIModel.txtUndeadUpgrade.color = GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}
}
