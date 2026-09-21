using System;
using System.Collections.Generic;
using System.Linq;
using Maccima_Games.Util;
using Ruinarch;
using UtilityScripts;

namespace Quests;

public class Eradication : VictoryCondition
{
	public override Type serializedData => typeof(SaveDataEradication);

	public Eradication()
		: base(VICTORY_CONDITION.Eradication)
	{
	}

	public Eradication(SaveDataVictoryCondition p_data)
		: base(VICTORY_CONDITION.Eradication, p_data)
	{
	}

	protected override void SubscribeListeners()
	{
		base.SubscribeListeners();
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_REMOVED_FROM_ALIVE_VILLAGERS, OnCharacterRemovedFromAliveVillagers);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_ADDED_TO_ALIVE_VILLAGERS, OnCharacterAddedToAliveVillagers);
		Messenger.AddListener<RELIGION>(CharacterSignals.ACTIVE_RELIGIOUS_CULTISTS_UPDATED, OnActiveReligiousCultistsUpdated);
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterJoinedFaction);
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
		Messenger.AddListener<Faction, Faction, FACTION_RELATIONSHIP_STATUS, FACTION_RELATIONSHIP_STATUS>(FactionSignals.CHANGE_FACTION_RELATIONSHIP, OnFactionChangedRelationshipStatus);
	}

	protected override void UnsubscribeListeners()
	{
		base.UnsubscribeListeners();
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_REMOVED_FROM_ALIVE_VILLAGERS, OnCharacterRemovedFromAliveVillagers);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_ADDED_TO_ALIVE_VILLAGERS, OnCharacterAddedToAliveVillagers);
		Messenger.RemoveListener<RELIGION>(CharacterSignals.ACTIVE_RELIGIOUS_CULTISTS_UPDATED, OnActiveReligiousCultistsUpdated);
		Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterJoinedFaction);
		Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
		Messenger.RemoveListener<Faction, Faction, FACTION_RELATIONSHIP_STATUS, FACTION_RELATIONSHIP_STATUS>(FactionSignals.CHANGE_FACTION_RELATIONSHIP, OnFactionChangedRelationshipStatus);
	}

	protected override string GetWinMessage()
	{
		return LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Eradication_Win_Message");
	}

	private void OnActiveReligiousCultistsUpdated(RELIGION p_religion)
	{
		if (p_religion == RELIGION.Demon_Worship)
		{
			base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
			CheckForGameWin();
		}
	}

	private void OnCharacterAddedToAliveVillagers(Character p_character)
	{
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
	}

	private void OnCharacterRemovedFromAliveVillagers(Character p_character)
	{
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		CheckForGameWin();
	}

	private void OnCharacterRemovedFromFaction(Character p_character, Faction p_faction)
	{
		if (DatabaseManager.Instance.characterDatabase.aliveVillagersList.Contains(p_character))
		{
			base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
			CheckForGameWin();
		}
	}

	private void OnCharacterJoinedFaction(Character p_character, Faction p_faction)
	{
		if (DatabaseManager.Instance.characterDatabase.aliveVillagersList.Contains(p_character))
		{
			base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
			CheckForGameWin();
		}
	}

	private void OnFactionChangedRelationshipStatus(Faction p_faction1, Faction p_faction2, FACTION_RELATIONSHIP_STATUS p_newStatus, FACTION_RELATIONSHIP_STATUS p_oldStatus)
	{
		if ((p_faction1.isPlayerFaction || p_faction2.isPlayerFaction) && p_newStatus == FACTION_RELATIONSHIP_STATUS.Friendly)
		{
			base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
			CheckForGameWin();
		}
	}

	private void CheckForGameWin()
	{
		if (DatabaseManager.Instance.characterDatabase.aliveVillagersList.Count((Character c) => !c.isAlliedWithPlayer) <= 0)
		{
			WinGame();
		}
	}

	protected override void AfterWinGame()
	{
		if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled)
		{
			AchievementManager.Instance.FulfillAchievement(ACHIEVEMENT.BABYARCH);
		}
		else if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Disabled)
		{
			AchievementManager.Instance.FulfillAchievement(ACHIEVEMENT.TEENARCH);
		}
	}

	protected override string GetBookmarkName()
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim(1);
		dictionary.Add("aliveVillagerCount", DatabaseManager.Instance.characterDatabase.aliveVillagersList.Count((Character c) => !c.isAlliedWithPlayer).ToString());
		return LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Eradication_Title", dictionary);
	}

	public override void OnSelectBookmark()
	{
		List<Character> list = RuinarchListPool<Character>.Claim(DatabaseManager.Instance.characterDatabase.aliveVillagersList.Count);
		for (int i = 0; i < DatabaseManager.Instance.characterDatabase.aliveVillagersList.Count; i++)
		{
			Character character = DatabaseManager.Instance.characterDatabase.aliveVillagersList[i];
			if (!character.isAlliedWithPlayer)
			{
				list.Add(character);
			}
		}
		InputManager.Instance.CharacterCenterCycleForward(list);
		RuinarchListPool<Character>.Release(list);
	}
}
