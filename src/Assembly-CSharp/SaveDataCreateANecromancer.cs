public class SaveDataCreateANecromancer : SaveDataGoalTask
{
	public override GoalTask Load()
	{
		return new CreateANecromancer(this);
	}
}
