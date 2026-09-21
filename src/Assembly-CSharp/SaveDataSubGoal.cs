public abstract class SaveDataSubGoal : SaveData<SubGoal>
{
	public SUB_GOAL subGoalType;

	public ACHIEVEMENT connectedAchievement;

	public bool isComplete;

	public bool isPinned;

	public Reward completionReward;

	public override void Save(SubGoal p_data)
	{
		subGoalType = p_data.subGoalType;
		connectedAchievement = p_data.connectedAchievement;
		isComplete = p_data.isComplete;
		isPinned = p_data.isPinned;
		completionReward = p_data.completionReward;
	}
}
