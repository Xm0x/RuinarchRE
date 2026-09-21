public class SaveDataUndeadSupremacy : SaveDataGoal
{
	public override Goal Load()
	{
		return new UndeadSupremacy(this);
	}
}
