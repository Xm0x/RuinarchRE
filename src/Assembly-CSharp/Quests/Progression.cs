using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;

namespace Quests;

public class Progression : VictoryCondition
{
	public override Type serializedData => typeof(SaveDataProgression);

	public Progression()
		: base(VICTORY_CONDITION.Progression)
	{
	}

	public Progression(SaveDataVictoryCondition p_data)
		: base(VICTORY_CONDITION.Progression, p_data)
	{
	}

	protected override void SubscribeListeners()
	{
		base.SubscribeListeners();
		Messenger.AddListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPortalUpgradeFinished);
	}

	protected override void UnsubscribeListeners()
	{
		base.UnsubscribeListeners();
		Messenger.RemoveListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPortalUpgradeFinished);
	}

	protected override string GetWinMessage()
	{
		return LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Progression_Win_Message");
	}

	private void OnPortalUpgradeFinished(int p_portalLevel)
	{
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		if (PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) is ThePortal thePortal && thePortal.IsMaxLevel())
		{
			WinGame();
		}
	}

	protected override void AfterWinGame()
	{
		AchievementManager.Instance.FulfillAchievement(ACHIEVEMENT.ADULTARCH);
	}

	protected override string GetBookmarkName()
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim(2);
		ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		int key = PlayerManager.Instance.player.playerSkillComponent.portalUpgradeItems.Last().Key;
		dictionary.Add("currentLevel", thePortal.level.ToString());
		dictionary.Add("maxLevel", key.ToString());
		return LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Progression_Title", dictionary);
	}

	public override void OnSelectBookmark()
	{
		ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		UIManager.Instance.ShowUpgradePortalUI(portal);
		AkSoundEngine.PostEvent("Play_Release_Powers", InnerMapCameraMove.Instance.gameObject);
	}
}
