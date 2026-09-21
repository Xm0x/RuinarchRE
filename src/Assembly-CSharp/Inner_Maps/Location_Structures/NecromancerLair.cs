namespace Inner_Maps.Location_Structures;

public class NecromancerLair : ManMadeStructure
{
	public NecromancerLair(Region location)
		: base(STRUCTURE_TYPE.NECROMANCER_LAIR, location)
	{
	}

	public NecromancerLair(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
	}
}
