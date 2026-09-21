using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class CharacterPanelUI : MonoBehaviour
{
	public static CharacterPanelUI Instance;

	public Dropdown classOptions;

	public Dropdown raceOptions;

	public Dropdown genderOptions;

	public Dropdown consumableOptions;

	public Dropdown traitOptions;

	public InputField nameInput;

	public InputField levelInput;

	public InputField armyInput;

	public TextMeshProUGUI hpLbl;

	public TextMeshProUGUI attackPowerLbl;

	public TextMeshProUGUI speedLbl;

	public TextMeshProUGUI spLbl;

	public TextMeshProUGUI skillsLbl;

	public TextMeshProUGUI combatAttributesLbl;

	public TextMeshProUGUI weaponLbl;

	public TextMeshProUGUI armorLbl;

	public TextMeshProUGUI accessoryLbl;

	public Toggle toggleArmy;

	private int _singleHP;

	private int _sp;

	private int _singleAttackPower;

	private int _singleSpeed;

	private int _attackPower;

	private int _speed;

	private int _hp;

	private List<string> _allCombatAttributeNames;

	public int hp => _singleHP;

	public int sp => _sp;

	public int speed => _singleSpeed;

	public int attackPower => _singleAttackPower;

	public List<string> allCombatAttributeNames => _allCombatAttributeNames;

	private void Awake()
	{
		Instance = this;
	}

	public void LoadAllData()
	{
		_allCombatAttributeNames = new List<string>();
		levelInput.text = "1";
		armyInput.text = "1";
		raceOptions.ClearOptions();
		genderOptions.ClearOptions();
		string[] names = Enum.GetNames(typeof(GENDER));
		genderOptions.AddOptions(names.ToList());
	}

	public void UpdateClassOptions()
	{
		classOptions.ClearOptions();
		classOptions.AddOptions(ClassPanelUI.Instance.allClasses);
	}

	public void UpdateTraitOptions()
	{
		traitOptions.ClearOptions();
		traitOptions.AddOptions(TraitPanelUI.Instance.allTraits);
	}

	public void UpdateItemOptions()
	{
	}

	private void ClearData()
	{
		classOptions.value = 0;
		genderOptions.value = 0;
		nameInput.text = string.Empty;
		levelInput.text = "1";
		armyInput.text = "1";
		attackPowerLbl.text = "0";
		speedLbl.text = "0";
		hpLbl.text = "0";
		spLbl.text = "0";
		skillsLbl.text = "None";
		combatAttributesLbl.text = "None";
		weaponLbl.text = "None";
		armorLbl.text = "None";
		accessoryLbl.text = "None";
		toggleArmy.isOn = false;
	}

	private void SaveCharacter()
	{
	}

	private void SaveCharacterJson(string path)
	{
	}

	private void LoadCharacter()
	{
	}

	private int GetDropdownIndex(string armorName, Dropdown ddOptions)
	{
		for (int i = 0; i < ddOptions.options.Count; i++)
		{
			if (ddOptions.options[i].text == armorName)
			{
				return i;
			}
		}
		return 0;
	}

	private CharacterClass GetClass(string className)
	{
		return JsonUtility.FromJson<CharacterClass>(File.ReadAllText(Utilities.coreStreamingDataPath + "/CharacterClasses/" + className + ".json"));
	}

	private RaceData GetRace(string raceName)
	{
		return null;
	}

	private void AllocateStats(RaceData raceSetting)
	{
	}

	private void LevelUp(int level, CharacterClass characterClass, RaceData raceSetting)
	{
		_ = level - 1;
		_ = 0;
	}

	private void ArmyModifier(bool state)
	{
		if (state)
		{
			_attackPower = _singleAttackPower * int.Parse(armyInput.text);
			_speed = _singleSpeed * int.Parse(armyInput.text);
			_hp = _singleHP * int.Parse(armyInput.text);
		}
		else
		{
			_attackPower = _singleAttackPower;
			_speed = _singleSpeed;
			_hp = _singleHP;
		}
	}

	private void UpdateUI()
	{
		attackPowerLbl.text = _attackPower.ToString();
		speedLbl.text = _speed.ToString();
		hpLbl.text = _hp.ToString();
		spLbl.text = _sp.ToString();
		UpdateTraitsUI();
	}

	private void UpdateTraitsUI()
	{
		combatAttributesLbl.text = string.Empty;
		if (_allCombatAttributeNames != null && _allCombatAttributeNames.Count > 0)
		{
			combatAttributesLbl.text += _allCombatAttributeNames[0];
			for (int i = 1; i < _allCombatAttributeNames.Count; i++)
			{
				TextMeshProUGUI textMeshProUGUI = combatAttributesLbl;
				textMeshProUGUI.text = textMeshProUGUI.text + ", " + _allCombatAttributeNames[i];
			}
		}
		if (string.IsNullOrEmpty(combatAttributesLbl.text))
		{
			combatAttributesLbl.text = "None";
		}
	}

	private void ClassChange(int index)
	{
		CharacterClass characterClass = GetClass(classOptions.options[index].text);
		RaceData race = GetRace(raceOptions.options[raceOptions.value].text);
		int num = int.Parse(levelInput.text);
		if (num < 1)
		{
			num = 1;
			levelInput.text = num.ToString();
		}
		else if (num > 100)
		{
			num = 100;
			levelInput.text = num.ToString();
		}
		AllocateStats(race);
		LevelUp(num, characterClass, race);
		ArmyModifier(toggleArmy.isOn);
		UpdateUI();
	}

	private void RaceChange(int index)
	{
		CharacterClass characterClass = GetClass(classOptions.options[classOptions.value].text);
		RaceData race = GetRace(raceOptions.options[index].text);
		int num = int.Parse(levelInput.text);
		if (num < 1)
		{
			num = 1;
			levelInput.text = num.ToString();
		}
		else if (num > 100)
		{
			num = 100;
			levelInput.text = num.ToString();
		}
		AllocateStats(race);
		LevelUp(num, characterClass, race);
		ArmyModifier(toggleArmy.isOn);
		UpdateUI();
	}

	public void OnClassChange(int index)
	{
		ClassChange(index);
	}

	public void OnRaceChange(int index)
	{
		RaceChange(index);
	}

	public void OnToggleArmy(bool state)
	{
		armyInput.gameObject.SetActive(state);
		ArmyModifier(state);
	}

	public void OnClickAddNewCharacter()
	{
		ClearData();
	}

	public void OnClickEditCharacter()
	{
		LoadCharacter();
	}

	public void OnClickSaveCharacter()
	{
		SaveCharacter();
	}

	public void OnClickApply()
	{
		ClassChange(classOptions.value);
	}

	public void OnAddCombatAttribute()
	{
		if (traitOptions.options.Count > 0 && !_allCombatAttributeNames.Contains(traitOptions.options[traitOptions.value].text))
		{
			_allCombatAttributeNames.Add(traitOptions.options[traitOptions.value].text);
		}
		UpdateTraitsUI();
	}

	public void OnRemoveCombatAttribute()
	{
		if (traitOptions.options.Count > 0 && _allCombatAttributeNames.Remove(traitOptions.options[traitOptions.value].text))
		{
			UpdateTraitsUI();
		}
	}
}
