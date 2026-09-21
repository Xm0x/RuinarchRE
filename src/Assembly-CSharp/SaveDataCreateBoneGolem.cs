public class SaveDataCreateBoneGolem : SaveDataGoalTask
{
	public override GoalTask Load()
	{
		return new CreateBoneGolem(this);
	}
}
