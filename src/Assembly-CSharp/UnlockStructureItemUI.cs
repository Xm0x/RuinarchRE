using System;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UtilityScripts;

public class UnlockStructureItemUI : MonoBehaviour
{
	public static Action<PLAYER_SKILL_TYPE, int> onClickUnlockStructure;

	[SerializeField]
	private TextMeshProUGUI lblName;

	[SerializeField]
	private TextMeshProUGUI lblCosts;

	[SerializeField]
	private RuinarchButton btn;

	[SerializeField]
	private GameObject goCover;

	private PLAYER_SKILL_TYPE _structureType;

	private PlayerSkillData _playerSkillData;

	public void SetStructureType(PLAYER_SKILL_TYPE p_type)
	{
		_structureType = p_type;
		_playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_type);
		lblName.text = _playerSkillData.name;
		lblCosts.text = $"{_playerSkillData.GetUnlockCost()}{Utilities.ManaIcon()}";
		btn.onClick.AddListener(OnClickStructureItem);
	}

	private void OnClickStructureItem()
	{
		onClickUnlockStructure?.Invoke(_structureType, _playerSkillData.GetUnlockCost());
	}

	public void SetCoverState(bool p_state)
	{
		goCover.SetActive(p_state);
	}

	public void UpdateSelectableState()
	{
		if (PlayerSkillManager.Instance.GetSkillData(_structureType).isInUse)
		{
			SetCoverState(p_state: true);
			btn.interactable = false;
		}
		else
		{
			bool flag = PlayerManager.Instance.player.currenciesComponent.mana >= _playerSkillData.GetUnlockCost();
			btn.interactable = flag;
			SetCoverState(!flag);
		}
	}
}
