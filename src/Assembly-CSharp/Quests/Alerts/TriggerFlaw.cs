using System.Collections.Generic;
using Maccima_Games.Util;
using Ruinarch;
using Tutorial;

namespace Quests.Alerts;

public class TriggerFlaw : GameAlert
{
	public TriggerFlaw()
		: base(Game_Alert.Trigger_Flaw)
	{
	}

	public TriggerFlaw(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Trigger_Flaw)
	{
	}

	public override void SetAsSpawned()
	{
		if (PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.TRIGGER_FLAW).charges > 0)
		{
			AlertValid();
		}
		Messenger.AddListener<SkillData, int>(PlayerSkillSignals.CHARGES_UPDATED, OnChargesAdjusted);
	}

	public override void SetAsActive()
	{
		base.SetAsActive();
		SaveManager.Instance.currentSaveDataPlayer.SetTutorialAlertAsDone(base.alertType);
		Messenger.RemoveListener<SkillData, int>(PlayerSkillSignals.CHARGES_UPDATED, OnChargesAdjusted);
	}

	private void AlertValid()
	{
		TutorialManager.Instance.AddAlertToGenericAlertPool(this);
	}

	private void AlertInvalid()
	{
		TutorialManager.Instance.RemoveFromGenericAlertPool(this);
	}

	public override void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		base.OnHoverOverBookmarkItem(p_pos);
		if (InputManager.Instance.isUsingGamepad)
		{
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim(1);
			dictionary.Add("shortcutKey", InputManager.Instance.GetShortcutDisplayStringForAction(SHORTCUT_ACTION.Right_Click));
			UIManager.Instance.ShowSmallInfo(GetLocalizedString(_strGameAlertType + "_Tooltip_1_Controller", dictionary), p_pos, "", autoReplaceText: false);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
		}
		else
		{
			UIManager.Instance.ShowSmallInfo(GetLocalizedString(_strGameAlertType + "_Tooltip_1"), p_pos, "", autoReplaceText: false);
		}
	}

	public override void OnHoverOutBookmarkItem()
	{
		base.OnHoverOutBookmarkItem();
		UIManager.Instance.HideSmallInfo();
	}

	private void OnChargesAdjusted(SkillData p_skillData, int p_amount)
	{
		if (p_skillData.type == PLAYER_SKILL_TYPE.TRIGGER_FLAW)
		{
			if (p_skillData.charges > 0)
			{
				AlertValid();
			}
			else
			{
				AlertInvalid();
			}
		}
	}
}
