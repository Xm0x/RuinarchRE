namespace Inner_Maps.Location_Structures;

public class FieryCave : NaturalStructureWithStructureObject
{
	public FieryCave(Region location)
		: base(STRUCTURE_TYPE.FIERY_CAVE, location)
	{
	}

	public FieryCave(Region location, SaveDataNaturalStructureWithStructureObject data)
		: base(location, data)
	{
	}
}
