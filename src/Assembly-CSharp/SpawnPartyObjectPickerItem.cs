using System;
using EZObjectPools;
using TMPro;
using UnityEngine;

public class SpawnPartyObjectPickerItem : PooledObject
{
	[SerializeField]
	private CharacterPortrait portrait;

	[SerializeField]
	private TextMeshProUGUI lblCharges;

	[SerializeField]
	private TextMeshProUGUI lblName;

	[SerializeField]
	private GameObject goCover;

	[SerializeField]
	private HoverHandler hoverHandler;

	private Character _character;

	private Action<Character> _onClick;

	private Action<Character> _onHoverOver;

	private Action<Character> _onHoverOut;

	private bool _interactable;

	public Character character => _character;

	private void OnEnable()
	{
		portrait.AddPointerClickAction(OnClick);
		hoverHandler.AddOnHoverOverAction(OnHoverOver);
		hoverHandler.AddOnHoverOutAction(OnHoverOut);
	}

	private void OnDisable()
	{
		portrait.RemovePointerClickAction(OnClick);
		hoverHandler.RemoveOnHoverOverAction(OnHoverOver);
		hoverHandler.RemoveOnHoverOutAction(OnHoverOut);
	}

	public void Initialize(Character p_character, Action<Character> p_onClick, Action<Character> p_onHoverOver, Action<Character> p_onHoverOut)
	{
		_character = p_character;
		_onClick = p_onClick;
		_onHoverOver = p_onHoverOver;
		_onHoverOut = p_onHoverOut;
		portrait.GeneratePortrait(_character);
		UpdateName();
		lblCharges.gameObject.SetActive(value: false);
	}

	private void UpdateName()
	{
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(_character.characterClass.className);
		lblName.text = characterClass.displayName;
	}

	public void SetInteractableState(bool p_state)
	{
		_interactable = p_state;
		goCover.SetActive(!p_state);
	}

	private void OnClick()
	{
		if (_interactable)
		{
			_onClick?.Invoke(_character);
		}
	}

	private void OnHoverOver()
	{
		_onHoverOver?.Invoke(_character);
	}

	private void OnHoverOut()
	{
		_onHoverOut?.Invoke(_character);
	}

	public override void Reset()
	{
		base.Reset();
		_character = null;
		_interactable = true;
	}
}
