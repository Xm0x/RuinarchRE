using System;
using UnityEngine;

namespace UtilityScripts;

[Serializable]
public abstract class RuinarchProgressable : IBookmarkable
{
	public interface IListener
	{
		void OnCurrentProgressChanged(RuinarchProgressable p_progressable);
	}

	public int minValue;

	public int maxValue;

	public int currentValue;

	public int totalValue;

	private Action<RuinarchProgressable> _onCurrentProgressChanged;

	private Action _onSelectProgressable;

	private Action<UIHoverPosition> _onHoverOverProgressable;

	private Action _onHoverOutProgressable;

	public abstract string progressableName { get; }

	public abstract BOOKMARK_TYPE bookmarkType { get; }

	public BookmarkableEventDispatcher bookmarkEventDispatcher { get; private set; }

	public string bookmarkName => progressableName;

	protected RuinarchProgressable()
	{
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
	}

	public void Load()
	{
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
	}

	protected void Setup(int p_minValue, int p_maxValue)
	{
		minValue = p_minValue;
		maxValue = p_maxValue;
		totalValue = p_maxValue - p_minValue;
		currentValue = minValue;
	}

	protected void Reset()
	{
		minValue = 0;
		maxValue = 0;
		currentValue = 0;
		totalValue = 0;
	}

	public void IncreaseProgress(int p_amount)
	{
		currentValue += p_amount;
		currentValue = Mathf.Clamp(currentValue, 0, maxValue);
		ExecuteOnProgressChangedEvent();
	}

	public void SetProgress(int p_amount)
	{
		currentValue = p_amount;
		currentValue = Mathf.Clamp(currentValue, 0, maxValue);
		ExecuteOnProgressChangedEvent();
	}

	public float GetCurrentProgressPercent()
	{
		float num = (float)currentValue / (float)totalValue;
		if (float.IsNaN(num))
		{
			num = 0f;
		}
		return num;
	}

	public bool IsComplete()
	{
		return currentValue >= maxValue;
	}

	public void ListenToProgress(IListener p_listener)
	{
		_onCurrentProgressChanged = (Action<RuinarchProgressable>)Delegate.Combine(_onCurrentProgressChanged, new Action<RuinarchProgressable>(p_listener.OnCurrentProgressChanged));
	}

	public void StopListeningToProgress(IListener p_listener)
	{
		_onCurrentProgressChanged = (Action<RuinarchProgressable>)Delegate.Remove(_onCurrentProgressChanged, new Action<RuinarchProgressable>(p_listener.OnCurrentProgressChanged));
	}

	private void ExecuteOnProgressChangedEvent()
	{
		_onCurrentProgressChanged?.Invoke(this);
	}

	public void SetOnSelectAction(Action p_action)
	{
		_onSelectProgressable = p_action;
	}

	public void SetOnHoverOverAction(Action<UIHoverPosition> p_action)
	{
		_onHoverOverProgressable = p_action;
	}

	public void SetOnHoverOutAction(Action p_action)
	{
		_onHoverOutProgressable = p_action;
	}

	public void OnSelect()
	{
		_onSelectProgressable?.Invoke();
	}

	public void OnSelectBookmark()
	{
		OnSelect();
	}

	public void RemoveBookmark()
	{
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this);
	}

	public void OnHoverOverBookmarkItem(UIHoverPosition pos)
	{
		_onHoverOverProgressable?.Invoke(pos);
	}

	public void OnHoverOutBookmarkItem()
	{
		_onHoverOutProgressable?.Invoke();
	}
}
