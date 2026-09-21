using System;

public class CreateLycanWithPelt : GoalTask
{
	public override Type serializedData => typeof(SaveDataCreateLycanWithPelt);

	public CreateLycanWithPelt()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Lycan_With_Pelt_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Lycan_With_Pelt_Task_Tooltip");
	}

	public CreateLycanWithPelt(SaveDataCreateLycanWithPelt p_data)
		: base(p_data)
	{
	}

	public override void StartTask()
	{
		Messenger.AddListener<Character>(CharacterSignals.BECAME_WEREWOLF_VIA_PELT, OnCharacterBecameWerewolfViaPelt);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.BECAME_WEREWOLF_VIA_PELT, OnCharacterBecameWerewolfViaPelt);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Lycan_With_Pelt_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Lycan_With_Pelt_Task_Tooltip");
	}

	private void OnCharacterBecameWerewolfViaPelt(Character p_character)
	{
		CompleteTask();
	}
}
