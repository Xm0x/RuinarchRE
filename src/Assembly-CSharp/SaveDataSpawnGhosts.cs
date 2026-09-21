public class SaveDataSpawnGhosts : SaveDataGoalTask
{
	public int neededGhostCount;

	public int spawnedGhostCount;

	public override void Save(GoalTask data)
	{
		base.Save(data);
		SpawnGhosts spawnGhosts = data as SpawnGhosts;
		neededGhostCount = spawnGhosts.neededGhostCount;
		spawnedGhostCount = spawnGhosts.spawnedGhostCount;
	}

	public override GoalTask Load()
	{
		return new SpawnGhosts(this);
	}
}
