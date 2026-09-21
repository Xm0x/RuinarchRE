using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class CustomDropdownList : PopupMenuBase
{
	[Header("Object Picker")]
	[SerializeField]
	private ScrollRect dropdownScrollView;

	[SerializeField]
	private GameObject dropdownListItemPrefab;

	private Action<string> onClickDropdownItem;

	public void ShowDropdown(List<string> items, Action<string> onClickDropdownItem, Func<string, bool> canChooseItem = null)
	{
		Utilities.DestroyChildren(dropdownScrollView.content);
		this.onClickDropdownItem = onClickDropdownItem;
		for (int i = 0; i < items.Count; i++)
		{
			string text = items[i];
			CustomDDListItem component = UIManager.Instance.InstantiateUIObject(dropdownListItemPrefab.name, dropdownScrollView.content).GetComponent<CustomDDListItem>();
			component.SetText(text);
			if (canChooseItem != null)
			{
				component.SetCoverState(!canChooseItem(text));
			}
			component.SetClickAction(OnClickDropdownItem);
		}
		base.Open();
	}

	public void SetPosition(Vector3 position)
	{
		base.gameObject.transform.localPosition = position;
	}

	public void OnClickDropdownItem(CustomDDListItem item)
	{
		onClickDropdownItem(item.itemText.text);
	}
}
