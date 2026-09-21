public class SaveDataKillVillagersWithMonster : SaveDataGoalTask
{
	public int neededKillCount;

	public int currentKillCount;

	public override void Save(GoalTask data)
	{
		base.Save(data);
		KillVillagersTask killVillagersTask = data as KillVillagersTask;
		neededKillCount = killVillagersTask.neededKillCount;
		currentKillCount = killVillagersTask.currentKillCount;
	}

	public override GoalTask Load()
	{
		return new KillVillagersTask(this);
	}
}
