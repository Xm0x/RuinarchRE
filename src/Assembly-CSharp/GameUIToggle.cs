using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class GameUIToggle : Toggle
{
	[SerializeField]
	private TextMeshProUGUI targetText;

	private Color onColor;

	private Color offColor;

	public override void OnPointerClick(PointerEventData eventData)
	{
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
	}
}
