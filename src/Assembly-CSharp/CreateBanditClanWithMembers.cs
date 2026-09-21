using System;
using Object_Pools;

public class CreateBanditClanWithMembers : GoalTask
{
	public int neededBanditClanMembers { get; private set; }

	public override Type serializedData => typeof(SaveDataCreateBanditClanWithMembers);

	public CreateBanditClanWithMembers()
	{
		switch (WorldSettings.Instance.worldSettingsData.mapSettings.mapSize)
		{
		case MAP_SIZE.Small:
			neededBanditClanMembers = 6;
			break;
		case MAP_SIZE.Medium:
			neededBanditClanMembers = 6;
			break;
		case MAP_SIZE.Large:
			neededBanditClanMembers = 6;
			break;
		case MAP_SIZE.Extra_Large:
			neededBanditClanMembers = 6;
			break;
		}
		UpdateTaskName();
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Bandit_Clan_Tooltip");
	}

	public CreateBanditClanWithMembers(SaveDataCreateBanditClanWithMembers p_data)
		: base(p_data)
	{
		neededBanditClanMembers = p_data.neededBanditClanMembers;
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
		base.taskTooltip = LocalizationManager.Instance.GetLocalizedValue("Goals_Table", "Create_Bandit_Clan_Tooltip");
	}

	private void UpdateTaskName()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Goals", "Goals_Table", "Create_Bandit_Clan_Task");
		if (FactionManager.Instance.banditFaction != null)
		{
			log.AddToFillers(null, FactionManager.Instance.banditFaction.GetAliveMembersCount() + "/" + neededBanditClanMembers, LOG_IDENTIFIER.STRING_1);
		}
		else
		{
			log.AddToFillers(null, "0/" + neededBanditClanMembers, LOG_IDENTIFIER.STRING_1);
		}
		log.FinalizeText();
		SetTaskName(log.rawText);
		LogPool.Release(log);
	}

	private void CheckIfTaskIsComplete()
	{
		if (FactionManager.Instance.banditFaction.GetAliveMembersCount() >= neededBanditClanMembers)
		{
			CompleteTask();
		}
	}

	private void OnCharacterAddedToFaction(Character p_character, Faction p_faction)
	{
		if (p_faction.factionType.type == FACTION_TYPE.Bandits)
		{
			UpdateTaskName();
			CheckIfTaskIsComplete();
		}
	}

	private void OnCharacterDied(Character p_character)
	{
		if ((p_character.prevFaction != null && p_character.prevFaction.factionType.type == FACTION_TYPE.Bandits) || (p_character.faction != null && p_character.faction.factionType.type == FACTION_TYPE.Bandits))
		{
			UpdateTaskName();
		}
	}

	private void OnCharacterRemovedFromFaction(Character p_character, Faction p_faction)
	{
		if (p_faction.factionType.type == FACTION_TYPE.Bandits)
		{
			UpdateTaskName();
		}
	}
}
