using System;
using System.Collections.Generic;

public class SaveDataGoal : SaveData<Goal>
{
	public string goalName;

	public SaveDataGoalTask[] tasks;

	public List<string> completedTasks;

	public bool isCompleted;

	public override void Save(Goal data)
	{
		goalName = data.goalName;
		tasks = new SaveDataGoalTask[data.tasks.Length];
		for (int i = 0; i < data.tasks.Length; i++)
		{
			GoalTask goalTask = data.tasks[i];
			SaveDataGoalTask saveDataGoalTask = Activator.CreateInstance(goalTask.serializedData) as SaveDataGoalTask;
			saveDataGoalTask.Save(goalTask);
			tasks[i] = saveDataGoalTask;
		}
		if (data.completedTasks.Count > 0)
		{
			completedTasks = new List<string>();
			for (int j = 0; j < data.completedTasks.Count; j++)
			{
				GoalTask goalTask2 = data.completedTasks[j];
				completedTasks.Add(goalTask2.persistentID);
			}
		}
		isCompleted = data.isCompleted;
	}
}
