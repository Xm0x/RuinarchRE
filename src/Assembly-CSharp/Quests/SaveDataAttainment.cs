namespace Quests;

public class SaveDataAttainment : SaveDataVictoryCondition
{
	public override VictoryCondition Load()
	{
		return new Attainment(this);
	}
}
