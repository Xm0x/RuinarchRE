using System;
using Object_Pools;

public class SpawnGhosts : GoalTask
{
	public int neededGhostCount { get; private set; }

	public int spawnedGhostCount { get; private set; }

	public override Type serializedData => typeof(SaveDataSpawnGhosts);

	public SpawnGhosts()
	{
		switch (WorldSettings.Instance.worldSettingsData.mapSettings.mapSize)
		{
		case MAP_SIZE.Small:
			neededGhostCount = 3;
			break;
		case MAP_SIZE.Medium:
			neededGhostCount = 5;
			break;
		case MAP_SIZE.Large:
			neededGhostCount = 5;
			break;
		case MAP_SIZE.Extra_Large:
			neededGhostCount = 5;
			break;
		}
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Spawn_Ghosts_Task_Tooltip");
	}

	public SpawnGhosts(SaveDataSpawnGhosts p_data)
		: base(p_data)
	{
		neededGhostCount = p_data.neededGhostCount;
		spawnedGhostCount = p_data.spawnedGhostCount;
	}

	public override void StartTask()
	{
		Messenger.AddListener<Summon>(CharacterSignals.GHOST_SPAWNED_THAT_COUNTS_FOR_TASK, OnSpawnedValidGhost);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Summon>(CharacterSignals.GHOST_SPAWNED_THAT_COUNTS_FOR_TASK, OnSpawnedValidGhost);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Spawn_Ghosts_Task_Tooltip");
	}

	private void UpdateTaskName()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Goals", "Goals_Table", "Spawn_Ghosts_Task");
		log.AddToFillers(null, spawnedGhostCount + "/" + neededGhostCount, LOG_IDENTIFIER.STRING_1);
		log.FinalizeText();
		SetTaskName(log.rawText);
		LogPool.Release(log);
	}

	private void OnSpawnedValidGhost(Summon p_ghost)
	{
		spawnedGhostCount++;
		UpdateTaskName();
		if (spawnedGhostCount >= neededGhostCount)
		{
			CompleteTask();
		}
	}
}
