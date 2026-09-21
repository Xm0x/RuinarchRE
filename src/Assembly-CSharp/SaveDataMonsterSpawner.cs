using System.Collections.Generic;

public class SaveDataMonsterSpawner : SaveDataTileObject
{
	public GameDate spawnDate;

	public GameDate chaosOrbsDate;

	public GameDate wildernessMonsterSpawnDate;

	public SUMMON_TYPE monsterType;

	public string monsterClassName;

	public STRUCTURE_TYPE currentStructureType;

	public int minLimit;

	public int maxLimit;

	public string pluralizedMonsterTypeString;

	public List<string> spawnedCharacterIDs;

	public bool stopProcessingOnPlacement;

	public bool shouldProcessWildernessPlacement;

	public bool willSpawnWildernessMonsterAgain;

	public override void Save(TileObject data)
	{
		base.Save(data);
		MonsterSpawner monsterSpawner = data as MonsterSpawner;
		spawnDate = monsterSpawner.spawnDate;
		chaosOrbsDate = monsterSpawner.chaosOrbsDate;
		wildernessMonsterSpawnDate = monsterSpawner.wildernessMonsterSpawnDate;
		monsterType = monsterSpawner.monsterType;
		monsterClassName = monsterSpawner.monsterClassName;
		currentStructureType = monsterSpawner.currentStructureType;
		minLimit = monsterSpawner.minLimit;
		maxLimit = monsterSpawner.maxLimit;
		pluralizedMonsterTypeString = monsterSpawner.pluralizedMonsterTypeString;
		if (monsterSpawner.spawnedCharacterIDs != null && monsterSpawner.spawnedCharacterIDs.Count > 0)
		{
			spawnedCharacterIDs = new List<string>(monsterSpawner.spawnedCharacterIDs);
		}
		stopProcessingOnPlacement = monsterSpawner.stopProcessingOnPlacement;
		shouldProcessWildernessPlacement = monsterSpawner.shouldProcessWildernessPlacement;
		willSpawnWildernessMonsterAgain = monsterSpawner.willSpawnWildernessMonsterAgain;
	}
}
