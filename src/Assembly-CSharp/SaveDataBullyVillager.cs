public class SaveDataBullyVillager : SaveDataGoalTask
{
	public override GoalTask Load()
	{
		return new BullyVillager(this);
	}
}
