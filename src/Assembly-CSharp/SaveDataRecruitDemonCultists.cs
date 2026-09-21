public class SaveDataRecruitDemonCultists : SaveDataGoalTask
{
	public int neededDemonCultistCount;

	public int recruitedDemonCultistCount;

	public override void Save(GoalTask data)
	{
		base.Save(data);
		RecruitDemonCultists recruitDemonCultists = data as RecruitDemonCultists;
		neededDemonCultistCount = recruitDemonCultists.neededDemonCultistCount;
		recruitedDemonCultistCount = recruitDemonCultists.recruitedDemonCultistCount;
	}

	public override GoalTask Load()
	{
		return new RecruitDemonCultists(this);
	}
}
