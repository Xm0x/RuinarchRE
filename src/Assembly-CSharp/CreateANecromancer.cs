using System;

public class CreateANecromancer : GoalTask
{
	public override Type serializedData => typeof(SaveDataCreateANecromancer);

	public CreateANecromancer()
	{
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Necromancer_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Necromancer_Task_Tooltip");
	}

	public CreateANecromancer(SaveDataCreateANecromancer p_data)
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
		base.taskName = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Necromancer_Task");
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Necromancer_Task_Tooltip");
	}

	private void OnCharacterChangedClass(Character p_character, CharacterClass p_oldClass, CharacterClass p_newClass)
	{
		if (p_newClass.className == "Necromancer")
		{
			CompleteTask();
		}
	}
}
