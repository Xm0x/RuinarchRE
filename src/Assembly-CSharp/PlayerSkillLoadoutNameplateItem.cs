using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillLoadoutNameplateItem : MonoBehaviour
{
	[SerializeField]
	private Image portrait;

	[SerializeField]
	private TextMeshProUGUI mainLbl;

	[SerializeField]
	private Toggle toggle;

	private Action<PlayerSkillData, bool> onToggleNameplate;

	private Action<PlayerSkillData> onHoverEnter;

	private Action<PlayerSkillData> onHoverExit;

	private PlayerSkillData skillData;

	public void SetObject(PLAYER_SKILL_TYPE o)
	{
		skillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(o);
		string text = (base.name = PlayerSkillManager.Instance.GetSkillData(o).localizedName);
		mainLbl.text = text;
		SetPortrait(skillData.buttonSprite);
	}

	public void SetToggleAction(Action<PlayerSkillData, bool> onToggleNameplate)
	{
		this.onToggleNameplate = onToggleNameplate;
	}

	public void SetOnHoverEnterAction(Action<PlayerSkillData> onHoverEnter)
	{
		this.onHoverEnter = onHoverEnter;
	}

	public void SetOnHoverExitAction(Action<PlayerSkillData> onHoverExit)
	{
		this.onHoverExit = onHoverExit;
	}

	public void SetPortrait(Sprite sprite)
	{
		portrait.sprite = sprite;
		portrait.gameObject.SetActive(portrait.sprite != null);
	}

	public void SetToggleGroup(ToggleGroup group)
	{
		toggle.group = group;
	}

	public void OnToggle(bool isOn)
	{
		onToggleNameplate(skillData, isOn);
	}

	public void OnHoverEnter()
	{
		onHoverEnter(skillData);
	}

	public void OnHoverExit()
	{
		onHoverExit(skillData);
	}

	public void SetInteractableState(bool state)
	{
		toggle.interactable = state;
	}
}
