using System;
using Quests;
using UnityEngine.Localization;
using UtilityScripts;

public class GoalComponent : GoalEventDispatcher.IGoalListener, GoalTaskEventDispatcher.IGoalTaskListener, LocalizationManagerEventDispatcher.ILocaleChangeListener
{
	public Goal[] activeGoals { get; private set; }

	public int completedGoals { get; private set; }

	public int completedTasks { get; private set; }

	public int pinnedTaskCount { get; private set; }

	public GenericTextBookmarkable bookmarkGoalReminder { get; private set; }

	public string goalReminderText { get; private set; }

	public SubGoal[] subGoals { get; private set; }

	public GoalComponent()
	{
		goalReminderText = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Pin_Task_Reminder");
		bookmarkGoalReminder = new GenericTextBookmarkable(GetGoalReminderText, () => BOOKMARK_TYPE.Text, OnSelectBookmarkGoalReminder, null, null, null);
	}

	public GoalComponent(SaveDataGoalComponent p_data)
	{
		goalReminderText = p_data.goalReminderText;
		bookmarkGoalReminder = new GenericTextBookmarkable(GetGoalReminderText, () => BOOKMARK_TYPE.Text, OnSelectBookmarkGoalReminder, null, null, null);
		if (p_data.activeGoals != null)
		{
			activeGoals = new Goal[p_data.activeGoals.Length];
			for (int num = 0; num < p_data.activeGoals.Length; num++)
			{
				Goal goal = p_data.activeGoals[num].Load();
				activeGoals[num] = goal;
			}
		}
		completedGoals = p_data.completedGoals;
		pinnedTaskCount = p_data.pinnedTaskCount;
		completedTasks = p_data.completedTasks;
		subGoals = new SubGoal[p_data.subGoals.Length];
		for (int num2 = 0; num2 < p_data.subGoals.Length; num2++)
		{
			SubGoal subGoal = p_data.subGoals[num2].Load();
			subGoals[num2] = subGoal;
		}
	}

	public void LoadReferences(SaveDataGoalComponent p_data)
	{
		if (p_data.activeGoals != null)
		{
			for (int i = 0; i < p_data.activeGoals.Length; i++)
			{
				SaveDataGoal p_data2 = p_data.activeGoals[i];
				Goal goal = activeGoals[i];
				goal.LoadReferences(p_data2);
				goal.eventDispatcher.SubscribeToGoal(this);
				for (int j = 0; j < goal.tasks.Length; j++)
				{
					goal.tasks[j].eventDispatcher.SubscribeToGoalTask(this);
				}
			}
		}
		for (int k = 0; k < p_data.subGoals.Length; k++)
		{
			SaveDataSubGoal p_data3 = p_data.subGoals[k];
			subGoals[k].LoadReferences(p_data3);
		}
	}

