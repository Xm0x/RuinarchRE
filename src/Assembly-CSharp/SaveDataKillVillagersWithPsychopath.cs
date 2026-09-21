public class SaveDataKillVillagersWithPsychopath : SaveDataGoalTask
{
	public int neededKillCount;

	public int triggeredKillCount;

	public override void Save(GoalTask data)
	{
		base.Save(data);
		KillVillagersWithPsychopath killVillagersWithPsychopath = data as KillVillagersWithPsychopath;
		neededKillCount = killVillagersWithPsychopath.neededKillCount;
		triggeredKillCount = killVillagersWithPsychopath.triggeredKillCount;
	}

	public override GoalTask Load()
	{
		return new KillVillagersWithPsychopath(this);
	}
}
