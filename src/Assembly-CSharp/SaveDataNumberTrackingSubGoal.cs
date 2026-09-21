public class SaveDataNumberTrackingSubGoal : SaveDataSubGoal
{
	public int intGoal;

	public int intCurrentValue;

	public override void Save(SubGoal p_data)
	{
		base.Save(p_data);
		if (p_data is NumberTrackingSubGoal numberTrackingSubGoal)
		{
			intGoal = numberTrackingSubGoal.intGoal;
			intCurrentValue = numberTrackingSubGoal.intCurrentValue;
		}
	}

	public override SubGoal Load()
	{
		return new NumberTrackingSubGoal(this);
	}
}
