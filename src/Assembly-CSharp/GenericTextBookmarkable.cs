using System;

public class GenericTextBookmarkable : IBookmarkable
{
	private Func<string> _nameGetter;

	private Func<BOOKMARK_TYPE> _bookmarkTypeGetter;

	private Action _onSelectAction;

	private Action _removeBookmarkAction;

	private Action<UIHoverPosition> _onHoverOverAction;

	private Action _onHoverOutAction;

	public BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

	public string bookmarkName => _nameGetter?.Invoke() ?? string.Empty;

	public BOOKMARK_TYPE bookmarkType => _bookmarkTypeGetter?.Invoke() ?? BOOKMARK_TYPE.Text;

	public GenericTextBookmarkable(Func<string> p_nameGetter, Func<BOOKMARK_TYPE> p_bookmarkTypeGetter, Action p_onSelectAction, Action p_removeBookmarkAction, Action<UIHoverPosition> p_onHoverOverAction, Action p_onHoverOutAction)
	{
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		_nameGetter = p_nameGetter;
		_bookmarkTypeGetter = p_bookmarkTypeGetter;
		_onSelectAction = p_onSelectAction;
		_removeBookmarkAction = p_removeBookmarkAction;
		_onHoverOverAction = p_onHoverOverAction;
		_onHoverOutAction = p_onHoverOutAction;
	}

	public void OnSelectBookmark()
	{
		_onSelectAction?.Invoke();
	}

	public void RemoveBookmark()
	{
		_removeBookmarkAction?.Invoke();
	}

	public void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		_onHoverOverAction?.Invoke(p_pos);
	}

	public void OnHoverOutBookmarkItem()
	{
		_onHoverOutAction?.Invoke();
	}
}
