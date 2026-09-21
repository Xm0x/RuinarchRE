using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class PsychopathPicker : MonoBehaviour
{
	[Header("Object Picker")]
	[SerializeField]
	private ScrollRect objectPickerScrollView;

	[SerializeField]
	private GameObject psychopathPickerItemPrefab;

	[SerializeField]
	private GameObject cover;

	[SerializeField]
	private Button closeBtn;

	[SerializeField]
	private ToggleGroup toggleGroup;

	private Action<string, string> onConfirmAction;

	private string pickedString;

	private string localizedPickedString;

	public void ShowPicker(List<string> items, Action<string, string> onConfirmAction, Action<string> onHoverEnterAction, Action<string> onHoverExitAction, List<string> localizedNames)
	{
		Utilities.DestroyChildren(objectPickerScrollView.content);
		this.onConfirmAction = onConfirmAction;
		for (int i = 0; i < items.Count; i++)
		{
			string text = items[i];
			string localizedName = localizedNames[i];
			PsychopathPickerItem component = UnityEngine.Object.Instantiate(psychopathPickerItemPrefab, objectPickerScrollView.content).GetComponent<PsychopathPickerItem>();
			component.SetObject(text);
			component.SetLocalizedName(localizedName);
			component.SetToggleGroup(toggleGroup);
			component.SetToggleAction(OnPickSkill);
			component.SetOnHoverEnterAction(onHoverEnterAction);
			component.SetOnHoverExitAction(onHoverExitAction);
		}
		Open();
	}

	private void OnPickSkill(string str, string localized, bool isOn)
	{
		if (isOn)
		{
			pickedString = str;
			localizedPickedString = localized;
			OnClickConfirm();
		}
	}

	public void Open()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
	}

	public void OnClickConfirm()
	{
		onConfirmAction(pickedString, localizedPickedString);
		Close();
	}
}
