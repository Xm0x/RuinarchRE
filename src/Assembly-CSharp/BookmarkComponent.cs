using System.Collections.Generic;

public class BookmarkComponent
{
	public Dictionary<BOOKMARK_CATEGORY, BookmarkCategory> bookmarkedObjects { get; }

	public BookmarkComponent()
	{
		bookmarkedObjects = new Dictionary<BOOKMARK_CATEGORY, BookmarkCategory>();
	}

	public void SubscribeListeners()
	{
		Messenger.AddListener<Party>(PartySignals.PARTY_UNDEPLOYED, OnPartyUnDeployed);
		Messenger.AddListener<Party>(PartySignals.PARTY_QUEST_FINISHED_SUCCESSFULLY, PartyQuestFinishedSuccessfully);
		Messenger.AddListener<Party>(PartySignals.PARTY_QUEST_FAILED, PartyQuestFailed);
		Messenger.AddListener<Party>(PartySignals.DISBAND_PARTY, OnPartyDisbanded);
		Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_STORED_TARGET, OnPlayerStoredTarget);
		Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, OnPlayerRemovedTarget);
	}

	private void OnPlayerStoredTarget(IStoredTarget p_storedTarget)
	{
		AddBookmark(p_storedTarget, BOOKMARK_CATEGORY.Targets);
	}

	private void OnPlayerRemovedTarget(IStoredTarget p_storedTarget)
	{
		RemoveBookmark(p_storedTarget, BOOKMARK_CATEGORY.Targets);
	}

	private void OnPartyDisbanded(Party p_party)
	{
		RemoveBookmark(p_party, BOOKMARK_CATEGORY.Player_Parties);
	}

	private void OnPartyUnDeployed(Party p_party)
	{
		RemoveBookmark(p_party, BOOKMARK_CATEGORY.Player_Parties);
	}

	private void PartyQuestFinishedSuccessfully(Party p_party)
	{
		RemoveBookmark(p_party, BOOKMARK_CATEGORY.Player_Parties);
	}

	private void PartyQuestFailed(Party p_party)
	{
		RemoveBookmark(p_party, BOOKMARK_CATEGORY.Player_Parties);
	}

	public void AddBookmark(IBookmarkable p_bookmarkable, BOOKMARK_CATEGORY p_category)
	{
		if (!bookmarkedObjects.ContainsKey(p_category))
		{
			BookmarkCategory bookmarkCategory = new BookmarkCategory(p_category);
			bookmarkedObjects.Add(p_category, bookmarkCategory);
			Messenger.Broadcast(PlayerSignals.BOOKMARK_CATEGORY_ADDED, bookmarkCategory);
		}
		bookmarkedObjects[p_category].AddBookmark(p_bookmarkable);
	}

	public void RemoveBookmark(IBookmarkable p_bookmarkable, BOOKMARK_CATEGORY p_category)
	{
		if (bookmarkedObjects.ContainsKey(p_category))
		{
			bookmarkedObjects[p_category].RemoveBookmark(p_bookmarkable);
		}
	}

	public void RemoveBookmark(IBookmarkable p_bookmarkable)
	{
		foreach (KeyValuePair<BOOKMARK_CATEGORY, BookmarkCategory> bookmarkedObject in bookmarkedObjects)
		{
			bookmarkedObject.Value.RemoveBookmark(p_bookmarkable);
		}
	}
}
