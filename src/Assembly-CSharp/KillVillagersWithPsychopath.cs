using System;
using Object_Pools;

public class KillVillagersWithPsychopath : GoalTask
{
	public int neededKillCount { get; private set; }

	public int triggeredKillCount { get; private set; }

	public override Type serializedData => typeof(SaveDataKillVillagersWithPsychopath);

	public KillVillagersWithPsychopath()
	{
		switch (WorldSettings.Instance.worldSettingsData.mapSettings.mapSize)
		{
		case MAP_SIZE.Small:
			neededKillCount = 3;
			break;
		case MAP_SIZE.Medium:
			neededKillCount = 3;
			break;
		case MAP_SIZE.Large:
			neededKillCount = 3;
			break;
		case MAP_SIZE.Extra_Large:
			neededKillCount = 3;
			break;
		}
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Kill_Villagers_Psychopath_Task_Tooltip");
	}

	public KillVillagersWithPsychopath(SaveDataKillVillagersWithPsychopath p_data)
		: base(p_data)
	{
		neededKillCount = p_data.neededKillCount;
		triggeredKillCount = p_data.triggeredKillCount;
	}

	public override void StartTask()
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Kill_Villagers_Psychopath_Task_Tooltip");
	}

	private void UpdateTaskName()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Goals", "Goals_Table", "Kill_Villagers_Psychopath_Task");
		log.AddToFillers(null, triggeredKillCount + "/" + neededKillCount, LOG_IDENTIFIER.STRING_1);
		log.FinalizeText();
		SetTaskName(log.rawText);
		LogPool.Release(log);
	}

	private void OnCharacterDied(Character p_character)
	{
		if (p_character.causeOfDeath == INTERACTION_TYPE.RITUAL_KILLING)
		{
			triggeredKillCount++;
			UpdateTaskName();
			if (triggeredKillCount >= neededKillCount)
			{
				CompleteTask();
			}
		}
	}
}
