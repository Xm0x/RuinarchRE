namespace Quests.Alerts;

public class Retaliation : GameAlert
{
	public Retaliation()
		: base(Game_Alert.Retaliation)
	{
	}

	public Retaliation(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Retaliation)
	{
	}

	public override void SetAsSpawned()
	{
		Messenger.AddListener<int>(PlayerSignals.RETALIATION_INCREASED, OnRetaliationCounterIncreased);
	}

	public override void SetAsActive()
	{
		base.SetAsActive();
		SaveManager.Instance.currentSaveDataPlayer.SetTutorialAlertAsDone(base.alertType);
		Messenger.RemoveListener<int>(PlayerSignals.RETALIATION_INCREASED, OnRetaliationCounterIncreased);
	}

	private void OnRetaliationCounterIncreased(int p_retaliationCounter)
	{
		SetAsActive();
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
}
