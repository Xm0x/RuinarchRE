public class SaveDataWyvernling : SaveDataSummon
{
	public GameDate growUpDate;

	public bool shouldGrowUpOnUnSeize;

	public override void Save(Character data)
	{
		base.Save(data);
		if (data is Wyvernling wyvernling)
		{
			growUpDate = wyvernling.growUpDate;
			shouldGrowUpOnUnSeize = wyvernling.shouldGrowUpOnUnSeize;
		}
	}
}
