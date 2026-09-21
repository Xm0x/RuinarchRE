public class SaveDataKillVillagersWithPlague : SaveDataGoalTask
{
	public int neededPlagueKillCount;

	public int currentPlagueKillCount;

	public override void Save(GoalTask data)
	{
		base.Save(data);
		KillVillagersWithPlague killVillagersWithPlague = data as KillVillagersWithPlague;
		neededPlagueKillCount = killVillagersWithPlague.neededPlagueKillCount;
		currentPlagueKillCount = killVillagersWithPlague.currentPlagueKillCount;
	}

	public override GoalTask Load()
	{
		return new KillVillagersWithPlague(this);
	}
}
