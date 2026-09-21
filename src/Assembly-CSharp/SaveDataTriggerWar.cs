public class SaveDataTriggerWar : SaveDataGoalTask
{
	public override GoalTask Load()
	{
		return new TriggerWar(this);
	}
}
