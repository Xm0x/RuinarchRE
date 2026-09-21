using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class FactionSettingUIItem : MonoBehaviour
{
	public static Action<FactionTemplate, FactionSettingUIItem> onClickMinus;

	public static Action<FactionTemplate, string> onChangeName;

	public static Action<FactionTemplate, FactionSettingUIItem> onClickRandomizeName;

	public static Action<FactionTemplate, int> onChangeFactionType;

	public static Action<FactionTemplate> onClickEditVillages;

	public static Action<FactionTemplate> onHoverOverEditVillages;

	public static Action<FactionTemplate> onHoverOutEditVillages;

	public Button btnMinus;

	public Image imgFactionEmblem;

	public TMP_InputField inputFieldName;

	public Button btnRandomizeName;

	public TMP_Dropdown dropDownFactionType;

	public TextMeshProUGUI txtVillageCount;

	public Button btnEditVillages;

	public HoverHandler hoverHandlerEditVillages;

	private FactionTemplate _factionTemplate;

	private void OnEnable()
	{
		btnMinus.onClick.AddListener(OnClickMinus);
		inputFieldName.onValueChanged.AddListener(OnChangeName);
		btnRandomizeName.onClick.AddListener(OnClickRandomizeName);
		dropDownFactionType.onValueChanged.AddListener(OnChangeFactionType);
		btnEditVillages.onClick.AddListener(OnClickEditVillages);
		hoverHandlerEditVillages.AddOnHoverOverAction(OnHoverOverEditVillages);
		hoverHandlerEditVillages.AddOnHoverOutAction(OnHoverOutEditVillages);
	}

	private void OnDisable()
	{
		btnMinus.onClick.RemoveListener(OnClickMinus);
		inputFieldName.onValueChanged.RemoveListener(OnChangeName);
		btnRandomizeName.onClick.RemoveListener(OnClickRandomizeName);
		dropDownFactionType.onValueChanged.RemoveListener(OnChangeFactionType);
		btnEditVillages.onClick.RemoveListener(OnClickEditVillages);
		hoverHandlerEditVillages.RemoveOnHoverOverAction(OnHoverOverEditVillages);
		hoverHandlerEditVillages.RemoveOnHoverOutAction(OnHoverOutEditVillages);
	}

	public void Initialize(List<string> p_choices)
	{
		dropDownFactionType.ClearOptions();
		dropDownFactionType.AddOptions(p_choices);
	}

	public void Reset()
	{
		dropDownFactionType.SetValueWithoutNotify(0);
	}

	public void SetItemDetails(FactionTemplate p_FactionTemplate)
	{
		_factionTemplate = p_FactionTemplate;
		imgFactionEmblem.sprite = p_FactionTemplate.factionEmblem;
		UpdateName(p_FactionTemplate.name);
		int value = 0;
		if (p_FactionTemplate.factionType == FACTION_TYPE.None)
		{
			value = GameUtilities.customWorldFactionTypeChoices.Length;
		}
		else
		{
			for (int i = 0; i < GameUtilities.customWorldFactionTypeChoices.Length; i++)
			{
				if (GameUtilities.customWorldFactionTypeChoices[i] == p_FactionTemplate.factionType)
				{
					value = i;
					break;
				}
			}
		}
		dropDownFactionType.value = value;
		txtVillageCount.text = p_FactionTemplate.villageSettings.Count.ToString();
	}

	public void SetMinusBtnState(bool p_state)
	{
		btnMinus.gameObject.SetActive(p_state);
	}

	public void UpdateName(string p_name)
	{
		inputFieldName.SetTextWithoutNotify(p_name);
	}

	private void OnClickMinus()
	{
		onClickMinus?.Invoke(_factionTemplate, this);
	}

	private void OnChangeName(string p_newValue)
	{
		onChangeName?.Invoke(_factionTemplate, p_newValue);
	}

	private void OnClickRandomizeName()
	{
		onClickRandomizeName?.Invoke(_factionTemplate, this);
	}

	private void OnChangeFactionType(int p_index)
	{
		onChangeFactionType?.Invoke(_factionTemplate, p_index);
	}

	private void OnClickEditVillages()
	{
		onClickEditVillages?.Invoke(_factionTemplate);
	}

	private void OnHoverOverEditVillages()
	{
		onHoverOverEditVillages?.Invoke(_factionTemplate);
	}

	private void OnHoverOutEditVillages()
	{
		onHoverOutEditVillages?.Invoke(_factionTemplate);
	}
}
