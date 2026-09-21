using System;
using Object_Pools;

public class RecruitDemonCultists : GoalTask
{
	public int neededDemonCultistCount { get; private set; }

	public int recruitedDemonCultistCount { get; private set; }

	public override Type serializedData => typeof(SaveDataRecruitDemonCultists);

	public RecruitDemonCultists()
	{
		switch (WorldSettings.Instance.worldSettingsData.mapSettings.mapSize)
		{
		case MAP_SIZE.Small:
			neededDemonCultistCount = 5;
			break;
		case MAP_SIZE.Medium:
			neededDemonCultistCount = 10;
			break;
		case MAP_SIZE.Large:
			neededDemonCultistCount = 10;
			break;
		case MAP_SIZE.Extra_Large:
			neededDemonCultistCount = 10;
			break;
		}
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Recruit_Demon_Cultists_Task_Tooltip");
	}

	public RecruitDemonCultists(SaveDataRecruitDemonCultists p_data)
		: base(p_data)
	{
		neededDemonCultistCount = p_data.neededDemonCultistCount;
		recruitedDemonCultistCount = p_data.recruitedDemonCultistCount;
	}

	public override void StartTask()
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOME_DEMON_CULTIST, OnCharacterBecameDemonCultist);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_BECOME_DEMON_CULTIST, OnCharacterBecameDemonCultist);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Recruit_Demon_Cultists_Task_Tooltip");
	}

	private void UpdateTaskName()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Goals", "Goals_Table", "Recruit_Demon_Cultists_Task");
		log.AddToFillers(null, recruitedDemonCultistCount + "/" + neededDemonCultistCount, LOG_IDENTIFIER.STRING_1);
		log.FinalizeText();
		SetTaskName(log.rawText);
		LogPool.Release(log);
	}

	private void OnCharacterBecameDemonCultist(Character p_character)
	{
		recruitedDemonCultistCount++;
		UpdateTaskName();
		if (recruitedDemonCultistCount >= neededDemonCultistCount)
		{
			CompleteTask();
		}
	}
}
