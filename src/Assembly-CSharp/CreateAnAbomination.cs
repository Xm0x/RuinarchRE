using System;

public class CreateAnAbomination : GoalTask
{
	public override Type serializedData => typeof(SaveDataCreateAnAbomination);

	public CreateAnAbomination()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Abomination_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Abomination_Tooltip");
	}

	public CreateAnAbomination(SaveDataCreateAnAbomination p_data)
		: base(p_data)
	{
	}

	public override void StartTask()
	{
		Messenger.AddListener<Summon>(MonsterSignals.ABOMINATION_SPAWNED_FROM_GERM, OnAbominationSpawnedFromGerm);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Summon>(MonsterSignals.ABOMINATION_SPAWNED_FROM_GERM, OnAbominationSpawnedFromGerm);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Abomination_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Abomination_Tooltip");
	}

	private void OnAbominationSpawnedFromGerm(Summon p_summon)
	{
		CompleteTask();
	}
}
