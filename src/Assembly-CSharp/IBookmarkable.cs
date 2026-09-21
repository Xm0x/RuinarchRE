public interface IBookmarkable
{
	string bookmarkName { get; }

	BOOKMARK_TYPE bookmarkType { get; }

	BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

	void OnSelectBookmark();

	void RemoveBookmark();

	void OnHoverOverBookmarkItem(UIHoverPosition p_pos);

	void OnHoverOutBookmarkItem();
}
