using System;
using Object_Pools;

public class TriggerGrudges : GoalTask
{
	public int neededGrudgeCount { get; private set; }

	public int triggeredGrudgeCount { get; private set; }

	public override Type serializedData => typeof(SaveDataTriggerGrudges);

	public TriggerGrudges()
	{
		switch (WorldSettings.Instance.worldSettingsData.mapSettings.mapSize)
		{
		case MAP_SIZE.Small:
			neededGrudgeCount = 1;
			break;
		case MAP_SIZE.Medium:
			neededGrudgeCount = 3;
			break;
		case MAP_SIZE.Large:
			neededGrudgeCount = 3;
			break;
		case MAP_SIZE.Extra_Large:
			neededGrudgeCount = 3;
			break;
		}
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Trigger_Grudges_Task_Tooltip");
	}

	public TriggerGrudges(SaveDataTriggerGrudges p_data)
		: base(p_data)
	{
		neededGrudgeCount = p_data.neededGrudgeCount;
		triggeredGrudgeCount = p_data.triggeredGrudgeCount;
	}

	public override void StartTask()
	{
		Messenger.AddListener<TRIGGER_GRUDGE_ACTION, Character, Character>(PlayerSignals.TRIGGERED_GRUDGE, OnGrudgeTriggered);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<TRIGGER_GRUDGE_ACTION, Character, Character>(PlayerSignals.TRIGGERED_GRUDGE, OnGrudgeTriggered);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Trigger_Grudges_Task_Tooltip");
	}

	private void UpdateTaskName()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Goals", "Goals_Table", "Trigger_Grudges_Task");
		log.AddToFillers(null, triggeredGrudgeCount + "/" + neededGrudgeCount, LOG_IDENTIFIER.STRING_1);
		log.FinalizeText();
		SetTaskName(log.rawText);
		LogPool.Release(log);
	}

	private void OnGrudgeTriggered(TRIGGER_GRUDGE_ACTION p_grudgeAction, Character p_actor, Character p_target)
	{
		triggeredGrudgeCount++;
		UpdateTaskName();
		if (triggeredGrudgeCount >= neededGrudgeCount)
		{
			CompleteTask();
		}
	}
}
