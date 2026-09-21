using Inner_Maps.Location_Structures;

public class SaveDataMine : SaveDataManMadeStructure
{
	public string connectedCaveID;

	public override void Save(LocationStructure locationStructure)
	{
		base.Save(locationStructure);
		Mine mine = locationStructure as Mine;
		if (mine.connectedCave != null)
		{
			connectedCaveID = mine.connectedCave.persistentID;
		}
	}
}
