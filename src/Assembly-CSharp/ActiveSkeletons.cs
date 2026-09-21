using System;
using System.Linq;
using Object_Pools;

public class ActiveSkeletons : GoalTask
{
	public int neededSkeletonCount { get; private set; }

	public override Type serializedData => typeof(SaveDataActiveSkeleton);

	public ActiveSkeletons()
	{
		switch (WorldSettings.Instance.worldSettingsData.mapSettings.mapSize)
		{
		case MAP_SIZE.Small:
			neededSkeletonCount = 8;
			break;
		case MAP_SIZE.Medium:
			neededSkeletonCount = 12;
			break;
		case MAP_SIZE.Large:
			neededSkeletonCount = 12;
			break;
		case MAP_SIZE.Extra_Large:
			neededSkeletonCount = 12;
			break;
		}
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Active_Skeletons_Task_Tooltip");
	}

	public ActiveSkeletons(SaveDataActiveSkeleton p_data)
		: base(p_data)
	{
		neededSkeletonCount = p_data.neededSkeletonCount;
	}

	public override void StartTask()
	{
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
	}

	protected override void EndTask()
	{
		Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
		Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
	}

	protected override void ReevaluateLocalizedTexts()
	{
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Active_Skeletons_Task_Tooltip");
	}

	private void UpdateTaskName()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Goals", "Goals_Table", "Active_Skeletons_Task");
		if (FactionManager.Instance.hasUndeadFaction)
		{
			log.AddToFillers(null, FactionManager.Instance.undeadFaction.characters.Count((Character c) => c is Skeleton && !c.isDead) + "/" + neededSkeletonCount, LOG_IDENTIFIER.STRING_1);
		}
		else
		{
			log.AddToFillers(null, neededSkeletonCount.ToString(), LOG_IDENTIFIER.STRING_1);
		}
		log.FinalizeText();
		SetTaskName(log.rawText);
		LogPool.Release(log);
	}

	private void CheckIfTaskIsComplete()
	{
		if (FactionManager.Instance.undeadFaction.characters.Count((Character c) => c is Skeleton && !c.isDead) >= neededSkeletonCount)
		{
			CompleteTask();
		}
	}

	private void OnCharacterAddedToFaction(Character p_character, Faction p_faction)
	{
		if (p_faction.factionType.type == FACTION_TYPE.Undead && p_character is Summon { summonType: SUMMON_TYPE.Skeleton })
		{
			UpdateTaskName();
			CheckIfTaskIsComplete();
		}
	}

	private void OnCharacterDied(Character p_character)
	{
		if (p_character is Skeleton)
		{
			UpdateTaskName();
		}
	}

	private void OnCharacterRemovedFromFaction(Character p_character, Faction p_faction)
	{
		if (p_faction.factionType.type == FACTION_TYPE.Undead)
		{
			UpdateTaskName();
		}
	}
}
