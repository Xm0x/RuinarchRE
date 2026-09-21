public class SaveDataSimpleSubGoal : SaveDataSubGoal
{
	public override SubGoal Load()
	{
		return new SimpleSubGoal(this);
	}
}
