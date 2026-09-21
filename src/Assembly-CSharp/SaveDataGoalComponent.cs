using System;

public class SaveDataGoalComponent : SaveData<GoalComponent>
{
	public SaveDataGoal[] activeGoals;

	public int completedGoals;

	public int pinnedTaskCount;

	public string goalReminderText;

	public int completedTasks;

	public SaveDataSubGoal[] subGoals;

	public override void Save(GoalComponent data)
	{
		base.Save(data);
		if (data.activeGoals != null)
		{
			activeGoals = new SaveDataGoal[data.activeGoals.Length];
			for (int i = 0; i < data.activeGoals.Length; i++)
			{
				Goal goal = data.activeGoals[i];
				SaveDataGoal saveDataGoal = Activator.CreateInstance(goal.serializedData) as SaveDataGoal;
				saveDataGoal.Save(goal);
				activeGoals[i] = saveDataGoal;
			}
		}
		completedGoals = data.completedGoals;
		pinnedTaskCount = data.pinnedTaskCount;
		goalReminderText = data.goalReminderText;
		completedTasks = data.completedTasks;
		subGoals = new SaveDataSubGoal[data.subGoals.Length];
		for (int j = 0; j < data.subGoals.Length; j++)
		{
			SubGoal subGoal = data.subGoals[j];
			SaveDataSubGoal saveDataSubGoal = Activator.CreateInstance(subGoal.serializedData) as SaveDataSubGoal;
			saveDataSubGoal.Save(subGoal);
			subGoals[j] = saveDataSubGoal;
		}
	}

	public override GoalComponent Load()
	{
		return new GoalComponent(this);
	}
}
