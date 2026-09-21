namespace Inner_Maps.Location_Structures;

public class Prism : DemonicStructure
{
	public Prism(Region location)
		: base(STRUCTURE_TYPE.PRISM, location)
	{
		SetMaxHPAndReset(2000);
	}

	public Prism(Region location, SaveDataDemonicStructure data)
		: base(location, data)
	{
	}
}
