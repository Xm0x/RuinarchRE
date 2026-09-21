using System;
using DG.Tweening;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookmarkTextItemUI : PooledObject, BookmarkableEventDispatcher.IListener
{
	[SerializeField]
	private TextMeshProUGUI lblName;

	[SerializeField]
	private Button btnMain;

	[SerializeField]
	private Button btnRemove;

	[SerializeField]
	private HoverHandler hoverHandler;

	[SerializeField]
	private UIHoverPosition hoverPosition;

	[SerializeField]
	private EnvelopContentUnityUI envelopContent;

	[SerializeField]
	private RectTransform animationContentParent;

	[SerializeField]
	private RectTransform glowEffect;

	private Action _onHoverOverAction;

	private Action _onHoverOutAction;

	private Vector2 defaultGlowSize;

	public void SetBookmark(IBookmarkable p_bookmarkable)
	{
		SetBookmarkItemText(p_bookmarkable.bookmarkName);
		p_bookmarkable.bookmarkEventDispatcher.Subscribe(this, p_bookmarkable);
		btnMain.onClick.AddListener(p_bookmarkable.OnSelectBookmark);
		btnRemove.onClick.AddListener(delegate
		{
			OnClickRemoveBookmark(p_bookmarkable);
		});
		btnRemove.gameObject.SetActive(p_bookmarkable.bookmarkType == BOOKMARK_TYPE.Text_With_Cancel || p_bookmarkable.bookmarkType == BOOKMARK_TYPE.Special);
		_onHoverOverAction = delegate
		{
			p_bookmarkable.OnHoverOverBookmarkItem(hoverPosition);
		};
		_onHoverOutAction = p_bookmarkable.OnHoverOutBookmarkItem;
		hoverHandler.AddOnHoverOverAction(OnHoverOverBookmark);
		hoverHandler.AddOnHoverOutAction(OnHoverOutBookmark);
		defaultGlowSize = new Vector2(361f, 99f);
	}

	public override void Reset()
	{
		base.Reset();
		_onHoverOverAction = null;
		_onHoverOutAction = null;
		hoverHandler.RemoveOnHoverOutAction(OnHoverOverBookmark);
		hoverHandler.RemoveOnHoverOverAction(OnHoverOutBookmark);
		btnMain.onClick.RemoveAllListeners();
		btnRemove.onClick.RemoveAllListeners();
		if (glowEffect != null)
		{
			glowEffect.gameObject.SetActive(value: false);
		}
	}

	private void OnClickRemoveBookmark(IBookmarkable p_bookmarkable)
	{
		p_bookmarkable.RemoveBookmark();
		if (p_bookmarkable is IStoredTarget p_target)
		{
			PlayerManager.Instance.player.storedTargetsComponent.Remove(p_target);
		}
	}

	public void OnBookmarkRemoved(IBookmarkable p_bookmarkable)
	{
		Action onHoverOutAction = _onHoverOutAction;
		RectTransform rectTransform = null;
		RectTransform rectTransform2 = null;
		if (base.transform.parent is RectTransform rectTransform3)
		{
			rectTransform = rectTransform3;
			rectTransform2 = base.transform.parent.parent.parent as RectTransform;
		}
		p_bookmarkable.bookmarkEventDispatcher.Unsubscribe(this, p_bookmarkable);
		ObjectPoolManager.Instance.DestroyObject(this);
		onHoverOutAction?.Invoke();
		if ((bool)rectTransform)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}
		if ((bool)rectTransform2)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform2);
		}
	}

	public void OnBookmarkChangedName(IBookmarkable p_bookmarkable)
	{
		SetBookmarkItemText(p_bookmarkable.bookmarkName);
	}

	private void SetBookmarkItemText(string p_text)
	{
		lblName.text = p_text;
		envelopContent.Execute();
		if (base.transform is RectTransform rect)
		{
			LayoutRebuilder.MarkLayoutForRebuild(rect);
		}
	}

	private void OnHoverOverBookmark()
	{
		_onHoverOverAction?.Invoke();
	}

	private void OnHoverOutBookmark()
	{
		_onHoverOutAction?.Invoke();
	}

	[ContextMenu("Test Animation")]
	public void PlayShowAnimation()
	{
		animationContentParent.anchoredPosition = new Vector2(315f, 0f);
		glowEffect.sizeDelta = new Vector2(0f, 0f);
		glowEffect.gameObject.SetActive(value: true);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(animationContentParent.DOAnchorPosX(0f, 0.3f));
		sequence.Append(glowEffect.DOSizeDelta(defaultGlowSize, 1f).SetLoops(2, LoopType.Yoyo));
		sequence.Play();
	}
}
