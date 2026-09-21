public class SaveDataOutbreak : SaveDataGoal
{
	public override Goal Load()
	{
		return new Outbreak(this);
	}
}
