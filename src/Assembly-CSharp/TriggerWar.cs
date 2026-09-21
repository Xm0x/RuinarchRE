using System;

public class TriggerWar : GoalTask
{
	public override Type serializedData => typeof(SaveDataTriggerWar);

	public TriggerWar()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Trigger_War_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Trigger_War_Task_Tooltip");
	}

	public TriggerWar(SaveDataTriggerWar p_data)
		: base(p_data)
	{
	}

	public override void StartTask()
	{
		Messenger.AddListener<Faction, Faction>(FactionSignals.WAR_DECLARED, OnWarDeclared);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Faction, Faction>(FactionSignals.WAR_DECLARED, OnWarDeclared);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Trigger_War_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Trigger_War_Task_Tooltip");
	}

	private void OnWarDeclared(Faction p_faction1, Faction p_faction2)
	{
		CompleteTask();
	}
}
