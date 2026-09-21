public class SaveDataSpreadPlagueToMonsters : SaveDataGoalTask
{
	public int neededSpreadPlagueCount;

	public int currentSpreadPlagueCount;

	public override void Save(GoalTask data)
	{
		base.Save(data);
		SpreadPlagueToMonsters spreadPlagueToMonsters = data as SpreadPlagueToMonsters;
		neededSpreadPlagueCount = spreadPlagueToMonsters.neededSpreadPlagueCount;
		currentSpreadPlagueCount = spreadPlagueToMonsters.currentSpreadPlagueCount;
	}

	public override GoalTask Load()
	{
		return new SpreadPlagueToMonsters(this);
	}
}
