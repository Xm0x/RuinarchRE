public class SaveDataSpreadPlague : SaveDataGoalTask
{
	public int neededSpreadPlagueCount;

	public int currentSpreadPlagueCount;

	public override void Save(GoalTask data)
	{
		base.Save(data);
		SpreadPlagueToVillagers spreadPlagueToVillagers = data as SpreadPlagueToVillagers;
		neededSpreadPlagueCount = spreadPlagueToVillagers.neededSpreadPlagueCount;
		currentSpreadPlagueCount = spreadPlagueToVillagers.currentSpreadPlagueCount;
	}

	public override GoalTask Load()
	{
		return new SpreadPlagueToVillagers(this);
	}
}
