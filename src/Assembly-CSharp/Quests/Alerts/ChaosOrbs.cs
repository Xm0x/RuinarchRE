using System.Collections;
using Tutorial;
using UtilityScripts;

namespace Quests.Alerts;

public class ChaosOrbs : GameAlert
{
	public ChaosOrbs()
		: base(Game_Alert.Chaos_Orbs)
	{
	}

	public ChaosOrbs(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Chaos_Orbs)
	{
	}

	public override void SetAsSpawned()
	{
		GameManager.Instance.StartCoroutine(WaitToBecomeActive());
	}

	public override void SetAsActive()
	{
		base.SetAsActive();
		SaveManager.Instance.currentSaveDataPlayer.SetTutorialAlertAsDone(base.alertType);
	}

	public override void OnSelectBookmark()
	{
		PlayerUI.Instance.ShowSpecificTutorial(TutorialManager.Tutorial_Type.Chaotic_Energy);
		RemoveBookmark();
	}

	public override void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		base.OnHoverOverBookmarkItem(p_pos);
		UIManager.Instance.ShowSmallInfo(GetLocalizedString(_strGameAlertType + "_Tooltip_1"), p_pos, "", autoReplaceText: false);
	}

	public override void OnHoverOutBookmarkItem()
	{
		base.OnHoverOutBookmarkItem();
		UIManager.Instance.HideSmallInfo();
	}

	private IEnumerator WaitToBecomeActive()
	{
		yield return GameUtilities.waitFor3Seconds;
		while (UIManager.Instance.IsShowingStartScreen())
		{
			yield return null;
		}
		yield return GameUtilities.waitFor2Seconds;
		SetAsActive();
	}
}
