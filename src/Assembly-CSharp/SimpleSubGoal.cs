using System;

public class SimpleSubGoal : SubGoal
{
	public override Type serializedData => typeof(SaveDataSimpleSubGoal);

	public SimpleSubGoal(SUB_GOAL p_subGoal, ACHIEVEMENT p_achievement)
		: base(p_subGoal, p_achievement)
	{
	}

	public SimpleSubGoal(SaveDataSubGoal p_data)
		: base(p_data)
	{
	}
}
