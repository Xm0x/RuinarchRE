using System;
using EZObjectPools;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;

public class SummonActiveItem : PooledObject
{
	[SerializeField]
	private RuinarchButton button;

	[SerializeField]
	private HoverHandler hoverHandler;

	[Header("Portrait")]
	[SerializeField]
	private Image imgPortrait;

	private Character _character;

	private Action<SummonActiveItem> _onHoverOverItem;

	private Action<SummonActiveItem> _onHoverOutItem;

	public Character character => _character;

	private void OnEnable()
	{
		button.onClick.AddListener(OnClickItem);
		hoverHandler.AddOnHoverOverAction(OnHoverOverItem);
		hoverHandler.AddOnHoverOutAction(OnHoverOutItem);
	}

	private void OnDisable()
	{
		button.onClick.RemoveListener(OnClickItem);
		hoverHandler.RemoveOnHoverOverAction(OnHoverOverItem);
		hoverHandler.RemoveOnHoverOutAction(OnHoverOutItem);
	}

	public void Initialize(Character p_character, Action<SummonActiveItem> p_onHoverOverItem, Action<SummonActiveItem> p_onHoverOutItem)
	{
		_character = p_character;
		_onHoverOverItem = p_onHoverOverItem;
		_onHoverOutItem = p_onHoverOutItem;
		CharacterClass characterClass = p_character.characterClass;
		imgPortrait.sprite = characterClass.smallPortraitSprite;
	}

	private void OnClickItem()
	{
		_character?.CenterOnCharacter();
	}

	private void OnHoverOverItem()
	{
		_onHoverOverItem?.Invoke(this);
	}

	private void OnHoverOutItem()
	{
		_onHoverOutItem?.Invoke(this);
	}

	public override void Reset()
	{
		base.Reset();
		_character = null;
		_onHoverOverItem = null;
		_onHoverOutItem = null;
	}
}
