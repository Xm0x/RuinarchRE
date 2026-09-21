using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class RacePanelUI : MonoBehaviour
{
	public static RacePanelUI Instance;

	public Dropdown raceOptions;

	public Dropdown traitOptions;

	public InputField attackMultiplierInput;

	public InputField hpMultiplierInput;

	public InputField attackSpeedMultiplierInput;

	public InputField staminaReductionMultiplierInput;

	public InputField speedModifierInput;

	public InputField hpPerLevelInput;

	public InputField attackPerLevelInput;

	public InputField neutralSpawnLevelModInput;

	public InputField runSpeedInput;

	public InputField walkSpeedInput;

	public GameObject traitsGO;

	public GameObject hpPerLevelGO;

	public GameObject attackPerLevelGO;

	public GameObject raceStringButtonPrefab;

	public ScrollRect traitsScrollRect;

	public ScrollRect hpPerLevelScrollRect;

	public ScrollRect attackPerLevelScrollRect;

	[NonSerialized]
	public RaceStringButton currentSelectedTraitButton;

	[NonSerialized]
	public RaceStringButton currentSelectedHPPerLevelButton;

	[NonSerialized]
	public RaceStringButton currentSelectedAttackPerLevelButton;

	private List<string> _traitNames;

	private List<int> _hpPerLevel;

	private List<int> _attackPerLevel;

	public List<string> traitNames => _traitNames;

	public List<int> hpPerLevel => _hpPerLevel;

	public List<int> attackPerLevel => _attackPerLevel;

	private void Awake()
	{
		Instance = this;
	}

	public void UpdateTraitOptions()
	{
		traitOptions.ClearOptions();
		traitOptions.AddOptions(TraitPanelUI.Instance.allTraits);
		traitOptions.AddOptions(TraitManager.instancedTraitsAndStatuses.ToList());
	}

	public void LoadAllData()
	{
		_traitNames = new List<string>();
		_hpPerLevel = new List<int>();
		_attackPerLevel = new List<int>();
		attackMultiplierInput.text = "0";
		hpMultiplierInput.text = "0";
		attackSpeedMultiplierInput.text = "0";
		staminaReductionMultiplierInput.text = "0";
		speedModifierInput.text = "0";
		hpPerLevelInput.text = "0";
		attackPerLevelInput.text = "0";
		neutralSpawnLevelModInput.text = "0";
		runSpeedInput.text = "0";
		walkSpeedInput.text = "0";
		raceOptions.ClearOptions();
		string[] names = Enum.GetNames(typeof(RACE));
		raceOptions.AddOptions(names.ToList());
	}

	private void ClearData()
	{
		currentSelectedTraitButton = null;
		currentSelectedHPPerLevelButton = null;
		currentSelectedAttackPerLevelButton = null;
		attackMultiplierInput.text = "0";
		hpMultiplierInput.text = "0";
		attackSpeedMultiplierInput.text = "0";
		staminaReductionMultiplierInput.text = "0";
		speedModifierInput.text = "0";
		hpPerLevelInput.text = "0";
		attackPerLevelInput.text = "0";
		neutralSpawnLevelModInput.text = "0";
		runSpeedInput.text = "0";
		walkSpeedInput.text = "0";
		raceOptions.value = 0;
		traitOptions.value = 0;
		_traitNames.Clear();
		_hpPerLevel.Clear();
		_attackPerLevel.Clear();
		Utilities.DestroyChildren(traitsScrollRect.content);
		Utilities.DestroyChildren(hpPerLevelScrollRect.content);
		Utilities.DestroyChildren(attackPerLevelScrollRect.content);
	}

	private void SaveRace()
	{
		_ = raceOptions.value;
		string path = Utilities.coreStreamingDataPath + "/RaceSettings/" + raceOptions.options[raceOptions.value].text + ".json";
		if (!Utilities.DoesFileExist(path))
		{
			SaveRaceJson(path);
		}
	}

	private void SaveRaceJson(string path)
	{
		Debug.Log("Successfully saved race at " + path);
	}

	private void LoadRace()
	{
	}

	private void LoadRaceDataToUI(RaceData raceSetting)
	{
		raceOptions.value = GetDropdownIndex(raceOptions, raceSetting.race.ToString());
		attackMultiplierInput.text = raceSetting.attackMultiplier.ToString();
		hpMultiplierInput.text = raceSetting.hpMultiplier.ToString();
		attackSpeedMultiplierInput.text = raceSetting.attackSpeedMultiplier.ToString();
		staminaReductionMultiplierInput.text = raceSetting.staminaReductionMultiplier.ToString();
		runSpeedInput.text = raceSetting.runSpeed.ToString();
		walkSpeedInput.text = raceSetting.walkSpeed.ToString();
		for (int i = 0; i < raceSetting.traitNames.Length; i++)
		{
			string text = raceSetting.traitNames[i];
			_traitNames.Add(text);
			UnityEngine.Object.Instantiate(raceStringButtonPrefab, traitsScrollRect.content).GetComponent<RaceStringButton>().SetText(text, "trait");
		}
	}

	private int GetDropdownIndex(Dropdown options, string name)
	{
		for (int i = 0; i < options.options.Count; i++)
		{
			if (options.options[i].text == name)
			{
				return i;
			}
		}
		return 0;
	}

	public void OnAddNewRace()
	{
		ClearData();
	}

	public void OnSaveRace()
	{
		SaveRace();
	}

	public void OnEditRace()
	{
		LoadRace();
	}

	public void OnAddTrait()
	{
		string text = traitOptions.options[traitOptions.value].text;
		if (!_traitNames.Contains(text))
		{
			_traitNames.Add(text);
			UnityEngine.Object.Instantiate(raceStringButtonPrefab, traitsScrollRect.content).GetComponent<RaceStringButton>().SetText(text, "trait");
		}
	}

	public void OnRemoveTrait()
	{
		if (currentSelectedTraitButton != null)
		{
			string text = currentSelectedTraitButton.buttonText.text;
			if (_traitNames.Remove(text))
			{
				UnityEngine.Object.Destroy(currentSelectedTraitButton.gameObject);
				currentSelectedTraitButton = null;
			}
		}
	}

	public void OnAddHPPerLevel()
	{
		string text = hpPerLevelInput.text;
		_hpPerLevel.Add(int.Parse(text));
		UnityEngine.Object.Instantiate(raceStringButtonPrefab, hpPerLevelScrollRect.content).GetComponent<RaceStringButton>().SetText(text, "hpperlevel");
	}

	public void OnRemoveHPPerLevel()
	{
		if (currentSelectedHPPerLevelButton != null)
		{
			int siblingIndex = currentSelectedHPPerLevelButton.gameObject.transform.GetSiblingIndex();
			_hpPerLevel.RemoveAt(siblingIndex);
			UnityEngine.Object.Destroy(currentSelectedHPPerLevelButton.gameObject);
			currentSelectedHPPerLevelButton = null;
		}
	}

	public void OnAddAttackPerLevel()
	{
		string text = attackPerLevelInput.text;
		_attackPerLevel.Add(int.Parse(text));
		UnityEngine.Object.Instantiate(raceStringButtonPrefab, attackPerLevelScrollRect.content).GetComponent<RaceStringButton>().SetText(text, "attackperlevel");
	}

	public void OnRemoveAttackPerLevel()
	{
		if (currentSelectedAttackPerLevelButton != null)
		{
			int siblingIndex = currentSelectedAttackPerLevelButton.gameObject.transform.GetSiblingIndex();
			_attackPerLevel.RemoveAt(siblingIndex);
			UnityEngine.Object.Destroy(currentSelectedAttackPerLevelButton.gameObject);
			currentSelectedAttackPerLevelButton = null;
		}
	}

	public void OnClickTraitsTab()
	{
		traitsGO.SetActive(value: true);
		hpPerLevelGO.SetActive(value: false);
		attackPerLevelGO.SetActive(value: false);
	}

	public void OnClickHPPerLevelTab()
	{
		traitsGO.SetActive(value: false);
		hpPerLevelGO.SetActive(value: true);
		attackPerLevelGO.SetActive(value: false);
	}

	public void OnClickAttackPerLevelTab()
	{
		traitsGO.SetActive(value: false);
		hpPerLevelGO.SetActive(value: false);
		attackPerLevelGO.SetActive(value: true);
	}
}
