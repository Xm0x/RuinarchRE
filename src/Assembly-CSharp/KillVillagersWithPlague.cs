using System;
using Object_Pools;
using Plague.Fatality;

public class KillVillagersWithPlague : GoalTask
{
	public int neededPlagueKillCount { get; private set; }

	public int currentPlagueKillCount { get; private set; }

	public override Type serializedData => typeof(SaveDataKillVillagersWithPlague);

	public KillVillagersWithPlague()
	{
		switch (WorldSettings.Instance.worldSettingsData.mapSettings.mapSize)
		{
		case MAP_SIZE.Small:
			neededPlagueKillCount = 5;
			break;
		case MAP_SIZE.Medium:
			neededPlagueKillCount = 10;
			break;
		case MAP_SIZE.Large:
			neededPlagueKillCount = 10;
			break;
		case MAP_SIZE.Extra_Large:
			neededPlagueKillCount = 10;
			break;
		}
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Kill_Villagers_Plague_Tooltip");
	}

	public KillVillagersWithPlague(SaveDataKillVillagersWithPlague p_data)
		: base(p_data)
	{
		neededPlagueKillCount = p_data.neededPlagueKillCount;
		currentPlagueKillCount = p_data.currentPlagueKillCount;
	}

	public override void StartTask()
	{
		Messenger.AddListener<Fatality, Character>(CharacterSignals.PLAGUE_FATALITY_ACTIVATED, OnPlagueFatalityActivated);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Fatality, Character>(CharacterSignals.PLAGUE_FATALITY_ACTIVATED, OnPlagueFatalityActivated);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Kill_Villagers_Plague_Tooltip");
	}

	private void UpdateTaskName()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Goals", "Goals_Table", "Kill_Villagers_Plague_Task");
		log.AddToFillers(null, currentPlagueKillCount + "/" + neededPlagueKillCount, LOG_IDENTIFIER.STRING_1);
		log.FinalizeText();
		SetTaskName(log.rawText);
		LogPool.Release(log);
	}

	private void OnPlagueFatalityActivated(Fatality p_fatality, Character p_character)
	{
		currentPlagueKillCount++;
		UpdateTaskName();
		if (currentPlagueKillCount >= neededPlagueKillCount)
		{
			CompleteTask();
		}
	}
}
