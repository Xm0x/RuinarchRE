using System;
using EZObjectPools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseLocationPortrait : PooledObject, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Image portrait;

	[SerializeField]
	private GameObject hoverObj;

	[SerializeField]
	private bool disableInteraction;

	[SerializeField]
	private HoverHandler hoverHandler;

	private Action _leftClickAction;

	private Action _rightClickAction;

	private Action _onHoverOverAction;

	private Action _onHoverOutAction;

	private void Awake()
	{
		hoverHandler.AddOnHoverOverAction(OnHoverOver);
		hoverHandler.AddOnHoverOutAction(OnHoverOut);
	}

	public void SetPortrait(STRUCTURE_TYPE landmarkType)
	{
		portrait.sprite = LandmarkManager.Instance.GetStructureData(landmarkType).structureSprite;
	}

	public void AddLeftClickAction(Action p_action)
	{
		_leftClickAction = (Action)Delegate.Combine(_leftClickAction, p_action);
	}

	public void AddRightClickAction(Action p_action)
	{
		_rightClickAction = (Action)Delegate.Combine(_rightClickAction, p_action);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!disableInteraction)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				_leftClickAction?.Invoke();
			}
			else if (eventData.button == PointerEventData.InputButton.Right)
			{
				_rightClickAction?.Invoke();
			}
		}
	}

	private void OnHoverOver()
	{
		if (!hoverObj.activeSelf)
		{
			hoverObj.gameObject.SetActive(value: true);
		}
		_onHoverOverAction?.Invoke();
	}

	private void OnHoverOut()
	{
		if (hoverObj.activeSelf)
		{
			hoverObj.gameObject.SetActive(value: false);
		}
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

	public override void Reset()
	{
		base.Reset();
		_leftClickAction = null;
		_rightClickAction = null;
	}
}
