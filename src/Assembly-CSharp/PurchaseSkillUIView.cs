using System;
using DG.Tweening;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class PurchaseSkillUIView : MVCUIView
{
	public interface IListener
	{
		void OnRerollClicked();

		void OnCloseClicked();

		void OnHoverOverReroll();

		void OnHoverOutReroll();

		void OnClickCancelReleaseAbility();

		void OnHoverOverCancelReleaseAbility();

		void OnHoverOutCancelReleaseAbility();
	}

	private Sequence _showSequence;

	private Sequence _itemsSequence;

	public PurchaseSkillUIModel UIModel => _baseAssetModel as PurchaseSkillUIModel;

	public static void Create(Canvas p_canvas, PurchaseSkillUIModel p_assets, Action<PurchaseSkillUIView> p_onCreate)
	{
		PurchaseSkillUIView purchaseSkillUIView = new GameObject(typeof(PurchaseSkillUIView).ToString()).AddComponent<PurchaseSkillUIView>();
		PurchaseSkillUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		purchaseSkillUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(purchaseSkillUIView);
	}

	public void DisableRerollButton()
	{
		UIModel.btnReroll.interactable = false;
	}

	public void EnableRerollButton()
	{
		UIModel.btnReroll.interactable = true;
	}

	public void ShowSkills()
	{
		UIModel.skillsParent.gameObject.SetActive(value: true);
		UIModel.txtMessageDisplay.gameObject.SetActive(value: false);
	}

	public void HideSkills()
	{
		UIModel.skillsParent.gameObject.SetActive(value: false);
		UIModel.txtMessageDisplay.gameObject.SetActive(value: true);
	}

	public Transform GetSkillsParent()
	{
		return UIModel.skillsParent;
	}

	public void SetMessage(string p_message)
	{
		UIModel.txtMessageDisplay.text = p_message;
	}

	public void SetRerollCooldownFill(float p_fill)
	{
		UIModel.imgCooldown.fillAmount = p_fill;
	}

	public void SetWindowCoverState(bool p_state)
	{
	}

	public void SetTimerState(bool p_state)
	{
		UIModel.goReleaseAbilityTimer.SetActive(p_state);
		if (p_state)
		{
			UIModel.timerReleaseAbility.RefreshName();
		}
	}

	public void SetCurrentChaoticEnergyText(int p_amount)
	{
		UIModel.lblChaoticEnergy.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Current Chaotic Energy") + ": " + Utilities.ChaoticEnergyIcon() + p_amount;
	}

	public void Subscribe(IListener p_listener)
	{
		PurchaseSkillUIModel uIModel = UIModel;
		uIModel.onCloseClicked = (Action)Delegate.Combine(uIModel.onCloseClicked, new Action(p_listener.OnCloseClicked));
		PurchaseSkillUIModel uIModel2 = UIModel;
		uIModel2.onRerollClicked = (Action)Delegate.Combine(uIModel2.onRerollClicked, new Action(p_listener.OnRerollClicked));
		PurchaseSkillUIModel uIModel3 = UIModel;
		uIModel3.onHoverOverReroll = (Action)Delegate.Combine(uIModel3.onHoverOverReroll, new Action(p_listener.OnHoverOverReroll));
		PurchaseSkillUIModel uIModel4 = UIModel;
		uIModel4.onHoverOutReroll = (Action)Delegate.Combine(uIModel4.onHoverOutReroll, new Action(p_listener.OnHoverOutReroll));
		PurchaseSkillUIModel uIModel5 = UIModel;
		uIModel5.onClickCancelReleaseAbility = (Action)Delegate.Combine(uIModel5.onClickCancelReleaseAbility, new Action(p_listener.OnClickCancelReleaseAbility));
		PurchaseSkillUIModel uIModel6 = UIModel;
		uIModel6.onHoverOverCancelReleaseAbility = (Action)Delegate.Combine(uIModel6.onHoverOverCancelReleaseAbility, new Action(p_listener.OnHoverOverCancelReleaseAbility));
		PurchaseSkillUIModel uIModel7 = UIModel;
		uIModel7.onHoverOutCancelReleaseAbility = (Action)Delegate.Combine(uIModel7.onHoverOutCancelReleaseAbility, new Action(p_listener.OnHoverOutCancelReleaseAbility));
	}

	public void Unsubscribe(IListener p_listener)
	{
		PurchaseSkillUIModel uIModel = UIModel;
		uIModel.onCloseClicked = (Action)Delegate.Remove(uIModel.onCloseClicked, new Action(p_listener.OnCloseClicked));
		PurchaseSkillUIModel uIModel2 = UIModel;
		uIModel2.onRerollClicked = (Action)Delegate.Remove(uIModel2.onRerollClicked, new Action(p_listener.OnRerollClicked));
		PurchaseSkillUIModel uIModel3 = UIModel;
		uIModel3.onHoverOverReroll = (Action)Delegate.Remove(uIModel3.onHoverOverReroll, new Action(p_listener.OnHoverOverReroll));
		PurchaseSkillUIModel uIModel4 = UIModel;
		uIModel4.onHoverOutReroll = (Action)Delegate.Remove(uIModel4.onHoverOutReroll, new Action(p_listener.OnHoverOutReroll));
		PurchaseSkillUIModel uIModel5 = UIModel;
		uIModel5.onClickCancelReleaseAbility = (Action)Delegate.Remove(uIModel5.onClickCancelReleaseAbility, new Action(p_listener.OnClickCancelReleaseAbility));
		PurchaseSkillUIModel uIModel6 = UIModel;
		uIModel6.onHoverOverCancelReleaseAbility = (Action)Delegate.Remove(uIModel6.onHoverOverCancelReleaseAbility, new Action(p_listener.OnHoverOverCancelReleaseAbility));
		PurchaseSkillUIModel uIModel7 = UIModel;
		uIModel7.onHoverOutCancelReleaseAbility = (Action)Delegate.Remove(uIModel7.onHoverOutCancelReleaseAbility, new Action(p_listener.OnHoverOutCancelReleaseAbility));
	}

	public void PlayShowAnimation()
	{
		UIModel.canvasGroupCover.alpha = 0f;
		UIModel.canvasGroupMainWindow.alpha = 0f;
		UIModel.canvasGroupFrame.alpha = 1f;
		Vector2 anchoredPosition = UIModel.rectTransformMainWindow.anchoredPosition;
		UIModel.rectTransformMainWindow.anchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y - 100f);
		Vector2 defaultFrameSize = UIModel.defaultFrameSize;
		UIModel.rectTransformFrame.sizeDelta = new Vector2(defaultFrameSize.x + 500f, defaultFrameSize.y);
		UIModel.canvasGroupFrameGlow.alpha = 0f;
		UIModel.canvasGroupFrameGlow.DOKill();
		UIModel.canvasGroupFrameGlow.DOFade(1f, 2f).SetEase(Ease.OutQuart).SetLoops(-1, LoopType.Yoyo);
		_showSequence = DOTween.Sequence();
		_showSequence.Append(UIModel.canvasGroupMainWindow.DOFade(1f, 0.5f));
		_showSequence.Join(UIModel.canvasGroupCover.DOFade(1f, 0.5f));
		_showSequence.Join(UIModel.rectTransformMainWindow.DOAnchorPos(anchoredPosition, 0.5f));
		_showSequence.Join(UIModel.rectTransformFrame.DOSizeDelta(defaultFrameSize, 0.8f));
		_showSequence.AppendInterval(0.02f);
		for (int i = 0; i < UIModel.skillItems.Count; i++)
		{
			PurchaseSkillItemUI purchaseSkillItemUI = UIModel.skillItems[i];
			_showSequence.Join(purchaseSkillItemUI.PrepareAnimation().SetDelay((float)i / 5f));
		}
		_showSequence.Play();
	}

	public void PlayItemsAnimation()
	{
		_itemsSequence?.Kill();
		_itemsSequence = DOTween.Sequence();
		for (int i = 0; i < UIModel.skillItems.Count; i++)
		{
			PurchaseSkillItemUI purchaseSkillItemUI = UIModel.skillItems[i];
			_itemsSequence.Join(purchaseSkillItemUI.PrepareAnimation().SetDelay((float)i / 5f));
		}
		_itemsSequence.Play();
	}

	public void PlayHideAnimation(Action onComplete)
	{
		_showSequence?.Kill(complete: true);
		_showSequence = null;
		Sequence sequence = DOTween.Sequence();
		Vector2 defaultFrameSize = UIModel.defaultFrameSize;
		defaultFrameSize.x += 1000f;
		sequence.Append(UIModel.canvasGroupMainWindow.DOFade(0f, 0.5f));
		sequence.Join(UIModel.canvasGroupCover.DOFade(0f, 0.5f));
		sequence.Join(UIModel.rectTransformFrame.DOSizeDelta(defaultFrameSize, 0.7f));
		sequence.Join(UIModel.canvasGroupFrame.DOFade(0f, 0.6f));
		sequence.OnComplete(delegate
		{
			onComplete();
		});
		sequence.Play();
	}
}
