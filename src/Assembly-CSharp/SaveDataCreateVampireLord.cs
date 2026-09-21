public class SaveDataCreateVampireLord : SaveDataGoalTask
{
	public override GoalTask Load()
	{
		return new CreateVampireLord(this);
	}
}
