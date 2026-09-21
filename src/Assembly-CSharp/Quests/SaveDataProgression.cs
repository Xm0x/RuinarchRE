namespace Quests;

public class SaveDataProgression : SaveDataVictoryCondition
{
	public override VictoryCondition Load()
	{
		return new Progression(this);
	}
}
