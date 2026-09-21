using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterPanelUI : MonoBehaviour
{
	public static MonsterPanelUI Instance;

	public InputField nameInput;

	public InputField levelInput;

	public InputField expInput;

	public InputField hpInput;

	public InputField spInput;

	public InputField powerInput;

	public InputField speedInput;

	public InputField dodgeInput;

	public InputField hitInput;

	public InputField critInput;

	public InputField itemDropRateInput;

	public InputField armyCountInput;

	public Dropdown typeOptions;

	public Dropdown skillOptions;

	public Dropdown itemDropOptions;

	public Toggle isSleepingOnSpawnToggle;

	public Transform skillContentTransform;

	public Transform itemDropContentTransform;

	public GameObject skillsGO;

	public GameObject itemDropGO;

	public GameObject monsterSkillBtnGO;

	public GameObject itemDropBtnPrefab;

	[NonSerialized]
	public MonsterSkillButton currentSelectedButton;

	private ItemDropBtn _currentSelectedItemDropBtn;

	private List<string> _allSkills;

	public List<string> allSkills => _allSkills;

	private void Awake()
	{
		Instance = this;
	}

	public void UpdateSkillList()
	{
		skillOptions.ClearOptions();
		skillOptions.AddOptions(SkillPanelUI.Instance.allSkills);
	}

	public void UpdateItemDropOptions()
	{
		itemDropOptions.ClearOptions();
		itemDropOptions.AddOptions(ItemPanelUI.Instance.allItems);
	}

	public void LoadAllData()
	{
		_allSkills = new List<string>();
		typeOptions.ClearOptions();
	}

	private void ClearData()
	{
		currentSelectedButton = null;
		nameInput.text = string.Empty;
		levelInput.text = "1";
		expInput.text = "0";
		hpInput.text = "0";
		spInput.text = "0";
		powerInput.text = "0";
		speedInput.text = "0";
		dodgeInput.text = "0";
		hitInput.text = "0";
		critInput.text = "0";
		itemDropRateInput.text = "0";
		armyCountInput.text = "1";
		typeOptions.value = 0;
		skillOptions.value = 0;
		itemDropOptions.value = 0;
		isSleepingOnSpawnToggle.isOn = false;
		_allSkills.Clear();
		foreach (Transform item in skillContentTransform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in itemDropContentTransform)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
	}

	private void SaveMonster()
	{
	}

	public void SetItemDropBn(ItemDropBtn btn)
	{
		_currentSelectedItemDropBtn = btn;
	}

	public void OnClickAddNewMonster()
	{
		ClearData();
	}

	public void OnClickSaveMonster()
	{
		SaveMonster();
	}

	public void OnClickAddSkill()
	{
		string text = skillOptions.options[skillOptions.value].text;
		if (!_allSkills.Contains(text))
		{
			_allSkills.Add(text);
			UnityEngine.Object.Instantiate(monsterSkillBtnGO, skillContentTransform).GetComponent<MonsterSkillButton>().buttonText.text = text;
		}
	}

	public void OnClickRemoveSkill()
	{
		if (currentSelectedButton != null)
		{
			string text = currentSelectedButton.buttonText.text;
			if (_allSkills.Remove(text))
			{
				UnityEngine.Object.Destroy(currentSelectedButton.gameObject);
				currentSelectedButton = null;
			}
		}
	}

	public void OnClickSkills()
	{
		skillsGO.SetActive(value: true);
		itemDropGO.SetActive(value: false);
	}

	public void OnClickItemDrops()
	{
		skillsGO.SetActive(value: false);
		itemDropGO.SetActive(value: true);
	}

	public void OnClickAddItemDrop()
	{
		_ = itemDropOptions.options[itemDropOptions.value].text;
	}

	public void OnClickRemoveItemDrop()
	{
		if (_currentSelectedItemDropBtn != null)
		{
			_ = _currentSelectedItemDropBtn.name;
		}
	}
}
