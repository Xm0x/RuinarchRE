public class SaveDataCreateDemonCult : SaveDataGoalTask
{
	public override GoalTask Load()
	{
		return new CreateDemonCult(this);
	}
}
