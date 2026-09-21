using System;
using Inner_Maps.Location_Structures;

public class BookmarkableEventDispatcher
{
	public interface IListener
	{
		void OnBookmarkRemoved(IBookmarkable p_bookmarkable);

		void OnBookmarkChangedName(IBookmarkable p_bookmarkable);
	}

	private Action<IBookmarkable> _onBookmarkRemoved;

	private Action<IBookmarkable> _onBookmarkableChangedNameOrElements;

	public void Subscribe(IListener p_listener, IBookmarkable p_subscribeTo)
	{
		_onBookmarkRemoved = (Action<IBookmarkable>)Delegate.Combine(_onBookmarkRemoved, new Action<IBookmarkable>(p_listener.OnBookmarkRemoved));
		_onBookmarkableChangedNameOrElements = (Action<IBookmarkable>)Delegate.Combine(_onBookmarkableChangedNameOrElements, new Action<IBookmarkable>(p_listener.OnBookmarkChangedName));
	}

	public void Unsubscribe(IListener p_listener, IBookmarkable p_unsubscribeFrom)
	{
		_onBookmarkRemoved = (Action<IBookmarkable>)Delegate.Remove(_onBookmarkRemoved, new Action<IBookmarkable>(p_listener.OnBookmarkRemoved));
		_onBookmarkableChangedNameOrElements = (Action<IBookmarkable>)Delegate.Remove(_onBookmarkableChangedNameOrElements, new Action<IBookmarkable>(p_listener.OnBookmarkChangedName));
	}

	public void ExecuteBookmarkRemovedEvent(IBookmarkable p_bookmarkable)
	{
		_onBookmarkRemoved?.Invoke(p_bookmarkable);
	}

	public void ExecuteBookmarkChangedNameOrElementsEvent(IBookmarkable p_bookmarkable)
	{
		_onBookmarkableChangedNameOrElements?.Invoke(p_bookmarkable);
	}

	public void ClearAll()
	{
		_onBookmarkRemoved = null;
		_onBookmarkableChangedNameOrElements = null;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
