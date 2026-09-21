using System;
using System.Collections.Generic;
using Maccima_Games.Util;
using Object_Pools;

namespace Quests;

public class Attainment : VictoryCondition
{
	public const int NeededTasksToComplete = 13;

	public override Type serializedData => typeof(SaveDataAttainment);

	public Attainment()
		: base(VICTORY_CONDITION.Attainment)
	{
	}

	public Attainment(SaveDataVictoryCondition p_data)
		: base(VICTORY_CONDITION.Attainment, p_data)
	{
	}

	protected override void SubscribeListeners()
	{
		base.SubscribeListeners();
		Messenger.AddListener<GoalTask>(PlayerSignals.GOAL_TASK_COMPLETED, OnGoalTaskCompleted);
	}

	protected override void UnsubscribeListeners()
	{
		base.UnsubscribeListeners();
		Messenger.RemoveListener<GoalTask>(PlayerSignals.GOAL_TASK_COMPLETED, OnGoalTaskCompleted);
	}

	private void OnGoalTaskCompleted(GoalTask p_task)
	{
		int completedTasks = PlayerManager.Instance.player.goalComponent.completedTasks;
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		if (completedTasks >= 13)
		{
			WinGame();
		}
	}

	protected override string GetWinMessage()
	{
		int completedTasks = PlayerManager.Instance.player.goalComponent.completedTasks;
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Needed_Tasks_Completed");
		log.AddToFillers(null, completedTasks.ToString(), LOG_IDENTIFIER.STRING_1);
		string logText = log.logText;
		LogPool.Release(log);
		return logText;
	}

	protected override void AfterWinGame()
	{
		AchievementManager.Instance.FulfillAchievement(ACHIEVEMENT.RUINARCH);
	}

	protected override string GetBookmarkName()
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim(2);
		dictionary.Add("completedTasks", PlayerManager.Instance.player.goalComponent.completedTasks.ToString());
		dictionary.Add("taskRequirement", 13.ToString());
		return LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Attainment_Title", dictionary);
	}

	public override void OnSelectBookmark()
	{
		PlayerUI.Instance.goalsUIController.ShowUI();
	}
}
