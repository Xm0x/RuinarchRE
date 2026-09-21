using System;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class BookmarkProgressItemUI : PooledObject, RuinarchProgressable.IListener, BookmarkableEventDispatcher.IListener
{
	[SerializeField]
	private TextMeshProUGUI lblName;

	[SerializeField]
	private Slider sliderProgress;

	[SerializeField]
	private Button btnMain;

	[SerializeField]
	private HoverHandler hoverHandler;

	[SerializeField]
	private UIHoverPosition hoverPosition;

	private Action _onResetAction;

	public void SetProgressable(RuinarchProgressable p_progressable)
	{
		p_progressable.ListenToProgress(this);
		btnMain.onClick.AddListener(p_progressable.OnSelect);
		_onResetAction = (Action)Delegate.Combine(_onResetAction, (Action)delegate
		{
			OnResetActions(p_progressable);
		});
		lblName.text = p_progressable.progressableName;
		UpdateProgressBar(p_progressable);
		p_progressable.bookmarkEventDispatcher.Subscribe(this, p_progressable);
		hoverHandler.AddOnHoverOverAction(delegate
		{
			p_progressable.OnHoverOverBookmarkItem(hoverPosition);
		});
		hoverHandler.AddOnHoverOutAction(p_progressable.OnHoverOutBookmarkItem);
	}

	public void OnCurrentProgressChanged(RuinarchProgressable p_progressable)
	{
		UpdateProgressBar(p_progressable);
	}

	private void UpdateProgressBar(RuinarchProgressable p_progressable)
	{
		sliderProgress.value = p_progressable.GetCurrentProgressPercent();
	}

	private void OnResetActions(RuinarchProgressable p_progressable)
	{
		btnMain.onClick.RemoveListener(p_progressable.OnSelect);
		p_progressable.StopListeningToProgress(this);
	}

	public void OnBookmarkRemoved(IBookmarkable p_bookmarkable)
	{
		RectTransform rectTransform = null;
		RectTransform rectTransform2 = null;
		if (base.transform.parent is RectTransform rectTransform3)
		{
			rectTransform = rectTransform3;
			rectTransform2 = base.transform.parent.parent.parent as RectTransform;
		}
		p_bookmarkable.bookmarkEventDispatcher.Unsubscribe(this, p_bookmarkable);
		ObjectPoolManager.Instance.DestroyObject(this);
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
		if (p_bookmarkable is RuinarchProgressable ruinarchProgressable)
		{
			lblName.text = ruinarchProgressable.progressableName;
		}
	}

	public override void Reset()
	{
		base.Reset();
		_onResetAction?.Invoke();
		_onResetAction = null;
		btnMain.onClick.RemoveAllListeners();
		hoverHandler.ClearHoverActions();
	}
}
