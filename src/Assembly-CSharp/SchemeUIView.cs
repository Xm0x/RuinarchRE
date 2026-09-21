using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class SchemeUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickClose();

		void OnClickConfirm();

		void OnClickBlackmail();

		void OnClickTemptation();

		void OnHoverOverBlackmailBtn(UIHoverPosition p_hoverPos);

		void OnHoverOverTemptBtn(UIHoverPosition p_hoverPos);

		void OnHoverOverSuccessRate();

		void OnHoverOutBlackmailBtn();

		void OnHoverOutTemptBtn();

		void OnHoverOutSuccessRate();
	}

	public SchemeUIModel UIModel => _baseAssetModel as SchemeUIModel;

	public static void Create(Canvas p_canvas, SchemeUIModel p_assets, Action<SchemeUIView> p_onCreate)
	{
		SchemeUIView schemeUIView = new GameObject(typeof(SchemeUIView).ToString()).AddComponent<SchemeUIView>();
		SchemeUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		schemeUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(schemeUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		SchemeUIModel uIModel = UIModel;
		uIModel.onCloseClicked = (Action)Delegate.Combine(uIModel.onCloseClicked, new Action(p_listener.OnClickClose));
		SchemeUIModel uIModel2 = UIModel;
		uIModel2.onClickConfirm = (Action)Delegate.Combine(uIModel2.onClickConfirm, new Action(p_listener.OnClickConfirm));
		SchemeUIModel uIModel3 = UIModel;
		uIModel3.onClickBlackmail = (Action)Delegate.Combine(uIModel3.onClickBlackmail, new Action(p_listener.OnClickBlackmail));
		SchemeUIModel uIModel4 = UIModel;
		uIModel4.onClickTemptation = (Action)Delegate.Combine(uIModel4.onClickTemptation, new Action(p_listener.OnClickTemptation));
		SchemeUIModel uIModel5 = UIModel;
		uIModel5.onHoverOverBlackmailBtn = (Action<UIHoverPosition>)Delegate.Combine(uIModel5.onHoverOverBlackmailBtn, new Action<UIHoverPosition>(p_listener.OnHoverOverBlackmailBtn));
		SchemeUIModel uIModel6 = UIModel;
		uIModel6.onHoverOutBlackmailBtn = (Action)Delegate.Combine(uIModel6.onHoverOutBlackmailBtn, new Action(p_listener.OnHoverOutBlackmailBtn));
		SchemeUIModel uIModel7 = UIModel;
		uIModel7.onHoverOverTemptBtn = (Action<UIHoverPosition>)Delegate.Combine(uIModel7.onHoverOverTemptBtn, new Action<UIHoverPosition>(p_listener.OnHoverOverTemptBtn));
		SchemeUIModel uIModel8 = UIModel;
		uIModel8.onHoverOutTemptBtn = (Action)Delegate.Combine(uIModel8.onHoverOutTemptBtn, new Action(p_listener.OnHoverOutTemptBtn));
		SchemeUIModel uIModel9 = UIModel;
		uIModel9.onHoverOverSuccessRate = (Action)Delegate.Combine(uIModel9.onHoverOverSuccessRate, new Action(p_listener.OnHoverOverSuccessRate));
		SchemeUIModel uIModel10 = UIModel;
		uIModel10.onHoverOutSuccessRate = (Action)Delegate.Combine(uIModel10.onHoverOutSuccessRate, new Action(p_listener.OnHoverOutSuccessRate));
	}

	public void Unsubscribe(IListener p_listener)
	{
		SchemeUIModel uIModel = UIModel;
		uIModel.onCloseClicked = (Action)Delegate.Remove(uIModel.onCloseClicked, new Action(p_listener.OnClickClose));
		SchemeUIModel uIModel2 = UIModel;
		uIModel2.onClickConfirm = (Action)Delegate.Remove(uIModel2.onClickConfirm, new Action(p_listener.OnClickConfirm));
		SchemeUIModel uIModel3 = UIModel;
		uIModel3.onClickBlackmail = (Action)Delegate.Remove(uIModel3.onClickBlackmail, new Action(p_listener.OnClickBlackmail));
		SchemeUIModel uIModel4 = UIModel;
		uIModel4.onClickTemptation = (Action)Delegate.Remove(uIModel4.onClickTemptation, new Action(p_listener.OnClickTemptation));
		SchemeUIModel uIModel5 = UIModel;
		uIModel5.onHoverOverBlackmailBtn = (Action<UIHoverPosition>)Delegate.Remove(uIModel5.onHoverOverBlackmailBtn, new Action<UIHoverPosition>(p_listener.OnHoverOverBlackmailBtn));
		SchemeUIModel uIModel6 = UIModel;
		uIModel6.onHoverOutBlackmailBtn = (Action)Delegate.Remove(uIModel6.onHoverOutBlackmailBtn, new Action(p_listener.OnHoverOutBlackmailBtn));
		SchemeUIModel uIModel7 = UIModel;
		uIModel7.onHoverOverTemptBtn = (Action<UIHoverPosition>)Delegate.Remove(uIModel7.onHoverOverTemptBtn, new Action<UIHoverPosition>(p_listener.OnHoverOverTemptBtn));
		SchemeUIModel uIModel8 = UIModel;
		uIModel8.onHoverOutTemptBtn = (Action)Delegate.Remove(uIModel8.onHoverOutTemptBtn, new Action(p_listener.OnHoverOutTemptBtn));
		SchemeUIModel uIModel9 = UIModel;
		uIModel9.onHoverOverSuccessRate = (Action)Delegate.Remove(uIModel9.onHoverOverSuccessRate, new Action(p_listener.OnHoverOverSuccessRate));
		SchemeUIModel uIModel10 = UIModel;
		uIModel10.onHoverOutSuccessRate = (Action)Delegate.Remove(uIModel10.onHoverOutSuccessRate, new Action(p_listener.OnHoverOutSuccessRate));
	}

	public void SetSuccessRate(string p_successRate)
	{
		UIModel.txtSuccessRate.text = p_successRate;
	}

	public void SetTitle(string p_title)
	{
		UIModel.txtTitle.text = p_title;
	}

	public void SetBlackmailBtnInteractableState(bool p_state)
	{
		UIModel.btnBlackmail.interactable = p_state;
	}

	public void SetTemptBtnInteractableState(bool p_state)
	{
		UIModel.btnTemptation.interactable = p_state;
	}
}
