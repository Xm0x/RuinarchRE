using System;
using UnityEngine.Localization;
using UtilityScripts;

public abstract class GoalTask : IBookmarkable, LocalizationManagerEventDispatcher.ILocaleChangeListener
{
	public string persistentID { get; }

	public string taskName { get; protected set; }

	public string taskTooltip { get; protected set; }

	public bool isComplete { get; private set; }

	public bool isPinned { get; private set; }

	public bool isCompletedAlertShowing { get; private set; }

	public GenericTextBookmarkable goalTaskCompletedBookmark { get; private set; }

	public BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

	public GoalTaskEventDispatcher eventDispatcher { get; }

	public abstract Type serializedData { get; }

	public string bookmarkName
	{
		get
		{
			if (!isComplete)
			{
				return taskName;
			}
			return Utilities.CheckmarkIcon() + " " + taskName;
		}
	}

	public BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Text_With_Cancel;

	protected GoalTask()
	{
		persistentID = Utilities.GetNewUniqueID();
		eventDispatcher = new GoalTaskEventDispatcher();
		isComplete = false;
		isPinned = false;
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		goalTaskCompletedBookmark = new GenericTextBookmarkable(GetTaskCompletedString, () => BOOKMARK_TYPE.Special, OnSelectTaskCompletedAlert, RemoveCompletedTaskAlertBookmark, null, null);
	}

	protected GoalTask(SaveDataGoalTask p_data)
	{
		persistentID = p_data.persistentID;
		taskName = p_data.taskName;
		taskTooltip = p_data.taskTooltip;
		isComplete = p_data.isComplete;
		isPinned = p_data.isPinned;
		isCompletedAlertShowing = p_data.isCompletedAlertShowing;
		eventDispatcher = new GoalTaskEventDispatcher();
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		goalTaskCompletedBookmark = new GenericTextBookmarkable(GetTaskCompletedString, () => BOOKMARK_TYPE.Special, OnSelectTaskCompletedAlert, RemoveCompletedTaskAlertBookmark, null, null);
	}

	public virtual void LoadReferences(SaveDataGoalTask p_data)
	{
		if (!isComplete)
		{
			StartTask();
		}
		else if (isCompletedAlertShowing)
		{
			PlayerManager.Instance.player.bookmarkComponent.AddBookmark(goalTaskCompletedBookmark, BOOKMARK_CATEGORY.Alerts);
		}
	}

	public abstract void StartTask();

	protected abstract void EndTask();

	protected virtual void ReevaluateLocalizedTexts()
	{
	}

	protected void CompleteTask()
	{
		if (!isComplete)
		{
			isComplete = true;
			bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
			EndTask();
			ShowCompletedAlert();
			eventDispatcher.ExecuteOnGoalTaskCompleted(this);
		}
	}

	protected void SetTaskName(string p_name)
	{
		taskName = p_name;
		eventDispatcher.ExecuteOnGoalTaskNameUpdated(this);
		bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
	}

	public void ForceCompleteTask()
	{
		CompleteTask();
	}

	public void Pin()
	{
		isPinned = true;
		eventDispatcher.ExecuteOnGoalTaskPinStateChanged(this, isPinned);
		PlayerManager.Instance.player.bookmarkComponent.AddBookmark(this, PlayerManager.Instance.player.goalComponent.GetGoalsBookmarkCategory());
	}

	public void UnPin()
	{
		isPinned = false;
		eventDispatcher.ExecuteOnGoalTaskPinStateChanged(this, isPinned);
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this, PlayerManager.Instance.player.goalComponent.GetGoalsBookmarkCategory());
	}

	public void OnSelectBookmark()
	{
		PlayerUI.Instance.goalsUIController.ShowUI();
	}

	public void RemoveBookmark()
	{
		UnPin();
	}

	public void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		UIManager.Instance.ShowSmallInfo(taskTooltip, p_pos);
	}

	public void OnHoverOutBookmarkItem()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private string GetTaskCompletedString()
	{
		return LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Completed_Task_Alert") + " " + taskName;
	}

	private void OnSelectTaskCompletedAlert()
	{
		PlayerUI.Instance.goalsUIController.ShowUI();
	}

	private void RemoveCompletedTaskAlertBookmark()
	{
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(goalTaskCompletedBookmark);
		isCompletedAlertShowing = false;
	}

	private void ShowCompletedAlert()
	{
		isCompletedAlertShowing = true;
		PlayerManager.Instance.player.bookmarkComponent.AddBookmark(goalTaskCompletedBookmark, BOOKMARK_CATEGORY.Alerts);
	}

	public void OnLocaleChanged(Locale locale)
	{
		ReevaluateLocalizedTexts();
		bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		eventDispatcher.ExecuteOnGoalTaskNameUpdated(this);
		goalTaskCompletedBookmark.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(goalTaskCompletedBookmark);
	}
}
