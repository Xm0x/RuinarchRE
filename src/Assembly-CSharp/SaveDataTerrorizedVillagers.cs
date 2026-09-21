public class SaveDataTerrorizedVillagers : SaveDataGoal
{
	public override Goal Load()
	{
		return new TerrorizedVillagers(this);
	}
}
