using System;
using Object_Pools;
using Traits;

public class SpreadPlagueToMonsters : GoalTask
{
	public int neededSpreadPlagueCount { get; private set; }

	public int currentSpreadPlagueCount { get; private set; }

	public override Type serializedData => typeof(SaveDataSpreadPlagueToMonsters);

	public SpreadPlagueToMonsters()
	{
		switch (WorldSettings.Instance.worldSettingsData.mapSettings.mapSize)
		{
		case MAP_SIZE.Small:
			neededSpreadPlagueCount = 5;
			break;
		case MAP_SIZE.Medium:
			neededSpreadPlagueCount = 10;
			break;
		case MAP_SIZE.Large:
			neededSpreadPlagueCount = 10;
			break;
		case MAP_SIZE.Extra_Large:
			neededSpreadPlagueCount = 10;
			break;
		}
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Spread_Plague_Task_Tooltip");
	}

	public SpreadPlagueToMonsters(SaveDataSpreadPlagueToMonsters p_data)
		: base(p_data)
	{
		neededSpreadPlagueCount = p_data.neededSpreadPlagueCount;
		currentSpreadPlagueCount = p_data.currentSpreadPlagueCount;
	}

	public override void StartTask()
	{
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Spread_Plague_Task_Tooltip");
	}

	private void UpdateTaskName()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Goals", "Goals_Table", "Spread_Plague_Monsters_Task");
		log.AddToFillers(null, currentSpreadPlagueCount + "/" + neededSpreadPlagueCount, LOG_IDENTIFIER.STRING_1);
		log.FinalizeText();
		SetTaskName(log.rawText);
		LogPool.Release(log);
	}

	private void OnCharacterGainedTrait(Character p_character, Trait p_trait)
	{
		if (p_trait is Plagued && p_character is Summon { summonType: not SUMMON_TYPE.Rat } && (p_character.faction == null || p_character.faction.factionType.type != FACTION_TYPE.Demons))
		{
			currentSpreadPlagueCount++;
			UpdateTaskName();
			if (currentSpreadPlagueCount >= neededSpreadPlagueCount)
			{
				CompleteTask();
			}
		}
	}
}
