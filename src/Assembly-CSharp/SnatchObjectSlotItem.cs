using System;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;

public class SnatchObjectSlotItem : MonoBehaviour
{
	public Action<SnatchObjectSlotItem> onLeftClick;

	public Action<SnatchObjectSlotItem> onRightClick;

	public Action<SnatchObjectSlotItem> onHoverOver;

	public Action<SnatchObjectSlotItem> onHoverOut;

	[SerializeField]
	private RuinarchButton btnMain;

	[SerializeField]
	private Image imgPortrait;

	[SerializeField]
	private GameObject goEmpty;

	[SerializeField]
	private HoverHandler hoverHandler;

	private Character _character;

	public Character data => _character;

	private void OnEnable()
	{
		btnMain.onClick.AddListener(OnLeftClick);
		btnMain.AddRightClickAction(OnRightClick);
		hoverHandler.AddOnHoverOverAction(OnHoverOver);
		hoverHandler.AddOnHoverOutAction(OnHoverOut);
	}

	private void OnDisable()
	{
		btnMain.onClick.RemoveListener(OnLeftClick);
		btnMain.RemoveRightClickAction(OnRightClick);
		hoverHandler.RemoveOnHoverOverAction(OnHoverOver);
		hoverHandler.RemoveOnHoverOutAction(OnHoverOut);
	}

	private void OnDestroy()
	{
		_character = null;
	}

	public void SetInteractionActions(Action<SnatchObjectSlotItem> p_onLeftClick, Action<SnatchObjectSlotItem> p_onRightClick, Action<SnatchObjectSlotItem> p_onHoverOver, Action<SnatchObjectSlotItem> p_onHoverOut)
	{
		onLeftClick = p_onLeftClick;
		onRightClick = p_onRightClick;
		onHoverOver = p_onHoverOver;
		onHoverOut = p_onHoverOut;
	}

	public void ClearInteractionActions()
	{
		onLeftClick = null;
		onRightClick = null;
		onHoverOver = null;
		onHoverOut = null;
	}

	public void SetCharacter(Character p_character)
	{
		_character = p_character;
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(_character.characterClass.className);
		imgPortrait.sprite = characterClass.smallPortraitSprite;
		imgPortrait.gameObject.SetActive(value: true);
		goEmpty.SetActive(value: false);
	}

	public void ClearMonsterUnderling()
	{
		imgPortrait.gameObject.SetActive(value: false);
		goEmpty.SetActive(value: true);
		_character = null;
	}

	private void OnLeftClick()
	{
		onLeftClick?.Invoke(this);
	}

	private void OnRightClick()
	{
		onRightClick?.Invoke(this);
	}

	private void OnHoverOver()
	{
		onHoverOver?.Invoke(this);
	}

	private void OnHoverOut()
	{
		onHoverOut?.Invoke(this);
	}
}
