using System;

[Serializable]
public class SaveDataSmallSpider : SaveDataSkinnableAnimal
{
	public GameDate growUpDate;

	public bool shouldGrowUpOnUnSeize;

	public override void Save(Character data)
	{
		base.Save(data);
		if (data is SmallSpider smallSpider)
		{
			growUpDate = smallSpider.growUpDate;
			shouldGrowUpOnUnSeize = smallSpider.shouldGrowUpOnUnSeize;
		}
	}
}
