using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPickerItem<T> : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI mainLbl;

	[SerializeField]
	protected TextMeshProUGUI subLbl;

	[SerializeField]
	protected Button mainBtn;

	public Action<T> onHoverEnterAction;

	public Action<T> onHoverExitAction;

	public virtual T obj { get; }

	public virtual void SetButtonState(bool state)
	{
		mainBtn.interactable = state;
	}

	public void OnHoverEnter()
	{
		if (onHoverEnterAction != null)
		{
			onHoverEnterAction(obj);
		}
	}

	public virtual void OnHoverExit()
	{
		if (onHoverExitAction != null)
		{
			onHoverExitAction(obj);
		}
	}
}
