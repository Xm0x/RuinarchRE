using System.Collections.Generic;

public class SaveDataAnimalBurrow : SaveDataTileObject
{
	public List<string> spawnedMonsters;

	public List<string> deadSpawnedMonsters;

	public override void Save(TileObject data)
	{
		base.Save(data);
		AnimalBurrow animalBurrow = data as AnimalBurrow;
		spawnedMonsters = SaveUtilities.ConvertSavableListToIDs(animalBurrow.spawnedMonsters);
		deadSpawnedMonsters = SaveUtilities.ConvertSavableListToIDs(animalBurrow.deadSpawnedMonsters);
	}
}
