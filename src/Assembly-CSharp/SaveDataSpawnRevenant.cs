public class SaveDataSpawnRevenant : SaveDataGoalTask
{
	public override GoalTask Load()
	{
		return new SpawnRevenant(this);
	}
}
