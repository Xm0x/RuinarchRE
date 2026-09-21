using System;
using System.Collections.Generic;
using UnityEngine.Localization;

public abstract class Goal : GoalTaskEventDispatcher.IGoalTaskListener, LocalizationManagerEventDispatcher.ILocaleChangeListener
{
	public string goalName { get; private set; }

	public PLAYER_GOAL goalType { get; }

	public GoalTask[] tasks { get; private set; }

	public List<GoalTask> completedTasks { get; private set; }

	public bool isCompleted { get; private set; }

	public GoalEventDispatcher eventDispatcher { get; private set; }

	public abstract Type serializedData { get; }

	protected Goal(PLAYER_GOAL p_goalType)
	{
		goalType = p_goalType;
		completedTasks = new List<GoalTask>();
		isCompleted = false;
		eventDispatcher = new GoalEventDispatcher();
	}

	protected Goal(SaveDataGoal p_data, PLAYER_GOAL p_goalType)
	{
		goalType = p_goalType;
		goalName = p_data.goalName;
		eventDispatcher = new GoalEventDispatcher();
		tasks = new GoalTask[p_data.tasks.Length];
		for (int i = 0; i < p_data.tasks.Length; i++)
		{
			GoalTask goalTask = p_data.tasks[i].Load();
			tasks[i] = goalTask;
		}
		completedTasks = new List<GoalTask>();
		if (p_data.completedTasks != null)
		{
			for (int j = 0; j < p_data.completedTasks.Count; j++)
			{
				string p_id = p_data.completedTasks[j];
				GoalTask goalTaskByPersistentID = GetGoalTaskByPersistentID(p_id);
				if (goalTaskByPersistentID != null)
				{
					completedTasks.Add(goalTaskByPersistentID);
				}
			}
		}
		isCompleted = p_data.isCompleted;
	}

	public void LoadReferences(SaveDataGoal p_data)
	{
		for (int i = 0; i < p_data.tasks.Length; i++)
		{
			SaveDataGoalTask p_data2 = p_data.tasks[i];
			tasks[i].LoadReferences(p_data2);
		}
		if (!isCompleted)
		{
			SubscribeToTasks();
		}
	}

	public void Initialize()
	{
		goalName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", goalType.ToStringEnum());
		tasks = CreateInitialTasks();
		StartTasks();
		SubscribeToTasks();
	}

	protected abstract GoalTask[] CreateInitialTasks();

	private void StartTasks()
	{
		for (int i = 0; i < tasks.Length; i++)
		{
			tasks[i].StartTask();
		}
	}

	private void SubscribeToTasks()
	{
		for (int i = 0; i < tasks.Length; i++)
		{
			tasks[i].eventDispatcher.SubscribeToGoalTask(this);
		}
	}

	private void UnsubscribeToTasks()
	{
		for (int i = 0; i < tasks.Length; i++)
		{
			tasks[i].eventDispatcher.UnsubscribeToGoalTask(this);
		}
	}

	private GoalTask GetGoalTaskByPersistentID(string p_id)
	{
		for (int i = 0; i < tasks.Length; i++)
		{
			GoalTask goalTask = tasks[i];
			if (goalTask.persistentID == p_id)
			{
				return goalTask;
			}
		}
		return null;
	}

	public void OnGoalTaskNameUpdated(GoalTask p_task)
	{
	}

	public void OnGoalTaskCompleted(GoalTask p_task)
	{
		completedTasks.Add(p_task);
		if (completedTasks.Count == tasks.Length)
		{
			GoalCompleted();
		}
	}

	public void OnGoalTaskPinStateChanged(GoalTask p_task, bool p_isPinned)
	{
		eventDispatcher.ExecuteOnChildTaskPinStateChanged(this, p_task, p_isPinned);
	}

	private void GoalCompleted()
	{
		if (!isCompleted)
		{
			isCompleted = true;
			eventDispatcher.ExecuteOnGoalCompleted(this);
			UnsubscribeToTasks();
			switch (goalType)
			{
			case PLAYER_GOAL.Undead_Supremacy:
				AchievementManager.Instance.FulfillAchievement(ACHIEVEMENT.UNDEAD_SUPREMACY);
				break;
			case PLAYER_GOAL.Creatures_of_the_Night:
				AchievementManager.Instance.FulfillAchievement(ACHIEVEMENT.CREATURES_OF_THE_NIGHT);
				break;
			case PLAYER_GOAL.Idol_Worship:
				AchievementManager.Instance.FulfillAchievement(ACHIEVEMENT.IDOL_WORSHIP);
				break;
			case PLAYER_GOAL.Terrorized_Villagers:
				AchievementManager.Instance.FulfillAchievement(ACHIEVEMENT.TERRORIZED_VILLAGERS);
				break;
			case PLAYER_GOAL.Outbreak:
				AchievementManager.Instance.FulfillAchievement(ACHIEVEMENT.OUTBREAK);
				break;
			case PLAYER_GOAL.Death_And_Destruction:
				AchievementManager.Instance.FulfillAchievement(ACHIEVEMENT.DEATH_AND_DESTRUCTION);
				break;
			}
		}
	}

	public void OnLocaleChanged(Locale locale)
	{
		goalName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", goalType.ToStringEnum());
	}
}
