public class SaveDataCreaturesOfTheNight : SaveDataGoal
{
	public override Goal Load()
	{
		return new CreaturesOfTheNight(this);
	}
}
