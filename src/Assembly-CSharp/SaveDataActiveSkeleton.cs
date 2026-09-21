public class SaveDataActiveSkeleton : SaveDataGoalTask
{
	public int neededSkeletonCount;

	public override void Save(GoalTask data)
	{
		base.Save(data);
		ActiveSkeletons activeSkeletons = data as ActiveSkeletons;
		neededSkeletonCount = activeSkeletons.neededSkeletonCount;
	}

	public override GoalTask Load()
	{
		return new ActiveSkeletons(this);
	}
}
