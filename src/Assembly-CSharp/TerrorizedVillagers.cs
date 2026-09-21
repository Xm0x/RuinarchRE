using System;

public class TerrorizedVillagers : Goal
{
	public override Type serializedData => typeof(SaveDataTerrorizedVillagers);

	public TerrorizedVillagers()
		: base(PLAYER_GOAL.Terrorized_Villagers)
	{
	}

	public TerrorizedVillagers(SaveDataGoal p_data)
		: base(p_data, PLAYER_GOAL.Terrorized_Villagers)
	{
	}

	protected override GoalTask[] CreateInitialTasks()
	{
		return new GoalTask[4]
		{
			new TriggerGrudges(),
			new TriggerCriticalBreaks(),
			new KillVillagersWithPsychopath(),
			new BullyVillager()
		};
	}
}