	public void Initialize()
	{
		if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Attainment)
		{
			PLAYER_GOAL[] enumValues = CollectionUtilities.GetEnumValues<PLAYER_GOAL>();
			activeGoals = new Goal[enumValues.Length];
			for (int i = 0; i < enumValues.Length; i++)
			{
				Type type = Type.GetType(enumValues[i].ToStringEnumNoSpace());
				if (type != null)
				{
					Goal goal = Activator.CreateInstance(type) as Goal;
					goal.Initialize();
					goal.eventDispatcher.SubscribeToGoal(this);
					activeGoals[i] = goal;
					for (int j = 0; j < goal.tasks.Length; j++)
					{
						goal.tasks[j].eventDispatcher.SubscribeToGoalTask(this);
					}
				}
			}
		}
		SUB_GOAL[] enumValues2 = CollectionUtilities.GetEnumValues<SUB_GOAL>();
		subGoals = new SubGoal[enumValues2.Length];
		for (int k = 0; k < enumValues2.Length; k++)
		{
			SUB_GOAL sUB_GOAL = enumValues2[k];
			SubGoal subGoal = Activator.CreateInstance(sUB_GOAL.ClassType(), sUB_GOAL, sUB_GOAL.GetAchievementType()) as SubGoal;
			subGoals[k] = subGoal;
		}
	}

	public void InitializeAfterLoadoutPicked()
	{
		UpdateBookmarkStateOfGoalReminder();
		if (activeGoals == null)
		{
			return;
		}
		for (int i = 0; i < activeGoals.Length; i++)
		{
			Goal goal = activeGoals[i];
			for (int j = 0; j < goal.tasks.Length; j++)
			{
				GoalTask goalTask = goal.tasks[j];
				if (goalTask.isPinned)
				{
					PlayerManager.Instance.player.bookmarkComponent.AddBookmark(goalTask, GetGoalsBookmarkCategory());
				}
			}
		}
	}

	public void OnGoalCompleted(Goal p_goal)
	{
		for (int i = 0; i < p_goal.tasks.Length; i++)
		{
			p_goal.tasks[i].eventDispatcher.UnsubscribeToGoalTask(this);
		}
		completedGoals++;
	}

	public void OnChildTaskPinStateChanged(Goal p_goal, GoalTask p_affectedTask, bool p_isPinned)
	{
		if (p_isPinned)
		{
			pinnedTaskCount++;
		}
		else
		{
			pinnedTaskCount--;
		}
		UpdateBookmarkStateOfGoalReminder();
	}

	private void OnSelectBookmarkGoalReminder()
	{
		PlayerUI.Instance.goalsUIController.ShowUI();
	}

	private string GetGoalReminderText()
	{
		return goalReminderText;
	}

	private void UpdateBookmarkStateOfGoalReminder()
	{
		if (activeGoals != null)
		{
			BOOKMARK_CATEGORY goalsBookmarkCategory = GetGoalsBookmarkCategory();
			if (pinnedTaskCount <= 0)
			{
				PlayerManager.Instance.player.bookmarkComponent.AddBookmark(bookmarkGoalReminder, goalsBookmarkCategory);
			}
			else
			{
				PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(bookmarkGoalReminder, goalsBookmarkCategory);
			}
		}
	}

	public BOOKMARK_CATEGORY GetGoalsBookmarkCategory()
	{
		BOOKMARK_CATEGORY result = BOOKMARK_CATEGORY.Sub_Goals;
		if (QuestManager.Instance.victoryCondition.type == VICTORY_CONDITION.Attainment)
		{
			result = BOOKMARK_CATEGORY.Win_Condition;
		}
		return result;
	}

	public void OnGoalTaskNameUpdated(GoalTask p_task)
	{
	}

	public void OnGoalTaskCompleted(GoalTask p_task)
	{
		completedTasks++;
		Messenger.Broadcast(PlayerSignals.GOAL_TASK_COMPLETED, p_task);
	}

	public void OnGoalTaskPinStateChanged(GoalTask p_task, bool p_isPinned)
	{
	}

	public void OnLocaleChanged(Locale locale)
	{
		goalReminderText = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Pin_Task_Reminder");
		bookmarkGoalReminder.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(bookmarkGoalReminder);
	}

	public SubGoal GetSubGoal(SUB_GOAL p_subGoal)
	{
		for (int i = 0; i < subGoals.Length; i++)
		{
			SubGoal subGoal = subGoals[i];
			if (subGoal.subGoalType == p_subGoal)
			{
				return subGoal;
			}
		}
		throw new Exception("No sub goal of type found! " + p_subGoal);
	}

	public void IncreaseNumberTrackingSubGoal(SUB_GOAL p_subGoal, int p_amount)
	{
		if (GetSubGoal(p_subGoal) is NumberTrackingSubGoal numberTrackingSubGoal)
		{
			numberTrackingSubGoal.IncreaseCurrentValue(p_amount);
			if (numberTrackingSubGoal.intCurrentValue >= numberTrackingSubGoal.intGoal)
			{
				CompleteSubGoal(numberTrackingSubGoal);
			}
		}
	}

	public void CompleteSubGoal(SUB_GOAL p_subGoal)
	{
		SubGoal subGoal = GetSubGoal(p_subGoal);
		CompleteSubGoal(subGoal);
	}

	public void CompleteSubGoal(SubGoal p_subGoal)
	{
		p_subGoal.CompleteSubGoal();
	}
}
