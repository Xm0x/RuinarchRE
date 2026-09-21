using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class SaveDataLichGraveyard : SaveDataManMadeStructure
{
	public List<string> spawnedMonstersID;

	public int spawnRate;

	public GameDate spawnDueDate;

	public int maxCapacity;

	public string factionID;

	public int limit;

	public override void Save(LocationStructure structure)
	{
		base.Save(structure);
		LichGraveyard lichGraveyard = structure as LichGraveyard;
		spawnedMonstersID = new List<string>(lichGraveyard.spawnedMonstersID);
		spawnRate = lichGraveyard.spawnRate;
		spawnDueDate = lichGraveyard.spawnDueDate;
		maxCapacity = lichGraveyard.maxCapacity;
		factionID = lichGraveyard.factionID;
		limit = lichGraveyard.limit;
	}
}
