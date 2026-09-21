using System;

public class IdolWorship : Goal
{
	public override Type serializedData => typeof(SaveDataIdolWorship);

	public IdolWorship()
		: base(PLAYER_GOAL.Idol_Worship)
	{
	}

	public IdolWorship(SaveDataGoal p_data)
		: base(p_data, PLAYER_GOAL.Idol_Worship)
	{
	}

	protected override GoalTask[] CreateInitialTasks()
	{
		return new GoalTask[4]
		{
			new RecruitDemonCultists(),
			new CreateDemonCultLeader(),
			new CreateDemonCult(),
			new CreateBoneGolem()
		};
	}
}
