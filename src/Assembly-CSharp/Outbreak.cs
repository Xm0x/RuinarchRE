using System;

public class Outbreak : Goal
{
	public override Type serializedData => typeof(SaveDataOutbreak);

	public Outbreak()
		: base(PLAYER_GOAL.Outbreak)
	{
	}

	public Outbreak(SaveDataGoal p_data)
		: base(p_data, PLAYER_GOAL.Outbreak)
	{
	}

	protected override GoalTask[] CreateInitialTasks()
	{
		return new GoalTask[4]
		{
			new CreateAnAbomination(),
			new SpreadPlagueToVillagers(),
			new SpreadPlagueToMonsters(),
			new KillVillagersWithPlague()
		};
	}
}
