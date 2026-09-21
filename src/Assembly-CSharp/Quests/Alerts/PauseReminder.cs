using System.Collections.Generic;
using Maccima_Games.Util;
using Ruinarch;
using Tutorial;

namespace Quests.Alerts;

public class PauseReminder : GameAlert
{
	public PauseReminder()
		: base(Game_Alert.Pause_Reminder)
	{
	}

	public PauseReminder(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Pause_Reminder)
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
		dictionary.Add("shortcutKey", InputManager.Instance.isUsingGamepad ? (InputManager.Instance.GetShortcutDisplayStringForAction(SHORTCUT_ACTION.Decrease_Speed) + "/" + InputManager.Instance.GetShortcutDisplayStringForAction(SHORTCUT_ACTION.Increase_Speed)) : InputManager.Instance.GetShortcutDisplayStringForAction(SHORTCUT_ACTION.Toggle_Pause));
		string localizedString = GetLocalizedString(_strGameAlertType + "_Tooltip_1", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		UIManager.Instance.ShowSmallInfo(localizedString, p_pos, "", autoReplaceText: false);
	}

	public override void OnHoverOutBookmarkItem()
	{
		base.OnHoverOutBookmarkItem();
		UIManager.Instance.HideSmallInfo();
	}

	public override void OnSelectBookmark()
	{
		PlayerUI.Instance.ShowSpecificTutorial(TutorialManager.Tutorial_Type.Time_Management);
		RemoveBookmark();
	}
}
