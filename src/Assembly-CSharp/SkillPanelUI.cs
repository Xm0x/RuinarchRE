using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class SkillPanelUI : MonoBehaviour
{
	public static SkillPanelUI Instance;

	public InputField skillNameInput;

	public InputField skillDescInput;

	public Dropdown skillTypeOptions;

	public Dropdown elementOptions;

	public Dropdown targetTypeOptions;

	public GameObject cellAmountGO;

	public GameObject weaponTypeBtnGO;

	public Transform contentTransform;

	[NonSerialized]
	public WeaponTypeButton currentSelectedWeaponTypeButton;

	[NonSerialized]
	public List<string> allSkills;

	private void Awake()
	{
		Instance = this;
	}

	public void LoadAllData()
	{
		elementOptions.ClearOptions();
		targetTypeOptions.ClearOptions();
		skillTypeOptions.ClearOptions();
		allSkills = new List<string>();
		UpdateSkillList();
	}

	private void ClearData()
	{
		currentSelectedWeaponTypeButton = null;
		skillNameInput.text = string.Empty;
		skillDescInput.text = string.Empty;
		elementOptions.value = 0;
		targetTypeOptions.value = 0;
		skillTypeOptions.value = 0;
	}

	private void SaveSkill()
	{
	}

	private void SaveSkillJson(string path)
	{
	}

	public void LoadSkill()
	{
	}

	private int GetOptionIndex(string name, Dropdown ddOptions)
	{
		for (int i = 0; i < ddOptions.options.Count; i++)
		{
			if (ddOptions.options[i].text.Equals(name, StringComparison.CurrentCultureIgnoreCase))
			{
				return i;
			}
		}
		return 0;
	}

	private void UpdateSkillList()
	{
		allSkills.Clear();
		string[] files = Directory.GetFiles(Utilities.coreStreamingDataPath + "/Skills/", "*.json");
		foreach (string path in files)
		{
			allSkills.Add(Path.GetFileNameWithoutExtension(path));
		}
		MonsterPanelUI.Instance.UpdateSkillList();
	}

	public void OnAddNewSkill()
	{
		ClearData();
	}

	public void OnAddWeaponType()
	{
	}

	public void OnRemoveWeaponType()
	{
	}

	public void OnSaveSkill()
	{
		SaveSkill();
	}

	public void OnEditSkill()
	{
		LoadSkill();
	}
}
