namespace Quests;

public class SaveDataEradication : SaveDataVictoryCondition
{
	public override VictoryCondition Load()
	{
		return new Eradication(this);
	}
}
