public class SaveDataDeathAndDestruction : SaveDataGoal
{
	public override Goal Load()
	{
		return new DeathAndDestruction(this);
	}
}
