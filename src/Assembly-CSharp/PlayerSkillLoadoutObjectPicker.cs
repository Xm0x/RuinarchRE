using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class PlayerSkillLoadoutObjectPicker : MonoBehaviour
{
	[Header("Object Picker")]
	[SerializeField]
	private ScrollRect objectPickerScrollView;

	[SerializeField]
	private GameObject playerskillLoadoutItemPrefab;

	[SerializeField]
	private GameObject cover;

	[SerializeField]
	private Button closeBtn;

	[SerializeField]
	private Button confirmBtn;

	[SerializeField]
	private ToggleGroup toggleGroup;

	public UIHoverPosition hoverPos;

	private Action<PlayerSkillData> onConfirmAction;

	private PlayerSkillData pickedSkill;

	public void ShowLoadoutPicker(PLAYER_SKILL_TYPE[] items, Action<PlayerSkillData> onConfirmAction, Action<PlayerSkillData> onHoverEnterAction, Action<PlayerSkillData> onHoverExitAction)
	{
		Utilities.DestroyChildren(objectPickerScrollView.content);
		this.onConfirmAction = onConfirmAction;
		foreach (PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE in items)
		{
			if (PlayerSkillManager.Instance.playerSkillDataDictionary.ContainsKey(pLAYER_SKILL_TYPE))
			{
				PlayerSkillLoadoutNameplateItem component = UnityEngine.Object.Instantiate(playerskillLoadoutItemPrefab, objectPickerScrollView.content).GetComponent<PlayerSkillLoadoutNameplateItem>();
				component.SetObject(pLAYER_SKILL_TYPE);
				component.SetToggleGroup(toggleGroup);
				component.SetToggleAction(OnPickSkill);
				component.SetOnHoverEnterAction(onHoverEnterAction);
				component.SetOnHoverExitAction(onHoverExitAction);
			}
		}
		UpdateConfirmBtnState();
		Open();
	}

	public void ShowLoadoutPicker(List<PLAYER_SKILL_TYPE> items, Action<PlayerSkillData> onConfirmAction, Action<PlayerSkillData> onHoverEnterAction, Action<PlayerSkillData> onHoverExitAction)
	{
		Utilities.DestroyChildren(objectPickerScrollView.content);
		this.onConfirmAction = onConfirmAction;
		for (int i = 0; i < items.Count; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = items[i];
			if (PlayerSkillManager.Instance.playerSkillDataDictionary.ContainsKey(pLAYER_SKILL_TYPE))
			{
				PlayerSkillLoadoutNameplateItem component = UnityEngine.Object.Instantiate(playerskillLoadoutItemPrefab, objectPickerScrollView.content).GetComponent<PlayerSkillLoadoutNameplateItem>();
				component.SetObject(pLAYER_SKILL_TYPE);
				component.SetToggleGroup(toggleGroup);
				component.SetToggleAction(OnPickSkill);
				component.SetOnHoverEnterAction(onHoverEnterAction);
				component.SetOnHoverExitAction(onHoverExitAction);
				if (pLAYER_SKILL_TYPE == PLAYER_SKILL_TYPE.OSTRACIZER)
				{
					component.SetInteractableState(state: false);
					component.transform.SetAsLastSibling();
				}
				else
				{
					component.SetInteractableState(state: true);
					component.transform.SetAsFirstSibling();
				}
			}
		}
		UpdateConfirmBtnState();
		Open();
	}

	private void OnPickSkill(PlayerSkillData skillData, bool isOn)
	{
		if (isOn)
		{
			pickedSkill = skillData;
			OnClickConfirm();
		}
	}

	private void UpdateConfirmBtnState()
	{
		confirmBtn.interactable = pickedSkill != null;
	}

	public void Open()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
	}

	public void OnClickConfirm()
	{
		onConfirmAction(pickedSkill);
		Close();
	}
}
