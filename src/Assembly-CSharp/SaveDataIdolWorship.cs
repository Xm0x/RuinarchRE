public class SaveDataIdolWorship : SaveDataGoal
{
	public override Goal Load()
	{
		return new IdolWorship(this);
	}
}
