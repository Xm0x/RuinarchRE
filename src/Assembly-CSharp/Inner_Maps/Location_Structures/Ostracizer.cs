namespace Inner_Maps.Location_Structures;

public class Ostracizer : DemonicStructure
{
	private bool _isLearnSpellInCooldown;

	private string _cooldownScheduleKey;

	public Ostracizer(Region location)
		: base(STRUCTURE_TYPE.OSTRACIZER, location)
	{
	}

	public Ostracizer(Region location, SaveDataDemonicStructure data)
		: base(location, data)
	{
	}
}
