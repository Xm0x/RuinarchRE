using Inner_Maps.Location_Structures;

public class RegionDatabase
{
	public Region mainRegion { get; private set; }

	public void RegisterRegion(Region region)
	{
		mainRegion = region;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		mainRegion.CheckIfStructureIsStillReferenced(p_structure);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		mainRegion.CheckIfCharacterIsStillReferenced(p_character);
	}
}
