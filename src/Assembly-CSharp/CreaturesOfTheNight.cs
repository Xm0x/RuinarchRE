using System;

public class CreaturesOfTheNight : Goal
{
	public override Type serializedData => typeof(SaveDataCreaturesOfTheNight);

	public CreaturesOfTheNight()
		: base(PLAYER_GOAL.Creatures_of_the_Night)
	{
	}

	public CreaturesOfTheNight(SaveDataGoal p_data)
		: base(p_data, PLAYER_GOAL.Creatures_of_the_Night)
	{
	}

	protected override GoalTask[] CreateInitialTasks()
	{
		return new GoalTask[4]
		{
			new CreateVampireLord(),
			new SpawnVampireClan(),
			new CreateLycanWithPelt(),
			new SpawnLycanClan()
		};
	}
}
