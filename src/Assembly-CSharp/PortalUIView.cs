using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class PortalUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickReleaseAbility();

		void OnClickUpgradePortal();

		void OnClickCancelReleaseAbility();

		void OnClickCancelUpgradePortal();

		void OnHoverOverCancelReleaseAbility();

		void OnHoverOutCancelReleaseAbility();

		void OnHoverOverCancelUpgradePortal();

		void OnHoverOutCancelUpgradePortal();

		void OnHoverOverUpgradePortal();

		void OnHoverOutUpgradePortal();

		void OnClickClose();
	}

	public PortalUIModel UIModel => _baseAssetModel as PortalUIModel;

	public static void Create(Canvas p_canvas, PortalUIModel p_assets, Action<PortalUIView> p_onCreate)
	{
		PortalUIView portalUIView = new GameObject(typeof(PortalUIView).ToString()).AddComponent<PortalUIView>();
		PortalUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		portalUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(portalUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		PortalUIModel uIModel = UIModel;
		uIModel.onReleaseAbilityClicked = (Action)Delegate.Combine(uIModel.onReleaseAbilityClicked, new Action(p_listener.OnClickReleaseAbility));
		PortalUIModel uIModel2 = UIModel;
		uIModel2.onUpgradePortalClicked = (Action)Delegate.Combine(uIModel2.onUpgradePortalClicked, new Action(p_listener.OnClickUpgradePortal));
		PortalUIModel uIModel3 = UIModel;
		uIModel3.onCancelReleaseAbilityClicked = (Action)Delegate.Combine(uIModel3.onCancelReleaseAbilityClicked, new Action(p_listener.OnClickCancelReleaseAbility));
		PortalUIModel uIModel4 = UIModel;
		uIModel4.onCancelUpgradePortalClicked = (Action)Delegate.Combine(uIModel4.onCancelUpgradePortalClicked, new Action(p_listener.OnClickCancelUpgradePortal));
		PortalUIModel uIModel5 = UIModel;
		uIModel5.onHoverOverCancelReleaseAbility = (Action)Delegate.Combine(uIModel5.onHoverOverCancelReleaseAbility, new Action(p_listener.OnHoverOverCancelReleaseAbility));
		PortalUIModel uIModel6 = UIModel;
		uIModel6.onHoverOutCancelReleaseAbility = (Action)Delegate.Combine(uIModel6.onHoverOutCancelReleaseAbility, new Action(p_listener.OnHoverOutCancelReleaseAbility));
		PortalUIModel uIModel7 = UIModel;
		uIModel7.onHoverOverCancelUpgradePortal = (Action)Delegate.Combine(uIModel7.onHoverOverCancelUpgradePortal, new Action(p_listener.OnHoverOverCancelUpgradePortal));
		PortalUIModel uIModel8 = UIModel;
		uIModel8.onHoverOutCancelUpgradePortal = (Action)Delegate.Combine(uIModel8.onHoverOutCancelUpgradePortal, new Action(p_listener.OnHoverOutCancelUpgradePortal));
		PortalUIModel uIModel9 = UIModel;
		uIModel9.onHoverOverUpgradePortal = (Action)Delegate.Combine(uIModel9.onHoverOverUpgradePortal, new Action(p_listener.OnHoverOverUpgradePortal));
		PortalUIModel uIModel10 = UIModel;
		uIModel10.onHoverOutUpgradePortal = (Action)Delegate.Combine(uIModel10.onHoverOutUpgradePortal, new Action(p_listener.OnHoverOutUpgradePortal));
		PortalUIModel uIModel11 = UIModel;
		uIModel11.onClickClose = (Action)Delegate.Combine(uIModel11.onClickClose, new Action(p_listener.OnClickClose));
	}

	public void Unsubscribe(IListener p_listener)
	{
		PortalUIModel uIModel = UIModel;
		uIModel.onReleaseAbilityClicked = (Action)Delegate.Remove(uIModel.onReleaseAbilityClicked, new Action(p_listener.OnClickReleaseAbility));
		PortalUIModel uIModel2 = UIModel;
		uIModel2.onUpgradePortalClicked = (Action)Delegate.Remove(uIModel2.onUpgradePortalClicked, new Action(p_listener.OnClickUpgradePortal));
		PortalUIModel uIModel3 = UIModel;
		uIModel3.onCancelReleaseAbilityClicked = (Action)Delegate.Remove(uIModel3.onCancelReleaseAbilityClicked, new Action(p_listener.OnClickCancelReleaseAbility));
		PortalUIModel uIModel4 = UIModel;
		uIModel4.onCancelUpgradePortalClicked = (Action)Delegate.Remove(uIModel4.onCancelUpgradePortalClicked, new Action(p_listener.OnClickCancelUpgradePortal));
		PortalUIModel uIModel5 = UIModel;
		uIModel5.onHoverOverCancelReleaseAbility = (Action)Delegate.Remove(uIModel5.onHoverOverCancelReleaseAbility, new Action(p_listener.OnHoverOverCancelReleaseAbility));
		PortalUIModel uIModel6 = UIModel;
		uIModel6.onHoverOutCancelReleaseAbility = (Action)Delegate.Remove(uIModel6.onHoverOutCancelReleaseAbility, new Action(p_listener.OnHoverOutCancelReleaseAbility));
		PortalUIModel uIModel7 = UIModel;
		uIModel7.onHoverOverCancelUpgradePortal = (Action)Delegate.Remove(uIModel7.onHoverOverCancelUpgradePortal, new Action(p_listener.OnHoverOverCancelUpgradePortal));
		PortalUIModel uIModel8 = UIModel;
		uIModel8.onHoverOutCancelUpgradePortal = (Action)Delegate.Remove(uIModel8.onHoverOutCancelUpgradePortal, new Action(p_listener.OnHoverOutCancelUpgradePortal));
		PortalUIModel uIModel9 = UIModel;
		uIModel9.onHoverOverUpgradePortal = (Action)Delegate.Remove(uIModel9.onHoverOverUpgradePortal, new Action(p_listener.OnHoverOverUpgradePortal));
		PortalUIModel uIModel10 = UIModel;
		uIModel10.onHoverOutUpgradePortal = (Action)Delegate.Remove(uIModel10.onHoverOutUpgradePortal, new Action(p_listener.OnHoverOutUpgradePortal));
		PortalUIModel uIModel11 = UIModel;
		uIModel11.onClickClose = (Action)Delegate.Remove(uIModel11.onClickClose, new Action(p_listener.OnClickClose));
	}

	public void ShowUnlockAbilityTimerAndHideButton(SkillData p_skillToUnlock)
	{
		UIModel.timerReleaseAbility.RefreshName();
		UIModel.goTimerReleaseAbility.SetActive(value: true);
		UIModel.btnReleaseAbility.gameObject.SetActive(value: false);
	}

	public void ShowUnlockAbilityButtonAndHideTimer()
	{
		UIModel.goTimerReleaseAbility.SetActive(value: false);
		UIModel.btnReleaseAbility.gameObject.SetActive(value: true);
	}

	public void ShowUpgradePortalTimerAndHideButton()
	{
		UIModel.timerUpgradePortal.RefreshName();
		UIModel.goTimerUpgradePortal.SetActive(value: true);
		UIModel.btnUpgradePortal.gameObject.SetActive(value: false);
	}

	public void ShowUpgradePortalButtonAndHideTimer()
	{
		UIModel.goTimerUpgradePortal.SetActive(value: false);
		UIModel.btnUpgradePortal.gameObject.SetActive(value: true);
	}

	public void SetUpgradePortalBtnInteractable(bool p_state)
	{
		UIModel.btnUpgradePortal.interactable = p_state;
	}
}
