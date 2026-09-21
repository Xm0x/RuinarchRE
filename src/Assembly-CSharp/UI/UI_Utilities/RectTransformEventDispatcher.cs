using System;
using UnityEngine;

namespace UI.UI_Utilities;

public class RectTransformEventDispatcher : MonoBehaviour
{
	private Action<RectTransform> onRectTransformDimensionsChange;

	[SerializeField]
	private RectTransform _rectTransform;

	private void Awake()
	{
		if (_rectTransform == null)
		{
			_rectTransform = base.transform as RectTransform;
		}
	}

	private void OnRectTransformDimensionsChange()
	{
		onRectTransformDimensionsChange?.Invoke(_rectTransform);
	}

	public void Subscribe(Action<RectTransform> p_listener)
	{
		onRectTransformDimensionsChange = (Action<RectTransform>)Delegate.Combine(onRectTransformDimensionsChange, p_listener);
	}

	public void Unsubscribe(Action<RectTransform> p_listener)
	{
		onRectTransformDimensionsChange = (Action<RectTransform>)Delegate.Remove(onRectTransformDimensionsChange, p_listener);
	}

	private void OnDestroy()
	{
		onRectTransformDimensionsChange = null;
	}
}
