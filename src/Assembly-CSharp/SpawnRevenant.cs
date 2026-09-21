using System;

public class SpawnRevenant : GoalTask
{
	public override Type serializedData => typeof(SaveDataSpawnRevenant);

	public SpawnRevenant()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Spawn_Revenant_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Spawn_Revenant_Task_Tooltip");
	}

	public SpawnRevenant(SaveDataSpawnRevenant p_data)
		: base(p_data)
	{
	}

	public override void StartTask()
	{
		Messenger.AddListener<Revenant>(CharacterSignals.REVENANT_SPAWNED_THAT_COUNTS_FOR_TASK, OnSpawnedValidRevenant);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Revenant>(CharacterSignals.REVENANT_SPAWNED_THAT_COUNTS_FOR_TASK, OnSpawnedValidRevenant);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Spawn_Revenant_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Spawn_Revenant_Task_Tooltip");
	}

	private void OnSpawnedValidRevenant(Revenant p_revenant)
	{
		CompleteTask();
	}
}
