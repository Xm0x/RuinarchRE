using System;
using Object_Pools;

public class KillVillagersTask : GoalTask
{
	public int neededKillCount { get; private set; }

	public int currentKillCount { get; private set; }

	public override Type serializedData => typeof(SaveDataKillVillagersWithMonster);

	public KillVillagersTask()
	{
		switch (WorldSettings.Instance.worldSettingsData.mapSettings.mapSize)
		{
		case MAP_SIZE.Small:
			neededKillCount = 10;
			break;
		case MAP_SIZE.Medium:
			neededKillCount = 20;
			break;
		case MAP_SIZE.Large:
			neededKillCount = 20;
			break;
		case MAP_SIZE.Extra_Large:
			neededKillCount = 20;
			break;
		}
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Kill_Villagers_Tooltip");
	}

	public KillVillagersTask(SaveDataKillVillagersWithMonster p_data)
		: base(p_data)
	{
		neededKillCount = p_data.neededKillCount;
		currentKillCount = p_data.currentKillCount;
	}

	public override void StartTask()
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DIED_FROM_PLAYER_SOURCE, OnCharacterDiedFromPlayerSource);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DIED_FROM_PLAYER_SOURCE, OnCharacterDiedFromPlayerSource);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Kill_Villagers_Tooltip");
	}

	private void UpdateTaskName()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Goals", "Goals_Table", "Kill_Villagers_Task");
		log.AddToFillers(null, currentKillCount + "/" + neededKillCount, LOG_IDENTIFIER.STRING_1);
		log.FinalizeText();
		SetTaskName(log.rawText);
		LogPool.Release(log);
	}

	private void OnCharacterDiedFromPlayerSource(Character p_character)
	{
		if (p_character.isNormalCharacter)
		{
			currentKillCount++;
			UpdateTaskName();
			if (currentKillCount >= neededKillCount)
			{
				CompleteTask();
			}
		}
	}
}
