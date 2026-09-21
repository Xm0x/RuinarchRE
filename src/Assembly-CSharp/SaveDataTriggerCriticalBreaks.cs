public class SaveDataTriggerCriticalBreaks : SaveDataGoalTask
{
	public int neededCriticalBreakCount;

	public int triggeredCriticalBreakCount;

	public override void Save(GoalTask data)
	{
		base.Save(data);
		TriggerCriticalBreaks triggerCriticalBreaks = data as TriggerCriticalBreaks;
		neededCriticalBreakCount = triggerCriticalBreaks.neededCriticalBreakCount;
		triggeredCriticalBreakCount = triggerCriticalBreaks.triggeredCriticalBreakCount;
	}

	public override GoalTask Load()
	{
		return new TriggerCriticalBreaks(this);
	}
}
