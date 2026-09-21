public class SaveDataMothman : SaveDataSummon
{
	public string stolenTraitName;

	public override void Save(Character data)
	{
		base.Save(data);
		if (data is Mothman mothman)
		{
			stolenTraitName = mothman.stolenTraitName;
		}
	}
}
