using System;
using EZObjectPools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseCharacterPortrait : PooledObject, IPointerClickHandler, IEventSystemHandler
{
	[Header("BG")]
	[SerializeField]
	private Image baseBG;

	[Header("Face")]
	[SerializeField]
	private Image wholeImage;

	[Header("Other")]
	[SerializeField]
	private GameObject hoverObj;

	[SerializeField]
	private bool ignoreInteractions;

	[SerializeField]
	private HoverHandler hoverHandler;

	private Action _onClickAction;

	private Action _onHoverOverAction;

	private Action _onHoverOutAction;

	private void Awake()
	{
		hoverHandler.AddOnHoverOverAction(OnHoverOver);
		hoverHandler.AddOnHoverOutAction(OnHoverOut);
	}

	public void GeneratePortrait(MINION_TYPE p_demonType)
	{
		Sprite portraitSprite = CharacterManager.Instance.GetCharacterClass(CharacterManager.Instance.GetMinionSettings(p_demonType).className).portraitSprite;
		UpdatePortrait(portraitSprite);
	}

	private void UpdatePortrait(Sprite p_sprite)
	{
		if (p_sprite != null)
		{
			SetWholeImageSprite(p_sprite);
			SetWholeImageState(state: true);
		}
		else
		{
			SetWholeImageSprite(null);
			SetWholeImageState(state: false);
		}
	}

	private void SetWholeImageState(bool state)
	{
		wholeImage.gameObject.SetActive(state);
	}

	private void SetWholeImageSprite(Sprite sprite)
	{
		wholeImage.sprite = sprite;
	}

	public void AddPointerClickAction(Action p_action)
	{
		_onClickAction = (Action)Delegate.Combine(_onClickAction, p_action);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!ignoreInteractions)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				OnLeftClick();
			}
			else if (eventData.button == PointerEventData.InputButton.Right)
			{
				OnRightClick();
			}
		}
	}

	public void OnClick(BaseEventData eventData)
	{
		if (!ignoreInteractions && base.gameObject.activeSelf)
		{
			OnPointerClick(eventData as PointerEventData);
		}
	}

	public void OnLeftClick()
	{
		_onClickAction?.Invoke();
	}

	private void OnRightClick()
	{
	}

	public void SetHoverHighlightState(bool state)
	{
		hoverObj.SetActive(state);
	}

	private void OnHoverOver()
	{
		_onHoverOverAction?.Invoke();
	}

	private void OnHoverOut()
	{
		_onHoverOutAction?.Invoke();
	}

	public void AddHoverOverAction(Action p_action)
	{
		_onHoverOverAction = (Action)Delegate.Combine(_onHoverOverAction, p_action);
	}

	public void AddHoverOutAction(Action p_action)
	{
		_onHoverOutAction = (Action)Delegate.Combine(_onHoverOutAction, p_action);
	}

	public void RemoveHoverOverAction(Action p_action)
	{
		_onHoverOverAction = (Action)Delegate.Remove(_onHoverOverAction, p_action);
	}

	public void RemoveHoverOutAction(Action p_action)
	{
		_onHoverOutAction = (Action)Delegate.Remove(_onHoverOutAction, p_action);
	}
}
