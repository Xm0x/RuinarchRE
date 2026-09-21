using System;

public class CreateDemonCultLeader : GoalTask
{
	public override Type serializedData => typeof(SaveDataCreateCultLeader);

	public CreateDemonCultLeader()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Cult_Leader_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Cult_Leader_Task_Tooltip");
	}

	public CreateDemonCultLeader(SaveDataCreateCultLeader p_data)
		: base(p_data)
	{
	}

	public override void StartTask()
	{
		Messenger.AddListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterChangedClass);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterChangedClass);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Cult_Leader_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Cult_Leader_Task_Tooltip");
	}

	private void OnCharacterChangedClass(Character p_character, CharacterClass p_previousClass, CharacterClass p_newClass)
	{
		if (p_newClass.className == "Demon Cult Leader")
		{
			CompleteTask();
		}
	}
}
