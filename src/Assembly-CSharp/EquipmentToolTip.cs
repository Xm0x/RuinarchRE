using TMPro;
using UnityEngine;
using UtilityScripts;

public class EquipmentToolTip : MonoBehaviour
{
	public RectTransform thisRect;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI descriptionText;

	public TextMeshProUGUI bonusesText;

	public UIHoverPosition defaultPosition;

	private void UpdatePosition(UIHoverPosition position)
	{
		base.gameObject.SetActive(value: true);
		UIHoverPosition uIHoverPosition = position;
		if (uIHoverPosition == null)
		{
			uIHoverPosition = defaultPosition;
		}
		thisRect.SetParent(uIHoverPosition.transform);
		thisRect.pivot = uIHoverPosition.pivot;
		Utilities.GetAnchorMinMax(uIHoverPosition.anchor, out var anchorMin, out var anchorMax);
		thisRect.anchorMin = anchorMin;
		thisRect.anchorMax = anchorMax;
		thisRect.anchoredPosition = Vector2.zero;
		thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 464f);
	}

	private void UpdateData(EquipmentItem p_item)
	{
		titleText.text = p_item.wholeName;
		string description = p_item.description;
		if (string.IsNullOrEmpty(description))
		{
			descriptionText.gameObject.SetActive(value: false);
		}
		else
		{
			descriptionText.text = description;
			descriptionText.gameObject.SetActive(value: true);
		}
		bonusesText.text = p_item.GetBonusDescription();
	}

	public void ShowEquipmentItem(EquipmentItem p_item, UIHoverPosition position = null)
	{
		UpdateData(p_item);
		UpdatePosition(position);
	}
}
