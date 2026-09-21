using System;
using UtilityScripts;

public abstract class SubGoal : IBookmarkable
{
	public SUB_GOAL subGoalType { get; }

	public ACHIEVEMENT connectedAchievement { get; }

	public bool isComplete { get; private set; }

	public bool isPinned { get; private set; }

	public BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

	public SubGoalEventDispatcher subGoalEventDispatcher { get; }

	public Reward completionReward { get; }

	public string localizedTitle { get; private set; }

	public string localizedDescriptiveName { get; private set; }

	public string bookmarkName
	{
		get
		{
			if (!isComplete)
			{
				return localizedDescriptiveName;
			}
			return Utilities.CheckmarkIcon() + " " + localizedDescriptiveName;
		}
	}

	public BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Text_With_Cancel;

	public abstract Type serializedData { get; }

	public SubGoal(SUB_GOAL p_subGoal, ACHIEVEMENT p_achievement)
	{
		subGoalType = p_subGoal;
		connectedAchievement = p_achievement;
		isComplete = false;
		isPinned = false;
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		subGoalEventDispatcher = new SubGoalEventDispatcher();
		completionReward = GetRewardForCompletion();
		localizedTitle = LocalizationManager.Instance.GetLocalizedValue("Achievements_Table", subGoalType.ToStringEnum() + "_NAME");
		localizedDescriptiveName = CreateLocalizedDescriptiveName();
	}

	public SubGoal(SaveDataSubGoal p_data)
	{
		subGoalType = p_data.subGoalType;
		connectedAchievement = p_data.connectedAchievement;
		isComplete = p_data.isComplete;
		isPinned = p_data.isPinned;
		bookmarkEventDispatcher = new BookmarkableEventDispatcher();
		subGoalEventDispatcher = new SubGoalEventDispatcher();
		completionReward = p_data.completionReward;
	}

	public virtual void LoadReferences(SaveDataSubGoal p_data)
	{
		localizedTitle = LocalizationManager.Instance.GetLocalizedValue("Achievements_Table", subGoalType.ToStringEnum() + "_NAME");
		localizedDescriptiveName = CreateLocalizedDescriptiveName();
		if (isPinned)
		{
			PlayerManager.Instance.player.bookmarkComponent.AddBookmark(this, BOOKMARK_CATEGORY.Sub_Goals);
		}
	}

	protected virtual string CreateLocalizedDescriptiveName()
	{
		return LocalizationManager.Instance.GetLocalizedValue("Achievements_Table", subGoalType.ToStringEnum() + "_DESC");
	}

	protected void RegenerateDescriptiveName()
	{
		localizedDescriptiveName = CreateLocalizedDescriptiveName();
		bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		subGoalEventDispatcher.ExecuteOnSubGoalNameUpdated(this);
	}

	public void CompleteSubGoal()
	{
		if (!isComplete)
		{
			isComplete = true;
			bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
			subGoalEventDispatcher.ExecuteOnSubGoalCompleted(this);
			AchievementManager.Instance.FulfillAchievement(connectedAchievement);
		}
	}

	public void GrantReward()
	{
		PlayerManager.Instance.player.currenciesComponent.AddCurrency(completionReward);
	}

