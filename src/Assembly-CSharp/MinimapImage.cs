using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinimapImage : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public RawImage rawImage;

	private Action<PointerEventData> _onClickMinimapImageAction;

	private bool _isClicked;

	private PointerEventData _pointerEventData;

	public void SetOnClickAction(Action<PointerEventData> onClickMinimapImageAction)
	{
		_onClickMinimapImageAction = onClickMinimapImageAction;
	}

	private void Update()
	{
		if (_isClicked)
		{
			_onClickMinimapImageAction?.Invoke(_pointerEventData);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_pointerEventData = eventData;
		_isClicked = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_isClicked = false;
	}
}
