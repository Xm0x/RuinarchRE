public class SaveDataTriggerGrudges : SaveDataGoalTask
{
	public int neededGrudgeCount;

	public int triggeredGrudgeCount;

	public override void Save(GoalTask data)
	{
		base.Save(data);
		TriggerGrudges triggerGrudges = data as TriggerGrudges;
		neededGrudgeCount = triggerGrudges.neededGrudgeCount;
		triggeredGrudgeCount = triggerGrudges.triggeredGrudgeCount;
	}

	public override GoalTask Load()
	{
		return new TriggerGrudges(this);
	}
}
