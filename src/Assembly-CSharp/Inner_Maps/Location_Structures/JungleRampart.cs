namespace Inner_Maps.Location_Structures;

public class JungleRampart : ManMadeStructure
{
	public JungleRampart(Region location)
		: base(STRUCTURE_TYPE.JUNGLE_RAMPART, location)
	{
		SetMaxHPAndReset(6000);
	}

	public JungleRampart(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		SetMaxHP(6000);
	}
}
