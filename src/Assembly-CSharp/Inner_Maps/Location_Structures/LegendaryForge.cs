namespace Inner_Maps.Location_Structures;

public class LegendaryForge : NaturalStructureWithStructureObject
{
	public LegendaryForge(Region location)
		: base(STRUCTURE_TYPE.LEGENDARY_FORGE, location)
	{
		SetMaxHPAndReset(6000);
	}

	public LegendaryForge(Region location, SaveDataNaturalStructureWithStructureObject data)
		: base(location, data)
	{
		SetMaxHP(6000);
	}
}
