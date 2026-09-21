using System.Collections.Generic;
using Maccima_Games.Util;
using Ruinarch;
using Tutorial;

namespace Quests.Alerts;

public class ApplyAffliction : GameAlert
{
	private string _strTooltip;

	public ApplyAffliction()
		: base(Game_Alert.Apply_Affliction)
	{
		ConstructTooltip();
	}

	public ApplyAffliction(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Apply_Affliction)
	{
		ConstructTooltip();
	}

	public override void SetAsSpawned()
	{
		if (HasAfflictionWithCharge())
		{
			AlertValid();
		}
		Messenger.AddListener<SkillData, int>(PlayerSkillSignals.CHARGES_UPDATED, OnChargesAdjusted);
	}

	public override void SetAsActive()
	{
		base.SetAsActive();
		SaveManager.Instance.currentSaveDataPlayer.SetTutorialAlertAsDone(base.alertType);
	}

	protected override void SetAsCleared()
	{
		base.SetAsCleared();
		Messenger.RemoveListener<SkillData, int>(PlayerSkillSignals.CHARGES_UPDATED, OnChargesAdjusted);
	}

	public override void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		base.OnHoverOverBookmarkItem(p_pos);
		UIManager.Instance.ShowSmallInfo(_strTooltip, p_pos, "", autoReplaceText: false);
	}

	public override void OnHoverOutBookmarkItem()
	{
		base.OnHoverOutBookmarkItem();
		UIManager.Instance.HideSmallInfo();
	}

	private void OnChargesAdjusted(SkillData p_skillData, int p_amount)
	{
		if (p_skillData.category == PLAYER_SKILL_CATEGORY.AFFLICTION)
		{
			if (p_skillData.charges > 0)
			{
				AlertValid();
			}
			else if (HasAfflictionWithCharge())
			{
				AlertValid();
			}
			else
			{
				AlertInvalid();
			}
		}
	}

	protected override void OnControlDeviceChanged(string p_device)
	{
		ConstructTooltip();
	}

	private void ConstructTooltip()
	{
		if (InputManager.Instance.isUsingGamepad)
		{
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim(1);
			dictionary.Add("shortcutKey", InputManager.Instance.GetShortcutDisplayStringForAction(SHORTCUT_ACTION.Right_Click));
			_strTooltip = GetLocalizedString(_strGameAlertType + "_Tooltip_1_Controller", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
		}
		else
		{
			_strTooltip = GetLocalizedString(_strGameAlertType + "_Tooltip_1");
		}
	}

	private void AlertValid()
	{
		TutorialManager.Instance.AddAlertToGenericAlertPool(this);
	}

	private void AlertInvalid()
	{
		TutorialManager.Instance.RemoveFromGenericAlertPool(this);
	}

	private bool HasAfflictionWithCharge()
	{
		for (int i = 0; i < PlayerSkillManager.Instance.allAfflictions.Length; i++)
		{
			PLAYER_SKILL_TYPE type = PlayerSkillManager.Instance.allAfflictions[i];
			if (PlayerSkillManager.Instance.GetSkillData(type).charges > 0)
			{
				return true;
			}
		}
		return false;
	}
}
