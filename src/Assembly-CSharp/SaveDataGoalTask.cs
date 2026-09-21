public abstract class SaveDataGoalTask : SaveData<GoalTask>
{
	public string persistentID;

	public string taskName;

	public string taskTooltip;

	public bool isComplete;

	public bool isPinned;

	public bool isCompletedAlertShowing;

	public override void Save(GoalTask data)
	{
		persistentID = data.persistentID;
		taskName = data.taskName;
		taskTooltip = data.taskTooltip;
		isComplete = data.isComplete;
		isPinned = data.isPinned;
		isCompletedAlertShowing = data.isCompletedAlertShowing;
	}
}
