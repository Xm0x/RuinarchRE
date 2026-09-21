namespace Inner_Maps.Location_Structures;

public class RabbitHole : AnimalDen
{
	public RabbitHole(Region location)
		: base(STRUCTURE_TYPE.RABBIT_HOLE, location)
	{
	}

	public RabbitHole(Region location, SaveDataNaturalStructureWithStructureObject data)
		: base(location, data)
	{
	}
}
