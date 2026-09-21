using System;
using TMPro;
using UnityEngine;
using UtilityScripts;

public class UnlockMinionItemUI : MonoBehaviour
{
	public static Action<PLAYER_SKILL_TYPE, int> onClickUnlockMinion;

	[SerializeField]
	private CharacterPortrait _portrait;

	[SerializeField]
	private TextMeshProUGUI lblName;

	[SerializeField]
	private TextMeshProUGUI lblCosts;

	[SerializeField]
	private GameObject goCheckMark;

	[SerializeField]
	private GameObject goPortraitCover;

	private PLAYER_SKILL_TYPE _minionType;

	private PlayerSkillData _playerSkillData;

	public void SetMinionType(PLAYER_SKILL_TYPE p_type)
	{
		_minionType = p_type;
		string s = p_type.ToStringEnum().Remove(0, 6);
		lblName.text = Utilities.NormalizeStringUpperCaseFirstLetterOnly(s);
		_playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_type);
		MinionPlayerSkill minionPlayerSkillData = PlayerSkillManager.Instance.GetMinionPlayerSkillData(p_type);
		lblCosts.text = $"{_playerSkillData.GetUnlockCost()}{Utilities.ManaIcon()}";
		_portrait.GeneratePortrait(CharacterManager.Instance.GeneratePortraitSettings(RACE.DEMON, minionPlayerSkillData.className));
		_portrait.AddPointerClickAction(OnClickMinionItem);
	}

	public void UpdateSelectableState()
	{
		if (PlayerSkillManager.Instance.GetSkillData(_minionType).isInUse)
		{
			SetCheckmarkState(p_state: true);
			SetCoverState(p_state: true);
		}
		else
		{
			SetCheckmarkState(p_state: false);
			SetCoverState(PlayerManager.Instance.player.currenciesComponent.mana < _playerSkillData.GetUnlockCost());
		}
	}

	private void OnClickMinionItem()
	{
		onClickUnlockMinion?.Invoke(_minionType, _playerSkillData.GetUnlockCost());
	}

	public void SetCoverState(bool p_state)
	{
		goPortraitCover.SetActive(p_state);
	}

	public void SetCheckmarkState(bool p_state)
	{
		goCheckMark.SetActive(p_state);
	}
}
