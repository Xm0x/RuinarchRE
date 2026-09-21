using System;
using System.Collections.Generic;

public class BookmarkCategory
{
	public interface IListener
	{
		void OnBookmarkAdded(IBookmarkable p_bookmarkable);

		void OnBookmarkCategoryEmptiedOut(BookmarkCategory p_category);
	}

	private Action<IBookmarkable> _onBookmarkAdded;

	private Action<BookmarkCategory> _onBookmarkCategoryEmptiedOut;

	public string displayName { get; }

	public BOOKMARK_CATEGORY bookmarkCategory { get; }

	public List<IBookmarkable> bookmarked { get; }

	public BookmarkCategory(BOOKMARK_CATEGORY p_category)
	{
		bookmarkCategory = p_category;
		bookmarked = new List<IBookmarkable>();
		displayName = p_category.LocalizedText();
	}

	public void AddBookmark(IBookmarkable p_bookmarkable)
	{
		bookmarked.Add(p_bookmarkable);
		ExecuteBookmarkAdded(p_bookmarkable);
	}

	public bool RemoveBookmark(IBookmarkable p_bookmarkable)
	{
		if (bookmarked.Remove(p_bookmarkable))
		{
			p_bookmarkable.bookmarkEventDispatcher.ExecuteBookmarkRemovedEvent(p_bookmarkable);
			if (IsEmpty())
			{
				ExecuteBookmarkEmptiedOut(this);
			}
			return true;
		}
		return false;
	}

	private bool IsEmpty()
	{
		return bookmarked.Count == 0;
	}

	public void SubscribeToEvents(IListener p_listener)
	{
		_onBookmarkAdded = (Action<IBookmarkable>)Delegate.Combine(_onBookmarkAdded, new Action<IBookmarkable>(p_listener.OnBookmarkAdded));
		_onBookmarkCategoryEmptiedOut = (Action<BookmarkCategory>)Delegate.Combine(_onBookmarkCategoryEmptiedOut, new Action<BookmarkCategory>(p_listener.OnBookmarkCategoryEmptiedOut));
	}

	public void UnsubscribeToEvents(IListener p_listener)
	{
		_onBookmarkAdded = (Action<IBookmarkable>)Delegate.Remove(_onBookmarkAdded, new Action<IBookmarkable>(p_listener.OnBookmarkAdded));
		_onBookmarkCategoryEmptiedOut = (Action<BookmarkCategory>)Delegate.Remove(_onBookmarkCategoryEmptiedOut, new Action<BookmarkCategory>(p_listener.OnBookmarkCategoryEmptiedOut));
	}

	private void ExecuteBookmarkAdded(IBookmarkable p_bookmarkable)
	{
		_onBookmarkAdded?.Invoke(p_bookmarkable);
	}

	private void ExecuteBookmarkEmptiedOut(BookmarkCategory p_bookmarkCategory)
	{
		_onBookmarkCategoryEmptiedOut?.Invoke(p_bookmarkCategory);
	}
}
