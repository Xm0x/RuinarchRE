using System;

public class DeathAndDestruction : Goal
{
	public override Type serializedData => typeof(SaveDataDeathAndDestruction);

	public DeathAndDestruction()
		: base(PLAYER_GOAL.Death_And_Destruction)
	{
	}

	public DeathAndDestruction(SaveDataGoal p_data)
		: base(p_data, PLAYER_GOAL.Death_And_Destruction)
	{
	}

	protected override GoalTask[] CreateInitialTasks()
	{
		return new GoalTask[4]
		{
			new KillVillagersTask(),
			new DestroyVillageStructures(),
			new TriggerWar(),
			new CreateBanditClanWithMembers()
		};
	}
}
