public class SaveDataCreateAnAbomination : SaveDataGoalTask
{
	public override GoalTask Load()
	{
		return new CreateAnAbomination(this);
	}
}
