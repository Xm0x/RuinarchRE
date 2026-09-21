using Inner_Maps.Location_Structures;

public class SaveDataThePortal : SaveDataDemonicStructure
{
	public int level;

	public override void Save(LocationStructure locationStructure)
	{
		base.Save(locationStructure);
		if (locationStructure is ThePortal thePortal)
		{
			level = thePortal.level;
		}
	}
}
