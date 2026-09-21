using System.Collections.Generic;
using Maccima_Games.Util;
using Ruinarch;
using Tutorial;

namespace Quests.Alerts;

public class ReleasePowers : GameAlert
{
	public ReleasePowers()
		: base(Game_Alert.Release_Powers)
	{
	}

	public ReleasePowers(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Release_Powers)
	{
	}

	public override void SetAsSpawned()
	{
		TutorialManager.Instance.AddAlertToGenericAlertPool(this);
	}

	public override void SetAsActive()
	{
		base.SetAsActive();
		SaveManager.Instance.currentSaveDataPlayer.SetTutorialAlertAsDone(base.alertType);
	}

	public override void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		base.OnHoverOverBookmarkItem(p_pos);
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim(1);
		dictionary.Add("shortcutKey", InputManager.Instance.GetShortcutDisplayStringForAction(SHORTCUT_ACTION.Center_Portal));
		string localizedString = GetLocalizedString(_strGameAlertType + "_Tooltip_1", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		UIManager.Instance.ShowSmallInfo(localizedString, p_pos, "", autoReplaceText: false);
	}

	public override void OnHoverOutBookmarkItem()
	{
		base.OnHoverOutBookmarkItem();
		UIManager.Instance.HideSmallInfo();
	}
}
