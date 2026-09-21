using System;

namespace Quests;

public abstract class VictoryCondition : IBookmarkable
{
	public VICTORY_CONDITION type { get; private set; }

	public BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

	public BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Text;

	public string bookmarkName => GetBookmarkName();

	public virtual Type serializedData => typeof(SaveDataVictoryCondition);

	protected VictoryCondition(VICTORY_CONDITION p_type)
	{
		type = p_type;
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
	}

	protected VictoryCondition(VICTORY_CONDITION p_type, SaveDataVictoryCondition p_data)
	{
		type = p_type;
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
	}

	public void Initialize()
	{
		SubscribeListeners();
	}

	public void LoadInitialize()
	{
		if (!PlayerManager.Instance.player.hasAlreadyWon)
		{
			SubscribeListeners();
		}
	}

	protected virtual void SubscribeListeners()
	{
	}

	protected virtual void UnsubscribeListeners()
	{
	}

	protected abstract string GetWinMessage();

	protected void WinGame()
	{
		if (!PlayerManager.Instance.player.hasAlreadyWon)
		{
			string winMessage = GetWinMessage();
			Messenger.Broadcast(PlayerSignals.WIN_GAME, winMessage);
			UnsubscribeListeners();
			RemoveBookmark();
			AfterWinGame();
		}
	}

	protected virtual void AfterWinGame()
	{
	}

	public virtual void OnSelectBookmark()
	{
	}

	public virtual void RemoveBookmark()
	{
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this, BOOKMARK_CATEGORY.Win_Condition);
	}

	public virtual void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
	}

	public virtual void OnHoverOutBookmarkItem()
	{
	}

	protected abstract string GetBookmarkName();
}
