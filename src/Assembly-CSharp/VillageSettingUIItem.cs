using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VillageSettingUIItem : MonoBehaviour
{
	public static Action<VillageSettingUIItem> onClickMinus;

	public static Action<VillageSettingUIItem, string> onChangeName;

	public static Action<VillageSettingUIItem> onClickRandomizeName;

	public static Action<VillageSettingUIItem, VILLAGE_SIZE> onChangeVillageSize;

	public Button btnMinus;

	public TMP_InputField inputFieldName;

	public Button btnRandomizeName;

	public TMP_Dropdown dropDownVillageSize;

	private void OnEnable()
	{
		btnMinus.onClick.AddListener(OnClickMinus);
		inputFieldName.onValueChanged.AddListener(OnChangeFactionName);
		btnRandomizeName.onClick.AddListener(OnClickRandomizeName);
		dropDownVillageSize.onValueChanged.AddListener(OnChangeVillageSize);
	}

	private void OnDisable()
	{
		btnMinus.onClick.RemoveListener(OnClickMinus);
		inputFieldName.onValueChanged.RemoveListener(OnChangeFactionName);
		btnRandomizeName.onClick.RemoveListener(OnClickRandomizeName);
		dropDownVillageSize.onValueChanged.RemoveListener(OnChangeVillageSize);
	}

	public void SetVillageSizeChoices(List<string> p_villageSizeChoices)
	{
		dropDownVillageSize.ClearOptions();
		dropDownVillageSize.AddOptions(p_villageSizeChoices);
	}

	public void SetItemDetails(VillageSetting p_villageSettings)
	{
		inputFieldName.SetTextWithoutNotify(p_villageSettings.villageName);
		dropDownVillageSize.SetValueWithoutNotify((int)p_villageSettings.villageSize);
	}

	public void SetMinusBtnState(bool p_state)
	{
		btnMinus.gameObject.SetActive(p_state);
	}

	private void OnClickMinus()
	{
		onClickMinus?.Invoke(this);
	}

	private void OnChangeFactionName(string p_newName)
	{
		onChangeName?.Invoke(this, p_newName);
	}

	private void OnClickRandomizeName()
	{
		onClickRandomizeName?.Invoke(this);
	}

	private void OnChangeVillageSize(int p_index)
	{
		onChangeVillageSize?.Invoke(this, (VILLAGE_SIZE)p_index);
	}
}
