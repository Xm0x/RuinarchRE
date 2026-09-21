using System;

namespace Inner_Maps.Location_Structures;

public class Biolab : DemonicStructure
{
	public GameDate replenishDate { get; private set; }

	public override Type serializedData => typeof(SaveDataBiolab);

	public Biolab(Region location)
		: base(STRUCTURE_TYPE.BIOLAB, location)
	{
		SetMaxHPAndReset(5000);
	}

	public Biolab(Region location, SaveDataBiolab data)
		: base(location, data)
	{
	}
}
