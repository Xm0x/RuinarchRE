public class SaveDataCreateCultLeader : SaveDataGoalTask
{
	public override GoalTask Load()
	{
		return new CreateDemonCultLeader(this);
	}
}
