public class TraitRemoveSchedule
{
	public GameDate removeDate;

	public string ticket;

	public void Initialize()
	{
	}

	public void Reset()
	{
		removeDate = default(GameDate);
		ticket = string.Empty;
	}
}
