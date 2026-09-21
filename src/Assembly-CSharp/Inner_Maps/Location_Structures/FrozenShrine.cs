namespace Inner_Maps.Location_Structures;

public class FrozenShrine : ManMadeStructure
{
	public FrozenShrine(Region location)
		: base(STRUCTURE_TYPE.FROZEN_SHRINE, location)
	{
		SetMaxHPAndReset(6000);
	}

	public FrozenShrine(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		SetMaxHP(6000);
	}
}
