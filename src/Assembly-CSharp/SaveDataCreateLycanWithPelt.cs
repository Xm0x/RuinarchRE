public class SaveDataCreateLycanWithPelt : SaveDataGoalTask
{
	public override GoalTask Load()
	{
		return new CreateLycanWithPelt(this);
	}
}
