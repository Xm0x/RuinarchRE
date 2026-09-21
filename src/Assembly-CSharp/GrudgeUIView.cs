using System;
using DG.Tweening;
using Ruinarch.MVCFramework;
using UnityEngine;

public class GrudgeUIView : MVCUIView
{
	public interface IListener
	{
		void OnCloseClicked();
	}

	private Sequence _showSequence;

	private Sequence _itemsSequence;

	public GrudgeUIModel UIModel => _baseAssetModel as GrudgeUIModel;

	public static void Create(Canvas p_canvas, GrudgeUIModel p_assets, Action<GrudgeUIView> p_onCreate)
	{
		GrudgeUIView grudgeUIView = new GameObject(typeof(GrudgeUIView).ToString()).AddComponent<GrudgeUIView>();
		GrudgeUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		grudgeUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(grudgeUIView);
	}

	public Transform GetContainerParent()
	{
		return UIModel.containerParent;
	}

	public void Subscribe(IListener p_listener)
	{
		GrudgeUIModel uIModel = UIModel;
		uIModel.onCloseClicked = (Action)Delegate.Combine(uIModel.onCloseClicked, new Action(p_listener.OnCloseClicked));
	}

	public void Unsubscribe(IListener p_listener)
	{
		GrudgeUIModel uIModel = UIModel;
		uIModel.onCloseClicked = (Action)Delegate.Remove(uIModel.onCloseClicked, new Action(p_listener.OnCloseClicked));
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
		for (int i = 0; i < UIModel.items.Count; i++)
		{
			GrudgeItemUI grudgeItemUI = UIModel.items[i];
			_showSequence.Join(grudgeItemUI.PrepareAnimation().SetDelay((float)i / 5f));
		}
		_showSequence.Play();
	}

	public void PlayItemsAnimation()
	{
		_itemsSequence?.Kill();
		_itemsSequence = DOTween.Sequence();
		for (int i = 0; i < UIModel.items.Count; i++)
		{
			GrudgeItemUI grudgeItemUI = UIModel.items[i];
			_itemsSequence.Join(grudgeItemUI.PrepareAnimation().SetDelay((float)i / 5f));
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
