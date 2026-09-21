using System;
using Object_Pools;

public class TriggerCriticalBreaks : GoalTask
{
	public int neededCriticalBreakCount { get; private set; }

	public int triggeredCriticalBreakCount { get; private set; }

	public override Type serializedData => typeof(SaveDataTriggerCriticalBreaks);

	public TriggerCriticalBreaks()
	{
		switch (WorldSettings.Instance.worldSettingsData.mapSettings.mapSize)
		{
		case MAP_SIZE.Small:
			neededCriticalBreakCount = 1;
			break;
		case MAP_SIZE.Medium:
			neededCriticalBreakCount = 3;
			break;
		case MAP_SIZE.Large:
			neededCriticalBreakCount = 3;
			break;
		case MAP_SIZE.Extra_Large:
			neededCriticalBreakCount = 3;
			break;
		}
		triggeredCriticalBreakCount = 0;
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Trigger_Critical_Breaks_Task_Tooltip");
	}

	public TriggerCriticalBreaks(SaveDataTriggerCriticalBreaks p_data)
		: base(p_data)
	{
		neededCriticalBreakCount = p_data.neededCriticalBreakCount;
		triggeredCriticalBreakCount = p_data.triggeredCriticalBreakCount;
	}

	public override void StartTask()
	{
		Messenger.AddListener<Character>(PlayerSignals.CRITICAL_BREAK_TRIGGERED, OnCriticalBreakTriggered);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Character>(PlayerSignals.CRITICAL_BREAK_TRIGGERED, OnCriticalBreakTriggered);
	}

	private void OnCriticalBreakTriggered(Character p_actor)
	{
		triggeredCriticalBreakCount++;
		UpdateTaskName();
		CheckForCompletion();
	}

	protected override void ReevaluateLocalizedTexts()
	{
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Trigger_Critical_Breaks_Task_Tooltip");
	}

	private void UpdateTaskName()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Goals", "Goals_Table", "Trigger_Critical_Breaks_Task");
		log.AddToFillers(null, triggeredCriticalBreakCount + "/" + neededCriticalBreakCount, LOG_IDENTIFIER.STRING_1);
		log.FinalizeText();
		SetTaskName(log.rawText);
		LogPool.Release(log);
	}

	private void CheckForCompletion()
	{
		if (triggeredCriticalBreakCount >= neededCriticalBreakCount)
		{
			CompleteTask();
		}
	}
}
