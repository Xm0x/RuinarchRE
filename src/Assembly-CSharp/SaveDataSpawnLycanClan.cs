public class SaveDataSpawnLycanClan : SaveDataGoalTask
{
	public override GoalTask Load()
	{
		return new SpawnLycanClan(this);
	}
}
