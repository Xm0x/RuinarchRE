using System;

public class NumberTrackingSubGoal : SubGoal
{
	public int intGoal { get; }

	public int intCurrentValue { get; private set; }

	public override Type serializedData => typeof(SaveDataNumberTrackingSubGoal);

	public NumberTrackingSubGoal(int p_goal, SUB_GOAL p_subGoal, ACHIEVEMENT p_achievement)
		: base(p_subGoal, p_achievement)
	{
		intGoal = p_goal;
		RegenerateDescriptiveName();
	}

	public NumberTrackingSubGoal(SaveDataNumberTrackingSubGoal p_data)
		: base(p_data)
	{
		intGoal = p_data.intGoal;
		intCurrentValue = p_data.intCurrentValue;
	}

	public void IncreaseCurrentValue(int p_amount)
	{
		intCurrentValue += p_amount;
		UniqueActionsOnIncreaseValue();
	}

	protected virtual void UniqueActionsOnIncreaseValue()
	{
	}
}