	private Reward GetRewardForCompletion()
	{
		switch (subGoalType)
		{
		case SUB_GOAL.GOAL_POISON_FOOD:
		case SUB_GOAL.GOAL_DESTROY_RESOURCE:
		case SUB_GOAL.GOAL_CREATE_PSYCHO:
		case SUB_GOAL.GOAL_REMOVE_BUFF:
		case SUB_GOAL.GOAL_MAKE_VILLAGER_EVIL:
		case SUB_GOAL.GOAL_TRIGGER_FLAW:
			return new Reward(CURRENCY.Chaotic_Energy, 40);
		case SUB_GOAL.GOAL_IMPRISON_VILLAGER:
		case SUB_GOAL.GOAL_DEMON_CULTIST:
		case SUB_GOAL.GOAL_KILL_ELF:
		case SUB_GOAL.GOAL_KILL_HUMAN:
		case SUB_GOAL.GOAL_SHARE_CRIME_INTEL:
		case SUB_GOAL.GOAL_SNATCH_OBJECT:
		case SUB_GOAL.GOAL_TRIGGER_AROUSAL:
		case SUB_GOAL.GOAL_POISON_CLOUD:
		case SUB_GOAL.GOAL_FROSTY_FOG:
		case SUB_GOAL.GOAL_BALL_LIGHTNING:
			if (WorldSettings.Instance.worldSettingsData.IsSpiritEnergyEnabledBasedOnVictoryCondition())
			{
				return new Reward(CURRENCY.Spirit_Energy, 25);
			}
			return new Reward(CURRENCY.Chaotic_Energy, 25);
		case SUB_GOAL.GOAL_CAPTURE_TRITON:
		case SUB_GOAL.GOAL_FALLEN_ANGEL:
			if (WorldSettings.Instance.worldSettingsData.IsSpiritEnergyEnabledBasedOnVictoryCondition())
			{
				return new Reward(CURRENCY.Spirit_Energy, 50);
			}
			return new Reward(CURRENCY.Chaotic_Energy, 50);
		default:
			if (WorldSettings.Instance.worldSettingsData.IsSpiritEnergyEnabledBasedOnVictoryCondition())
			{
				return new Reward(CURRENCY.Spirit_Energy, 10);
			}
			return new Reward(CURRENCY.Chaotic_Energy, 10);
		}
	}

	public void Pin()
	{
		isPinned = true;
		subGoalEventDispatcher.ExecuteOnSubGoalPinStateChanged(this, isPinned);
		PlayerManager.Instance.player.bookmarkComponent.AddBookmark(this, BOOKMARK_CATEGORY.Sub_Goals);
	}

	public void UnPin()
	{
		isPinned = false;
		subGoalEventDispatcher.ExecuteOnSubGoalPinStateChanged(this, isPinned);
		RemoveBookmark();
	}

	public void OnSelectBookmark()
	{
		PlayerUI.Instance.goalsUIController.ShowUI();
	}

	public void RemoveBookmark()
	{
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this, BOOKMARK_CATEGORY.Sub_Goals);
	}

	public void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
	}

	public void OnHoverOutBookmarkItem()
	{
	}

	public int GetSideGoalIndex()
	{
		return subGoalType switch
		{
			SUB_GOAL.GOAL_POISON_FOOD => 0, 
			SUB_GOAL.GOAL_TRIGGER_FLAW => 1, 
			SUB_GOAL.GOAL_DESTROY_RESOURCE => 2, 
			SUB_GOAL.GOAL_CREATE_PSYCHO => 3, 
			SUB_GOAL.GOAL_REMOVE_BUFF => 4, 
			SUB_GOAL.GOAL_MAKE_VILLAGER_EVIL => 5, 
			SUB_GOAL.GOAL_KILL_ELF => 6, 
			SUB_GOAL.GOAL_KILL_HUMAN => 7, 
			SUB_GOAL.GOAL_SHARE_CRIME_INTEL => 8, 
			SUB_GOAL.GOAL_SNATCH_OBJECT => 9, 
			SUB_GOAL.GOAL_IMPRISON_VILLAGER => 10, 
			SUB_GOAL.GOAL_DEMON_CULTIST => 11, 
			SUB_GOAL.GOAL_TRIGGER_AROUSAL => 12, 
			SUB_GOAL.GOAL_POISON_CLOUD => 13, 
			SUB_GOAL.GOAL_FROSTY_FOG => 14, 
			SUB_GOAL.GOAL_BALL_LIGHTNING => 15, 
			SUB_GOAL.GOAL_FALLEN_ANGEL => 16, 
			SUB_GOAL.GOAL_CAPTURE_TRITON => 17, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
