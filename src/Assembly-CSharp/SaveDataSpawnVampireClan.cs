public class SaveDataSpawnVampireClan : SaveDataGoalTask
{
	public override GoalTask Load()
	{
		return new SpawnVampireClan(this);
	}
}
