using System;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class TransmissionUIView : MVCUIView
{
	public interface IListener
	{
		void OnAirBorneUpgradeClicked();

		void OnConsumptionUpgradeClicked();

		void OnPhysicalContactUpgradeClicked();

		void OnCombatUpgradeClicked();

		void OnAirBorneHoveredOver(UIHoverPosition p_hoverPosition);

		void OnConsumptionHoveredOver(UIHoverPosition p_hoverPosition);

		void OnPhysicalContactHoveredOver(UIHoverPosition p_hoverPosition);

		void OnCombatHoveredOver(UIHoverPosition p_hoverPosition);

		void OnAirBorneHoveredOut();

		void OnConsumptionHoveredOut();

		void OnPhysicalContactHoveredOut();

		void OnCombatHoveredOut();
	}

	public TransmissionUIModel UIModel => _baseAssetModel as TransmissionUIModel;

	public static void Create(Canvas p_canvas, TransmissionUIModel p_assets, Action<TransmissionUIView> p_onCreate)
	{
		TransmissionUIView transmissionUIView = new GameObject(typeof(TransmissionUIView).ToString()).AddComponent<TransmissionUIView>();
		TransmissionUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		transmissionUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(transmissionUIView);
	}

	private RuinarchText GetTransmissionCostText(PLAGUE_TRANSMISSION p_transmissionType)
	{
		return p_transmissionType switch
		{
			PLAGUE_TRANSMISSION.Airborne => UIModel.txtAirBorneCost, 
			PLAGUE_TRANSMISSION.Consumption => UIModel.txtConsumptionCost, 
			PLAGUE_TRANSMISSION.Physical_Contact => UIModel.txtDirectContactCost, 
			PLAGUE_TRANSMISSION.Combat => UIModel.txtCombatUpgradeCost, 
			_ => throw new ArgumentOutOfRangeException("p_transmissionType", p_transmissionType, null), 
		};
	}

	private RuinarchText GetTransmissionRateText(PLAGUE_TRANSMISSION p_transmissionType)
	{
		return p_transmissionType switch
		{
			PLAGUE_TRANSMISSION.Airborne => UIModel.txtAirBorneRate, 
			PLAGUE_TRANSMISSION.Consumption => UIModel.txtConsumptionRate, 
			PLAGUE_TRANSMISSION.Physical_Contact => UIModel.txtDirectContactRate, 
			PLAGUE_TRANSMISSION.Combat => UIModel.txtCombatRate, 
			_ => throw new ArgumentOutOfRangeException("p_transmissionType", p_transmissionType, null), 
		};
	}

	private Button GetTransmissionUpgradeButton(PLAGUE_TRANSMISSION p_transmissionType)
	{
		return p_transmissionType switch
		{
			PLAGUE_TRANSMISSION.Airborne => UIModel.btnAirBorneUpgrade, 
			PLAGUE_TRANSMISSION.Consumption => UIModel.btnConsumptionUpgrade, 
			PLAGUE_TRANSMISSION.Physical_Contact => UIModel.btnDirectContactUpgrade, 
			PLAGUE_TRANSMISSION.Combat => UIModel.btnCombatUpgrade, 
			_ => throw new ArgumentOutOfRangeException("p_transmissionType", p_transmissionType, null), 
		};
	}

	private TextMeshProUGUI GetTransmissionUpgradeText(PLAGUE_TRANSMISSION p_transmissionType)
	{
		return p_transmissionType switch
		{
			PLAGUE_TRANSMISSION.Airborne => UIModel.txtAirBorneUpgrade, 
			PLAGUE_TRANSMISSION.Consumption => UIModel.txtConsumptionUpgrade, 
			PLAGUE_TRANSMISSION.Physical_Contact => UIModel.txtDirectContactUpgrade, 
			PLAGUE_TRANSMISSION.Combat => UIModel.txtCombatUpgrade, 
			_ => throw new ArgumentOutOfRangeException("p_transmissionType", p_transmissionType, null), 
		};
	}

	public void UpdateTransmissionCost(PLAGUE_TRANSMISSION p_transmissionType, string p_newCost)
	{
		GetTransmissionCostText(p_transmissionType).text = p_newCost;
	}

	public void UpdateTransmissionRate(PLAGUE_TRANSMISSION p_transmissionType, string p_newRate)
	{
		GetTransmissionRateText(p_transmissionType).text = p_newRate;
	}

	public void UpdateTransmissionUpgradeButtonInteractable(PLAGUE_TRANSMISSION p_transmissionType, bool p_interactable)
	{
		GetTransmissionUpgradeButton(p_transmissionType).interactable = p_interactable;
		GetTransmissionUpgradeText(p_transmissionType).color = GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}

	public void Subscribe(IListener p_listener)
	{
		TransmissionUIModel uIModel = UIModel;
		uIModel.onAirBorneUpgradeClicked = (Action)Delegate.Combine(uIModel.onAirBorneUpgradeClicked, new Action(p_listener.OnAirBorneUpgradeClicked));
		TransmissionUIModel uIModel2 = UIModel;
		uIModel2.onConsumptionUpgradeClicked = (Action)Delegate.Combine(uIModel2.onConsumptionUpgradeClicked, new Action(p_listener.OnConsumptionUpgradeClicked));
		TransmissionUIModel uIModel3 = UIModel;
		uIModel3.onDirectContactUpgradeClicked = (Action)Delegate.Combine(uIModel3.onDirectContactUpgradeClicked, new Action(p_listener.OnPhysicalContactUpgradeClicked));
		TransmissionUIModel uIModel4 = UIModel;
		uIModel4.onCombatUpgradeClicked = (Action)Delegate.Combine(uIModel4.onCombatUpgradeClicked, new Action(p_listener.OnCombatUpgradeClicked));
		TransmissionUIModel uIModel5 = UIModel;
		uIModel5.onAirBorneHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel5.onAirBorneHoveredOver, new Action<UIHoverPosition>(p_listener.OnAirBorneHoveredOver));
		TransmissionUIModel uIModel6 = UIModel;
		uIModel6.onConsumptionHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel6.onConsumptionHoveredOver, new Action<UIHoverPosition>(p_listener.OnConsumptionHoveredOver));
		TransmissionUIModel uIModel7 = UIModel;
		uIModel7.onDirectContactHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel7.onDirectContactHoveredOver, new Action<UIHoverPosition>(p_listener.OnPhysicalContactHoveredOver));
		TransmissionUIModel uIModel8 = UIModel;
		uIModel8.onCombatHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel8.onCombatHoveredOver, new Action<UIHoverPosition>(p_listener.OnCombatHoveredOver));
		TransmissionUIModel uIModel9 = UIModel;
		uIModel9.onAirBorneHoveredOut = (Action)Delegate.Combine(uIModel9.onAirBorneHoveredOut, new Action(p_listener.OnAirBorneHoveredOut));
		TransmissionUIModel uIModel10 = UIModel;
		uIModel10.onConsumptionHoveredOut = (Action)Delegate.Combine(uIModel10.onConsumptionHoveredOut, new Action(p_listener.OnConsumptionHoveredOut));
		TransmissionUIModel uIModel11 = UIModel;
		uIModel11.onDirectContactHoveredOut = (Action)Delegate.Combine(uIModel11.onDirectContactHoveredOut, new Action(p_listener.OnPhysicalContactHoveredOut));
		TransmissionUIModel uIModel12 = UIModel;
		uIModel12.onCombatHoveredOut = (Action)Delegate.Combine(uIModel12.onCombatHoveredOut, new Action(p_listener.OnCombatHoveredOut));
	}

	public void Unsubscribe(IListener p_listener)
	{
		TransmissionUIModel uIModel = UIModel;
		uIModel.onAirBorneUpgradeClicked = (Action)Delegate.Remove(uIModel.onAirBorneUpgradeClicked, new Action(p_listener.OnAirBorneUpgradeClicked));
		TransmissionUIModel uIModel2 = UIModel;
		uIModel2.onConsumptionUpgradeClicked = (Action)Delegate.Remove(uIModel2.onConsumptionUpgradeClicked, new Action(p_listener.OnConsumptionUpgradeClicked));
		TransmissionUIModel uIModel3 = UIModel;
		uIModel3.onDirectContactUpgradeClicked = (Action)Delegate.Remove(uIModel3.onDirectContactUpgradeClicked, new Action(p_listener.OnPhysicalContactUpgradeClicked));
		TransmissionUIModel uIModel4 = UIModel;
		uIModel4.onCombatUpgradeClicked = (Action)Delegate.Remove(uIModel4.onCombatUpgradeClicked, new Action(p_listener.OnCombatUpgradeClicked));
		TransmissionUIModel uIModel5 = UIModel;
		uIModel5.onAirBorneHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel5.onAirBorneHoveredOver, new Action<UIHoverPosition>(p_listener.OnAirBorneHoveredOver));
		TransmissionUIModel uIModel6 = UIModel;
		uIModel6.onConsumptionHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel6.onConsumptionHoveredOver, new Action<UIHoverPosition>(p_listener.OnConsumptionHoveredOver));
		TransmissionUIModel uIModel7 = UIModel;
		uIModel7.onDirectContactHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel7.onDirectContactHoveredOver, new Action<UIHoverPosition>(p_listener.OnPhysicalContactHoveredOver));
		TransmissionUIModel uIModel8 = UIModel;
		uIModel8.onCombatHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel8.onCombatHoveredOver, new Action<UIHoverPosition>(p_listener.OnCombatHoveredOver));
		TransmissionUIModel uIModel9 = UIModel;
		uIModel9.onAirBorneHoveredOut = (Action)Delegate.Remove(uIModel9.onAirBorneHoveredOut, new Action(p_listener.OnAirBorneHoveredOut));
		TransmissionUIModel uIModel10 = UIModel;
		uIModel10.onConsumptionHoveredOut = (Action)Delegate.Remove(uIModel10.onConsumptionHoveredOut, new Action(p_listener.OnConsumptionHoveredOut));
		TransmissionUIModel uIModel11 = UIModel;
		uIModel11.onDirectContactHoveredOut = (Action)Delegate.Remove(uIModel11.onDirectContactHoveredOut, new Action(p_listener.OnPhysicalContactHoveredOut));
		TransmissionUIModel uIModel12 = UIModel;
		uIModel12.onCombatHoveredOut = (Action)Delegate.Remove(uIModel12.onCombatHoveredOut, new Action(p_listener.OnCombatHoveredOut));
	}
}
