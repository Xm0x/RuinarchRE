using System;

public class UndeadSupremacy : Goal
{
	public override Type serializedData => typeof(SaveDataUndeadSupremacy);

	public UndeadSupremacy()
		: base(PLAYER_GOAL.Undead_Supremacy)
	{
	}

	public UndeadSupremacy(SaveDataUndeadSupremacy p_data)
		: base(p_data, PLAYER_GOAL.Undead_Supremacy)
	{
	}

	protected override GoalTask[] CreateInitialTasks()
	{
		return new GoalTask[4]
		{
			new CreateANecromancer(),
			new ActiveSkeletons(),
			new SpawnGhosts(),
			new SpawnRevenant()
		};
	}
}
