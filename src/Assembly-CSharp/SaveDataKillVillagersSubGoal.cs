public class SaveDataKillVillagersSubGoal : SaveDataNumberTrackingSubGoal
{
	public override SubGoal Load()
	{
		return new KillVillagersSubGoal(this);
	}
}
