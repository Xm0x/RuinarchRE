using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UtilityScripts;

public class StringPickerItem : ObjectPickerItem<string>, IPointerClickHandler, IEventSystemHandler
{
	public Action<string> onClickAction;

	private string str;

	public GameObject portraitCover;

	public Image iconImg;

	public string identifier;

	public override string obj => str;

	public void SetString(string str, string identifier)
	{
		this.str = str;
		this.identifier = identifier;
		iconImg.gameObject.SetActive(value: false);
		UpdateVisuals();
	}

	public override void SetButtonState(bool state)
	{
		base.SetButtonState(state);
		portraitCover.SetActive(!state);
	}

	private void UpdateVisuals()
	{
		mainLbl.text = str;
		if (!(identifier != string.Empty))
		{
			return;
		}
		if (identifier == "trait")
		{
			if (TraitManager.Instance.HasTraitIcon(str))
			{
				iconImg.sprite = TraitManager.Instance.GetTraitPortrait(str);
				iconImg.gameObject.SetActive(value: true);
			}
			else
			{
				iconImg.gameObject.SetActive(value: false);
			}
		}
		else if (identifier == "landmark")
		{
			iconImg.gameObject.SetActive(value: false);
		}
		else if (identifier == "intervention ability")
		{
			iconImg.sprite = PlayerManager.Instance.GetJobActionSprite(str);
			iconImg.gameObject.SetActive(value: true);
		}
		else if (identifier == "minion")
		{
			iconImg.sprite = CharacterManager.Instance.GetCharacterClass(str).portraitSprite;
			iconImg.gameObject.SetActive(value: true);
		}
		else if (identifier == "player skill")
		{
			iconImg.sprite = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>((PLAYER_SKILL_TYPE)Enum.Parse(typeof(PLAYER_SKILL_TYPE), Utilities.NotNormalizedConversionStringToEnum(str).ToUpper())).buttonSprite;
			iconImg.gameObject.SetActive(value: true);
		}
		iconImg.SetNativeSize();
		if (iconImg.sprite == null)
		{
			iconImg.gameObject.SetActive(value: false);
		}
	}

	private void OnClick()
	{
		if (onClickAction != null)
		{
			onClickAction(str);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		OnClick();
	}
}
