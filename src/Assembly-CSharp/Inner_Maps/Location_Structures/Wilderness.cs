namespace Inner_Maps.Location_Structures;

public class Wilderness : NaturalStructure
{
	public Wilderness(Region location)
		: base(STRUCTURE_TYPE.WILDERNESS, location)
	{
	}

	public Wilderness(Region location, SaveDataNaturalStructure data)
		: base(location, data)
	{
	}

	public override void CenterOnStructure()
	{
	}

	public override void ShowSelectorOnStructure()
	{
	}

	protected override string GetUIString()
	{
		if (string.IsNullOrEmpty(_uiString))
		{
			_uiString = base.name;
		}
		return _uiString;
	}
}
