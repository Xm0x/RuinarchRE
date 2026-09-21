using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class SaveDataMageTower : SaveDataManMadeStructure
{
	public List<string> spawnedGolemIDs;

	public string protectorID;

	public override void Save(LocationStructure locationStructure)
	{
		base.Save(locationStructure);
		MageTower mageTower = locationStructure as MageTower;
		spawnedGolemIDs = new List<string>(mageTower.spawnedGolemIDs);
		protectorID = mageTower.protectorID;
	}
}
