using System;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class FatalityUIView : MVCUIView
{
	public interface IListener
	{
		void OnSepticShockUpgradeClicked();

		void OnHeartAttackUpgradeClicked();

		void OnStrokeUpgradeClicked();

		void OnTotalOrganFailureUpgradeClicked();

		void OnPneumoniaUpgradeClicked();

		void OnSepticShockHoveredOver(UIHoverPosition hoverPosition);

		void OnSepticShockHoveredOut();

		void OnHeartAttackHoveredOver(UIHoverPosition hoverPosition);

		void OnHeartAttackHoveredOut();

		void OnStrokeHoveredOver(UIHoverPosition hoverPosition);

		void OnStrokeHoveredOut();

		void OnTotalOrganFailureHoveredOver(UIHoverPosition hoverPosition);

		void OnTotalOrganFailureHoveredOut();

		void OnPneumoniaHoveredOver(UIHoverPosition hoverPosition);

		void OnPneumoniaHoveredOut();
	}

	public FatalityUIModel UIModel => _baseAssetModel as FatalityUIModel;

	public static void Create(Canvas p_canvas, FatalityUIModel p_assets, Action<FatalityUIView> p_onCreate)
	{
		FatalityUIView fatalityUIView = new GameObject(typeof(FatalityUIView).ToString()).AddComponent<FatalityUIView>();
		FatalityUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		fatalityUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(fatalityUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		FatalityUIModel uIModel = UIModel;
		uIModel.onSepticShockUpgradeClicked = (Action)Delegate.Combine(uIModel.onSepticShockUpgradeClicked, new Action(p_listener.OnSepticShockUpgradeClicked));
		FatalityUIModel uIModel2 = UIModel;
		uIModel2.onHeartAttackUpgradeClicked = (Action)Delegate.Combine(uIModel2.onHeartAttackUpgradeClicked, new Action(p_listener.OnHeartAttackUpgradeClicked));
		FatalityUIModel uIModel3 = UIModel;
		uIModel3.onStrokeUpgradeClicked = (Action)Delegate.Combine(uIModel3.onStrokeUpgradeClicked, new Action(p_listener.OnStrokeUpgradeClicked));
		FatalityUIModel uIModel4 = UIModel;
		uIModel4.onTotalOrganFailureUpgradeClicked = (Action)Delegate.Combine(uIModel4.onTotalOrganFailureUpgradeClicked, new Action(p_listener.OnTotalOrganFailureUpgradeClicked));
		FatalityUIModel uIModel5 = UIModel;
		uIModel5.onPneumoniaUpgradeClicked = (Action)Delegate.Combine(uIModel5.onPneumoniaUpgradeClicked, new Action(p_listener.OnPneumoniaUpgradeClicked));
		FatalityUIModel uIModel6 = UIModel;
		uIModel6.onSepticShockHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel6.onSepticShockHoveredOver, new Action<UIHoverPosition>(p_listener.OnSepticShockHoveredOver));
		FatalityUIModel uIModel7 = UIModel;
		uIModel7.onSepticShockHoveredOut = (Action)Delegate.Combine(uIModel7.onSepticShockHoveredOut, new Action(p_listener.OnSepticShockHoveredOut));
		FatalityUIModel uIModel8 = UIModel;
		uIModel8.onHeartAttackHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel8.onHeartAttackHoveredOver, new Action<UIHoverPosition>(p_listener.OnHeartAttackHoveredOver));
		FatalityUIModel uIModel9 = UIModel;
		uIModel9.onHeartAttackHoveredOut = (Action)Delegate.Combine(uIModel9.onHeartAttackHoveredOut, new Action(p_listener.OnHeartAttackHoveredOut));
		FatalityUIModel uIModel10 = UIModel;
		uIModel10.onStrokeHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel10.onStrokeHoveredOver, new Action<UIHoverPosition>(p_listener.OnStrokeHoveredOver));
		FatalityUIModel uIModel11 = UIModel;
		uIModel11.onStrokeHoveredOut = (Action)Delegate.Combine(uIModel11.onStrokeHoveredOut, new Action(p_listener.OnStrokeHoveredOut));
		FatalityUIModel uIModel12 = UIModel;
		uIModel12.onTotalOrganFailureHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel12.onTotalOrganFailureHoveredOver, new Action<UIHoverPosition>(p_listener.OnTotalOrganFailureHoveredOver));
		FatalityUIModel uIModel13 = UIModel;
		uIModel13.onTotalOrganFailureHoveredOut = (Action)Delegate.Combine(uIModel13.onTotalOrganFailureHoveredOut, new Action(p_listener.OnTotalOrganFailureHoveredOut));
		FatalityUIModel uIModel14 = UIModel;
		uIModel14.onPneumoniaHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel14.onPneumoniaHoveredOver, new Action<UIHoverPosition>(p_listener.OnPneumoniaHoveredOver));
		FatalityUIModel uIModel15 = UIModel;
		uIModel15.onPneumoniaHoveredOut = (Action)Delegate.Combine(uIModel15.onPneumoniaHoveredOut, new Action(p_listener.OnPneumoniaHoveredOut));
	}

	public void Unsubscribe(IListener p_listener)
	{
		FatalityUIModel uIModel = UIModel;
		uIModel.onSepticShockUpgradeClicked = (Action)Delegate.Remove(uIModel.onSepticShockUpgradeClicked, new Action(p_listener.OnSepticShockUpgradeClicked));
		FatalityUIModel uIModel2 = UIModel;
		uIModel2.onHeartAttackUpgradeClicked = (Action)Delegate.Remove(uIModel2.onHeartAttackUpgradeClicked, new Action(p_listener.OnHeartAttackUpgradeClicked));
		FatalityUIModel uIModel3 = UIModel;
		uIModel3.onStrokeUpgradeClicked = (Action)Delegate.Remove(uIModel3.onStrokeUpgradeClicked, new Action(p_listener.OnStrokeUpgradeClicked));
		FatalityUIModel uIModel4 = UIModel;
		uIModel4.onTotalOrganFailureUpgradeClicked = (Action)Delegate.Remove(uIModel4.onTotalOrganFailureUpgradeClicked, new Action(p_listener.OnTotalOrganFailureUpgradeClicked));
		FatalityUIModel uIModel5 = UIModel;
		uIModel5.onPneumoniaUpgradeClicked = (Action)Delegate.Remove(uIModel5.onPneumoniaUpgradeClicked, new Action(p_listener.OnPneumoniaUpgradeClicked));
		FatalityUIModel uIModel6 = UIModel;
		uIModel6.onSepticShockHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel6.onSepticShockHoveredOver, new Action<UIHoverPosition>(p_listener.OnSepticShockHoveredOver));
		FatalityUIModel uIModel7 = UIModel;
		uIModel7.onSepticShockHoveredOut = (Action)Delegate.Remove(uIModel7.onSepticShockHoveredOut, new Action(p_listener.OnSepticShockHoveredOut));
		FatalityUIModel uIModel8 = UIModel;
		uIModel8.onHeartAttackHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel8.onHeartAttackHoveredOver, new Action<UIHoverPosition>(p_listener.OnHeartAttackHoveredOver));
		FatalityUIModel uIModel9 = UIModel;
		uIModel9.onHeartAttackHoveredOut = (Action)Delegate.Remove(uIModel9.onHeartAttackHoveredOut, new Action(p_listener.OnHeartAttackHoveredOut));
		FatalityUIModel uIModel10 = UIModel;
		uIModel10.onStrokeHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel10.onStrokeHoveredOver, new Action<UIHoverPosition>(p_listener.OnStrokeHoveredOver));
		FatalityUIModel uIModel11 = UIModel;
		uIModel11.onStrokeHoveredOut = (Action)Delegate.Remove(uIModel11.onStrokeHoveredOut, new Action(p_listener.OnStrokeHoveredOut));
		FatalityUIModel uIModel12 = UIModel;
		uIModel12.onTotalOrganFailureHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel12.onTotalOrganFailureHoveredOver, new Action<UIHoverPosition>(p_listener.OnTotalOrganFailureHoveredOver));
		FatalityUIModel uIModel13 = UIModel;
		uIModel13.onTotalOrganFailureHoveredOut = (Action)Delegate.Remove(uIModel13.onTotalOrganFailureHoveredOut, new Action(p_listener.OnTotalOrganFailureHoveredOut));
		FatalityUIModel uIModel14 = UIModel;
		uIModel14.onPneumoniaHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel14.onPneumoniaHoveredOver, new Action<UIHoverPosition>(p_listener.OnPneumoniaHoveredOver));
		FatalityUIModel uIModel15 = UIModel;
		uIModel15.onPneumoniaHoveredOut = (Action)Delegate.Remove(uIModel15.onPneumoniaHoveredOut, new Action(p_listener.OnPneumoniaHoveredOut));
	}

	public void UpdateSepticShockUpgradeButtonInteractable(bool p_interactable)
	{
		UIModel.btnSepticShockUpgrade.interactable = p_interactable;
		UIModel.txtSepticShockUpgrade.color = GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}

	public void UpdateSepticShockCostState(bool p_state)
	{
		UIModel.txtSepticShockCost.gameObject.SetActive(p_state);
	}

	public void UpdateSepticShockCost(string p_cost)
	{
		UIModel.txtSepticShockCost.text = p_cost;
	}

	public void UpdateSepticShockCheckmarkState(bool p_state)
	{
		UIModel.checkMarkSepticShockUpgrade.SetActive(p_state);
	}

	public void UpdateHeartAttackUpgradeButtonInteractable(bool p_interactable)
	{
		UIModel.btnHeartAttackUpgrade.interactable = p_interactable;
		UIModel.txtHeartAttackUpgrade.color = GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}

	public void UpdateHeartAttackCostState(bool p_state)
	{
		UIModel.txtHeartAttackCost.gameObject.SetActive(p_state);
	}

	public void UpdateHeartAttackCost(string p_cost)
	{
		UIModel.txtHeartAttackCost.text = p_cost;
	}

	public void UpdateHeartAttackCheckmarkState(bool p_state)
	{
		UIModel.checkMarkHeartAttackUpgrade.SetActive(p_state);
	}

	public void UpdateStrokeUpgradeButtonInteractable(bool p_interactable)
	{
		UIModel.btnStrokeUpgrade.interactable = p_interactable;
		UIModel.txtStrokeUpgrade.color = GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}

	public void UpdateStrokeCostState(bool p_state)
	{
		UIModel.txtStrokeCost.gameObject.SetActive(p_state);
	}

	public void UpdateStrokeCost(string p_cost)
	{
		UIModel.txtStrokeCost.text = p_cost;
	}

	public void UpdateStrokeCheckmarkState(bool p_state)
	{
		UIModel.checkMarkStrokeUpgrade.SetActive(p_state);
	}

	public void UpdateTotalOrganFailureUpgradeButtonInteractable(bool p_interactable)
	{
		UIModel.btnTotalOrganFailureUpgrade.interactable = p_interactable;
		UIModel.txtTotalOrganFailureUpgrade.color = GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}

	public void UpdateTotalOrganFailureCostState(bool p_state)
	{
		UIModel.txtTotalOrganFailureCost.gameObject.SetActive(p_state);
	}

	public void UpdateTotalOrganFailureCost(string p_cost)
	{
		UIModel.txtTotalOrganFailureCost.text = p_cost;
	}

	public void UpdateTotalOrganFailureCheckmarkState(bool p_state)
	{
		UIModel.checkMarkTotalOrganFailureUpgrade.SetActive(p_state);
	}

	public void UpdatePneumoniaUpgradeButtonInteractable(bool p_interactable)
	{
		UIModel.btnPneumoniaUpgrade.interactable = p_interactable;
		UIModel.txtPneumoniaUpgrade.color = GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}

	public void UpdatePneumoniaCostState(bool p_state)
	{
		UIModel.txtPneumoniaCost.gameObject.SetActive(p_state);
	}

	public void UpdatePneumoniaCost(string p_cost)
	{
		UIModel.txtPneumoniaCost.text = p_cost;
	}

	public void UpdatePneumoniaCheckmarkState(bool p_state)
	{
		UIModel.checkMarkPneumoniaUpgrade.SetActive(p_state);
	}
}
