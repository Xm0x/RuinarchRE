using System;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotItem : MonoBehaviour
{
	public RuinarchButton skillSlotItemButton;

	public RuinarchButton minusButton;

	public Image buttonImage;

	public Image icon;

	public Image fixedIcon;

	public Sprite optionalButtonDefault;

	public Sprite optionalButtonHighlighted;

	public Sprite optionalButtonPressed;

	public Sprite defaultButtonDefault;

	public Sprite defaultButtonHighlighted;

	public Sprite defaultButtonPressed;

	public Sprite defaultButtonDisabled;

	public TextMeshProUGUI spellText;

	private Action<PlayerSkillData> onHoverEnter;

	private Action<PlayerSkillData> onHoverExit;

	private bool isFixed;

	private PLAYER_ARCHETYPE archetype;

	public PlayerSkillData skillData { get; private set; }

	public void SetSkillSlotItem(PLAYER_ARCHETYPE archetype, PlayerSkillData skillData, bool isFixed)
	{
		this.skillData = skillData;
		this.isFixed = isFixed;
		this.archetype = archetype;
		UpdateSkillSlotItem();
	}

	public void SetSkillSlotItem(PLAYER_ARCHETYPE archetype, PLAYER_SKILL_TYPE skillType, bool isFixed)
	{
		SetSkillSlotItem(archetype, PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(skillType), isFixed);
	}

	public void SetOnHoverEnterAction(Action<PlayerSkillData> onHoverEnter)
	{
		this.onHoverEnter = onHoverEnter;
	}

	public void SetOnHoverExitAction(Action<PlayerSkillData> onHoverExit)
	{
		this.onHoverExit = onHoverExit;
	}

	private void UpdateSkillSlotItem()
	{
		UpdateButtonSprites();
		UpdateMinusButton();
		UpdateIcon();
		UpdateText();
		UpdateFixedIcon();
	}

	private void UpdateButtonSprites()
	{
		SpriteState spriteState = default(SpriteState);
		if (!isFixed && skillData == null)
		{
			spriteState.highlightedSprite = optionalButtonHighlighted;
			spriteState.pressedSprite = optionalButtonPressed;
			spriteState.selectedSprite = optionalButtonPressed;
			spriteState.disabledSprite = optionalButtonDefault;
			buttonImage.sprite = optionalButtonDefault;
		}
		else
		{
			spriteState.highlightedSprite = defaultButtonHighlighted;
			spriteState.pressedSprite = defaultButtonPressed;
			spriteState.selectedSprite = defaultButtonPressed;
			spriteState.disabledSprite = defaultButtonDefault;
			buttonImage.sprite = defaultButtonDefault;
		}
		skillSlotItemButton.spriteState = spriteState;
	}

	private void UpdateMinusButton()
	{
		minusButton.gameObject.SetActive(!isFixed && skillData != null);
	}

	private void UpdateIcon()
	{
		if (skillData != null)
		{
			icon.sprite = skillData.buttonSprite;
			icon.gameObject.SetActive(value: true);
		}
		else
		{
			icon.gameObject.SetActive(value: false);
		}
	}

	private void UpdateFixedIcon()
	{
		fixedIcon.gameObject.SetActive(isFixed);
	}

	public void ClearData()
	{
		SetSkillSlotItem(archetype, null, isFixed);
	}

	private void UpdateText()
	{
		if (this.skillData != null)
		{
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(this.skillData.skill);
			spellText.text = skillData.localizedName;
		}
		else if (!isFixed)
		{
			spellText.text = "Click to Assign a Skill";
		}
		else
		{
			spellText.text = "Unassigned";
		}
	}

	public void OnClickThis()
	{
		if (!isFixed)
		{
			Messenger.Broadcast(UISignals.SKILL_SLOT_ITEM_CLICKED, this, archetype);
		}
	}

	public void OnClickMinus()
	{
		if (!isFixed)
		{
			ClearData();
		}
	}

	public void OnHoverEnter()
	{
		onHoverEnter(skillData);
	}

	public void OnHoverExit()
	{
		onHoverExit(skillData);
	}
}
