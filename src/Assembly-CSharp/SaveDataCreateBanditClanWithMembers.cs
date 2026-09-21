public class SaveDataCreateBanditClanWithMembers : SaveDataGoalTask
{
	public int neededBanditClanMembers;

	public override void Save(GoalTask data)
	{
		base.Save(data);
		CreateBanditClanWithMembers createBanditClanWithMembers = data as CreateBanditClanWithMembers;
		neededBanditClanMembers = createBanditClanWithMembers.neededBanditClanMembers;
	}

	public override GoalTask Load()
	{
		return new CreateBanditClanWithMembers(this);
	}
}
